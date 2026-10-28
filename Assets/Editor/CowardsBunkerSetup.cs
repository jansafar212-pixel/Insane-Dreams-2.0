using System;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Deliberately drawn coarse modules, following the existing native-asset recipe.
// This edits the current operation in place; it never regenerates encounters.
public static class CowardsBunkerSetup
{
    public const string Folder="Assets/Art/CowardsBunker",Sheet=Folder+"/Environment.asset";
    static readonly Color32 Ink=new Color32(49,43,37,255),Soil=new Color32(106,77,51,255),Earth=new Color32(136,100,61,255),Grass=new Color32(95,112,60,255),Stone=new Color32(103,112,102,255),Wall=new Color32(39,48,52,255),Seam=new Color32(34,42,45,255),Patch=new Color32(48,56,57,255),Wood=new Color32(137,87,52,255),Plank=new Color32(177,128,70,255),Bag=new Color32(156,139,88,255),BagShade=new Color32(110,99,67,255),Tree=new Color32(45,60,53,255);
    sealed class Drawing
    {
        public string name;public int w,h;public Color32[] pixels;
        public Drawing(string n,int width,int height){name=n;w=width;h=height;pixels=new Color32[w*h];}
        public void R(int x,int y,int width,int height,Color32 c){for(int yy=Math.Max(0,y);yy<Math.Min(h,y+height);yy++)for(int xx=Math.Max(0,x);xx<Math.Min(w,x+width);xx++)pixels[yy*w+xx]=c;}
    }
    static List<Drawing> Drawings()
    {
        var all=new List<Drawing>();
        var d=new Drawing("EarthFloor",32,8);d.R(0,0,32,8,Soil);d.R(0,6,32,2,Stone);d.R(0,5,32,1,Ink);d.R(2,1,6,2,Earth);d.R(17,0,8,2,Ink);d.R(23,3,6,1,Earth);d.R(9,4,3,1,Ink);all.Add(d);
        d=new Drawing("WallPanel",32,36);d.R(0,0,32,36,Wall);d.R(0,0,1,36,Seam);d.R(0,11,32,1,Seam);d.R(0,27,32,1,Seam);d.R(19,12,1,15,Seam);d.R(8,0,1,11,Seam);d.R(2,2,5,2,Patch);d.R(23,16,6,4,Patch);d.R(26,15,3,1,Patch);d.R(1,28,2,3,Patch);all.Add(d);
        d=new Drawing("ScuffedDeck",24,4);d.R(0,0,24,4,Seam);d.R(0,1,24,2,Stone);d.R(0,3,24,1,Bag);d.R(4,1,2,1,Seam);d.R(18,2,3,1,Seam);all.Add(d);
        d=new Drawing("EarthBank",40,16);d.R(0,0,40,8,Soil);d.R(2,8,36,3,Soil);d.R(5,11,30,3,Soil);d.R(10,14,17,2,Soil);d.R(2,6,8,2,Earth);d.R(10,10,13,2,Earth);d.R(28,4,9,2,Earth);d.R(0,0,40,2,Ink);d.R(14,3,6,2,Ink);d.R(31,7,4,2,Ink);all.Add(d);
        d=new Drawing("Tuft",12,5);d.R(0,0,12,1,Grass);d.R(1,1,2,2,Grass);d.R(3,1,1,4,Grass);d.R(5,1,2,2,Grass);d.R(8,1,1,3,Grass);d.R(10,1,1,2,Grass);d.R(0,0,12,1,BagShade);all.Add(d);
        d=new Drawing("Rocks",12,5);d.R(0,0,7,2,Ink);d.R(1,2,5,2,Stone);d.R(2,4,3,1,Stone);d.R(1,1,5,1,Stone);d.R(9,0,3,1,Ink);d.R(9,1,2,2,Stone);all.Add(d);
        d=new Drawing("SparseTree",24,38);d.R(10,0,3,28,Tree);d.R(8,13,2,3,Tree);d.R(6,16,3,3,Tree);d.R(5,19,3,7,Tree);d.R(13,20,4,2,Tree);d.R(16,22,2,8,Tree);d.R(5,25,15,8,Tree);d.R(2,28,6,5,Tree);d.R(8,33,10,5,Tree);d.R(19,26,4,6,Tree);all.Add(d);
        d=new Drawing("Scrap",10,4);d.R(0,0,8,1,Ink);d.R(1,1,6,1,Wood);d.R(2,2,5,1,Wood);d.R(8,0,2,2,Stone);all.Add(d);
        d=new Drawing("Sandbags",13,9);d.R(0,0,13,9,Ink);d.R(1,1,5,2,Bag);d.R(7,1,5,2,Bag);d.R(0,4,4,2,Bag);d.R(5,4,6,2,Bag);d.R(12,4,1,2,Bag);d.R(1,7,5,2,Bag);d.R(7,7,5,2,Bag);d.R(2,1,3,1,BagShade);d.R(8,4,2,1,BagShade);d.R(8,7,3,1,BagShade);all.Add(d);
        d=new Drawing("TimberRoof",60,3);d.R(0,0,60,3,Ink);d.R(1,1,58,2,Wood);d.R(1,2,18,1,Plank);d.R(21,2,17,1,Plank);d.R(41,2,17,1,Plank);d.R(18,1,2,1,Ink);d.R(38,1,2,1,Ink);all.Add(d);
        d=new Drawing("TimberPost",3,24);d.R(0,0,3,24,Ink);d.R(1,0,2,24,Wood);d.R(2,2,1,18,Plank);d.R(1,6,1,2,Ink);all.Add(d);
        return all;
    }
    static Dictionary<string,Sprite> Art()
    {
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","CowardsBunker");
        var existing=AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToDictionary(s=>s.name);if(existing.Count>0)return existing;
        var t=new Texture2D(512,512,TextureFormat.RGBA32,false){name="CowardsBunkerEnvironment",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,anisoLevel=0};var pixels=new Color32[512*512];var rects=new List<(Drawing,Rect)>();int x=4,y=4,row=0;
        foreach(var d in Drawings()){int w=d.w*4,h=d.h*4;if(x+w+4>512){x=4;y+=row+8;row=0;}if(y+h+4>512)throw new InvalidOperationException("Sheet overflow");for(int yy=0;yy<h;yy++)for(int xx=0;xx<w;xx++)pixels[(y+yy)*512+x+xx]=d.pixels[yy/4*d.w+xx/4];rects.Add((d,new Rect(x,y,w,h)));x+=w+8;row=Math.Max(row,h);}
        t.SetPixels32(pixels);t.Apply();AssetDatabase.CreateAsset(t,Sheet);
        foreach(var e in rects){var s=Sprite.Create(t,e.Item2,Vector2.one*.5f,32,0,SpriteMeshType.FullRect);s.name=e.Item1.name;AssetDatabase.AddObjectToAsset(s,t);existing.Add(s.name,s);}AssetDatabase.SaveAssetIfDirty(t);return existing;
    }
    static SpriteRenderer Place(Transform root,Dictionary<string,Sprite> art,string name,string drawing,float x,float y,int order=-17)
    {var g=new GameObject(name);g.transform.SetParent(root,false);g.transform.position=new Vector2(x,y);var r=g.AddComponent<SpriteRenderer>();r.sprite=art[drawing];r.sortingOrder=order;return r;}
    static void Edit(Object o,Action<SerializedObject> action){var s=new SerializedObject(o);action(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    public static string Apply()
    {
        if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Stop Play Mode and preserve unsaved work first");
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");var roots=scene.GetRootGameObjects();if(roots.Any(g=>g.name=="CowardsBunker"))return "Already authored; inspect rather than regenerate";
        var art=Art();var terrain=roots.Single(g=>g.name=="FirstOperationTerrain").transform;var scenery=roots.Single(g=>g.name=="FirstOperationScenery").transform;
        foreach(var r in terrain.GetComponentsInChildren<SpriteRenderer>()){if(r.name.EndsWith("Floor"))r.sprite=art["EarthFloor"];else if(r.name.EndsWith("Deck")||r.name.EndsWith("Dais")||r.name=="OfficeFlank"||r.name=="SamuraiStep")r.sprite=art["ScuffedDeck"];}
        foreach(var r in scenery.GetComponentsInChildren<SpriteRenderer>())if(r.name.StartsWith("WallBay_"))r.sprite=art["WallPanel"];
        var details=new GameObject("HandmadeEarthAndVegetation");details.transform.SetParent(scenery,false);
        foreach(var p in new[]{(-23f,0f),(-4f,0f),(12f,0f),(40f,0f),(74f,0f),(111f,0f),(133f,0f),(154f,0f)}){Place(details.transform,art,"Tuft","Tuft",p.Item1,p.Item2+.3f);Place(details.transform,art,"Rocks","Rocks",p.Item1+1.75f,p.Item2+.3f);}
        foreach(float x in new[]{-8f,8f,47f,78f,119f,150f})Place(details.transform,art,"SparseTree","SparseTree",x,2.375f,-27);
        foreach(float x in new[]{-20f,16f,51f,81f,117f,144f})Place(details.transform,art,"SoilAtWallFoot","EarthBank",x,.75f,-22);
        foreach(float x in new[]{-1f,36f,71f,109f,130f})Place(details.transform,art,"ScatteredScrap","Scrap",x,.1875f,-14);
        // Existing cover positions and dais; only these two small barriers resize.
        foreach(string name in new[]{"OfficerBarricade","OfficerRetreatCover"}){var t=terrain.Find(name);t.position=new Vector2(t.position.x,2.1625f);var c=t.GetComponent<BoxCollider2D>();c.size=new Vector2(1.625f,1.125f);var r=t.GetComponent<SpriteRenderer>();r.sprite=art["Sandbags"];r.size=c.size;t.GetComponent<ImpactSurface2D>().Configure(ImpactMaterial.Concrete);}
        var bunker=new GameObject("CowardsBunker");
        var roof=Place(bunker.transform,art,"BunkerRoof","TimberRoof",142.75f,4.875f,0);roof.drawMode=SpriteDrawMode.Tiled;roof.size=new Vector2(4.5f,.375f);roof.gameObject.AddComponent<BoxCollider2D>().size=roof.size;roof.gameObject.AddComponent<ImpactSurface2D>().Configure(ImpactMaterial.Wood);
        Place(bunker.transform,art,"RearTimber","TimberPost",144.875f,3.15f,-18);Place(bunker.transform,art,"FrontTimber","TimberPost",140.625f,3.15f,-18);
        Place(bunker.transform,art,"SunkenEarth","EarthBank",141.5f,1.05f,-19);Place(bunker.transform,art,"RoofGrass","Tuft",142.5f,5.35f,-16);Place(bunker.transform,art,"EarthLip","EarthBank",147.5f,.95f,-19);
        var peek=new GameObject("OfficerExposedEdge");peek.transform.SetParent(bunker.transform);peek.transform.position=new Vector2(136,2.3f);
        var officer=roots.SelectMany(g=>g.GetComponentsInChildren<OfficerShotTelegraph>(true)).Single();Edit(officer,s=>{s.FindProperty("bunkerPeek").objectReferenceValue=peek.transform;s.FindProperty("bunkerCover").objectReferenceValue=terrain.Find("OfficerBarricade").GetComponent<Collider2D>();});
        var sign=new GameObject("ManagementShelterSign");sign.transform.SetParent(bunker.transform);sign.transform.position=new Vector2(141,5.9f);var text=sign.AddComponent<TextMesh>();text.text="MANAGEMENT SHELTER\nSTAFF: PLEASE STAND IN FRONT";text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=32;text.characterSize=.11f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color32(166,162,132,255);sign.GetComponent<MeshRenderer>().sortingOrder=-16;
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Demo visuals updated; two sandbag covers, one roof, rear escape and authored Officer peek. No roster/layout/ladder/trigger changes.";
    }
}
