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
        GameSession session;GameCatalog catalog;string directory,path;float volume;
        [SetUp]public void Setup(){volume=AudioListener.volume;directory=Path.Combine(Path.GetTempPath(),"AkumuLoadout0040-"+Guid.NewGuid().ToString("N"));path=Path.Combine(directory,"progress.json");catalog=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"));catalog.playtestWalletDiagnostics=false;session=new GameObject("IsolatedLoadout0040").AddComponent<GameSession>();session.Initialize(catalog,path);}
        [TearDown]public void Cleanup(){Object.DestroyImmediate(session.gameObject);Object.DestroyImmediate(catalog);AudioListener.volume=volume;foreach(string suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);if(Directory.Exists(directory))Directory.Delete(directory);}
        void Shop(){session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));}
        void Own(params string[] ids){session.Data.balance=100000;float now=10;foreach(string id in ids)if(!session.Data.owned.Contains(id)){Assert.IsTrue(session.Buy(id,now),id);now++;}}
        [Test]public void NoHiddenThirdFirearmExistsAndOldNumericRequestsAreRejected()
        {
            var go=new GameObject("TwoSlotRuntime");try{var load=go.AddComponent<PlayerLoadout>();var gun=new RuntimeWeapon(catalog.Find("pistol").weapon);Assert.AreEqual(3,Enum.GetValues(typeof(EquipmentSlot)).Length);Assert.AreEqual(2,(int)EquipmentSlot.Melee);Assert.IsNull(typeof(PlayerLoadout).GetField("firearm3",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic));Assert.IsNull(typeof(GameProgress).GetField("secondary2"));Assert.IsFalse(load.Equip((EquipmentSlot)3,gun));load.Select((EquipmentSlot)3);Assert.AreEqual(EquipmentSlot.Firearm1,load.SelectedSlot);Assert.IsNull(load.Get((EquipmentSlot)3));JsonUtility.FromJsonOverwrite("{\"firearm3\":{}}",load);Assert.IsNull(load.Get((EquipmentSlot)3));}finally{Object.DestroyImmediate(go);}
        }
        [Test]public void LegacyInspectorRoleMismatchCannotExposeAnotherUsableSidearm()
        {
            var go=new GameObject("LegacySerializedRole");try{var load=go.AddComponent<PlayerLoadout>();var s=new SerializedObject(load);s.FindProperty("firearm1").FindPropertyRelative("definition").objectReferenceValue=catalog.Find("pistol").weapon;s.ApplyModifiedPropertiesWithoutUndo();Assert.IsNull(load.Get(EquipmentSlot.Firearm1));Assert.IsNull(load.SelectedWeapon);}finally{Object.DestroyImmediate(go);}
        }
        [Test]public void EveryAddedFirearmCanBePurchasedOnceAndEquippedInItsUnchangedRole()
        {
            Shop();session.Data.balance=100000;float now=10;foreach(var spec in new[]{("sks",EquipmentSlot.Firearm1),("aa12",EquipmentSlot.Firearm1),("sawed-off",EquipmentSlot.Firearm2)}){var offer=catalog.Find(spec.Item1);int before=session.Data.balance;Assert.IsTrue(session.Buy(offer.id,now));Assert.AreEqual(before-offer.price,session.Data.balance);Assert.IsTrue(session.Equip(offer.id,spec.Item2));Assert.IsFalse(session.Buy(offer.id,now+1));Assert.AreEqual(before-offer.price,session.Data.balance);Assert.AreEqual(32,offer.weapon.EquippedSprite.pixelsPerUnit);now+=2;}Assert.AreEqual(15,catalog.Find("pistol").weapon.MagazineCapacity);
        }
        [Test]public void ReplacingEitherSlotPreservesOwnershipAmmoAndAllowsEmptySlots()
        {
            Shop();Own("sks","aa12","desert-eagle");session.Data.Ammo("pistol").magazine=7;session.Data.Ammo("pistol").reserve=23;
            Assert.IsTrue(session.Equip("sks"));Assert.IsTrue(session.Equip("pistol",true));Assert.IsFalse(session.Equip("aa12",true));Assert.IsFalse(session.Equip("pistol"));Assert.IsFalse(session.Equip("desert-eagle",(EquipmentSlot)3));
            Assert.IsTrue(session.Equip("aa12"));Assert.IsTrue(session.Equip("desert-eagle",true));Assert.AreEqual("aa12",session.Data.primary);Assert.AreEqual("desert-eagle",session.Data.secondary);Assert.Contains("sks",session.Data.owned);Assert.Contains("pistol",session.Data.owned);Assert.AreEqual(7,session.Data.Ammo("pistol").magazine);Assert.AreEqual(23,session.Data.Ammo("pistol").reserve);
            Assert.IsTrue(session.Unequip(EquipmentSlot.Firearm1));Assert.IsTrue(session.Unequip(EquipmentSlot.Firearm2));Assert.IsTrue(session.PrepareMission());Assert.IsNull(session.Data.primary);Assert.IsNull(session.Data.secondary);
        }
        [TestCase("pistol","desert-eagle","pistol")][TestCase("missing","desert-eagle","desert-eagle")][TestCase(null,"desert-eagle","desert-eagle")][TestCase("pistol","pistol","pistol")]
        public void ThreeFirearmSaveMigrationIsDeterministicLosslessAndIdempotent(string secondary,string extra,string expected)
        {
            var legacy=new GameProgress{balance=15432,primary="sks",secondary=secondary,grenades=new[]{2,1},tape=73,developmentCreditRevision=35};legacy.owned.AddRange(new[]{"sks","pistol","desert-eagle"});legacy.completed.Add("warehouse");legacy.Ammo("desert-eagle").magazine=4;legacy.Ammo("desert-eagle").reserve=39;
            Directory.CreateDirectory(directory);string json=JsonUtility.ToJson(legacy);json=json.Insert(json.LastIndexOf('}'),",\"secondary2\":\""+extra+"\"");File.WriteAllText(path,json);session.Initialize(catalog,path);Assert.AreEqual("sks",session.Data.primary);Assert.AreEqual(expected,session.Data.secondary);Assert.AreEqual(15432,session.Data.balance);Assert.AreEqual(4,session.Data.Ammo("desert-eagle").magazine);Assert.AreEqual(39,session.Data.Ammo("desert-eagle").reserve);Assert.Contains("desert-eagle",session.Data.owned);Assert.AreEqual(73,session.Data.tape);CollectionAssert.AreEqual(new[]{2,1,0},session.Data.grenades);
            Assert.IsTrue(session.Save());StringAssert.DoesNotContain("secondary2",File.ReadAllText(path));string first=JsonUtility.ToJson(session.Data);session.Initialize(catalog,path);Assert.AreEqual(first,JsonUtility.ToJson(session.Data));
        }
        [Test]public void TwoRuntimeWeaponsKeepIndependentClocksAndPartialStocks()
        {
            Shop();Own("aa12");var go=new GameObject("TwoGuns");try{var load=go.AddComponent<PlayerLoadout>();var a=new RuntimeWeapon(catalog.Find("aa12").weapon);var b=new RuntimeWeapon(catalog.Find("pistol").weapon);a.SetAmmunition(3,40);b.SetAmmunition(7,20);Assert.IsTrue(load.Equip(EquipmentSlot.Firearm1,a));Assert.IsTrue(load.Equip(EquipmentSlot.Firearm2,b));Assert.IsTrue(a.Cycle.TryReload(10));Assert.IsFalse(a.Cycle.TryAttack(10));Assert.IsTrue(b.Cycle.TryAttack(10));load.Select(EquipmentSlot.Firearm2);load.Select(EquipmentSlot.Firearm1);a.Cycle.Tick(a.Cycle.ReloadEndsAt);Assert.AreEqual(43,a.MagazineAmmo+a.ReserveAmmo);Assert.AreEqual(6,b.MagazineAmmo);Assert.AreEqual(20,b.ReserveAmmo);}finally{Object.DestroyImmediate(go);}
        }
        [Test]public void LegacyOverfilledMagazineAndSpentWalletPreserveProgress()
        {
            session.Data.primary="pistol";session.Data.secondary="sks";session.Data.owned.Add("sks");session.Data.Ammo("pistol").magazine=27;session.Data.Ammo("pistol").reserve=432;session.Data.balance=9;session.Data.tape=62;Assert.IsTrue(session.Save());session.Initialize(catalog,path);Assert.AreEqual("sks",session.Data.primary);Assert.AreEqual("pistol",session.Data.secondary);Assert.AreEqual(15,session.Data.Ammo("pistol").magazine);Assert.AreEqual(444,session.Data.Ammo("pistol").reserve);Assert.AreEqual(10000,session.Data.balance);Assert.AreEqual(62,session.Data.tape);
        }
        [Test]public void HeavyArmorLimitsRemainUsefulAndReloadPenaltyBounded(){var heavy=new GameProgress{bodyArmorTier=2,helmetTier=2};foreach(var gun in catalog.offers.Where(o=>o.category==ShopCategory.Weapons&&o.weapon?.WeaponCategory==WeaponCategory.Firearm)){Assert.GreaterOrEqual(catalog.ReserveCapacity(heavy,gun),gun.weapon.MagazineCapacity);Assert.GreaterOrEqual(catalog.ReserveCapacity(heavy,gun),gun.reserveCapacity/3);}Assert.LessOrEqual(catalog.ReloadMultiplier(heavy),1.3f);}
    }
}
