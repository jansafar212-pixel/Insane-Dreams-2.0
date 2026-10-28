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
    public sealed class Sprint0027NavigationTests
    {
        readonly List<GameObject> objects=new List<GameObject>();bool background;
        GameObject Keep(GameObject o){objects.Add(o);return o;}
        void Solid(Vector2 p,Vector2 size){var g=Keep(new GameObject("NavigationSolid"));g.transform.position=p;g.AddComponent<BoxCollider2D>().size=size;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;Solid(new Vector2(27000,-.5f),new Vector2(100,1));yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in objects)if(g!=null)Object.Destroy(g);objects.Clear();yield return null;Application.runInBackground=background;}
        [UnityTest]public IEnumerator PatrolRejectsLowCeilingHopAndTurnsWithoutChangingFloors()
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(27000,.72f),Quaternion.identity));g.GetComponent<EnemyCombat>().enabled=false;var brain=g.GetComponent<LarperBrain>();brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            Solid(new Vector2(27001.3f,.4f),new Vector2(.8f,.8f));Solid(new Vector2(27001,1.65f),new Vector2(3,.2f));float until=Time.time+3;bool jumped=false;while(Time.time<until){jumped|=g.GetComponent<PlayerMotor2D>().JumpsUsed>0;yield return new WaitForFixedUpdate();}Assert.IsFalse(jumped);Assert.Less(g.transform.position.x,27000);Assert.Less(Mathf.Abs(g.transform.position.y-.72f),.12f);
        }
        [UnityTest]public IEnumerator RepeatedGunfireBelowPlatformRefreshesUncertainMemoryWithoutGrantingVision()
        {
            Solid(new Vector2(27000,3),new Vector2(10,.4f));
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(27000,3.92f),Quaternion.identity));
            g.GetComponent<LarperBrain>().enabled=false;var c=g.GetComponent<EnemyCombat>();c.BindTarget(null);
            var source=Keep(new GameObject("LowerGunfire"));source.transform.position=new Vector2(27002.37f,.72f);yield return null;
            for(int i=0;i<3;i++)
            {
                CombatNoise.Report(new CombatNoiseEvent(source,CombatTeam.Player,CombatNoiseKind.Gunfire,source.transform.position,source.transform.position+Vector3.right*2,1));
                yield return new WaitForSeconds(.3f);
            }
            Assert.AreEqual(3,c.NoiseEventsHeard);Assert.AreEqual(ThreatAwareness.Suspicious,c.Awareness);
            Assert.Greater(c.HeardMemoryRemaining,3);Assert.Greater(Vector2.Distance(c.HeardPosition,source.transform.position),.01f);
            Assert.IsFalse(c.TargetVisible);Assert.AreEqual(0,g.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest]public IEnumerator GunfireOnNearbyUpperFloorIsHeardButDoesNotAuthorizeSightOrShooting()
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(27000,.72f),Quaternion.identity));g.GetComponent<LarperBrain>().enabled=false;var c=g.GetComponent<EnemyCombat>();c.BindTarget(null);Solid(new Vector2(27000,3),new Vector2(10,.4f));yield return null;
            var source=Keep(new GameObject("UpperGunfire"));source.transform.position=new Vector2(27002,4);CombatNoise.Report(new CombatNoiseEvent(source,CombatTeam.Player,CombatNoiseKind.Gunfire,source.transform.position,new Vector2(27005,4),1));yield return null;Assert.AreEqual(ThreatAwareness.Suspicious,c.Awareness);Assert.Greater(c.HeardPosition.y,3);Assert.IsFalse(c.TargetVisible);Assert.AreEqual(0,g.GetComponent<AttackExecutor>().ShotsFired);
        }
    }
}
#endif
