using System.IO;
using System.Linq;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Sprint009PresentationSetup
{
    private const string Art="Assets/Art/Sprint009";
    private static Color Ink=>new Color32(24,28,43,255);
    private static Color Steel=>new Color32(83,109,122,255);
    private static Color Gold=>new Color32(239,184,62,255);
    private static Sprite Draw(string name,int w,int h,Vector2 pivot,System.Func<int,int,Color> paint)
    {
        var texture=new Texture2D(w,h,TextureFormat.RGBA32,false);for(int y=0;y<h;y++)for(int x=0;x<w;x++)texture.SetPixel(x,y,paint(x,y));texture.Apply();
        string path=Art+"/"+name+".png";File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=32;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;
        importer.wrapMode=TextureWrapMode.Clamp;var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=pivot;settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode first.");
        if(!AssetDatabase.IsValidFolder(Art))AssetDatabase.CreateFolder("Assets/Art","Sprint009");
        var second=Draw("CompactFirearm",20,8,new Vector2(.15f,.5f),(x,y)=>y>=3&&y<=6?x<4?Gold:Steel:x>=6&&x<=8?Ink:Color.clear);
        var melee=Draw("ScrapWrench",18,8,new Vector2(.15f,.5f),(x,y)=>(y==3||y==4)&&x<15?Steel:x>=12&&(y<2||y>5)?Steel:Color.clear);
        for(int actor=0;actor<2;actor++)
        {
            bool player=actor==0;string name=player?"Worker":"BucketLarper";int h=player?10:11;
            var skin=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/Sprint008"+name+"Skin.asset");
            Color suit=player?new Color32(186,96,55,255):new Color32(119,83,146,255),flesh=new Color32(186,175,157,255);
            skin.head=Draw(name+"_Head",12,h,new Vector2(.5f,.5f),(x,y)=>x==0||x==11||y==0||y==h-1?Ink:y>=h-3?player?Gold:Steel:flesh);
            skin.torso=Draw(name+"_Torso",12,17,new Vector2(.5f,.5f),(x,y)=>x==0||x==11||y==0||y==16?Ink:player&&(y==10||y==11)?Gold:!player&&(x+y)%9==0?new Color32(62,133,169,255):x==5?Ink:suit);
            skin.arm=Draw(name+"_Arm",5,18,new Vector2(.5f,1),(x,y)=>x==0||x==4||y==0?Ink:y<4?flesh:y==9?Steel:suit);
            skin.leg=Draw(name+"_Leg",6,18,new Vector2(.5f,1),(x,y)=>x==0||x==5||y<3?Ink:y==9?Steel:player?new Color32(46,76,105,255):new Color32(60,67,75,255));
            for(int f=0;f<6;f++)
            {
                int expression=f;
                skin.faces[f]=Draw(name+"_Face_"+(FaceExpression)f,12,h,new Vector2(.5f,.5f),(x,y)=>
                {
                    bool eyes=(x==3||x==8)&&y==h-5,mouth=x>=4&&x<=7&&y==2;
                    if(expression==1){eyes=(x==3||x==4||x==8||x==9)&&y==h-5;mouth=mouth||x==8&&y==3;}
                    if(expression==2)eyes=eyes||y==h-4&&(x==2||x==9);
                    if(expression==3||expression==4)mouth=x>=5&&x<=6&&y>=1&&y<=3;
                    if(expression==4)eyes=eyes||y==h-3&&(x==3||x==8);
                    if(expression==5)eyes=((x==2||x==4||x==7||x==9)&&(y==h-6||y==h-4))||((x==3||x==8)&&y==h-5);
                    return eyes||mouth?Ink:Color.clear;
                });
            }
            skin.secondaryWeapon=second;skin.meleeWeapon=melee;
            skin.provenance="Sprint 009 proportional revision of the independently code-authored Sprint 008 style. New head/face/body pixel patterns; no historical image input. Publication/provenance review still applies.";EditorUtility.SetDirty(skin);
            foreach(string path in new[]{"Assets/Prefabs/Visuals/"+(player?"Player":"Larper")+"CutoutVisual.prefab",player?"Assets/Prefabs/Player/WarehousePlayer.prefab":"Assets/Prefabs/Enemies/Larper.prefab"})
            {
                var root=PrefabUtility.LoadPrefabContents(path);try
                {
                    var rig=root.GetComponentInChildren<CutoutRig>(true);rig.ApplySkin(skin);rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);
                    rig.speechAnchor.localPosition=new Vector3(0,skin.head.bounds.extents.y+.09375f,0);
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }finally{PrefabUtility.UnloadPrefabContents(root);}
            }
        }
        var larper=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/Sprint008BucketLarperSkin.asset");
        var dollRoot=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab");try
        {
            var doll=dollRoot.GetComponent<CartoonRagdoll>();Sprite[] sprites={larper.torso,larper.head,larper.arm,larper.arm,larper.leg,larper.leg};float[] masses={.78f,.18f,.13f,.13f,.24f,.24f};
            var live=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/LarperCutoutVisual.prefab").GetComponent<CutoutRig>().BodyParts;
            for(int i=0;i<6;i++)
            {
                var part=doll.parts[i];part.GetComponent<SpriteRenderer>().sprite=sprites[i];part.mass=masses[i];part.transform.localPosition=live[i].localPosition;part.transform.localRotation=live[i].localRotation;
                var box=part.GetComponent<BoxCollider2D>();box.size=(Vector2)sprites[i].bounds.size*.85f;box.offset=sprites[i].bounds.center;part.centerOfMass=box.offset;
                if(i>0){var joint=part.GetComponent<HingeJoint2D>();joint.anchor=i==1?new Vector2(0,sprites[i].bounds.min.y):Vector2.zero;joint.autoConfigureConnectedAnchor=true;}
            }
            doll.face.sprite=larper.Face(FaceExpression.Stunned);PrefabUtility.SaveAsPrefabAsset(dollRoot,"Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab");
        }finally{PrefabUtility.UnloadPrefabContents(dollRoot);}
        var foot=Draw("LadderFoot",32,4,new Vector2(.5f,.5f),(x,y)=>x<5||x>26?Steel:y==0?Gold:Color.clear);
        var top=Draw("LadderHandrail",32,4,new Vector2(.5f,.5f),(x,y)=>x<5||x>26?y==3?Gold:Steel:Color.clear);
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var art=scene.GetRootGameObjects().First(g=>g.name=="WarehouseArt_Sprint008");
        foreach(var volume in Object.FindObjectsByType<InsaneDreams.Traversal.LadderVolume>(FindObjectsSortMode.None))
        {
            var graphic=art.transform.Find("Graphic_"+volume.name);var old=graphic.GetComponent<SpriteRenderer>();old.enabled=false;
            var visual=graphic.GetComponent<LadderVisual2D>();if(visual==null)visual=graphic.gameObject.AddComponent<LadderVisual2D>();
            // Floor-adjacent platforms author visual feet, not traversal endpoints. The roof
            // access starts beside the mezzanine; its capture volume intentionally begins lower.
            float floor=volume.Bottom;float best=float.PositiveInfinity;
            foreach(var box in scene.GetRootGameObjects().First(g=>g.name=="Environment").GetComponentsInChildren<BoxCollider2D>())
            {
                float level=box.bounds.max.y;float side=Mathf.Max(0,Mathf.Abs(box.bounds.center.x-volume.CenterX)-box.bounds.extents.x);
                if(!box.isTrigger&&level>=volume.Bottom-.01f&&level<=volume.Bottom+1.5f&&side<=Mathf.Abs(volume.TopExitHorizontalOffset))
                {float delta=level-volume.Bottom;if(delta<best){best=delta;floor=level;}}
            }
            visual.Configure(floor,volume.Top,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint008/Ladder.png"),foot,top);
            foreach(var renderer in visual.GetComponentsInChildren<SpriteRenderer>())renderer.sharedMaterial=AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Saved smaller heads, larger articulated bodies, matched six-part physics, equipment sprites and endpoint-authored ladder visuals; traversal colliders untouched.";
    }
}
