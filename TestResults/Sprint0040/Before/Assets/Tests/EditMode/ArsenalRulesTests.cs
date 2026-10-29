using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class ArsenalRulesTests
    {
        [Test] public void SevenCoreWeaponsAndRetainedGlockHaveDistinctDataWithoutChangingEnemyEagle()
        {
            var c=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");Assert.AreEqual(11,c.weapons.Length);Assert.AreEqual(27,c.weapons[7].MagazineCapacity);
            Assert.AreEqual(WeaponFireMode.Automatic,c.weapons[1].FireMode);Assert.AreEqual(7,c.weapons[4].Pellets);Assert.Greater(c.weapons[5].MagazineCapacity,c.weapons[3].MagazineCapacity);Assert.AreEqual(WeaponFireMode.ReleaseToFire,c.weapons[6].FireMode);
            Assert.AreEqual(2.4f,AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/FirstOperation/OfficerPistol.asset").FiringInterval);
            Assert.AreEqual(14,c.weapons[0].Damage);Assert.AreEqual(20,c.weapons[3].Damage);
            Assert.AreEqual(100,c.weapons[5].MagazineCapacity);Assert.AreEqual(8,c.weapons[5].ReloadDuration);Assert.AreEqual(200,c.weapons[5].DeveloperReserve);
            foreach(var w in c.weapons){Assert.IsTrue(w.HasValidIdentity);Assert.NotNull(w.EquippedSprite);Assert.AreEqual(32,w.EquippedSprite.pixelsPerUnit);Assert.AreEqual(FilterMode.Point,w.EquippedSprite.texture.filterMode);}
        }
        [Test] public void EveryMagazineReloadAndInterruptedReloadConservesAmmunitionWithArmorPenalty()
        {
            foreach(var d in AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset").weapons)
            {
                var w=new RuntimeWeapon(d,100);w.SetReloadMultiplier(1.2f);float now=10;
                for(int i=0;i<d.MagazineCapacity;i++){Assert.IsTrue(w.Cycle.TryAttack(now),d.name);now+=d.FiringInterval+.01f;}
                Assert.AreEqual(0,w.MagazineAmmo);Assert.IsFalse(w.Cycle.TryAttack(now));Assert.IsTrue(w.Cycle.TryReload(now));
                Assert.AreEqual(now+d.ReloadStepDuration*1.2f,w.Cycle.ReloadEndsAt,.001f);w.Cycle.CancelReload();w.Cycle.Tick(now+20);Assert.AreEqual(0,w.MagazineAmmo);Assert.AreEqual(100,w.ReserveAmmo);
                Assert.IsTrue(w.Cycle.TryReload(now+21));w.Cycle.Tick(now+21+d.ReloadDuration*1.2f+.01f);Assert.AreEqual(d.MagazineCapacity,w.MagazineAmmo);Assert.AreEqual(100-d.MagazineCapacity,w.ReserveAmmo);
                w.Cycle.Tick(now+100);Assert.AreEqual(100,w.MagazineAmmo+w.ReserveAmmo,"Transfer must occur exactly once");
            }
        }
        [Test] public void CloseRangeWeaponsLoseEffectivenessWhilePrecisionAndLegacyDamageStayConstant()
        {
            var c=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");
            foreach(int i in new[]{2,4,5})Assert.Less(c.weapons[i].DamageAtDistance(c.weapons[i].AttackRange),c.weapons[i].DamageAtDistance(1));
            foreach(int i in new[]{0,1,3,6})Assert.AreEqual(c.weapons[i].Damage,c.weapons[i].DamageAtDistance(c.weapons[i].AttackRange));
        }
        [Test] public void NewLongGunsDoNotInheritThePistolMagazine()
        {
            var c=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");foreach(int i in new[]{4,6})Assert.IsNull(c.weapons[i].MagazineSprite,c.weapons[i].name);
            Assert.NotNull(c.weapons[5].MagazineSprite);
            Assert.NotNull(c.weapons[1].MagazineSprite);
        }
    }
}
