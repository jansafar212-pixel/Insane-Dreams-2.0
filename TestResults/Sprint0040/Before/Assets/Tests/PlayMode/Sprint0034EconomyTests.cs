#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using InsaneDreams.Progression;
using InsaneDreams.Inventory;
using InsaneDreams.Combat;
using InsaneDreams.Missions;
using InsaneDreams.Enemies;
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
    public sealed class Sprint0034EconomyTests
    {
        GameSession session;string dir,path;bool background;float volume;WarehouseMission mission;
        [UnitySetUp]public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;
            dir=Path.Combine(Path.GetTempPath(),"AkumuEconomy-"+Guid.NewGuid().ToString("N"));path=Path.Combine(dir,"progress.json");
            session=new GameObject("EconomyTestSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),path);
            session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            Time.timeScale=1;if(SceneManager.GetActiveScene().name!="Warehouse_Testbed")yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));
            if(session!=null)Object.Destroy(session.gameObject);yield return null;AudioListener.volume=volume;Application.runInBackground=background;
            if(Directory.Exists(dir))Directory.Delete(dir,true);
        }
        IEnumerator LoadMission()
        {
            Assert.IsTrue(session.PrepareMission());yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            mission=Object.FindAnyObjectByType<WarehouseMission>();mission.Player.GetComponent<Health>().ProtectFor(120);
            foreach(var p in mission.Encounters.SelectMany(e=>e.Points)){p.Spawned.enabled=false;p.Spawned.GetComponent<EnemyCombat>().enabled=false;p.Spawned.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}
        }
        GameProgress Disk()=>new ProgressStore(path).Load(()=>throw new Exception("Expected persisted progress"));
        [Test]public void AllExistingArsenalCanBePurchasedEquippedAndReloadedFromDiskWithoutDuplicateCharges()
        {
            session.Data.balance=Mathf.Max(10000,session.Catalog.offers.Where(o=>o.category==ShopCategory.Weapons).Sum(o=>o.price)+1000);float now=1;
            foreach(var gun in session.Catalog.offers.Where(o=>o.category==ShopCategory.Weapons&&o.weapon.WeaponCategory==WeaponCategory.Firearm))
            {
                if(!session.Data.owned.Contains(gun.id)){int before=session.Data.balance;Assert.IsTrue(session.Buy(gun.id,now++),gun.id);Assert.AreEqual(before-gun.price,session.Data.balance);}
                int balance=session.Data.balance;Assert.IsFalse(session.Buy(gun.id,now++));Assert.AreEqual(balance,session.Data.balance);var slot=gun.weapon.Fits(EquipmentSlot.Firearm1)?EquipmentSlot.Firearm1:EquipmentSlot.Firearm2;Assert.IsTrue(session.Equip(gun.id,slot));Assert.AreEqual(gun.id,slot==EquipmentSlot.Firearm1?Disk().primary:Disk().secondary);
            }
            Assert.IsTrue(session.Equip("pistol",true));var stored=Disk();Assert.AreEqual(session.Catalog.offers.Count(o=>o.category==ShopCategory.Weapons&&o.weapon!=null&&o.weapon.WeaponCategory==WeaponCategory.Firearm),stored.owned.Count(id=>session.Catalog.Find(id)?.weapon?.WeaponCategory==WeaponCategory.Firearm));Assert.AreEqual("pistol",stored.secondary);
        }
        [Test]public void ExistingVersionOneProfileRetainsBankedCashOwnedCarbineAmmoAndDamagedArmor()
        {
            var legacy=new GameProgress{balance=1000,primary="carbine",armor=37,bodyArmorTier=1};legacy.owned.Add("carbine");legacy.Ammo("carbine").magazine=4;legacy.Ammo("carbine").reserve=108;Assert.IsTrue(new ProgressStore(path).Save(legacy));
            session.Initialize(session.Catalog,path,false);Assert.AreEqual(1000,session.Data.balance);CollectionAssert.Contains(session.Data.owned,"carbine");CollectionAssert.Contains(session.Data.owned,"pistol");Assert.AreEqual("carbine",session.Data.primary);Assert.AreEqual(108,session.Data.Ammo("carbine").reserve);Assert.AreEqual(4,session.Data.Ammo("carbine").magazine);Assert.AreEqual("armor",session.Data.bodyArmorId);Assert.AreEqual(37,session.Data.protection.Single(x=>x.id=="armor").condition);Assert.IsTrue(session.Save());Assert.AreEqual(1000,Disk().balance);
        }
        [Test]public void ExactAmmoPackFitAndOwnershipAreRequiredWithoutOvercharging()
        {
            session.Data.balance=10000;Assert.IsFalse(session.Equip("sniper"));Assert.IsFalse(session.Buy("sniper-ammo",1));Assert.IsTrue(session.Buy("sniper",2));
            var gun=session.Catalog.Find("sniper");var pack=session.Catalog.Find("sniper-ammo");var stock=session.Data.Ammo(gun.id);int cap=session.Catalog.ReserveCapacity(session.Data,gun);
            stock.reserve=cap-pack.quantity+1;int balance=session.Data.balance;Assert.IsFalse(session.Buy(pack.id,3));Assert.AreEqual(balance,session.Data.balance);Assert.AreEqual(cap-pack.quantity+1,stock.reserve);
            stock.reserve=cap-pack.quantity;Assert.IsTrue(session.Buy(pack.id,4));Assert.AreEqual(cap,stock.reserve);Assert.AreEqual(balance-pack.price,session.Data.balance);Assert.IsFalse(session.Buy(pack.id,5));Assert.IsFalse(session.Buy("pistol-ammo",float.NaN));
        }
        [Test]public void ZeroMoneyStillProvidesFiveBasicMagazinesWithoutFreePremiumRefills()
        {
            session.Data.balance=2000;Assert.IsTrue(session.Buy("sniper",1));session.Equip("sniper");session.Data.balance=0;session.Data.Ammo("pistol").magazine=0;session.Data.Ammo("pistol").reserve=0;session.Data.Ammo("sniper").magazine=0;session.Data.Ammo("sniper").reserve=0;
            Assert.IsTrue(session.PrepareMission());var basic=session.Data.Ammo("pistol");Assert.AreEqual(75,basic.magazine+basic.reserve);Assert.AreEqual("sniper",session.Data.primary);Assert.AreEqual("pistol",session.Data.secondary);Assert.AreEqual(0,session.Data.Ammo("sniper").magazine+session.Data.Ammo("sniper").reserve);Assert.AreEqual(0,session.Data.balance);
        }
        [Test]public void FailedAtomicPurchasesDeploymentAndSettlementRollBackMemoryAndDisk()
        {
            session.Data.balance=5000;Assert.IsTrue(session.Save());string original=File.ReadAllText(path);Directory.CreateDirectory(path+".tmp");
            Assert.IsFalse(session.Buy("smg",1));Assert.IsFalse(session.Data.owned.Contains("smg"));Assert.AreEqual(5000,session.Data.balance);Assert.AreEqual(original,File.ReadAllText(path));
            Assert.IsFalse(session.PrepareMission());Assert.IsTrue(string.IsNullOrEmpty(session.Data.activeRun));Assert.AreEqual(FlowScreen.Shop,session.Screen);
            Directory.Delete(path+".tmp");Assert.IsTrue(session.PrepareMission());Directory.CreateDirectory(path+".tmp");Assert.IsFalse(session.Settle(100,true));Assert.IsNotNull(session.Data.activeRun);Assert.AreEqual(5000,session.Data.balance);Assert.IsEmpty(session.Data.completed);
            Directory.Delete(path+".tmp");Assert.IsTrue(session.Settle(100,true));Assert.AreEqual(5350,session.Data.balance);Assert.IsFalse(session.Settle(100,true));
        }
        [Test]public void ArmorOwnershipSelectionAndDamagePersistWithoutFreeRepairs()
        {
            session.Data.balance=2000;Assert.IsTrue(session.Buy("armor",1));session.Data.armor=12;Assert.IsTrue(session.Buy("heavy-armor",2));Assert.AreEqual(90,session.Data.armor);Assert.IsTrue(session.EquipArmor("armor"));Assert.AreEqual(12,session.Data.armor);
            Assert.IsTrue(session.EquipArmor(null));Assert.AreEqual(0,session.Data.bodyArmorTier);Assert.AreEqual(0,session.Data.armor);Assert.IsTrue(session.EquipArmor("heavy-armor"));Assert.AreEqual(90,session.Data.armor);Assert.IsFalse(session.Buy("heavy-armor",3));
            Assert.IsTrue(session.Buy("heavy-helmet",4));var stored=Disk();Assert.AreEqual("heavy-armor",stored.bodyArmorId);Assert.AreEqual("heavy-helmet",stored.helmetId);Assert.AreEqual(12,stored.protection.Single(x=>x.id=="armor").condition);Assert.LessOrEqual(session.Catalog.ReloadMultiplier(stored),1.3f);
        }
        [Test]public void ConsumableCapsAndFailurePayoutNeverCreateMoneyOrFreeInjections()
        {
            session.Data.balance=2000;session.Data.faintanyl=1;Assert.IsTrue(session.Buy("faintanyl",1));Assert.AreEqual(2,session.Data.faintanyl);Assert.IsFalse(session.Buy("faintanyl",2));
            for(int i=0;i<3;i++){session.Data.grenades[i]=session.Catalog.GrenadeCapacity(session.Data);Assert.IsFalse(session.Buy(new[]{"frag-grenade","flash-grenade","smoke-grenade"}[i],3+i));}
            session.Data.tape=0;Assert.IsTrue(session.Buy("tape",10));Assert.AreEqual(50,session.Data.tape);int bank=session.Data.balance;Assert.IsTrue(session.PrepareMission());Assert.IsTrue(session.Settle(10000,false));Assert.AreEqual(bank+50,session.Data.balance);Assert.AreEqual(0,session.Data.lastReward);Assert.IsFalse(session.Settle(10000,false));
        }
        [UnityTest]public IEnumerator ExistingAmmoSupplyUsesEquippedWeaponCapacityAndLeavesOverflowInBox()
        {
            session.Data.balance=5000;Assert.IsTrue(session.Buy("lmg",1));Assert.IsTrue(session.Equip("lmg"));session.Data.Ammo("lmg").reserve=198;yield return LoadMission();
            var load=mission.Player.GetComponent<PlayerLoadout>();Assert.AreEqual(200,load.ReserveCapacityFor(load.SelectedWeapon.Definition));Assert.AreEqual(2,load.AddReserveAmmunition(72));Assert.AreEqual(200,load.SelectedWeapon.ReserveAmmo);Assert.AreEqual(0,load.AddReserveAmmunition(72));
            mission.Player.GetComponent<PlayerRevive>().RestoreInventory(0);mission.Player.GetComponent<Health>().ProtectFor(0);mission.Player.GetComponent<Health>().ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy));yield return null;
            Assert.AreEqual(200,Disk().Ammo("lmg").reserve);Assert.AreEqual(0,Disk().faintanyl);
        }
        [UnityTest]public IEnumerator BackgroundAndPauseSecureUnusedSuppliesThenResumeReservesBeforePlay()
        {
            session.Data.faintanyl=2;session.Data.grenades=new[]{3,2,1};yield return LoadMission();var bridge=mission.GetComponent<CampaignMissionBridge>();var load=mission.Player.GetComponent<PlayerLoadout>();load.SelectedWeapon.SetAmmunition(3,11);mission.Player.GetComponent<PlayerRevive>().RestoreInventory(1);
            bridge.SendMessage("OnApplicationPause",true);Assert.AreEqual(3,Disk().Ammo("pistol").magazine);Assert.AreEqual(11,Disk().Ammo("pistol").reserve);Assert.AreEqual(1,Disk().faintanyl);CollectionAssert.AreEqual(new[]{3,2,1},Disk().grenades);
            bridge.SendMessage("OnApplicationPause",false);Assert.AreEqual(0,Disk().Ammo("pistol").magazine);Assert.AreEqual(0,Disk().faintanyl);Assert.AreEqual(3,load.SelectedWeapon.MagazineAmmo);Assert.AreEqual(1,mission.Player.GetComponent<PlayerRevive>().Injections);
            bridge.SetPaused(true);Assert.AreEqual(1,Disk().faintanyl);Assert.AreEqual(0,Time.timeScale);Directory.CreateDirectory(path+".tmp");bridge.SetPaused(false);Assert.AreEqual(0,Time.timeScale);Assert.IsFalse(mission.Player.enabled);Directory.Delete(path+".tmp");bridge.SetPaused(false);Assert.AreEqual(1,Time.timeScale);Assert.AreEqual(0,Disk().faintanyl);
        }
        [UnityTest]public IEnumerator ThreeFirearmMissionPackingSwitchingAndPausePreserveEachMagazineAndStoredOverflow()
        {
            session.Data.balance=10000;
            Assert.IsTrue(session.Buy("sks",1));Assert.IsTrue(session.Buy("sawed-off",2));
            Assert.IsTrue(session.Buy("heavy-armor",3));Assert.IsTrue(session.Buy("heavy-helmet",4));
            string[] ids={"sks","pistol","sawed-off"};
            var slots=new[]{EquipmentSlot.Firearm1,EquipmentSlot.Firearm2,EquipmentSlot.Firearm3};
            int[] magazines={7,15,2},overflow={17,23,19},carried=new int[3],storedReserve=new int[3];
            for(int i=0;i<ids.Length;i++)
            {
                Assert.IsTrue(session.Equip(ids[i],slots[i]));
                carried[i]=session.Catalog.ReserveCapacity(session.Data,session.Catalog.Find(ids[i]));
                storedReserve[i]=carried[i]+overflow[i];
                session.Data.Ammo(ids[i]).magazine=magazines[i];session.Data.Ammo(ids[i]).reserve=storedReserve[i];
            }
            Assert.IsTrue(session.Save());yield return LoadMission();
            var load=mission.Player.GetComponent<PlayerLoadout>();var bridge=mission.GetComponent<CampaignMissionBridge>();
            for(int i=0;i<ids.Length;i++)
            {
                var weapon=load.Get(slots[i]);Assert.IsNotNull(weapon,ids[i]+" must survive native mission startup");
                Assert.AreSame(session.Catalog.Find(ids[i]).weapon,weapon.Definition);
                Assert.AreEqual(magazines[i],weapon.MagazineAmmo,ids[i]);Assert.AreEqual(carried[i],weapon.ReserveAmmo,ids[i]);
                Assert.AreEqual(0,Disk().Ammo(ids[i]).magazine);Assert.AreEqual(overflow[i],Disk().Ammo(ids[i]).reserve,ids[i]+" keeps only unpacked rounds on disk while deployed");
            }
            foreach(var slot in slots)
            {
                load.Select(slot);yield return new WaitForSeconds(.25f);Assert.AreSame(load.Get(slot),load.SelectedWeapon);
                var hud=Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Single(d=>d.rootVisualElement.Q<Button>("slot3")!=null).rootVisualElement;
                string name=slot==EquipmentSlot.Firearm1?"slot1":slot==EquipmentSlot.Firearm2?"slot2":"slot3";
                Assert.IsTrue(hud.Q<Button>(name).ClassListContains("is-selected"));Assert.GreaterOrEqual(hud.Q<Button>(name).worldBound.width,44);Assert.GreaterOrEqual(hud.Q<Button>(name).worldBound.height,44);
                Assert.AreEqual(load.SelectedWeapon.MagazineAmmo+"/"+load.SelectedWeapon.Definition.MagazineCapacity,hud.Q<Label>("ammoValue").text);
                for(int i=0;i<ids.Length;i++){Assert.AreEqual(magazines[i],load.Get(slots[i]).MagazineAmmo);Assert.AreEqual(carried[i],load.Get(slots[i]).ReserveAmmo);}
            }
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("TestResults/Sprint0039/ThreeSlotHud.png"));yield return new WaitForSeconds(.15f);
            // Distinct controlled expenditures make a stale third-slot save observable.
            for(int i=0;i<ids.Length;i++)load.Get(slots[i]).SetAmmunition(magazines[i]-1,carried[i]-2);
            bridge.SetPaused(true);Assert.AreEqual(0,Time.timeScale);var saved=Disk();
            Assert.AreEqual(ids[0],saved.primary);Assert.AreEqual(ids[1],saved.secondary);Assert.AreEqual(ids[2],saved.secondary2);
            for(int i=0;i<ids.Length;i++){Assert.AreEqual(magazines[i]-1,saved.Ammo(ids[i]).magazine,ids[i]);Assert.AreEqual(storedReserve[i]-2,saved.Ammo(ids[i]).reserve,ids[i]+" restores carried plus unpacked ammunition when paused");}
            bridge.SetPaused(false);Assert.AreEqual(1,Time.timeScale);saved=Disk();
            for(int i=0;i<ids.Length;i++)
            {
                Assert.AreEqual(0,saved.Ammo(ids[i]).magazine);Assert.AreEqual(overflow[i],saved.Ammo(ids[i]).reserve,ids[i]+" reserves carried ammunition again before resuming");
                Assert.AreEqual(magazines[i]-1,load.Get(slots[i]).MagazineAmmo);Assert.AreEqual(carried[i]-2,load.Get(slots[i]).ReserveAmmo);
            }
        }
        [UnityTest]public IEnumerator ForcedInterruptionCannotRestoreSpentInventoryOrDuplicateWalletPayouts()
        {
            session.Data.balance=5000;Assert.IsTrue(session.Buy("carbine",1));Assert.IsTrue(session.Equip("carbine"));session.Data.Ammo("carbine").magazine=7;session.Data.Ammo("carbine").reserve=194;session.Data.tape=81;session.Data.faintanyl=2;session.Data.grenades=new[]{3,2,1};int bank=session.Data.balance;
            yield return LoadMission();var disk=Disk();Assert.AreEqual(0,disk.Ammo("carbine").magazine);Assert.AreEqual(50,disk.Ammo("carbine").reserve);Assert.AreEqual(0,disk.tape);Assert.AreEqual(0,disk.faintanyl);Assert.AreEqual(0,disk.grenades.Sum());
            var supplies=mission.Player.GetComponent<PlayerSupplies>();Assert.IsTrue(supplies.CollectWallet("same-wallet",20));Assert.IsFalse(supplies.CollectWallet("same-wallet",20));Assert.AreEqual(20,Disk().pendingEarnings);
            mission.Player.GetComponent<PlayerRevive>().RestoreInventory(1);Assert.AreEqual(1,session.Data.faintanyl);Assert.AreEqual(0,Disk().faintanyl);
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Object.Destroy(session.gameObject);yield return null;
            session=new GameObject("ReloadedEconomy").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),path,false);Assert.AreEqual(bank+5,session.Data.balance);Assert.IsTrue(string.IsNullOrEmpty(session.Data.activeRun));Assert.AreEqual(50,session.Data.Ammo("carbine").reserve);Assert.AreEqual(0,session.Data.faintanyl);Assert.IsFalse(session.Settle(20,false));Assert.AreEqual(bank+5,Disk().balance);
        }
        [UnityTest]public IEnumerator CompletionReturnsUnusedStockPaysOnceAndAllowsReplayAndShopping()
        {
            session.Data.balance=2000;Assert.IsTrue(session.Buy("smg",1));Assert.IsTrue(session.Equip("smg"));session.Data.Ammo("smg").reserve=60;session.Data.faintanyl=2;yield return LoadMission();
            var weapon=mission.Player.GetComponent<PlayerLoadout>().SelectedWeapon;Assert.IsTrue(weapon.Cycle.TryAttack(Time.time));Assert.IsTrue(weapon.Cycle.TryReload(Time.time));weapon.Cycle.Tick(Time.time+weapon.Definition.ReloadDuration+.01f);int rounds=weapon.MagazineAmmo+weapon.ReserveAmmo;
            Assert.IsTrue(mission.Player.GetComponent<PlayerSupplies>().CollectWallet("finish-wallet",100));var revive=mission.Player.GetComponent<PlayerRevive>();revive.RestoreInventory(1);
            foreach(var p in mission.Encounters.SelectMany(e=>e.Points))p.Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(10000,CombatTeam.Player,mission.Player.gameObject));
            var exit=Object.FindObjectsByType<MissionTrigger>().Single(t=>t.Zone==MissionZone.Exit);var rb=mission.Player.GetComponent<Rigidbody2D>();rb.position=new Vector2(exit.Region.bounds.center.x,.75f);mission.Player.transform.position=rb.position;Physics2D.SyncTransforms();mission.EnterZone(MissionZone.Exit,exit.Region);yield return null;
            Assert.AreEqual(MissionStage.Complete,mission.Stage);Assert.AreEqual(2000-350+350,session.Data.balance);Assert.AreEqual(350,session.Data.lastPayout);Assert.AreEqual(1,Disk().faintanyl);Assert.AreEqual(rounds,Disk().Ammo("smg").magazine+Disk().Ammo("smg").reserve);Assert.IsFalse(session.Settle(100,true));
            mission.GetComponent<CampaignMissionBridge>().ReturnToLevels();while(SceneManager.GetActiveScene().name!="MainMenu")yield return null;yield return null;
            Assert.IsTrue(session.SelectLevel("warehouse"));Assert.IsTrue(session.Buy("smg-ammo",Time.unscaledTime+1));Assert.IsTrue(session.PrepareMission());Assert.AreEqual(1,session.Data.faintanyl);
        }
    }
}
#endif
