#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0223OpeningTests
    {
        [System.Serializable] private sealed class Snapshot
        {public float time;public Vector3[] positions;public string[] states,covers;}
        [System.Serializable] private sealed class Trace
        {public float minimumHorizontalGap=100,longestCrowding;public List<Snapshot> samples=new List<Snapshot>();}
        [UnityTest] public IEnumerator ActualOpeningEncounterKeepsThreeActorsWithoutPersistentClustering()
        {
            bool background=Application.runInBackground;Application.runInBackground=true;var random=Random.state;Random.InitState(223);
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));
                var player=Object.FindAnyObjectByType<PlayerActions>();player.GetComponent<Health>().ProtectFor(60);
                string[] names={"EnemySpawn_WitnessPatrol","EnemySpawn_-7_0,8","Sprint012_Encounter_0"};
                yield return null;
                var points=Object.FindObjectsByType<EnemySpawnPoint>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                var opening=points.Where(p=>names.Contains(p.name)).OrderBy(p=>p.transform.position.x).Select(p=>p.Spawned.GetComponent<EnemyCombat>()).ToArray();
                Assert.AreEqual(3,opening.Length);Assert.GreaterOrEqual(opening[1].transform.position.x-opening[0].transform.position.x,2.5f);
                Assert.GreaterOrEqual(opening[2].transform.position.x-opening[1].transform.position.x,2.5f);
                var trace=new Trace();float[] crowded=new float[3];float start=Time.time;
                for(int sample=0;sample<120;sample++)
                {
                    yield return new WaitForSeconds(.1f);
                    int pair=0;
                    for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)
                    {
                        var delta=opening[i].transform.position-opening[j].transform.position;
                        if(Mathf.Abs(delta.y)<1.6f)
                        {
                            trace.minimumHorizontalGap=Mathf.Min(trace.minimumHorizontalGap,Mathf.Abs(delta.x));
                            crowded[pair]=Mathf.Abs(delta.x)<1?.1f+crowded[pair]:0;
                        }
                        else crowded[pair]=0;
                        trace.longestCrowding=Mathf.Max(trace.longestCrowding,crowded[pair++]);
                        if(opening[i].SelectedCover!=null)Assert.AreNotEqual(opening[i].SelectedCover,opening[j].SelectedCover);
                    }
                    if(sample%10==0)trace.samples.Add(new Snapshot{time=Time.time-start,positions=opening.Select(e=>e.transform.position).ToArray(),states=opening.Select(e=>e.State.ToString()).ToArray(),covers=opening.Select(e=>e.SelectedCover!=null?e.SelectedCover.name:"").ToArray()});
                }
                System.IO.File.WriteAllText("TestResults/Sprint0223OpeningTrace.json",JsonUtility.ToJson(trace,true));
                Assert.Less(trace.longestCrowding,1.5f,"Opening actors must not remain stacked on one small crate.");
                Assert.IsTrue(opening.All(e=>e.gameObject.activeInHierarchy&&!e.GetComponent<Health>().IsDefeated));
                Assert.AreEqual(7,Object.FindObjectsByType<LarperBrain>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length);
            }
            finally{Random.state=random;Application.runInBackground=background;}
        }
    }
}
#endif
