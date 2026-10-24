using UnityEditor;
using UnityEngine;
using InsaneDreams.Inventory;
public static class Hotfix00252Setup
{
    public static void Apply()
    {
        var buzz=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");
        var backup=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset");
        // Reuse approved Glock art; no weapon statistics or carbine data change.
        var s=new SerializedObject(backup);s.FindProperty("equippedSprite").objectReferenceValue=buzz.EquippedSprite;s.FindProperty("magazineSprite").objectReferenceValue=buzz.MagazineSprite;s.FindProperty("magazineMountOffset").vector2Value=buzz.MagazineMountOffset;s.ApplyModifiedPropertiesWithoutUndo();
        const string path="Assets/Prefabs/Player/WarehousePlayer.prefab";var g=PrefabUtility.LoadPrefabContents(path);
        try{var reload=g.GetComponent<InsaneDreams.Visuals.PistolMagazineReload>();if(reload==null)reload=g.AddComponent<InsaneDreams.Visuals.PistolMagazineReload>();s=new SerializedObject(reload);var a=s.FindProperty("pistols");a.arraySize=2;a.GetArrayElementAtIndex(0).objectReferenceValue=backup;a.GetArrayElementAtIndex(1).objectReferenceValue=buzz;s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(g,path);}finally{PrefabUtility.UnloadPrefabContents(g);}
        foreach(var name in new[]{"Ordinary","Cowardly","Overconfident"})
        {
            var p=AssetDatabase.LoadAssetAtPath<InsaneDreams.Enemies.LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+name+".asset");s=new SerializedObject(p);var a=s.FindProperty("suppressed");
            foreach(var line in new[]{"THEY'RE SHOOTING AT ME!","I'M NOT PAID ENOUGH!"}){bool found=false;for(int i=0;i<a.arraySize;i++)found|=a.GetArrayElementAtIndex(i).stringValue==line;if(!found){int n=a.arraySize;a.arraySize++;a.GetArrayElementAtIndex(n).stringValue=line;}}s.ApplyModifiedPropertiesWithoutUndo();
        }
        AssetDatabase.SaveAssets();
    }
}
