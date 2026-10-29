#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
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
    public sealed class Sprint0031Tests
    {
        GameSession session;string path;WarehouseMission mission;bool background;float volume;UnityEngine.Random.State random;
        [UnitySetUp]public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;random=UnityEngine.Random.state;UnityEngine.Random.InitState(2902);path=Path.Combine(Path.GetTempPath(),"Akumu29-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("Sprint29Session").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),path);session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.PrepareMission();yield return Load("Warehouse_Demo");Quiet();InitView();yield return Resize(1280,720);
        }
        IEnumerator Load(string name){yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/"+name+".unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;mission=Object.FindAnyObjectByType<WarehouseMission>();}
        void Quiet(){mission.Player.GetComponent<Health>().ProtectFor(120);foreach(var p in mission.Encounters.SelectMany(e=>e.Points)){p.Spawned.GetComponent<EnemyCombat>().enabled=false;p.Spawned.enabled=false;p.Spawned.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}}
        [UnityTearDown]public IEnumerator Cleanup(){RestoreView();Time.timeScale=1;yield return Load("Warehouse_Testbed");if(session!=null)Object.Destroy(session.gameObject);yield return null;foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);Application.runInBackground=background;AudioListener.volume=volume;UnityEngine.Random.state=random;}
        VisualElement Root=>Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
        static void Click(Button b){Assert.NotNull(b);Assert.IsTrue(b.enabledInHierarchy,b.text);using(var e=NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}}
        Button Named(string text)=>Root.Query<Button>().ToList().Single(b=>b.text==text);
        IEnumerator Capture(string name){int frame=Time.frameCount;yield return new WaitForSecondsRealtime(.2f);Assert.Greater(Time.frameCount,frame+1,"UI rendering must advance, including while gameplay is paused");yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("TestResults/Sprint0031/"+name+".png");yield return new WaitForEndOfFrame();yield return null;}
        static readonly System.Reflection.BindingFlags F=System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        EditorWindow gameView;object sizes,group;System.Reflection.PropertyInfo selectedSize;int originalSize;readonly System.Collections.Generic.List<int> addedSizes=new System.Collections.Generic.List<int>();
        void InitView()
        {
            var asm=typeof(Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var kind=type.GetProperty("currentGroupType",F).GetValue(sizes);group=type.GetMethod("GetGroup",F).Invoke(sizes,new[]{kind});gameView=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));selectedSize=gameView.GetType().GetProperty("selectedSizeIndex",F);originalSize=(int)selectedSize.GetValue(gameView);
        }
        IEnumerator Resize(int w,int h)
        {
            var gt=group.GetType();int before=(int)gt.GetMethod("GetBuiltinCount",F).Invoke(group,null)+(int)gt.GetMethod("GetCustomCount",F).Invoke(group,null);var asm=typeof(Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSize");var kind=Enum.Parse(asm.GetType("UnityEditor.GameViewSizeType"),"FixedResolution");var size=Activator.CreateInstance(type,F,null,new object[]{kind,w,h,"AKUMU0031 "+w+"x"+h},null);gt.GetMethod("AddCustomSize",F).Invoke(group,new[]{size});addedSizes.Add(before);selectedSize.SetValue(gameView,before);gameView.Repaint();gameView.Focus();float end=Time.realtimeSinceStartup+4;while((Screen.width!=w||Screen.height!=h)&&Time.realtimeSinceStartup<end)yield return null;Assert.AreEqual(w,Screen.width,"Actual GameView width, not resized screenshot");Assert.AreEqual(h,Screen.height);yield return new WaitForSecondsRealtime(.25f);
        }
        void RestoreView()
        {if(gameView==null)return;selectedSize.SetValue(gameView,originalSize);int built=(int)group.GetType().GetMethod("GetBuiltinCount",F).Invoke(group,null);for(int i=addedSizes.Count-1;i>=0;i--)group.GetType().GetMethod("RemoveCustomSize",F).Invoke(group,new object[]{addedSizes[i]});addedSizes.Clear();gameView.Repaint();}
        IEnumerator Menu(){mission.GetComponent<CampaignMissionBridge>().ReturnToLevels();while(SceneManager.GetActiveScene().name!="MainMenu")yield return null;yield return null;yield return null;Click(Named("MAIN MENU"));yield return null;}
        Button Offer(string id){var item=session.Catalog.Find(id);return Named(item.title+"  $"+item.price);}
        [UnityTest]public IEnumerator AllMenuShopSettingsScreensUseSharedSkinAndPreservePurchasesAndPersistence()
        {
            yield return Menu();Assert.Greater(Root.Q("akumuWordmark").childCount,60);Assert.AreEqual(0,Named("PLAY").resolvedStyle.borderTopLeftRadius);Assert.GreaterOrEqual(Named("PLAY").worldBound.height,60);yield return Capture("After_MainMenu");Named("PLAY").Focus();yield return null;yield return Capture("After_MenuFocus");Click(Named("SETTINGS"));yield return null;var sliders=Root.Query<Slider>(className:"flow-slider").ToList();Assert.AreEqual(2,sliders.Count);sliders[0].value=.37f;sliders[1].value=1.1f;Assert.AreEqual("37%",Root.Q<Label>("volumeReadout").text);Assert.AreEqual(1.1f,new ProgressStore(path).Load(()=>new GameProgress()).joystickScale,.001f);yield return Capture("After_Settings");sliders[1].value=1;Click(Named("BACK"));yield return null;Click(Named("PLAY"));yield return null;yield return Capture("After_LevelSelect");Click(Named("PREPARE EQUIPMENT"));yield return null;session.Data.balance=1000;session.Data.tape=150;session.Data.faintanyl=1;Click(Named("Weapons"));yield return null;Assert.AreEqual(4,Root.Q("shopCategories").childCount);var item=session.Catalog.offers.First(o=>o.category==ShopCategory.Weapons&&o.weapon!=null&&o.weapon.WeaponCategory==WeaponCategory.Firearm&&!session.Data.owned.Contains(o.id));Click(Offer(item.id));yield return null;Assert.IsTrue(Offer(item.id).ClassListContains("is-selected"));int wallet=session.Data.balance;Click(Named("BUY"));yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(wallet-item.price,session.Data.balance);Assert.IsTrue(session.Data.owned.Contains(item.id));yield return Capture("After_ShopWeapons");Click(Named("Body armor"));yield return null;yield return Capture("After_ShopArmor");Click(Named("Ammunition"));yield return null;Click(Offer("frag-grenade"));yield return null;Click(Named("BUY"));yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(1,session.Data.grenades[0]);yield return Capture("After_ShopAmmo");Click(Named("Medical"));yield return null;Assert.AreEqual(2,Root.Q("medicalShop").childCount);Click(Root.Q<Button>("procureFaintanyl"));yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(2,session.Data.faintanyl);Assert.IsFalse(Root.Q<Button>("procureFaintanyl").enabledInHierarchy);Assert.IsTrue(Root.Query<Label>().ToList().Any(l=>l.text=="STOCK FULL / ALREADY EQUIPPED"));yield return Capture("After_ShopMedical");Assert.LessOrEqual(Root.Q("procureFaintanyl").worldBound.yMax,Root.worldBound.yMax,"Medical purchase remains visible in landscape");session.Data.balance=0;Click(Named("Medical"));yield return null;Assert.IsTrue(Root.Query<Label>().ToList().Any(l=>l.text.StartsWith("NEED $")));yield return Capture("After_ShopNoMoney");Click(Named("BACK TO LEVELS"));yield return null;Click(Named("PREPARE EQUIPMENT"));yield return null;Assert.AreEqual(2,new ProgressStore(path).Load(()=>new GameProgress()).faintanyl);
        }
        [UnityTest]public IEnumerator HudSlotsGrenadesAmmoPauseDeathAndVictoryStayFunctionalAndVisuallyConsistent()
        {
            session.Data.faintanyl=2;session.Data.grenades=new[]{3,2,1};mission.GetComponent<CampaignMissionBridge>().ApplyEquipment();var revive=mission.Player.GetComponent<PlayerRevive>();var controls=Object.FindAnyObjectByType<InsaneDreams.Input.WarehouseTouchControls>();controls.Bind(mission.Player);controls.Bind(mission.Player);Assert.AreEqual(2,Root.Q("revivePips").childCount);revive.RestoreInventory(1);Assert.IsFalse(Root.Q("revivePip0").ClassListContains("is-empty"));Assert.IsTrue(Root.Q("revivePip1").ClassListContains("is-empty"));Assert.AreEqual(DisplayStyle.Flex,Root.Q("revivePip1").resolvedStyle.display);revive.RestoreInventory(2);yield return Capture("After_HUD");var load=mission.Player.GetComponent<PlayerLoadout>();var gun=load.SelectedWeapon;gun.SetAmmunition(2,20);mission.Player.SetFireIntent(true);yield return new WaitForSeconds(.13f);mission.Player.SetFireIntent(false);yield return new WaitForSeconds(.25f);Assert.AreEqual(gun.MagazineAmmo+"/"+gun.Definition.MagazineCapacity,Root.Q<Label>("ammoValue").text);mission.Player.GetComponent<PlayerCombat>().Reload();yield return new WaitForSeconds(gun.Definition.ReloadDuration+.25f);Assert.AreEqual(gun.MagazineAmmo+"/"+gun.Definition.MagazineCapacity,Root.Q<Label>("ammoValue").text);load.Select(EquipmentSlot.Melee);yield return new WaitForSeconds(.25f);Click(Root.Q<Button>("throwable1"));yield return new WaitForSeconds(.25f);Assert.IsTrue(Root.Q("throwable1").ClassListContains("is-selected"));Assert.AreEqual("2/5",Root.Q<Label>("ammoValue").text);mission.Player.SetTouchAim(new Vector2(1,.4f));yield return new WaitForSeconds(.2f);yield return Capture("After_GrenadeAim");mission.Player.SetTouchAim(Vector2.zero);yield return new WaitForSeconds(.25f);Assert.AreEqual("1/5",Root.Q<Label>("ammoValue").text);var bridge=mission.GetComponent<CampaignMissionBridge>();bridge.SetPaused(true);yield return null;Assert.AreEqual(0,Time.timeScale);yield return Capture("After_Pause");bridge.SetPaused(false);Assert.AreEqual(1,Time.timeScale);revive.RestoreInventory(0);var hp=mission.Player.GetComponent<Health>();hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy));yield return new WaitForSeconds(.3f);Assert.IsTrue(mission.AttemptFailed);yield return Capture("After_Death");mission.Replay();yield return new WaitForSeconds(.25f);Quiet();foreach(var p in mission.Encounters.SelectMany(e=>e.Points))p.Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(10000,CombatTeam.Player,mission.Player.gameObject));var exit=Object.FindObjectsByType<MissionTrigger>().Single(t=>t.Zone==MissionZone.Exit);var rb=mission.Player.GetComponent<Rigidbody2D>();rb.position=new Vector2(exit.Region.bounds.center.x,.75f);mission.Player.transform.position=rb.position;Physics2D.SyncTransforms();mission.EnterZone(MissionZone.Exit,exit.Region);yield return null;Assert.AreEqual(MissionStage.Complete,mission.Stage);yield return Capture("After_Victory");
        }
        [UnityTest]public IEnumerator ActualAspectRatiosAndSimulatedCutoutsPreserveControlBoundsAndCompactPanels()
        {
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1560,720),new Vector2Int(1600,720),new Vector2Int(1920,720),new Vector2Int(1920,1080)})
            {
                yield return Resize(size.x,size.y);var safe=Root.Q("safeArea");safe.style.left=60;safe.style.right=30;safe.style.top=24;safe.style.bottom=16;yield return null;yield return null;var move=Root.Q("moveStick").worldBound;var aim=Root.Q("aimStick").worldBound;Assert.GreaterOrEqual(move.width,140);Assert.AreEqual(move.width,aim.width,.1f);Assert.IsFalse(move.Overlaps(aim));Assert.AreEqual(safe.worldBound.center.x,Root.Q("campaignPause").worldBound.center.x,1);foreach(string id in new[]{"moveStick","aimStick","reload","heal","jumpButton","parachuteButton","campaignPause"}){var r=Root.Q(id).worldBound;Assert.GreaterOrEqual(r.xMin,safe.worldBound.xMin-.5f,id);Assert.LessOrEqual(r.xMax,safe.worldBound.xMax+.5f,id);Assert.GreaterOrEqual(r.yMin,safe.worldBound.yMin-.5f,id);Assert.LessOrEqual(r.yMax,safe.worldBound.yMax+.5f,id);Assert.GreaterOrEqual(r.height,44,id);}Assert.LessOrEqual(Root.Q(className:"vitals-panel").worldBound.width,240);Assert.LessOrEqual(Root.Q(className:"equipment").worldBound.width,210);Assert.IsFalse(Root.Q("reload").worldBound.Overlaps(aim));Assert.IsFalse(Root.Q("heal").worldBound.Overlaps(move));yield return Capture("Aspect_"+size.x+"x"+size.y);
            }
        }
    }
}
#endif
