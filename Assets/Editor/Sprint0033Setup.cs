using System;
using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Missions;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Sprint0033Setup
{
    public const float Left=140,Right=149,Floor=1.6f,RoofTop=7.825f;
    static void Resize(Transform root,string name,float x,float y,float w,float h)
    {
        var t=root.Find(name);if(t==null)throw new InvalidOperationException("Missing approved bunker part: "+name);
        t.position=new Vector2(x,y);var r=t.GetComponent<SpriteRenderer>();r.drawMode=SpriteDrawMode.Tiled;r.size=new Vector2(w,h);
        var c=t.GetComponent<BoxCollider2D>();if(c!=null)c.size=r.size;
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Stop Play Mode and compilation first");
        var current=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(current.isDirty)throw new InvalidOperationException("Preserve unsaved scene first");
        var s=current.path=="Assets/Scenes/Warehouse_Demo.unity"?current:EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");
        var roots=s.GetRootGameObjects();var root=roots.Single(g=>g.name=="CowardsBunker").transform;
        Resize(root,"ConcreteFrontLower",140.1875f,2.05f,.375f,.9f);
        Resize(root,"ConcreteFrontUpper",140.1875f,5.675f,.375f,3.55f);
        Resize(root,"ConcreteRearWall",148.8125f,4.525f,.375f,5.85f);
        Resize(root,"ConcreteRoof",144.5f,7.6375f,9,.375f);
        Resize(root,"ConcreteBase",144.5f,1.5375f,9,.125f);
        Resize(root,"DarkInterior",144.5f,4.525f,9,5.85f);
        // Simple side cutaway: keep solid exterior edges readable but remove the
        // full-width foreground mask that used to hide the Officer's limbs/gun.
        Resize(root,"FrontFaceLower",144.5f,1.6625f,9,.125f);
        Resize(root,"FrontFaceUpper",144.5f,7.2f,9,.5f);
        // Stay just inside the existing grounded wall-probe stopping distance.
        // The longer approach otherwise stops .18u short of the old anchor.
        var peek=root.Find("OfficerFiringHolePosition");peek.position=new Vector2(141.25f,2.33f);
        var hide=root.Find("OfficerInteriorHide");hide.position=new Vector2(146.5f,2.33f);
        var officer=roots.SelectMany(g=>g.GetComponentsInChildren<OfficerShotTelegraph>(true)).Single();
        var so=new SerializedObject(officer);so.FindProperty("embrasureHeight").vector2Value=new Vector2(2.5f,3.9f);so.ApplyModifiedPropertiesWithoutUndo();
        var point=roots.SelectMany(g=>g.GetComponentsInChildren<WarehouseMission>()).Single().Encounters[4].Points[0];point.transform.position=hide.position;
        officer.transform.position=hide.position;officer.GetComponent<Rigidbody2D>().position=hide.position;
        var terrain=roots.Single(g=>g.name=="FirstOperationTerrain").transform;
        var ladder=terrain.Find("PillboxRoofAccessLadder");
        if(ladder==null){ladder=new GameObject("PillboxRoofAccessLadder").transform;ladder.SetParent(terrain,false);}
        float top=RoofTop+.7f;ladder.position=new Vector2(139,(Floor+top)*.5f);
        var volume=ladder.GetComponent<BoxCollider2D>();if(volume==null)volume=ladder.gameObject.AddComponent<BoxCollider2D>();volume.isTrigger=true;volume.size=new Vector2(1,top-Floor);
        var climb=ladder.GetComponent<LadderVolume>();if(climb==null)climb=ladder.gameObject.AddComponent<LadderVolume>();
        var settings=new SerializedObject(climb);settings.FindProperty("topExitHorizontalOffset").floatValue=1.8f;settings.ApplyModifiedPropertiesWithoutUndo();
        var visual=ladder.GetComponent<LadderVisual2D>();if(visual==null)visual=ladder.gameObject.AddComponent<LadderVisual2D>();
        Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/LevelArt0026/"+name+".asset");
        visual.Configure(Floor,top,Art("Ladder"),Art("LadderFoot"),Art("LadderTop"));
        Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();
        return "Saved real9x6.35 shell (2x width/height),8.25x5.85 clear interior,1.4-high firing slit, cutaway fascia,146.5 hide anchor and existing-system roof ladder. All original surroundings/roster retained.";
    }
}
