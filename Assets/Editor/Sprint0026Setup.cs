using System;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Explicit, idempotent Editor authoring; never runs automatically on reload.
public static class Sprint0026Setup
{
    public static readonly Vector2[] CoverPositions={new Vector2(3.5f,.5f),new Vector2(-5.5f,5.9f),new Vector2(13.3f,12.5f)};
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
        const string prefab="Assets/Prefabs/Enemies/Larper.prefab";
        var actor=PrefabUtility.LoadPrefabContents(prefab);
        try
        {
            var combat=new SerializedObject(actor.GetComponent<EnemyCombat>());
            combat.FindProperty("panicEscapeSuppression").floatValue=55;
            combat.FindProperty("panicEscapeCooldown").vector2Value=new Vector2(2,3);
            combat.FindProperty("sustainedNearMisses").intValue=2;
            combat.FindProperty("sustainedSuppressionSeconds").floatValue=.3f;
            combat.ApplyModifiedPropertiesWithoutUndo();
            var awareness=new SerializedObject(actor.GetComponent<LarperAwareness>());
            awareness.FindProperty("combatCalloutSeconds").floatValue=2.8f;awareness.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(actor,prefab);
        }
        finally{PrefabUtility.UnloadPrefabContents(actor);}
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity",OpenSceneMode.Additive);
        try
        {
            var roots=scene.GetRootGameObjects();
            var parent=roots.FirstOrDefault(g=>g.name=="Sprint0026_Cover");
            if(parent==null){parent=new GameObject("Sprint0026_Cover");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(parent,scene);}
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint008/ShippingCrate.png");
            if(sprite==null)throw new InvalidOperationException("Existing crate sprite unavailable.");
            for(int i=0;i<CoverPositions.Length;i++)
            {
                string name="TacticalCrate_"+i;var t=parent.transform.Find(name);
                if(t!=null){t.position=CoverPositions[i];continue;}
                var g=new GameObject(name);g.transform.SetParent(parent.transform);g.transform.position=CoverPositions[i];
                var box=g.AddComponent<BoxCollider2D>();box.size=Vector2.one;
                var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.drawMode=SpriteDrawMode.Tiled;sr.size=Vector2.one;sr.sortingOrder=2;
                g.AddComponent<ImpactSurface2D>().Configure(ImpactMaterial.Wood);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        finally{EditorSceneManager.CloseScene(scene,true);}
        AssetDatabase.SaveAssets();return "Larper threshold55/cooldown2–3/callout2.8; three existing-style crates added to Demo ground, mezzanine and processing roof.";
    }
}
