using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class CombatRulesTests
    {
        private readonly List<Object> created = new List<Object>();
        private T Keep<T>(T obj) where T : Object { created.Add(obj); return obj; }
        private WeaponDefinition Definition(string name = "WarehouseCarbine") =>
            Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/" + name + ".asset")));
        [TearDown] public void Cleanup() { foreach (var obj in created) Object.DestroyImmediate(obj); created.Clear(); }
        [Test] public void CooldownCannotBeBypassedAndEmptyAttemptsDoNotSpendAmmo()
        {
            var weapon = new RuntimeWeapon(Definition(), 10); int initial = weapon.MagazineAmmo;
            Assert.IsTrue(weapon.Cycle.TryAttack(0)); Assert.IsFalse(weapon.Cycle.TryAttack(0));
            Assert.IsFalse(weapon.Cycle.TryAttack(.19f)); Assert.IsTrue(weapon.Cycle.TryAttack(.2f));
            Assert.AreEqual(initial - 2, weapon.MagazineAmmo);
            weapon.SetAmmunition(0, 10); float next = weapon.Cycle.NextAttackAt;
            Assert.IsFalse(weapon.Cycle.TryAttack(10)); Assert.AreEqual(next, weapon.Cycle.NextAttackAt);
            Assert.IsFalse(weapon.Cycle.TryAttack(float.NaN)); Assert.AreEqual(10, weapon.ReserveAmmo);
        }
        [Test] public void MagazineDepletesExactlyOncePerSuccessfulAttack()
        {
            var weapon = new RuntimeWeapon(Definition(), 12); int count = weapon.MagazineAmmo;
            for (int i = 0; i < count; i++) Assert.IsTrue(weapon.Cycle.TryAttack(i));
            Assert.AreEqual(0, weapon.MagazineAmmo); Assert.AreEqual(12, weapon.ReserveAmmo);
            Assert.IsFalse(weapon.Cycle.TryAttack(count));
        }
        [Test] public void ReloadTransfersOnlyAtDeadlineAndConservesAmmunition()
        {
            var weapon = new RuntimeWeapon(Definition()); weapon.SetAmmunition(2, 4);
            Assert.IsTrue(weapon.Cycle.TryReload(10)); Assert.IsFalse(weapon.Cycle.TryReload(10));
            Assert.IsFalse(weapon.Cycle.TryAttack(10.5f)); weapon.Cycle.Tick(11.49f);
            Assert.AreEqual(2, weapon.MagazineAmmo); Assert.AreEqual(4, weapon.ReserveAmmo);
            weapon.Cycle.Tick(11.5f); Assert.IsFalse(weapon.Cycle.IsReloading);
            Assert.AreEqual(6, weapon.MagazineAmmo); Assert.AreEqual(0, weapon.ReserveAmmo);
            weapon.Cycle.Tick(100); Assert.AreEqual(6, weapon.MagazineAmmo); Assert.IsFalse(weapon.Cycle.TryReload(100));
        }
        [Test] public void CancelledReloadTransfersNothingAndCanRestart()
        {
            var weapon = new RuntimeWeapon(Definition()); weapon.SetAmmunition(1, 8);
            weapon.Cycle.TryReload(0); weapon.Cycle.CancelReload(); weapon.Cycle.Tick(10);
            Assert.AreEqual(1, weapon.MagazineAmmo); Assert.AreEqual(8, weapon.ReserveAmmo);
            Assert.IsTrue(weapon.Cycle.TryReload(10)); weapon.Cycle.Tick(11.5f);
            Assert.AreEqual(9, weapon.MagazineAmmo); Assert.AreEqual(0, weapon.ReserveAmmo);
        }
        [Test] public void MeleeUsesCooldownButNeverAmmunitionOrReload()
        {
            var weapon = new RuntimeWeapon(Definition("PracticeBaton"));
            Assert.IsTrue(weapon.Cycle.TryAttack(0)); Assert.IsFalse(weapon.Cycle.TryAttack(.1f));
            Assert.IsTrue(weapon.Cycle.TryAttack(.5f)); Assert.AreEqual(0, weapon.MagazineAmmo);
            Assert.AreEqual(0, weapon.ReserveAmmo); Assert.IsFalse(weapon.Cycle.TryReload(1));
        }
        [Test] public void AttacksAndReloadNeverMutateSharedDefinitions()
        {
            var definition = Definition(); string before = JsonUtility.ToJson(definition);
            var first = new RuntimeWeapon(definition, 20); var second = new RuntimeWeapon(definition, 20);
            first.Cycle.TryAttack(0); first.Cycle.TryReload(1); first.Cycle.Tick(3);
            Assert.AreEqual(definition.MagazineCapacity, second.MagazineAmmo);
            Assert.AreEqual(before, JsonUtility.ToJson(definition));
        }
        [Test] public void DamageFiltersInvalidFriendlyAndSelfHitsAndDefeatsOnce()
        {
            var go = Keep(new GameObject("HealthRuleTest")); var health = go.AddComponent<Health>();
            var data = new SerializedObject(health); data.FindProperty("maximum").floatValue = 40;
            data.FindProperty("team").enumValueIndex = (int)CombatTeam.Enemy; data.ApplyModifiedPropertiesWithoutUndo(); health.Restore();
            int defeats = 0; health.Defeated += _ => defeats++;
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(5, CombatTeam.Enemy)));
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(-1, CombatTeam.Player)));
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(float.NaN, CombatTeam.Player)));
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(5, CombatTeam.Player, go)));
            Assert.IsTrue(health.ApplyDamage(new DamageInfo(10, CombatTeam.Player))); Assert.AreEqual(30, health.Current);
            Assert.IsTrue(health.ApplyDamage(new DamageInfo(100, CombatTeam.Player))); Assert.AreEqual(0, health.Current);
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(100, CombatTeam.Player))); Assert.AreEqual(1, defeats);
        }
    }
}
