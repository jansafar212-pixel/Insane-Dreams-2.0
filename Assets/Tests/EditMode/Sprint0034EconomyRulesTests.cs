#if UNITY_EDITOR
using System.Linq;
using System.IO;
using InsaneDreams.Progression;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0034EconomyRulesTests
    {
        GameCatalog Catalog=>AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");
        [Test]public void CatalogContainsOnlyExistingArsenalAndMedicalAndSeparateFiniteAmmoPacks()
        {
            var c=Catalog;var guns=c.offers.Where(o=>o.category==ShopCategory.Weapons&&o.weapon.WeaponCategory==WeaponCategory.Firearm).ToArray();Assert.AreEqual(8,guns.Length);Assert.AreEqual(c.offers.Length,c.offers.Select(o=>o.id).Distinct().Count());
            var actual=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");foreach(var gun in guns){CollectionAssert.Contains(actual.weapons,gun.weapon);var ammo=c.Find(gun.id+"-ammo");Assert.AreSame(gun.weapon,ammo.weapon);Assert.AreEqual(gun.weapon.MagazineCapacity,ammo.quantity);Assert.Greater(ammo.price,0);Assert.GreaterOrEqual(gun.reserveCapacity,ammo.quantity);}
            Assert.IsTrue(c.Find("pistol").starter);Assert.AreEqual(0,c.Find("pistol").price);Assert.AreEqual("relax its not addictive, just habit forming",c.Find("faintanyl").description);Assert.AreEqual(1,c.offers.Count(o=>o.category==ShopCategory.Faintanyl));Assert.Greater(c.Find("sniper-ammo").price/(float)c.Find("sniper-ammo").quantity,c.Find("pistol-ammo").price/(float)c.Find("pistol-ammo").quantity);
        }
        [Test]public void ArmorLimitsAreBoundedAndHelmetsHaveSmallerPenalties()
        {
            var c=Catalog;foreach(var gun in c.offers.Where(o=>o.category==ShopCategory.Weapons&&o.weapon.WeaponCategory==WeaponCategory.Firearm))foreach(int body in new[]{0,1,2})foreach(int helmet in new[]{0,1,2})
            {var d=new GameProgress{bodyArmorTier=body,helmetTier=helmet};int limit=c.ReserveCapacity(d,gun);Assert.GreaterOrEqual(limit,gun.weapon.MagazineCapacity);Assert.LessOrEqual(limit,gun.reserveCapacity);Assert.LessOrEqual(c.ReloadMultiplier(d),1.3f);Assert.GreaterOrEqual(c.GrenadeCapacity(d),3);}
            Assert.Less(c.heavyHelmet.reloadMultiplier,c.heavyBody.reloadMultiplier);Assert.Less(c.heavyHelmet.reservePenalty,c.heavyBody.reservePenalty);
        }
        [Test]public void BalanceSimulatesPoorAverageAndSavingPlayersFromActualLevelWalletCount()
        {
            var c=Catalog;var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Demo.unity");int count=0;
            try{foreach(var root in scene.GetRootGameObjects())count+=root.GetComponentsInChildren<InsaneDreams.Enemies.EnemyWalletDrop>(true).Length;}finally{UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);}
            Assert.AreEqual(13,count);int averageWallets=Mathf.RoundToInt(count*12.5f),success=c.Payout(averageWallets,true),poor=c.Payout(20,false),afterFirst=c.startingMoney+success;
            Assert.Less(poor,success);Assert.LessOrEqual(c.failurePayoutCap,c.completionReward/4);Assert.GreaterOrEqual(afterFirst,c.Find("smg").price+c.Find("smg-ammo").price+c.Find("tape").price);Assert.Less(afterFirst,c.Find("desert-eagle").price);
            int premiumRuns=Mathf.CeilToInt((c.Find("desert-eagle").price-c.startingMoney)/(float)success);Assert.GreaterOrEqual(premiumRuns,2);Assert.LessOrEqual(premiumRuns,4);
            Directory.CreateDirectory("TestResults/Economy");File.WriteAllText("TestResults/Economy/BalanceSimulation.txt",$"Scene wallets={count}, range $5-20, expected collection ${count*12.5f:0.0} before rounding. Completion reward ${c.completionReward}.\nPoor: $0 balance, 20 wallet dollars on failure => ${poor}; free basic allowance {c.basicAllowanceMagazines} magazines (75 rounds); no premium refill.\nAverage: new ${c.startingMoney} + rounded mean wallet payout ${success} => ${afterFirst}; SMG ${c.Find("smg").price} + ammo ${c.Find("smg-ammo").price} + tape ${c.Find("tape").price} => ${afterFirst-c.Find("smg").price-c.Find("smg-ammo").price-c.Find("tape").price} left.\nSaver: {premiumRuns} average completions => ${c.startingMoney+premiumRuns*success}; Desert Eagle ${c.Find("desert-eagle").price}, leaves ${c.startingMoney+premiumRuns*success-c.Find("desert-eagle").price}. No mission spending assumed; actual wallet collection/consumption varies.\nFailure maximum ${c.failurePayoutCap}, no completion reward, banked balance retained. No per-enemy kill payout added.\n");
        }
    }
}
#endif
