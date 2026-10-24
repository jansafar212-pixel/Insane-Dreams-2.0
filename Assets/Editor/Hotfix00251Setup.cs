using UnityEditor;
using UnityEngine;
using InsaneDreams.Inventory;
public static class Hotfix00251Setup
{
    // Split the existing native pixels; preserve the original art and every combat value.
    public static void Apply()
    {
        const string folder="Assets/Art/Hotfix00251";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","Hotfix00251");
        var source=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/FocusedHotfix/CrookedCarbine.asset");
        var body=new Texture2D(30,11,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};body.SetPixels(source.texture.GetPixels());
        var mag=new Texture2D(4,5,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};
        for(int y=0;y<5;y++)for(int x=0;x<4;x++){mag.SetPixel(x,y,body.GetPixel(x+14,y));body.SetPixel(x+14,y,Color.clear);}
        var bodySprite=Save(folder,"CarbineReceiver",body,new Vector2(7f/30,5f/11));var magSprite=Save(folder,"CarbineMagazine",mag,new Vector2(.5f,1));
        var definition=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");var so=new SerializedObject(definition);
        so.FindProperty("equippedSprite").objectReferenceValue=bodySprite;so.FindProperty("magazineSprite").objectReferenceValue=magSprite;
        so.FindProperty("magazineMountOffset").vector2Value=new Vector2(9f/32,0);so.FindProperty("physicalMagazineReload").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();
        const string path="Assets/Prefabs/Enemies/Larper.prefab";var g=PrefabUtility.LoadPrefabContents(path);
        try{so=new SerializedObject(g.GetComponent<InsaneDreams.Enemies.EnemyCombat>());so.FindProperty("combatRepositionSeconds").vector2Value=new Vector2(4,7);so.FindProperty("combatRepositionChance").floatValue=.35f;so.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(g,path);}finally{PrefabUtility.UnloadPrefabContents(g);}
        AssetDatabase.SaveAssets();
    }
    private static Sprite Save(string folder,string name,Texture2D texture,Vector2 pivot)
    {
        texture.name=name+"Pixels";texture.wrapMode=TextureWrapMode.Clamp;texture.Apply();string path=folder+"/"+name;
        var old=AssetDatabase.LoadAssetAtPath<Texture2D>(path+"Pixels.asset");
        if(old==null){old=texture;AssetDatabase.CreateAsset(old,path+"Pixels.asset");}else{EditorUtility.CopySerialized(texture,old);Object.DestroyImmediate(texture);}
        var sprite=Sprite.Create(old,new Rect(0,0,old.width,old.height),pivot,32,0,SpriteMeshType.FullRect);sprite.name=name;
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path+".asset");if(existing==null){existing=sprite;AssetDatabase.CreateAsset(existing,path+".asset");}else{EditorUtility.CopySerialized(sprite,existing);Object.DestroyImmediate(sprite);}return existing;
    }
}
