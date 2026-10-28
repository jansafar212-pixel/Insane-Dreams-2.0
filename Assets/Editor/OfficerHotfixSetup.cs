using System;
using System.IO;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEngine;

public static class OfficerHotfixSetup
{
    const string Folder="Assets/Data/FirstOperation";
    static void Edit(UnityEngine.Object o,Action<SerializedObject> change){var s=new SerializedObject(o);change(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    static Sprite Pixels(string name,int w,int h,Vector2 pivot,Func<int,int,Color> pixel)
    {
        string path=Folder+"/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing!=null)return existing;
        var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)t.SetPixel(x,y,pixel(x,y));t.Apply();AssetDatabase.CreateAsset(t,Folder+"/"+name+"Pixels.asset");
        var sprite=Sprite.Create(t,new Rect(0,0,w,h),pivot,32,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode");
        Color ink=new Color32(25,29,43,255),steel=new Color32(153,160,159,255),dark=new Color32(74,83,89,255),grip=new Color32(65,49,40,255);
        var pistol=Pixels("DesertEagle",27,14,new Vector2(6f/27,6f/14),(x,y)=>
        {
            // Heavy stepped slide/barrel; a hollow trigger guard and slanted grip.
            if(y>=9&&y<=12&&x>=3&&x<=26)
            {if(y==12&&(x<5||x>23))return Color.clear;if(x==26||y==9)return ink;return y==10?dark:steel;}
            if(y==13&&x>=6&&x<=8||y==13&&x>=22&&x<=23)return ink;
            if(y==8&&x>=5&&x<=15||y==7&&(x==10||x==15)||y==6&&x>=9&&x<=14)return ink;
            int left=y>=5?4:y>=3?3:2;if(y>=1&&y<=8&&x>=left&&x<=left+4)return x==left||x==left+4||y==1?ink:grip;
            return Color.clear;
        });
        var magazine=Pixels("DesertEagleMagazine",3,5,new Vector2(.5f,1),(x,y)=>y==0?ink:x==0?dark:steel);
        var gun=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Folder+"/OfficerPistol.asset");
        Edit(gun,s=>{s.FindProperty("equippedSprite").objectReferenceValue=pistol;s.FindProperty("icon").objectReferenceValue=pistol;s.FindProperty("magazineSprite").objectReferenceValue=magazine;s.FindProperty("magazineMountOffset").vector2Value=new Vector2(-2f/32,-3f/32);s.FindProperty("physicalMagazineReload").boolValue=true;s.FindProperty("magazineCapacity").intValue=7;s.FindProperty("reloadDuration").floatValue=2.8f;s.FindProperty("damage").floatValue=55;});
        string path=Folder+"/Officer.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            if(root.GetComponent<OfficerWeaponPresentation>()==null)root.AddComponent<OfficerWeaponPresentation>();
            var reload=root.GetComponent<PistolMagazineReload>();if(reload==null)reload=root.AddComponent<PistolMagazineReload>();
            Edit(reload,s=>{var a=s.FindProperty("pistols");a.arraySize=1;a.GetArrayElementAtIndex(0).objectReferenceValue=gun;});
            Edit(root.GetComponent<OfficerShotTelegraph>(),s=>s.FindProperty("windupSeconds").floatValue=1.15f);
            Edit(root.GetComponent<EnemyCombat>(),s=>s.FindProperty("reloadDurationVariation").floatValue=0);
            var rig=root.GetComponentInChildren<CutoutRig>();rig.weapon.sprite=pistol;rig.SetMagazine(magazine,gun.MagazineMountOffset,true);rig.EnsureMuzzle();rig.muzzle.localPosition=new Vector3(pistol.bounds.max.x,5f/32,0);
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();
        File.WriteAllBytes("TestResults/OfficerHotfix/DesertEagle.png",pistol.texture.EncodeToPNG());
        return "Officer-only native sprites/prefab: 55 damage, 7 rounds, 2.8s reload, 1.15s laser; no scene or placement edits";
    }
}
