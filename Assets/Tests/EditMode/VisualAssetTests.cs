using InsaneDreams.Visuals;
using InsaneDreams.Enemies;
using InsaneDreams.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace InsaneDreams.Tests
{
    public sealed class VisualAssetTests
    {
        [TestCase("Assets/Prefabs/Player/WarehousePlayer.prefab")]
        [TestCase("Assets/Prefabs/Enemies/Larper.prefab")]
        public void GameplayPrefabsHaveCompleteReplaceableVisualsWithoutVisualPhysics(string path)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var rig=prefab.GetComponentInChildren<CutoutRig>(true);
            Assert.IsTrue(rig.IsBound);Assert.IsNotNull(prefab.GetComponent<CharacterVisualDriver>());
            Assert.IsEmpty(rig.GetComponentsInChildren<Rigidbody2D>());Assert.IsEmpty(rig.GetComponentsInChildren<Collider2D>());
            Assert.IsFalse(prefab.GetComponent<SpriteRenderer>().enabled);
            Assert.AreSame(rig.head,rig.face.transform.parent);Assert.AreSame(rig.head,rig.speechAnchor.parent);
            foreach(var part in rig.BodyParts)Assert.IsNotNull(part.GetComponent<SpriteRenderer>().sprite);
            Assert.Greater(rig.face.sortingOrder,rig.head.GetComponent<SpriteRenderer>().sortingOrder);
            foreach(var renderer in rig.GetComponentsInChildren<SpriteRenderer>(true))Assert.IsNotNull(renderer.sharedMaterial);
        }
        [Test] public void ImportedVisualPixelsAreUncompressedPointFilteredAndConsistent()
        {
            var paths=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Art/Sprint007"});Assert.Greater(paths.Length,10);
            foreach(var guid in paths)
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.AreEqual(FilterMode.Point,importer.filterMode,path);Assert.IsFalse(importer.mipmapEnabled,path);
                Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression,path);
                Assert.AreEqual(32,importer.spritePixelsPerUnit,path);Assert.IsTrue(importer.alphaIsTransparency,path);
            }
        }
        [Test] public void RagdollPrefabHasSixBodiesFiveLimitedJointsAndDoesNotBlockGameplayMasks()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab");var doll=root.GetComponent<CartoonRagdoll>();
            Assert.AreEqual(6,doll.parts.Length);Assert.AreEqual(5,root.GetComponentsInChildren<HingeJoint2D>().Length);
            Assert.IsNull(root.GetComponentInChildren<Health>());Assert.IsNull(root.GetComponentInChildren<EnemyCombat>());
            foreach(var body in doll.parts)
            {
                Assert.Greater(body.mass,0);Assert.IsFalse(body.simulated);
                var box=body.GetComponent<BoxCollider2D>();Assert.Greater(box.size.x,0);Assert.Greater(box.size.y,0);Assert.IsFalse(box.isTrigger);
                Assert.AreEqual(0,5 & (1<<body.gameObject.layer));Assert.AreEqual(0,box.excludeLayers.value&1);
                if(body==doll.parts[0])continue;var joint=body.GetComponent<HingeJoint2D>();Assert.AreSame(doll.parts[0],joint.connectedBody);
                Assert.IsTrue(joint.useLimits);Assert.IsFalse(joint.enableCollision);Assert.LessOrEqual(joint.limits.max,80);Assert.GreaterOrEqual(joint.limits.min,-80);
            }
        }
        [TestCase(1280,720)] [TestCase(2560,1080)]
        public void SpeechPlacementSeparatesNearbyBubblesAndClampsToLandscape(int width,int height)
        {
            var screen=new Rect(0,0,width,height);var desired=new Rect(width-100,2,220,60);
            var first=LarperSpeechBubble.AvoidOverlap(desired,screen,new Rect[0]);
            var second=LarperSpeechBubble.AvoidOverlap(desired,screen,new[]{first});
            Assert.IsFalse(first.Overlaps(second));Assert.GreaterOrEqual(second.xMin,0);Assert.GreaterOrEqual(second.yMin,0);
            Assert.LessOrEqual(second.xMax,width);Assert.LessOrEqual(second.yMax,height);
        }
        [TestCase(1280,720)] [TestCase(2560,1080)]
        public void CameraUsesIntegerArtPixelsAtLandscapeSizesWithoutMovingTarget(int width,int height)
        {
            var root=new GameObject("PixelCameraTest");var target=new GameObject("UnsnappedPhysicsTarget");var output=new RenderTexture(width,height,0);
            try
            {
                var camera=root.AddComponent<Camera>();camera.targetTexture=output;target.transform.position=new Vector3(2.123f,3.456f,0);
                var position=target.transform.position;var follow=root.AddComponent<InsaneDreams.CameraSystem.WarehouseCameraFollow>();
                var settings=new UnityEditor.SerializedObject(follow);settings.FindProperty("gameplayZoomMultiplier").floatValue=1;settings.ApplyModifiedPropertiesWithoutUndo();follow.Bind(target.transform);
                float ratio=height/(camera.orthographicSize*2*32);Assert.That(ratio,Is.EqualTo(Mathf.Round(ratio)).Within(.0001f));
                float baseline=camera.orthographicSize;
                settings.Update();settings.FindProperty("gameplayZoomMultiplier").floatValue=1.1f;settings.ApplyModifiedPropertiesWithoutUndo();follow.SnapToTarget();
                Assert.AreEqual(baseline/1.1f,camera.orthographicSize,.0001f);
                Assert.That(root.transform.position.x*32,Is.EqualTo(Mathf.Round(root.transform.position.x*32)).Within(.0001f));
                Assert.That(root.transform.position.y*32,Is.EqualTo(Mathf.Round(root.transform.position.y*32)).Within(.0001f));
                Assert.AreEqual(position,target.transform.position);
            }
            finally{Object.DestroyImmediate(root);Object.DestroyImmediate(target);Object.DestroyImmediate(output);}
        }
    }
}
