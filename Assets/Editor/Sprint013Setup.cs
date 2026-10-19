using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;

public static class Sprint013Setup
{
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode first.");
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype/CombatTracer.mat");
        material.shader=Shader.Find("Sprites/Default");material.shaderKeywords=System.Array.Empty<string>();material.color=Color.white;material.renderQueue=3000;EditorUtility.SetDirty(material);
        foreach(string path in new[]{"Assets/Prefabs/Enemies/Larper.prefab","Assets/Prefabs/Player/WarehousePlayer.prefab"})
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var combat=root.GetComponent<EnemyCombat>();
                if(combat!=null){var s=new SerializedObject(combat);s.FindProperty("detectionRange").floatValue=22;s.FindProperty("engagementRange").floatValue=18;s.ApplyModifiedPropertiesWithoutUndo();}
                var tracer=new SerializedObject(root.GetComponent<AttackExecutor>());
                tracer.FindProperty("tracerSeconds").floatValue=.17f;tracer.FindProperty("tracerWidth").floatValue=.04f;
                tracer.FindProperty("friendlyTracer").colorValue=new Color(.3f,.95f,1,.95f);tracer.FindProperty("enemyTracer").colorValue=new Color(1,.4f,.2f,.95f);tracer.ApplyModifiedPropertiesWithoutUndo();
                var feedback=new SerializedObject(root.GetComponent<CombatFeedback>());feedback.FindProperty("hurtSeconds").floatValue=.1f;feedback.FindProperty("hurtColor").colorValue=new Color(1,.75f,.6f);feedback.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/Warehouse_Testbed.unity")throw new System.InvalidOperationException("Open Warehouse_Testbed for surface authoring.");
        int count=0;
        foreach(var root in scene.GetRootGameObjects())foreach(var collider in root.GetComponentsInChildren<Collider2D>(true))
        {
            if(collider.isTrigger||collider.gameObject.layer!=0)continue;
            var marker=collider.GetComponent<ImpactSurface2D>();if(marker==null)marker=collider.gameObject.AddComponent<ImpactSurface2D>();
            string n=collider.name;
            marker.Configure(n.StartsWith("Crate_")||n.StartsWith("LowCover_")?ImpactMaterial.Wood:
                n.StartsWith("Mezzanine")||n.StartsWith("Roof")||n.StartsWith("UpperStep")||n.Contains("Pipe")||n=="CrouchPassage"?ImpactMaterial.Metal:ImpactMaterial.Concrete);
            EditorUtility.SetDirty(marker);count++;
        }
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Camera tuning22/18; both combat prefabs updated; "+count+" solid colliders classified. Audio slots intentionally unassigned: suitable project clips absent.";
    }
}
