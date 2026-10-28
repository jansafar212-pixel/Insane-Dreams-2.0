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
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0025Tests
    {
        private const float X=75000;
        private readonly List<GameObject> owned=new List<GameObject>();private bool background;
        private GameObject player;private PlayerActions actions;
        private GameObject Keep(GameObject g){owned.Add(g);return g;}
        private GameObject Solid(Vector2 p,Vector2 size){var g=Keep(new GameObject("S25Solid"));g.transform.position=p;g.AddComponent<BoxCollider2D>().size=size;return g;}
        private GameObject Enemy(float x)
        {var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X+x,.85f),Quaternion.identity));g.GetComponent<EnemyCombat>().enabled=false;g.GetComponent<LarperBrain>().enabled=false;g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;return g;}
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;Solid(new Vector2(X,-.5f),new Vector2(80,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,.85f),Quaternion.identity));actions=player.GetComponent<PlayerActions>();var s=new SerializedObject(actions);s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;}
        [UnityTest]public IEnumerator PlayerMeleeSwingsHitsLocallyAndNeverEmitsFirearmCues()
        {
            // This legacy melee unit fixture predates slot 3 becoming grenade selection.
            Object.DestroyImmediate(player.GetComponent<PlayerThrowables>());
            var loadout=player.GetComponent<PlayerLoadout>();var catalog=AssetDatabase.LoadAssetAtPath<InsaneDreams.Progression.GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");loadout.Equip(EquipmentSlot.Melee,new RuntimeWeapon(catalog.Find(catalog.meleeId).weapon));loadout.Select(EquipmentSlot.Melee);yield return null;
            var melee=loadout.SelectedWeapon;var enemy=Enemy(melee.Definition.AttackRange*.7f);var health=enemy.GetComponent<Health>();float hp=health.Current;
            var attack=player.GetComponent<AttackExecutor>();var feedback=player.GetComponent<CombatFeedback>();var rig=player.GetComponent<CharacterVisualDriver>().Rig;
            Assert.IsTrue(attack.Attack(melee,Vector2.right,Time.time));yield return null;
            Assert.Less(health.Current,hp);Assert.AreEqual(0,feedback.ShotCues);Assert.Greater(feedback.HitCues,0);
            Assert.IsFalse(player.transform.Find("MuzzleCue").GetComponent<SpriteRenderer>().enabled);foreach(var line in player.GetComponentsInChildren<LineRenderer>())Assert.IsFalse(line.enabled);
            float angle=rig.weaponMount.localEulerAngles.z;yield return new WaitForSeconds(.1f);Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(angle,rig.weaponMount.localEulerAngles.z)),15);
            enemy.transform.position=new Vector2(X+melee.Definition.AttackRange+.7f,.85f);Physics2D.SyncTransforms();hp=health.Current;Assert.IsTrue(attack.Attack(melee,Vector2.right,Time.time+2));Assert.AreEqual(hp,health.Current);
            yield return new WaitForSeconds(.25f);loadout.Select(EquipmentSlot.Firearm1);yield return null;int ammo=loadout.SelectedWeapon.MagazineAmmo;Assert.IsTrue(attack.Attack(loadout.SelectedWeapon,Vector2.right,Time.time+4));Assert.AreEqual(ammo-1,loadout.SelectedWeapon.MagazineAmmo);Assert.AreEqual(1,feedback.ShotCues);Assert.IsTrue(player.transform.Find("MuzzleCue").GetComponent<SpriteRenderer>().enabled);
        }
        [UnityTest]public IEnumerator RealNearMissBurstsCreateBoundedPlayerPressureAndCoverBlocksIt()
        {
            var g=Keep(new GameObject("S25HostileShot"));var hp=g.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)CombatTeam.Enemy;s.ApplyModifiedPropertiesWithoutUndo();var attack=g.AddComponent<AttackExecutor>();
            var weapon=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset"),100);g.transform.position=new Vector2(X-6,player.GetComponent<Collider2D>().bounds.max.y+.15f);Physics2D.SyncTransforms();
            var pressure=player.GetComponent<PlayerSuppression>();for(int i=0;i<4;i++){Assert.IsTrue(attack.Attack(weapon,Vector2.right,Time.time));yield return new WaitForSeconds(.1f);}
            Assert.That(pressure.Pressure,Is.InRange(.4f,1));Assert.AreEqual(200,player.GetComponent<PlayerHealing>().DuctTape);
            actions.Jump();yield return new WaitForSeconds(.1f);Assert.IsFalse(player.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().IsGrounded,"Pressure must not restrict jumping");
            yield return new WaitForSeconds(2.5f);Assert.AreEqual(0,pressure.Pressure,.001f);
            Solid(new Vector2(X-2,2),new Vector2(.2f,4));Physics2D.SyncTransforms();Assert.IsTrue(attack.Attack(weapon,Vector2.right,Time.time));Assert.AreEqual(0,pressure.Pressure,.001f);
        }
        [UnityTest]public IEnumerator DedicatedBlindFireControlIsConditionalAndReleasesOnCoverExit()
        {
            var doc=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();doc.GetComponent<WarehouseTouchControls>().Bind(actions);yield return new WaitForSeconds(.2f);
            var blind=doc.rootVisualElement.Q<VisualElement>("blindFireButton");Assert.IsNull(doc.rootVisualElement.Q("fireButton"));Assert.AreEqual(DisplayStyle.None,blind.resolvedStyle.display);
            Solid(new Vector2(X+1.1f,.45f),new Vector2(1,.9f));actions.SetTouchAim(Vector2.right);actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.4f);Assert.IsTrue(player.GetComponent<PlayerCover2D>().IsCovered);Assert.AreEqual(DisplayStyle.Flex,blind.resolvedStyle.display);
            using(var evt=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=blind.worldBound.center})){evt.target=blind;blind.SendEvent(evt);}yield return new WaitForSeconds(.1f);Assert.IsTrue(player.GetComponent<PlayerCover2D>().IsBlindFiring);
            actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.3f);Assert.IsFalse(player.GetComponent<PlayerCover2D>().IsCovered);Assert.AreEqual(DisplayStyle.None,blind.resolvedStyle.display);Assert.IsFalse(actions.FireHeld);
        }
        [UnityTest]public IEnumerator SamuraiViewportUsesVisibleSpritesEvenWithTinyDisplacedPhysicsShape()
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(X+4,.53f),Quaternion.identity));var e=g.GetComponent<EnemyCombat>();e.enabled=false;g.GetComponent<LarperBrain>().enabled=false;g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var camera=Keep(new GameObject("S25Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5;camera.transform.position=new Vector3(X+4,2,-10);e.BindGameplayCamera(camera);yield return null;
            var box=g.GetComponent<BoxCollider2D>();box.size=Vector2.one*.01f;box.offset=Vector2.one*100;Physics2D.SyncTransforms();Assert.IsTrue(e.InGameplayViewport,"Visible sprites, not displaced physics bounds, determine viewport entry");camera.transform.position+=Vector3.right*100;Assert.IsFalse(e.InGameplayViewport);
        }
        [UnityTest]public IEnumerator ActualOpeningFireQueuesReadableSpeechWithoutReplacingActiveBubbles()
        {
            LarperAwareness.ResetEncounterSpeech();var doc=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();var cam=Keep(new GameObject("S25SpeechCamera")).AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=8;cam.transform.position=new Vector3(X+3,3,-10);
            var e=Enemy(3);var bubble=e.GetComponent<LarperSpeechBubble>();bubble.Bind(cam,doc);var awareness=e.GetComponent<LarperAwareness>();awareness.Configure(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Ordinary.asset"));bool opening=false;awareness.LineDelivered+=c=>opening|=c==LarperSpeechEvent.OpeningFire;yield return new WaitForSeconds(.2f);
            var gun=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset"),100);Assert.IsTrue(e.GetComponent<AttackExecutor>().Attack(gun,Vector2.right,Time.time));yield return null;Assert.IsTrue(opening);Assert.IsTrue(bubble.IsVisible);string line=bubble.LastLine;
            awareness.CoverCue();Assert.AreEqual(line,bubble.LastLine);yield return new WaitForSeconds(3);Assert.IsTrue(bubble.IsVisible,"Speech must remain readable for at least three seconds");Assert.LessOrEqual(LarperSpeechBubble.VisibleCount,2);
        }
        [UnityTest]public IEnumerator FaintanylButtonRevivesInPlaceTwiceWithTwoSecondProtection()
        {
            var revive=player.GetComponent<PlayerRevive>();var health=player.GetComponent<Health>();var body=player.GetComponent<Rigidbody2D>();
            revive.RestoreInventory(2);var manual=new SerializedObject(revive);manual.FindProperty("automaticRevival").boolValue=false;manual.ApplyModifiedPropertiesWithoutUndo();var recoverySettings=new SerializedObject(player.GetComponent<PlayerHealthRecovery>());recoverySettings.FindProperty("allowCheckpointRespawn").boolValue=true;recoverySettings.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreEqual(2,revive.Injections);Assert.AreEqual(0,revive.AddInjections(99));
            var doc=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();doc.GetComponent<WarehouseTouchControls>().Bind(actions);
            int checkpoints=0;player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Recovered+=()=>checkpoints++;
            var loadout=player.GetComponent<PlayerLoadout>();loadout.SelectedWeapon.SetAmmunition(3,7);
            for(int i=0;i<2;i++)
            {
                health.ProtectFor(0);Assert.IsTrue(health.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy)));Vector2 deathPosition=body.position;yield return new WaitForSeconds(.15f);
                Assert.IsTrue(health.IsDefeated);Assert.IsFalse(body.simulated);Assert.IsFalse(actions.enabled);
                var button=doc.rootVisualElement.Q<Button>("reviveButton");Assert.AreEqual(DisplayStyle.Flex,doc.rootVisualElement.Q("reviveOverlay").resolvedStyle.display);
                Assert.That(Vector2.Distance(button.worldBound.center,doc.rootVisualElement.worldBound.center),Is.LessThan(65),"Revive should be centered");
                if(i==0){using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}}
                else
                {
                    using(var down=PointerDownEvent.GetPooled(new Touch{fingerId=2,phase=TouchPhase.Began,position=button.worldBound.center},EventModifiers.None)){down.target=button;button.SendEvent(down);}
                    yield return null;
                    using(var up=PointerUpEvent.GetPooled(new Touch{fingerId=2,phase=TouchPhase.Ended,position=button.worldBound.center},EventModifiers.None)){up.target=button;button.SendEvent(up);}
                }
                yield return null;
                Assert.IsFalse(health.IsDefeated);Assert.IsTrue(body.simulated);Assert.IsTrue(actions.enabled);Assert.AreEqual(1-i,revive.Injections);Assert.IsFalse(revive.TryRevive());
                Assert.That(Vector2.Distance(body.position,deathPosition),Is.LessThan(.1f));Assert.AreEqual(0,checkpoints);Assert.AreEqual(3,loadout.SelectedWeapon.MagazineAmmo);Assert.AreEqual(7,loadout.SelectedWeapon.ReserveAmmo);
                Assert.IsFalse(health.ApplyDamage(new DamageInfo(10,CombatTeam.Enemy)));yield return new WaitForSeconds(1.7f);Assert.IsFalse(health.ApplyDamage(new DamageInfo(10,CombatTeam.Enemy)));yield return new WaitForSeconds(.35f);Assert.IsTrue(health.ApplyDamage(new DamageInfo(10,CombatTeam.Enemy)));
            }
            Assert.AreEqual(200,player.GetComponent<PlayerHealing>().DuctTape);
            health.ProtectFor(0);Assert.IsTrue(health.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy)));Assert.IsTrue(health.IsDefeated);yield return new WaitForSeconds(1.4f);Assert.IsFalse(health.IsDefeated);Assert.AreEqual(1,checkpoints,"Empty inventory preserves checkpoint recovery after the ragdoll collapse");
        }
        [UnityTest]public IEnumerator FaintanylCanBeDeclinedWithoutConsumingAndCheckpointResumesControls()
        {
            var revive=player.GetComponent<PlayerRevive>();revive.RestoreInventory(2);var hp=player.GetComponent<Health>();hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));
            Assert.IsTrue(revive.ReturnToCheckpoint());Assert.AreEqual(1,revive.Injections);Assert.IsFalse(hp.IsDefeated);Assert.IsTrue(actions.enabled);Assert.IsTrue(player.GetComponent<PlayerCombat>().enabled);Assert.IsTrue(player.GetComponent<Rigidbody2D>().simulated);
            revive.RestoreInventory(50);Assert.AreEqual(2,revive.Injections);revive.RestoreInventory(-5);Assert.AreEqual(0,revive.Injections);Assert.AreEqual(2,revive.AddInjections(3));yield return null;
        }
        [Test]public void FaintanylSaveRoundTripAndLegacyDefaultsAreFinite()
        {
            var legacy=JsonUtility.FromJson<InsaneDreams.Progression.GameProgress>("{\"version\":1}");legacy.Normalize();Assert.AreEqual(0,legacy.faintanyl);
            legacy.faintanyl=0;var restored=JsonUtility.FromJson<InsaneDreams.Progression.GameProgress>(JsonUtility.ToJson(legacy));Assert.AreEqual(0,restored.faintanyl);
        }
    }
}
#endif
