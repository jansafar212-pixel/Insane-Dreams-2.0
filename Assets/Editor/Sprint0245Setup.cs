using System.Linq;
using InsaneDreams.Visuals;
using InsaneDreams.Combat;
using InsaneDreams.Missions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class Sprint0245Setup
{
    private const string Art="Assets/Art/Sprint0245/";
    private static readonly Color32 Peach=new Color32(255,214,165,255);
    private static readonly Color32 Ink=new Color32(25,29,43,255);
    private static readonly Color32[] Palette={Peach,Ink,new Color32(80,43,12,255),new Color32(194,43,22,255),new Color32(34,66,220,255),new Color32(12,99,116,255),new Color32(219,43,44,255),new Color32(255,255,235,255)};
    private static void Save(Object asset,string path)
    {var old=AssetDatabase.LoadMainAssetAtPath(path);if(old!=null){EditorUtility.CopySerialized(asset,old);Object.DestroyImmediate(asset);}else AssetDatabase.CreateAsset(asset,path);}
    private static Sprite Sprite(Texture2D texture,string name,Rect rect,Vector2 pivot)
    {string path=Art+name+".asset";var s=UnityEngine.Sprite.Create(texture,rect,pivot,32,0,SpriteMeshType.FullRect);s.name=name;Save(s,path);return AssetDatabase.LoadAssetAtPath<Sprite>(path);}
    private static Color Nearest(Color pixel)
    {if(pixel.a<.6f)return Color.clear;Color best=Palette[0];float distance=float.PositiveInfinity;foreach(Color c in Palette){float d=(new Vector3(pixel.r-c.r,pixel.g-c.g,pixel.b-c.b)).sqrMagnitude;if(d<distance){distance=d;best=c;}}return best;}
    private static Texture2D Atlas()
    {
        var raw=new Texture2D(2,2);raw.LoadImage(System.IO.File.ReadAllBytes(Art+"CandidateSource.png"));var atlas=new Texture2D(64,64,TextureFormat.RGBA32,false){name="CandidateAtlas",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        atlas.SetPixels(new Color[4096]);
        for(int actor=0;actor<2;actor++)for(int part=0;part<4;part++)
        {
            int x0=part*raw.width/4,x1=(part+1)*raw.width/4,y0=(1-actor)*raw.height/2,y1=y0+raw.height/2;
            int minX=x1,minY=y1,maxX=x0,maxY=y0;
            for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(raw.GetPixel(x,y).a>.9f){minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
            int w=part<2?12:part==2?5:6,h=part==0?11:part==1?17:18,dx=part*16+(16-w)/2,dy=(1-actor)*32;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                Color c=Nearest(raw.GetPixel(minX+Mathf.RoundToInt((maxX-minX)*(x+.5f)/w),minY+Mathf.RoundToInt((maxY-minY)*(y+.5f)/h)));
                if(part==0&&actor==0&&y<8)c=Peach;
                // Skin is one palette entry; sleeves remain distinct, no brown cuffs.
                if(part==2&&y<12)c=Peach;
                atlas.SetPixel(dx+x,dy+y,c);
            }
        }
        atlas.Apply();Object.DestroyImmediate(raw);Save(atlas,Art+"CandidateAtlas.asset");return AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"CandidateAtlas.asset");
    }
    private static Sprite[] Faces()
    {
        var t=new Texture2D(84,11,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,name="LarperExpressions"};t.SetPixels(new Color[924]);
        var result=new Sprite[6];
        for(int f=0;f<7;f++)for(int y=0;y<11;y++)for(int x=0;x<12;x++)
        {
            bool eyes=(x==3||x==8)&&y==6,mouth=x>=4&&x<=7&&y==2;
            if(f==1){eyes|=y==7&&(x==4||x==7);mouth|=x==8&&y==3;}
            if(f==2)eyes|=y==7&&(x==2||x==9);
            if(f==3||f==4)mouth=x>=5&&x<=6&&y>=1&&y<=3;
            if(f==4)eyes|=y==8&&(x==3||x==8);
            if(f==5)eyes=((x==2||x==4||x==7||x==9)&&(y==5||y==7))||((x==3||x==8)&&y==6);
            if(f==6){eyes=(x==3||x==8)&&(y==6||y==7)||y==8&&(x==2||x==9);mouth=x>=4&&x<=7&&y>=1&&y<=3;}
            if(eyes||mouth)t.SetPixel(f*12+x,y,Ink);
        }
        t.Apply();Save(t,Art+"LarperExpressions.asset");t=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"LarperExpressions.asset");
        for(int f=0;f<6;f++)result[f]=Sprite(t,"Larper_Face_"+(FaceExpression)f,new Rect(f*12,0,12,11),new Vector2(.5f,.5f));Sprite(t,"Larper_Face_Panic",new Rect(72,0,12,11),new Vector2(.5f,.5f));return result;
    }
    private static CharacterSkin Skin(Texture2D atlas,int actor,Sprite[] faces)
    {
        string name=actor==0?"LarperCandidate":"SamuraiCandidate",path="Assets/Data/Visuals/"+name+".asset";
        var skin=AssetDatabase.LoadAssetAtPath<CharacterSkin>(path);if(skin==null){skin=Object.Instantiate(AssetDatabase.LoadAssetAtPath<CharacterSkin>(actor==0?"Assets/Data/Visuals/Sprint008BucketLarperSkin.asset":"Assets/Data/Visuals/SamuraiSkin.asset"));AssetDatabase.CreateAsset(skin,path);}
        Sprite[] parts=new Sprite[4];string[] names={"Head","Torso","Arm","Leg"};
        for(int i=0;i<4;i++){int w=i<2?12:i==2?5:6,h=i==0?11:i==1?17:18;parts[i]=Sprite(atlas,(actor==0?"Larper":"Samurai")+"_"+names[i],new Rect(i*16+(16-w)/2,(1-actor)*32,w,h),new Vector2(.5f,i<2?.5f:1));}
        skin.head=parts[0];skin.torso=parts[1];skin.arm=parts[2];skin.leg=parts[3];skin.faces=faces;skin.panicFace=actor==0?AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Larper_Face_Panic.asset"):null;skin.clothing=Color.white;skin.provenance="Sprint 024.5 candidate — awaiting artistic approval. Historical Enemak PNGs and selected numbered GIFs inspected; primitive generated atlas quantized to eight flat colors, 32 PPU. Originals preserved.";EditorUtility.SetDirty(skin);return skin;
    }
    private static Transform Segment(Transform parent,string name,Sprite sprite,int order,Color color)
    {var t=parent.Find(name);if(t==null){t=new GameObject(name).transform;t.SetParent(parent,false);}var r=t.GetComponent<SpriteRenderer>();if(r==null)r=t.gameObject.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;r.color=color;r.sharedMaterial=parent.GetComponent<SpriteRenderer>().sharedMaterial;return t;}
    private static Sprite Piece(Sprite original,string name,float bottom,float top)
    {var rect=original.rect;rect.y+=rect.height*bottom;rect.height=original.rect.height*(top-bottom);return Sprite(original.texture,name,rect,new Vector2(.5f,1));}
    private static void Rig(GameObject root,CharacterSkin skin,bool firearm)
    {
        var rig=root.GetComponentInChildren<CutoutRig>(true);var joints=rig.GetComponent<SamuraiArticulatedRig>();if(joints==null)joints=rig.gameObject.AddComponent<SamuraiArticulatedRig>();foreach(var extra in rig.GetComponents<SamuraiArticulatedRig>().Where(j=>j!=joints).ToArray())Object.DestroyImmediate(extra);joints.firearm=firearm;
        string prefix=firearm?"Larper":"Samurai";
        // A variant can carry older child chains in addition to inherited Larper
        // chains. Keep one chain per limb; never render both generations together.
        foreach(var limb in new[]{rig.leftArm,rig.rightArm,rig.leftLeg,rig.rightLeg})
            foreach(var group in limb.Cast<Transform>().GroupBy(t=>t.name).ToArray())
                foreach(var duplicate in group.Skip(1).ToArray())Object.DestroyImmediate(duplicate.gameObject);
        joints.upperArm=Piece(skin.arm,prefix+"_UpperArm",.48f,1);joints.forearm=Piece(skin.arm,prefix+"_Forearm",.16f,.50f);joints.hand=Piece(skin.arm,prefix+"_Hand",0,.18f);
        joints.thigh=Piece(skin.leg,prefix+"_Thigh",.48f,1);joints.shin=Piece(skin.leg,prefix+"_Shin",.16f,.50f);joints.foot=Piece(skin.leg,prefix+"_Foot",0,.18f);
        joints.leftElbow=Segment(rig.leftArm,"Elbow",joints.forearm,9,rig.RearArmTint);joints.leftHand=Segment(joints.leftElbow,"WristHand",joints.hand,9,rig.RearArmTint);
        joints.rightElbow=Segment(rig.rightArm,"Elbow",joints.forearm,14,Color.white);joints.rightHand=Segment(joints.rightElbow,"WristHand",joints.hand,17,Color.white);
        joints.leftKnee=Segment(rig.leftLeg,"Knee",joints.shin,8,Color.white);joints.leftFoot=Segment(joints.leftKnee,"AnkleFoot",joints.foot,8,Color.white);
        joints.rightKnee=Segment(rig.rightLeg,"Knee",joints.shin,9,Color.white);joints.rightFoot=Segment(joints.rightKnee,"AnkleFoot",joints.foot,9,Color.white);
        joints.grip=joints.rightHand.Find("SwordGrip");if(joints.grip==null){joints.grip=new GameObject("SwordGrip").transform;joints.grip.SetParent(joints.rightHand,false);}
        var data=new SerializedObject(rig);data.FindProperty("segmented").objectReferenceValue=joints;data.FindProperty("rearArmShade").floatValue=1;data.ApplyModifiedPropertiesWithoutUndo();rig.ApplySkin(skin);rig.face.enabled=firearm;
        foreach(var t in new[]{joints.leftElbow,joints.leftHand})t.GetComponent<SpriteRenderer>().color=Color.white;
        if(firearm)foreach(var t in rig.accessoryMounts.GetComponentsInChildren<Transform>(true).Where(t=>t!=rig.accessoryMounts).ToArray())if(t!=null)Object.DestroyImmediate(t.gameObject);
        rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);if(!firearm)joints.Pose(CharacterPose.Idle,0,15,true,0);
    }
    private static Sprite[] ArmorArt()
    {
        var texture=new Texture2D(30,13,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,name="ArmorStates"};texture.SetPixels(new Color[390]);
        for(int state=0;state<3;state++)for(int y=0;y<13;y++)for(int x=0;x<10;x++)
        {if(y>10&&(x<2||x>7)||state==2&&x>3&&x<7&&y<7)continue;Color c=x==0||x==9||y==0?Ink:new Color32(83,104,57,255);if(state>0&&x==y/2+2)c=new Color32(177,165,104,255);texture.SetPixel(state*10+x,y,c);}
        texture.Apply();Save(texture,Art+"ArmorStates.asset");texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"ArmorStates.asset");return Enumerable.Range(0,3).Select(i=>Sprite(texture,"Armor_"+i,new Rect(i*10,0,10,13),new Vector2(.5f,.5f))).ToArray();
    }
    private static Sprite Katana()
    {
        var source=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/SamuraiSkin.asset").weapon;
        var raw=new Texture2D(2,2);raw.LoadImage(System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(source.texture)));
        var t=new Texture2D(40,10,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,name="KatanaCandidate"};
        Color[] colors={Ink,new Color32(216,236,237,255),new Color32(211,160,47,255),new Color32(202,42,35,255)};
        for(int y=0;y<10;y++)for(int x=0;x<40;x++)
        {var p=raw.GetPixel((int)(source.rect.x+(x+.5f)*source.rect.width/40),(int)(source.rect.y+(y+.5f)*source.rect.height/10));Color c=Color.clear;if(p.a>.6f){float best=float.PositiveInfinity;foreach(var color in colors){float d=new Vector3(p.r-color.r,p.g-color.g,p.b-color.b).sqrMagnitude;if(d<best){best=d;c=color;}}}t.SetPixel(x,y,c);}
        Object.DestroyImmediate(raw);t.Apply();Save(t,Art+"KatanaCandidate.asset");return Sprite(AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"KatanaCandidate.asset"),"Samurai_Katana",new Rect(0,0,40,10),new Vector2(.12f,.5f));
    }
    private static void Armor(GameObject actor,Sprite[] sprites)
    {
        var rig=actor.GetComponentInChildren<CutoutRig>(true);if(rig==null)return;var visual=actor.GetComponent<ArmorVisual>();if(visual==null)visual=actor.AddComponent<ArmorVisual>();
        var r=Segment(rig.torso,"ArmorVest",sprites[0],11,Color.white).GetComponent<SpriteRenderer>();r.enabled=false;
        visual.overlay=r;visual.intact=sprites[0];visual.damaged=sprites[1];visual.broken=sprites[2];
    }
    public static void Apply()
    {
        var atlas=Atlas();var faces=Faces();var larper=Skin(atlas,0,faces);var samurai=Skin(atlas,1,faces);var armor=ArmorArt();
        samurai.weapon=samurai.meleeWeapon=Katana();EditorUtility.SetDirty(samurai);
        var sword=AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/SamuraiSword.asset");var swordData=new SerializedObject(sword);swordData.FindProperty("equippedSprite").objectReferenceValue=samurai.weapon;swordData.ApplyModifiedPropertiesWithoutUndo();
        foreach(string path in new[]{"Assets/Prefabs/Enemies/Larper.prefab","Assets/Prefabs/Enemies/Samurai.prefab"})
        {var root=PrefabUtility.LoadPrefabContents(path);try{Rig(root,path.Contains("Samurai")?samurai:larper,!path.Contains("Samurai"));PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}}
        // Fireworks has its own functioning presentation; opt out of inherited Larper articulation.
        const string fireworks="Assets/Prefabs/Enemies/FireworksExpert.prefab";var expert=PrefabUtility.LoadPrefabContents(fireworks);try{var rig=expert.GetComponentInChildren<CutoutRig>(true);var data=new SerializedObject(rig);data.FindProperty("segmented").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();foreach(var j in rig.GetComponents<SamuraiArticulatedRig>())Object.DestroyImmediate(j);foreach(var t in new[]{rig.leftArm,rig.rightArm,rig.leftLeg,rig.rightLeg})foreach(Transform child in t.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);rig.ApplySkin(rig.skin);PrefabUtility.SaveAsPrefabAsset(expert,fireworks);}finally{PrefabUtility.UnloadPrefabContents(expert);}
        foreach(string path in new[]{"Assets/Scenes/Warehouse_Demo.unity","Assets/Scenes/Warehouse_Mission01.unity","Assets/Scenes/Warehouse_Testbed.unity"})
        {
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();foreach(var a in all.Select(t=>t.GetComponent<BodyArmor>()).Where(a=>a!=null))Armor(a.gameObject,armor);
                var mission=all.Select(t=>t.GetComponent<WarehouseMission>()).FirstOrDefault(m=>m!=null);
                if(mission!=null&&!all.Any(t=>t.name=="Mission_OfficeCheckpoint"))
                {
                    var data=new SerializedObject(mission);var records=all.Single(t=>t.GetComponent<MissionTrigger>()?.Zone==MissionZone.Records);
                    var old=records.GetComponent<InsaneDreams.Checkpoints.CheckpointMarker>();
                    var go=new GameObject("Mission_OfficeCheckpoint");go.layer=2;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,scene);go.transform.position=new Vector3(15.3f,6.4f,0);
                    var collider=go.AddComponent<BoxCollider2D>();collider.isTrigger=true;collider.size=new Vector2(1.3f,2);
                    var marker=go.AddComponent<InsaneDreams.Checkpoints.CheckpointMarker>();EditorUtility.CopySerialized(old,marker);
                    foreach(string name in new[]{"Sprint0242_Flag","CheckpointBeacon"}){var visual=records.Find(name);if(visual!=null){visual.SetParent(go.transform,false);var cue=visual.GetComponent<WorldMarkerCue>();if(cue!=null)cue.checkpoint=marker;}}
                    var flag=go.GetComponentsInChildren<SpriteRenderer>(true).First(r=>r.name=="Flag");var markerData=new SerializedObject(marker);markerData.FindProperty("markerVisual").objectReferenceValue=flag;markerData.ApplyModifiedPropertiesWithoutUndo();marker.enabled=false;
                    var spawn=(Transform)data.FindProperty("officeSpawn").objectReferenceValue;spawn.position=new Vector3(14.5f,6.25f,0);
                    data.FindProperty("officeCheckpoint").objectReferenceValue=marker;data.ApplyModifiedPropertiesWithoutUndo();Object.DestroyImmediate(old);
                    var trigger=go.AddComponent<MissionTrigger>();var zone=new SerializedObject(trigger);zone.FindProperty("mission").objectReferenceValue=mission;zone.FindProperty("zone").enumValueIndex=(int)MissionZone.OfficeCheckpoint;zone.ApplyModifiedPropertiesWithoutUndo();
                }
                foreach(var t in all.Where(t=>t.name=="Mission_OfficeCheckpoint"))t.gameObject.layer=2;
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }finally{EditorSceneManager.CloseScene(scene,true);}
        }
        AssetDatabase.SaveAssets();
    }
}
