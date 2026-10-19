using System.Linq;
using InsaneDreams.CameraSystem;
using InsaneDreams.Checkpoints;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Level;
using InsaneDreams.Missions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
public static class Sprint014MissionSetup
{
    public const string MissionPath="Assets/Scenes/Warehouse_Mission01.unity";
    private static void Edit(Object value,System.Action<SerializedObject> action){var data=new SerializedObject(value);action(data);data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(value);}
    private static GameObject New(string name,Vector2 position,Transform parent=null){var go=new GameObject(name);go.transform.position=position;if(parent!=null)go.transform.SetParent(parent,true);return go;}
    private static Sprite Square=>new SerializedObject(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab").GetComponent<CombatFeedback>()).FindProperty("effectSprite").objectReferenceValue as Sprite;
    private static void Sign(string name,string text,Vector2 position,float size=.21f)
    {
        var go=New(name,position);var t=go.AddComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=64;t.characterSize=size*.14f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.7f,.76f,.76f);
        var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=t.font.material;renderer.sortingOrder=1;
    }
    private static MissionTrigger Trigger(WarehouseMission mission,MissionZone zone,Vector2 position,Vector2 size)
    {
        var go=New("Mission_"+zone,position);go.layer=2;var box=go.AddComponent<BoxCollider2D>();box.isTrigger=true;box.size=size;var trigger=go.AddComponent<MissionTrigger>();
        Edit(trigger,s=>{s.FindProperty("mission").objectReferenceValue=mission;s.FindProperty("zone").enumValueIndex=(int)zone;});return trigger;
    }
    private static CheckpointMarker Marker(MissionTrigger trigger,Transform spawn)
    {
        var marker=trigger.gameObject.AddComponent<CheckpointMarker>();marker.enabled=false;
        var visual=New("CheckpointBeacon",trigger.transform.position,trigger.transform).AddComponent<SpriteRenderer>();visual.sprite=Square;visual.drawMode=SpriteDrawMode.Tiled;visual.size=new Vector2(.12f,1.2f);visual.color=new Color(.35f,.5f,.55f);visual.sortingOrder=2;
        Edit(marker,s=>{s.FindProperty("spawnPoint").objectReferenceValue=spawn;s.FindProperty("markerVisual").objectReferenceValue=visual;});return marker;
    }
    private static MissionEncounter Encounter(string name,Vector2[] positions,Transform parent,int personalityOffset)
    {
        var root=New(name,Vector2.zero,parent);var group=root.AddComponent<MissionEncounter>();var points=new EnemySpawnPoint[positions.Length];
        for(int i=0;i<positions.Length;i++)
        {
            var point=New(name+"_Spawn_"+i,positions[i],root.transform).AddComponent<EnemySpawnPoint>();points[i]=point;
            Edit(point,s=>{s.FindProperty("archetype").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset");s.FindProperty("spawnOnStart").boolValue=false;s.FindProperty("spawnedParent").objectReferenceValue=root.transform;s.FindProperty("personality").objectReferenceValue=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+new[]{"Ordinary","Cowardly","Overconfident"}[(i+personalityOffset)%3]+".asset");});
        }
        Edit(group,s=>{var a=s.FindProperty("points");a.arraySize=points.Length;for(int i=0;i<points.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=points[i];});return group;
    }
    public static string Create()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode first.");
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(MissionPath)!=null)throw new System.InvalidOperationException("Mission already exists; edit it in place rather than regenerating.");
        var source=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Testbed.unity");if(source.isDirty)throw new System.InvalidOperationException("Save the testbed before copying it.");
        EditorSceneManager.SaveScene(source,MissionPath,true);return ConfigureCopy();
    }
    public static string ResumePartialCopy()
    {
        var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(active.path!=MissionPath||active.GetRootGameObjects().Any(g=>g.name=="WarehouseIncident"))throw new System.InvalidOperationException("Only an unconfigured mission copy can resume.");
        return ConfigureCopy();
    }
    private static string ConfigureCopy()
    {
        var scene=EditorSceneManager.OpenScene(MissionPath);
        var roots=scene.GetRootGameObjects();var player=roots.SelectMany(g=>g.GetComponentsInChildren<PlayerActions>(true)).Single();var controls=roots.SelectMany(g=>g.GetComponentsInChildren<WarehouseTouchControls>(true)).Single();
        var camera=roots.SelectMany(g=>g.GetComponentsInChildren<WarehouseCameraFollow>(true)).Single();var level=roots.SelectMany(g=>g.GetComponentsInChildren<LevelEntry>(true)).Single();
        foreach(var point in roots.SelectMany(g=>g.GetComponentsInChildren<EnemySpawnPoint>(true)).ToArray())Object.DestroyImmediate(point.gameObject);
        foreach(var checkpoint in roots.SelectMany(g=>g.GetComponentsInChildren<CheckpointMarker>(true)).ToArray())Object.DestroyImmediate(checkpoint.gameObject);
        foreach(var text in roots.Where(g=>g!=null).SelectMany(g=>g.GetComponentsInChildren<TextMesh>(true)).ToArray())if(text!=null)Object.DestroyImmediate(text.gameObject);
        Object.DestroyImmediate(roots.Single(g=>g!=null&&g.name=="Enemies"));Object.DestroyImmediate(roots.Single(g=>g!=null&&g.name=="RagdollPractice"));
        var mission=New("WarehouseIncident",Vector2.zero).AddComponent<WarehouseMission>();var encounterRoot=New("MissionEncounters",Vector2.zero).transform;
        var start=New("MissionStart",new Vector2(-24,.85f)).transform;var storage=New("StorageCheckpointSpawn",new Vector2(1,6.25f)).transform;var office=New("OfficeCheckpointSpawn",new Vector2(18.7f,6.25f)).transform;
        var a=Encounter("Arrival",new[]{new Vector2(-16,.85f)},encounterRoot,2);
        var b=Encounter("Storage",new[]{new Vector2(-7,.85f),new Vector2(3,.85f),new Vector2(-7,6.25f),new Vector2(2,6.25f)},encounterRoot,0);
        var c=Encounter("Processing",new[]{new Vector2(14.7f,12.85f),new Vector2(18.2f,12.85f),new Vector2(20.95f,12.85f),new Vector2(14.5f,6.25f),new Vector2(20.8f,6.25f)},encounterRoot,1);
        Trigger(mission,MissionZone.Entrance,new Vector2(-20,1),new Vector2(1,3));Trigger(mission,MissionZone.Storage,new Vector2(-9.5f,3),new Vector2(1.5f,9));
        var storageZone=Trigger(mission,MissionZone.Checkpoint,new Vector2(2.2f,6.4f),new Vector2(1.5f,2));var storageMarker=Marker(storageZone,storage);
        Trigger(mission,MissionZone.Processing,new Vector2(7.5f,9.5f),new Vector2(2,3));
        var records=Trigger(mission,MissionZone.Records,new Vector2(20,6.25f),new Vector2(2,2));var officeMarker=Marker(records,office);
        var paper=New("CorporateRecordsCase",new Vector2(20,5.85f)).AddComponent<SpriteRenderer>();paper.sprite=Square;paper.drawMode=SpriteDrawMode.Tiled;paper.size=new Vector2(.65f,.45f);paper.color=new Color(.78f,.77f,.64f);paper.sortingOrder=3;
        Trigger(mission,MissionZone.Exit,new Vector2(25,1),new Vector2(1.5f,3));
        var door=New("MissionExitDoor",new Vector2(25,1)).AddComponent<SpriteRenderer>();door.sprite=Square;door.drawMode=SpriteDrawMode.Tiled;door.size=new Vector2(1.3f,2);door.color=new Color(.2f,.3f,.32f);door.sortingOrder=1;
        foreach(var p in new[]{new Vector2(-3,5.9f),new Vector2(16.4f,12.5f),new Vector2(20,12.5f),new Vector2(17.5f,5.9f)})
        {
            var crate=New("MissionCrate_"+p.x+"_"+p.y,p);crate.AddComponent<BoxCollider2D>().size=Vector2.one;var sr=crate.AddComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint008/ShippingCrate.png");sr.drawMode=SpriteDrawMode.Tiled;sr.size=Vector2.one;sr.sortingOrder=2;crate.AddComponent<ImpactSurface2D>().Configure(ImpactMaterial.Wood);
        }
        var document=controls.GetComponent<UIDocument>();document.visualTreeAsset=AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/MissionControls.uxml");PrefabUtility.RecordPrefabInstancePropertyModifications(document);EditorUtility.SetDirty(document);var hud=controls.gameObject.AddComponent<MissionHud>();
        level.Configure(player,start,camera,controls);var points=encounterRoot.GetComponentsInChildren<EnemySpawnPoint>();Edit(level,s=>{var array=s.FindProperty("encounters");array.arraySize=points.Length;for(int i=0;i<points.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=points[i];});
        Edit(player.GetComponent<PlayerRecovery>(),s=>{s.FindProperty("initialSpawn").objectReferenceValue=start;s.FindProperty("followCamera").objectReferenceValue=camera;});player.transform.position=start.position;
        Edit(mission,s=>{s.FindProperty("player").objectReferenceValue=player;s.FindProperty("initialSpawn").objectReferenceValue=start;s.FindProperty("storageSpawn").objectReferenceValue=storage;s.FindProperty("officeSpawn").objectReferenceValue=office;s.FindProperty("storageCheckpoint").objectReferenceValue=storageMarker;s.FindProperty("officeCheckpoint").objectReferenceValue=officeMarker;s.FindProperty("recordsZone").objectReferenceValue=records;s.FindProperty("cameraFollow").objectReferenceValue=camera;s.FindProperty("hud").objectReferenceValue=hud;var array=s.FindProperty("encounters");array.arraySize=3;array.GetArrayElementAtIndex(0).objectReferenceValue=a;array.GetArrayElementAtIndex(1).objectReferenceValue=b;array.GetArrayElementAtIndex(2).objectReferenceValue=c;});
        Sign("ArrivalGuide","ENTRANCE >\nDUCK UNDER THE PIPE",new Vector2(-22,2.4f));Sign("StorageGuide","STORAGE\nMEZZANINE VIA WEST LADDER",new Vector2(-2,3.5f));Sign("SafetyPoster","SAFETY IS OUR NUMBER ONE PRIORITY\nACCIDENT-FREE DAYS: 0",new Vector2(-4,8.5f),.16f);
        Sign("GantryGuide","PROCESSING >\nFOLLOW THE STEPPED GANTRY",new Vector2(3.5f,10.8f),.17f);Sign("OfficeGuide","RECORDS OFFICE\nEXIT: EAST GROUND FLOOR",new Vector2(18.5f,8.8f),.17f);Sign("FeedbackPoster","EMPLOYEE FEEDBACK IS ALWAYS WELCOME*\n*May affect continued employment.",new Vector2(18,3),.15f);Sign("ExitGuide","EXIT",new Vector2(25,2.6f),.25f);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(MissionPath,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=MissionPath)).ToArray();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Mission authored separately: 1/4/5 staged Larpers, six zones, two saved checkpoints, records and exit; testbed unchanged.";
    }
}
