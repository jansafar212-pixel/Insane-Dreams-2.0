#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
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
namespace InsaneDreams.Tests {
 public sealed class PostPlaytestHotfixTests {
  GameSession session;string save;bool background;float volume;UnityEngine.Random.State random;
  readonly List<GameObject> owned=new List<GameObject>();
  GameObject Keep(GameObject g){owned.Add(g);return g;}
  [UnitySetUp] public IEnumerator Setup(){Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;random=UnityEngine.Random.state;UnityEngine.Random.InitState(264);save=Path.Combine(Path.GetTempPath(),"AkumuPost-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("PostPlaytestSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);yield return null;}
  [UnityTearDown] public IEnumerator Cleanup(){var bridge=Object.FindAnyObjectByType<CampaignMissionBridge>();if(bridge!=null)bridge.SetPaused(false);Time.timeScale=1;foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();if(SceneManager.GetActiveScene().name!="Warehouse_Testbed")yield return Load("Warehouse_Testbed");if(session!=null)Object.Destroy(session.gameObject);yield return null;foreach(string suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);AudioListener.volume=volume;UnityEngine.Random.state=random;Application.runInBackground=background;}
  IEnumerator Load(string scene){yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/"+scene+".unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.2f);}
  VisualElement Root=>Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
  void Click(Button b){Assert.NotNull(b);using(var e=NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}}
  void Tap(VisualElement element){var touch=new Touch{fingerId=5,position=element.worldBound.center,phase=TouchPhase.Began,type=TouchType.Direct,tapCount=1,pressure=1,maximumPossiblePressure=1};using(var down=PointerDownEvent.GetPooled(touch,EventModifiers.None))element.SendEvent(down);touch.phase=TouchPhase.Ended;using(var up=PointerUpEvent.GetPooled(touch,EventModifiers.None))element.SendEvent(up);}
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults",name+".png"));yield return new WaitForEndOfFrame();yield return null;}
  [UnityTest] public IEnumerator CompactWeaponPanelKeepsSwitchingAndPauseBounds(){
   yield return Load("Warehouse_Demo");foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;}
   var panel=Root.Q(className:"equipment");string stage=Root.Q("pauseIcon")==null?"Before":"After";
   File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/PostPlaytest_"+stage+".json"),$"{{\"equipment\":\"{panel.worldBound}\",\"health\":\"{Root.Q(className:"vitals-panel").worldBound}\",\"objective\":\"{Root.Q(className:"mission-card").worldBound}\",\"pause\":\"{Root.Q("pauseVisual").worldBound}\",\"pauseTouch\":\"{Root.Q("campaignPause").worldBound}\"}}");yield return Capture("PostPlaytest_Hud"+stage);
   var player=Object.FindAnyObjectByType<WarehouseMission>().Player;var loadout=player.GetComponent<PlayerLoadout>();Tap(Root.Q<Button>("melee"));yield return null;Assert.AreEqual(EquipmentSlot.Melee,loadout.SelectedSlot);Tap(Root.Q<Button>("slot1"));yield return null;Assert.AreEqual(EquipmentSlot.Firearm1,loadout.SelectedSlot);
   Assert.AreEqual(2,Root.Q("pauseIcon").childCount);Assert.IsEmpty(Root.Q<Button>("campaignPause").text);Assert.AreEqual(52,Root.Q("pauseVisual").worldBound.width,1);Assert.AreEqual(40,Root.Q("pauseVisual").worldBound.height,1);Assert.AreEqual(220,Root.Q(className:"vitals-panel").worldBound.width,1);
   var safe=Root.Q("safeArea");foreach(var size in new[]{new Vector2(960,720),new Vector2(1280,720),new Vector2(2340,1080)}){Root.style.width=size.x;Root.style.height=size.y;safe.style.left=40;safe.style.right=30;safe.style.top=20;safe.style.bottom=20;yield return null;yield return null;Assert.IsFalse(panel.worldBound.Overlaps(Root.Q("aimStick").worldBound));Assert.IsFalse(panel.worldBound.Overlaps(Root.Q("campaignPause").worldBound));Assert.Less(panel.worldBound.width*panel.worldBound.height,220*220*.72f);foreach(var name in new[]{"slot1","slot2","melee"}){Assert.GreaterOrEqual(Root.Q(name).worldBound.width,44);Assert.GreaterOrEqual(Root.Q(name).worldBound.height,43);}}
   Click(Root.Q<Button>("campaignPause"));Assert.AreEqual(0,Time.timeScale);Object.FindAnyObjectByType<CampaignMissionBridge>().SetPaused(false);
  }
  [UnityTest] public IEnumerator FaintanylShopUsesExistingAtomicCurrencyStockAndPersistence(){
   session.Data.faintanyl=0;session.Data.balance=1000;session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));yield return Load("MainMenu");
   foreach(string category in new[]{"Weapons","Ammunition","Body armor","Throwables"}){Click(Root.Query<Button>().ToList().Single(b=>b.text==category));yield return null;Assert.IsNull(Root.Q("procureFaintanyl"),category);Assert.IsNull(Root.Q("faintanylShop"),category);}
   Click(Root.Query<Button>().ToList().Single(b=>b.text=="Medical"));yield return null;
   var offer=session.Catalog.offers.Single(o=>o.category==ShopCategory.Faintanyl);Assert.AreEqual("relax its not addictive, just habit forming",offer.description);Assert.IsTrue(Root.Query<Label>().ToList().Any(l=>l.text==offer.description));var button=Root.Q<Button>("procureFaintanyl");Assert.AreEqual("Procure Faintanyl",button.text);int balance=session.Data.balance;Click(button);Assert.AreEqual(1,session.Data.faintanyl);Assert.AreEqual(balance-offer.price,session.Data.balance);Click(Root.Q<Button>("procureFaintanyl"));Assert.AreEqual(1,session.Data.faintanyl,"Immediate duplicate transaction rejected");
   yield return new WaitForSecondsRealtime(.3f);Click(Root.Q<Button>("procureFaintanyl"));Assert.AreEqual(2,session.Data.faintanyl);Assert.IsFalse(Root.Q<Button>("procureFaintanyl").enabledSelf);Assert.IsFalse(session.Buy(offer.id,Time.unscaledTime+1));var stored=new ProgressStore(save).Load(()=>new GameProgress());Assert.AreEqual(2,stored.faintanyl);Assert.AreEqual(balance-offer.price*2,stored.balance);Assert.IsTrue(Root.Query<Button>().ToList().Any(b=>b.text=="Weapons"));Assert.IsFalse(Root.Query<Button>().ToList().Any(b=>b.text=="Duct tape"));Root.Q<ScrollView>().ScrollTo(Root.Q("faintanylShop"));yield return null;yield return Capture("PostPlaytest_Shop");
   session.Data.faintanyl=0;session.Data.balance=offer.price-1;Assert.IsFalse(session.CanBuy(offer.id));Assert.IsFalse(session.Buy(offer.id,Time.unscaledTime+2));Assert.AreEqual(0,session.Data.faintanyl);
  }
  GameObject Player(){var floor=Keep(new GameObject("PlayerRagdollFloor"));floor.transform.position=new Vector3(9200,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(30,1);var p=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(9200,.71f),Quaternion.identity));var s=new SerializedObject(p.GetComponent<PlayerActions>());s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();return p;}
  [UnityTest] public IEnumerator DeathCopiesRealRigImpactAndRevivePlaysOneSyringeThenRestoresCombat(){
   var p=Player();yield return new WaitForSeconds(.1f);var hp=p.GetComponent<Health>();var revive=p.GetComponent<PlayerRevive>();var death=p.GetComponent<PlayerDeathRagdoll>();var visual=p.GetComponent<CharacterVisualDriver>();var syringe=p.GetComponent<PlayerSyringeVisual>();float ordinarySpeed=0;
   for(int i=0;i<2;i++){
    var direction=i==0?Vector2.right:Vector2.left;hp.ProtectFor(0);Assert.IsTrue(hp.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy,null,new PhysicalImpact(p.transform.position,direction,i==0?.15f:3))));Assert.IsNotNull(death.Corpse);Assert.IsFalse(visual.Rig.gameObject.activeSelf);Assert.IsFalse(p.GetComponent<Rigidbody2D>().simulated);Assert.IsFalse(p.GetComponent<PlayerActions>().enabled);Assert.AreEqual(1,death.Corpse.ImpactCount);Assert.AreEqual(6,death.Corpse.Bodies.Length);
    yield return new WaitForFixedUpdate();float speed=death.Corpse.Bodies.Sum(b=>Mathf.Abs(b.linearVelocity.x));if(i==0)ordinarySpeed=speed;else Assert.Greater(speed,ordinarySpeed*1.3f,"Stronger final impulse gives stronger explosive-style death");Assert.Greater(death.Corpse.Bodies.Sum(b=>b.linearVelocity.x)*direction.x,0);
    foreach(var body in death.Corpse.Bodies){Assert.IsTrue(body.simulated);Assert.IsNotNull(body.GetComponent<Collider2D>());Assert.AreEqual(-2,body.GetComponent<Collider2D>().excludeLayers.value);Assert.IsTrue(float.IsFinite(body.position.x));}
    Assert.IsTrue(revive.TryRevive());Assert.IsFalse(revive.TryRevive());Assert.IsNull(death.Corpse);Assert.IsTrue(visual.Rig.gameObject.activeSelf);Assert.AreEqual(1-i,revive.Injections);Assert.AreEqual(i+1,syringe.PlayCount);yield return null;Assert.IsTrue(syringe.IsPlaying);Assert.IsTrue(syringe.Syringe.enabled);Assert.Greater(syringe.Syringe.sortingOrder,visual.Rig.torso.GetComponent<SpriteRenderer>().sortingOrder);Assert.Less(Vector2.Distance(syringe.Syringe.transform.position,visual.Rig.leftArm.TransformPoint(Vector3.down*visual.Rig.skin.arm.bounds.size.y)),.01f);
    var weapon=p.GetComponent<PlayerLoadout>().SelectedWeapon;int rounds=weapon.MagazineAmmo;p.GetComponent<PlayerCombat>().Reload();Assert.IsFalse(weapon.Cycle.IsReloading);p.GetComponent<PlayerActions>().SetFireIntent(true);yield return new WaitForSeconds(.2f);Assert.AreEqual(rounds,weapon.MagazineAmmo,"No conflicting firing during syringe use");p.GetComponent<PlayerActions>().SetFireIntent(false);yield return new WaitForSeconds(.45f);Assert.IsFalse(syringe.IsPlaying);Assert.IsFalse(syringe.Syringe.enabled);Assert.AreEqual(rounds,weapon.MagazineAmmo);p.GetComponent<PlayerActions>().SetFireIntent(true);yield return new WaitForSeconds(.12f);Assert.Less(weapon.MagazineAmmo,rounds,"Shooting resumes when the gesture finishes");p.GetComponent<PlayerActions>().SetFireIntent(false);Assert.IsTrue(p.GetComponent<Rigidbody2D>().simulated);Assert.IsTrue(p.GetComponent<PlayerActions>().enabled);Assert.IsTrue(p.GetComponent<PlayerCombat>().enabled);Assert.IsFalse(hp.ApplyDamage(new DamageInfo(10,CombatTeam.Enemy)));
   }
   hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));Assert.IsTrue(hp.IsDefeated);Assert.IsNotNull(death.Corpse);yield return new WaitForSeconds(1.4f);Assert.IsFalse(hp.IsDefeated);Assert.IsNull(death.Corpse);Assert.IsTrue(visual.Rig.gameObject.activeSelf);Assert.IsTrue(p.GetComponent<Rigidbody2D>().simulated);Assert.IsTrue(p.GetComponent<PlayerActions>().enabled);Assert.AreEqual(3,death.SpawnCount);Assert.AreEqual(2,syringe.PlayCount);
  }
  [UnityTest] public IEnumerator CheckpointCancelsCorpseAndAnimationWithoutConsumingInjection(){var p=Player();yield return null;var hp=p.GetComponent<Health>();var revive=p.GetComponent<PlayerRevive>();hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));Assert.IsTrue(revive.ReturnToCheckpoint());Assert.AreEqual(2,revive.Injections);Assert.IsNull(p.GetComponent<PlayerDeathRagdoll>().Corpse);Assert.IsTrue(p.GetComponent<CharacterVisualDriver>().Rig.gameObject.activeSelf);hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));Assert.IsTrue(revive.TryRevive());p.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Respawn();yield return null;Assert.IsFalse(p.GetComponent<PlayerSyringeVisual>().IsPlaying);Assert.IsFalse(p.GetComponent<PlayerSyringeVisual>().Syringe.enabled);}
  [UnityTest] public IEnumerator NativeWarehouseOrdinaryAndActualExplosiveDeathsAndBothFacingSyringeCaptures(){
   yield return Load("Warehouse_Demo");foreach(var enemy in Object.FindObjectsByType<EnemyCombat>()){enemy.enabled=false;enemy.GetComponent<LarperBrain>().enabled=false;}
   var p=Object.FindAnyObjectByType<WarehouseMission>().Player;var settings=new SerializedObject(p);settings.FindProperty("readKeyboard").boolValue=false;settings.ApplyModifiedPropertiesWithoutUndo();var hp=p.GetComponent<Health>();var revive=p.GetComponent<PlayerRevive>();var death=p.GetComponent<PlayerDeathRagdoll>();
   for(int i=0;i<2;i++){
    p.SetTouchMove(i==0?Vector2.right:Vector2.left);yield return new WaitForSeconds(.1f);p.SetTouchMove(Vector2.zero);yield return null;hp.ProtectFor(0);
    if(i==0)hp.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy,null,new PhysicalImpact(p.transform.position,Vector2.right,.15f)));
    else{hp.ApplyDamage(new DamageInfo(80,CombatTeam.Enemy));var fx=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab"))).GetComponent<FireworkProjectile>();Physics2D.SyncTransforms();Vector2 origin=(Vector2)p.transform.position+Vector2.right*.8f;fx.Prepare(null,origin,Color.yellow,true);fx.ExplodeHeld(origin);Assert.AreEqual(1,fx.LastAffected);}
    Assert.IsTrue(hp.IsDefeated);Assert.IsNotNull(death.Corpse);yield return new WaitForSeconds(.25f);foreach(var body in death.Corpse.Bodies){Assert.Greater(body.position.y,-.8f);Assert.LessOrEqual(body.linearVelocity.magnitude,12.1f);}yield return Capture("PostPlaytest_Death"+i);
    Assert.IsTrue(revive.TryRevive());yield return new WaitForSeconds(.22f);var syringe=p.GetComponent<PlayerSyringeVisual>();Assert.IsTrue(syringe.IsPlaying);Assert.AreEqual(i==0?1:-1,Mathf.Sign(syringe.Syringe.transform.lossyScale.x));yield return Capture("PostPlaytest_Syringe"+i);yield return new WaitForSeconds(.7f);Assert.IsFalse(syringe.IsPlaying);
   }
  }
  [UnityTest,Explicit("Diagnostic comparison; does not count as a passing gameplay regression.")]
  public IEnumerator LegacyFailureDiagnosticsWithNewPlayerPresentationDisabled(){
   var ladderFixture=new TraversalRegressionTests();yield return ladderFixture.Setup();
   var baselinePlayer=(GameObject)typeof(TraversalRegressionTests).GetField("player",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(ladderFixture);
   baselinePlayer.GetComponent<PlayerDeathRagdoll>().enabled=false;baselinePlayer.GetComponent<PlayerSyringeVisual>().enabled=false;
   var check=ladderFixture.LadderTopAndDiagonalArbitrationDoNotGenerateExtraJump();string ladderFailure=null;
   while(true){object step;try{if(!check.MoveNext())break;step=check.Current;}catch(AssertionException e){ladderFailure=e.Message;break;}yield return step;}
   yield return ladderFixture.Cleanup();
   var enemyFixture=new ReactiveRagdollTests();yield return enemyFixture.Setup();check=enemyFixture.FatalRayTransfersHitDirectionAndRepeatedShotsReuseOneInactiveEnemyBody();string enemyFailure=null;
   while(true){object step;try{if(!check.MoveNext())break;step=check.Current;}catch(AssertionException e){enemyFailure=e.Message;break;}yield return step;}
   var corpse=Object.FindObjectsByType<CartoonRagdoll>().FirstOrDefault(d=>d.name=="Larper_CartoonRagdoll");
   string diagnostics=$"Ladder with both new player components disabled: {ladderFailure}\nEnemy-only fixture in this controlled random sequence: {enemyFailure??"Completed without an assertion failure"}\n";
   if(corpse!=null)diagnostics+=$"Legacy root bodies={corpse.parts.Length}, max vx={corpse.parts.Max(b=>b.linearVelocity.x)}; actual simulation bodies={corpse.Bodies.Length}, max vx={corpse.Bodies.Max(b=>b.linearVelocity.x)}\n";
   File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/PostPlaytest_LegacyDiagnostics.txt"),diagnostics);yield return enemyFixture.Cleanup();Assert.IsNotNull(ladderFailure);
  }
 }
}
#endif
