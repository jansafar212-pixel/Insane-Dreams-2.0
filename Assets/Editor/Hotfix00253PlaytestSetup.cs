using UnityEditor;
using UnityEngine;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Prototype;

// Follow-up playtest brief reuses the historical0025.3 label; preserve its setup.
public static class Hotfix00253PlaytestSetup
{
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode first.");
        const string samurai="Assets/Prefabs/Enemies/Samurai.prefab";
        var root=PrefabUtility.LoadPrefabContents(samurai);
        try{var s=new SerializedObject(root.GetComponent<PlayerMotor2D>());s.FindProperty("jumpSpeed").floatValue=15;s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,samurai);}
        finally{PrefabUtility.UnloadPrefabContents(root);}
        foreach(var path in new[]{"Assets/Prefabs/Player/WarehousePlayer.prefab","Assets/Prefabs/Enemies/Larper.prefab"})
        {
            root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var s=new SerializedObject(root.GetComponent<CombatFeedback>());var list=s.FindProperty("shotAnchoredFlashWeapons");list.arraySize=2;
                list.GetArrayElementAtIndex(0).objectReferenceValue=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset");
                list.GetArrayElementAtIndex(1).objectReferenceValue=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");
                s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        AssetDatabase.SaveAssets();return "Samurai jump13.28→15; player/Larper Glock flashes anchored to recorded shots. Other weapon flashes unchanged.";
    }
}
