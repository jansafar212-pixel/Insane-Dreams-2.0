#if UNITY_EDITOR
using System;
using System.IO;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0035Tests
    {
        string dir,path;GameSession session;GameCatalog catalog;
        [SetUp]public void Setup()
        {
            Assert.IsNull(GameSession.Instance);
            dir=Path.Combine(Path.GetTempPath(),"Akumu35-"+Guid.NewGuid().ToString("N"));path=Path.Combine(dir,"save.json");
            catalog=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"));
            catalog.developmentPlaytestCredits=true;session=new GameObject("Sprint35Session").AddComponent<GameSession>();
        }
        [TearDown]public void Cleanup()
        {Object.DestroyImmediate(session.gameObject);Object.DestroyImmediate(catalog);if(Directory.Exists(dir))Directory.Delete(dir,true);}
        GameProgress Disk()=>new ProgressStore(path).Load(()=>new GameProgress());
        [Test]public void ExistingLowSavePurchaseEquipsThenRestartRestoresPlaytestFloor()
        {
            Assert.IsTrue(new ProgressStore(path).Save(new GameProgress{balance=23,primary="pistol"}));
            session.Initialize(catalog,path);Assert.AreEqual(10000,session.Data.balance);Assert.AreEqual(35,Disk().developmentCreditRevision);
            session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));
            Assert.IsTrue(session.Buy("smg",1));int expected=10000-catalog.Find("smg").price;
            Assert.AreEqual(expected,Disk().balance);Assert.IsTrue(session.Equip("smg"));session.Initialize(catalog,path);Assert.AreEqual(10000,session.Data.balance);expected=10000;
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");Assert.IsTrue(session.PrepareMission());
            Assert.AreEqual(expected,Disk().balance);Assert.IsTrue(session.Settle(0,false));
            Assert.AreEqual(expected,Disk().balance);session.Initialize(catalog,path);Assert.AreEqual(10000,session.Data.balance);CollectionAssert.Contains(session.Data.owned,"smg");
        }
        [Test]public void NewProfilesAndPreviouslyGrantedLowProfilesReceiveFloorWithoutReducingRichBalances()
        {
            session.Initialize(catalog,path);Assert.AreEqual(10000,Disk().balance);
            var rich=new GameProgress{balance=15000};new ProgressStore(path).Save(rich);session.Initialize(catalog,path);Assert.AreEqual(15000,Disk().balance);
            session.Data.balance=12;Assert.IsTrue(session.Save());session.Initialize(catalog,path);Assert.AreEqual(10000,session.Data.balance);
        }
        [Test]public void DisabledSettingPreservesReleaseEconomyAndCanBeEnabledLater()
        {
            catalog.developmentPlaytestCredits=false;session.Initialize(catalog,path);Assert.AreEqual(catalog.startingMoney,session.Data.balance);Assert.AreEqual(0,session.Data.developmentCreditRevision);
            catalog.developmentPlaytestCredits=true;Assert.IsTrue(session.ApplyDevelopmentCredits());Assert.AreEqual(10000,Disk().balance);
        }
        [Test]public void FailedTopUpRollsBackMarkerAndBalanceAndCanRetry()
        {
            new ProgressStore(path).Save(new GameProgress{balance=19});Directory.CreateDirectory(path+".tmp");
            session.Initialize(catalog,path);Assert.AreEqual(19,session.Data.balance);Assert.AreEqual(0,session.Data.developmentCreditRevision);Assert.AreEqual(19,Disk().balance);
            Directory.Delete(path+".tmp");Assert.IsTrue(session.ApplyDevelopmentCredits());Assert.AreEqual(10000,Disk().balance);Assert.IsFalse(session.ApplyDevelopmentCredits());
        }
        [Test]public void BasicPistolUsesOrdinaryArtAndExactlyFifteenRounds()
        {
            var w=catalog.Find("pistol").weapon;Assert.AreEqual(15,w.MagazineCapacity);Assert.IsNull(w.MagazineSprite);
            Assert.AreEqual("LarperPistol",w.EquippedSprite.name);Assert.AreEqual(15,catalog.Find("pistol-ammo").quantity);
            var runtime=new InsaneDreams.Inventory.RuntimeWeapon(w);runtime.SetAmmunition(15,30);
            for(int i=0;i<15;i++)Assert.IsTrue(runtime.Cycle.TryAttack(i*(w.FiringInterval+.01f)));
            Assert.AreEqual(0,runtime.MagazineAmmo);Assert.IsFalse(runtime.Cycle.TryAttack(100));
        }
    }
}
#endif
