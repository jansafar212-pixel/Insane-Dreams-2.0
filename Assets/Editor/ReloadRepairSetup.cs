using UnityEditor;
using UnityEngine;
using InsaneDreams.Inventory;
public static class ReloadRepairSetup
{
    const string Folder="Assets/Art/ReloadRepair";
    static Sprite SpriteAsset(string name,Texture2D texture,Vector2 pivot)
    {
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/"+name+".asset");if(existing!=null){Object.DestroyImmediate(texture);return existing;}
        texture.name=name+"Pixels";texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;texture.Apply();AssetDatabase.CreateAsset(texture,Folder+"/"+name+"Pixels.asset");
        var s=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),pivot,32,0,SpriteMeshType.FullRect);s.name=name;AssetDatabase.CreateAsset(s,Folder+"/"+name+".asset");return s;
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Stop Play Mode first");
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","ReloadRepair");
        var smg=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/SMG.asset");
        var original=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Arsenal/SMGSprite.asset");var t=Object.Instantiate(original.texture);
        for(int y=1;y<=6;y++)for(int x=12;x<=14;x++)t.SetPixel(x,y,Color.clear);
        var body=SpriteAsset("SmgBody",t,new Vector2(original.pivot.x/original.rect.width,original.pivot.y/original.rect.height));
        var mag=new Texture2D(3,7,TextureFormat.RGBA32,false);for(int y=0;y<7;y++)for(int x=0;x<3;x++)mag.SetPixel(x,y,x==0||y==0?new Color32(25,29,43,255):new Color32(112,128,127,255));
        var magazine=SpriteAsset("SmgMagazine",mag,new Vector2(.5f,1));
        var so=new SerializedObject(smg);so.FindProperty("equippedSprite").objectReferenceValue=body;so.FindProperty("icon").objectReferenceValue=original;so.FindProperty("magazineSprite").objectReferenceValue=magazine;so.FindProperty("magazineMountOffset").vector2Value=new Vector2(6.5f/32,2f/32);so.FindProperty("physicalMagazineReload").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(smg);
        var shell=new Texture2D(3,5,TextureFormat.RGBA32,false);for(int y=0;y<5;y++)for(int x=0;x<3;x++)shell.SetPixel(x,y,y==0?new Color32(217,188,90,255):x==0?new Color32(88,55,40,255):new Color32(180,70,45,255));
        var shot=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/Shotgun.asset");so=new SerializedObject(shot);so.FindProperty("loadsIndividually").boolValue=true;so.FindProperty("shellSprite").objectReferenceValue=SpriteAsset("ShotgunShell",shell,Vector2.one*.5f);so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(shot);AssetDatabase.SaveAssets();
        return "SMG existing exchange rig; shotgun six steps over nominal2.6s; original sprites and weapon combat tuning preserved";
    }
}
