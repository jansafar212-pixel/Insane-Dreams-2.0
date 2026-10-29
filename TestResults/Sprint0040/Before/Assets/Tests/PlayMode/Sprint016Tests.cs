#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint016Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();
        private Camera camera;private bool background;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Solid(Vector2 position,Vector2 size){var go=Keep(new GameObject("S016Floor"));go.transform.position=position;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();}
        private void Float(Object target,string key,float value){var s=new SerializedObject(target);s.FindProperty(key).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;
            Solid(new Vector2(9000,-.5f),new Vector2(120,1));
            camera=Keep(new GameObject("S016Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4;camera.aspect=1;camera.transform.position=new Vector3(8980,1,-10);yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;}
        private EnemyCombat Enemy(float x,float y=.85f)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(x,y),Quaternion.identity));
            go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);return e;
        }
        private EnemySpawnPoint Point(float x,float y=.85f)
        {
            var go=Keep(new GameObject("S016Point"));go.transform.position=new Vector3(x,y);var p=go.AddComponent<EnemySpawnPoint>();var s=new SerializedObject(p);
            s.FindProperty("archetype").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset");s.FindProperty("spawnOnStart").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();p.BindGameplayCamera(camera);return p;
        }
        private LarperBrain Spawn(EnemySpawnPoint point){var actor=point.Spawn();objects.Add(actor.gameObject);actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;return actor;}
        [UnityTest] public IEnumerator NonoverlappingButCloseSpawnWaitsThenRestoresFullPopulationAtAuthoredLocations()
        {
            var a=Point(9000);var b=Point(9001);var c=Point(9003.4f);var first=Spawn(a);var second=Spawn(b);var third=Spawn(c);
            yield return new WaitForSeconds(.3f);Assert.IsTrue(first.gameObject.activeSelf);Assert.IsFalse(second.gameObject.activeSelf);Assert.IsTrue(b.IsPending);Assert.IsTrue(third.gameObject.activeSelf);
            Assert.AreEqual(.8f,first.GetComponent<BoxCollider2D>().size.x);Assert.AreEqual(1.5f,new SerializedObject(b).FindProperty("minimumSpawnSpacing").floatValue);
            first.GetComponent<Rigidbody2D>().position=new Vector2(8998.5f,.85f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.3f);
            Assert.IsTrue(second.gameObject.activeSelf);Assert.IsFalse(b.IsPending);Assert.AreEqual(9001,second.transform.position.x,.01f);
            Assert.AreEqual(3,new[]{first,second,third}.Count(e=>e.gameObject.activeSelf));Assert.AreSame(second,b.Spawn());
        }
        [UnityTest] public IEnumerator SeparatePlatformsAndExplicitDenseSpacingRemainPossible()
        {
            Enemy(9000);Solid(new Vector2(9000,2.5f),new Vector2(8,1));var high=Point(9000,3.85f);var upper=Spawn(high);
            var dense=Point(9001);Float(dense,"minimumSpawnSpacing",.9f);var neighbor=Spawn(dense);
            yield return new WaitForSeconds(.3f);Assert.IsTrue(upper.gameObject.activeSelf);Assert.IsTrue(neighbor.gameObject.activeSelf);Assert.AreEqual(3.85f,upper.transform.position.y,.01f);
            Assert.IsFalse(upper.GetComponent<BoxCollider2D>().IsTouching(neighbor.GetComponent<BoxCollider2D>()));
        }
        private IEnumerator Positions(int count)
        {
            camera.transform.position=new Vector3(9010,1,-10);camera.orthographicSize=25;
            var player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(9000,.85f),Quaternion.identity));player.GetComponent<Health>().ProtectFor(60);
            var actors=new List<EnemyCombat>();for(int i=0;i<count;i++){var e=Enemy(9010+i*2.5f);e.BindTarget(player.GetComponent<Health>());actors.Add(e);}
            yield return new WaitForSeconds(.6f);foreach(var e in actors){Assert.IsTrue(e.TargetVisible);Assert.IsTrue(e.HasCombatPosition);Assert.AreEqual(1.75f,new SerializedObject(e).FindProperty("minimumCombatSpacing").floatValue);Assert.AreNotEqual(EngagementState.Withdrawing,e.State);}
            var held=actors.Select(e=>e.CombatPosition).ToArray();for(int i=0;i<count;i++)for(int j=i+1;j<count;j++)Assert.GreaterOrEqual(Vector2.Distance(held[i],held[j]),1.749f);
            yield return new WaitForSeconds(.3f);for(int i=0;i<count;i++)Assert.Less(Vector2.Distance(held[i],actors[i].CombatPosition),.01f,"Valid reservations should not oscillate (allow sub-centimeter physics settling).");
            actors[0].gameObject.SetActive(false);Assert.IsFalse(actors[0].HasCombatPosition);
        }
        [UnityTest] public IEnumerator TwoLarpersKeepComfortableStablePositions(){yield return Positions(2);}
        [UnityTest] public IEnumerator ThreeLarpersKeepComfortableStablePositions(){yield return Positions(3);}
        [UnityTest] public IEnumerator FiveLarpersKeepComfortableStablePositions(){yield return Positions(5);}
    }
}
#endif
