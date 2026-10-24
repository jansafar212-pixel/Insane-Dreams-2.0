using System;
using System.Linq;
using InsaneDreams.Visuals;
using InsaneDreams.Enemies;
using InsaneDreams.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Native, editable flat-pixel recipes extending the existing 0245/0246 authoring.
// No source-image import, rescaling of the rig or changes to historical originals.
public static class Sprint0247Setup
{
    private const string Art="Assets/Art/Sprint0247/";
    private static readonly Color Ink=new Color32(25,29,43,255),Peach=new Color32(255,214,165,255),
        Brown=new Color32(80,43,12,255),Red=new Color32(194,43,22,255),Blue=new Color32(34,66,220,255),
        Teal=new Color32(12,99,116,255),Paper=new Color32(255,255,235,255),Card=new Color32(181,132,65,255),
        Yellow=new Color32(247,201,44,255),Orange=new Color32(226,103,30,255);
    private static Sprite Draw(string name,int w,int h,Vector2 pivot,Func<int,int,Color> pixel)
    {
        string path=Art+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)t.SetPixel(x,y,pixel(x,y));t.Apply();
        string texturePath=Art+name+"Pixels.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if(texture==null){texture=t;AssetDatabase.CreateAsset(texture,texturePath);}else{EditorUtility.CopySerialized(t,texture);Object.DestroyImmediate(t);}
        var s=Sprite.Create(texture,new Rect(0,0,w,h),pivot,32,0,SpriteMeshType.FullRect);s.name=name;
        if(old==null){AssetDatabase.CreateAsset(s,path);return s;}EditorUtility.CopySerialized(s,old);Object.DestroyImmediate(s);return old;
    }
    private static Sprite Piece(Sprite s,string name,float bottom,float top)
    {
        string path=Art+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old!=null)return old;
        var r=s.rect;r.y+=r.height*bottom;r.height=s.rect.height*(top-bottom);
        var part=Sprite.Create(s.texture,r,new Vector2(.5f,1),32,0,SpriteMeshType.FullRect);part.name=name;AssetDatabase.CreateAsset(part,path);return part;
    }
    private static CharacterSkin Skin(string name,CharacterSkin source,Sprite head,Sprite torso,Sprite arm,Sprite leg)
    {
        string path="Assets/Data/Visuals/"+name+".asset";var s=AssetDatabase.LoadAssetAtPath<CharacterSkin>(path);
        if(s==null){s=Object.Instantiate(source);s.name=name;AssetDatabase.CreateAsset(s,path);}
        s.head=head;s.torso=torso;s.arm=arm;s.leg=leg;s.clothing=Color.white;
        s.provenance="Sprint 024.7 native pixel recipes; current Larper production baseline + inspected Enemak Postava references. Historical originals preserved; new redesign awaiting artistic playtest review.";
        EditorUtility.SetDirty(s);return s;
    }
    private static void Number(Object o,string field,float value){var s=new SerializedObject(o);s.FindProperty(field).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    private static void Lines(LarperPersonalityProfile p,string field,params string[] lines)
    {var s=new SerializedObject(p);var a=s.FindProperty(field);a.arraySize=lines.Length;for(int i=0;i<lines.Length;i++)a.GetArrayElementAtIndex(i).stringValue=lines[i];s.ApplyModifiedPropertiesWithoutUndo();}
    private static CharacterSkin Player(CharacterSkin larper)
    {
        var old=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/Sprint008WorkerSkin.asset");
        var h=Draw("HeroHead",12,11,new Vector2(.5f,.5f),(x,y)=>
        {if(y>=9)return x<10&&y==9||x>=2&&x<8?Yellow:Color.clear;if(y==8)return x<2||x>9?Brown:Yellow;if(y==7&&x==0)return Brown;return Peach;});
        var t=Draw("HeroShirt",12,17,new Vector2(.5f,.5f),(x,y)=>
        {if(y>14&&(x==0||x>9))return Color.clear;if(y>9&&x>=4&&x<=6)return Peach;if(x==8&&y>2&&y<9)return Paper;if(y==2&&x>1&&x<10)return Brown;return Orange;});
        var a=Draw("HeroArm",5,18,new Vector2(.5f,1),(x,y)=>y>11?Orange:Peach);
        var l=Draw("HeroLeg",6,18,new Vector2(.5f,1),(x,y)=>y<3?Brown:Blue);
        var s=Skin("Hero0247",old,h,t,a,l);s.faces=larper.faces;s.panicFace=larper.panicFace;EditorUtility.SetDirty(s);return s;
    }
    private static CharacterSkin Samurai(CharacterSkin larper)
    {
        var old=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/SamuraiCandidate.asset");
        var h=Draw("SamuraiHelmet",14,10,new Vector2(.5f,.5f),(x,y)=>
        {
            if(y>=7){if(y==9&&x<3||y==8&&x>11)return Color.clear;return x==2&&y==9||x==11&&y==7?Red:Card;}
            if(y==6)return x<2||x>11?Ink:Card;
            if(x<2||x>11)return y>3&&(x==1||x==12)?Card:Color.clear;
            return Peach;
        });
        var t=Draw("SamuraiCardboard",10,11,new Vector2(.5f,.5f),(x,y)=>
        {if(y==10&&x>7)return Color.clear;if(x<=2)return y<7?Teal:Card;if(x>=7)return y>5?Red:Card;if(y==3||x==5&&y>3)return Ink;return Card;});
        var a=Draw("SamuraiArm",4,12,new Vector2(.5f,1),(x,y)=>y>=8?(x<2?Card:Red):Peach);
        var l=Draw("SamuraiLeg",5,12,new Vector2(.5f,1),(x,y)=>y<2?Brown:x==0?Card:Teal);
        var s=Skin("Samurai0247",old,h,t,a,l);s.faces=new Sprite[6];
        for(int f=0;f<6;f++){int expression=f;s.faces[f]=Draw("SamuraiFace"+f,14,10,new Vector2(.5f,.5f),(x,y)=>
        {bool eyes=(x==4||x==9)&&y==4;bool brow=(x==3||x==10)&&y==5;
            bool mouth=expression==4||expression==3?x==7&&y>=1&&y<=2:y==1&&x>=6&&x<=8;
            if(expression==5)eyes=(x==4||x==9)&&(y==3||y==5);return eyes||brow||mouth?Ink:Color.clear;});}
        s.panicFace=s.faces[4];
        s.weapon=s.meleeWeapon=Draw("HugeCardboardKatana",46,7,new Vector2(3f/46,.5f),(x,y)=>
        {if(x<5)return y>=2&&y<=4?(x%2==0?Red:Ink):Color.clear;if(x==5||x==6)return y>=1&&y<=5?Yellow:Color.clear;
            int bend=x>28?1:0;return y>=2+bend&&y<=4+bend?(y==2+bend?Ink:Paper):Color.clear;});
        EditorUtility.SetDirty(s);return s;
    }
    private static CharacterSkin Fireworks(CharacterSkin larper)
    {
        var old=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/FireworksExpertSkin.asset");
        var h=Draw("FireworksHead",14,12,new Vector2(.5f,.5f),(x,y)=>
        {if(y>=9)return y==11&&(x<3||x>9)?Color.clear:Yellow;if(x==0||x==13)return y>6?Yellow:Color.clear;
            if(y==3&&(x<5||x>8)||y==2&&(x==3||x==10))return Brown;return Peach;});
        var t=Draw("FireworksCoat",14,17,new Vector2(.5f,.5f),(x,y)=>
        {if(y==16&&(x<2||x>10))return Color.clear;if(x>=9&&y>3)return x==10?Red:x==12?Yellow:Blue;if(x==3&&y>3&&y<12)return Paper;return Teal;});
        var a=Draw("FireworksArm",5,18,new Vector2(.5f,1),(x,y)=>y>11?Teal:Peach);
        var l=Draw("FireworksLeg",6,18,new Vector2(.5f,1),(x,y)=>y<3?Brown:Blue);
        var s=Skin("Fireworks0247",old,h,t,a,l);s.faces=larper.faces;s.panicFace=larper.panicFace;EditorUtility.SetDirty(s);return s;
    }
    public static void Apply()
    {
        if(!AssetDatabase.IsValidFolder(Art.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Art","Sprint0247");
        var larper=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/LarperCandidate.asset");
        var hero=Player(larper);var samurai=Samurai(larper);var expert=Fireworks(larper);
        foreach(var name in new[]{"WarehousePlayer","Larper","Samurai","FireworksExpert"})
        {
            string path="Assets/Prefabs/"+(name=="WarehousePlayer"?"Player/":"Enemies/")+name+".prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                Number(root.GetComponent<PlayerMotor2D>(),"moveSpeed",name=="WarehousePlayer"?8.4f:name=="Larper"?7.7f:7);
                var r=root.GetComponentInChildren<CutoutRig>(true);
                if(name!="Larper")
                {
                    var skin=name=="WarehousePlayer"?hero:name=="Samurai"?samurai:expert;
                    if(name=="Samurai")
                    {
                        var j=r.Segmented;j.upperArm=Piece(skin.arm,"SamuraiUpper",.48f,1);j.forearm=Piece(skin.arm,"SamuraiFore",.16f,.50f);j.hand=Piece(skin.arm,"SamuraiHand",0,.18f);
                        j.thigh=Piece(skin.leg,"SamuraiThigh",.48f,1);j.shin=Piece(skin.leg,"SamuraiShin",.16f,.50f);j.foot=Piece(skin.leg,"SamuraiFoot",0,.18f);
                        Number(r,"torsoCenterY",-.2f);Number(r,"shoulderHalfWidth",.14f);
                    }
                    Number(r,"rearArmShade",1);r.ApplySkin(skin);r.face.enabled=true;
                    foreach(var part in r.BodyParts)part.GetComponent<SpriteRenderer>().color=Color.white;
                    r.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);if(name=="Samurai")r.Segmented.Pose(CharacterPose.Idle,0,15,true,0);
                }
                if(name=="Larper")
                {
                    var c=root.GetComponent<EnemyCombat>();Number(c,"nearMissDistance",1.5f);Number(c,"nearMissSuppression",58);Number(c,"moderateSuppression",14);Number(c,"highSuppression",42);Number(c,"defensiveCrouchProbability",.8f);Number(c,"overheadDistanceBias",.15f);
                    var data=new SerializedObject(c);data.FindProperty("defensiveCrouchCooldown").vector2Value=new Vector2(2.5f,4.25f);data.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        // Keep the standalone player visual fallback consistent with the actor prefab.
        const string visualPath="Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab";var visual=PrefabUtility.LoadPrefabContents(visualPath);
        try{var r=visual.GetComponent<CutoutRig>();Number(r,"rearArmShade",1);r.ApplySkin(hero);foreach(var part in r.BodyParts)part.GetComponent<SpriteRenderer>().color=Color.white;r.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);PrefabUtility.SaveAsPrefabAsset(visual,visualPath);}finally{PrefabUtility.UnloadPrefabContents(visual);}
        var sword=AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/SamuraiSword.asset");var sd=new SerializedObject(sword);sd.FindProperty("equippedSprite").objectReferenceValue=samurai.weapon;sd.ApplyModifiedPropertiesWithoutUndo();
        var jp=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Samurai.asset");
        Lines(jp,"detected","拙者は寿司の為に戦う。","我こそ段ボール将軍！");Lines(jp,"misses","刀よ、そなたは長すぎる！");Lines(jp,"witnessed","友よ、昼寝には早いぞ。");Lines(jp,"withdraw","これは勇ましき後退でござる！");Lines(jp,"panic","待て！鉄砲は卑怯でござる！");Lines(jp,"suppressed","拙者の兜は紙でござる！","勇者は床と語るものなり。");
        Lines(jp,"lost","忍法、見失いの術！");Lines(jp,"blocked","退路が小さすぎるぞ！");
        foreach(string n in new[]{"Ordinary","Cowardly","Overconfident"})
        {var p=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+n+".asset");Lines(p,"suppressed",n=="Cowardly"?"THAT WAS MY HAIR!":n=="Overconfident"?"I FEAR NOTHING! DUCK!":"TACTICAL FLOOR INSPECTION!","PLEASE MISS SOMEWHERE ELSE!","COVER IS MY BEST FRIEND!");}
        foreach(string path in new[]{"Assets/Scenes/Warehouse_Demo.unity","Assets/Scenes/Warehouse_Mission01.unity","Assets/Scenes/Warehouse_Testbed.unity"})
        {
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
                foreach(var follow in all.Select(t=>t.GetComponent<InsaneDreams.CameraSystem.WarehouseCameraFollow>()).Where(c=>c!=null))Number(follow,"gameplayZoomMultiplier",1.155f);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }finally{EditorSceneManager.CloseScene(scene,true);}
        }
        AssetDatabase.SaveAssets();
    }
}
