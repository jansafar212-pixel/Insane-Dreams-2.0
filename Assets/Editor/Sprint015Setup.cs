using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Traversal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.EditorTools
{
    public static class Sprint015Setup
    {
        private static void Float(Object target,string key,float value){var s=new SerializedObject(target);s.FindProperty(key).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode before authoring.");
            var weapon=AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");
            var w=new SerializedObject(weapon);w.FindProperty("magazineCapacity").intValue=27;w.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(weapon);
            foreach(var path in new[]{"Assets/Prefabs/Player/WarehousePlayer.prefab","Assets/Prefabs/Enemies/Larper.prefab"})
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try{
                    if(root.GetComponent<InsaneDreams.Input.PlayerActions>()!=null&&root.GetComponent<PlayerHealing>()==null)root.AddComponent<PlayerHealing>();
                    Float(root.GetComponent<PlayerPosture2D>(),"heightMultiplier",.7f);
                    Float(root.GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true),"crouchedMuzzleCeiling",-.06f);
                    var combat=root.GetComponent<EnemyCombat>();if(combat!=null){Float(combat,"visibilityMemory",3);var s=new SerializedObject(combat);s.FindProperty("initialReserveAmmo").intValue=135;s.FindProperty("allowOffscreenFire").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();}
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            // Reopen to pick up prefab changes before attaching authored occupants.
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Mission01.unity");
            foreach(var point in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<EnemySpawnPoint>(true)))
            {
                var s=new SerializedObject(point);var placed=s.FindProperty("placedActor");
                if(placed.objectReferenceValue!=null)continue;
                var actor=(GameObject)PrefabUtility.InstantiatePrefab(point.Archetype.Prefab,scene);
                actor.name="Occupant_"+point.name;actor.transform.SetParent(point.transform,false);actor.transform.SetPositionAndRotation(point.transform.position,point.transform.rotation);
                placed.objectReferenceValue=actor.GetComponent<LarperBrain>();s.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
