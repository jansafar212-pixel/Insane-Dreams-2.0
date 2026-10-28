#if UNITY_EDITOR
using System;
using System.IO;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint024ProgressTests
    {
        [Test] public void ActiveAndSavedBuildProfilesCannotBypassCampaignWithTestbedOnlyScenes()
        {
            var effective=UnityEditor.EditorBuildSettings.scenes;
            Assert.AreEqual("Assets/Scenes/MainMenu.unity",System.Array.Find(effective,s=>s.enabled).path);
            Assert.IsTrue(System.Array.Exists(effective,s=>s.enabled&&s.path=="Assets/Scenes/Warehouse_Demo.unity"));
            foreach(string guid in UnityEditor.AssetDatabase.FindAssets("t:BuildProfile"))
            {
                string path=UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var profile=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Build.Profile.BuildProfile>(path);
                var data=new UnityEditor.SerializedObject(profile);
                Assert.IsFalse(data.FindProperty("m_OverrideGlobalSceneList").boolValue,path+" must inherit the campaign scene list.");
            }
        }
        [Test] public void AtomicSaveReloadKeepsMoneyAndCompletionWithoutOverwritingUnknownFutureVersion()
        {
            string dir=Path.Combine(Path.GetTempPath(),"Akumu024-"+Guid.NewGuid().ToString("N")),path=Path.Combine(dir,"progress.json");
            try
            {
                var store=new ProgressStore(path);var data=store.Load(()=>new GameProgress());
                data.balance=93;data.completed.Add("warehouse");Assert.IsTrue(store.Save(data));
                data.balance=85;Assert.IsTrue(store.Save(data));Assert.IsTrue(File.Exists(path+".bak"));
                var loaded=new ProgressStore(path).Load(()=>new GameProgress());Assert.AreEqual(85,loaded.balance);CollectionAssert.Contains(loaded.completed,"warehouse");
                File.WriteAllText(path,"{\"version\":99,\"balance\":777}");
                var future=new ProgressStore(path);future.Load(()=>new GameProgress());Assert.IsTrue(future.ReadOnly);
                Assert.IsFalse(future.Save(data));StringAssert.Contains("777",File.ReadAllText(path));
            }
            finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void InvalidResourceValuesNormalizeWithoutNegativeBalances()
        {
            var p=new GameProgress{balance=-1,tape=float.NaN,armor=float.PositiveInfinity,joystickScale=100};
            p.Normalize();Assert.AreEqual(0,p.balance);Assert.AreEqual(200,p.tape);Assert.AreEqual(0,p.armor);Assert.AreEqual(1.5f,p.joystickScale);
        }
        [Test] public void DemoHasBoundActorsStagedSpecialistsAndOriginalMissionRemainsSeparate()
        {
            var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Demo.unity");
            try
            {
                var brains=new System.Collections.Generic.List<InsaneDreams.Enemies.LarperBrain>();int samurai=0,fireworks=0;
                foreach(var root in scene.GetRootGameObjects())
                {
                    brains.AddRange(root.GetComponentsInChildren<InsaneDreams.Enemies.LarperBrain>(true));
                    samurai+=root.GetComponentsInChildren<InsaneDreams.Enemies.SamuraiCombat>(true).Length;fireworks+=root.GetComponentsInChildren<InsaneDreams.Enemies.FireworksCombat>(true).Length;
                    foreach(var t in root.GetComponentsInChildren<Transform>(true))Assert.AreEqual(0,UnityEditor.GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject),t.name);
                }
                Assert.AreEqual(15,brains.Count);Assert.AreEqual(1,samurai);Assert.AreEqual(1,fireworks);
                InsaneDreams.Missions.WarehouseMission mission=null;foreach(var root in scene.GetRootGameObjects()){mission=root.GetComponentInChildren<InsaneDreams.Missions.WarehouseMission>();if(mission!=null)break;}
                Assert.IsNotNull(mission);
                foreach(var point in mission.Encounters[0].Points)Assert.IsNull(point.Archetype.Prefab.GetComponent<InsaneDreams.Enemies.SamuraiCombat>());
                Assert.IsNotNull(mission.Encounters[2].Points[0].Archetype.Prefab.GetComponent<InsaneDreams.Enemies.SamuraiCombat>());
                Assert.IsNotNull(mission.Encounters[3].Points[0].Archetype.Prefab.GetComponent<InsaneDreams.Enemies.FireworksCombat>());
                foreach(var brain in brains)Assert.IsTrue(brain.GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true).IsBound);
            }
            finally{UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);}
            var catalog=UnityEditor.AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");
            Assert.AreEqual(1,catalog.levels.Length);Assert.AreEqual("Warehouse_Demo",catalog.levels[0].scene);Assert.IsNotNull(catalog.levels[0].icon);
            Assert.AreEqual("Assets/Scenes/MainMenu.unity",UnityEditor.EditorBuildSettings.scenes[0].path);
            var menu=UnityEditor.SceneManagement.EditorSceneManager.OpenPreviewScene("Assets/Scenes/MainMenu.unity");
            try
            {
                UnityEngine.UIElements.UIDocument doc=null;foreach(var root in menu.GetRootGameObjects()){doc=root.GetComponentInChildren<UnityEngine.UIElements.UIDocument>();if(doc!=null)break;}
                Assert.IsNotNull(doc);Assert.IsNotNull(doc.panelSettings);Assert.IsNotNull(doc.visualTreeAsset);
            }
            finally{UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(menu);}
        }
    }
}
#endif
