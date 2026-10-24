using UnityEditor;
using UnityEngine;
using InsaneDreams.Enemies;
public static class Hotfix00254Setup
{
    public static void Apply()
    {
        const string path="Assets/Prefabs/Enemies/Larper.prefab";var g=PrefabUtility.LoadPrefabContents(path);
        try{var s=new SerializedObject(g.GetComponent<EnemyCombat>());s.FindProperty("panicEscapeArchetype").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset");s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(g,path);}finally{PrefabUtility.UnloadPrefabContents(g);}
        foreach(var name in new[]{"Ordinary","Cowardly","Overconfident"})
        {
            var p=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+name+".asset");var s=new SerializedObject(p);var a=s.FindProperty("withdraw");
            foreach(var line in new[]{"TACTICAL RETREAT!","I'M TOO RICH TO DIE!","COVER ME, IDIOTS!","THIS IS NOT LIKE WARZONE!"})
            {bool found=false;for(int i=0;i<a.arraySize;i++)found|=a.GetArrayElementAtIndex(i).stringValue==line;if(!found){int n=a.arraySize;a.arraySize++;a.GetArrayElementAtIndex(n).stringValue=line;}}
            s.ApplyModifiedPropertiesWithoutUndo();
        }
        AssetDatabase.SaveAssets();
    }
}
