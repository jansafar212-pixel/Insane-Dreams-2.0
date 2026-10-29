using System;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class Sprint0040Setup
{
    static void Profile(GameObject root)
    {
        foreach(var reaction in root.GetComponentsInChildren<HitReaction2D>(true))
        {
            if(reaction.GetComponent<Health>().Team!=CombatTeam.Enemy||!reaction.UsesLivingPhysics)continue;
            var s=new SerializedObject(reaction);
            foreach(var pair in new[]{("springReturn",9f),("impulseDegrees",160f),("jointDamping",2f),("springDamping",1f),("maximumAngularSpeed",460f),("maximumJointAngle",34f),("maximumTorsoAngle",26f),("springTorque",1.5f),("responseSeconds",1.1f),("torsoSpill",.45f),("heavyTorsoSpill",.7f)})s.FindProperty(pair.Item1).floatValue=pair.Item2;
            s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(reaction);
        }
        foreach(var load in root.GetComponentsInChildren<PlayerLoadout>(true)){var s=new SerializedObject(load);if(s.FindProperty("selectedSlot").intValue>2)s.FindProperty("selectedSlot").intValue=0;s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(load);}
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first");
        foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs","Assets/Data/FirstOperation"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);var p=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(p.GetComponentInChildren<HitReaction2D>(true)==null&&p.GetComponentInChildren<PlayerLoadout>(true)==null)continue;
            var root=PrefabUtility.LoadPrefabContents(path);try{Profile(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        foreach(string path in LevelArt0026Setup.Scenes)
        {
            var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try{if(scene.isDirty)throw new InvalidOperationException("Preserve unsaved scene changes first: "+path);foreach(var root in scene.GetRootGameObjects())Profile(root);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        AssetDatabase.SaveAssets();return "Two-slot native serialization refreshed; enemy-only springs configured; no weapon balance or AI profile changes";
    }
}
