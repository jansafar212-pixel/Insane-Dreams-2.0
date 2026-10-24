using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0242RulesTests
    {
        [Test] public void DemoAddedFiveSupportedSpacedResidentsWhileTestbedOpeningAndSpecialistsRemainIntact()
        {
            var s=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Demo.unity");
            try
            {
                var all=s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();var mission=all.Select(t=>t.GetComponent<WarehouseMission>()).Single(m=>m!=null);
                CollectionAssert.AreEqual(new[]{3,6,8},mission.Encounters.Select(g=>g.Count));var points=mission.Encounters.SelectMany(g=>g.Points).ToArray();var added=points.Where(p=>p.name.StartsWith("Sprint0242_")).ToArray();Assert.AreEqual(5,added.Length);Assert.AreEqual(17,all.Count(t=>t.GetComponent<LarperBrain>()!=null));
                var solids=all.Select(t=>t.GetComponent<Collider2D>()).Where(c=>c!=null&&!c.isTrigger&&c.gameObject.layer==0).ToArray();
                foreach(var p in added)
                {
                    var a=(LarperBrain)new SerializedObject(p).FindProperty("placedActor").objectReferenceValue;Assert.IsNotNull(a);Assert.IsTrue(a.gameObject.activeSelf);Assert.IsFalse(new SerializedObject(p).FindProperty("spawnOnStart").boolValue);
                    Assert.IsTrue(solids.Any(c=>c.bounds.min.x<=p.transform.position.x-.32f&&c.bounds.max.x>=p.transform.position.x+.32f&&Mathf.Abs(c.bounds.max.y-(p.transform.position.y-.7f))<.3f),p.name+" requires ground.");
                    foreach(var other in points.Where(q=>q!=p))Assert.Greater(Vector2.Distance(p.transform.position,other.transform.position),1.5f,p.name+" overlaps "+other.name);
                }
                Assert.AreEqual(3,all.Count(t=>t.GetComponent<SamuraiCombat>()!=null));Assert.AreEqual(1,all.Count(t=>t.GetComponent<FireworksCombat>()!=null));
                var passage=new Bounds(new Vector3(12,-.3f),new Vector3(.95f,12.2f,.1f));Assert.IsFalse(solids.Any(c=>c.bounds.Intersects(passage)),"The actual ladder shaft must be clear.");
            }finally{EditorSceneManager.ClosePreviewScene(s);}
            s=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Testbed.unity");try{Assert.AreEqual(7,s.GetRootGameObjects().Sum(r=>r.GetComponentsInChildren<LarperBrain>(true).Length));}finally{EditorSceneManager.ClosePreviewScene(s);}
        }
        [Test] public void BundlesSuppliesAndProgressMarkersHaveSeparateSavedVisualsWithoutLauncher()
        {
            var expert=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/FireworksExpert.prefab");Assert.IsFalse(expert.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="LaunchTube"));
            var projectile=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab");Assert.IsNotNull(projectile.GetComponent<Rigidbody2D>());Assert.IsNotNull(projectile.GetComponent<CircleCollider2D>());
            var bundle=new SerializedObject(projectile.GetComponent<FireworkProjectile>()).FindProperty("rocket").objectReferenceValue as SpriteRenderer;
            var crate=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/SupplyCrate.prefab");Assert.AreNotSame(bundle.sprite,crate.transform.Find("Crate").GetComponent<SpriteRenderer>().sprite);Assert.IsFalse(crate.transform.Find("SupplyBand").gameObject.activeSelf);
            var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Demo.unity");try{Assert.AreEqual(3,scene.GetRootGameObjects().Sum(r=>r.GetComponentsInChildren<WorldMarkerCue>(true).Length));}finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
