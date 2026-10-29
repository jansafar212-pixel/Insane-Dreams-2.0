#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0036PresentationTests
    {
        GameSession session;string path;bool background;
        [UnitySetUp]public IEnumerator Setup(){Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;path=Path.Combine(Path.GetTempPath(),"Akumu36-"+Guid.NewGuid()+".json");session=new GameObject("PresentationSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),path);yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Object.Destroy(session.gameObject);yield return null;Application.runInBackground=background;foreach(var s in new[]{"",".bak",".tmp"})if(File.Exists(path+s))File.Delete(path+s);}
        void Draw(){var f=Object.FindAnyObjectByType<GameFlowController>();f.GetType().GetMethod("Draw",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(f,null);}
        IEnumerator Capture(string name){yield return null;yield return new WaitForEndOfFrame();Directory.CreateDirectory("TestResults/Sprint0036");ScreenCapture.CaptureScreenshot(Path.GetFullPath("TestResults/Sprint0036/"+name+".png"));yield return new WaitForEndOfFrame();}
        [UnityTest]public IEnumerator ScreensCategoriesAndResponsiveCardsUseRealInventoryWithoutDuplicates()
        {
            Assert.AreEqual(10000,session.Data.balance);yield return Capture("MainMenu");session.Navigate(FlowScreen.Settings);Draw();yield return Capture("Settings");session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");Draw();
            var flow=Object.FindAnyObjectByType<GameFlowController>();var root=flow.GetComponent<UIDocument>().rootVisualElement;
            foreach(var category in new[]{ShopCategory.Weapons,ShopCategory.Ammunition,ShopCategory.Throwables,ShopCategory.BodyArmor,ShopCategory.Faintanyl})
            {
                typeof(GameFlowController).GetField("category",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,category);typeof(GameFlowController).GetField("selected",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,null);Draw();yield return Capture(category.ToString());
                Assert.AreEqual(5,root.Q("shopCategories").Query<Button>().ToList().Count);
                if(category==ShopCategory.Faintanyl){Assert.NotNull(root.Q("procureDuctTape"));Assert.NotNull(root.Q("procureFaintanyl"));Assert.IsTrue(root.Query<Label>().ToList().Any(l=>l.text=="relax its not addictive, just habit forming"));}
                else Assert.Greater(root.Query(className:"shop-entry").ToList().Count,0);
                foreach(float aspect in new[]{16f/9,2f,19.5f/9,20f/9,4f/3})
                {root.style.width=720*aspect;root.style.height=720;yield return null;yield return null;foreach(var b in root.Q("shopCategories").Query<Button>().ToList()){Assert.GreaterOrEqual(b.layout.height,48);Assert.LessOrEqual(b.worldBound.xMax,root.worldBound.xMax+1);}}
                root.style.width=StyleKeyword.Null;root.style.height=StyleKeyword.Null;
            }
            Assert.IsTrue(session.Buy("smg",10));Assert.AreEqual(10000-session.Catalog.Find("smg").price,session.Data.balance);session.Initialize(session.Catalog,path);Assert.AreEqual(10000,session.Data.balance);CollectionAssert.Contains(session.Data.owned,"smg");
        }
        [UnityTest]public IEnumerator MissionPauseEmptyRevivalAndTerminalPanelsRemainIntegrated()
        {
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");Assert.IsTrue(session.PrepareMission());
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            var mission=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>();
            foreach(var e in Object.FindObjectsByType<InsaneDreams.Enemies.EnemyCombat>(FindObjectsSortMode.None)){e.enabled=false;e.GetComponent<InsaneDreams.Enemies.LarperBrain>().enabled=false;}
            var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            yield return Capture("HUD");Assert.AreEqual(Color.clear,root.Q<Image>("revivePip0").tintColor);
            var bridge=mission.GetComponent<CampaignMissionBridge>();bridge.SetPaused(true);Assert.AreEqual(0,Time.timeScale);yield return Capture("Pause");bridge.SetPaused(false);Assert.AreEqual(1,Time.timeScale);
            // Controlled presentation probe; reward correctness is covered by flow/economy tests.
            typeof(InsaneDreams.Missions.WarehouseMission).GetProperty("Stage").SetValue(mission,InsaneDreams.Missions.MissionStage.Complete);
            Object.FindAnyObjectByType<InsaneDreams.Missions.MissionHud>().Bind(mission);yield return Capture("Victory");Assert.AreEqual(0,InsaneDreams.Enemies.LarperSpeechBubble.VisibleCount);
            typeof(InsaneDreams.Missions.WarehouseMission).GetProperty("AttemptFailed").SetValue(mission,true);
            Object.FindAnyObjectByType<InsaneDreams.Missions.MissionHud>().Bind(mission);yield return Capture("Failure");
            Assert.IsTrue(root.Q("missionResults").ClassListContains("is-failed"));
        }
    }
}
#endif
