using UnityEditor;
using UnityEngine;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Traversal;

public static class Sprint0122AiSetup
{
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode before authoring.");
        var weapon=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");
        var data=new SerializedObject(weapon);data.FindProperty("reloadDuration").floatValue=4;data.FindProperty("firearmRange").floatValue=28;data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(weapon);
        const string path="Assets/Prefabs/Enemies/Larper.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            if(root.GetComponent<PlayerPosture2D>()==null)root.AddComponent<PlayerPosture2D>();
            var settings=new SerializedObject(root.GetComponent<EnemyCombat>());
            foreach(var pair in new[]{("detectionRange",28f),("engagementRange",24f),("effectiveRange",11f),("closeSpread",3f),("distantSpread",40f),("withdrawalDistance",6f),("withdrawalSeconds",4.5f),("minimumWithdrawalDistance",2.5f),("coverSearchDistance",7f),("reloadDurationVariation",.05f)})settings.FindProperty(pair.Item1).floatValue=pair.Item2;
            settings.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,path);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();return "Buzzbox reload 4s/ray28; Larper detection28/engagement24/accuracy11; retreat6 and bounded cover search7; existing crouch controller added.";
    }
}
