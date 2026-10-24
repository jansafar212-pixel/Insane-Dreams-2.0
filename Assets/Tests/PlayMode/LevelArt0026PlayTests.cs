#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class LevelArt0026PlayTests
    {
        bool background;float volume;string save;GameSession session;
        [UnitySetUp] public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;Assert.IsNull(GameSession.Instance);save=Path.Combine(Path.GetTempPath(),"AkumuArt26-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("Art26Session").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);yield return null;}
        [UnityTearDown] public IEnumerator Cleanup(){if(SceneManager.GetActiveScene().name!="Warehouse_Testbed")yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));if(session!=null)Object.Destroy(session.gameObject);yield return null;foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);AudioListener.volume=volume;Application.runInBackground=background;}
        [UnityTest] public IEnumerator ActualWarehouseShowsNewCratesWithoutChangingTheirCollisionOrLadderEndpoints()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.3f);
            foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;}
            var sprites=Object.FindObjectsByType<SpriteRenderer>().Where(r=>r.gameObject.scene==SceneManager.GetActiveScene()&&r.enabled&&AssetDatabase.GetAssetPath(r.sprite).StartsWith("Assets/Art/LevelArt0026")).ToArray();Assert.Greater(sprites.Length,60);
            var decoration=SceneManager.GetActiveScene().GetRootGameObjects().Single(g=>g.name=="HandmadeEnvironmentDetails");foreach(var c in decoration.GetComponentsInChildren<Component>())Assert.IsTrue(c is Transform||c is SpriteRenderer);Assert.AreEqual(19,decoration.GetComponentsInChildren<SpriteRenderer>().Length);
            foreach(var r in sprites.Where(r=>r.name=="LevelArt0026_Visual"))
            {
                var box=r.transform.parent.GetComponent<BoxCollider2D>();if(box==null||box.isTrigger)continue;Assert.Less(Vector2.Distance(box.bounds.size,r.bounds.size),.0002f);Assert.Less(Vector2.Distance(box.bounds.center,r.bounds.center),.0002f);
                Vector2 start=(Vector2)box.bounds.center+Vector2.left*(box.bounds.extents.x+.1f);var hit=Physics2D.Raycast(start,Vector2.right,.2f,1<<box.gameObject.layer);Assert.AreEqual(box,hit.collider,"Solid crate ray: "+box.name);
            }
            foreach(var ladder in Object.FindObjectsByType<LadderVisual2D>())
            {
                Assert.AreEqual(ladder.BottomWorld,ladder.BottomCap.bounds.min.y,.0002f);Assert.AreEqual(ladder.TopWorld,ladder.TopCap.bounds.max.y,.0002f);Assert.AreEqual(32,ladder.Body.sprite.pixelsPerUnit);
            }
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/HandmadeDetails_Runtime.png"));yield return new WaitForEndOfFrame();yield return null;
            var player=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>().Player;player.GetComponent<InsaneDreams.Combat.Health>().ProtectFor(120);
            var firstLadder=Object.FindObjectsByType<LadderVisual2D>().OrderBy(l=>l.transform.position.x).First();var body=player.GetComponent<Rigidbody2D>();body.position=new Vector2(firstLadder.transform.position.x+2,6.11f);body.linearVelocity=Vector2.zero;player.transform.position=body.position;Physics2D.SyncTransforms();Object.FindAnyObjectByType<InsaneDreams.CameraSystem.WarehouseCameraFollow>().SnapToTarget();yield return new WaitForSeconds(.15f);
            foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;}
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/HandmadeDetails_LadderRuntime.png"));yield return new WaitForEndOfFrame();yield return null;
            body.position=new Vector2(18,6.11f);body.linearVelocity=Vector2.zero;player.transform.position=body.position;Physics2D.SyncTransforms();Object.FindAnyObjectByType<InsaneDreams.CameraSystem.WarehouseCameraFollow>().SnapToTarget();yield return new WaitForSeconds(.15f);foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;}
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/HandmadeDetails_ProcessingRuntime.png"));yield return new WaitForEndOfFrame();yield return null;
        }
    }
}
#endif
