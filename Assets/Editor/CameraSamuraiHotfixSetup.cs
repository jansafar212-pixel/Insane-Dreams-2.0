using UnityEditor;
using UnityEngine;
using InsaneDreams.Visuals;
public static class CameraSamuraiHotfixSetup
{
    private const string Art="Assets/Art/CameraSamuraiHotfix/";
    private static readonly Color Peach=new Color32(255,214,165,255),Brown=new Color32(80,43,12,255),Yellow=new Color32(247,201,44,255),Orange=new Color32(226,103,30,255);
    private static Sprite Draw(string name,int w,int h,Vector2 pivot,System.Func<int,int,Color> pixel)
    {
        var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};for(int y=0;y<h;y++)for(int x=0;x<w;x++)t.SetPixel(x,y,pixel(x,y));t.Apply();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+"Pixels.asset");if(texture==null){texture=t;AssetDatabase.CreateAsset(t,Art+name+"Pixels.asset");}else{EditorUtility.CopySerialized(t,texture);Object.DestroyImmediate(t);}
        var s=Sprite.Create(texture,new Rect(0,0,w,h),pivot,32,0,SpriteMeshType.FullRect);s.name=name;var old=AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".asset");if(old==null){AssetDatabase.CreateAsset(s,Art+name+".asset");return s;}EditorUtility.CopySerialized(s,old);Object.DestroyImmediate(s);return old;
    }
    public static void RefinePlayer()
    {
        if(!AssetDatabase.IsValidFolder(Art.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Art","CameraSamuraiHotfix");
        var head=Draw("WorkerCapRefined",12,11,new Vector2(.5f,.5f),(x,y)=>
        {if(y==10)return x>=2&&x<=7?Yellow:Color.clear;if(y==9)return x>=1&&x<=8?Yellow:Color.clear;if(y==8)return x<=10?Yellow:Brown;if(y>=5&&(x==0||x==11)||y==7&&x==1)return Brown;return Peach;});
        var torso=Draw("WorkerShirtRefined",12,17,new Vector2(.5f,.5f),(x,y)=>
        {if(y==16&&(x==0||x==11))return Color.clear;if(y>=14&&x>=5&&x<=7)return Peach;if(y==2&&x>=1&&x<=10||x==6&&y>=3&&y<=9)return Brown;return Orange;});
        var arm=Draw("WorkerSleeveRefined",5,18,new Vector2(.5f,1),(x,y)=>
        {if(y==17&&x==4||y==0&&(x==0||x==4))return Color.clear;return y>=9?Orange:Peach;});
        var leg=Draw("WorkerTrousersRefined",6,18,new Vector2(.5f,1),(x,y)=>
        {if(y>=3&&(x==0||x==5)||y==0&&x==5)return Color.clear;return y<3?Brown:new Color32(34,66,220,255);});
        var previous=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/Hero0247.asset");const string skinPath="Assets/Data/Visuals/HeroRefined.asset";
        var skin=AssetDatabase.LoadAssetAtPath<CharacterSkin>(skinPath);if(skin==null){skin=Object.Instantiate(previous);skin.name="HeroRefined";AssetDatabase.CreateAsset(skin,skinPath);}
        skin.head=head;skin.torso=torso;skin.arm=arm;skin.leg=leg;skin.weapon=AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset").EquippedSprite;
        skin.provenance="Focused player refinement: existing worker cap/orange shirt, flat native pixels; shorter neckline, coherent shoulders/sleeves/hand corners and separate trouser silhouettes; unchanged 32PPU module bounds/pivots/equipment. Awaiting artistic playtest review.";EditorUtility.SetDirty(skin);
        foreach(var path in new[]{"Assets/Prefabs/Player/WarehousePlayer.prefab","Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab"})
        {var g=PrefabUtility.LoadPrefabContents(path);try{var r=g.GetComponentInChildren<CutoutRig>(true);r.ApplySkin(skin);r.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);PrefabUtility.SaveAsPrefabAsset(g,path);}finally{PrefabUtility.UnloadPrefabContents(g);}}
        AssetDatabase.SaveAssets();
    }
}
