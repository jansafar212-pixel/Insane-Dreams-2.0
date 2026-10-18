using System.IO;
using System.Linq;
using InsaneDreams.Visuals;
using InsaneDreams.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Independently code-authored pixel foundation. Reads no historical images or artwork.
// Explicit authoring entry point only; no automatic imports, scene or build changes.
public static class Sprint008ArtSetup
{
    private const string Art="Assets/Art/Sprint008";
    private static Color Ink=>new Color32(24,28,43,255);
    private static Color Steel=>new Color32(83,109,122,255);
    private static Color Light=>new Color32(159,188,184,255);
    private static Color Rust=>new Color32(186,96,55,255);
    private static Color Gold=>new Color32(239,184,62,255);
    private static Color Blue=>new Color32(62,133,169,255);
    private static void Folder(string path)
    {if(AssetDatabase.IsValidFolder(path))return;string parent=Path.GetDirectoryName(path).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));}
    private static Sprite Pixel(string name,int w,int h,Vector2 pivot,System.Func<int,int,Color> draw)
    {
        var image=new Texture2D(w,h,TextureFormat.RGBA32,false);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)image.SetPixel(x,y,draw(x,y));image.Apply();
        string path=Art+"/"+name+".png";File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=32;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=pivot;settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static CharacterSkin Skin(bool player)
    {
        string name=player?"Worker":"BucketLarper";int w=player?16:18,h=player?20:24;
        Color suit=player?Rust:new Color32(119,83,146,255),skin=new Color32(186,175,157,255);
        var head=Pixel(name+"_Head",w,h,new Vector2(.5f,.5f),(x,y)=>
        {
            if(x<1||x>=w-1||y<1||y>=h-1)return Color.clear;
            if(x==1||x==w-2||y==1||y==h-2)return Ink;
            if(y>=h-7)return y==h-7?Ink:player?Gold:Steel;
            if(player&&y>=h-10)return x>4&&x<w-3?Blue:Ink;
            if(!player&&x==3&&y>8)return Light;
            return skin;
        });
        var torso=Pixel(name+"_Torso",12,14,new Vector2(.5f,.5f),(x,y)=>
        {if(x==0||x==11||y==0||y==13)return Ink;if(player&&(y==8||y==9))return Gold;if(!player&&(x+y)%9==0)return Blue;if(x==5)return Ink;return suit;});
        var arm=Pixel(name+"_Arm",5,14,new Vector2(.5f,1),(x,y)=>x==0||x==4||y==0?Ink:y<4?skin:y==7?Light:suit);
        var leg=Pixel(name+"_Leg",6,15,new Vector2(.5f,1),(x,y)=>x==0||x==5||y<3?Ink:y==7?Steel:player?new Color32(46,76,105,255):new Color32(60,67,75,255));
        var faces=new Sprite[6];
        for(int f=0;f<6;f++)
        {
            int expression=f;
            faces[f]=Pixel(name+"_Face_"+(FaceExpression)f,w,h,new Vector2(.5f,.5f),(x,y)=>
            {
                int eyeY=player?10:13,left=player?4:4,right=w-5;
                bool eyes=(x==left||x==right)&&y>=eyeY-1&&y<=eyeY;
                bool mouth=y==4&&x>=5&&x<=w-5;
                if(expression==1){eyes=(x>=left&&x<=left+1||x>=right&&x<=right+1)&&y==eyeY-1;mouth=y==4&&x>=5&&x<=w-4||x==w-4&&y==5;}
                if(expression==2)eyes=eyes||y==eyeY+2&&(x==left-1||x==right+1)||y==eyeY+1&&(x==left||x==right);
                if(expression==3||expression==4){eyes=(x>=left-1&&x<=left+1||x>=right-1&&x<=right+1)&&y>=eyeY-1&&y<=eyeY+1;mouth=x>=w/2-1&&x<=w/2+1&&y>=3&&y<=6;}
                if(expression==4)eyes=eyes||y==eyeY+4&&(x==left||x==right);
                if(expression==5){eyes=(Mathf.Abs(x-left)<=1&&Mathf.Abs(y-eyeY)<=1&&(Mathf.Abs(x-left)==Mathf.Abs(y-eyeY)))||(Mathf.Abs(x-right)<=1&&Mathf.Abs(y-eyeY)<=1&&Mathf.Abs(x-right)==Mathf.Abs(y-eyeY));mouth=y==4&&(x==5||x==7||x==9||x==11);}
                return eyes||mouth?Ink:Color.clear;
            });
        }
        var weapon=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/ScrapCarbine.png");var chute=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/PatchworkCanopy.png");
        string path="Assets/Data/Visuals/Sprint008"+name+"Skin.asset";var result=AssetDatabase.LoadAssetAtPath<CharacterSkin>(path);
        if(result==null){result=ScriptableObject.CreateInstance<CharacterSkin>();AssetDatabase.CreateAsset(result,path);}
        result.head=head;result.torso=torso;result.arm=arm;result.leg=leg;result.faces=faces;result.weapon=weapon;result.parachute=chute;result.clothing=Color.white;
        result.provenance="Sprint 008 code-authored original pixel foundation; no historical image input, tracing or sampling. Candidate art requiring human style/provenance review, not a legal-clearance assertion. See ART_MANIFEST.md.";EditorUtility.SetDirty(result);return result;
    }
    private static void Rig(string path,CharacterSkin skin)
    {
        var root=PrefabUtility.LoadPrefabContents(path);try{var rig=root.GetComponent<CutoutRig>();rig.ApplySkin(skin);rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    private static SpriteRenderer Decor(Transform parent,string name,Sprite sprite,Vector2 point,Vector2 size,int order)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=point;
        var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.drawMode=SpriteDrawMode.Tiled;renderer.size=size;renderer.sortingOrder=order;
        renderer.sharedMaterial=AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");return renderer;
    }
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode before authoring.");
        Folder(Art);Folder("Assets/Data/Visuals");
        Pixel("ScrapCarbine",24,8,new Vector2(.15f,.5f),(x,y)=>
        {if(x<2&&y>=2&&y<=5)return Rust;if(y>=3&&y<=6)return y==6?Light:x>=19?Ink:Steel;if(x>=7&&x<=9&&y>=0)return Rust;if(x==13&&y==2)return Gold;return Color.clear;});
        Pixel("PatchworkCanopy",64,36,new Vector2(.5f,0),(x,y)=>
        {int roof=34-Mathf.Abs(x-31)/3;if(y>=22&&y<=roof)return y==22||y==roof?Ink:x/11%2==0?Rust:Gold;if(y<22&&(Mathf.Abs(x-(32-y))<1||Mathf.Abs(x-(32+y))<1))return Light;return Color.clear;});
        var spark=Pixel("ImpactSpark",8,8,new Vector2(.5f,.5f),(x,y)=>x==3||y==3||x==y&&x>1&&x<6?Color.white:Color.clear);
        var concrete=Pixel("Concrete",32,16,new Vector2(.5f,.5f),(x,y)=>y==15?Light:y>=13?Steel:y==0||x==0?Ink:(x*7+y*13)%43==0?new Color32(109,119,119,255):new Color32(66,79,84,255));
        var metal=Pixel("MetalWalkway",32,8,new Vector2(.5f,.5f),(x,y)=>y==7?Light:y==0?Ink:(x+y)%8==0?Gold:(x%8==1&&y==3)?Ink:Steel);
        var hazard=Pixel("HazardBeam",16,8,new Vector2(.5f,.5f),(x,y)=>y==0||y==7?Ink:(x+y)%12<6?Gold:Ink);
        var wall=Pixel("ConcreteWall",64,64,new Vector2(.5f,.5f),(x,y)=>y%16==0||(x+(y/16%2)*32)%64==0?new Color32(27,38,48,255):(x*3+y*11)%83==0?new Color32(47,59,65,255):new Color32(37,48,58,255));
        var beam=Pixel("SupportBeam",8,32,new Vector2(.5f,.5f),(x,y)=>x==0||x==7?Ink:x==1?Steel:(y%12==1&&(x==3||x==4))?Light:new Color32(48,62,72,255));
        var door=Pixel("LoadingDoor",48,80,new Vector2(.5f,.5f),(x,y)=>x<3||x>44||y>76?Ink:y%8==0?Light:x==35&&y>31&&y<40?Gold:new Color32(74,91,104,255));
        var window=Pixel("FactoryWindow",48,32,new Vector2(.5f,.5f),(x,y)=>x<2||x>45||y<2||y>29||x%16<2||y==15?Steel:x+y>40?new Color32(68,93,106,255):new Color32(48,70,87,255));
        var vent=Pixel("Vent",32,24,new Vector2(.5f,.5f),(x,y)=>x==0||x==31||y==0||y==23?Steel:y%4==0?Light:Ink);
        var pipe=Pixel("Pipe",8,32,new Vector2(.5f,.5f),(x,y)=>x==0||x==7?Ink:y%16==0?Rust:x==2?Light:Steel);
        var crate=Pixel("ShippingCrate",32,32,new Vector2(.5f,.5f),(x,y)=>x<2||x>29||y<2||y>29||Mathf.Abs(x-y)<2?Ink:x%8==0?Rust:new Color32(132,104,70,255));
        var ladder=Pixel("Ladder",32,16,new Vector2(.5f,.5f),(x,y)=>x<3||x>28?Steel:x<5||x>26||y>=6&&y<=8?Gold:Color.clear);
        var player=Skin(true);var larper=Skin(false);
        Rig("Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab",player);Rig("Assets/Prefabs/Visuals/LarperCutoutVisual.prefab",larper);
        foreach(var path in new[]{"Assets/Prefabs/Player/WarehousePlayer.prefab","Assets/Prefabs/Enemies/Larper.prefab"})
        {
            var root=PrefabUtility.LoadPrefabContents(path);try
            {
                root.GetComponentInChildren<CutoutRig>(true).ApplySkin(path.Contains("WarehousePlayer")?player:larper);
                var feedback=new SerializedObject(root.GetComponent<CombatFeedback>());feedback.FindProperty("surfaceImpactSprite").objectReferenceValue=spark;feedback.ApplyModifiedPropertiesWithoutUndo();
                if(root.GetComponent<MovementAudioHooks>()==null)root.AddComponent<MovementAudioHooks>();
                foreach(var old in root.GetComponentsInChildren<SpriteRenderer>(true))
                    if(old.name=="Canopy"||old.name=="CordLeft"||old.name=="CordRight")old.enabled=false;
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        var dollRoot=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab");try
        {
            var doll=dollRoot.GetComponent<CartoonRagdoll>();Sprite[] sprites={larper.torso,larper.head,larper.arm,larper.arm,larper.leg,larper.leg};
            for(int i=0;i<6;i++){doll.parts[i].GetComponent<SpriteRenderer>().sprite=sprites[i];var box=doll.parts[i].GetComponent<BoxCollider2D>();box.size=(Vector2)sprites[i].bounds.size*.85f;box.offset=sprites[i].bounds.center;}
            doll.face.sprite=larper.Face(FaceExpression.Stunned);PrefabUtility.SaveAsPrefabAsset(dollRoot,"Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab");
        }finally{PrefabUtility.UnloadPrefabContents(dollRoot);}
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/Warehouse_Testbed.unity")throw new System.InvalidOperationException("Open Warehouse_Testbed first.");
        var previous=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="WarehouseArt_Sprint008");if(previous!=null)Object.DestroyImmediate(previous);
        var artRoot=new GameObject("WarehouseArt_Sprint008");var environment=scene.GetRootGameObjects().First(g=>g.name=="Environment");
        foreach(var c in environment.GetComponentsInChildren<BoxCollider2D>(true))
        {
            if(c.isTrigger)continue;var old=c.GetComponent<SpriteRenderer>();if(old==null)continue;old.enabled=false;
            bool platform=c.name.Contains("Mezzanine")||c.name.Contains("Roof")||c.name.Contains("Step")||c.name.Contains("Bridge");
            var sprite=c.name.Contains("Crate")?crate:c.name=="CrouchPassage"?hazard:platform?metal:concrete;
            Decor(artRoot.transform,"Surface_"+c.name,sprite,c.bounds.center,c.bounds.size,0);
        }
        Decor(artRoot.transform,"DistantMasonry",wall,new Vector2(0,5),new Vector2(54,28),-30);
        for(int x=-24;x<=24;x+=6)
        {
            Decor(artRoot.transform,"Window_"+x,window,new Vector2(x,3.5f),new Vector2(3,2),-23);
            Decor(artRoot.transform,"WindowUpper_"+x,window,new Vector2(x,10),new Vector2(3,2),-23);
            Decor(artRoot.transform,"Beam_"+x,beam,new Vector2(x-2,5),new Vector2(.25f,26),-20);
        }
        Decor(artRoot.transform,"LoadingDoor",door,new Vector2(-24,1.25f),new Vector2(1.5f,2.5f),-18);
        Decor(artRoot.transform,"VentBay",vent,new Vector2(5,2.5f),new Vector2(2,1.5f),-18);
        Decor(artRoot.transform,"PipeBay",pipe,new Vector2(8,2),new Vector2(.25f,4),-17);
        foreach(var l in Object.FindObjectsByType<InsaneDreams.Traversal.LadderVolume>(FindObjectsSortMode.None))
        {
            foreach(var old in l.GetComponentsInChildren<SpriteRenderer>(true))old.enabled=false;
            var box=l.GetComponent<BoxCollider2D>();Decor(artRoot.transform,"Graphic_"+l.name,ladder,box.bounds.center,new Vector2(1,box.bounds.size.y),1);
        }
        // Hide only the obsolete flat backdrop, never its transforms or any gameplay collider.
        foreach(var old in environment.GetComponentsInChildren<SpriteRenderer>(true))if(old.sortingOrder<0)old.enabled=false;
        var stationObject=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="RagdollPractice");
        if(stationObject==null)stationObject=new GameObject("RagdollPractice");
        var station=stationObject.GetComponent<InsaneDreams.Level.RagdollPracticeStation>();if(station==null)station=stationObject.AddComponent<InsaneDreams.Level.RagdollPracticeStation>();
        var ground=stationObject.transform.Find("GroundTarget");if(ground==null){ground=new GameObject("GroundTarget").transform;ground.SetParent(stationObject.transform);}
        var drop=stationObject.transform.Find("DropTarget");if(drop==null){drop=new GameObject("DropTarget").transform;drop.SetParent(stationObject.transform);}
        ground.position=new Vector3(6,.85f);drop.position=new Vector3(8,7.6f);
        var so=new SerializedObject(station);so.FindProperty("larperPrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab");so.FindProperty("groundTarget").objectReferenceValue=ground;so.FindProperty("dropTarget").objectReferenceValue=drop;so.FindProperty("document").objectReferenceValue=Object.FindAnyObjectByType<UnityEngine.UIElements.UIDocument>();so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Saved independent code-authored character skins, 11 reusable warehouse sprites, collider-free warehouse art and opt-in practice station. Historical references preserved.";
    }
}
