using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using InsaneDreams.Enemies;
using InsaneDreams.Missions;
using InsaneDreams.Visuals;
public static class Hotfix00253Setup
{
    private static Sprite VfxSprite(string name,bool soft)
    {
        const string folder="Assets/Art/Hotfix00253";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","Hotfix00253");
        string path=folder+"/"+name+".asset";foreach(var o in AssetDatabase.LoadAllAssetsAtPath(path))if(o is Sprite found)return found;
        int size=soft?16:9;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float dx=(x+.5f-size*.5f)/(size*.5f),dy=(y+.5f-size*.5f)/(size*.5f),r=Mathf.Sqrt(dx*dx+dy*dy);
            float alpha=soft?Mathf.Pow(Mathf.Clamp01(1-r),2):Mathf.Abs(dx)<.15f||Mathf.Abs(dy)<.15f||Mathf.Abs(dx)+Mathf.Abs(dy)<.65f?1:0;
            texture.SetPixel(x,y,new Color(1,1,1,alpha));
        }
        texture.Apply();AssetDatabase.CreateAsset(texture,path);var sprite=Sprite.Create(texture,new Rect(0,0,size,size),Vector2.one*.5f,size);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;
    }
    public static void Apply()
    {
        var flash=VfxSprite("FireworkFlash",false);var glow=VfxSprite("FireworkGlow",true);
        const string firework="Assets/Prefabs/Enemies/CartoonFirework.prefab";var fx=PrefabUtility.LoadPrefabContents(firework);
        try{var s=new SerializedObject(fx.GetComponent<FireworkProjectile>());s.FindProperty("flashSprite").objectReferenceValue=flash;s.FindProperty("glowSprite").objectReferenceValue=glow;s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(fx,firework);}finally{PrefabUtility.UnloadPrefabContents(fx);}
        const string larper="Assets/Prefabs/Enemies/Larper.prefab";
        var root=PrefabUtility.LoadPrefabContents(larper);
        try
        {
            var reload=root.GetComponent<PistolMagazineReload>();if(reload==null)reload=root.AddComponent<PistolMagazineReload>();
            var s=new SerializedObject(reload);s.FindProperty("debrisCapacity").intValue=2;var a=s.FindProperty("pistols");a.arraySize=1;a.GetArrayElementAtIndex(0).objectReferenceValue=AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");s.ApplyModifiedPropertiesWithoutUndo();
            s=new SerializedObject(root.GetComponent<EnemyCombat>());s.FindProperty("nearMissSuppression").floatValue=63.8f;s.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root,larper);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        const string expert="Assets/Prefabs/Enemies/FireworksExpert.prefab";root=PrefabUtility.LoadPrefabContents(expert);
        try{var reload=root.GetComponent<PistolMagazineReload>();if(reload!=null)Object.DestroyImmediate(reload);var s=new SerializedObject(root.GetComponent<FireworksCombat>());s.FindProperty("recoverySeconds").floatValue=3;s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,expert);}finally{PrefabUtility.UnloadPrefabContents(root);}
        const string samurai="Assets/Prefabs/Enemies/Samurai.prefab";root=PrefabUtility.LoadPrefabContents(samurai);
        try{var reload=root.GetComponent<PistolMagazineReload>();if(reload!=null)Object.DestroyImmediate(reload);PrefabUtility.SaveAsPrefabAsset(root,samurai);}finally{PrefabUtility.UnloadPrefabContents(root);}
        foreach(var path in new[]{"Assets/Scenes/Warehouse_Demo.unity","Assets/Scenes/Warehouse_Mission01.unity"})
        {
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                foreach(var g in scene.GetRootGameObjects())foreach(var z in g.GetComponentsInChildren<MissionTrigger>(true))if(z.Zone==MissionZone.Processing)
                {
                    // Same eastward boundary, full playable height including jumps.
                    var box=z.GetComponent<BoxCollider2D>();var size=box.size;size.y=28;box.size=size;
                    var p=z.transform.position;p.y=6;z.transform.position=p;
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }finally{EditorSceneManager.CloseScene(scene,true);}
        }
        AssetDatabase.SaveAssets();
    }
}
