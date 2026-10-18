using System.IO;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEngine;

// Explicitly invoked authoring utility. Never runs automatically or enters a player build.
public static class Sprint007VisualSetup
{
    private const string Art = "Assets/Art/Sprint007";
    private const string Prefabs = "Assets/Prefabs/Visuals";
    private static Color Ink => new Color32(26, 22, 30, 255);
    private static Color Skin => new Color32(255, 207, 148, 255);
    private static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        Folder(Path.GetDirectoryName(path).Replace('\\', '/'));
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }
    private static Sprite Make(string name, int w, int h, Vector2 pivot, System.Func<int,int,Color> paint)
    {
        var image = new Texture2D(w,h,TextureFormat.RGBA32,false);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)image.SetPixel(x,y,paint(x,y));
        image.Apply();string path=Art+"/"+name+".png";File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=32;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=pivot;settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
        importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static T Asset<T>(string path) where T:ScriptableObject
    { var result=AssetDatabase.LoadAssetAtPath<T>(path);if(result==null){result=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(result,path);}return result; }
    private static Transform Part(Transform parent,string name,Sprite sprite,int order)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;
        renderer.sharedMaterial=AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");return go.transform;
    }
    private static GameObject RigPrefab(string name,CharacterSkin skin)
    {
        var root=new GameObject(name);var rig=root.AddComponent<CutoutRig>();
        rig.torso=Part(root.transform,"Torso",skin.torso,10);rig.head=Part(root.transform,"Head",skin.head,12);
        rig.leftArm=Part(root.transform,"LeftArm",skin.arm,9);rig.rightArm=Part(root.transform,"RightArm",skin.arm,14);
        rig.leftLeg=Part(root.transform,"LeftLeg",skin.leg,8);rig.rightLeg=Part(root.transform,"RightLeg",skin.leg,9);
        rig.face=Part(rig.head,"FaceOverlay",skin.faces[0],13).GetComponent<SpriteRenderer>();
        rig.speechAnchor=new GameObject("SpeechBubbleAnchor").transform;rig.speechAnchor.SetParent(rig.head,false);rig.speechAnchor.localPosition=new Vector3(0,.46f,0);
        rig.accessoryMounts=new GameObject("AccessoryMounts").transform;rig.accessoryMounts.SetParent(rig.head,false);
        rig.weaponMount=new GameObject("WeaponMount").transform;rig.weaponMount.SetParent(root.transform,false);
        rig.weapon=Part(rig.weaponMount,"Weapon",skin.weapon,15).GetComponent<SpriteRenderer>();
        rig.parachute=Part(root.transform,"Parachute",skin.parachute,7).GetComponent<SpriteRenderer>();rig.parachute.transform.localPosition=new Vector3(0,.72f,0);
        rig.ApplySkin(skin);rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,Prefabs+"/"+name+".prefab");Object.DestroyImmediate(root);return prefab;
    }
    private static CartoonRagdoll Ragdoll(CharacterSkin skin)
    {
        var root=new GameObject("LarperCartoonRagdoll");var doll=root.AddComponent<CartoonRagdoll>();
        string[] names={"Torso","Head","LeftArm","RightArm","LeftLeg","RightLeg"};
        Sprite[] sprites={skin.torso,skin.head,skin.arm,skin.arm,skin.leg,skin.leg};doll.parts=new Rigidbody2D[6];
        for(int i=0;i<6;i++)
        {
            var part=Part(root.transform,names[i],sprites[i],i==1?12:i<4?10:8);part.gameObject.layer=1;
            var body=part.gameObject.AddComponent<Rigidbody2D>();body.mass=i==0?.7f:i==1?.35f:.15f;body.gravityScale=2;
            body.linearDamping=.5f;body.angularDamping=1.5f;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            body.interpolation=RigidbodyInterpolation2D.Interpolate;body.sleepMode=RigidbodySleepMode2D.StartAwake;body.simulated=false;doll.parts[i]=body;
            var collider=part.gameObject.AddComponent<BoxCollider2D>();collider.size=(Vector2)sprites[i].bounds.size*.85f;collider.offset=sprites[i].bounds.center;
            // Built-in TransparentFX: queried gameplay masks exclude it; corpses collide only with Default geometry.
            collider.excludeLayers=~1;
            if(i>0){var joint=part.gameObject.AddComponent<HingeJoint2D>();joint.connectedBody=doll.parts[0];joint.autoConfigureConnectedAnchor=true; joint.anchor=i==1?new Vector2(0,-.3f):Vector2.zero; joint.enableCollision=false; joint.useLimits=true; joint.limits=new JointAngleLimits2D{min=i==1?-35:-80,max=i==1?35:80};}
        }
        doll.face=Part(doll.parts[1].transform,"FaceOverlay",skin.faces[(int)FaceExpression.Stunned],13).GetComponent<SpriteRenderer>();
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,Prefabs+"/LarperCartoonRagdoll.prefab");Object.DestroyImmediate(root);return prefab.GetComponent<CartoonRagdoll>();
    }
    private static void Bind(string path,GameObject visual,CartoonRagdoll ragdoll)
    {
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var existing=root.GetComponentInChildren<CutoutRig>(true);if(existing!=null)Object.DestroyImmediate(existing.gameObject);
            var child=(GameObject)PrefabUtility.InstantiatePrefab(visual,root.transform);child.name="VisualRoot";
            var driver=root.GetComponent<CharacterVisualDriver>();if(driver==null)driver=root.AddComponent<CharacterVisualDriver>();
            var so=new SerializedObject(driver);so.FindProperty("rig").objectReferenceValue=child.GetComponent<CutoutRig>();so.ApplyModifiedPropertiesWithoutUndo();
            if(root.TryGetComponent<SpriteRenderer>(out var source))source.enabled=false;
            if(ragdoll!=null){var death=root.GetComponent<RagdollOnDefeat>();if(death==null)death=root.AddComponent<RagdollOnDefeat>();var settings=new SerializedObject(death);settings.FindProperty("prefab").objectReferenceValue=ragdoll;settings.ApplyModifiedPropertiesWithoutUndo();}
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode first.");
        Folder(Art);Folder(Prefabs);Folder("Assets/Data/Visuals");
        // The icon visibly consists of a 24x24 head enlarged in exact 25px blocks.
        // Sample block centres from x500..1099,y100..699; preserve the archived source unchanged.
        var original=new Texture2D(2,2);original.LoadImage(File.ReadAllBytes("HistoricalArtSources/Insane dreams/Assets/Sprites/Ikona.gif.png"));
        System.Func<int,int,Color> sample=(x,y)=>original.GetPixel(500+x*25+12,original.height-1-(100+(23-y)*25+12));
        var head=Make("Derived_Ikona_HeadBase",24,24,new Vector2(.5f,.5f),(x,y)=>{var c=sample(x,y);return c.r<.2f||c.r>.95f&&c.g>.95f&&c.b>.95f?Skin:c;});
        var faces=new Sprite[6];
        for(int f=0;f<6;f++)
        {
            int expression=f;
            faces[f]=Make(f==1?"Derived_Ikona_Smug":"TEMP_Face_"+((FaceExpression)f),24,24,new Vector2(.5f,.5f),(x,y)=>
            {
                if(expression==1){var c=sample(x,y);return c.r<.2f||c.r>.95f&&c.g>.95f&&c.b>.95f?c:Color.clear;}
                bool eye=(x>=4&&x<=6||x>=16&&x<=18)&&y>=15&&y<=18;
                bool mouth=x>=8&&x<=15&&y==6;
                if(expression==2){eye=(x>=3&&x<=7&&y==19-(x-3)/2)||(x>=16&&x<=20&&y==17+(x-16)/2)||((x==6||x==17)&&y>=14&&y<=16);mouth=x>=8&&x<=15&&y==7;}
                if(expression==3||expression==4){eye=(x>=3&&x<=7||x>=16&&x<=20)&&y>=14&&y<=19;mouth=x>=9&&x<=14&&y>=4&&y<=9;}
                if(expression==5){eye=((x>=3&&x<=7)&&(y==x+11||y==19-(x-3)))||((x>=16&&x<=20)&&(y==x-2||y==18-(x-16)));mouth=y==6&&x>=9&&x<=15;}
                return eye||mouth?Ink:Color.clear;
            });
        }
        Object.DestroyImmediate(original);
        var torso=Make("TEMP_Torso",12,14,new Vector2(.5f,.5f),(x,y)=>x==0||x==11||y==0||y==13?Ink:y==7&&x>7?new Color(.5f,.5f,.5f):Color.white);
        var arm=Make("TEMP_Arm",5,14,new Vector2(.5f,1),(x,y)=>x==0||x==4||y==0?Ink:y<4?Skin:new Color(.65f,.62f,.46f));
        var leg=Make("TEMP_Leg",6,15,new Vector2(.5f,1),(x,y)=>x==0||x==5||y<3?Ink:new Color(.25f,.24f,.3f));
        var weapon=Make("TEMP_Weapon",16,6,new Vector2(.15f,.5f),(x,y)=>y>=3?Ink:x>=2&&x<=5?new Color(.4f,.27f,.17f):Color.clear);
        var chute=Make("TEMP_Parachute",64,40,new Vector2(.5f,0),(x,y)=>
        {
            int roof=37-Mathf.Abs(x-32)/4;
            if(y>=24&&y<=roof)return (x/8)%2==0?new Color(.75f,.25f,.15f):new Color(.9f,.65f,.2f);
            if(y<24&&(Mathf.Abs(x-(int)(32-y*1.15f))<=.5f||Mathf.Abs(x-(int)(32+y*1.15f))<=.5f))return new Color(.7f,.7f,.6f);
            return Color.clear;
        });
        CharacterSkin player=null,larper=null;
        for(int i=0;i<2;i++)
        {
            var skin=Asset<CharacterSkin>("Assets/Data/Visuals/"+(i==0?"Player":"Larper")+"TemporarySkin.asset");
            skin.head=head;skin.torso=torso;skin.arm=arm;skin.leg=leg;skin.weapon=weapon;skin.parachute=chute;skin.faces=faces;
            skin.clothing=i==0?new Color(.24f,.55f,.85f):new Color(.55f,.59f,.25f);
            skin.provenance="TEMPORARY body/expressions/weapon/canopy. Head palette and smug face derived by exact block sampling from archived Ikona.gif.png. Main Google Drive character artwork not recovered. See ART_MANIFEST.md.";
            EditorUtility.SetDirty(skin);if(i==0)player=skin;else larper=skin;
        }
        var playerRig=RigPrefab("PlayerCutoutVisual",player);var larperRig=RigPrefab("LarperCutoutVisual",larper);var doll=Ragdoll(larper);
        Bind("Assets/Prefabs/Player/WarehousePlayer.prefab",playerRig,null);Bind("Assets/Prefabs/Enemies/Larper.prefab",larperRig,doll);
        AssetDatabase.SaveAssets();return "Authored two replaceable skins, two cutout visual prefabs, one six-body/five-joint ragdoll; bound WarehousePlayer and Larper prefabs.";
    }

    private static Texture2D Read(string path)
    {var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes("HistoricalArtSources/Drive/"+path));return texture;}
    private static Color Sample(Texture2D image,RectInt crop,int x,int y,int w,int h)
    {
        int px=crop.x+Mathf.Min(crop.width-1,Mathf.FloorToInt((x+.5f)*crop.width/w));
        int py=crop.y+Mathf.Min(crop.height-1,Mathf.FloorToInt((h-1-y+.5f)*crop.height/h));
        return image.GetPixel(px,image.height-1-py);
    }
    private static Sprite Crop(Texture2D image,string name,RectInt rect,int w,int h,Vector2 pivot)
    {return Make(name,w,h,pivot,(x,y)=>Sample(image,rect,x,y,w,h));}
    public static string IntegrateDriveOriginals()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode first.");
        var player=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/PlayerTemporarySkin.asset");
        var larper=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/LarperTemporarySkin.asset");
        var human=Read("Postavy/Hráč/Obrazek 0.gif.preview.png");var parts=Read("Postavy/Ragdoll/Ragdol d.gif.preview.png");
        var gun=Read("Zbraně/MKG.gif.png");var canopy=Read("Padák/Padák.gif.png");
        // Original barrel points left. Normalize it to +X, the existing independent-aim convention.
        var weapon=Make("Derived_MKG",30,9,new Vector2(.2f,.5f),(x,y)=>Sample(gun,new RectInt(240,165,890,270),29-x,y,30,9));
        var chute=Make("Derived_Padak",48,28,new Vector2(.5f,0),(x,y)=>
        {var c=Sample(canopy,new RectInt(405,210,360,210),x,y,48,28);return c.a>0?new Color(.94f,.83f,.55f):Color.clear;});
        // Two distinct original palettes/silhouettes. Normalized part dimensions fit the existing
        // 0.8x1.4 collision envelope; these derivatives never overwrite downloaded originals.
        for(int actor=0;actor<2;actor++)
        {
            var skin=actor==0?player:larper;string prefix=actor==0?"Derived_Player_":"Derived_Larper_";
            Texture2D original=actor==0?human:parts;
            RectInt headRect=actor==0?new RectInt(170,50,80,80):new RectInt(678,142,90,140);
            int hw=actor==0?16:18,hh=actor==0?16:28;
            // Retain original hair/hat and skin texture; isolate original facial pixels for the
            // neutral overlay. Other expressions are explicitly temporary, matching that palette.
            System.Func<int,int,bool> facial=(x,y)=>actor==0?x>=11&&y>=3&&y<=11:y>=4&&y<=18;
            Color faceSkin=actor==0?new Color32(255,175,82,255):new Color32(255,218,167,255);
            skin.head=Make(prefix+"Head",hw,hh,new Vector2(.5f,.5f),(x,y)=>
            {var c=Sample(original,headRect,x,y,hw,hh);bool feature=actor==0?c.b>c.r||c.g>.85f&&c.b>.85f||c.r>.8f&&c.g<.4f:c.r<.1f;return facial(x,y)&&c.a>0&&feature?faceSkin:c;});
            var faces=new Sprite[6];
            for(int f=0;f<6;f++)
            {
                int expression=f;
                faces[f]=Make(prefix+(f==0?"OriginalFace":"TEMP_Face_"+(FaceExpression)f),hw,hh,new Vector2(.5f,.5f),(x,y)=>
                {
                    var c=Sample(original,headRect,x,y,hw,hh);
                    bool feature=actor==0?c.b>c.r||c.g>.85f&&c.b>.85f||c.r>.8f&&c.g<.4f:c.r<.1f;
                    if(expression==0)return facial(x,y)&&c.a>0&&feature?c:Color.clear;
                    int fx=actor==0?x-10:x-3,fy=actor==0?y-3:(y-5)/2;
                    bool eye=actor==0?fx>=2&&fx<=4&&fy>=5&&fy<=7:(fx>=0&&fx<=2||fx>=8&&fx<=10)&&fy>=5&&fy<=7;
                    bool mouth=actor==0?fx>=2&&fx<=5&&fy==0:fx>=1&&fx<=9&&fy==0;
                    if(expression==1){eye=eye&&fy==5;mouth=mouth||fx==9&&fy==1;}
                    if(expression==2){eye=eye&&fy<=6;mouth=mouth||fy==1&&fx==5;}
                    if(expression==3||expression==4){mouth=fx>=3&&fx<=7&&fy>=0&&fy<=2;}
                    if(expression==4)eye=eye||fy==9&&(fx>=0&&fx<=2||fx>=8&&fx<=10);
                    if(expression==5){eye=(fx==0||fx==2||fx==8||fx==10)&&(fy==5||fy==7)||((fx==1||fx==9)&&fy==6);}
                    return eye||mouth?Ink:Color.clear;
                });
            }
            skin.faces=faces;
            skin.torso=actor==0?Crop(human,prefix+"Torso",new RectInt(190,130,40,170),12,14,new Vector2(.5f,.5f)):Crop(parts,prefix+"Torso",new RectInt(524,71,100,250),12,14,new Vector2(.5f,.5f));
            skin.arm=actor==0?Crop(human,prefix+"Arm",new RectInt(150,210,40,100),5,14,new Vector2(.5f,1)):Make(prefix+"Arm",5,14,new Vector2(.5f,1),(x,y)=>Sample(parts,new RectInt(59,88,190,50),13-y,x,14,5));
            skin.leg=actor==0?Crop(human,prefix+"Leg",new RectInt(220,350,30,150),6,15,new Vector2(.5f,1)):Crop(parts,prefix+"Leg",new RectInt(431,104,50,180),6,15,new Vector2(.5f,1));
            skin.weapon=weapon;skin.parachute=chute;skin.clothing=Color.white;
            skin.provenance="Original Drive parts adapted with nearest-neighbour crop/resampling. Expressions except neutral remain TEMP. Parachute recolored cream for contrast. Original downloads unchanged; see ART_MANIFEST.md for source IDs and crop rectangles.";
            EditorUtility.SetDirty(skin);
        }
        Object.DestroyImmediate(human);Object.DestroyImmediate(parts);Object.DestroyImmediate(gun);Object.DestroyImmediate(canopy);
        var playerRig=RigPrefab("PlayerCutoutVisual",player);var larperRig=RigPrefab("LarperCutoutVisual",larper);var doll=Ragdoll(larper);
        Bind("Assets/Prefabs/Player/WarehousePlayer.prefab",playerRig,null);Bind("Assets/Prefabs/Enemies/Larper.prefab",larperRig,doll);
        AssetDatabase.SaveAssets();return "Integrated selected Drive originals as replaceable cutout skins; retained temporary expression variants, no gameplay changes.";
    }
}
