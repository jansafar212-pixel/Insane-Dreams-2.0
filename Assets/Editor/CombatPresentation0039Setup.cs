using System;
using System.Linq;
using InsaneDreams.Combat;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Native authoring only. Call ApplyScene inside the sprint's centrally managed save.
public static class CombatPresentation0039Setup
{
    const string Folder="Assets/Art/CombatPresentation0039",Path=Folder+"/PixelTracer.mat";
    static Material Material()
    {
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","CombatPresentation0039");
        var material=AssetDatabase.LoadAssetAtPath<Material>(Path);if(material!=null)return material;
        var shader=Shader.Find("AKUMU/PixelTracer");if(shader==null)throw new InvalidOperationException("PixelTracer shader must compile before authoring");
        material=new Material(shader){name="PixelTracer"};AssetDatabase.CreateAsset(material,Path);return material;
    }
    static void Actor(GameObject root,Material material)
    {
        foreach(var attack in root.GetComponentsInChildren<AttackExecutor>(true))
        {
            var data=new SerializedObject(attack);data.FindProperty("tracerMaterial").objectReferenceValue=material;data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(attack);
        }
        foreach(var feedback in root.GetComponentsInChildren<CombatFeedback>(true))
        {
            var health=feedback.GetComponent<Health>();if(health==null||health.Team!=CombatTeam.Enemy)continue;
            var data=new SerializedObject(feedback);data.FindProperty("hurtColor").colorValue=new Color(1,.18f,.12f,1);data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(feedback);
        }
    }
    public static int ApplyPrefabs()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first");
        var material=Material();int changed=0;
        foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs","Assets/Data/FirstOperation"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab.GetComponentInChildren<AttackExecutor>(true)==null)continue;
            var root=PrefabUtility.LoadPrefabContents(path);try{Actor(root,material);PrefabUtility.SaveAsPrefabAsset(root,path);changed++;}finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        AssetDatabase.SaveAssets();return changed;
    }
    public static string ApplyScene(Scene scene)
    {
        var material=Material();int wood=0,metal=0,concrete=0;
        foreach(var root in scene.GetRootGameObjects())
        {
            Actor(root,material);
            foreach(var collider in root.GetComponentsInChildren<Collider2D>(true))
            {
                if(collider.isTrigger||collider.GetComponentInParent<Health>()!=null)continue;
                var renderer=collider.GetComponent<SpriteRenderer>();if(renderer==null||renderer.sprite==null)continue;
                string art=renderer.sprite.name;
                // Classify the existing rendered modules, not broad gameplay names.
                ImpactMaterial? kind=art.StartsWith("CrateWood",StringComparison.Ordinal)?ImpactMaterial.Wood:
                    art.StartsWith("CrateTin",StringComparison.Ordinal)||art.StartsWith("CrateRust",StringComparison.Ordinal)||art.StartsWith("Walkway",StringComparison.Ordinal)?ImpactMaterial.Metal:
                    art.StartsWith("Concrete",StringComparison.Ordinal)||art.StartsWith("Boundary",StringComparison.Ordinal)||art=="Sandbags"?ImpactMaterial.Concrete:(ImpactMaterial?)null;
                if(!kind.HasValue)continue;
                var surface=collider.GetComponent<ImpactSurface2D>();if(surface==null)surface=collider.gameObject.AddComponent<ImpactSurface2D>();surface.Configure(kind.Value);EditorUtility.SetDirty(surface);
                if(kind==ImpactMaterial.Wood)wood++;else if(kind==ImpactMaterial.Metal)metal++;else concrete++;
            }
        }
        return scene.name+": wood="+wood+", metal="+metal+", concrete="+concrete;
    }
}
