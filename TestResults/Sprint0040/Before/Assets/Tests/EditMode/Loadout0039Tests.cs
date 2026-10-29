using System;
using System.IO;
using System.Linq;
using InsaneDreams.Inventory;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class Loadout0039Tests
    {
        [Test]public void EmptyInlineInspectorWeaponIsAnEmptySlot()
        {
            var go=new GameObject("EmptyInspectorLoadout");
            try{var load=go.AddComponent<PlayerLoadout>();JsonUtility.FromJsonOverwrite("{\"firearm3\":{}}",load);Assert.IsNull(load.Get(EquipmentSlot.Firearm3));load.Select(EquipmentSlot.Firearm3);Assert.IsNull(load.SelectedWeapon);}
            finally{Object.DestroyImmediate(go);}
        }
        GameSession session;GameCatalog catalog;string directory,path;float volume;
        [SetUp]public void Setup()
        {
            volume=AudioListener.volume;directory=Path.Combine(Path.GetTempPath(),"AkumuLoadout0039-"+Guid.NewGuid().ToString("N"));path=Path.Combine(directory,"progress.json");
            catalog=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"));catalog.playtestWalletDiagnostics=false;
            session=new GameObject("IsolatedLoadout0039Session").AddComponent<GameSession>();session.Initialize(catalog,path);
        }
        [TearDown]public void Cleanup()
        {
            if(session!=null)Object.DestroyImmediate(session.gameObject);if(catalog!=null)Object.DestroyImmediate(catalog);AudioListener.volume=volume;
            foreach(string suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);
            if(Directory.Exists(directory))Directory.Delete(directory);
        }
        void Shop()
        {
            session.Navigate(FlowScreen.Levels);var level=catalog.levels.First(l=>session.IsUnlocked(l)&&!string.IsNullOrEmpty(l.scene));Assert.IsTrue(session.SelectLevel(level.id));
        }
        ShopOffer Gun(string id)
        {
            var offer=catalog.Find(id);Assert.NotNull(offer,id);Assert.AreEqual(ShopCategory.Weapons,offer.category,id);Assert.NotNull(offer.weapon,id);return offer;
        }
        void Own(params string[] ids)
        {
            session.Data.balance=100000;float now=10;foreach(string id in ids)if(!session.Data.owned.Contains(id)){Assert.IsTrue(session.Buy(id,now),id);now++;}
        }
        [Test]public void EveryAddedFirearmCanBePurchasedOnceWithItsRealPriceAndEquippedInItsRole()
        {
            Shop();session.Data.balance=100000;
            var specs=new[]{("sks",FirearmRole.Primary,EquipmentSlot.Firearm1),("aa12",FirearmRole.Primary,EquipmentSlot.Firearm1),("sawed-off",FirearmRole.Secondary,EquipmentSlot.Firearm3)};
            float now=10;foreach(var spec in specs)
            {
                var offer=Gun(spec.Item1);int before=session.Data.balance;Assert.AreEqual(spec.Item2,offer.weapon.FirearmRole);
                Assert.IsTrue(session.Buy(offer.id,now));Assert.AreEqual(before-offer.price,session.Data.balance);Assert.IsTrue(session.Data.owned.Contains(offer.id));
                Assert.AreEqual(offer.weapon.MagazineCapacity,session.Data.Ammo(offer.id).magazine);Assert.IsTrue(session.Equip(offer.id,spec.Item3));
                Assert.IsFalse(session.Buy(offer.id,now+1));Assert.AreEqual(before-offer.price,session.Data.balance);now+=2;
                Assert.NotNull(offer.weapon.EquippedSprite);Assert.AreEqual(32,offer.weapon.EquippedSprite.pixelsPerUnit);Assert.AreEqual(FilterMode.Point,offer.weapon.EquippedSprite.texture.filterMode);
            }
            Assert.AreEqual(15,Gun("pistol").weapon.MagazineCapacity);
        }
        [Test]public void OnePrimaryAndTwoDifferentSecondariesEnforceRolesWithoutLosingOwnership()
        {
            Shop();Own("sks","aa12","desert-eagle","sawed-off");
            Assert.IsTrue(session.Equip("sks",EquipmentSlot.Firearm1));Assert.IsTrue(session.Equip("pistol",EquipmentSlot.Firearm2));Assert.IsTrue(session.Equip("desert-eagle",EquipmentSlot.Firearm3));
            Assert.IsFalse(session.Equip("aa12",EquipmentSlot.Firearm2));Assert.IsFalse(session.Equip("sks",EquipmentSlot.Firearm3));Assert.IsFalse(session.Equip("pistol",EquipmentSlot.Firearm1));
            Assert.AreEqual("sks",session.Data.primary);Assert.AreEqual("pistol",session.Data.secondary);Assert.AreEqual("desert-eagle",session.Data.secondary2);
            Assert.IsTrue(session.Equip("pistol",EquipmentSlot.Firearm3));Assert.IsNull(session.Data.secondary);Assert.AreEqual("pistol",session.Data.secondary2,"Moving a weapon must not duplicate it");
            Assert.IsTrue(session.Data.owned.Contains("desert-eagle"));Assert.IsTrue(session.Equip("sawed-off",EquipmentSlot.Firearm2));
            Assert.IsTrue(session.Unequip(EquipmentSlot.Firearm1));Assert.IsTrue(session.Unequip(EquipmentSlot.Firearm2));Assert.IsTrue(session.Unequip(EquipmentSlot.Firearm3));
            Assert.IsTrue(session.PrepareMission());Assert.IsNull(session.Data.primary);Assert.IsNull(session.Data.secondary);Assert.IsNull(session.Data.secondary2);
            Assert.IsTrue(session.Data.owned.Contains("sks"));Assert.IsTrue(session.Data.owned.Contains("sawed-off"));
        }
        [Test]public void ThreeRuntimeWeaponsKeepIndependentAmmunitionAndPersistTheirPartialStocks()
        {
            Shop();Own("aa12","desert-eagle");var go=new GameObject("IsolatedThreeSlotLoadout");
            try
            {
                // Use the AA-12's atomic magazine exchange here. The SKS deliberately
                // allows loaded-round firing to interrupt its individual-round reload.
                var load=go.AddComponent<PlayerLoadout>();var primary=new RuntimeWeapon(Gun("aa12").weapon);var side1=new RuntimeWeapon(Gun("pistol").weapon);var side2=new RuntimeWeapon(Gun("desert-eagle").weapon);
                primary.SetAmmunition(3,40);side1.SetAmmunition(7,20);side2.SetAmmunition(2,11);
                Assert.IsTrue(load.Equip(EquipmentSlot.Firearm1,primary));Assert.IsTrue(load.Equip(EquipmentSlot.Firearm2,side1));Assert.IsTrue(load.Equip(EquipmentSlot.Firearm3,side2));
                Assert.IsFalse(load.Equip(EquipmentSlot.Firearm3,new RuntimeWeapon(side1.Definition)));Assert.IsFalse(load.Equip(EquipmentSlot.Firearm2,new RuntimeWeapon(primary.Definition)));
                Assert.IsTrue(primary.Cycle.TryReload(10));Assert.IsTrue(side1.Cycle.TryAttack(10));Assert.IsFalse(primary.Cycle.TryAttack(10));
                for(int i=0;i<4;i++)foreach(var slot in new[]{EquipmentSlot.Firearm1,EquipmentSlot.Firearm2,EquipmentSlot.Firearm3})load.Select(slot);
                Assert.AreEqual(3,primary.MagazineAmmo);Assert.AreEqual(6,side1.MagazineAmmo);Assert.AreEqual(2,side2.MagazineAmmo);
                primary.Cycle.Tick(primary.Cycle.ReloadEndsAt);Assert.AreEqual(43,primary.MagazineAmmo+primary.ReserveAmmo);Assert.AreEqual(6,side1.MagazineAmmo);Assert.AreEqual(20,side1.ReserveAmmo);Assert.AreEqual(11,side2.ReserveAmmo);
                foreach(var entry in new[]{("aa12",primary,EquipmentSlot.Firearm1),("pistol",side1,EquipmentSlot.Firearm2),("desert-eagle",side2,EquipmentSlot.Firearm3)})
                {
                    Assert.IsTrue(session.Equip(entry.Item1,entry.Item3));var stock=session.Data.Ammo(entry.Item1);stock.magazine=entry.Item2.MagazineAmmo;stock.reserve=entry.Item2.ReserveAmmo;
                    var restored=RuntimeWeapon.Restore(entry.Item2.Capture(),entry.Item2.Definition);Assert.AreEqual(entry.Item2.InstanceId,restored.InstanceId);Assert.AreEqual(stock.magazine,restored.MagazineAmmo);Assert.AreEqual(stock.reserve,restored.ReserveAmmo);
                }
                Assert.IsTrue(session.Save());session.Initialize(catalog,path);Assert.AreEqual("aa12",session.Data.primary);Assert.AreEqual("pistol",session.Data.secondary);Assert.AreEqual("desert-eagle",session.Data.secondary2);
                Assert.AreEqual(primary.MagazineAmmo,session.Data.Ammo("aa12").magazine);Assert.AreEqual(primary.ReserveAmmo,session.Data.Ammo("aa12").reserve);Assert.AreEqual(6,session.Data.Ammo("pistol").magazine);Assert.AreEqual(2,session.Data.Ammo("desert-eagle").magazine);
            }
            finally{Object.DestroyImmediate(go);}
        }
        [Test]public void OldTwoPrimarySaveMigratesWithoutDiscardingOwnedWeaponsAmmoOrShortGrenadeArrays()
        {
            var legacy=new GameProgress{balance=37,primary="sks",secondary="aa12",grenades=new[]{2,1},tape=73,developmentCreditRevision=35};
            legacy.owned.AddRange(new[]{"sks","aa12","pistol"});legacy.completed.Add("warehouse");legacy.Ammo("sks").magazine=3;legacy.Ammo("sks").reserve=17;legacy.Ammo("aa12").magazine=5;legacy.Ammo("aa12").reserve=41;
            Assert.IsTrue(new ProgressStore(path).Save(legacy));session.Initialize(catalog,path);Assert.AreEqual("sks",session.Data.primary);Assert.IsNull(session.Data.secondary);Assert.IsNull(session.Data.secondary2);
            CollectionAssert.IsSubsetOf(new[]{"sks","aa12","pistol"},session.Data.owned);Assert.AreEqual(5,session.Data.Ammo("aa12").magazine);Assert.AreEqual(41,session.Data.Ammo("aa12").reserve);
            CollectionAssert.AreEqual(new[]{2,1,0},session.Data.grenades);Assert.AreEqual(73,session.Data.tape);Assert.IsTrue(session.Data.completed.Contains("warehouse"));Assert.GreaterOrEqual(session.Data.balance,10000);
            session.Data.primary="pistol";session.Data.secondary="sks";session.Data.Ammo("pistol").magazine=27;session.Data.Ammo("pistol").reserve=432;
            Assert.IsTrue(session.Save());session.Initialize(catalog,path);Assert.AreEqual("sks",session.Data.primary);Assert.AreEqual("pistol",session.Data.secondary);Assert.IsNull(session.Data.secondary2);
            Assert.AreEqual(15,session.Data.Ammo("pistol").magazine);Assert.AreEqual(444,session.Data.Ammo("pistol").reserve,"Previously loaded excess is preserved in storage");
        }
        [Test]public void FreshAndSpentExistingSavesRestoreWalletMinimumWithoutResettingOtherProgress()
        {
            Assert.IsTrue(catalog.developmentPlaytestCredits);Assert.GreaterOrEqual(session.Data.balance,10000);session.Data.balance=9;session.Data.developmentCreditRevision=35;session.Data.tape=62;session.Data.faintanyl=1;session.Data.grenades=new[]{1,2,3};session.Data.completed.Add("warehouse");session.Data.Ammo("pistol").magazine=7;session.Data.Ammo("pistol").reserve=11;
            Assert.IsTrue(session.Save());session.Initialize(catalog,path);Assert.AreEqual(10000,session.Data.balance);Assert.AreEqual(62,session.Data.tape);Assert.AreEqual(1,session.Data.faintanyl);CollectionAssert.AreEqual(new[]{1,2,3},session.Data.grenades);Assert.IsTrue(session.Data.completed.Contains("warehouse"));Assert.AreEqual(7,session.Data.Ammo("pistol").magazine);Assert.AreEqual(11,session.Data.Ammo("pistol").reserve);
            session.Data.balance=25000;Assert.IsTrue(session.Save());session.Initialize(catalog,path);Assert.AreEqual(25000,session.Data.balance);
        }
        [Test]public void HeavyBodyAndHelmetKeepUsefulAmmunitionCapacityAndBoundedReloadPenalty()
        {
            var unarmored=new GameProgress();var heavy=new GameProgress{bodyArmorTier=2,helmetTier=2,armor=90,helmet=35};
            foreach(string id in new[]{"sks","aa12","sawed-off","pistol","desert-eagle","lmg"})
            {
                var gun=Gun(id);int normal=catalog.ReserveCapacity(unarmored,gun),armored=catalog.ReserveCapacity(heavy,gun);
                Assert.GreaterOrEqual(armored,Math.Min(normal,gun.weapon.MagazineCapacity),id);Assert.GreaterOrEqual(armored,normal/3,id);Assert.LessOrEqual(armored,normal,id);
            }
            Assert.That(catalog.ReloadMultiplier(heavy),Is.InRange(1f,1.3f));Assert.AreEqual(2,(int)EquipmentSlot.Melee,"Keep old numeric slot values save compatible");Assert.AreEqual(3,(int)EquipmentSlot.Firearm3);
        }
    }
}
