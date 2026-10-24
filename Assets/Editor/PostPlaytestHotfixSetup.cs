using InsaneDreams.Enemies;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEngine;
public static class PostPlaytestHotfixSetup
{
    public static void Apply()
    {
        const string folder="Assets/Art/PostPlaytestHotfix";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","PostPlaytestHotfix");
        string path=folder+"/FaintanylSyringe.asset";Sprite syringe=null;
        foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path))if(asset is Sprite s)syringe=s;
        if(syringe==null)
        {
            // Flat, intentionally oversized equipment pixels; no body artwork is altered.
            var texture=new Texture2D(16,7,TextureFormat.RGBA32,false){name="FictionalFaintanylSyringe",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            var ink=new Color32(25,29,43,255);var paper=new Color32(255,255,235,255);var teal=new Color32(12,99,116,255);var yellow=new Color32(247,201,44,255);
            for(int y=0;y<7;y++)for(int x=0;x<16;x++)
            {Color c=Color.clear;if(x<4&&y==3)c=paper;if(x>=4&&x<=12&&y>=1&&y<=5)c=y==1||y==5||x==4||x==12?ink:x<8?teal:paper;if(x==10&&y>=2&&y<=4)c=ink;if(x>=13&&y==3||x==15&&y>=1&&y<=5)c=yellow;texture.SetPixel(x,y,c);}
            texture.Apply();AssetDatabase.CreateAsset(texture,path);syringe=Sprite.Create(texture,new Rect(0,0,16,7),new Vector2(.8f,.5f),32);syringe.name="FaintanylSyringe";AssetDatabase.AddObjectToAsset(syringe,texture);
        }
        var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");
        var offer=System.Array.Find(catalog.offers,x=>x.category==ShopCategory.Faintanyl);
        if(offer==null){offer=new ShopOffer{id="faintanyl",title="Faintanyl",category=ShopCategory.Faintanyl,price=75,quantity=1};var offers=new System.Collections.Generic.List<ShopOffer>(catalog.offers);offers.Add(offer);catalog.offers=offers.ToArray();}
        offer.description="relax its not addictive, just habit forming";EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssetIfDirty(catalog);
        const string player="Assets/Prefabs/Player/WarehousePlayer.prefab";var root=PrefabUtility.LoadPrefabContents(player);
        try
        {
            var death=root.GetComponent<PlayerDeathRagdoll>();if(death==null)death=root.AddComponent<PlayerDeathRagdoll>();var serialized=new SerializedObject(death);serialized.FindProperty("prefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<CartoonRagdoll>("Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab");serialized.ApplyModifiedPropertiesWithoutUndo();
            var gesture=root.GetComponent<PlayerSyringeVisual>();if(gesture==null)gesture=root.AddComponent<PlayerSyringeVisual>();serialized=new SerializedObject(gesture);serialized.FindProperty("syringeSprite").objectReferenceValue=syringe;serialized.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,player);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssetIfDirty(syringe.texture);
    }
}
