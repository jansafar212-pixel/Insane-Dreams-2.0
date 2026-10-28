using System;
using System.IO;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEngine;
public static class Sprint00271Setup
{
    static void Edit(UnityEngine.Object o,Action<SerializedObject> action){var s=new SerializedObject(o);action(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode");
        const string dir="Assets/Data/FirstOperation/";
        var pistol=AssetDatabase.LoadAssetAtPath<Sprite>(dir+"DesertEagle.asset");var texture=pistol.texture;
        Color32 oldGrip=new Color32(65,49,40,255),black=new Color32(38,42,46,255);
        var pixels=texture.GetPixels32();for(int i=0;i<pixels.Length;i++)if(pixels[i].Equals(oldGrip))pixels[i]=black;
        texture.SetPixels32(pixels);texture.Apply();EditorUtility.SetDirty(texture);
        var magazine=AssetDatabase.LoadAssetAtPath<Sprite>(dir+"DesertEagleMagazine.asset");var mt=magazine.texture;
        if(!mt.Reinitialize(2,4,TextureFormat.RGBA32,false))throw new InvalidOperationException("Magazine resize failed");
        for(int y=0;y<4;y++)for(int x=0;x<2;x++)mt.SetPixel(x,y,y==0?new Color32(25,29,43,255):x==0?new Color32(74,83,89,255):new Color32(153,160,159,255));mt.Apply();EditorUtility.SetDirty(mt);
        var sprite=Sprite.Create(mt,new Rect(0,0,2,4),new Vector2(.5f,1),32,0,SpriteMeshType.FullRect);sprite.name=magazine.name;EditorUtility.CopySerialized(sprite,magazine);UnityEngine.Object.DestroyImmediate(sprite);EditorUtility.SetDirty(magazine);
        var gun=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(dir+"OfficerPistol.asset");Edit(gun,s=>s.FindProperty("magazineMountOffset").vector2Value=new Vector2(-1f/32,1f/32));
        string path=dir+"Officer.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try{
            Edit(root.GetComponent<EnemyCombat>(),s=>{s.FindProperty("closeSpread").floatValue=1.25f;s.FindProperty("distantSpread").floatValue=4;s.FindProperty("aimSampleSeconds").floatValue=.06f;s.FindProperty("aimDegreesPerSecond").floatValue=360;});
            var rig=root.GetComponentInChildren<CutoutRig>(true);rig.SetMagazine(magazine,gun.MagazineMountOffset,true);rig.magazine.sortingOrder=rig.weapon.sortingOrder-1;
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");foreach(var offer in catalog.offers){if(offer.category==ShopCategory.DuctTape)offer.title="Duct Tape";if(offer.category==ShopCategory.Faintanyl)offer.title="Faintanyl";}EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        File.WriteAllBytes("TestResults/Sprint00271/DesertEagleAfter.png",texture.EncodeToPNG());
        return "Officer-only aim1.25/4deg, .06s/360deg tracking; black grip, seated2x4 magazine; medical names corrected. No scenes changed.";
    }
}
