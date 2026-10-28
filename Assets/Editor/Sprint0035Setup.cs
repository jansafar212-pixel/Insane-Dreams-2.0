using System;
using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class Sprint0035Setup
{
    static void Edit(Object obj,Action<SerializedObject> edit)
    {var s=new SerializedObject(obj);edit(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj);}
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Stop Play Mode and preserve unsaved work first.");
        var pistol=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset");
        // The existing ordinary pistol sprite already contains its short grip/magazine.
        // Only the mistakenly inherited Larper extended overlay is removed.
        Edit(pistol,s=>{s.FindProperty("magazineCapacity").intValue=15;s.FindProperty("magazineSprite").objectReferenceValue=null;});
        var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");
        catalog.developmentPlaytestCredits=true;catalog.Find("pistol-ammo").quantity=15;EditorUtility.SetDirty(catalog);
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");
        var mission=Object.FindAnyObjectByType<WarehouseMission>();
        foreach(var group in mission.Encounters)
        {
            var keep=group.Points.Where(p=>!p.name.StartsWith("CombatHotfix_")).ToArray();
            foreach(var p in group.Points.Except(keep).ToArray())
            {
                var actor=new SerializedObject(p).FindProperty("placedActor").objectReferenceValue as Component;
                if(actor!=null)Object.DestroyImmediate(actor.gameObject);
                Object.DestroyImmediate(p.gameObject);
            }
            Edit(group,s=>{var a=s.FindProperty("points");a.arraySize=keep.Length;for(int i=0;i<keep.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=keep[i];});
        }
        var entry=Object.FindAnyObjectByType<InsaneDreams.Level.LevelEntry>();
        var points=mission.Encounters.SelectMany(g=>g.Points).ToArray();
        Edit(entry,s=>{var a=s.FindProperty("encounters");a.arraySize=points.Length;for(int i=0;i<points.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=points[i];});
        var officer=Object.FindAnyObjectByType<OfficerShotTelegraph>();
        var rear=GameObject.Find("CowardsBunker").transform.Find("ConcreteRearWall");
        // Existing rear upper wall/sprite, open doorway from floor 1.6 to lintel 3.6.
        const float top=7.45f,bottom=3.6f;
        rear.position=new Vector3(rear.position.x,(top+bottom)*.5f,rear.position.z);
        rear.GetComponent<BoxCollider2D>().size=new Vector2(.375f,top-bottom);
        rear.GetComponent<SpriteRenderer>().size=new Vector2(.375f,top-bottom);
        var exit=rear.parent.Find("OfficerRearExit");
        if(exit==null){exit=new GameObject("OfficerRearExit").transform;exit.SetParent(rear.parent);}
        exit.position=new Vector2(149.7f,2.33f);
        Edit(officer,s=>{s.FindProperty("bunkerExit").objectReferenceValue=exit;s.FindProperty("nearbyAllies").arraySize=0;});
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return points.Length+" actors; 15-round ordinary pistol; rear egress; development floor enabled.";
    }
}
