using System;
using System.Linq;
using System.IO;
using InsaneDreams.CameraSystem;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Level;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Explicit authored layout, not a procedural level generator. Invoke only for Sprint0028.
public static class FirstOperationSetup
{
    public const string Folder="Assets/Data/FirstOperation", ScenePath="Assets/Scenes/Warehouse_Demo.unity";
    static Transform terrain,backdrop,encounters;
    static void Edit(Object o,Action<SerializedObject> action){var s=new SerializedObject(o);action(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    static GameObject New(string name,Vector2 p,Transform parent=null){var g=new GameObject(name);g.transform.position=p;if(parent!=null)g.transform.SetParent(parent,true);return g;}
    static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/LevelArt0026/"+name+".asset");
    static SpriteRenderer Visual(string name,Sprite sprite,Vector2 p,Vector2 size,int order,Transform parent)
    {var r=New(name,p,parent).AddComponent<SpriteRenderer>();r.sprite=sprite;r.drawMode=SpriteDrawMode.Tiled;r.size=size;r.sortingOrder=order;return r;}
    static GameObject Solid(string name,float x,float top,float width,float height,Sprite sprite)
    {var r=Visual(name,sprite,new Vector2(x,top-height/2),new Vector2(width,height),0,terrain);r.gameObject.AddComponent<BoxCollider2D>().size=r.size;r.gameObject.AddComponent<ImpactSurface2D>().Configure(ImpactMaterial.Concrete);return r.gameObject;}
    static void Cover(string name,float x,float floor,int variant=0)
    {string[] art={"CrateWoodBrace_32x32","CrateTinCorners_32x32","CrateRustPanel_52x29"};var s=Art(art[variant%3]);Solid(name,x,floor+s.bounds.size.y,s.bounds.size.x,s.bounds.size.y,s);}
    static void Ladder(string name,float x,float low,float high,bool right=true)
    {var g=New(name,new Vector2(x,(low+high+.7f)/2),terrain);var b=g.AddComponent<BoxCollider2D>();b.isTrigger=true;b.size=new Vector2(1,high-low+.7f);var l=g.AddComponent<LadderVolume>();Edit(l,s=>s.FindProperty("topExitHorizontalOffset").floatValue=right?1.8f:-1.8f);g.AddComponent<LadderVisual2D>().Configure(low,high+.7f,Art("Ladder"),Art("LadderFoot"),Art("LadderTop"));}
    static void Sign(string text,float x,float y,float size=.2f)
    {var g=New("Sign_"+text.Replace('\n','_'),new Vector2(x,y),backdrop);var t=g.AddComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=64;t.characterSize=size*.14f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.72f,.75f,.68f);var r=g.GetComponent<MeshRenderer>();r.sharedMaterial=t.font.material;r.sortingOrder=-10;}
    static void Detail(string sprite,float x,float y)
    {var s=AssetDatabase.LoadAllAssetsAtPath(HandmadeEnvironmentDetailsSetup.Sheet).OfType<Sprite>().Single(p=>p.name==sprite);Visual(sprite,s,new Vector2(x,y),s.bounds.size,-16,backdrop);}
    static MissionTrigger Zone(WarehouseMission mission,MissionZone zone,float x)
    {var g=New("Operation_"+zone,new Vector2(x,5));g.layer=2;var b=g.AddComponent<BoxCollider2D>();b.isTrigger=true;b.size=new Vector2(2,12);var z=g.AddComponent<MissionTrigger>();Edit(z,s=>{s.FindProperty("mission").objectReferenceValue=mission;s.FindProperty("zone").enumValueIndex=(int)zone;});return z;}
    static void Supply(float x)
    {var g=New("OperationSupply_"+x,new Vector2(x,.4f),terrain);var b=g.AddComponent<BoxCollider2D>();b.isTrigger=true;b.size=new Vector2(2.5f,1.5f);var c=g.AddComponent<SupplyCrate2D>();Edit(c,s=>{s.FindProperty("ductTape").floatValue=60;s.FindProperty("ammunition").intValue=108;});var r=g.AddComponent<SpriteRenderer>();r.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint0242/AmmoSupplyCrate.asset");r.sortingOrder=2;}
    static T Copy<T>(string source,string name)where T:ScriptableObject
    {string path=Folder+"/"+name+".asset";var value=AssetDatabase.LoadAssetAtPath<T>(path);if(value==null){value=Object.Instantiate(AssetDatabase.LoadAssetAtPath<T>(source));value.name=name;AssetDatabase.CreateAsset(value,path);Edit(value,s=>{var id=s.FindProperty("stableId");if(id!=null)id.stringValue=Guid.NewGuid().ToString("N");});}return value;}
    static Sprite Pixels(string name,int w,int h,Vector2 pivot,Func<int,int,Color> draw)
    {string path=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old!=null)return old;var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};for(int y=0;y<h;y++)for(int x=0;x<w;x++)t.SetPixel(x,y,draw(x,y));t.Apply();AssetDatabase.CreateAsset(t,path);var s=Sprite.Create(t,new Rect(0,0,w,h),pivot,32,0,SpriteMeshType.FullRect);s.name=name;AssetDatabase.AddObjectToAsset(s,t);return s;}
    static Sprite Segment(Sprite full,string name,float low,float high)
    {string path=Folder+"/"+name+".asset";var s=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(s!=null)return s;var rect=full.rect;rect.y+=full.rect.height*low;rect.height=full.rect.height*(high-low);s=Sprite.Create(full.texture,rect,new Vector2(.5f,1),32,0,SpriteMeshType.FullRect);s.name=name;AssetDatabase.CreateAsset(s,path);return s;}
    static void Variants()
    {
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Data","FirstOperation");
        var gun=Copy<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset","OfficerPistol");Edit(gun,s=>{s.FindProperty("displayName").stringValue="Officer's Impossibly Important Pistol";s.FindProperty("damage").floatValue=18;s.FindProperty("firingInterval").floatValue=1.4f;s.FindProperty("magazineCapacity").intValue=6;});
        var skin=Copy<CharacterSkin>("Assets/Data/Visuals/LarperCandidate.asset","OfficerSkin");Color ink=new Color32(25,29,43,255),cloth=new Color32(45,64,72,255),gold=new Color32(185,156,79,255),skinColor=new Color32(255,214,165,255),shoe=new Color32(80,43,12,255);
        skin.head=Pixels("OfficerHead",12,11,new Vector2(.5f,.5f),(x,y)=>y>=9?ink:y==8?(x>=1&&x<=10?ink:Color.clear):y==7&&x==6?gold:skinColor);
        skin.torso=Pixels("OfficerTorso",12,17,Vector2.one*.5f,(x,y)=>x==0||x==11?Color.clear:y<=2?ink:x==6?ink:x>=8&&x<=9&&y>=11&&y<=12?gold:cloth);
        skin.arm=Pixels("OfficerArm",5,18,new Vector2(.5f,1),(x,y)=>x==0?Color.clear:y>=9?cloth:skinColor);
        skin.leg=Pixels("OfficerLeg",6,18,new Vector2(.5f,1),(x,y)=>x==0?Color.clear:y<3?shoe:cloth);
        skin.provenance="Independent flat-pixel Officer recipe; same Larper module bounds and pivots. Historical Postava 1 inspected as reference only; no source pixels copied. Awaiting artistic/playtest review.";EditorUtility.SetDirty(skin);
        string officer=Folder+"/Officer.prefab";
        if(AssetDatabase.LoadAssetAtPath<GameObject>(officer)==null)
        {var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemies/Larper.prefab");try{root.name="Officer";root.AddComponent<OfficerShotTelegraph>();var rig=root.GetComponentInChildren<CutoutRig>();rig.ApplySkin(skin);var joint=rig.GetComponent<SamuraiArticulatedRig>();if(joint!=null){joint.upperArm=Segment(skin.arm,"OfficerUpperArm",.48f,1);joint.forearm=Segment(skin.arm,"OfficerForearm",.16f,.5f);joint.hand=Segment(skin.arm,"OfficerHand",0,.18f);joint.thigh=Segment(skin.leg,"OfficerThigh",.48f,1);joint.shin=Segment(skin.leg,"OfficerShin",.16f,.5f);joint.foot=Segment(skin.leg,"OfficerFoot",0,.18f);joint.BindSprites();}Edit(root.GetComponent<EnemyCombat>(),s=>{s.FindProperty("firearm").objectReferenceValue=gun;s.FindProperty("preferredDistance").floatValue=7;s.FindProperty("reactionSeconds").floatValue=.7f;s.FindProperty("combatRepositionSeconds").vector2Value=new Vector2(2.5f,4.5f);s.FindProperty("combatRepositionChance").floatValue=.55f;s.FindProperty("ordinaryReloadFraction").floatValue=.5f;});PrefabUtility.SaveAsPrefabAsset(root,officer);}finally{PrefabUtility.UnloadPrefabContents(root);}}
        var archetype=Copy<EnemyArchetype>("Assets/Data/Enemies/Larper.asset","Officer");Edit(archetype,s=>s.FindProperty("prefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(officer));
        string rocket=Folder+"/OperationFirework.prefab";
        if(AssetDatabase.LoadAssetAtPath<GameObject>(rocket)==null){var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemies/CartoonFirework.prefab");try{Edit(root.GetComponent<FireworkProjectile>(),s=>{s.FindProperty("damage").floatValue=26;s.FindProperty("radius").floatValue=2.8f;s.FindProperty("shortFuse").vector2Value=new Vector2(.65f,.9f);s.FindProperty("normalFuse").vector2Value=new Vector2(1.1f,1.8f);s.FindProperty("longFuse").vector2Value=new Vector2(2.2f,3.2f);s.FindProperty("shortFuseChance").floatValue=.12f;});PrefabUtility.SaveAsPrefabAsset(root,rocket);}finally{PrefabUtility.UnloadPrefabContents(root);}}
        string expert=Folder+"/OperationFireworksExpert.prefab";
        if(AssetDatabase.LoadAssetAtPath<GameObject>(expert)==null){var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemies/FireworksExpert.prefab");try{Edit(root.GetComponent<FireworksCombat>(),s=>{s.FindProperty("projectilePrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(rocket).GetComponent<FireworkProjectile>();s.FindProperty("preparationSeconds").floatValue=1.05f;s.FindProperty("recoverySeconds").floatValue=2.1f;s.FindProperty("preferredRange").floatValue=8;});PrefabUtility.SaveAsPrefabAsset(root,expert);}finally{PrefabUtility.UnloadPrefabContents(root);}}
        var fireworks=Copy<EnemyArchetype>("Assets/Data/Enemies/FireworksExpert.asset","OperationFireworks");Edit(fireworks,s=>s.FindProperty("prefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(expert));AssetDatabase.SaveAssets();
    }
    static MissionEncounter Group(string name,(string kind,float x,float floor,string personality)[] positions,Health target)
    {
        var g=New(name,Vector2.zero,encounters);var group=g.AddComponent<MissionEncounter>();var points=new EnemySpawnPoint[positions.Length];
        for(int i=0;i<positions.Length;i++)
        {
            var p=positions[i];string asset=p.kind=="Officer"||p.kind=="OperationFireworks"?Folder+"/"+p.kind+".asset":"Assets/Data/Enemies/"+p.kind+".asset";var kind=AssetDatabase.LoadAssetAtPath<EnemyArchetype>(asset);var actor=(GameObject)PrefabUtility.InstantiatePrefab(kind.Prefab,g.scene);actor.name=name+"_"+p.kind+"_"+i;actor.transform.SetParent(g.transform);var box=actor.GetComponent<BoxCollider2D>();Vector2 at=new Vector2(p.x,p.floor+box.size.y*.5f-box.offset.y+.03f);actor.transform.position=at;actor.GetComponent<Rigidbody2D>().position=at;Edit(actor.GetComponent<LarperBrain>(),s=>s.FindProperty("archetype").objectReferenceValue=kind);
            if(p.kind=="Larper")Edit(actor.GetComponent<EnemyCombat>(),s=>{s.FindProperty("reactionSeconds").floatValue=.7f;s.FindProperty("reactionJitterSeconds").floatValue=.12f;});
            var point=New(name+"_Point_"+i,at,g.transform).AddComponent<EnemySpawnPoint>();points[i]=point;Edit(point,s=>{s.FindProperty("archetype").objectReferenceValue=kind;s.FindProperty("spawnOnStart").boolValue=false;s.FindProperty("placedActor").objectReferenceValue=actor.GetComponent<LarperBrain>();s.FindProperty("spawnedParent").objectReferenceValue=g.transform;s.FindProperty("personality").objectReferenceValue=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+p.personality+".asset");});
        }
        Edit(group,s=>{var a=s.FindProperty("points");a.arraySize=points.Length;for(int i=0;i<points.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=points[i];});return group;
    }
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first");Variants();var scene=EditorSceneManager.OpenScene(ScenePath);if(scene.isDirty)throw new InvalidOperationException("Scene dirty");
        if(scene.GetRootGameObjects().Any(g=>g.name=="FirstOperationTerrain"))throw new InvalidOperationException("Operation already authored; inspect/edit rather than regenerating");
        var roots=scene.GetRootGameObjects();string[] keep={"Player","CameraRig","MobileControls","UIEventSystem","LevelEntry","WarehouseIncident","MissionStart"};foreach(var r in roots)if(!keep.Contains(r.name))Object.DestroyImmediate(r);
        terrain=New("FirstOperationTerrain",Vector2.zero).transform;backdrop=New("FirstOperationScenery",Vector2.zero).transform;encounters=New("FirstOperationEncounters",Vector2.zero).transform;
        // Continuous walkable floor: no death pits or mandatory precision jumps.
        Solid("LoadingFloor",-12,0,32,1,Art("Concrete_1728x32"));Solid("StorageFloor",22,0,36,1,Art("Concrete_1728x32"));Solid("PursuitFloor",57,0,34,1,Art("Concrete_1728x32"));Solid("FireworkFloor",92,0,36,1,Art("Concrete_1728x32"));Solid("CommandFloor",134.5f,0,49,1,Art("Concrete_1728x32"));
        Solid("BoundaryWest",-28.5f,13,1,14,Art("Boundary_32x928"));Solid("BoundaryEast",159.5f,13,1,14,Art("Boundary_32x928"));Solid("BoundaryCeiling",65.5f,14,188,1,Art("Boundary_1792x32"));
        Solid("StorageLowDeck",21,3.2f,14,.5f,Art("Walkway_276x16"));Solid("StorageHighDeck",31.5f,6.4f,11,.5f,Art("Walkway_276x16"));Ladder("StorageWestLadder",13.3f,0,3.2f);Ladder("StorageUpperLadder",25.3f,3.2f,6.4f);
        Solid("SamuraiStep",55.5f,1.6f,3,.5f,Art("Walkway_84x13"));Solid("SamuraiDeck",66,3.2f,12,.5f,Art("Walkway_276x16"));Ladder("PursuitDeckLadder",59.3f,0,3.2f);
        Solid("FireworkLookout",87.5f,6.4f,7,.5f,Art("Walkway_160x12"));Solid("FireworkLoadingDeck",99.5f,3.2f,11,.5f,Art("Walkway_276x16"));Ladder("FireworkLoadingLadder",93.3f,0,3.2f);Ladder("FireworkUpperLadder",91.7f,0,6.4f,false);
        Solid("OfficeFlank",128,3.2f,9,.5f,Art("Walkway_276x16"));Ladder("OfficeFlankLadder",122.8f,0,3.2f);
        Solid("OfficerDais",144.25f,1.6f,17.5f,.5f,Art("Walkway_276x16"));
        Cover("EntranceCover",-21,0);Cover("ArrivalRetreatA",-12,0,1);Cover("ArrivalRetreatB",-4,0,2);
        Cover("StorageFloorCover",29,0,2);Cover("StorageLowCover",21.5f,3.2f);Cover("StorageHighCover",32.5f,6.4f,1);
        Cover("PursuitEntryCover",45,0,1);Cover("PursuitRetreat",69.5f,0);Cover("PursuitDeckCover",70,3.2f,2);
        Cover("FireworkEntryCover",79,0,2);Cover("FireworkAllyCover",88.5f,0);Cover("FireworkExitCover",108,0,1);Cover("ExpertCover",102,3.2f,1);
        Cover("OfficeApproachA",114,0);Cover("OfficeApproachB",125,0,2);Cover("OfficerBarricade",138,1.6f,2);Cover("OfficerRetreatCover",146,1.6f,1);
        for(int i=0;i<5;i++){float center=new[]{-12,22,57,92,134}[i];Visual("WallBay_"+i,Art("Masonry_128x144"),new Vector2(center,6.2f),new Vector2(new[]{32,36,34,36,49}[i],13),-30,backdrop);foreach(float dx in new[]{-7f,7f})Visual("BayBeam",Art("Support_8x832"),new Vector2(center+dx,6.2f),new Vector2(.25f,13),-20,backdrop);Visual("BayWindow",Art("Window_96x64"),new Vector2(center,8.5f),new Vector2(3,2),-18,backdrop);}
        Detail("NopeWarning",-20,3.2f);Detail("ElectricalBox",-5,2.8f);Detail("PipeValve",11,4);Detail("StorageShelf",33,2.7f);Detail("BoxFan",50,5);Detail("ConcreteRepair",62,7);Detail("WarningLamp",79,4);Detail("StorageShelf",103,7.5f);Detail("FineWarning",118,4.5f);Detail("ElectricalBox",149,3.2f);
        Detail("WarningLamp",-15,3);Detail("BoxFan",-9,6);Detail("ConcreteRepair",23,8);Detail("Painted02",27,9);Detail("PipeValve",44,4);Detail("OldStain",57,2.5f);Detail("NopeWarning",96,6.5f);Detail("PipeValve",82,3.4f);Detail("ElectricalBox",108,5);Detail("StorageShelf",135,7);Detail("WarningLamp",143,4);
        var shadow=AssetDatabase.LoadAllAssetsAtPath(HandmadeEnvironmentDetailsSetup.Sheet).OfType<Sprite>().Single(p=>p.name=="HardShadow");foreach(var item in new[]{(21f,2.575f,14f),(31.5f,5.775f,11f),(66f,2.575f,12f),(99.5f,2.575f,11f),(128f,2.575f,9f)})Visual("DeckShadow",shadow,new Vector2(item.Item1,item.Item2),new Vector2(item.Item3,.25f),-15,backdrop);
        Sign("01 / GOODS IN\nNO TRAINING PROVIDED",-19,5);Sign("02 / STORAGE\nPROMOTION REQUIRES A LADDER",17,9);Sign("03 / PURSUIT BAY\nNO RUNNING. EXCEPT HIM.",50,9);Sign("04 / FIREWORK STORAGE\nPROBABLY NOT FLAMMABLE",83,9);Sign("05 / COMMAND\nMANAGEMENT IS TAKING COVER",122,9);Sign("DISPATCH / EXIT >",153,3);
        Supply(2);Supply(38);Supply(73);Supply(112);Supply(132);
        var mission=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<WarehouseMission>()).Single();var player=mission.Player;var start=scene.GetRootGameObjects().Single(g=>g.name=="MissionStart").transform;start.position=new Vector2(-24,.75f);player.transform.position=start.position;
        var groups=new[]{Group("01_Warmup",new[]{("Larper",-18.5f,0f,"Ordinary"),("Larper",-8f,0f,"Cowardly"),("Larper",0f,0f,"Overconfident")},player.GetComponent<Health>()),Group("02_Verticality",new[]{("Larper",18f,3.2f,"Cowardly"),("Larper",31f,0f,"Ordinary"),("Larper",35f,6.4f,"Overconfident")},player.GetComponent<Health>()),Group("03_Samurai",new[]{("Samurai",58f,0f,"Samurai"),("Larper",64f,0f,"Cowardly"),("Larper",68f,3.2f,"Ordinary")},player.GetComponent<Health>()),Group("04_Fireworks",new[]{("OperationFireworks",94.8f,3.2f,"FireworksExpert"),("Larper",86f,0f,"Ordinary"),("Larper",106f,0f,"Cowardly")},player.GetComponent<Health>()),Group("05_Officer",new[]{("Officer",140f,1.6f,"Ordinary")},player.GetComponent<Health>())};
        Edit(mission,s=>{s.FindProperty("firstOperation").boolValue=true;var a=s.FindProperty("encounters");a.arraySize=5;for(int i=0;i<5;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=groups[i];});
        Zone(mission,MissionZone.Entrance,-22);Zone(mission,MissionZone.Storage,5);Zone(mission,MissionZone.SamuraiArena,41);Zone(mission,MissionZone.FireworksArena,75);Zone(mission,MissionZone.Command,111);Zone(mission,MissionZone.Exit,155);
        Visual("DispatchDoor",Art("ExitDoor_42x64"),new Vector2(155,1),new Vector2(1.3125f,2),-5,backdrop);
        var camera=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<WarehouseCameraFollow>()).Single();Edit(camera,s=>{s.FindProperty("levelMin").vector2Value=new Vector2(-28,-4);s.FindProperty("levelMax").vector2Value=new Vector2(159,13);});
        var level=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<LevelEntry>()).Single();Edit(level,s=>{var points=encounters.GetComponentsInChildren<EnemySpawnPoint>();var a=s.FindProperty("encounters");a.arraySize=points.Length;for(int i=0;i<points.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=points[i];});
        Edit(player.GetComponent<PlayerHealthRecovery>(),s=>s.FindProperty("allowCheckpointRespawn").boolValue=false);Edit(player.GetComponent<PlayerRevive>(),s=>s.FindProperty("initialInjections").intValue=0);
        var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");var entry=catalog.levels.Single(l=>l.scene=="Warehouse_Demo");entry.title="THE FIRST OPERATION";entry.description="Loading yard, storage decks, one very committed swordsman, questionable fireworks and management's last stand. No checkpoints; bring supplies.";EditorUtility.SetDirty(catalog);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "First Operation authored: five arenas, 10 Larpers + Samurai + Fireworks + Officer, no checkpoints; Demo only.";
    }
}





