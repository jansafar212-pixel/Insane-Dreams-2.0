using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Individual small drawings and deliberate placements; never scatter/noise or a runtime decorator.
public static class HandmadeEnvironmentDetailsSetup
{
    public const string Folder="Assets/Art/HandmadeEnvironmentDetails";
    public const string Sheet=Folder+"/Details.asset";
    public const string Root="HandmadeEnvironmentDetails";
    static readonly Color32 Clear=new Color32(0,0,0,0),Ink=new Color32(34,31,35,255),Metal=new Color32(73,84,82,255),Dark=new Color32(34,41,46,255),Gold=new Color32(185,156,79,255),Rust=new Color32(137,92,62,255),Paper=new Color32(166,162,132,255),Wood=new Color32(88,55,40,255);
    sealed class Drawing
    {
        public readonly string name;public readonly int width,height;public readonly Color32[] pixels;
        public Drawing(string name,int width,int height){this.name=name;this.width=width;this.height=height;pixels=new Color32[width*height];}
        public void Dot(int x,int y,Color32 c){if(x>=0&&x<width&&y>=0&&y<height)pixels[y*width+x]=c;}
        public void Rect(int x,int y,int w,int h,Color32 c){for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)Dot(xx,yy,c);}
        public void Line(int x,int y,int endX,int endY,Color32 c){int dx=Math.Abs(endX-x),sx=x<endX?1:-1,dy=-Math.Abs(endY-y),sy=y<endY?1:-1,e=dx+dy;while(true){Dot(x,y,c);if(x==endX&&y==endY)break;int twice=2*e;if(twice>=dy){e+=dy;x+=sx;}if(twice<=dx){e+=dx;y+=sy;}}}
    }
    static readonly Dictionary<char,string> Letters=new Dictionary<char,string>{
        {'N',"101/111/111/101/101"},{'O',"111/101/101/101/111"},{'P',"110/101/110/100/100"},{'E',"111/100/110/100/111"},
        {'F',"111/100/110/100/100"},{'I',"111/010/010/010/111"},{'0',"111/101/101/101/111"},{'2',"111/001/111/100/111"}};
    static void Text(Drawing d,string text,int x,int y,Color32 color){for(int i=0;i<text.Length;i++){var rows=Letters[text[i]].Split('/');for(int row=0;row<5;row++)for(int col=0;col<3;col++)if(rows[row][col]=='1')d.Dot(x+i*4+col,y+4-row,color);}}
    static Drawing Poster(string name,string word,Color32 paper)
    {
        var d=new Drawing(name,20,12);d.Rect(0,0,20,12,Ink);d.Rect(1,1,18,10,paper);d.Dot(0,11,Clear);d.Dot(19,0,Clear);d.Rect(3,2,6,1,Rust);Text(d,word,2,4,Ink);return d;
    }
    static List<Drawing> Drawings()
    {
        var result=new List<Drawing>{Poster("NopeWarning","NOPE",Gold),Poster("FineWarning","FINE",Paper)};
        var box=new Drawing("ElectricalBox",12,14);box.Rect(1,0,10,14,Ink);box.Rect(2,1,8,12,Metal);box.Line(8,2,8,11,Ink);box.Rect(3,9,3,3,Gold);box.Line(5,11,4,10,Ink);box.Line(4,10,5,9,Ink);box.Rect(3,3,2,3,Dark);box.Dot(9,6,Rust);result.Add(box);
        var pipe=new Drawing("PipeValve",16,20);pipe.Rect(2,0,4,17,Ink);pipe.Rect(3,0,2,16,Metal);pipe.Rect(2,15,12,4,Ink);pipe.Rect(4,16,10,2,Metal);pipe.Rect(1,6,6,5,Ink);pipe.Rect(2,7,4,3,Rust);pipe.Line(3,7,3,9,Ink);pipe.Line(2,8,5,8,Ink);result.Add(pipe);
        var fan=new Drawing("BoxFan",16,16);fan.Rect(0,0,16,16,Ink);fan.Rect(1,1,14,14,Metal);fan.Rect(3,3,10,10,Dark);fan.Rect(5,9,3,4,Rust);fan.Rect(9,6,4,3,Rust);fan.Rect(7,3,3,4,Rust);fan.Rect(3,7,4,3,Rust);fan.Rect(7,7,2,2,Ink);result.Add(fan);
        var shelf=new Drawing("StorageShelf",24,16);shelf.Rect(1,0,2,16,Dark);shelf.Rect(21,0,2,16,Dark);shelf.Rect(0,1,24,2,Metal);shelf.Rect(0,8,24,2,Metal);shelf.Rect(3,3,5,4,Wood);shelf.Rect(10,3,7,3,Rust);shelf.Rect(5,10,6,4,Rust);shelf.Rect(14,10,5,5,Wood);shelf.Line(7,10,7,13,Dark);result.Add(shelf);
        var crack=new Drawing("ConcreteRepair",12,10);crack.Rect(1,1,8,7,Metal);crack.Rect(3,0,7,8,Metal);crack.Rect(7,5,4,4,Metal);crack.Line(4,9,5,6,Dark);crack.Line(5,6,3,4,Dark);crack.Line(3,4,6,1,Dark);crack.Line(4,4,8,4,Dark);result.Add(crack);
        var stain=new Drawing("OldStain",12,8);stain.Rect(1,3,8,4,Wood);stain.Rect(3,1,5,5,Wood);stain.Rect(8,4,3,2,Wood);stain.Line(3,2,3,0,Wood);result.Add(stain);
        var number=new Drawing("Painted02",12,8);Text(number,"02",2,1,Rust);result.Add(number);
        var lamp=new Drawing("WarningLamp",8,6);lamp.Rect(1,0,6,2,Dark);lamp.Rect(2,1,4,4,Ink);lamp.Rect(3,2,2,2,Rust);lamp.Rect(3,4,2,1,Gold);result.Add(lamp);
        var bolts=new Drawing("BoltPlate",8,2);bolts.Rect(0,0,8,1,Dark);bolts.Dot(1,1,Paper);bolts.Dot(6,1,Paper);result.Add(bolts);
        var rust=new Drawing("RustPlate",12,2);rust.Rect(1,0,7,2,Rust);rust.Rect(7,0,4,1,Rust);rust.Dot(3,1,Wood);result.Add(rust);
        var hazard=new Drawing("PaintStripe",12,2);for(int x=0;x<12;x++)for(int y=0;y<2;y++)hazard.Dot(x,y,(x/2+y)%2==0?Gold:Ink);result.Add(hazard);
        var wear=new Drawing("FloorWear",12,4);wear.Line(1,2,4,2,Dark);wear.Line(4,2,6,0,Dark);wear.Line(6,0,10,0,Dark);wear.Rect(7,2,3,1,Rust);result.Add(wear);
        var shadow=new Drawing("HardShadow",1,1);shadow.Dot(0,0,Dark);result.Add(shadow);return result;
    }
    static Dictionary<string,Sprite> Art()
    {
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","HandmadeEnvironmentDetails");
        var existing=AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToDictionary(s=>s.name);if(existing.Count>0)return existing;
        var texture=new Texture2D(256,256,TextureFormat.RGBA32,false){name="HandmadeDetails",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,anisoLevel=0};var pixels=new Color32[256*256];var rects=new List<(Drawing,Rect)>();int x=4,y=4,row=0;
        foreach(var d in Drawings())
        {
            int w=d.width*4,h=d.height*4;if(x+w+4>256){x=4;y+=row+8;row=0;}if(y+h+4>256)throw new InvalidOperationException("Detail sheet overflow");
            for(int yy=0;yy<h;yy++)for(int xx=0;xx<w;xx++)pixels[(y+yy)*256+x+xx]=d.pixels[(yy/4)*d.width+xx/4];rects.Add((d,new Rect(x,y,w,h)));x+=w+8;row=Math.Max(row,h);
        }
        texture.SetPixels32(pixels);texture.Apply();AssetDatabase.CreateAsset(texture,Sheet);
        foreach(var entry in rects){var sprite=Sprite.Create(texture,entry.Item2,Vector2.one*.5f,32,0,SpriteMeshType.FullRect);sprite.name=entry.Item1.name;AssetDatabase.AddObjectToAsset(sprite,texture);existing.Add(sprite.name,sprite);}AssetDatabase.SaveAssetIfDirty(texture);return existing;
    }
    static SpriteRenderer Place(Transform root,Dictionary<string,Sprite> art,string name,string drawing,Vector2 position,int order=-16)
    {
        var g=new GameObject(name);g.transform.SetParent(root,false);g.transform.position=position;var r=g.AddComponent<SpriteRenderer>();r.sprite=art[drawing];r.sortingOrder=order;r.sharedMaterial=AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");return r;
    }
    public static string[] Differences(Scene scene)
    {
        var before=JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText("TestResults/HandmadeDetails_"+scene.name+"_Before.json"));var actual=LevelArt0026Setup.GameplaySnapshot(scene);return before.Where(e=>!actual.TryGetValue(e.Key,out string v)||v!=e.Value).Select(e=>e.Key).ToArray();
    }
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");var art=Art();int total=0;
        foreach(string path in LevelArt0026Setup.Scenes)
        {
            var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                if(scene.GetRootGameObjects().Any(g=>g.name==Root))continue;
                var root=new GameObject(Root);SceneManager.MoveGameObjectToScene(root,scene);
                Place(root.transform,art,"ArrivalWarning","NopeWarning",new Vector2(-21.25f,3.5f));
                Place(root.transform,art,"ArrivalPowerBox","ElectricalBox",new Vector2(-14.75f,2.875f));
                Place(root.transform,art,"LadderPipeValve","PipeValve",new Vector2(-7.25f,2.875f));
                Place(root.transform,art,"StorageRepair","ConcreteRepair",new Vector2(-18.5f,8.375f));
                Place(root.transform,art,"ProcessingFan","BoxFan",new Vector2(-1.75f,3.125f));
                Place(root.transform,art,"ProcessingNumber","Painted02",new Vector2(1.5f,8.75f));
                Place(root.transform,art,"ProcessingLamp","WarningLamp",new Vector2(10.25f,8.75f));
                Place(root.transform,art,"OfficeCabinet","ElectricalBox",new Vector2(15.5f,8.5f));
                Place(root.transform,art,"ExitShelves","StorageShelf",new Vector2(16.5f,2.875f));
                Place(root.transform,art,"ExitStain","OldStain",new Vector2(21,3.125f));
                Place(root.transform,art,"OfficeFineWarning","FineWarning",new Vector2(20.75f,9.25f));
                var surfaces=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).Where(r=>r.name.StartsWith("Surface_")).ToArray();
                foreach(var specification in new[]{("Surface_Mezzanine_Left",-17f,"BoltPlate"),("Surface_Mezzanine_Central",-2f,"RustPlate"),("Surface_Roof_Right",20f,"PaintStripe"),("Surface_Ground_Entry",-20f,"FloorWear")})
                {
                    var surface=surfaces.Single(r=>r.name==specification.Item1);var r=Place(root.transform,art,"SurfaceMark_"+specification.Item1,specification.Item3,new Vector2(specification.Item2,surface.bounds.center.y),1);
                    if(!surface.bounds.Contains(r.bounds.min)||!surface.bounds.Contains(r.bounds.max))throw new InvalidOperationException("Surface mark escapes solid art bounds: "+r.name);
                }
                foreach(string name in new[]{"Surface_Mezzanine_Left","Surface_Mezzanine_Central","Surface_Roof_Right","Surface_UpperStep_02"})
                {
                    var surface=surfaces.Single(r=>r.name==name);var r=Place(root.transform,art,"Shadow_"+name,"HardShadow",new Vector2(surface.bounds.center.x+.125f,surface.bounds.min.y-.125f),-15);r.drawMode=SpriteDrawMode.Tiled;r.size=new Vector2(surface.bounds.size.x,.25f);
                }
                var differences=Differences(scene);if(differences.Length>0)throw new InvalidOperationException("Protected scene changes: "+string.Join(";",differences));
                total+=root.transform.childCount;EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);File.WriteAllText("TestResults/HandmadeDetails_"+scene.name+"_After.json",JsonConvert.SerializeObject(LevelArt0026Setup.GameplaySnapshot(scene),Formatting.Indented));
            }
            finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        return total+" decorative SpriteRenderers added across three warehouses; one shared sheet; no new collision or behavior.";
    }
}
