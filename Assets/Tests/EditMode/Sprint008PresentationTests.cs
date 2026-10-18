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
                    string sprite=AssetDatabase.GetAssetPath(renderer.sprite);Assert.IsTrue(sprite.StartsWith("Assets/Art/Sprint008/")||sprite.StartsWith("Assets/Art/Sprint009/")||sprite.StartsWith("Assets/Art/Sprint012/"),path+" "+renderer.name);
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
                var colliders=roots.SelectMany(r=>r.GetComponentsInChildren<Collider2D>(true)).ToArray();Assert.AreEqual(baseline.colliders.Length+1,colliders.Length); // Sprint 012 adds one intentional low-cover box.
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
