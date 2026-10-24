using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint015RulesTests
    {
        [Test] public void MissionOccupantsAndInspectorDefaultsPreserveModularReferences()
        {
            var player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab");
            var healing=new SerializedObject(player.GetComponent<PlayerHealing>());Assert.AreEqual(25,healing.FindProperty("healingPerSecond").floatValue);Assert.AreEqual(200,healing.FindProperty("initialDuctTape").floatValue);Assert.AreEqual(200,healing.FindProperty("maximumDuctTape").floatValue);Assert.IsTrue(healing.FindProperty("requireReleaseAfterDamage").boolValue);
            Assert.AreEqual(.7f,new SerializedObject(player.GetComponent<PlayerPosture2D>()).FindProperty("heightMultiplier").floatValue);
            var enemy=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab");var combat=new SerializedObject(enemy.GetComponent<EnemyCombat>());
            Assert.IsFalse(combat.FindProperty("allowOffscreenFire").boolValue);Assert.AreEqual(3,combat.FindProperty("visibilityMemory").floatValue);Assert.AreEqual(.1f,combat.FindProperty("perceptionSeconds").floatValue);
            var weapon=(WeaponDefinition)combat.FindProperty("firearm").objectReferenceValue;Assert.AreEqual(27,weapon.MagazineCapacity);Assert.GreaterOrEqual(combat.FindProperty("initialReserveAmmo").intValue,weapon.MagazineCapacity*5);Assert.AreEqual(4,weapon.ReloadDuration);Assert.AreEqual(40,combat.FindProperty("distantSpread").floatValue);
            var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Mission01.unity");
            try{
                var points=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<EnemySpawnPoint>(true)).ToArray();Assert.AreEqual(10,points.Length);
                foreach(var point in points){var s=new SerializedObject(point);var actor=(LarperBrain)s.FindProperty("placedActor").objectReferenceValue;Assert.IsNotNull(actor);Assert.IsTrue(actor.gameObject.activeSelf);Assert.IsNotNull(PrefabUtility.GetCorrespondingObjectFromSource(actor));Assert.AreEqual(point.transform.position,actor.transform.position);Assert.AreEqual(1.25f,s.FindProperty("cameraSafetyMargin").floatValue);}
                Assert.AreEqual(1,scene.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<PlayerActions>(true).Length));
            }finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
