using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEngine;
public static class Sprint0246Setup
{
    private static Sprite Draw(string name,int w,int h,Vector2 pivot,System.Func<int,int,Color> pixel)
    {
        const string folder="Assets/Art/Sprint0246";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","Sprint0246");
        string path=folder+"/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing!=null)return existing;
        var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)t.SetPixel(x,y,pixel(x,y));t.Apply();AssetDatabase.CreateAsset(t,folder+"/"+name+"Pixels.asset");
        var s=Sprite.Create(t,new Rect(0,0,w,h),pivot,32,0,SpriteMeshType.FullRect);s.name=name;AssetDatabase.CreateAsset(s,path);return s;
    }
    public static void Apply()
    {
        Color ink=new Color32(25,29,43,255),steel=new Color32(60,67,75,255),edge=new Color32(111,123,130,255);
        var gun=Draw("LarperPistol",20,8,new Vector2(.2f,.5f),(x,y)=>
        {if(y>=4&&y<=6&&x>=1&&x<=19)return y==6&&x<18?edge:x==19||x==1?ink:steel;if(x>=3&&x<=6&&y<4)return x==3||y==0?ink:steel;if(y==3&&x>=7&&x<=9)return ink;return Color.clear;});
        var mag=Draw("ExtendedMagazine",5,13,new Vector2(.5f,1),(x,y)=>x==0||x==4||y==0?ink:y==7?edge:steel);
        var def=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");var data=new SerializedObject(def);
        data.FindProperty("equippedSprite").objectReferenceValue=gun;data.FindProperty("magazineSprite").objectReferenceValue=mag;data.FindProperty("magazineMountOffset").vector2Value=new Vector2(1f/32,-2f/32);data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(def);
        const string prefab="Assets/Prefabs/Enemies/Larper.prefab";var root=PrefabUtility.LoadPrefabContents(prefab);
        try{var r=root.GetComponentInChildren<CutoutRig>(true);r.ApplySkin(r.skin);r.weapon.sprite=gun;r.SetMagazine(mag,def.MagazineMountOffset);r.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);PrefabUtility.SaveAsPrefabAsset(root,prefab);}finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();CampaignEntry.UseMainMenuEntry();
    }
}
