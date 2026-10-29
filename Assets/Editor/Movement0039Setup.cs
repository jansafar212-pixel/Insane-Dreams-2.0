using InsaneDreams.Input;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor authoring only. Retains the old canopy and uses the existing skin/rig renderer.
public static class Movement0039Setup
{
    const string Folder="Assets/Art/Movement0039";
    const string SpritePath=Folder+"/ConnectedCanopy.asset";
    static Sprite Canopy()
    {
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);if(existing!=null)return existing;
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","Movement0039");
        var t=new Texture2D(64,44,TextureFormat.RGBA32,false){name="ConnectedCanopyPixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        var clear=new Color32(0,0,0,0);var ink=new Color32(24,28,43,255);var rust=new Color32(186,96,55,255);var gold=new Color32(239,184,62,255);var rope=new Color32(159,188,184,255);
        for(int y=0;y<44;y++)for(int x=0;x<64;x++)
        {
            Color32 color=clear;
            // Two rigid pixel-stepped cords end at the two existing shoulders.
            if(y<=30)
            {
                int left=26-y/2,right=37+y/2;
                if(x>=left&&x<left+2||x>=right&&x<right+2)color=rope;
            }
            int roof=42-Mathf.Abs(x-31)/3,bottom=28+(x/10%2)*2;
            if(x>=2&&x<62&&y>=bottom&&y<=roof)
                color=y>=roof-1||y<=bottom+1||x==2||x==61?ink:x/10%2==0?rust:gold;
            if(y<4&&(x>=25&&x<=28||x>=36&&x<=39))color=ink;
            t.SetPixel(x,y,color);
        }
        t.Apply();AssetDatabase.CreateAsset(t,Folder+"/ConnectedCanopyPixels.asset");
        var s=Sprite.Create(t,new Rect(0,0,64,44),new Vector2(.5f,0),32,0,SpriteMeshType.FullRect);s.name="ConnectedCanopy";AssetDatabase.CreateAsset(s,SpritePath);return s;
    }
    static void Connect(CutoutRig rig,Sprite canopy)
    {
        rig.skin.parachute=canopy;EditorUtility.SetDirty(rig.skin);rig.parachute.sprite=canopy;
        float shoulderY=new SerializedObject(rig).FindProperty("torsoCenterY").floatValue+rig.skin.torso.bounds.extents.y-.0625f;
        rig.parachute.transform.localPosition=new Vector3(0,shoulderY,rig.parachute.transform.localPosition.z);
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Stop Play Mode before authoring");
        var canopy=Canopy();const string path="Assets/Prefabs/Player/WarehousePlayer.prefab";
        var root=PrefabUtility.LoadPrefabContents(path);
        try{Connect(root.GetComponentInChildren<CutoutRig>(true),canopy);PrefabUtility.SaveAsPrefabAsset(root,path);}
        finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();return "Connected 64x44 Point/32PPU canopy; same single SpriteRenderer, no runtime component";
    }
    public static void ApplyScene(Scene scene)
    {
        var canopy=Canopy();foreach(var root in scene.GetRootGameObjects())foreach(var player in root.GetComponentsInChildren<PlayerActions>(true))
        {var rig=player.GetComponentInChildren<CutoutRig>(true);if(rig!=null)Connect(rig,canopy);}
    }
}
