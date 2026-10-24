using System.Linq;
using InsaneDreams.Visuals;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class Sprint0248Setup
{
    private const string Art="Assets/Art/Sprint0248/";
    private static readonly Color Ink=new Color32(25,29,43,255),Steel=new Color32(111,123,130,255),Paper=new Color32(255,255,235,255),Wood=new Color32(181,132,65,255),Yellow=new Color32(247,201,44,255),Red=new Color32(194,43,22,255),Shirt=new Color32(173,60,149,255),Peach=new Color32(255,214,165,255);
    private static Sprite Draw(string name,int w,int h,Vector2 pivot,System.Func<int,int,Color> pixel)
    {
        var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};for(int y=0;y<h;y++)for(int x=0;x<w;x++)t.SetPixel(x,y,pixel(x,y));t.Apply();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+"Pixels.asset");if(texture==null){texture=t;AssetDatabase.CreateAsset(t,Art+name+"Pixels.asset");}else{EditorUtility.CopySerialized(t,texture);Object.DestroyImmediate(t);}
        var s=Sprite.Create(texture,new Rect(0,0,w,h),pivot,32,0,SpriteMeshType.FullRect);s.name=name;var old=AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".asset");if(old==null){AssetDatabase.CreateAsset(s,Art+name+".asset");return s;}EditorUtility.CopySerialized(s,old);Object.DestroyImmediate(s);return old;
    }
    private static void Float(Object target,string name,float value){var so=new SerializedObject(target);so.FindProperty(name).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    private static void Reference(Object target,string name,Object value){var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    public static void Apply()
    {
        if(!AssetDatabase.IsValidFolder(Art.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Art","Sprint0248");
        var carbine=Draw("ScrapCarbine",30,11,new Vector2(7f/30,5f/11),(x,y)=>
        {if(x<7&&y>=3&&y<=7)return y==3||x==0?Ink:Wood;if(x>=7&&x<21&&y>=4&&y<=8)return y==8?Steel:x==7||y==4?Ink:Steel;if(x>=21&&y>=6&&y<=7)return y==7?Steel:Ink;if(x>=9&&x<=11&&y>=1&&y<=4)return x==9?Ink:Wood;if(x>=15&&x<=18&&y<=4)return x==15||y==0?Ink:Steel;if((x==18||x==8)&&y==9)return Ink;return Color.clear;});
        var flash=Draw("CarbineFlash",15,9,new Vector2(0,.5f),(x,y)=>
        {int dy=Mathf.Abs(y-4);if(x<3&&dy<2||x>=3&&x<9&&dy<=3-x/4||x>=9&&dy==0)return x<6?Paper:Yellow;if(x<7&&(y==0||y==8)&&x>2)return Red;return Color.clear;});
        var head=Draw("CheapHardhat",16,13,new Vector2(.5f,.5f),(x,y)=>
        {if(y>=9){if(y>=12&&(x<4||x>11)||y==11&&(x<2||x>13))return Color.clear;return y==9||x==8?Yellow:new Color32(255,226,44,255);}if(x<2||x>13)return Color.clear;if(y==3&&(x<6||x>9)||y==2&&(x==4||x==11))return Ink;return Peach;});
        var shirt=Draw("FireworksShirt",14,17,new Vector2(.5f,.5f),(x,y)=>
        {if(y==16&&(x<2||x>10))return Color.clear;if(x>=5&&x<=8&&y>=5&&y<=9)return x==5||y==5?Ink:Yellow;if(y==10&&x==7||y==11&&x==8)return Paper;if(y==12&&(x==6||x==9)||y==11&&x==10)return Yellow;if(y==3&&x>2&&x<11&&x%3!=0)return Paper;return Shirt;});
        var arm=Draw("FireworksSleeve",5,18,new Vector2(.5f,1),(x,y)=>y>11?Shirt:Peach);
        var old=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/Fireworks0247.asset");var skin=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/Fireworks0248.asset");if(skin==null){skin=Object.Instantiate(old);skin.name="Fireworks0248";AssetDatabase.CreateAsset(skin,"Assets/Data/Visuals/Fireworks0248.asset");}skin.head=head;skin.torso=shirt;skin.arm=arm;skin.provenance="Sprint 024.8 primitive native-pixel shirt/scribbled bomb logo and oversized cheap hardhat; Larper/Enemak Graphics Bible.";EditorUtility.SetDirty(skin);
        var weapon=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");Reference(weapon,"equippedSprite",carbine);
        Float(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset"),"firingInterval",.07f);
        foreach(string n in new[]{"Samurai","FireworksExpert","WarehousePlayer"})
        {string path="Assets/Prefabs/"+(n=="WarehousePlayer"?"Player/":"Enemies/")+n+".prefab";var g=PrefabUtility.LoadPrefabContents(path);try{var r=g.GetComponentInChildren<CutoutRig>(true);if(n=="Samurai"){Float(r,"visualScale",1.05f);FitBody(g,r);}if(n=="FireworksExpert"){r.ApplySkin(skin);Float(g.GetComponent<FireworksCombat>(),"heldMalfunctionChance",.08f);}if(n=="WarehousePlayer"){Reference(g.GetComponent<InsaneDreams.Combat.CombatFeedback>(),"muzzleFlashSprite",flash);Reference(g.GetComponent<InsaneDreams.Combat.CombatFeedback>(),"muzzleFlashWeapon",weapon);r.weapon.sprite=carbine;}r.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);if(n=="Samurai")r.Segmented.Pose(CharacterPose.Idle,0,15,true,0);PrefabUtility.SaveAsPrefabAsset(g,path);}finally{PrefabUtility.UnloadPrefabContents(g);}}
        const string bundle="Assets/Prefabs/Enemies/CartoonFirework.prefab";var b=PrefabUtility.LoadPrefabContents(bundle);try{Float(b.GetComponent<FireworkProjectile>(),"radius",2.7f);PrefabUtility.SaveAsPrefabAsset(b,bundle);}finally{PrefabUtility.UnloadPrefabContents(b);}
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity",OpenSceneMode.Additive);
        try
        {
            var mission=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<WarehouseMission>(true)).Single();
            var original=mission.Encounters[1].Points.Single(p=>p.name=="Storage_Spawn_1");
            var actorPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab");float half=actorPrefab.GetComponent<BoxCollider2D>().size.y*.5f;
            Place(original,new Vector2(-4.8f,half+.015f));
            Add(mission.Encounters[1],original,"Sprint0248_Storage_Samurai",new Vector2(-.8f,5.4f+half+.015f),scene);
            Add(mission.Encounters[2],original,"Sprint0248_Processing_Samurai",new Vector2(23,half+.015f),scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }finally{EditorSceneManager.CloseScene(scene,true);}
        foreach(string path in new[]{"Assets/Scenes/Warehouse_Demo.unity","Assets/Scenes/Warehouse_Testbed.unity","Assets/Scenes/Warehouse_Mission01.unity"})
        {var s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);try{foreach(var c in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<InsaneDreams.CameraSystem.WarehouseCameraFollow>(true)))Float(c,"directionTransitionSeconds",.75f);EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);}finally{EditorSceneManager.CloseScene(s,true);}}
        AssetDatabase.SaveAssets();
    }
    private static void Place(EnemySpawnPoint point,Vector2 position)
    {point.transform.position=position;var actor=(LarperBrain)new SerializedObject(point).FindProperty("placedActor").objectReferenceValue;actor.transform.position=position;}
    private static void FitBody(GameObject actor,CutoutRig rig)
    {
        // Rebase the existing visual around its actual standing body bounds. Keeping
        // the collider centered also preserves navigation's center/half-height model.
        // The held sword is deliberately excluded from this body hurtbox.
        rig.transform.localPosition=Vector3.up*rig.GroundAlignmentOffset;
        rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);
        rig.Segmented.Pose(CharacterPose.Idle,0,15,true,0);
        var bounds=rig.head.GetComponent<SpriteRenderer>().bounds;
        foreach(var t in rig.BodyParts.Concat(rig.Segmented.ExtraParts))bounds.Encapsulate(t.GetComponent<SpriteRenderer>().bounds);
        var center=actor.transform.InverseTransformPoint(bounds.center);var data=new SerializedObject(rig);data.FindProperty("presentationOffset").vector2Value=-(Vector2)center;data.ApplyModifiedPropertiesWithoutUndo();
        rig.transform.localPosition-=center;
        var box=actor.GetComponent<BoxCollider2D>();box.size=bounds.size;box.offset=Vector2.zero;
        var hitbox=actor.GetComponent<SamuraiBodyHitbox>();if(hitbox==null)hitbox=actor.AddComponent<SamuraiBodyHitbox>();
        var child=actor.transform.Find("VisibleBodyHitbox");if(child==null){child=new GameObject("VisibleBodyHitbox").transform;child.SetParent(actor.transform,false);}child.gameObject.layer=actor.layer;
        var visual=child.GetComponent<BoxCollider2D>();if(visual==null)visual=child.gameObject.AddComponent<BoxCollider2D>();visual.excludeLayers=~0;visual.isTrigger=false;
        hitbox.Configure(rig,box,visual);
        actor.GetComponent<Rigidbody2D>().centerOfMass=Vector2.zero;
    }
    private static void Add(MissionEncounter group,EnemySpawnPoint template,string name,Vector2 position,UnityEngine.SceneManagement.Scene scene)
    {
        var existing=group.Points.FirstOrDefault(p=>p.name==name);if(existing!=null)
        {var placed=(LarperBrain)new SerializedObject(existing).FindProperty("placedActor").objectReferenceValue;foreach(var duplicate in existing.GetComponentsInChildren<LarperBrain>(true).Where(a=>a!=placed).ToArray())Object.DestroyImmediate(duplicate.gameObject);Place(existing,position);return;}
        // Copy point settings only. Instantiating an authored point also clones its
        // child resident, creating an unregistered duplicate in addition to our prefab.
        var go=new GameObject(name);go.transform.SetParent(group.transform,false);var point=go.AddComponent<EnemySpawnPoint>();EditorUtility.CopySerialized(template,point);
        var actor=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),scene);actor.name=name+"_Actor";
        Reference(point,"placedActor",actor.GetComponent<LarperBrain>());Place(point,position);
        var data=new SerializedObject(group);var points=data.FindProperty("points");int index=points.arraySize++;points.GetArrayElementAtIndex(index).objectReferenceValue=point;data.ApplyModifiedPropertiesWithoutUndo();
    }
}
