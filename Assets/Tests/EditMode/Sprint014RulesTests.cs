using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Level;
using InsaneDreams.Missions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
namespace InsaneDreams.Tests
{
    public sealed class Sprint014RulesTests
    {
        private const string Path="Assets/Scenes/Warehouse_Mission01.unity";
        [Test] public void MissionIsSeparateWithStagedPrefabsValidReferencesAndMissionUiOverride()
        {
            var scene=EditorSceneManager.OpenPreviewScene(Path);
            try{
                var roots=scene.GetRootGameObjects();var mission=roots.SelectMany(g=>g.GetComponentsInChildren<WarehouseMission>()).Single();var data=new SerializedObject(mission);
                foreach(string field in new[]{"player","initialSpawn","storageSpawn","officeSpawn","storageCheckpoint","officeCheckpoint","recordsZone","cameraFollow","hud"})Assert.IsNotNull(data.FindProperty(field).objectReferenceValue,field);
                CollectionAssert.AreEqual(new[]{1,4,5},mission.Encounters.Select(e=>e.Count).ToArray());var points=roots.SelectMany(g=>g.GetComponentsInChildren<EnemySpawnPoint>()).ToArray();Assert.AreEqual(10,points.Length);
                foreach(var point in points){Assert.IsNotNull(point.GetComponentInParent<MissionEncounter>());Assert.IsFalse(new SerializedObject(point).FindProperty("spawnOnStart").boolValue);Assert.IsNull(point.ValidationError);}
                var document=roots.SelectMany(g=>g.GetComponentsInChildren<UIDocument>()).Single();Assert.AreEqual("Assets/UI/MissionControls.uxml",AssetDatabase.GetAssetPath(document.visualTreeAsset));var tree=document.visualTreeAsset.Instantiate();Assert.IsNotNull(tree.Q<Button>("replayMission"));Assert.IsNotNull(tree.Q<Button>("interact"));Assert.IsNotNull(tree.Q("resultsExtras"));
                Assert.AreEqual(0,roots.SelectMany(g=>g.GetComponentsInChildren<RagdollPracticeStation>()).Count());Assert.AreEqual(6,roots.SelectMany(g=>g.GetComponentsInChildren<MissionTrigger>()).Count());
                foreach(var zone in roots.SelectMany(g=>g.GetComponentsInChildren<MissionTrigger>()))Assert.AreEqual(2,zone.gameObject.layer,"Mission zones must not enter solid-geometry probes: "+zone.name);
                var zones=roots.SelectMany(g=>g.GetComponentsInChildren<MissionTrigger>()).ToArray();foreach(var zone in zones)Assert.AreEqual(Vector3.one,zone.transform.lossyScale,"Visuals must not scale trigger volumes.");
                Assert.AreEqual(new Vector2(2,2),(Vector2)zones.Single(z=>z.Zone==MissionZone.Records).Region.bounds.size);
                Assert.AreEqual(new Vector2(1.5f,2),(Vector2)zones.Single(z=>z.Zone==MissionZone.Checkpoint).Region.bounds.size);
                foreach(var root in roots)foreach(var t in root.GetComponentsInChildren<Transform>(true))Assert.AreEqual(0,GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject),t.name);
                var player=roots.SelectMany(g=>g.GetComponentsInChildren<PlayerActions>()).Single();Assert.IsNotNull(PrefabUtility.GetCorrespondingObjectFromSource(player.gameObject));
                Assert.IsTrue(EditorBuildSettings.scenes.Any(s=>s.path==Path&&s.enabled));
            }finally{EditorSceneManager.ClosePreviewScene(scene);}
            var testbed=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Testbed.unity");
            try{Assert.AreEqual(31,testbed.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Collider2D>(true).Length));Assert.AreEqual(1,testbed.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<RagdollPracticeStation>(true).Length));Assert.AreEqual(0,testbed.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<WarehouseMission>(true).Length));}
            finally{EditorSceneManager.ClosePreviewScene(testbed);}
        }
        [Test] public void MissionSpawnsCheckpointsAndSteppedRouteHaveSupportedClearanceWithinExistingJumpLimits()
        {
            var scene=EditorSceneManager.OpenPreviewScene(Path);
            try{
                var roots=scene.GetRootGameObjects();var geometry=roots.SelectMany(g=>g.GetComponentsInChildren<Collider2D>(true)).Where(c=>!c.isTrigger&&c.gameObject.layer==0).ToArray();var mission=roots.SelectMany(g=>g.GetComponentsInChildren<WarehouseMission>()).Single();var data=new SerializedObject(mission);
                var locations=mission.Encounters.SelectMany(e=>e.Points.Select(p=>p.transform.position)).Concat(new[]{"initialSpawn","storageSpawn","officeSpawn"}.Select(name=>((Transform)data.FindProperty(name).objectReferenceValue).position));
                foreach(var p in locations)
                {
                    Assert.IsTrue(geometry.Any(c=>c.bounds.min.x<=p.x-.4f&&c.bounds.max.x>=p.x+.4f&&Mathf.Abs(c.bounds.max.y-(p.y-.7f))<.3f),"Unsupported spawn "+p);
                    var body=new Bounds(p,new Vector3(.78f,1.38f,.01f));Assert.IsFalse(geometry.Any(c=>c.bounds.Intersects(body)),"Obstructed spawn "+p);
                }
                var platforms=new[]{"Mezzanine_Central","UpperStep_01","UpperStep_02","UpperStep_03","Roof_Right"}.Select(n=>geometry.Single(c=>c.name==n)).ToArray();
                for(int i=1;i<platforms.Length;i++){Assert.LessOrEqual(platforms[i].bounds.max.y-platforms[i-1].bounds.max.y,2.45f);Assert.LessOrEqual(platforms[i].bounds.min.x-platforms[i-1].bounds.max.x,3f);}
            }finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
