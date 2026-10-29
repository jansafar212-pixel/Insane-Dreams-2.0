#if UNITY_EDITOR
using System.Collections;
using InsaneDreams.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0027MineTests
    {
        Scene scene;bool background;
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;scene=SceneManager.CreateScene("Sprint0027_ControlledMineTest");yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){yield return SceneManager.UnloadSceneAsync(scene);Application.runInBackground=background;}
        [UnityTest]public IEnumerator PlayerAndEnemyActivateEachMineOnlyOnceIncludingEarlyOverlap()
        {
            for(int i=0;i<2;i++)
            {
                var pos=new Vector2(33000+i*8,.08f);var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Sprint0027/EnvironmentalMine.prefab"),pos,Quaternion.identity);SceneManager.MoveGameObjectToScene(root,scene);var mine=root.GetComponent<EnvironmentalMine2D>();var actor=new GameObject("MineContactActor");SceneManager.MoveGameObjectToScene(actor,scene);actor.layer=2;actor.transform.position=pos+Vector2.up*.6f;actor.AddComponent<BoxCollider2D>().size=new Vector2(.5f,1.3f);actor.AddComponent<Rigidbody2D>().gravityScale=0;var hp=actor.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)(i==0?CombatTeam.Enemy:CombatTeam.Player);s.ApplyModifiedPropertiesWithoutUndo();int damageEvents=0;hp.Damaged+=_=>damageEvents++;
                yield return new WaitForSeconds(.5f);Assert.IsTrue(mine.Activated);Assert.AreEqual(1,mine.Activations);Assert.AreEqual(1,damageEvents);Assert.Less(hp.Current,100);float remaining=hp.Current;yield return new WaitForSeconds(.5f);Assert.AreEqual(remaining,hp.Current);Assert.IsFalse(root.GetComponent<Collider2D>().enabled);
            }
        }
    }
}
#endif
