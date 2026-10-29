#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    public sealed class Sprint0029CombatTests
    {
        readonly List<GameObject> objects=new List<GameObject>();bool background;float volume;Random.State random;
        GameObject Keep(GameObject g){objects.Add(g);return g;}
        GameObject Actor(string path,float x,float y){var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),new Vector3(x,y),Quaternion.identity));var brain=g.GetComponent<LarperBrain>();brain.enabled=false;brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>(path.Contains("Officer")?"Assets/Data/FirstOperation/Officer.asset":"Assets/Data/Enemies/Larper.asset"));g.GetComponent<EnemyCombat>().BindTarget(null);return g;}
        GameObject Solid(string name,Vector2 at,Vector2 size){var g=Keep(new GameObject(name));g.transform.position=at;g.AddComponent<BoxCollider2D>().size=size;return g;}
        Health Target(Vector2 at){var g=Solid("ControlledTarget",at,new Vector2(.8f,1.4f));g.layer=2;var hp=g.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("maximum").floatValue=10000;s.FindProperty("team").enumValueIndex=(int)CombatTeam.Player;s.ApplyModifiedPropertiesWithoutUndo();hp.Restore();return hp;}
        Camera CameraAt(float x,float y){var c=Keep(new GameObject("ControlledCombatCamera")).AddComponent<Camera>();c.orthographic=true;c.orthographicSize=6;c.aspect=16f/9;c.transform.position=new Vector3(x,y,-10);return c;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;random=Random.state;Random.InitState(2901);yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in objects)if(g!=null)Object.Destroy(g);objects.Clear();yield return null;Application.runInBackground=background;AudioListener.volume=volume;Random.state=random;}
        [UnityTest]public IEnumerator OfficerRequiresClearMuzzleEvenWhenEyesSeeOverCover()
        {
            var g=Actor("Assets/Data/FirstOperation/Officer.prefab",71000,3.7f);g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;var c=g.GetComponent<EnemyCombat>();var hp=Target(new Vector2(71006,3.7f));c.BindGameplayCamera(CameraAt(71003,4));c.BindTarget(hp);yield return new WaitForSeconds(.1f);
            Vector2 muzzle=g.GetComponent<AttackExecutor>().Origin;var shield=Solid("MuzzleOnlyShield",muzzle+Vector2.right*.25f,new Vector2(.08f,.22f));Physics2D.SyncTransforms();yield return new WaitForSeconds(1.2f);
            Assert.IsTrue(c.TargetVisible,"Eye ray deliberately passes above shield");Assert.AreEqual(0,g.GetComponent<AttackExecutor>().ShotsFired);Assert.AreEqual(7,c.Weapon.MagazineAmmo);Assert.IsFalse(g.GetComponent<OfficerShotTelegraph>().Winding);
            Object.Destroy(shield);yield return new WaitForSeconds(2);Assert.Greater(g.GetComponent<AttackExecutor>().ShotsFired,0);Assert.Less(hp.Current,hp.Maximum);
        }
        [UnityTest]public IEnumerator EnemyGunfireAndExplosionsAreHeardAcrossDeckWithoutExactTargetOrOffscreenFire()
        {
            var g=Actor("Assets/Prefabs/Enemies/Larper.prefab",72000,4.2f);g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;var c=g.GetComponent<EnemyCombat>();var hp=Target(new Vector2(72004,.7f));c.BindTarget(hp);c.BindGameplayCamera(CameraAt(0,3));Solid("OccludingDeck",new Vector2(72003,2.8f),new Vector2(18,.5f));var ally=Keep(new GameObject("HeardAlly"));ally.transform.position=new Vector2(72006,.7f);yield return null;
            CombatNoise.Report(new CombatNoiseEvent(ally,CombatTeam.Enemy,CombatNoiseKind.Gunfire,ally.transform.position,ally.transform.position+Vector3.right,1));Assert.AreEqual(ThreatAwareness.Suspicious,c.Awareness);Assert.Greater(Vector2.Distance(c.HeardPosition,hp.transform.position),1);Assert.AreEqual(0,c.Suppression);yield return new WaitForSeconds(.3f);Assert.IsFalse(c.TargetVisible);Assert.AreEqual(0,g.GetComponent<AttackExecutor>().ShotsFired);
            Vector2 blast=new Vector2(72008,.7f);CombatNoise.Report(new CombatNoiseEvent(ally,CombatTeam.Enemy,CombatNoiseKind.Explosion,blast,blast,1));Assert.Less(Vector2.Distance(c.HeardPosition,blast),2);Vector2 remembered=c.HeardPosition;yield return new WaitForSeconds(.3f);CombatNoise.Report(new CombatNoiseEvent(ally,CombatTeam.Enemy,CombatNoiseKind.Explosion,blast+Vector2.right*60,blast+Vector2.right*60,1));Assert.AreEqual(remembered,c.HeardPosition);
        }
        [UnityTest]public IEnumerator LarperFirstVisualAttackWaitsAndBriefOcclusionDoesNotRestartLongDelay()
        {
            var g=Actor("Assets/Prefabs/Enemies/Larper.prefab",73000,.72f);g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;var c=g.GetComponent<EnemyCombat>();var hp=Target(new Vector2(73006,.72f));c.BindGameplayCamera(CameraAt(73003,3));float first=-1,start=Time.time;g.GetComponent<AttackExecutor>().Fired+=(a,b,d)=>{if(first<0)first=Time.time;};c.BindTarget(hp);yield return new WaitForSeconds(.55f);Assert.Less(first,0);float deadline=Time.time+2;while(first<0&&Time.time<deadline)yield return null;Assert.That(first-start,Is.InRange(.69f,1.5f));
            var wall=Solid("BriefOcclusion",new Vector2(73003,1),new Vector2(.1f,4));Physics2D.SyncTransforms();yield return new WaitForSeconds(.16f);Object.Destroy(wall);yield return new WaitForSeconds(.2f);float ready=(float)typeof(EnemyCombat).GetField("readyAt",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(c);Assert.Less(ready,Time.time+.1f,"Short LOS interruptions retain existing alertness");
        }
        [UnityTest]public IEnumerator CloseBoxEdgeRecoversWithRunUpButTallWallDoesNotCauseEndlessJumps()
        {
            Solid("NavigationFloor",new Vector2(74000,-.5f),new Vector2(40,1));var box=Solid("LowBox",new Vector2(74001.1f,.5625f),new Vector2(1.3f,1.125f));var g=Actor("Assets/Prefabs/Enemies/Larper.prefab",74000,.72f);g.GetComponent<EnemyCombat>().enabled=false;var brain=g.GetComponent<LarperBrain>();brain.enabled=true;brain.DecisionsSuspended=true;yield return new WaitForSeconds(.2f);brain.SetPursuit(1);float minX=74000,maxY=.72f,deadline=Time.time+6;
            while(Time.time<deadline&&g.transform.position.x<74002.5f){yield return new WaitForFixedUpdate();minX=Mathf.Min(minX,g.transform.position.x);maxY=Mathf.Max(maxY,g.transform.position.y);}Assert.Less(minX,73999.8f,"Back off to gain clearance");Assert.GreaterOrEqual(g.transform.position.x,74002.5f,"Cross the reachable box");Assert.Greater(maxY,1.8f);
            brain.SetPursuit(0);Object.Destroy(box);Solid("ImpossibleWall",new Vector2(74007,2.5f),new Vector2(1,5));yield return new WaitForFixedUpdate();var rb=g.GetComponent<Rigidbody2D>();rb.position=new Vector2(74006,.72f);rb.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();brain.SetPursuit(1);yield return new WaitForSeconds(3);Assert.Less(g.transform.position.y,1);Assert.Less(g.transform.position.x,74006.2f);
        }
    }
}
#endif
