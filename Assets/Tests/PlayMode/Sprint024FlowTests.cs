#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint024FlowTests
    {
        private GameSession session;private string dir;private float volume;private bool previousBackground;
        [UnitySetUp] public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);volume=AudioListener.volume;previousBackground=Application.runInBackground;Application.runInBackground=true;
            dir=Path.Combine(Path.GetTempPath(),"Akumu024-"+Guid.NewGuid().ToString("N"));
            session=new GameObject("TestCampaignSession").AddComponent<GameSession>();
            session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),Path.Combine(dir,"progress.json"));session.Data.balance=0;yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Object.Destroy(session.gameObject);yield return null;AudioListener.volume=volume;
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Warehouse_Demo"||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="MainMenu")
                yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            if(Directory.Exists(dir))Directory.Delete(dir,true);
            Application.runInBackground=previousBackground;
        }
        [UnityTest]public IEnumerator Hotfix00251PauseCentersInSafeTopAreaWithoutCoveringHudAndStillPauses()
        {
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");Assert.IsTrue(session.PrepareMission());
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));yield return null;yield return null;
            var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;var safe=root.Q("safeArea");var button=root.Q<Button>("campaignPause");
            foreach(var size in new[]{new Vector2(1280,720),new Vector2(1920,1080),new Vector2(2340,1080)})
            {
                root.style.width=size.x;root.style.height=size.y;safe.style.left=60;safe.style.right=30;safe.style.top=40;safe.style.bottom=20;
                yield return null;yield return null;
                Assert.AreEqual(safe.worldBound.center.x,button.worldBound.center.x,1);Assert.GreaterOrEqual(button.worldBound.yMin,safe.worldBound.yMin+10);Assert.Less(button.worldBound.yMin,safe.worldBound.yMin+25);
                Assert.GreaterOrEqual(button.worldBound.width,100);Assert.GreaterOrEqual(button.worldBound.height,52);Assert.IsFalse(button.worldBound.Overlaps(root.Q(className:"mission-card").worldBound));Assert.IsFalse(button.worldBound.Overlaps(root.Q(className:"equipment").worldBound));
            }
            using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}yield return null;Assert.AreEqual(0,Time.timeScale);
            Object.FindAnyObjectByType<CampaignMissionBridge>().SetPaused(false);Assert.AreEqual(1,Time.timeScale);
        }
        [UnityTest] public IEnumerator FaintanylRevivePersistsImmediatelyWithoutResettingMissionAndRetryStock()
        {
            session.Data.faintanyl=2; // Explicit pre-purchased stock, never a free retry refill.
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");Assert.IsTrue(session.PrepareMission());
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));yield return null;
            var mission=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>();var player=mission.Player;
            var revive=player.GetComponent<InsaneDreams.Combat.PlayerRevive>();var health=player.GetComponent<InsaneDreams.Combat.Health>();
            int recovered=0;player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Recovered+=()=>recovered++;
            var weapon=player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon;weapon.SetAmmunition(2,5);
            health.ProtectFor(0);health.ApplyDamage(new InsaneDreams.Combat.DamageInfo(1000,InsaneDreams.Combat.CombatTeam.Enemy));Assert.IsTrue(health.IsDefeated);Assert.AreEqual(1,mission.Deaths);
            yield return new WaitForSeconds(.1f);
            var bridge=mission.GetComponent<CampaignMissionBridge>();bridge.SetPaused(true);yield return null;
            var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;var pause=root.Q(className:"campaign-overlay");var death=root.Q("reviveOverlay");
            Assert.Greater(root.hierarchy.IndexOf(pause),root.hierarchy.IndexOf(death),"Pause must remain usable above death UI");
            bridge.SetPaused(false);Assert.IsFalse(player.enabled,"Unpausing must preserve death suspension");
            Assert.IsTrue(revive.TryRevive());Assert.AreEqual(0,recovered);Assert.AreEqual(2,weapon.MagazineAmmo);Assert.AreEqual(5,weapon.ReserveAmmo);
            Assert.AreEqual(1,session.Data.faintanyl);var saved=new ProgressStore(session.Store.Path).Load(()=>new GameProgress());Assert.AreEqual(0,saved.faintanyl,"Deployed stock is reserved on disk until settlement");
            mission.Replay();Assert.AreEqual(1,revive.Injections,"Retry cannot replenish consumed injections");Assert.AreEqual(1,session.Data.faintanyl);yield return null;
        }
        [Test] public void PreparationIsMandatoryPurchasesOptionalAndResultMoneySettlesOnlyOnce()
        {
            Assert.IsFalse(session.PrepareMission());Assert.IsFalse(session.SelectLevel("warehouse"));
            session.Navigate(FlowScreen.Levels);Assert.IsFalse(session.SelectLevel("fake"));Assert.IsTrue(session.SelectLevel("warehouse"));
            Assert.AreEqual(FlowScreen.Shop,session.Screen);Assert.AreEqual(0,session.Data.balance);
            Assert.IsTrue(session.PrepareMission());Assert.IsFalse(session.PrepareMission());
            Assert.IsTrue(session.Settle(37,true));Assert.IsFalse(session.Settle(37,true));
            Assert.AreEqual(session.Catalog.Payout(37,true),session.Data.balance);CollectionAssert.Contains(session.Data.completed,"warehouse");
            var stored=new ProgressStore(session.Store.Path).Load(()=>new GameProgress());Assert.AreEqual(session.Catalog.Payout(37,true),stored.balance);
        }
        [Test] public void PurchasesRequireFundsOwnershipAndDebounceAndEquipmentPersists()
        {
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");
            Assert.IsFalse(session.Buy("buzzbox",0));Assert.IsFalse(session.Equip("buzzbox"));Assert.IsFalse(session.Buy("buzzbox-ammo",0));
            session.Data.balance=500;Assert.IsTrue(session.Buy("buzzbox",1));Assert.AreEqual(500-session.Catalog.Find("buzzbox").price,session.Data.balance);
            Assert.IsFalse(session.Buy("buzzbox",2));Assert.IsTrue(session.Equip("buzzbox",true));
            Assert.IsTrue(session.Buy("buzzbox-ammo",3));Assert.IsFalse(session.Buy("buzzbox-ammo",3.01f));Assert.AreEqual(session.Catalog.Find("buzzbox-ammo").quantity,session.Data.Ammo("buzzbox").reserve);
            session.Data.tape=20;Assert.IsTrue(session.Buy("tape",4));Assert.AreEqual(70,session.Data.tape);
            Assert.IsTrue(session.Buy("armor",5));Assert.AreEqual(60,session.Data.armor);
            Assert.IsFalse(session.Buy("armor",6));
            var data=new ProgressStore(session.Store.Path).Load(()=>new GameProgress());Assert.AreEqual("buzzbox",data.secondary);Assert.AreEqual(session.Catalog.Find("buzzbox-ammo").quantity,data.Ammo("buzzbox").reserve);Assert.AreEqual(60,data.armor);
        }
        [UnityTest] public IEnumerator PurchasedEquipmentReachesMissionAndRetryDoesNotRefillFiniteStock()
        {
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.Data.balance=1000;
            Assert.IsTrue(session.Buy("carbine",0));Assert.IsTrue(session.Equip("carbine"));Assert.IsTrue(session.Buy("armor",1));session.Data.tape=42;session.Data.Ammo("carbine").magazine=3;session.Data.Ammo("carbine").reserve=6;
            Assert.IsTrue(session.PrepareMission());
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));yield return null;
            var mission=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>();mission.Player.GetComponent<InsaneDreams.Combat.Health>().ProtectFor(60);
            var weapon=mission.Player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon;
            Assert.AreEqual(3,weapon.MagazineAmmo);Assert.AreEqual(6,weapon.ReserveAmmo);
            Assert.AreEqual(42,mission.Player.GetComponent<InsaneDreams.Combat.PlayerHealing>().DuctTape);
            Assert.AreEqual(60,mission.Player.GetComponent<InsaneDreams.Combat.BodyArmor>().Condition);
            Assert.IsTrue(mission.Player.GetComponent<InsaneDreams.Inventory.PlayerSupplies>().CollectWallet("controlled-wallet",20));
            weapon.SetAmmunition(1,2);mission.Replay();yield return null;
            Assert.AreEqual(1000-session.Catalog.Find("carbine").price-session.Catalog.Find("armor").price+session.Catalog.Payout(20,false),session.Data.balance);Assert.AreEqual(1,mission.Player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon.MagazineAmmo);
            Assert.AreEqual(2,mission.Player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon.ReserveAmmo);
        }
        [Test] public void ArmorAbsorbsBodyDamageConsumesConditionAndDoesNotProtectHeadOrNeutralHazards()
        {
            var go=new GameObject("ArmorTest");var box=go.AddComponent<BoxCollider2D>();var armor=go.AddComponent<InsaneDreams.Combat.BodyArmor>();var hp=go.AddComponent<InsaneDreams.Combat.Health>();
            try
            {
                armor.Restore(60);Assert.IsTrue(hp.ApplyDamage(new InsaneDreams.Combat.DamageInfo(20,InsaneDreams.Combat.CombatTeam.Enemy)));
                Assert.AreEqual(93,hp.Current,.01);Assert.AreEqual(47,armor.Condition,.01);
                var impact=new InsaneDreams.Combat.PhysicalImpact(new Vector2(0,box.bounds.max.y),Vector2.right,1);
                hp.ApplyDamage(new InsaneDreams.Combat.DamageInfo(10,InsaneDreams.Combat.CombatTeam.Enemy,null,impact));Assert.AreEqual(83,hp.Current,.01);Assert.AreEqual(47,armor.Condition,.01);
                armor.Restore(2);hp.ApplyDamage(new InsaneDreams.Combat.DamageInfo(10,InsaneDreams.Combat.CombatTeam.Enemy));Assert.AreEqual(75,hp.Current,.01);Assert.AreEqual(0,armor.Condition);
                armor.Restore(60);hp.ApplyDamage(new InsaneDreams.Combat.DamageInfo(10,InsaneDreams.Combat.CombatTeam.Neutral));Assert.AreEqual(65,hp.Current,.01);Assert.AreEqual(60,armor.Condition);
            }
            finally{Object.DestroyImmediate(go);}
        }
        [UnityTest] public IEnumerator InterruptedRunRestoresSavedStockAndSettlesPendingEarningsOnlyOnce()
        {
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.PrepareMission();
            session.Data.pendingEarnings=19;session.Data.tape=81;session.Save();var catalog=session.Catalog;string path=session.Store.Path;
            Object.Destroy(session.gameObject);yield return null;
            session=new GameObject("ReloadedCampaign").AddComponent<GameSession>();session.Initialize(catalog,path,false);
            Assert.AreEqual(session.Catalog.Payout(19,false),session.Data.balance);Assert.AreEqual(81,session.Data.tape);Assert.IsNull(session.Data.activeRun);Assert.IsEmpty(session.Data.completed);
            var reload=new ProgressStore(path).Load(()=>new GameProgress());Assert.AreEqual(session.Catalog.Payout(19,false),reload.balance);Assert.AreEqual(0,reload.pendingEarnings);
        }
        private IEnumerator Press(string text,bool keyboard=false,bool touch=false)
        {
            var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            var button=root.Query<Button>().ToList().Find(b=>b.text==text);Assert.IsNotNull(button,text);
            float deadline=Time.realtimeSinceStartup+3;
            while((float.IsNaN(button.worldBound.width)||float.IsNaN(button.worldBound.height)||button.worldBound.width<=0||button.worldBound.height<=0)&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.IsTrue(button.enabledInHierarchy,text+" disabled");
            Assert.Greater(button.worldBound.width,1,text+" width "+button.worldBound);
            Assert.Greater(button.worldBound.height,1,text+" height "+button.worldBound);
            var scroll=root.Q<ScrollView>();
            if(scroll!=null&&scroll.Contains(button)){scroll.ScrollTo(button);yield return null;yield return null;}
            button.Focus();yield return null;
            if(keyboard){using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}}
            else if(touch)
            {
                using(var down=PointerDownEvent.GetPooled(new Touch{fingerId=2,phase=TouchPhase.Began,position=button.worldBound.center},EventModifiers.None)){down.target=button;button.SendEvent(down);}
                yield return null;
                using(var up=PointerUpEvent.GetPooled(new Touch{fingerId=2,phase=TouchPhase.Ended,position=button.worldBound.center},EventModifiers.None)){up.target=button;button.SendEvent(up);}
            }
            else
            {
                using(var down=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=button.worldBound.center})){down.target=button;button.SendEvent(down);}
                yield return null;
                using(var up=PointerUpEvent.GetPooled(new Event{type=EventType.MouseUp,button=0,mousePosition=button.worldBound.center})){up.target=button;button.SendEvent(up);}
            }
            yield return new WaitForSecondsRealtime(.15f);
        }
        private IEnumerator Scene(string name)
        {
            float end=Time.realtimeSinceStartup+8;
            while(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=name&&Time.realtimeSinceStartup<end)yield return null;
            Assert.AreEqual(name,UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);yield return null;yield return null;
        }
        [UnityTest] public IEnumerator RealMenuShopMissionResultsAndReturnFlowBanksOneReward()
        {
            session.Data.balance=500;
            string entry=Array.Find(EditorBuildSettings.scenes,s=>s.enabled).path;Assert.AreEqual("Assets/Scenes/MainMenu.unity",entry);
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(entry,new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));yield return null;yield return null;
            Assert.AreEqual(FlowScreen.MainMenu,session.Screen);Assert.AreEqual("MainMenu",UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            Assert.IsNotNull(Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>());
            yield return Press("PLAY",true);Assert.AreEqual(FlowScreen.Levels,session.Screen);yield return Press("PREPARE EQUIPMENT");Assert.AreEqual(FlowScreen.Shop,session.Screen);
            yield return Press("Body armor");yield return Press("BUY",false,true);Assert.AreEqual(500-session.Catalog.Find("armor").price,session.Data.balance);Assert.AreEqual(60,session.Data.armor);
            yield return Press("START MISSION — PURCHASE OPTIONAL");yield return Scene("Warehouse_Demo");
            var mission=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>();mission.Player.GetComponent<InsaneDreams.Combat.Health>().ProtectFor(60);
            Assert.AreEqual(60,mission.Player.GetComponent<InsaneDreams.Combat.BodyArmor>().Condition);
            var bridge=mission.GetComponent<CampaignMissionBridge>();bridge.SetPaused(true);Assert.AreEqual(0,Time.timeScale);Assert.IsFalse(mission.Player.enabled);bridge.SetPaused(false);Assert.AreEqual(1,Time.timeScale);Assert.IsTrue(mission.Player.enabled);
            var zones=Object.FindObjectsByType<InsaneDreams.Missions.MissionTrigger>(FindObjectsSortMode.None);
            bool Enter(InsaneDreams.Missions.MissionZone kind)
            {
                var zone=Array.Find(zones,z=>z.Zone==kind);var body=mission.Player.GetComponent<Rigidbody2D>();body.position=zone.Region.bounds.center;body.linearVelocity=Vector2.zero;mission.Player.transform.position=body.position;Physics2D.SyncTransforms();return mission.EnterZone(kind,zone.Region);
            }
            void Defeat(int group){foreach(var point in (mission.FirstOperation&&group==4?mission.Encounters[group].Points.Take(1):mission.Encounters[group].Points))point.Spawned.GetComponent<InsaneDreams.Combat.Health>().ApplyDamage(new InsaneDreams.Combat.DamageInfo(1000,InsaneDreams.Combat.CombatTeam.Player,mission.Player.gameObject));}
            Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.Entrance));
            if(mission.FirstOperation)
            {
                Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.Storage));Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.SamuraiArena));
                Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.FireworksArena));Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.Command));
                Assert.IsFalse(Enter(InsaneDreams.Missions.MissionZone.Exit));Defeat(4);
                Assert.AreEqual(0,mission.CheckpointCommits);Assert.AreEqual(1,mission.TotalDefeats,"Retreating ordinary enemies cannot lock the exit");
            }
            else
            {
                Defeat(0);Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.Storage));Defeat(1);
                Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.Checkpoint));Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.Processing));Defeat(2);
                Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.Records));Assert.IsTrue(mission.TryRecoverRecords());
            }
            Assert.IsTrue(mission.Player.GetComponent<InsaneDreams.Inventory.PlayerSupplies>().CollectWallet("result-test",37));
            var playerBody=mission.Player.GetComponent<Rigidbody2D>();playerBody.position=new Vector2(mission.FirstOperation?152:22,.85f);playerBody.linearVelocity=Vector2.zero;mission.Player.transform.position=playerBody.position;Physics2D.SyncTransforms();yield return new WaitForSeconds(.15f);
            var hitReaction=mission.Player.GetComponent<InsaneDreams.Combat.HitReaction2D>();var hitSettings=new SerializedObject(hitReaction);hitSettings.FindProperty("knockdownChance").floatValue=1;hitSettings.ApplyModifiedPropertiesWithoutUndo();
            var playerHealth=mission.Player.GetComponent<InsaneDreams.Combat.Health>();playerHealth.ProtectFor(0);playerHealth.ApplyDamage(new InsaneDreams.Combat.DamageInfo(1,InsaneDreams.Combat.CombatTeam.Enemy,null,new InsaneDreams.Combat.PhysicalImpact(playerBody.position,Vector2.right,4)));
            Assert.IsFalse(hitReaction.KnockedDown,"Ordinary gunfire must preserve player mobility");
            // Real nearby dropped wallets can also be collected while crossing the scene.
            Assert.GreaterOrEqual(mission.Earnings,37);int expectedBalance=session.Data.balance+session.Catalog.Payout(mission.Earnings,true);
            Assert.IsTrue(Enter(InsaneDreams.Missions.MissionZone.Exit));Assert.AreEqual(expectedBalance,session.Data.balance);
            Assert.IsFalse(Enter(InsaneDreams.Missions.MissionZone.Exit));Assert.AreEqual(expectedBalance,session.Data.balance);
            yield return new WaitForSeconds(.75f);Assert.IsFalse(mission.Player.enabled);Assert.IsFalse(hitReaction.KnockedDown);
            StringAssert.Contains("$"+mission.Earnings,Object.FindAnyObjectByType<UIDocument>().rootVisualElement.Q<Label>("resultStats").text);
            yield return Press("LEVEL SELECTION");yield return Scene("MainMenu");Assert.AreEqual(FlowScreen.Levels,session.Screen);CollectionAssert.Contains(session.Data.completed,"warehouse");
        }
        [UnityTest] public IEnumerator AlternateGapStepCanBeReachedUsingExistingDoubleJump()
        {
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.PrepareMission();
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));yield return null;
            var mission=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>();var p=mission.Player;var body=p.GetComponent<Rigidbody2D>();p.GetComponent<InsaneDreams.Combat.Health>().ProtectFor(60);
            body.position=new Vector2(mission.FirstOperation?52.8f:7.8f,mission.FirstOperation?.75f:1.75f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return new WaitForSeconds(.15f);
            p.SetTouchMove(Vector2.right);p.Jump();yield return new WaitForSeconds(.18f);p.Jump();
            for(int i=0;i<30&&body.position.x<(mission.FirstOperation?55.5f:10.2f);i++)yield return new WaitForFixedUpdate();
            p.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(1.2f);
            Assert.That(body.position.x,Is.InRange(mission.FirstOperation?54.5f:9.4f,mission.FirstOperation?56.8f:11f));Assert.IsTrue(p.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().IsGrounded);
            Assert.AreEqual(mission.FirstOperation?1.6f:2.175f,p.GetComponent<BoxCollider2D>().bounds.min.y,.12f);
        }
        [Test] public void FailedSaveRollsBackPurchaseWithoutChargingOrGrantingAnItem()
        {
            Directory.CreateDirectory(dir);session.Initialize(session.Catalog,dir);session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.Data.balance=500;
            Assert.IsFalse(session.Buy("buzzbox",1));Assert.AreEqual(500,session.Data.balance);CollectionAssert.DoesNotContain(session.Data.owned,"buzzbox");
            if(File.Exists(dir+".tmp"))File.Delete(dir+".tmp");
        }
        [UnityTest] public IEnumerator PhysicalExitFullLoopRepeatsWithPurchasedEquipmentAndSavedEarnings()
        {yield return PhysicalExitLoop(true);}
        [UnityTest] public IEnumerator PhysicalExitFullLoopRepeatsWithoutAnyPurchases()
        {yield return PhysicalExitLoop(false);}

        private IEnumerator CrossExit(InsaneDreams.Missions.WarehouseMission mission,bool complete)
        {
            var player=mission.Player;var body=player.GetComponent<Rigidbody2D>();
            var exitRegion=Object.FindObjectsByType<InsaneDreams.Missions.MissionTrigger>().Single(z=>z.Zone==InsaneDreams.Missions.MissionZone.Exit).Region;
            player.SetTouchMove(Vector2.zero);body.position=new Vector2(exitRegion.bounds.min.x-.55f,.71f);body.linearVelocity=Vector2.zero;player.transform.position=body.position;Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            player.SetTouchMove(Vector2.right);
            float deadline=Time.realtimeSinceStartup+5;
            while(Time.realtimeSinceStartup<deadline&&mission.Stage!=InsaneDreams.Missions.MissionStage.Complete&&body.position.x<exitRegion.bounds.center.x-.1f)yield return new WaitForFixedUpdate();
            player.SetTouchMove(Vector2.zero);
            if(complete)Assert.AreEqual(InsaneDreams.Missions.MissionStage.Complete,mission.Stage,"Real player movement must activate the exit through physics callbacks.");
            else
            {
                Assert.Greater(body.position.x,exitRegion.bounds.min.x-.75f,"The real player must reach the locked trigger.");
                Assert.AreEqual(0,mission.Completions);StringAssert.Contains("EXIT LOCKED",mission.Notification);
                // Feedback remains visible beyond the transient notification timeout.
                yield return new WaitForSeconds(4.1f);StringAssert.Contains("EXIT LOCKED",mission.Notification);
                StringAssert.Contains("EXIT LOCKED",Object.FindAnyObjectByType<UIDocument>().rootVisualElement.Q<Label>("missionNotice").text);
            }
        }
        private IEnumerator PhysicalExitLoop(bool purchase)
        {
            session.Data.balance=purchase?1000:0;
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));yield return null;yield return null;
            yield return Press("PLAY",true);
            for(int run=0;run<2;run++)
            {
                Assert.AreEqual(FlowScreen.Levels,session.Screen);yield return Press("PREPARE EQUIPMENT");Assert.AreEqual(FlowScreen.Shop,session.Screen);
                if(run==0&&purchase)
                {
                    yield return Press(session.Catalog.Find("buzzbox").title+"  $"+session.Catalog.Find("buzzbox").price);yield return Press("BUY",false,true);yield return Press("EQUIP / SECONDARY WEAPON");
                    yield return Press("Ammunition");yield return Press(session.Catalog.Find("buzzbox-ammo").title+"  $"+session.Catalog.Find("buzzbox-ammo").price);yield return Press("BUY",false,true);
                    yield return Press("Body armor");yield return Press("BUY",false,true);Assert.AreEqual(1000-session.Catalog.Find("buzzbox").price-session.Catalog.Find("buzzbox-ammo").price-session.Catalog.Find("armor").price,session.Data.balance);
                }
                int balanceBefore=session.Data.balance;
                yield return Press("START MISSION — PURCHASE OPTIONAL");yield return Scene("Warehouse_Demo");
                var mission=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>();var player=mission.Player;
                player.GetComponent<InsaneDreams.Combat.Health>().ProtectFor(120);
                Assert.AreEqual(purchase?60:0,player.GetComponent<InsaneDreams.Combat.BodyArmor>().Condition);
                string expectedId=purchase?"buzzbox":session.Catalog.primaryId;var expectedDefinition=session.Catalog.Find(expectedId).weapon;
                var expectedSlot=expectedDefinition.Fits(InsaneDreams.Inventory.EquipmentSlot.Firearm1)?InsaneDreams.Inventory.EquipmentSlot.Firearm1:InsaneDreams.Inventory.EquipmentSlot.Firearm2;
                Assert.AreEqual(expectedId,expectedSlot==InsaneDreams.Inventory.EquipmentSlot.Firearm1?session.Data.primary:session.Data.secondary);
                var equipped=player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().Get(expectedSlot);Assert.AreSame(expectedDefinition,equipped.Definition);
                if(purchase)Assert.AreEqual(session.Catalog.Find("buzzbox-ammo").quantity,equipped.ReserveAmmo);
                var active=player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon;var activeOffer=session.Catalog.offers.Single(o=>o.category==ShopCategory.Weapons&&o.weapon==active.Definition);
                Assert.AreEqual(session.Data.Ammo(activeOffer.id).reserve,active.ReserveAmmo);
                var zones=Object.FindObjectsByType<InsaneDreams.Missions.MissionTrigger>(FindObjectsSortMode.None);
                var exit=Array.Find(zones,z=>z.Zone==InsaneDreams.Missions.MissionZone.Exit);Assert.IsTrue(exit.Region.enabled);Assert.IsTrue(exit.Region.isTrigger);Assert.AreSame(mission,exit.Mission);
                // An actual non-player rigidbody crossing must not activate the checkpoint.
                var enemy=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(exit.Region.bounds.center.x,.85f),Quaternion.identity);
                enemy.GetComponent<InsaneDreams.Enemies.LarperBrain>().enabled=false;enemy.GetComponent<InsaneDreams.Enemies.EnemyCombat>().enabled=false;
                enemy.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
                yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.AreEqual(0,mission.Completions);Assert.AreEqual(InsaneDreams.Missions.MissionStage.EnterWarehouse,mission.Stage);Object.Destroy(enemy);yield return null;
                if(run==0)yield return CrossExit(mission,false);
                // Control objectives/combat to keep this a flow/physics test, not an AI playthrough.
                void EnterForSetup(InsaneDreams.Missions.MissionZone kind)
                {
                    var zone=Array.Find(zones,z=>z.Zone==kind);var body=player.GetComponent<Rigidbody2D>();player.SetTouchMove(Vector2.zero);body.position=zone.Region.bounds.center;body.linearVelocity=Vector2.zero;player.transform.position=body.position;Physics2D.SyncTransforms();Assert.IsTrue(mission.EnterZone(kind,zone.Region));
                }
                void DefeatForSetup(int group){foreach(var point in (mission.FirstOperation&&group==4?mission.Encounters[group].Points.Take(1):mission.Encounters[group].Points))point.Spawned.GetComponent<InsaneDreams.Combat.Health>().ApplyDamage(new InsaneDreams.Combat.DamageInfo(1000,InsaneDreams.Combat.CombatTeam.Player,player.gameObject));}
                EnterForSetup(InsaneDreams.Missions.MissionZone.Entrance);DefeatForSetup(0);EnterForSetup(InsaneDreams.Missions.MissionZone.Storage);DefeatForSetup(1);
                if(mission.FirstOperation)
                {EnterForSetup(InsaneDreams.Missions.MissionZone.SamuraiArena);EnterForSetup(InsaneDreams.Missions.MissionZone.FireworksArena);EnterForSetup(InsaneDreams.Missions.MissionZone.Command);DefeatForSetup(4);Assert.AreEqual(InsaneDreams.Missions.MissionStage.Escape,mission.Stage);}
                else
                {EnterForSetup(InsaneDreams.Missions.MissionZone.Checkpoint);EnterForSetup(InsaneDreams.Missions.MissionZone.Processing);DefeatForSetup(2);EnterForSetup(InsaneDreams.Missions.MissionZone.Records);yield return Press("INTERACT");Assert.IsTrue(mission.HasRecords);Assert.AreEqual(InsaneDreams.Missions.MissionStage.Escape,mission.Stage);}
                Assert.IsTrue(player.GetComponent<InsaneDreams.Inventory.PlayerSupplies>().CollectWallet("physical-loop-"+run,37+run));
                yield return CrossExit(mission,true);
                Assert.AreEqual(1,mission.Completions);Assert.IsFalse(player.enabled);Assert.IsFalse(player.GetComponent<InsaneDreams.Combat.PlayerCombat>().enabled);
                var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;Assert.IsFalse(root.Q("missionResults").ClassListContains("hidden"));
                Assert.GreaterOrEqual(mission.Earnings,37+run);StringAssert.Contains("MISSION EARNINGS   $"+mission.Earnings,root.Q<Label>("resultStats").text);
                int expected=balanceBefore+session.Catalog.Payout(mission.Earnings,true);Assert.AreEqual(expected,session.Data.balance);
                for(int i=0;i<5;i++)yield return new WaitForFixedUpdate();Assert.AreEqual(1,mission.Completions);Assert.AreEqual(expected,session.Data.balance);
                var stored=new ProgressStore(session.Store.Path).Load(()=>new GameProgress());Assert.AreEqual(expected,stored.balance);Assert.AreEqual(purchase?60:0,stored.armor);CollectionAssert.Contains(stored.completed,"warehouse");
                CollectionAssert.Contains(stored.owned,"pistol");Assert.AreEqual(session.Data.primary,stored.primary);
                if(purchase){CollectionAssert.Contains(stored.owned,"buzzbox");Assert.AreEqual(session.Catalog.Find("buzzbox-ammo").quantity,stored.Ammo("buzzbox").reserve);}
                yield return Press("LEVEL SELECTION");yield return Scene("MainMenu");Assert.AreEqual(FlowScreen.Levels,session.Screen);Assert.AreEqual(expected,session.Data.balance);
            }
        }
    }
}
#endif
