using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint012RulesTests
    {
        [Test] public void BasicPistolHasOriginalModularMagazineAndFiniteAutomaticBurst()
        {
            var def=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");
            Assert.IsNotNull(def);Assert.AreEqual(27,def.MagazineCapacity);Assert.AreEqual(5,def.Damage);
            Assert.IsTrue(AssetDatabase.GetAssetPath(def.EquippedSprite).StartsWith("Assets/Art/Sprint012/"));Assert.Greater(def.MagazineSprite.rect.height,def.EquippedSprite.rect.height*2);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab");
            Assert.AreSame(def,new SerializedObject(prefab.GetComponent<EnemyCombat>()).FindProperty("firearm").objectReferenceValue);
            var runtime=new RuntimeWeapon(def,54);
            for(int i=0;i<27;i++){Assert.IsTrue(runtime.Cycle.TryAttack(i*.11f));Assert.IsFalse(runtime.Cycle.TryAttack(i*.11f+.01f));}
            Assert.IsFalse(runtime.Cycle.TryAttack(3));Assert.IsTrue(runtime.Cycle.TryReload(3));Assert.IsFalse(runtime.Cycle.TryAttack(4));
            runtime.Cycle.Tick(3+def.ReloadDuration+.01f);Assert.AreEqual(27,runtime.MagazineAmmo);Assert.AreEqual(27,runtime.ReserveAmmo);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"));
        }
        [Test] public void Sprint0122ReloadVariationPreservesDeadlineAmmoAndSharedDefinition()
        {
            var def=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");var weapon=new RuntimeWeapon(def,36);weapon.SetAmmunition(0,36);
            Assert.AreEqual(4,def.ReloadDuration);Assert.IsFalse(weapon.Cycle.TryReload(0,float.NaN));Assert.IsFalse(weapon.Cycle.TryReload(0,0));
            Assert.IsTrue(weapon.Cycle.TryReload(0,1.05f));Assert.IsFalse(weapon.Cycle.TryReload(1));Assert.IsFalse(weapon.Cycle.TryAttack(3.8f));
            weapon.Cycle.Tick(4.19f);Assert.AreEqual(0,weapon.MagazineAmmo);Assert.AreEqual(36,weapon.ReserveAmmo);
            weapon.Cycle.Tick(4.21f);Assert.AreEqual(27,weapon.MagazineAmmo);Assert.AreEqual(9,weapon.ReserveAmmo);Assert.AreEqual(4,def.ReloadDuration);
        }
        [Test] public void CombatSpeechDensityAndDuplicateSuppressionDoNotReplaceOrdinaryGate()
        {
            var gate=new EncounterSpeechGate();Assert.IsTrue(gate.TryReserve(0,30,false));
            Assert.IsFalse(gate.TryCombat(.3f,"RELOADING!"));Assert.IsTrue(gate.TryCombat(.8f,"RELOADING!"));
            Assert.IsFalse(gate.TryCombat(.9f,"FALL BACK!"));Assert.IsFalse(gate.TryCombat(1.6f,"RELOADING!"));
            Assert.IsTrue(gate.TryCombat(1.6f,"FALL BACK!"));Assert.IsFalse(gate.TryReserve(2,30,false));
        }
        [Test] public void WarehouseHasSevenConfiguredActiveEncountersAndOneRepresentativeLowCover()
        {
            var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Testbed.unity");
            try{
                var roots=scene.GetRootGameObjects();var spawns=roots.SelectMany(g=>g.GetComponentsInChildren<EnemySpawnPoint>(true));
                Assert.AreEqual(7,spawns.Count(s=>s.gameObject.activeInHierarchy&&new SerializedObject(s).FindProperty("spawnOnStart").boolValue));
                var cover=roots.Single(g=>g.name=="LowCover_Sprint012");var box=cover.GetComponent<BoxCollider2D>();
                Assert.That(box.bounds.max.y,Is.EqualTo(.9f).Within(.001f));Assert.AreEqual(box.size,cover.GetComponent<SpriteRenderer>().size);
            }finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
