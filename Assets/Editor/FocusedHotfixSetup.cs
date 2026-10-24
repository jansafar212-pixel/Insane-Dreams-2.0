using UnityEditor;
using UnityEngine;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
// Native editable flat-pixel recipe; historical references and weapon balance are untouched.
public static class FocusedHotfixSetup
{
    public static void Carbine()
    {
        const string folder="Assets/Art/FocusedHotfix";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","FocusedHotfix");
        Color ink=new Color32(25,29,43,255),body=new Color32(60,67,75,255),steel=new Color32(111,123,130,255),wood=new Color32(181,132,65,255);
        var texture=new Texture2D(30,11,TextureFormat.RGBA32,false){name="CrookedCarbinePixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<11;y++)for(int x=0;x<30;x++)
        {
            Color p=Color.clear;
            // Crooked wooden butt, narrow neck, squat receiver and separate handguard.
            if(x<6&&y>=3&&y<=7)p=x==0||y==3?ink:wood;
            if(x<2&&y>=2&&y<=8)p=x==0?ink:wood;
            if(x>=6&&x<=8&&y>=5&&y<=6)p=wood;
            if(x>=8&&x<=20&&y>=4&&y<=8)p=y==8?steel:y==4||x==8?ink:body;
            if(x>=20&&x<=23&&y>=5&&y<=7)p=y==5?ink:wood;
            if(x>=24&&y>=6&&y<=7)p=y==6||x==29?ink:steel;
            if(x>=8&&x<=10&&y>=1&&y<=4)p=x==8||y==1?ink:wood;
            if(x>=14&&x<=16&&y<=4)p=x==14||y==0?ink:body;
            if(x==17&&y<=1)p=ink;
            if(x==11&&y==9||x==27&&y==8)p=ink;
            texture.SetPixel(x,y,p);
        }
        texture.Apply();
        var oldTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/CrookedCarbinePixels.asset");
        if(oldTexture==null){oldTexture=texture;AssetDatabase.CreateAsset(texture,folder+"/CrookedCarbinePixels.asset");}
        else{EditorUtility.CopySerialized(texture,oldTexture);Object.DestroyImmediate(texture);}
        var sprite=Sprite.Create(oldTexture,new Rect(0,0,30,11),new Vector2(7f/30,5f/11),32,0,SpriteMeshType.FullRect);sprite.name="CrookedCarbine";
        var old=AssetDatabase.LoadAssetAtPath<Sprite>(folder+"/CrookedCarbine.asset");
        if(old==null){old=sprite;AssetDatabase.CreateAsset(sprite,folder+"/CrookedCarbine.asset");}else{EditorUtility.CopySerialized(sprite,old);Object.DestroyImmediate(sprite);}
        var weapon=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");var so=new SerializedObject(weapon);so.FindProperty("equippedSprite").objectReferenceValue=old;so.ApplyModifiedPropertiesWithoutUndo();
        const string path="Assets/Prefabs/Player/WarehousePlayer.prefab";var g=PrefabUtility.LoadPrefabContents(path);
        try{g.GetComponentInChildren<CutoutRig>(true).weapon.sprite=old;PrefabUtility.SaveAsPrefabAsset(g,path);}finally{PrefabUtility.UnloadPrefabContents(g);}
        AssetDatabase.SaveAssets();
    }
}
