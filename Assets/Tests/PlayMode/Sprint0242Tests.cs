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
    public sealed class Sprint0242Tests
    {
        private const float X=38000;
        private readonly List<GameObject> objects=new List<GameObject>();private bool background;private Random.State random;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Float(Object target,string name,float value){var s=new SerializedObject(target);s.FindProperty(name).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private GameObject Solid(Vector2 pos,Vector2 size){var go=Keep(new GameObject("S0242Solid"));go.transform.position=pos;go.AddComponent<BoxCollider2D>().size=size;return go;}
        private Health Victim(Vector2 pos,CombatTeam team)
        {var go=Keep(new GameObject("S0242Victim"));go.layer=2;go.transform.position=pos;go.AddComponent<BoxCollider2D>().size=new Vector2(.6f,1);var hp=go.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)team;s.ApplyModifiedPropertiesWithoutUndo();return hp;}
        private FireworkProjectile Bundle()
        {var b=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab"))).GetComponent<FireworkProjectile>();var s=new SerializedObject(b);s.FindProperty("shortFuseChance").floatValue=0;s.FindProperty("longFuseChance").floatValue=0;s.FindProperty("normalFuse").vector2Value=new Vector2(2.5f,2.5f);s.ApplyModifiedPropertiesWithoutUndo();return b;}
        [UnitySetUp] public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(242);Solid(new Vector2(X,-.5f),new Vector2(80,1));yield return null;}
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Random.state=random;Application.runInBackground=background;}
        [UnityTest] public IEnumerator PhysicalBundleArcsLandsVisibleThenDetonatesExactlyOnceAndReusesItsEffects()
        {
            var source=Keep(new GameObject("Thrower"));var hp=Victim(new Vector2(X+5,.8f),CombatTeam.Player);var b=Bundle();
            b.Prepare(source,new Vector2(X+5,.15f),Color.cyan,false);b.Hold(new Vector2(X,.8f));float launchedAt=Time.time;b.Launch(new Vector2(X,.8f));
            yield return new WaitForSeconds(.25f);Assert.Greater(b.BundlePosition.y,1.5f);Assert.IsTrue(b.GetComponent<Rigidbody2D>().simulated);Assert.IsFalse(b.Exploded);
            float deadline=Time.realtimeSinceStartup+3;while(!b.Landed&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.IsTrue(b.Landed,"Real OnCollisionEnter2D must arm the landed delay.");Assert.IsFalse(b.Exploded);Assert.IsTrue(b.GetComponentInChildren<SpriteRenderer>().enabled);Assert.AreEqual(100,hp.Current);
            int effects=b.GetComponentsInChildren<SpriteRenderer>(true).Length;
            float fuseRemaining=b.DetonationDelay-(Time.time-launchedAt);yield return new WaitForSeconds(fuseRemaining*.6f);Assert.IsFalse(b.Exploded);Assert.AreEqual(100,hp.Current);
            yield return new WaitForSeconds(fuseRemaining*.45f);Assert.IsTrue(b.Exploded);Assert.Less(hp.Current,100);float remaining=hp.Current;b.Detonate();Assert.AreEqual(remaining,hp.Current);Assert.AreEqual(1,b.LastAffected);
            yield return new WaitForSeconds(.9f);Assert.IsFalse(b.Busy);b.Prepare(source,new Vector2(X+5,.15f),Color.magenta,false);Assert.AreEqual(effects,b.GetComponentsInChildren<SpriteRenderer>(true).Length);Assert.IsFalse(b.Exploded);b.Cancel();Assert.IsFalse(b.Busy);
        }
        [UnityTest] public IEnumerator WallCollisionStopsBundleWithoutImmediateExplosionAndProtectsVictimsBehindIt()
        {
            var source=Keep(new GameObject("Thrower"));var hp=Victim(new Vector2(X+3,.8f),CombatTeam.Player);Solid(new Vector2(X+2,3),new Vector2(.3f,7));Physics2D.SyncTransforms();
            var b=Bundle();b.Prepare(source,new Vector2(X+4,.2f),Color.yellow,true);b.Launch(new Vector2(X,.8f));
            float deadline=Time.realtimeSinceStartup+3;while(!b.Landed&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.IsTrue(b.Landed);Assert.Less(b.BundlePosition.x,X+2);Assert.IsFalse(b.Exploded);
            yield return new WaitForSeconds(b.DetonationDelay+.05f);Assert.IsTrue(b.Exploded);Assert.AreEqual(100,hp.Current);
        }
        [UnityTest] public IEnumerator ForcedHeldFailureCancelsThrowDamagesOwnerAndAllyThenSpecialistCanThrowAgain()
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/FireworksExpert.prefab"),new Vector3(X,.85f),Quaternion.identity));
            go.GetComponent<EnemyCombat>().enabled=false;go.GetComponent<LarperBrain>().enabled=false;go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var expert=go.GetComponent<FireworksCombat>();Float(expert,"heldMalfunctionChance",1);var hp=go.GetComponent<Health>();var ally=Victim(new Vector2(X+1,.85f),CombatTeam.Enemy);var target=Victim(new Vector2(X+7,.85f),CombatTeam.Player);
            yield return new WaitForSeconds(2.2f);float before=hp.Current;float deadline=Time.time+1.2f;
            while(expert.HeldMalfunctions==0&&Time.time<deadline){expert.Tick(target,target.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(1,expert.HeldMalfunctions);Assert.AreEqual(0,expert.Launches);Assert.IsFalse(expert.Preparing);Assert.IsTrue(expert.Surprised);Assert.Less(hp.Current,before);Assert.Less(ally.Current,100);Assert.IsFalse(hp.IsDefeated);
            var rig=go.GetComponent<CharacterVisualDriver>().Rig;Assert.AreEqual(FaceExpression.Surprised,go.GetComponent<CharacterVisualDriver>().Expression);
            Assert.Less(Vector2.Distance(rig.weaponMount.position,rig.rightArm.TransformPoint(Vector3.down*rig.skin.arm.bounds.size.y)),.03f);
            Float(expert,"heldMalfunctionChance",0);yield return new WaitForSeconds(4);deadline=Time.time+1.2f;
            while(expert.Launches==0&&Time.time<deadline){expert.Tick(target,target.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(1,expert.Launches);Assert.AreEqual(1,expert.HeldMalfunctions);Assert.AreEqual(2,expert.PoolSize);
        }
        [UnityTest] public IEnumerator HeldBlastCanDefeatItsOwnerWithoutChangingGeneralSelfDamageRules()
        {
            var owner=Victim(new Vector2(X,.8f),CombatTeam.Enemy);owner.ApplyDamage(new DamageInfo(90,CombatTeam.Player));
            Assert.IsFalse(owner.ApplyDamage(new DamageInfo(100,CombatTeam.Enemy,owner.gameObject,null,true)));
            var b=Bundle();b.Prepare(owner.gameObject,owner.transform.position,Color.yellow,false);b.ExplodeHeld(owner.transform.position);
            Assert.IsTrue(owner.IsDefeated);Assert.AreEqual(1,b.LastAffected);b.Detonate();Assert.AreEqual(1,b.LastAffected);yield return null;
        }
        [UnityTest] public IEnumerator ActualHitscanBurstSuppressesSeveralEnemiesWithoutDamageOrForcedPanicThenDecays()
        {
            var source=Keep(new GameObject("S0242ShotSource"));source.transform.position=new Vector2(X-8,1.9f);var health=source.AddComponent<Health>();var so=new SerializedObject(health);so.FindProperty("team").enumValueIndex=(int)CombatTeam.Player;so.ApplyModifiedPropertiesWithoutUndo();var shots=source.AddComponent<AttackExecutor>();
            var enemies=new EnemyCombat[3];var hp=new float[3];
            for(int i=0;i<3;i++)
            {var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X+i*4,.71f),Quaternion.identity));go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;go.GetComponent<LarperBrain>().enabled=false;enemies[i]=go.GetComponent<EnemyCombat>();Float(enemies[i],"defensiveCrouchProbability",i==0?1:0);Float(enemies[i],"suppressionRetreatChance",0);hp[i]=go.GetComponent<Health>().Current;}
            yield return new WaitForSeconds(.2f);var weapon=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),72);
            for(int i=0;i<3;i++){Assert.IsTrue(shots.Attack(weapon,Vector2.right,Time.time));yield return new WaitForSeconds(weapon.Definition.FiringInterval+.02f);}
            Assert.IsTrue(enemies[0].IsDefensivelyCrouching);Assert.IsFalse(enemies[1].IsDefensivelyCrouching);
            for(int i=0;i<3;i++){Assert.Greater(enemies[i].Suppression,0);Assert.AreEqual(hp[i],enemies[i].GetComponent<Health>().Current);Assert.AreEqual(0,enemies[i].GetComponent<LarperAwareness>().CollapseReactions);}
            yield return new WaitForSeconds(10);foreach(var e in enemies){Assert.AreEqual(0,e.Suppression);Assert.IsFalse(e.IsDefensivelyCrouching);}
        }
    }
}
#endif
