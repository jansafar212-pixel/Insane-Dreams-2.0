using System.Linq;
using InsaneDreams.CameraSystem;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint013RulesTests
    {
        [Test] public void BodyBoundsRespectGameplayProjectionDepthEdgesAndCulling()
        {
            var go=new GameObject("ProjectionFixture");
            try{
                var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.aspect=1;camera.orthographicSize=5;go.transform.position=Vector3.back*10;
                Assert.IsTrue(GameplayViewport.Overlaps(camera,new Bounds(new Vector3(5.2f,0),new Vector3(.8f,1.4f,.1f)),2));
                Assert.IsFalse(GameplayViewport.Overlaps(camera,new Bounds(new Vector3(5.5f,0),new Vector3(.8f,1.4f,.1f)),2));
                Assert.IsFalse(GameplayViewport.Overlaps(camera,new Bounds(new Vector3(0,0,-20),Vector3.one),2));
                camera.cullingMask=1;Assert.IsFalse(GameplayViewport.Overlaps(camera,new Bounds(Vector3.zero,Vector3.one),2));
            }finally{Object.DestroyImmediate(go);}
        }
        [Test] public void CameraTuningAndExplicitWarehouseSurfacesPreservePistolAndSceneContracts()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab");var ai=new SerializedObject(prefab.GetComponent<EnemyCombat>());
            var tracer=prefab.GetComponentInChildren<LineRenderer>();Assert.AreEqual("Sprites/Default",tracer.sharedMaterial.shader.name);Assert.AreEqual(Color.white,tracer.sharedMaterial.color);
            Assert.AreEqual(22,ai.FindProperty("detectionRange").floatValue);Assert.AreEqual(18,ai.FindProperty("engagementRange").floatValue);Assert.AreEqual(40,ai.FindProperty("distantSpread").floatValue);Assert.AreEqual(6,ai.FindProperty("withdrawalDistance").floatValue);
            var pistol=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");Assert.AreEqual(27,pistol.MagazineCapacity);Assert.AreEqual(.105f,pistol.FiringInterval);Assert.AreEqual(4,pistol.ReloadDuration);Assert.AreEqual(5,pistol.Damage);
            var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Testbed.unity");
            try{
                var colliders=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider2D>(true)).ToArray();Assert.AreEqual(31,colliders.Length);
                foreach(var c in colliders.Where(c=>!c.isTrigger&&c.gameObject.layer==0))Assert.IsNotNull(c.GetComponent<ImpactSurface2D>(),c.name);
                Assert.AreEqual(ImpactMaterial.Wood,colliders.Single(c=>c.name=="Crate_21").GetComponent<ImpactSurface2D>().Material);
                Assert.AreEqual(ImpactMaterial.Metal,colliders.Single(c=>c.name=="BasementPipeBridge").GetComponent<ImpactSurface2D>().Material);
                Assert.AreEqual(ImpactMaterial.Concrete,colliders.Single(c=>c.name=="Ground_Exit").GetComponent<ImpactSurface2D>().Material);
            }finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
