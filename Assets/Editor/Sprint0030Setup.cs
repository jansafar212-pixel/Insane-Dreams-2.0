using System;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class Sprint0030Setup
{
    static void Equip(GameObject root)
    {
        if(root.GetComponent<BodyArmor>()==null)root.AddComponent<BodyArmor>();
        var officer=new SerializedObject(root.GetComponent<OfficerShotTelegraph>());officer.FindProperty("bodyProtection").floatValue=50;officer.FindProperty("bodyAbsorption").floatValue=.65f;officer.FindProperty("hitRetreatCooldown").floatValue=2.5f;officer.FindProperty("closeHideSeconds").floatValue=.45f;officer.ApplyModifiedPropertiesWithoutUndo();
        var rig=root.GetComponentInChildren<CutoutRig>(true);var overlay=rig.torso.Find("OfficerBallisticVest");if(overlay==null){overlay=new GameObject("OfficerBallisticVest").transform;overlay.SetParent(rig.torso,false);overlay.gameObject.AddComponent<SpriteRenderer>();}
        var r=overlay.GetComponent<SpriteRenderer>();var torso=rig.torso.GetComponent<SpriteRenderer>();r.sortingOrder=torso.sortingOrder+1;r.sortingLayerID=torso.sortingLayerID;r.sharedMaterial=torso.sharedMaterial;r.enabled=false;
        var visual=root.GetComponent<ArmorVisual>();if(visual==null)visual=root.AddComponent<ArmorVisual>();visual.overlay=r;visual.intact=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint0245/Armor_0.asset");visual.damaged=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint0245/Armor_1.asset");EditorUtility.SetDirty(visual);
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Preserve scene and stop Play Mode");
        const string path="Assets/Data/FirstOperation/Officer.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{Equip(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        var s=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");foreach(var o in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<OfficerShotTelegraph>(true)))Equip(o.gameObject);EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();return "Officer finite50-point vest,65%absorption,2.5s hit-retreat debounce,.45s nearby-threat hide cap; existing vest sprites reused; health/weapon/bunker unchanged.";
    }
}
