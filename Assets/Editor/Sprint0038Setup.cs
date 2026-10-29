using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsaneDreams.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// One-off native asset authoring; no runtime decoration or extra behavior.
public static class Sprint0038Setup
{
    const string Folder="Assets/Art/Sprint0038";
    static Sprite Save(string name,Texture2D t,Vector2 pivot)
    {
        string path=Folder+"/"+name+".asset";
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(existing!=null){Object.DestroyImmediate(t);return existing;}
        t.name=name+"Pixels";t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.Apply();
        AssetDatabase.CreateAsset(t,Folder+"/"+name+"Pixels.asset");
        var s=Sprite.Create(t,new Rect(0,0,t.width,t.height),pivot,32,0,SpriteMeshType.FullRect);s.name=name;AssetDatabase.CreateAsset(s,path);return s;
    }
    public static void Pistol()
    {
        var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset");
        var original=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint0246/LarperPistol.asset");
        var body=Object.Instantiate(original.texture);var mag=new Texture2D(4,4,TextureFormat.RGBA32,false);
        // Extract exactly the existing flush grip: recomposed idle silhouette is identical.
        for(int y=0;y<4;y++)for(int x=0;x<4;x++){mag.SetPixel(x,y,body.GetPixel(x+3,y));body.SetPixel(x+3,y,Color.clear);}
        var so=new SerializedObject(w);
        so.FindProperty("equippedSprite").objectReferenceValue=Save("PistolBody",body,new Vector2(original.pivot.x/original.rect.width,original.pivot.y/original.rect.height));
        so.FindProperty("icon").objectReferenceValue=original;
        so.FindProperty("magazineSprite").objectReferenceValue=Save("PistolMagazine",mag,new Vector2(.5f,1));
        so.FindProperty("magazineMountOffset").vector2Value=new Vector2(1f/32,0);
        so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(w);
    }
    static readonly Dictionary<char,string> Letters=new Dictionary<char,string>{
        {'L',"100/100/100/100/111"},{'A',"010/101/111/101/101"},{'R',"110/101/110/101/101"},{'P',"110/101/110/100/100"},
        {'Z',"111/001/010/100/111"},{'O',"111/101/101/101/111"},{'N',"101/111/111/101/101"},{'E',"111/100/110/100/111"},
        {'W',"101/101/111/111/101"},{'K',"101/101/110/101/101"},{'S',"111/100/111/001/111"},{'U',"101/101/101/101/111"},{'C',"111/100/100/100/111"}};
    static Sprite Tag(string name,string text,Color32 paint,int seed)
    {
        int width=text.Length*4+3,height=9;
        var small=new Color32[width*height];
        for(int i=0;i<text.Length;i++)
        {
            if(text[i]==' ')continue;var rows=Letters[text[i]].Split('/');
            int rise=(i+seed)%3==0?1:0;
            for(int y=0;y<5;y++)for(int x=0;x<3;x++)if(rows[y][x]=='1')small[(2+4-y+rise)*width+2+i*4+x]=paint;
        }
        // Uneven underline and one stray stroke, deliberately drawn rather than noise.
        for(int x=3;x<width-4;x++)if(x%11!=0)small[(x<width/2?0:1)*width+x]=paint;
        var t=new Texture2D(width*2,height*2,TextureFormat.RGBA32,false);
        for(int y=0;y<t.height;y++)for(int x=0;x<t.width;x++)t.SetPixel(x,y,small[(y/2)*width+x/2]);
        return Save(name,t,Vector2.one*.5f);
    }
    static Sprite Skull()
    {
        string[] rows={"..#####.....",".#######....","#########...","##..##..##..","##..##..#...",".###.###....","..######....","..#.#.#.....","...###......",".#.....#....","..##.##.....",".#..#..#...."};
        var t=new Texture2D(24,24,TextureFormat.RGBA32,false);var c=new Color32(137,92,62,255);
        for(int y=0;y<24;y++)for(int x=0;x<24;x++)t.SetPixel(x,y,rows[11-y/2][x/2]=='#'?c:new Color32());
        return Save("BadSkull",t,Vector2.one*.5f);
    }
    public static string Apply(bool preservedUnsavedWork=false)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty&&!preservedUnsavedWork)throw new InvalidOperationException("Preserve unsaved work first");
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","Sprint0038");
        Pistol();var tags=new[]{Tag("LarpZone","LARP ZONE",new Color32(137,92,62,255),0),Tag("WorkSucks","WORK SUCKS",new Color32(166,162,132,255),1),Skull()};
        var positions=new[]{new Vector2(-17,2.2f),new Vector2(13,4.5f),new Vector2(45,1.9f),new Vector2(83,5.1f),new Vector2(130,2.1f)};
        int count=0;
        foreach(string path in new[]{"Assets/Scenes/Warehouse_Demo.unity"})
        {
            var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                var before=LevelArt0026Setup.GameplaySnapshot(scene);
                if(scene.GetRootGameObjects().Any(g=>g.name=="FactoryGraffiti0038"))continue;
                var root=new GameObject("FactoryGraffiti0038");SceneManager.MoveGameObjectToScene(root,scene);
                for(int i=0;i<positions.Length;i++)
                {
                    var g=new GameObject(tags[i%3].name);g.transform.SetParent(root.transform,false);g.transform.position=positions[i];
                    var r=g.AddComponent<SpriteRenderer>();r.sprite=tags[i%3];r.sortingOrder=-25;r.sharedMaterial=AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");count++;
                }
                var after=LevelArt0026Setup.GameplaySnapshot(scene);
                if(before.Any(p=>!after.TryGetValue(p.Key,out string v)||p.Value!=v))throw new InvalidOperationException("Protected scene data changed");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                File.WriteAllText("TestResults/Sprint0038/"+scene.name+"Audit.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{protectedEntries=before.Count,unchanged=true,graffiti=5}));
            }
            finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        AssetDatabase.SaveAssets();return count+" static background marks; flush pistol extracted, no balance changes";
    }
}
