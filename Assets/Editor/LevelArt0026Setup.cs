using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsaneDreams.Visuals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Explicit Editor-only flat-pixel recipes. No runtime art generator or gameplay changes.
public static class LevelArt0026Setup
{
    public const string Art="Assets/Art/LevelArt0026";
    public static readonly string[] Scenes={"Assets/Scenes/Warehouse_Demo.unity","Assets/Scenes/Warehouse_Testbed.unity","Assets/Scenes/Warehouse_Mission01.unity"};
    static readonly Color32 Ink=new Color32(34,31,35,255),Wood=new Color32(137,87,52,255),Plank=new Color32(177,128,70,255),Seam=new Color32(88,55,40,255),Steel=new Color32(102,112,109,255),Rust=new Color32(137,92,62,255),Gold=new Color32(185,156,79,255);
    static readonly Color32[] Palette={Ink,Wood,Plank,Seam,Steel,Rust,Gold,new Color32(34,41,46,255),new Color32(40,49,54,255),new Color32(53,63,66,255),new Color32(45,64,72,255),new Color32(73,84,82,255),new Color32(61,66,66,255),new Color32(78,89,89,255),new Color32(115,113,100,255),new Color32(166,162,132,255)};
    static Color32 Nearest(Color32 color)
    {
        if(color.a==0)return color;int best=int.MaxValue;Color32 chosen=Ink;
        foreach(var p in Palette){int r=p.r-color.r,g=p.g-color.g,b=p.b-color.b,d=r*r+g*g+b*b;if(d>=best)continue;best=d;chosen=p;if(d==0)break;}return chosen;
    }
    static Color32 C(byte r,byte g,byte b)=>Nearest(new Color32(r,g,b,255));
    static void ChunkPixels(string name,Color32[] pixels,int width,int height)
    {
        var source=(Color32[])pixels.Clone();
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)
        {
            int column=x/4,row=y/4;Color32 color=source[Math.Min(height-1,row*4+2)*width+Math.Min(width-1,column*4+2)];
            if(name=="Ladder")
            {
                color=column==0||column==7?Ink:column==1||column==6?Gold:row==1?Ink:row==2?Gold:new Color32(0,0,0,0);
            }
            else if(name=="LadderFoot"||name=="LadderTop")color=column==0||column==7?Ink:Gold;
            else if(name.StartsWith("Support_"))color=column==0?C(34,41,46):C(53,63,66);
            else if(name.StartsWith("Pipe_"))color=column==0?C(34,41,46):C(73,84,82);
            pixels[y*width+x]=color;
        }
    }
    public static string ChunkEnvironmentPixels()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before baking environment art.");
        int count=0;foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Art}))
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));if(texture.name.StartsWith("Crate"))continue;
            var pixels=texture.GetPixels32();ChunkPixels(texture.name,pixels,texture.width,texture.height);texture.SetPixels32(pixels);texture.filterMode=FilterMode.Point;texture.anisoLevel=0;texture.Apply();EditorUtility.SetDirty(texture);AssetDatabase.SaveAssetIfDirty(texture);count++;
        }
        return count+" existing non-crate textures baked to a four-native-pixel drawing grid; sprite bounds and approved crates unchanged.";
    }
    public static void NormalizeSharedPalette()
    {
        foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Art}))
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));var pixels=texture.GetPixels32();for(int i=0;i<pixels.Length;i++)pixels[i]=Nearest(pixels[i]);texture.SetPixels32(pixels);texture.Apply();EditorUtility.SetDirty(texture);AssetDatabase.SaveAssetIfDirty(texture);
        }
        var scene=EditorSceneManager.OpenScene(Scenes[0],OpenSceneMode.Additive);try{CaptureViews(scene,"After");}finally{EditorSceneManager.CloseScene(scene,true);}
    }
    static int Stable(string name){int value=0;foreach(char c in name)value=(value*31+c)&0x7fffffff;return value;}
    static int CrateKind(string name)=>name=="Surface_Crate_-18"?0:name=="Surface_Crate_-3"?1:name=="Surface_Crate_21"?2:Stable(name)%4;
    static string PathOf(Transform t)=>t==null?"":PathOf(t.parent)+"/"+t.name+"["+t.GetSiblingIndex()+"]";
    public static Dictionary<string,string> GameplaySnapshot(Scene scene)
    {
        var result=new Dictionary<string,string>();
        foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))
        {
            string key=PathOf(t);result[key+"/Transform"]=JsonConvert.SerializeObject(new {p=new[]{t.localPosition.x,t.localPosition.y,t.localPosition.z},q=new[]{t.localRotation.x,t.localRotation.y,t.localRotation.z,t.localRotation.w},s=new[]{t.localScale.x,t.localScale.y,t.localScale.z},t.gameObject.layer,t.gameObject.activeSelf,t.gameObject.name});
            var components=t.GetComponents<Component>();
            for(int i=0;i<components.Length;i++)
            {
                var c=components[i];if(c==null){result[key+"/MissingScript"]=i.ToString();continue;}if(c is Transform||c is SpriteRenderer)continue;
                string json=EditorJsonUtility.ToJson(c);
                // Legacy art snapshots predate the opt-in Operation mode. Normalize
                // only its behavior-preserving defaults; non-default values remain
                // visible to the snapshot guard and have dedicated mission checks.
                if(c is InsaneDreams.Combat.PlayerHealthRecovery||c is InsaneDreams.Missions.WarehouseMission)
                {
                    var state=JObject.Parse(json);var data=state["MonoBehaviour"] as JObject??state;
                    if(c is InsaneDreams.Combat.PlayerHealthRecovery&&data["allowCheckpointRespawn"]?.Value<bool>()==true)data.Remove("allowCheckpointRespawn");
                    if(c is InsaneDreams.Missions.WarehouseMission&&data["firstOperation"]?.Value<bool>()==false)data.Remove("firstOperation");
                    json=state.ToString(Formatting.None);
                }
                result[key+"/"+c.GetType().FullName+"/"+i]=json;
            }
        }
        return result;
    }
    public static void CaptureBaseline()
    {
        Directory.CreateDirectory("TestResults");
        foreach(string path in Scenes)
        {
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try{File.WriteAllText("TestResults/LevelArt0026_"+scene.name+"_GameplayBefore.json",JsonConvert.SerializeObject(GameplaySnapshot(scene),Formatting.Indented));if(scene.name=="Warehouse_Demo")CaptureViews(scene,"Before");}
            finally{EditorSceneManager.CloseScene(scene,true);}
        }
    }
    public static string[] GameplayDifferences(Scene scene)
    {
        var before=JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText("TestResults/LevelArt0026_"+scene.name+"_GameplayBefore.json"));var actual=GameplaySnapshot(scene);
        return before.Where(e=>!actual.TryGetValue(e.Key,out string value)||value!=e.Value).Select(e=>e.Key).ToArray();
    }
    static bool rewriteExisting;
    static Sprite Pixel(string name,int width,int height,Func<int,int,Color32> pixel)
    {
        string path=Art+"/"+name+".asset";
        var existing=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(existing!=null&&!rewriteExisting)return existing;
        var texture=existing!=null?existing.texture:new Texture2D(width,height,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,anisoLevel=0};
        var pixels=new Color32[width*height];for(int y=0;y<height;y++)for(int x=0;x<width;x++)pixels[y*width+x]=pixel(x,y);
        if(!name.StartsWith("Crate"))ChunkPixels(name,pixels,width,height);
        texture.SetPixels32(pixels);texture.Apply();if(existing!=null){EditorUtility.SetDirty(texture);AssetDatabase.SaveAssetIfDirty(texture);return existing;}AssetDatabase.CreateAsset(texture,path);
        var sprite=Sprite.Create(texture,new Rect(0,0,width,height),Vector2.one*.5f,32,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,texture);AssetDatabase.SaveAssetIfDirty(texture);return sprite;
    }
    static Sprite Crate(int variant,int w,int h)
    {
        string[] names={"CrateWoodBrace","CrateWoodPlanks","CrateTinCorners","CrateRustPanel"};
        return Pixel(names[variant]+"_"+w+"x"+h,w,h,(x,y)=>{
            int px=x/2*2,py=y/2*2;
            if(x<2||y<2||x>=w-2||y>=h-2)return Ink;
            if(variant<2)
            {
                if(x<5||x>=w-5||y<5||y>=h-5)return Plank;
                int seam=variant==0?px:py;if(seam%(variant==0?10:8)<2)return Seam;
                bool brace=Math.Abs(px*(h-10)-py*(w-10))<(Math.Max(w,h)-10)*2;
                if(variant==0&&brace||variant==1&&Math.Abs((w-px-2)*(h-10)-py*(w-10))<Math.Max(w,h)*1.5f)return Plank;
                if((x<8||x>=w-8)&&(y<8||y>=h-8))return Ink;
                return Wood;
            }
            bool corner=(x<8||x>=w-8)&&(y<8||y>=h-8);
            if(corner)return (x==4||x==5||x==w-6||x==w-5)&&(y==4||y==5||y==h-6||y==h-5)?Gold:Ink;
            if(variant==2){if(x==w/2||x==w/2+1)return Ink;if(y>=h-9&&y<h-6&&x>8&&x<w-8)return Gold;return Steel;}
            if(x<5||x>=w-5||y<5||y>=h-5)return Rust;
            if(py==h/2/2*2&&x>7&&x<w-7)return Ink;
            return C(115,113,100);
        });
    }
    static Sprite Surface(string kind,int w,int h)
    {
        return Pixel(kind+"_"+w+"x"+h,w,h,(x,y)=>{
            if(kind=="Walkway")return y>=h-4?Gold:y<3||x<3||x>=w-3?Ink:C(78,89,89);
            if(kind=="Concrete")return y>=h-4?C(166,162,132):y<3||x<3||x>=w-3?Ink:Steel;
            if(kind=="Boundary")return x<3||x>=w-3||y<3||y>=h-3?Ink:C(78,89,89);
            if(kind=="Hazard")return y<2||y>=h-2?Ink:((x/4+y/4)/3)%2==0?Gold:Ink;
            return Ink;
        });
    }
    static Sprite Decor(string kind,int w,int h)
    {
        return Pixel(kind+"_"+w+"x"+h,w,h,(x,y)=>{
            if(kind=="Masonry")return C(40,49,54);
            if(kind=="Window"){
                if(x<4||x>=w-4||y<4||y>=h-4)return C(34,41,46);
                if(x<8||x>=w-8||y<8||y>=h-8||x>=w/3&&x<w/3+6)return C(73,84,82);
                return C(45,64,72);
            }
            if(kind=="Support")return x<2||x>=w-2?C(34,41,46):C(53,63,66);
            if(kind=="Pipe")return x<2||x>=w-2?C(34,41,46):C(73,84,82);
            if(kind=="Vent")return x<4||x>=w-4||y<4||y>=h-4||y>=8&&y<14||y>=22&&y<28||y>=36&&y<42?C(34,41,46):C(73,84,82);
            if(kind=="Door"||kind=="ExitDoor")return x<4||x>=w-4||y<4||y>=h-4?Ink:y>=h/4&&y<h/4+4||y>=h/2&&y<h/2+4||y>=h*3/4&&y<h*3/4+4?C(34,41,46):C(73,84,82);
            return Ink;
        });
    }
    static Sprite Ladder(string kind)
    {
        int h=kind=="Ladder"?16:4;
        return Pixel(kind,32,h,(x,y)=>{
            bool rail=x<6||x>=26;if(rail)return x<2||x>=30?Ink:Gold;
            if(kind=="Ladder")return y>=6&&y<12?(y<8||y>=10?Ink:Gold):new Color32(0,0,0,0);
            return y<2?Ink:Gold;
        });
    }
    public static string SimplifyNonCrateAssets()
    {
        int changed=0;rewriteExisting=true;
        try{
            foreach(string path in AssetDatabase.FindAssets("t:Texture2D",new[]{Art}).Select(AssetDatabase.GUIDToAssetPath)){
                string name=Path.GetFileNameWithoutExtension(path);if(name.StartsWith("Crate"))continue;
                if(name=="Ladder"||name=="LadderFoot"||name=="LadderTop"){Ladder(name);changed++;continue;}
                int split=name.LastIndexOf('_');string kind=name.Substring(0,split);string[] size=name.Substring(split+1).Split('x');int w=int.Parse(size[0]),h=int.Parse(size[1]);
                if(kind=="Walkway"||kind=="Concrete"||kind=="Boundary"||kind=="Hazard")Surface(kind,w,h);else Decor(kind,w,h);changed++;
            }
        }finally{rewriteExisting=false;}
        foreach(string path in Scenes){
            var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try{
                foreach(var r in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true))){
                    if(!AssetDatabase.GetAssetPath(r.sprite).StartsWith(Art+"/Window_"))continue;
                    r.enabled=new[]{"Window_-18","Window_0","Window_18","WindowUpper_-24","WindowUpper_-6","WindowUpper_12"}.Contains(r.name);EditorUtility.SetDirty(r);
                }
                var before=JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText("TestResults/LevelArt00261_"+scene.name+"_GameplayBefore.json"));var after=GameplaySnapshot(scene);
                foreach(var entry in before)if(!after.TryGetValue(entry.Key,out string value)||value!=entry.Value)throw new InvalidOperationException("Protected scene change: "+entry.Key);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                File.WriteAllText("TestResults/LevelArt00261_"+scene.name+"_GameplayAfter.json",JsonConvert.SerializeObject(after,Formatting.Indented));
            }finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        return changed+" existing non-crate textures simplified; no new textures; crate assets skipped.";
    }
    static int Pixels(float size)=>Math.Max(1,Mathf.CeilToInt(size*32-.0001f));
    static void Assign(SpriteRenderer source,Sprite sprite,List<object> changes)
    {
        Bounds before=source.bounds;SpriteRenderer target=source;var scale=source.transform.lossyScale;
        if(source.GetComponent<Collider2D>()!=null||Mathf.Abs(scale.x-1)>.0001f||Mathf.Abs(scale.y-1)>.0001f)
        {
            // Counter-scale only a new visual child; collider-bearing transforms stay byte-for-byte equivalent.
            var child=new GameObject("LevelArt0026_Visual");child.layer=source.gameObject.layer;child.transform.SetParent(source.transform,false);child.transform.position=before.center;
            child.transform.localScale=new Vector3(1/scale.x,1/scale.y,1);target=child.AddComponent<SpriteRenderer>();target.sharedMaterial=source.sharedMaterial;target.sortingLayerID=source.sortingLayerID;target.sortingOrder=source.sortingOrder;source.enabled=false;
        }
        target.sprite=sprite;target.color=Color.white;target.drawMode=SpriteDrawMode.Tiled;target.size=before.size;target.tileMode=SpriteTileMode.Continuous;
        EditorUtility.SetDirty(target);var after=target.bounds;
        if((before.center-after.center).sqrMagnitude>.00001f||(before.size-after.size).sqrMagnitude>.00001f)throw new InvalidOperationException("Visual bounds changed: "+source.name);
        PrefabUtility.RecordPrefabInstancePropertyModifications(source);PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        changes.Add(new {name=PathOf(source.transform),sprite=AssetDatabase.GetAssetPath(sprite),size=new[]{before.size.x,before.size.y},ppu=sprite.pixelsPerUnit});
    }
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
        if(!AssetDatabase.IsValidFolder(Art))AssetDatabase.CreateFolder("Assets/Art","LevelArt0026");
        var changes=new List<object>();
        foreach(string path in Scenes)
        {
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                var original=JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText("TestResults/LevelArt0026_"+scene.name+"_GameplayBefore.json"));
                var renderers=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
                foreach(var r in renderers)
                {
                    if(!r.enabled||!r.gameObject.activeInHierarchy||r.sprite==null)continue;
                    if(r.name.StartsWith("CrateBand_")&&r.transform.parent?.name=="WarehouseProps"){r.enabled=false;continue;}
                    string old=AssetDatabase.GetAssetPath(r.sprite);int w=Pixels(r.bounds.size.x),h=Pixels(r.bounds.size.y);Sprite replacement=null;
                    if(old=="Assets/Art/Sprint008/ShippingCrate.png"||r.name.StartsWith("DemoCover_"))replacement=Crate(CrateKind(r.name),w,h);
                    else if(old=="Assets/Art/Sprint008/MetalWalkway.png")replacement=Surface("Walkway",w,h);
                    else if(old=="Assets/Art/Sprint008/Concrete.png")replacement=Surface(r.name.Contains("Boundary")?"Boundary":"Concrete",w,h);
                    else if(old=="Assets/Art/Sprint008/HazardBeam.png")replacement=Surface("Hazard",w,h);
                    else if(old=="Assets/Art/Sprint008/ConcreteWall.png")replacement=Decor("Masonry",128,144);
                    else if(old=="Assets/Art/Sprint008/FactoryWindow.png")replacement=Decor("Window",w,h);
                    else if(old=="Assets/Art/Sprint008/SupportBeam.png")replacement=Decor("Support",w,h);
                    else if(old=="Assets/Art/Sprint008/LoadingDoor.png")replacement=Decor("Door",w,h);
                    else if(old=="Assets/Art/Sprint008/Vent.png")replacement=Decor("Vent",w,h);
                    else if(old=="Assets/Art/Sprint008/Pipe.png")replacement=Decor("Pipe",w,h);
                    else if(old=="Assets/Art/Sprint008/Ladder.png")replacement=Ladder("Ladder");
                    else if(old=="Assets/Art/Sprint009/LadderFoot.png")replacement=Ladder("LadderFoot");
                    else if(old=="Assets/Art/Sprint009/LadderHandrail.png")replacement=Ladder("LadderTop");
                    else if(r.name=="MissionExitDoor")replacement=Decor("ExitDoor",w,h);
                    if(replacement!=null)Assign(r,replacement,changes);
                }
                var after=GameplaySnapshot(scene);foreach(var entry in original)if(!after.TryGetValue(entry.Key,out string value)||value!=entry.Value)throw new InvalidOperationException("Protected scene data changed: "+scene.name+entry.Key);
                File.WriteAllText("TestResults/LevelArt0026_"+scene.name+"_GameplayAfter.json",JsonConvert.SerializeObject(after,Formatting.Indented));
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                if(scene.name=="Warehouse_Demo")CaptureViews(scene,"After");
            }
            finally{EditorSceneManager.CloseScene(scene,true);}
        }
        File.WriteAllText("TestResults/LevelArt0026_Assignments.json",JsonConvert.SerializeObject(changes,Formatting.Indented));return "Assigned "+changes.Count+" environment renderers across three warehouse scenes; original nonvisual data and transforms preserved.";
    }
    static void CaptureViews(Scene scene,string phase)
    {
        var source=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Camera>(true)).First();
        Vector3[] views={new Vector3(-14,2.3f,-10),new Vector3(1,7,-10),new Vector3(18,11,-10)};
        for(int i=0;i<views.Length;i++)
        {
            var g=new GameObject("LevelArt0026_Capture");SceneManager.MoveGameObjectToScene(g,scene);var camera=g.AddComponent<Camera>();camera.CopyFrom(source);camera.enabled=false;camera.scene=scene;camera.transform.position=views[i];
            var previous=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGBA32,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("TestResults/LevelArt0026_"+phase+"_"+i+".png",image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(rt);Object.DestroyImmediate(image);Object.DestroyImmediate(g);}
        }
    }
}
