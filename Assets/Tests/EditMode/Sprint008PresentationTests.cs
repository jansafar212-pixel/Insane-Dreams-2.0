using System;
using System.IO;
using System.Linq;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint008PresentationTests
    {
        [Test] public void ActiveSkinsAndRagdollUseIndependentSpritesWithSafePixelImports()
        {
            foreach(string path in new[]{"Assets/Prefabs/Player/WarehousePlayer.prefab","Assets/Prefabs/Enemies/Larper.prefab","Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab","Assets/Prefabs/Visuals/LarperCutoutVisual.prefab","Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab"})
            {
                var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);var rig=root.GetComponentInChildren<CutoutRig>(true);
                var renderers=rig!=null?rig.GetComponentsInChildren<SpriteRenderer>(true):root.GetComponentsInChildren<SpriteRenderer>(true);
                foreach(var renderer in renderers)
                {
                    string sprite=AssetDatabase.GetAssetPath(renderer.sprite);Assert.IsTrue(sprite.StartsWith("Assets/Art/Sprint008/")||sprite.StartsWith("Assets/Art/Sprint009/")||sprite.StartsWith("Assets/Art/Sprint012/")||sprite.StartsWith("Assets/Art/Sprint0245/")||sprite.StartsWith("Assets/Art/Sprint0246/")||sprite.StartsWith("Assets/Art/Sprint0247/")||sprite.StartsWith("Assets/Art/Sprint0248/")||sprite.StartsWith("Assets/Art/FocusedHotfix/")||sprite.StartsWith("Assets/Art/CameraSamuraiHotfix/"),path+" "+renderer.name);
                    if(sprite.StartsWith("Assets/Art/Sprint0245/")||sprite.StartsWith("Assets/Art/Sprint0246/")||sprite.StartsWith("Assets/Art/Sprint0247/")||sprite.StartsWith("Assets/Art/Sprint0248/")||sprite.StartsWith("Assets/Art/FocusedHotfix/")||sprite.StartsWith("Assets/Art/CameraSamuraiHotfix/")){Assert.AreEqual(FilterMode.Point,renderer.sprite.texture.filterMode);Assert.AreEqual(32,renderer.sprite.pixelsPerUnit);Assert.AreEqual(1,renderer.sprite.texture.mipmapCount);continue;}
                    var importer=(TextureImporter)AssetImporter.GetAtPath(sprite);Assert.AreEqual(FilterMode.Point,importer.filterMode);Assert.AreEqual(32,importer.spritePixelsPerUnit);
                    Assert.IsFalse(importer.mipmapEnabled);Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
                }
            }
        }
        [Serializable] private sealed class Snapshot { public Entry[] colliders; }
        [Serializable] private sealed class Entry { public string name,type,position,scale,offset,size; public bool trigger; }
        [Test] public void WarehouseArtAddsNoCollisionAndPreservesPreSprintGeometryAndPracticeBindings()
        {
            var baseline=JsonUtility.FromJson<Snapshot>(File.ReadAllText("TestResults/Sprint008GeometryBaseline.json"));
            var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Testbed.unity");
            try
            {
                var roots=scene.GetRootGameObjects();var art=roots.Single(g=>g.name=="WarehouseArt_Sprint008");Assert.IsEmpty(art.GetComponentsInChildren<Collider2D>(true));
                var allColliders=roots.SelectMany(r=>r.GetComponentsInChildren<Collider2D>(true)).ToArray();
                Assert.AreEqual(7,allColliders.Count(c=>c.GetComponentInParent<InsaneDreams.Enemies.LarperBrain>()!=null)); // Sprint022.2 authors the existing startup residents.
                Assert.AreEqual(2,allColliders.Count(c=>c.GetComponentInParent<InsaneDreams.Inventory.SupplyCrate2D>()!=null&&c.isTrigger));
                var colliders=allColliders.Where(c=>c.GetComponentInParent<InsaneDreams.Enemies.LarperBrain>()==null&&c.GetComponentInParent<InsaneDreams.Inventory.SupplyCrate2D>()==null).ToArray();Assert.AreEqual(baseline.colliders.Length+1,colliders.Length); // Preserve original solid geometry independently of the two pickup triggers.
                foreach(var before in baseline.colliders)
                {
                    var c=colliders.Single(x=>x.name==before.name);Assert.AreEqual(before.type,c.GetType().Name);
                    Assert.AreEqual(before.position,c.transform.position.ToString("F5"),c.name);Assert.AreEqual(before.scale,c.transform.lossyScale.ToString("F5"),c.name);
                    Assert.AreEqual(before.offset,c.offset.ToString("F5"),c.name);Assert.AreEqual(before.trigger,c.isTrigger,c.name);
                    if(c is BoxCollider2D box)Assert.AreEqual(before.size,box.size.ToString("F5"),c.name);
                }
                var station=roots.Single(g=>g.name=="RagdollPractice").GetComponent<InsaneDreams.Level.RagdollPracticeStation>();var settings=new SerializedObject(station);
                foreach(string property in new[]{"larperPrefab","groundTarget","dropTarget","document"})Assert.IsNotNull(settings.FindProperty(property).objectReferenceValue,property);
            }
            finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
