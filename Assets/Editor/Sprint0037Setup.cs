using System;
using System.Linq;
using System.IO;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class Sprint0037Setup
{
    const string Evidence="TestResults/Sprint0037";
    public static string CombatSnapshot()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(AssetDatabase.FindAssets("t:WeaponDefinition").Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).Select(p=>
        {
            var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(p);var s=new SerializedObject(w);
            return new{path=p,w.Damage,w.FiringInterval,w.MagazineCapacity,w.ReloadDuration,w.AttackRange,w.SpreadDegrees,w.BloomPerShot,w.MaximumBloom,w.Pellets,w.FireMode,w.ImpactImpulse,w.CustomTracer,tracerColor=w.TracerColor.ToString(),falloff=s.FindProperty("falloffStart").floatValue,farMultiplier=s.FindProperty("farDamageMultiplier").floatValue};
        }).ToArray(),Newtonsoft.Json.Formatting.Indented);
    }
    static void Actor(GameObject g)
    {
        if(g.GetComponent<Health>()==null||g.GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>()==null)return;
        var list=g.GetComponents<HitReaction2D>();var reaction=list.FirstOrDefault()??g.AddComponent<HitReaction2D>();foreach(var extra in list.Skip(1))Object.DestroyImmediate(extra,true);
        bool player=g.GetComponent<InsaneDreams.Input.PlayerActions>()!=null;
        bool samurai=g.GetComponent<InsaneDreams.Enemies.SamuraiCombat>()!=null;
        bool officer=g.GetComponent<InsaneDreams.Enemies.OfficerShotTelegraph>()!=null;
        var s=new SerializedObject(reaction);s.FindProperty("livingPhysics").boolValue=true;
        s.FindProperty("stumbleImpulse").floatValue=officer?1.5f:1.65f;s.FindProperty("knockdownImpulse").floatValue=officer?7:samurai?6:4.5f;
        s.FindProperty("knockdownChance").floatValue=player?0:1;s.FindProperty("maximumJointAngle").floatValue=player?8:officer?18:samurai?20:26;
        s.FindProperty("susceptibility").floatValue=officer?.8f:samurai?.85f:1;
        s.FindProperty("recoverySeconds").floatValue=samurai?.5f:officer?.55f:.65f;s.FindProperty("cooldown").floatValue=3;
        s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(reaction);
        if(player)
        {
            var feedback=g.GetComponents<PlayerDamageFeedback>();if(feedback.Length==0)g.AddComponent<PlayerDamageFeedback>();foreach(var extra in feedback.Skip(1))Object.DestroyImmediate(extra,true);
        }
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Preserve scene changes and stop Play first");
        Directory.CreateDirectory(Evidence);string baseline=CombatSnapshot();File.WriteAllText(Evidence+"/CombatBefore.json",baseline);
        var catalog=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");
        foreach(var w in catalog.weapons)
        {
            float strength=w.name=="SMG"||w.name=="LarperBuzzbox"?.4f:w.name=="Shotgun"?.5f:w.name=="DesertEagle"?2.6f:w.name=="Sniper"?3.2f:w.name=="LMG"?1.35f:w.name=="BackupPistol"?.65f:1.1f;
            var s=new SerializedObject(w);s.FindProperty("hitReactionStrength").floatValue=strength;
            if(w.name=="DesertEagle")s.FindProperty("tracerScale").floatValue=1.6f;if(w.name=="Sniper")s.FindProperty("tracerScale").floatValue=2.2f;
            s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(w);
        }
        string[] prefabs={"Assets/Prefabs/Player/WarehousePlayer.prefab","Assets/Prefabs/Enemies/Larper.prefab","Assets/Prefabs/Enemies/Samurai.prefab","Assets/Prefabs/Enemies/FireworksExpert.prefab","Assets/Data/FirstOperation/Officer.prefab","Assets/Data/FirstOperation/OperationFireworksExpert.prefab"};
        foreach(string path in prefabs)
        {
            var root=PrefabUtility.LoadPrefabContents(path);try{Actor(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        foreach(string path in new[]{"Assets/Scenes/Warehouse_Demo.unity","Assets/Scenes/Warehouse_Mission01.unity","Assets/Scenes/Warehouse_Testbed.unity"})
        {
            var scene=EditorSceneManager.OpenScene(path);foreach(var h in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Health>(true)))Actor(h.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();string after=CombatSnapshot();File.WriteAllText(Evidence+"/CombatAfter.json",after);if(baseline!=after)throw new Exception("Unexpected combat-balance mutation");
        EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");return "Existing reaction owner configured on six prefabs/three scenes; DE1.6/sniper2.2; combat tuning unchanged";
    }
}
