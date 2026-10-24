#if UNITY_EDITOR
using System.Linq;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0246RulesTests
    {
        [Test]public void ConfiguredEditorEntryIsMainMenuAndBothBuildPlatformsInheritMenuFirst()
        {var old=EditorSceneManager.playModeStartScene;try{var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("CampaignEntry")).First(t=>t!=null);type.GetMethod("UseMainMenuEntry").Invoke(null,null);Assert.AreEqual("Assets/Scenes/MainMenu.unity",AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));Assert.AreEqual("Assets/Scenes/MainMenu.unity",EditorBuildSettings.scenes.First(s=>s.enabled).path);Assert.IsFalse(EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path.Contains("Testbed")));foreach(var guid in AssetDatabase.FindAssets("t:BuildProfile")){var p=AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));Assert.IsFalse(new SerializedObject(p).FindProperty("m_OverrideGlobalSceneList").boolValue);}Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Warehouse_Testbed.unity"));}finally{EditorSceneManager.playModeStartScene=old;}}
        [Test]public void PistolVisualsAndSocketChangeWithoutWeaponBalanceChanges()
        {var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");Assert.AreEqual(5,w.Damage);Assert.AreEqual(.07f,w.FiringInterval);Assert.AreEqual(27,w.MagazineCapacity);Assert.AreEqual(4,w.ReloadDuration);Assert.AreEqual(28,w.AttackRange);Assert.AreEqual(.9f,w.ImpactImpulse);Assert.AreEqual(new Vector2(1f/32,-2f/32),w.MagazineMountOffset);Assert.AreEqual(new Vector2(20,8),w.EquippedSprite.rect.size);Assert.AreEqual(new Vector2(5,13),w.MagazineSprite.rect.size);Assert.AreEqual(32,w.EquippedSprite.pixelsPerUnit);Assert.AreEqual(FilterMode.Point,w.MagazineSprite.texture.filterMode);Assert.LessOrEqual(w.EquippedSprite.texture.GetPixels32().Where(c=>c.a>0).Distinct().Count(),3);}
    }
}
#endif
