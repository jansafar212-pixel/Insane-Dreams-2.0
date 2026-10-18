using System;
using System.Collections.Generic;
using System.Reflection;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class DataAndGestureTests
    {
        private readonly List<Object> created = new List<Object>();
        private T Keep<T>(T value) where T : Object { created.Add(value); return value; }
        private static void Set(object target, string name, object value)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null) { field.SetValue(target, value); return; }
            }
            throw new MissingFieldException(name);
        }
        private WeaponDefinition Weapon(WeaponCategory category)
        {
            var data = Keep(ScriptableObject.CreateInstance<WeaponDefinition>());
            Set(data, "stableId", Guid.NewGuid().ToString("N")); Set(data, "weaponCategory", category);
            return data;
        }
        [TearDown] public void Cleanup() { foreach (Object obj in created) Object.DestroyImmediate(obj); created.Clear(); }

        [Test] public void UpwardGestureRequiresNeutralAndNeverRepeatsOnLanding()
        {
            var gesture = new JoystickJumpGesture();
            Assert.IsTrue(gesture.Evaluate(Vector2.up, false, false, true));
            Assert.IsFalse(gesture.Evaluate(Vector2.up, false, false, false));
            Assert.IsFalse(gesture.Evaluate(Vector2.up, false, false, true));
            Assert.IsFalse(gesture.Evaluate(new Vector2(0, 0.4f), false, false, true));
            gesture.Evaluate(Vector2.zero, false, false, true);
            Assert.IsTrue(gesture.Evaluate(new Vector2(.7f, .7f), false, false, true));
        }
        [Test] public void LadderArbitrationConsumesHeldGestureAcrossTransitions()
        {
            var gesture = new JoystickJumpGesture();
            Assert.IsFalse(gesture.Evaluate(Vector2.up, true, false, true));
            Assert.IsFalse(gesture.Evaluate(Vector2.up, false, false, true));
            gesture.Evaluate(Vector2.zero, true, false, true);
            Assert.IsTrue(gesture.Evaluate(new Vector2(.7f, .7f), true, false, true));
            Assert.IsTrue(gesture.ClimbSuppressed);
            gesture.Evaluate(Vector2.zero, true, true, false);
            Assert.IsFalse(gesture.Evaluate(Vector2.up, true, true, false));
            Assert.IsFalse(gesture.Evaluate(Vector2.up, false, false, true));
        }
        [Test] public void UpwardDiagonalClimbsButLateralDiagonalRetainsJumpAndNeutralHysteresis()
        {
            foreach (float x in new[] { -.5f, .5f })
            {
                var gesture = new JoystickJumpGesture();
                Assert.IsFalse(gesture.Evaluate(new Vector2(x, .85f), true, false, true));
                Assert.IsFalse(gesture.ClimbSuppressed);
                Assert.IsFalse(gesture.Evaluate(Vector2.up, false, false, true), "Held climb cannot become a jump at exit");
                gesture.Evaluate(new Vector2(0, .4f), false, false, true);
                Assert.IsFalse(gesture.Evaluate(Vector2.up, false, false, true), "Partial release must not rearm");
                gesture.Evaluate(Vector2.zero, true, false, true);
                Assert.IsTrue(gesture.Evaluate(new Vector2(Mathf.Sign(x) * .7f, .7f), true, false, true));
                Assert.IsTrue(gesture.ClimbSuppressed);
            }
        }
        [Test] public void EmptySlotsAndMeleeAreExplicit()
        {
            var loadout = Keep(new GameObject("LoadoutTest")).AddComponent<PlayerLoadout>();
            loadout.Select(EquipmentSlot.Firearm2); Assert.IsNull(loadout.SelectedWeapon);
            var melee = new RuntimeWeapon(Weapon(WeaponCategory.Melee));
            Assert.IsFalse(loadout.Equip(EquipmentSlot.Firearm1, melee));
            Assert.IsTrue(loadout.Equip(EquipmentSlot.Melee, melee));
            loadout.Select(EquipmentSlot.Melee); Assert.AreSame(melee, loadout.SelectedWeapon);
            melee.SetAmmunition(99, 99); Assert.IsFalse(melee.UsesAmmunition); Assert.AreEqual(0, melee.MagazineAmmo);
            Assert.AreSame(melee, loadout.Unequip(EquipmentSlot.Melee)); Assert.IsNull(loadout.SelectedWeapon);
        }
        [Test] public void SwitchingPreservesIndependentRuntimeAmmoAndDefinitions()
        {
            var definition = Weapon(WeaponCategory.Firearm); string before = JsonUtility.ToJson(definition);
            var loadout = Keep(new GameObject("LoadoutTest")).AddComponent<PlayerLoadout>();
            var first = new RuntimeWeapon(definition); var second = new RuntimeWeapon(definition);
            first.SetAmmunition(3, 7); second.SetAmmunition(8, 20);
            Assert.IsTrue(loadout.Equip(EquipmentSlot.Firearm1, first));
            Assert.IsTrue(loadout.Equip(EquipmentSlot.Firearm2, second));
            loadout.Select(EquipmentSlot.Firearm2); loadout.Select(EquipmentSlot.Firearm1);
            Assert.AreEqual(3, loadout.SelectedWeapon.MagazineAmmo); Assert.AreEqual(8, second.MagazineAmmo);
            Assert.AreNotEqual(first.InstanceId, second.InstanceId);
            Assert.AreEqual(before, JsonUtility.ToJson(definition));
        }
        [Test] public void StableSnapshotsResolveByDefinitionIdentity()
        {
            var definition = Weapon(WeaponCategory.Firearm); var weapon = new RuntimeWeapon(definition);
            weapon.SetAmmunition(2, 9); var restored = RuntimeWeapon.Restore(weapon.Capture(), definition);
            Assert.AreEqual(weapon.InstanceId, restored.InstanceId); Assert.AreEqual(2, restored.MagazineAmmo);
            Assert.Throws<ArgumentException>(() => RuntimeWeapon.Restore(weapon.Capture(), Weapon(WeaponCategory.Firearm)));
            Set(definition, "stableId", "invalid"); Assert.IsFalse(definition.HasValidIdentity);
        }
        [Test] public void EnemyIdentityAndExplicitCapabilitiesAreIndependent()
        {
            var data = Keep(ScriptableObject.CreateInstance<InsaneDreams.Enemies.EnemyArchetype>());
            Set(data, "stableId", Guid.NewGuid().ToString("N"));
            Set(data, "capabilities", InsaneDreams.Enemies.MovementCapabilities.Walk);
            Assert.IsTrue(data.HasValidIdentity); Assert.IsTrue(data.Allows(InsaneDreams.Enemies.MovementCapabilities.Walk));
            Assert.IsFalse(data.Allows(InsaneDreams.Enemies.MovementCapabilities.Climb));
            Assert.IsFalse(data.Allows(InsaneDreams.Enemies.MovementCapabilities.Drop));
            Assert.IsFalse(data.Allows(InsaneDreams.Enemies.MovementCapabilities.Jump));
            Assert.IsNotNull(data.ValidationError); // No prefab assigned: useful authoring feedback.
        }
        [Test] public void AuthoredDefinitionsHaveValidUniqueIdentities()
        {
            var identities = new HashSet<string>();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:DefinitionAsset"))
            {
                var data = UnityEditor.AssetDatabase.LoadAssetAtPath<InsaneDreams.Data.DefinitionAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                Assert.IsTrue(data.HasValidIdentity, data.name); Assert.IsTrue(identities.Add(data.StableId), data.name);
            }
            Assert.GreaterOrEqual(identities.Count, 4);
        }
        [Test] public void SafeAreaInsetsRespectCutoutsInBothLandscapeOrientations()
        {
            var leftCutout = WarehouseTouchControls.SafeAreaInsets(new Rect(80, 0, 1200, 720), new Vector2(1280, 720));
            Assert.That(leftCutout.x, Is.EqualTo(.0625f).Within(.0001f)); Assert.AreEqual(0, leftCutout.y);
            var rightCutout = WarehouseTouchControls.SafeAreaInsets(new Rect(0, 0, 1200, 720), new Vector2(1280, 720));
            Assert.AreEqual(0, rightCutout.x); Assert.That(rightCutout.y, Is.EqualTo(.0625f).Within(.0001f));
            var bars = WarehouseTouchControls.SafeAreaInsets(new Rect(0, 40, 1280, 640), new Vector2(1280, 720));
            Assert.That(bars.z, Is.EqualTo(40f / 720).Within(.0001f)); Assert.That(bars.w, Is.EqualTo(40f / 720).Within(.0001f));
        }
    }
}
