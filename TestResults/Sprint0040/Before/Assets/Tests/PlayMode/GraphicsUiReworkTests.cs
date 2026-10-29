#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
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
 public sealed class GraphicsUiReworkTests
 {
  GameSession session;string save;bool background;float volume;
  [UnitySetUp]public IEnumerator Setup()
  {
   Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;
   save=Path.Combine(Path.GetTempPath(),"AkumuUi-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("UiReviewSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);session.Data.balance=2000;yield return null;
  }
  [UnityTearDown]public IEnumerator Cleanup()
  {
   Time.timeScale=1;if(SceneManager.GetActiveScene().name=="MainMenu"||SceneManager.GetActiveScene().name=="Warehouse_Demo")yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));
   if(session!=null)Object.Destroy(session.gameObject);yield return null;foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);AudioListener.volume=volume;Application.runInBackground=background;
  }
  VisualElement Root=>Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
  void Click(Button b){Assert.NotNull(b);Assert.IsTrue(b.enabledInHierarchy);using(var e=NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}}
  Button Button(string text)=>Root.Query<Button>().ToList().FirstOrDefault(b=>b.text==text);
  void Tap(VisualElement element)
  {
   var touch=new Touch{fingerId=5,position=element.worldBound.center,phase=TouchPhase.Began,type=TouchType.Direct,tapCount=1,pressure=1,maximumPossiblePressure=1};using(var down=PointerDownEvent.GetPooled(touch,EventModifiers.None))element.SendEvent(down);touch.phase=TouchPhase.Ended;using(var up=PointerUpEvent.GetPooled(touch,EventModifiers.None))element.SendEvent(up);
  }
  IEnumerator Load(string name){yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/"+name+".unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.3f);}
  IEnumerator Capture(string name)
  {
   string path=Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults",name+".png");yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(path);yield return new WaitForEndOfFrame();yield return null;Assert.IsTrue(File.Exists(path),path);
  }
  [UnityTest]public IEnumerator MenuSettingsLevelShopPurchaseAndLaunchKeepTheirRealCallbacks()
  {
   yield return Load("MainMenu");Assert.NotNull(Root.Q(className:"flow-logo"));Assert.NotNull(Root.Q(className:"warehouse-scene"));Assert.IsTrue(Button("PLAY").enabledInHierarchy);yield return Capture("GraphicsUiRework_MainMenu");
   Click(Button("SETTINGS"));yield return null;Assert.AreEqual(FlowScreen.Settings,session.Screen);var sliders=Root.Query<Slider>(className:"flow-slider").ToList();Assert.AreEqual(2,sliders.Count);sliders[0].value=.37f;sliders[1].value=1.2f;Assert.AreEqual(.37f,session.Data.volume,.001f);Assert.AreEqual(1.2f,session.Data.joystickScale,.001f);Assert.IsTrue(File.Exists(save));
   Click(Button("BACK"));Click(Button("PLAY"));yield return null;Assert.AreEqual(FlowScreen.Levels,session.Screen);Click(Button("PREPARE EQUIPMENT"));yield return null;Assert.AreEqual(FlowScreen.Shop,session.Screen);
   foreach(string tab in new[]{"Weapons","Ammunition","Body armor","Duct tape"}){Click(Button(tab));yield return null;Assert.NotNull(Button("BUY"));}
   session.Data.tape=0;Click(Button("Duct tape"));float cash=session.Data.balance;Click(Button("BUY"));Assert.Greater(session.Data.tape,0);Assert.Less(session.Data.balance,cash);yield return null;yield return Capture("GraphicsUiRework_Shop");
   Click(Button("START MISSION — PURCHASE OPTIONAL"));float end=Time.realtimeSinceStartup+8;while(SceneManager.GetActiveScene().name!="Warehouse_Demo"&&Time.realtimeSinceStartup<end)yield return null;yield return new WaitForSeconds(.2f);Assert.NotNull(Object.FindAnyObjectByType<WarehouseMission>());
   var bridge=Object.FindAnyObjectByType<CampaignMissionBridge>();bridge.SetPaused(true);Assert.AreEqual(0,Time.timeScale);Click(Button("RESUME"));Assert.AreEqual(1,Time.timeScale);
  }
  [UnityTest]public IEnumerator StructuredHudBindsRealHealthAmmoAndKeepsSecondaryJoystickFire()
  {
   session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));
   var sidearm=session.Catalog.offers.First(o=>o.category==ShopCategory.Weapons&&o.weapon==AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset"));
   if(!session.Data.owned.Contains(sidearm.id))Assert.IsTrue(session.Buy(sidearm.id,Time.unscaledTime));Assert.IsTrue(session.Equip(sidearm.id,true));
   Assert.IsTrue(session.PrepareMission());yield return Load("Warehouse_Demo");
   var p=Object.FindAnyObjectByType<WarehouseMission>().Player;var s=new SerializedObject(p);s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;}
   var loadout=p.GetComponent<PlayerLoadout>();loadout.SelectedWeapon.SetAmmunition(7,123);p.GetComponent<BodyArmor>().Restore(0,100,.5f);var hp=p.GetComponent<Health>();hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(25,CombatTeam.Enemy));hp.ProtectFor(120);yield return new WaitForSeconds(.3f);
   Assert.IsNull(Root.Q("fireButton"));Assert.IsNull(Root.Q("fireSecondary"));Assert.AreEqual($"7/{loadout.SelectedWeapon.Definition.MagazineCapacity}",Root.Q<Label>("ammoValue").text);Assert.AreEqual("RESERVE 123",Root.Q<Label>("reserveValue").text);Assert.AreEqual($"{hp.Current:0} / {hp.Maximum:0}",Root.Q<Label>("healthValue").text);Assert.AreEqual(hp.Current/hp.Maximum*100,Root.Q("healthFill").style.width.value.value,.01f);Assert.NotNull(Root.Q<Image>("weaponIcon").sprite);
   yield return Capture("GraphicsUiRework_Hud");Tap(Root.Q<Button>("slot2"));yield return new WaitForSeconds(.25f);Assert.AreEqual(EquipmentSlot.Firearm2,loadout.SelectedSlot);Assert.IsTrue(Root.Q("slot2").ClassListContains("is-selected"));
   loadout.SelectedWeapon.SetAmmunition(loadout.SelectedWeapon.Definition.MagazineCapacity-2,50);yield return new WaitForSeconds(.2f);Assert.AreEqual($"{loadout.SelectedWeapon.MagazineAmmo}/{loadout.SelectedWeapon.Definition.MagazineCapacity}",Root.Q<Label>("ammoValue").text);int ammo=loadout.SelectedWeapon.MagazineAmmo;var pad=Root.Q("aimStick");Vector2 point=pad.worldBound.center+Vector2.right*pad.worldBound.width*.45f;
   using(var down=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=point})){down.target=pad;pad.SendEvent(down);}yield return new WaitForSeconds(.12f);Assert.IsTrue(p.FireHeld);Assert.Less(loadout.SelectedWeapon.MagazineAmmo,ammo);
   using(var up=PointerUpEvent.GetPooled(new Event{type=EventType.MouseUp,button=0,mousePosition=point})){up.target=pad;pad.SendEvent(up);}yield return null;Assert.IsFalse(p.FireHeld);
   yield return new WaitForSeconds(.2f);var secondary=loadout.SelectedWeapon;Assert.AreEqual($"{secondary.MagazineAmmo}/{secondary.Definition.MagazineCapacity}",Root.Q<Label>("ammoValue").text);int reserve=secondary.ReserveAmmo;int missing=secondary.Definition.MagazineCapacity-secondary.MagazineAmmo;
   p.RequestReload();Assert.IsTrue(secondary.Cycle.IsReloading);yield return new WaitForSeconds(secondary.Definition.ReloadDuration+.25f);Assert.AreEqual($"{secondary.Definition.MagazineCapacity}/{secondary.Definition.MagazineCapacity}",Root.Q<Label>("ammoValue").text);Assert.AreEqual($"RESERVE {reserve-missing}",Root.Q<Label>("reserveValue").text);
   Tap(Root.Q<Button>("melee"));yield return new WaitForSeconds(.2f);Assert.AreEqual("--",Root.Q<Label>("ammoValue").text);Tap(Root.Q<Button>("slot1"));yield return new WaitForSeconds(.2f);Assert.AreEqual($"7/{loadout.SelectedWeapon.Definition.MagazineCapacity}",Root.Q<Label>("ammoValue").text);Assert.AreEqual("RESERVE 123",Root.Q<Label>("reserveValue").text);
   var safe=Root.Q("safeArea");foreach(var size in new[]{new Vector2(960,720),new Vector2(1280,720),new Vector2(2340,1080)})
   {
    Root.style.width=size.x;Root.style.height=size.y;safe.style.left=40;safe.style.right=30;safe.style.top=20;safe.style.bottom=20;yield return null;yield return null;
    foreach(var name in new[]{"moveStick","aimStick","reload","heal","jumpButton","slot1","slot2"})Assert.IsTrue(safe.worldBound.Contains(Root.Q(name).worldBound.center),name);
    Assert.AreEqual(44,safe.worldBound.xMax-Root.Q("aimStick").worldBound.xMax,1);Assert.IsFalse(Root.Q(className:"vitals-panel").worldBound.Overlaps(Root.Q(className:"equipment").worldBound));
   }
  }
 }
}
#endif
