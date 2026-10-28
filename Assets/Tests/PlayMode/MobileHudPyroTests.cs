#if UNITY_EDITOR
using System.Collections;
using System.IO;
using InsaneDreams.Progression;
using InsaneDreams.Enemies;
using InsaneDreams.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace InsaneDreams.Tests {
 public class MobileHudPyroTests {
  bool originalBackground;float originalScale;Random.State originalRandom;
  [UnitySetUp]public IEnumerator PreserveGlobals(){originalBackground=Application.runInBackground;originalScale=Time.timeScale;originalRandom=Random.state;yield return null;}
  [UnityTearDown]public IEnumerator AlwaysCleanUp(){Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));if(GameSession.Instance!=null)Object.Destroy(GameSession.Instance.gameObject);yield return null;Time.timeScale=originalScale;Random.state=originalRandom;Application.runInBackground=originalBackground;foreach(var suffix in new[]{"",".bak",".tmp"}){string file=Path.Combine(Path.GetTempPath(),"AkumuMobileHud-test.json")+suffix;if(File.Exists(file))File.Delete(file);}}
  [UnityTest] public IEnumerator HudMetricsAndSafeArea() {
   bool background=Application.runInBackground;Application.runInBackground=true;
   string save=Path.Combine(Path.GetTempPath(),"AkumuMobileHud-test.json");
   var session=new GameObject("HudTestSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.3f);
   foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;}
   var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;var safe=root.Q("safeArea");
   var vitals=root.Q(className:"vitals-panel");var card=root.Q(className:"mission-card");var pause=root.Q<Button>("campaignPause");var face=root.Q("pauseVisual");
   string stage=face==null?"Before":"After";string dir=Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults");
   File.WriteAllText(Path.Combine(dir,"MobileHudPyro_"+stage+".json"),$"{{\"health\":\"{vitals.worldBound}\",\"objective\":\"{card.worldBound}\",\"pauseTouch\":\"{pause.worldBound}\",\"pauseVisible\":\"{(face??pause).worldBound}\"}}");
   yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,"MobileHudPyro_"+stage+".png"));yield return new WaitForEndOfFrame();yield return null;
   foreach(var size in new[]{new Vector2(960,720),new Vector2(1280,720),new Vector2(2340,1080)}) {
    root.style.width=size.x;root.style.height=size.y;safe.style.left=40;safe.style.right=30;safe.style.top=20;safe.style.bottom=20;yield return null;yield return null;
    Assert.IsFalse(vitals.worldBound.Overlaps(pause.worldBound));Assert.IsFalse(vitals.worldBound.Overlaps(card.worldBound));Assert.IsFalse(card.worldBound.Overlaps(root.Q("moveStick").worldBound));Assert.IsTrue(safe.worldBound.Contains(card.worldBound.center));Assert.GreaterOrEqual(pause.worldBound.width,123);Assert.GreaterOrEqual(pause.worldBound.height,55);
    if(face!=null){Assert.AreEqual(52,face.worldBound.width,1);Assert.AreEqual(40,face.worldBound.height,1);}
   }
   var mission=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>();var fields=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
   string longNotice="CHECKPOINT LOCKED - CLEAR ENTRANCE / STORAGE FIRST - 12 enemies remain (check both floors). Activate the storage mezzanine checkpoint, then reach processing.";
   mission.GetType().GetField("notice",fields).SetValue(mission,longNotice);mission.GetType().GetField("noticeUntil",fields).SetValue(mission,Time.time+30);yield return new WaitForSeconds(.2f);
   Assert.AreEqual(longNotice,root.Q<Label>("missionNotice").text);Assert.LessOrEqual(card.worldBound.height,156);var details=root.Q<ScrollView>("missionDetails");Assert.NotNull(details);details.scrollOffset=new Vector2(0,500);yield return null;if(details.contentContainer.layout.height>details.contentViewport.layout.height+1)Assert.Greater(details.scrollOffset.y,0);else Assert.LessOrEqual(details.contentContainer.layout.height,details.contentViewport.layout.height+1);
   using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=pause;pause.SendEvent(submit);}Assert.AreEqual(0,Time.timeScale);Object.FindAnyObjectByType<CampaignMissionBridge>().SetPaused(false);Assert.AreEqual(1,Time.timeScale);
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Object.Destroy(session.gameObject);yield return null;if(File.Exists(save))File.Delete(save);Application.runInBackground=background;
  }
  [UnityTest] public IEnumerator ActualBlastDamageRadiusAndVisualRingMatch() {
   var g=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab"));var b=g.GetComponent<FireworkProjectile>();
   var victims=new GameObject[3];for(int i=0;i<3;i++){victims[i]=new GameObject("BlastProbe");victims[i].layer=2;victims[i].transform.position=new Vector2(8000+new[]{0f,2.9f,3.4f}[i],30);victims[i].AddComponent<CircleCollider2D>().radius=.01f;victims[i].AddComponent<Health>();}Physics2D.SyncTransforms();
   b.Prepare(null,new Vector2(8000,30),Color.yellow,true);var ring=g.GetComponentInChildren<LineRenderer>();Assert.AreEqual(3.2f,Vector2.Distance(ring.GetPosition(0),new Vector2(8000,30)),.01f);
   b.ExplodeHeld(new Vector2(8000,30));Assert.AreEqual(68.8f,victims[0].GetComponent<Health>().Current,.01f);Assert.Less(victims[1].GetComponent<Health>().Current,100);Assert.AreEqual(100,victims[2].GetComponent<Health>().Current);b.Detonate();Assert.AreEqual(68.8f,victims[0].GetComponent<Health>().Current,.01f);Assert.AreEqual(35,b.PooledVisualCount);
   foreach(var v in victims)Object.Destroy(v);Object.Destroy(g);yield return null;
  }
  [UnityTest] public IEnumerator SustainedRealPoolThrowsFasterAndCloseRetreatNoLongerVetoesThrow() {
   bool background=Application.runInBackground;Application.runInBackground=true;var random=Random.state;float scale=Time.timeScale;Time.timeScale=4;
   var target=new GameObject("PyroTarget");target.transform.position=new Vector2(8506,20);var hp=target.AddComponent<Health>();hp.ProtectFor(200);
   var counts=new int[2];
   for(int run=0;run<2;run++){
    Random.InitState(203);var g=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/FireworksExpert.prefab"),new Vector3(8500,20),Quaternion.identity);g.GetComponent<LarperBrain>().enabled=false;g.GetComponent<EnemyCombat>().BindTarget(null);var permission=new SerializedObject(g.GetComponent<EnemyCombat>());permission.FindProperty("allowOffscreenFire").boolValue=true;permission.ApplyModifiedPropertiesWithoutUndo();g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;g.GetComponent<Health>().ProtectFor(200);var f=g.GetComponent<FireworksCombat>();var settings=new SerializedObject(f);settings.FindProperty("heldMalfunctionChance").floatValue=0;if(run==0){settings.FindProperty("recoverySeconds").floatValue=3;settings.FindProperty("recoveryVariation").floatValue=0;}settings.ApplyModifiedPropertiesWithoutUndo();
    float end=Time.time+28;while(Time.time<end){f.Tick(hp,target.transform.position,true,true,true);Assert.AreEqual(2,f.PoolSize);yield return null;}counts[run]=f.Launches;Object.Destroy(g);yield return null;
   }
   File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/MobileHudPyro_ThrowCounts.json"),$"{{\"seconds\":28,\"old\":{counts[0]},\"new\":{counts[1]}}}");Assert.GreaterOrEqual(counts[1],counts[0]*1.6f);
   var close=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/FireworksExpert.prefab"),new Vector3(8504,20),Quaternion.identity);close.GetComponent<LarperBrain>().enabled=false;close.GetComponent<EnemyCombat>().BindTarget(null);var closePermission=new SerializedObject(close.GetComponent<EnemyCombat>());closePermission.FindProperty("allowOffscreenFire").boolValue=true;closePermission.ApplyModifiedPropertiesWithoutUndo();close.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;close.GetComponent<Health>().ProtectFor(200);var combat=close.GetComponent<FireworksCombat>();float deadline=Time.time+5;while(combat.Launches==0&&Time.time<deadline){combat.Tick(hp,target.transform.position,true,true,true);yield return null;}Assert.Greater(combat.Launches,0,"A valid close-range retreat must not permanently veto throwing");
   Object.Destroy(close);Object.Destroy(target);yield return null;Random.state=random;Time.timeScale=scale;Application.runInBackground=background;
  }

 }
}
#endif
