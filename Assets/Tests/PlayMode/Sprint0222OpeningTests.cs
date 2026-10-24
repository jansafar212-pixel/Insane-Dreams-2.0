#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0222OpeningTests
    {
        private bool background;
        private static readonly string[] openingNames={"EnemySpawn_WitnessPatrol","EnemySpawn_-7_0,8","Sprint012_Encounter_0"};
        private EnemySpawnPoint[] Points()=>SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<EnemySpawnPoint>(true)).ToArray();
        private IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));
            Object.FindAnyObjectByType<PlayerActions>().GetComponent<Health>().ProtectFor(60);yield return null;
        }
        [UnitySetUp] public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;yield return Load();}
        [UnityTearDown] public IEnumerator Cleanup(){Application.runInBackground=background;yield return null;}
        private void AssertComposition()
        {
            var all=Points();var initial=all.Where(p=>new SerializedObject(p).FindProperty("spawnOnStart").boolValue).ToArray();
            Assert.AreEqual(9,all.Length);Assert.AreEqual(7,initial.Length);
            Assert.AreEqual(7,SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<LarperBrain>(true)).Count());
            var opening=all.Where(p=>openingNames.Contains(p.name)).ToArray();Assert.AreEqual(3,opening.Length);
            foreach(var p in initial)
            {
                Assert.IsNotNull(p.Spawned);Assert.AreSame(new SerializedObject(p).FindProperty("placedActor").objectReferenceValue,p.Spawned);
                Assert.IsTrue(p.Spawned.gameObject.activeInHierarchy);Assert.IsFalse(p.IsPending);Assert.AreEqual(SpawnBlockReason.None,p.BlockReason);
                Assert.AreSame(p.Archetype,p.Spawned.Archetype);
            }
            foreach(var p in opening){Assert.IsFalse(p.Spawned.GetComponent<Health>().IsDefeated);Assert.That(p.transform.position.x,Is.InRange(-10,-4.5f));}
        }
        [UnityTest] public IEnumerator AllThreeOpeningAndAllSevenStartupResidentsExistBeforeFirstControlInput()
        {
            AssertComposition();var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(1);var hits=new RaycastHit2D[8];
            foreach(var p in Points().Where(p=>openingNames.Contains(p.name)))
            {
                var box=p.Spawned.GetComponent<BoxCollider2D>();
                for(int side=-1;side<=1;side+=2)
                {
                    int count=Physics2D.Raycast(new Vector2(box.bounds.center.x+side*box.bounds.extents.x*.8f,box.bounds.min.y+.04f),Vector2.down,filter,hits,.6f);
                    Assert.IsTrue(Enumerable.Range(0,count).Any(i=>hits[i].normal.y>.7f),p.name+" needs supported terrain.");
                }
            }
            var actors=Points().Where(p=>p.Spawned!=null).Select(p=>p.Spawned).ToArray();foreach(var p in Points().Where(p=>p.Spawned!=null))for(int i=0;i<3;i++)Assert.AreSame(p.Spawned,p.Spawn());
            yield return new WaitForSeconds(.2f);Assert.AreEqual(actors.Length,SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<LarperBrain>(true)).Count());
        }
        [UnityTest] public IEnumerator RestartRestoresExactlyThreeWithoutDuplicatesAndBacktrackingDoesNotReviveDefeat()
        {
            var p=Points().First(p=>p.name==openingNames[0]);p.Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));
            var actor=p.Spawned;p.Spawn();Object.FindAnyObjectByType<PlayerActions>().transform.localScale=new Vector3(-1,1,1);
            yield return new WaitForSeconds(.2f);Assert.IsTrue(actor.GetComponent<Health>().IsDefeated);Assert.IsFalse(actor.gameObject.activeSelf);Assert.IsFalse(p.IsPending);
            yield return Load();AssertComposition();yield return Load();AssertComposition();
        }
    }
}
#endif
