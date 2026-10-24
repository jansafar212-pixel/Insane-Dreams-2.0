#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0248Tests
    {
        private readonly List<GameObject> owned=new List<GameObject>();private bool background;private Random.State random;private const float X=49000;
        private GameObject Keep(GameObject g){owned.Add(g);return g;}
        private static void Float(Object o,string field,float v){var s=new SerializedObject(o);s.FindProperty(field).floatValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(248);yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Random.state=random;Application.runInBackground=background;}
        private FireworkProjectile Bundle()=>Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab"))).GetComponent<FireworkProjectile>();
        private Health Victim(float x,CombatTeam team)
        {var g=Keep(new GameObject("S0248Victim"));g.layer=2;g.transform.position=new Vector2(X+x,20);g.AddComponent<BoxCollider2D>().size=new Vector2(.3f,.6f);var hp=g.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)team;s.ApplyModifiedPropertiesWithoutUndo();return hp;}
        [UnityTest]public IEnumerator EveryFireworkCanHurtAllTeamsAndSelfExactlyOnceWhileWallAndBulletRulesRemain()
        {
            var owner=Victim(0,CombatTeam.Enemy);var ally=Victim(1,CombatTeam.Enemy);ally.gameObject.AddComponent<CircleCollider2D>().radius=.2f;var player=Victim(-1,CombatTeam.Player);var shielded=Victim(2.4f,CombatTeam.Enemy);
            var wall=Keep(new GameObject("S0248Shield"));wall.transform.position=new Vector2(X+1.8f,20);wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,4);Physics2D.SyncTransforms();
            Assert.IsFalse(ally.ApplyDamage(new DamageInfo(5,CombatTeam.Enemy,owner.gameObject)));
            var b=Bundle();b.Prepare(owner.gameObject,new Vector2(X,20),Color.cyan,false);b.ExplodeHeld(new Vector2(X,20));
            Assert.Less(owner.Current,100);Assert.Less(ally.Current,100);Assert.Less(player.Current,100);Assert.AreEqual(100,shielded.Current);Assert.AreEqual(3,b.LastAffected);
            float hp=ally.Current;b.Detonate();Assert.AreEqual(hp,ally.Current);yield return new WaitForSeconds(.4f);Assert.IsTrue(b.Busy,"The colorful burst must linger beyond the old .35-second cutoff.");yield return new WaitForSeconds(.5f);Assert.IsFalse(b.Busy);
        }
        [UnityTest]public IEnumerator FuseBucketsVaryAndShortFuseDetonatesAtActualAirbornePosition()
        {
            var b=Bundle();var source=Keep(new GameObject("Owner"));bool shortSeen=false,normalSeen=false,longSeen=false;
            for(int i=0;i<100;i++){b.Prepare(source,new Vector2(X+5,20),Color.yellow,true);float f=b.DetonationDelay;shortSeen|=f>=.25f&&f<=.45f;normalSeen|=f>=.8f&&f<=1.5f;longSeen|=f>=2&&f<=3.2f;b.Cancel();}Assert.IsTrue(shortSeen&&normalSeen&&longSeen);
            Float(b,"shortFuseChance",1);b.Prepare(source,new Vector2(X+5,20),Color.magenta,true);b.Launch(new Vector2(X,20));yield return new WaitForSeconds(.48f);Assert.IsTrue(b.Exploded);Assert.IsFalse(b.Landed);Assert.AreNotEqual(new Vector2(X+5,20),b.Destination);Assert.Less(Vector2.Distance(b.Destination,b.BundlePosition),.05f);
        }
        [UnityTest]public IEnumerator FasterPistolEmpties27RoundsWithOneCuePerShotAndFiniteReload()
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X,20),Quaternion.identity));g.GetComponent<EnemyCombat>().enabled=false;g.GetComponent<LarperBrain>().enabled=false;g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var weapon=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset"),27);var a=g.GetComponent<AttackExecutor>();var cues=g.GetComponent<CombatFeedback>();
            yield return null;float start=Time.time;for(int i=0;i<27;i++){Assert.IsTrue(a.Attack(weapon,Vector2.right,Time.time));Assert.IsFalse(a.Attack(weapon,Vector2.right,Time.time));yield return new WaitForSeconds(.075f);}Assert.Less(Time.time-start,2.6f);Assert.AreEqual(27,a.ShotsFired);Assert.AreEqual(27,cues.ShotCues);Assert.AreEqual(0,weapon.MagazineAmmo);Assert.IsFalse(a.Attack(weapon,Vector2.right,Time.time));Assert.IsTrue(weapon.Cycle.TryReload(Time.time));weapon.Cycle.Tick(Time.time+3.9f);Assert.AreEqual(0,weapon.MagazineAmmo);weapon.Cycle.Tick(Time.time+4.01f);Assert.AreEqual(27,weapon.MagazineAmmo);Assert.AreEqual(0,weapon.ReserveAmmo);
        }
        [UnityTest]public IEnumerator ScaledSamuraiKeepsFeetGripAndEveryCorpseSegmentAtFivePercentIncrease()
        {
            var floor=Keep(new GameObject("S0248Floor"));floor.transform.position=new Vector2(X,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(10,1);
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(X,.715f),Quaternion.identity));g.GetComponent<EnemyCombat>().enabled=false;g.GetComponent<LarperBrain>().enabled=false;yield return new WaitForSeconds(.3f);var r=g.GetComponentInChildren<CutoutRig>();
            Assert.AreEqual(1.05f,r.transform.lossyScale.y,.0001f);float toe=r.Segmented.rightFoot.TransformPoint(Vector3.down*r.Segmented.foot.bounds.size.y).y;Assert.AreEqual(0,toe,.08f);Assert.Less(Vector3.Distance(r.weaponMount.position,r.Segmented.grip.position),.001f);
            g.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;var doll=g.GetComponent<RagdollOnDefeat>().LastRagdoll;Keep(doll.gameObject);Assert.AreEqual(14,doll.Bodies.Length);foreach(var body in doll.Bodies){Assert.AreEqual(1.05f,body.transform.lossyScale.y,.0001f);Assert.AreEqual(-2,body.GetComponent<Collider2D>().excludeLayers.value);}
        }
        [UnityTest]public IEnumerator CarbineFlashUsesNewDirectionalSpriteAtMuzzleAndExpiresPerShot()
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,20),Quaternion.identity));g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var actions=g.GetComponent<InsaneDreams.Input.PlayerActions>();var data=new SerializedObject(actions);data.FindProperty("readKeyboard").boolValue=false;data.ApplyModifiedPropertiesWithoutUndo();actions.SetTouchAim(Vector2.right);yield return null;
            var weapon=g.GetComponent<PlayerLoadout>().SelectedWeapon;var attack=g.GetComponent<AttackExecutor>();Assert.IsTrue(attack.Attack(weapon,Vector2.right,Time.time));yield return null;
            var flash=g.transform.Find("MuzzleCue").GetComponent<SpriteRenderer>();Assert.IsTrue(flash.enabled);Assert.AreEqual("CarbineFlash",flash.sprite.name);Assert.AreEqual(new Vector2(15,9),flash.sprite.rect.size);Assert.Less(Vector2.Distance(flash.transform.position,g.GetComponent<CharacterVisualDriver>().Rig.MuzzlePosition),.01f);
            yield return new WaitForSeconds(.09f);Assert.IsFalse(flash.enabled);Assert.AreEqual(1,g.GetComponent<CombatFeedback>().ShotCues);Assert.AreEqual(23,weapon.MagazineAmmo);
        }
        [UnityTest]public IEnumerator AnimatedSamuraiHurtboxMatchesBodyAcrossFacingAndPosesWithoutInvisibleMovementHits()
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(X,20),Quaternion.identity));g.GetComponent<EnemyCombat>().enabled=false;g.GetComponent<LarperBrain>().enabled=false;g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;g.GetComponent<CharacterVisualDriver>().enabled=false;
            var r=g.GetComponentInChildren<CutoutRig>();var hurt=g.GetComponent<SamuraiBodyHitbox>();yield return null;
            foreach(float facing in new[]{1f,-1f})foreach(var pose in new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Crouch,CharacterPose.Climb})
            {
                r.Pose(pose,1,Vector2.right*facing,facing,0,FaceExpression.Neutral);r.Segmented.Pose(pose,1,35,pose!=CharacterPose.Jump,0);hurt.RefreshBounds();Physics2D.SyncTransforms();
                var bounds=r.head.GetComponent<SpriteRenderer>().bounds;foreach(var t in r.BodyParts)bounds.Encapsulate(t.GetComponent<SpriteRenderer>().bounds);foreach(var t in r.Segmented.ExtraParts)bounds.Encapsulate(t.GetComponent<SpriteRenderer>().bounds);
                Assert.Less(Vector3.Distance(bounds.center,hurt.Collider.bounds.center),.01f);Assert.AreEqual(bounds.size.x,hurt.Collider.bounds.size.x,.01f);Assert.AreEqual(bounds.size.y,hurt.Collider.bounds.size.y,.01f);Assert.AreEqual(-1,hurt.Collider.excludeLayers.value);
            }
            var move=g.GetComponent<BoxCollider2D>();move.size=new Vector2(4,4);var shooter=Keep(new GameObject("S0248HurtboxShooter"));shooter.transform.position=new Vector2(X-4,21.3f);shooter.AddComponent<Health>();var attacks=shooter.AddComponent<AttackExecutor>();var gun=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),24);Physics2D.SyncTransforms();
            Assert.Less(attacks.Origin.y,move.bounds.max.y);Assert.Greater(attacks.Origin.y,hurt.Collider.bounds.max.y);
            float before=g.GetComponent<Health>().Current;
            Assert.IsTrue(attacks.Attack(gun,Vector2.right,Time.time));Assert.AreEqual(ShotSurface.None,attacks.LastSurface);Assert.AreEqual(before,g.GetComponent<Health>().Current);
            shooter.transform.position=new Vector2(X-4,hurt.Collider.bounds.center.y);Physics2D.SyncTransforms();Assert.IsTrue(attacks.Attack(gun,((Vector2)hurt.Collider.bounds.center-attacks.Origin).normalized,Time.time+.3f));Assert.AreEqual(before-20,g.GetComponent<Health>().Current);
        }
    }
}
#endif
