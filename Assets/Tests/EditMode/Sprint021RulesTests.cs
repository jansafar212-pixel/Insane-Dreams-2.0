using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
namespace InsaneDreams.Tests
{
    public sealed class Sprint021RulesTests
    {
        private RuntimeWeapon Weapon()=>new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset"),135);
        [Test] public void MalfunctionBlocksOperationsThenRecoversWithoutChangingAmmoOrCadence()
        {
            var w=Weapon();Assert.IsTrue(w.Cycle.TryAttack(0));int magazine=w.MagazineAmmo;
            Assert.IsFalse(w.Cycle.TryMalfunction(0,1));Assert.IsTrue(w.Cycle.TryMalfunction(1,1.4f));
            Assert.IsFalse(w.Cycle.TryMalfunction(1.1f,1));Assert.IsFalse(w.Cycle.TryAttack(1.5f));Assert.IsFalse(w.Cycle.TryReload(1.5f));
            Assert.AreEqual(magazine,w.MagazineAmmo);Assert.AreEqual(135,w.ReserveAmmo);Assert.IsFalse(w.Cycle.IsMalfunctioning(2.41f));
            Assert.IsTrue(w.Cycle.TryAttack(2.41f));Assert.AreEqual(magazine-1,w.MagazineAmmo);
            Assert.IsTrue(w.Cycle.TryReload(3));w.Cycle.Tick(3+w.Definition.ReloadDuration+.01f);Assert.AreEqual(w.Definition.MagazineCapacity,w.MagazineAmmo);Assert.AreEqual(133,w.ReserveAmmo);
        }
        [Test] public void InvalidEmptyReloadingAndMeleeOperationsCannotMalfunction()
        {
            var w=Weapon();Assert.IsFalse(w.Cycle.TryMalfunction(float.NaN,1));Assert.IsFalse(w.Cycle.TryMalfunction(0,float.PositiveInfinity));Assert.IsFalse(w.Cycle.TryMalfunction(0,0));
            w.SetAmmunition(0,135);Assert.IsFalse(w.Cycle.TryMalfunction(0,1));Assert.IsTrue(w.Cycle.TryReload(0));Assert.IsFalse(w.Cycle.TryMalfunction(1,1));
            var melee=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/PracticeBaton.asset"));Assert.IsFalse(melee.Cycle.TryMalfunction(0,1));
        }
    }
}
