using System;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HitFeedbackHotfixSetup
{
    const string Folder="Assets/Art/HitFeedbackHotfix";
    static Sprite Save(string name,Texture2D texture,Vector2 pivot)
    {
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/"+name+".asset");
        if(existing!=null){UnityEngine.Object.DestroyImmediate(texture);return existing;}
        texture.name=name+"Pixels";texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;texture.Apply();
        AssetDatabase.CreateAsset(texture,Folder+"/"+name+"Pixels.asset");
        var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),pivot,32,0,SpriteMeshType.FullRect);
        sprite.name=name;AssetDatabase.CreateAsset(sprite,Folder+"/"+name+".asset");return sprite;
    }
    static void Configure(GameObject root,Material material)
    {
        foreach(var f in root.GetComponentsInChildren<CombatFeedback>(true))
        {var s=new SerializedObject(f);s.FindProperty("damageFlashMaterial").objectReferenceValue=material;s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(f);}
        foreach(var r in root.GetComponentsInChildren<HitReaction2D>(true))
        {
            if(r.GetComponent<Health>().Team!=CombatTeam.Enemy||!r.UsesLivingPhysics)continue;
            var s=new SerializedObject(r);s.FindProperty("impulseDegrees").floatValue=175;s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(r);
        }
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode");
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","HitFeedbackHotfix");
        var original=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Arsenal0039/AA12.asset");
        var body=new Texture2D(34,14,TextureFormat.RGBA32,false);body.SetPixels(original.texture.GetPixels());
        var magazine=new Texture2D(6,4,TextureFormat.RGBA32,false);
        for(int y=0;y<4;y++)for(int x=0;x<6;x++){magazine.SetPixel(x,y,body.GetPixel(x+12,y+1));body.SetPixel(x+12,y+1,Color.clear);}
        var bodySprite=Save("AA12Body",body,new Vector2(9f/34,5f/14));
        var magazineSprite=Save("AA12Magazine",magazine,new Vector2(.5f,1));
        var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/AA12.asset");var sw=new SerializedObject(w);
        sw.FindProperty("equippedSprite").objectReferenceValue=bodySprite;
        sw.FindProperty("magazineSprite").objectReferenceValue=magazineSprite;
        sw.FindProperty("magazineMountOffset").vector2Value=new Vector2(6f/32,0);
        sw.FindProperty("physicalMagazineReload").boolValue=true;sw.FindProperty("loadsIndividually").boolValue=false;
        sw.FindProperty("shellSprite").objectReferenceValue=null;sw.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(w);
        var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/HitFlash.mat");
        if(material==null){material=new Material(Shader.Find("AKUMU/HitFlash"));AssetDatabase.CreateAsset(material,Folder+"/HitFlash.mat");}
        foreach(string g in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs","Assets/Data/FirstOperation"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(g);var p=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(p.GetComponentInChildren<CombatFeedback>(true)==null)continue;
            var instance=PrefabUtility.LoadPrefabContents(path);try{Configure(instance,material);PrefabUtility.SaveAsPrefabAsset(instance,path);}finally{PrefabUtility.UnloadPrefabContents(instance);}
        }
        foreach(string path in LevelArt0026Setup.Scenes)
        {
            var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;
            if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try{if(scene.isDirty)throw new InvalidOperationException("Preserve unsaved changes: "+path);foreach(var root in scene.GetRootGameObjects())Configure(root,material);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
            finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        AssetDatabase.SaveAssets();return "AA12 existing pixels split into body/magazine; shared flash material wired; enemy visual impulse160→175; balance unchanged";
    }
}
