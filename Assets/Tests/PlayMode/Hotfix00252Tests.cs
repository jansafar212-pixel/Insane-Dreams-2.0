#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Hotfix00252Tests
    {
        private const float X=5300;private bool background;private Random.State random;
        private readonly List<GameObject> owned=new List<GameObject>();private GameObject player;private Camera view;
        private GameObject Keep(GameObject g){owned.Add(g);return g;}
        private GameObject Solid(float x,float y,Vector2 size){var g=Keep(new GameObject("H252Geometry"));g.transform.position=new Vector2(X+x,y);g.AddComponent<BoxCollider2D>().size=size;return g;}
        private void Float(Object target,string field,float value){var s=new SerializedObject(target);s.FindProperty(field).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(252);Solid(0,-.5f,new Vector2(80,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X-8,.85f),Quaternion.identity));player.GetComponent<Health>().ProtectFor(60);
            view=Keep(new GameObject("H252Camera")).AddComponent<Camera>();view.orthographic=true;view.orthographicSize=8;view.aspect=2;view.transform.position=new Vector3(X,2,-10);yield return new WaitForSeconds(.2f);
        }
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;Random.state=random;}
        private EnemyCombat Enemy(string kind="Larper",string personality="Ordinary")
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/"+kind+".prefab"),new Vector3(X,kind=="Samurai"?.53f:.85f),Quaternion.identity));
            g.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/"+kind+".asset"));var e=g.GetComponent<EnemyCombat>();e.BindTarget(player.GetComponent<Health>());e.BindGameplayCamera(view);g.GetComponent<LarperAwareness>().Configure(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+personality+".asset"));return e;
        }
        private IEnumerator Burst(EnemyCombat e,int rounds=6,float extra=.15f)
        {
            var source=Keep(new GameObject("H252RealGunfire"));source.transform.position=new Vector2(X-8,e.GetComponent<BoxCollider2D>().bounds.max.y+extra);var h=source.AddComponent<Health>();var s=new SerializedObject(h);s.FindProperty("team").enumValueIndex=(int)CombatTeam.Player;s.ApplyModifiedPropertiesWithoutUndo();
            var attack=source.AddComponent<AttackExecutor>();var gun=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),72);
            for(int i=0;i<rounds;i++){Assert.IsTrue(attack.Attack(gun,Vector2.right,Time.time));yield return new WaitForSeconds(.21f);}
        }
        [UnityTest]public IEnumerator SustainedRealOverheadFireEscapesEvenWhenOptionalRetreatRollIsZero()
        {
            var e=Enemy();Float(e,"suppressionRetreatChance",0);Float(e,"defensiveCrouchProbability",1);yield return new WaitForSeconds(.2f);float start=e.transform.position.x,hp=e.GetComponent<Health>().Current;
            yield return Burst(e);Assert.GreaterOrEqual(e.NearMissEvents,5);Assert.GreaterOrEqual(e.SuppressionEvasions,1);Assert.Greater(Mathf.Abs(e.transform.position.x-start),.25f);Assert.AreEqual(hp,e.GetComponent<Health>().Current);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().CollapseReactions);
            yield return new WaitForSeconds(10);Assert.AreEqual(0,e.Suppression);
        }
        [UnityTest]public IEnumerator SaturatedReloadingLarperCanCancelReloadAndMoveToSafetyWithoutAmmoTransfer()
        {
            var e=Enemy();e.Weapon.SetAmmunition(0,27);Assert.IsTrue(e.Weapon.Cycle.TryReload(Time.time));yield return Burst(e);Assert.GreaterOrEqual(e.SuppressionEvasions,1);Assert.Greater(Mathf.Abs(e.transform.position.x-X),.25f);Assert.AreEqual(0,e.Weapon.MagazineAmmo);Assert.AreEqual(27,e.Weapon.ReserveAmmo);
        }
        [UnityTest]public IEnumerator BlockedLongRetreatCanChooseShortAlternativeRatherThanFakeFullMagazineReload()
        {
            // Leave a genuinely reachable local step (>=1 unit), while blocking a major retreat.
            Solid(2.2f,2,new Vector2(.3f,4));var e=Enemy();Float(e,"suppressionRetreatChance",0);yield return new WaitForSeconds(.2f);float start=e.transform.position.x,travel=0;
            var burst=Burst(e);var trace=new System.Text.StringBuilder();
            while(burst.MoveNext()){yield return burst.Current;travel=Mathf.Max(travel,Mathf.Abs(e.transform.position.x-start));trace.AppendLine($"x={e.transform.position.x-X:F3} state={e.State} suppression={e.Suppression:F2} evasions={e.SuppressionEvasions}");}
            System.IO.File.WriteAllText("TestResults/Sprint0027/ShortRetreatTrace.txt",trace.ToString());
            // Returning toward combat after a local escape must not erase the
            // actual retreat movement observed during the burst.
            Assert.GreaterOrEqual(e.SuppressionEvasions,1);Assert.Greater(travel,.25f,trace.ToString());Assert.Less(e.transform.position.x,X+1.75f);Assert.AreEqual(0,e.PanicEscapes);Assert.IsFalse(e.Weapon.Cycle.IsReloading);
        }
        [UnityTest]public IEnumerator DistantOrClippedShotsDoNotGenerateNearMissPressure()
        {
            var e=Enemy();yield return Burst(e,2,4);Assert.AreEqual(0,e.NearMissEvents);Solid(-4,2,new Vector2(.4f,4));yield return Burst(e,3);Assert.AreEqual(0,e.NearMissEvents);Assert.AreEqual(0,e.Suppression);
        }
        [UnityTest]public IEnumerator SamuraiKeepsMeleeRoleUnderCloseFire()
        {
            var e=Enemy("Samurai");yield return new WaitForSeconds(.2f);yield return Burst(e,6,.9f);Assert.Greater(e.NearMissEvents,0);Assert.AreEqual(0,e.SuppressionEvasions);Assert.IsFalse(e.IsDefensivelyCrouching);Assert.AreNotEqual(EngagementState.Withdrawing,e.State);Assert.AreNotEqual(EngagementState.Panicked,e.State);
        }
        [UnityTest]public IEnumerator GlockHandAttachmentReleaseGravityCollisionAndPoolLifetime()
        {
            player.GetComponent<CharacterVisualDriver>().enabled=false;var rig=player.GetComponentInChildren<CutoutRig>();var handler=player.GetComponent<PistolMagazineReload>();var d=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset");var w=new RuntimeWeapon(d);w.SetAmmunition(3,10);w.Cycle.TryReload(Time.time);
            rig.weapon.sprite=d.EquippedSprite;rig.SetMagazine(d.MagazineSprite,d.MagazineMountOffset);
            void Pose(float p,bool reload=true){rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral,false,true,reload,p);handler.Pose(rig,w,reload,p);}
            Pose(.1f);Assert.That(Vector2.Distance(rig.magazine.transform.localPosition,d.MagazineMountOffset),Is.LessThan(.001f));Pose(.3f);
            Vector3 hand=rig.leftArm.TransformPoint(Vector3.down*rig.skin.arm.bounds.size.y);Assert.Less(Vector3.Distance(hand,rig.magazine.transform.position),.03f);
            Pose(.46f);var drop=handler.LastReleased;Assert.IsNotNull(drop);float y=drop.position.y;Assert.Greater(y,.1f,"release "+drop.position+" player "+player.transform.position);Assert.Less(Mathf.Abs(drop.position.x-X),20,"release x "+drop.position);yield return new WaitForSeconds(.2f);Assert.Less(drop.position.y,y-.04f);Pose(.7f);var replacement=rig.weapon.transform.Find("PistolReplacementMagazine");hand=rig.leftArm.TransformPoint(Vector3.down*rig.skin.arm.bounds.size.y);Assert.Less(Vector3.Distance(hand,replacement.position),.03f);
            Pose(0,false);Assert.IsTrue(rig.magazine.enabled);Assert.IsFalse(replacement.GetComponent<SpriteRenderer>().enabled);Assert.AreEqual(3,w.MagazineAmmo);yield return new WaitForSeconds(1.5f);Assert.Greater(drop.GetComponent<Collider2D>().bounds.min.y,-.08f);yield return new WaitForSeconds(2.5f);Assert.AreEqual(0,handler.ActiveDebris);
            for(int i=0;i<5;i++){w.Cycle.CancelReload();w.Cycle.TryReload(Time.time+i+1);Pose(.46f);}Assert.LessOrEqual(handler.ActiveDebris,3);
            w.Cycle.Tick(w.Cycle.ReloadEndsAt);int ammo=w.MagazineAmmo,reserve=w.ReserveAmmo;Assert.AreEqual(13,ammo+reserve);Assert.Greater(ammo,3);Assert.IsFalse(w.Cycle.IsReloading);Pose(0,false);w.Cycle.Tick(w.Cycle.ReloadEndsAt+1);Assert.AreEqual(ammo,w.MagazineAmmo);Assert.AreEqual(reserve,w.ReserveAmmo);Assert.IsTrue(rig.magazine.enabled);
        }
        [UnityTest]public IEnumerator CarbinePoseIsUntouchedByPistolHandler()
        {
            player.GetComponent<CharacterVisualDriver>().enabled=false;var rig=player.GetComponentInChildren<CutoutRig>();var h=player.GetComponent<PistolMagazineReload>();var d=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");var w=new RuntimeWeapon(d);Assert.IsFalse(h.Handles(d));
            rig.weapon.sprite=d.EquippedSprite;rig.SetMagazine(d.MagazineSprite,d.MagazineMountOffset,true);foreach(float p in new[]{.1f,.3f,.5f,.7f,.95f}){rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral,false,true,true,p);Vector3 arm=rig.rightArm.localEulerAngles,mag=rig.magazine.transform.localPosition,mount=rig.weaponMount.localPosition;h.Pose(rig,w,true,p);Assert.AreEqual(arm,rig.rightArm.localEulerAngles);Assert.AreEqual(mag,rig.magazine.transform.localPosition);Assert.AreEqual(mount,rig.weaponMount.localPosition);}yield return null;
        }
    }
}
#endif
