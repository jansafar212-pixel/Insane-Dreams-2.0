#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Prototype;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint024EnemyTests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private bool background;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private GameObject Solid(Vector2 p,Vector2 size){var go=Keep(new GameObject("S024Solid"));go.transform.position=p;go.AddComponent<BoxCollider2D>().size=size;return go;}
        [UnitySetUp] public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;Solid(new Vector2(36000,-.5f),new Vector2(60,1));yield return null;}
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;}
        [UnityTest] public IEnumerator SamuraiTelegraphsCommitsWithoutRetargetingAndWallBlocksStrike()
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(36000,.71f),Quaternion.identity));
            go.GetComponent<EnemyCombat>().enabled=false;go.GetComponent<LarperBrain>().enabled=false;
            var target=Keep(new GameObject("S024Target"));target.transform.position=new Vector2(36001,.71f);target.layer=2;target.AddComponent<BoxCollider2D>();var hp=target.AddComponent<Health>();
            var samurai=go.GetComponent<SamuraiCombat>();var executor=go.GetComponent<AttackExecutor>();
            yield return new WaitForSeconds(.9f);
            for(int i=0;i<10&&!samurai.Winding;i++){samurai.Tick(hp,target.transform.position,true,true,true);yield return null;}
            Assert.IsTrue(samurai.Winding);Assert.AreEqual(0,executor.ShotsFired);var committed=samurai.CommittedAim;
            Solid(new Vector2(36000.4f,1),new Vector2(.1f,2));Physics2D.SyncTransforms();float end=Time.time+.6f;
            while(Time.time<end){samurai.Tick(hp,target.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(1,executor.ShotsFired);Assert.AreEqual(100,hp.Current);Assert.AreEqual(committed,executor.LastShotDirection);
            Assert.IsFalse(samurai.Winding);Assert.AreEqual(EngagementState.Reacting,samurai.Tick(hp,target.transform.position,true,true,true));
        }
        [UnityTest] public IEnumerator WeakImpactKeepsControlAndForcedHeavyKnockdownRecoversWithoutRootTeleport()
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(36000,.85f),Quaternion.identity));
            var reaction=go.AddComponent<HitReaction2D>();var hp=go.GetComponent<Health>();yield return new WaitForSeconds(.2f);
            var so=new SerializedObject(reaction);so.FindProperty("knockdownChance").floatValue=1;so.ApplyModifiedPropertiesWithoutUndo();
            hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(1,CombatTeam.Enemy,null,new PhysicalImpact(go.transform.position,Vector2.right,.9f)));Assert.IsFalse(reaction.KnockedDown);
            Vector2 before=go.transform.position;hp.ApplyDamage(new DamageInfo(1,CombatTeam.Enemy,null,new PhysicalImpact(go.transform.position,Vector2.right,4)));
            Assert.IsTrue(reaction.KnockedDown);Assert.IsFalse(go.GetComponent<InsaneDreams.Input.PlayerActions>().enabled);
            yield return new WaitForSeconds(.75f);Assert.IsFalse(reaction.KnockedDown);Assert.IsTrue(go.GetComponent<InsaneDreams.Input.PlayerActions>().enabled);
            Assert.Less(Vector2.Distance(before,go.transform.position),.15f);
            reaction.CancelReaction();hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(1,CombatTeam.Enemy,null,new PhysicalImpact(go.transform.position,Vector2.right,4)));
            Assert.IsTrue(reaction.KnockedDown);go.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Respawn();
            Assert.IsFalse(reaction.KnockedDown);Assert.IsTrue(go.GetComponent<InsaneDreams.Input.PlayerActions>().enabled);
        }
        private Health Dummy(Vector2 position,CombatTeam team)
        {
            var go=Keep(new GameObject("S024Victim"));go.layer=2;go.transform.position=position;go.AddComponent<BoxCollider2D>().size=new Vector2(.6f,1);
            var hp=go.AddComponent<Health>();var so=new SerializedObject(hp);so.FindProperty("team").enumValueIndex=(int)team;so.ApplyModifiedPropertiesWithoutUndo();return hp;
        }
        [UnityTest] public IEnumerator FireworkBlastDeduplicatesVictimsRespectsWallsAndOnlyOptInAttacksHurtAllies()
        {
            var source=Keep(new GameObject("S024Source"));source.transform.position=new Vector2(35997,1);
            var player=Dummy(new Vector2(36000,.8f),CombatTeam.Player);var ally=Dummy(new Vector2(36001,.8f),CombatTeam.Enemy);var shielded=Dummy(new Vector2(36002,.8f),CombatTeam.Player);
            var child=new GameObject("SecondHitbox");child.layer=2;child.transform.SetParent(player.transform,false);child.AddComponent<BoxCollider2D>().size=new Vector2(.4f,.8f);
            Solid(new Vector2(36001.5f,1),new Vector2(.15f,3));Physics2D.SyncTransforms();
            var rocket=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab"))).GetComponent<FireworkProjectile>();
            rocket.Prepare(source,new Vector2(36000,.8f),Color.cyan,false);rocket.Detonate();
            Assert.AreEqual(76,player.Current,.01f);Assert.AreEqual(100,ally.Current);Assert.AreEqual(100,shielded.Current);Assert.AreEqual(1,rocket.LastAffected);
            player.Restore();rocket.gameObject.SetActive(false);rocket.Prepare(source,new Vector2(36000,.8f),Color.magenta,true);rocket.Detonate();
            Assert.Less(player.Current,100);Assert.Less(ally.Current,100);Assert.AreEqual(100,shielded.Current);Assert.AreEqual(2,rocket.LastAffected);
            yield return new WaitForSeconds(.4f);Assert.IsFalse(rocket.Busy);
        }
        [UnityTest] public IEnumerator FireworksTelegraphLocksDestinationAndReusesTwoProjectileInstances()
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/FireworksExpert.prefab"),new Vector3(36000,.71f),Quaternion.identity));
            go.GetComponent<EnemyCombat>().enabled=false;go.GetComponent<LarperBrain>().enabled=false;var expert=go.GetComponent<FireworksCombat>();var target=Dummy(new Vector2(36007,.8f),CombatTeam.Player);
            var settings=new SerializedObject(expert);settings.FindProperty("heldMalfunctionChance").floatValue=0;settings.ApplyModifiedPropertiesWithoutUndo();
            yield return new WaitForSeconds(2.2f);expert.Tick(target,target.transform.position,true,true,true);Assert.IsTrue(expert.Preparing);Assert.AreEqual(0,expert.Launches);Assert.AreEqual(2,expert.PoolSize);
            var active=Object.FindObjectsByType<FireworkProjectile>(FindObjectsSortMode.None);var rocket=System.Array.Find(active,p=>p.Busy);Assert.IsNotNull(rocket);Vector2 destination=rocket.Destination;
            target.transform.position=new Vector2(36010,.8f);float until=Time.time+1;
            while(Time.time<until){expert.Tick(target,target.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(1,expert.Launches);Assert.AreEqual(destination,rocket.Destination);Assert.IsFalse(expert.Preparing);
            float done=Time.realtimeSinceStartup+4;while(rocket.Busy&&Time.realtimeSinceStartup<done)yield return null;
            Assert.IsFalse(rocket.Busy);Assert.AreEqual(2,expert.PoolSize);
        }
    }
}
#endif
