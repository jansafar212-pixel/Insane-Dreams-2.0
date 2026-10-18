using System.Linq;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint009RigTests
    {
        [Test] public void ProportionsExpressionsAndPivotsMatchTheStandingSkeleton()
        {
            foreach(string who in new[]{"Player","Larper"})
            {
                var rig=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/"+who+"CutoutVisual.prefab").GetComponent<CutoutRig>();
                Bounds all=rig.torso.GetComponent<SpriteRenderer>().bounds;foreach(var part in rig.BodyParts)all.Encapsulate(part.GetComponent<SpriteRenderer>().bounds);
                float ratio=rig.head.GetComponent<SpriteRenderer>().bounds.size.y/all.size.y;Assert.That(ratio,Is.InRange(.20f,.255f),who);
                Assert.Greater(rig.skin.leg.bounds.size.y,.5f);Assert.Greater(rig.skin.arm.bounds.size.y,.5f);Assert.Greater(rig.skin.torso.bounds.size.y,.5f);
                Assert.That(rig.head.GetComponent<SpriteRenderer>().bounds.min.y,Is.EqualTo(rig.torso.GetComponent<SpriteRenderer>().bounds.max.y).Within(.005f));
                foreach(var sprite in rig.skin.faces){Assert.AreEqual(rig.skin.head.rect.size,sprite.rect.size);Assert.AreEqual(rig.skin.head.pivot,sprite.pivot);}
                Assert.Greater(rig.speechAnchor.localPosition.y,rig.skin.head.bounds.extents.y);
                Assert.IsNotNull(rig.skin.meleeWeapon);Assert.IsNotNull(rig.skin.secondaryWeapon);
            }
        }
        [Test] public void RagdollColliderMassAndNeckFollowTheResizedLivingParts()
        {
            var rig=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/LarperCutoutVisual.prefab").GetComponent<CutoutRig>();
            var doll=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab").GetComponent<CartoonRagdoll>();
            for(int i=0;i<6;i++)
            {
                var sprite=rig.BodyParts[i].GetComponent<SpriteRenderer>().sprite;var box=doll.parts[i].GetComponent<BoxCollider2D>();
                Assert.AreEqual((Vector2)sprite.bounds.size*.85f,box.size);Assert.AreEqual((Vector2)sprite.bounds.center,box.offset);
            }
            Assert.Less(doll.parts[1].mass,doll.parts[4].mass);Assert.Greater(doll.parts[0].mass,doll.parts[1].mass*3);
            Assert.That(doll.parts[1].GetComponent<HingeJoint2D>().anchor.y,Is.EqualTo(rig.skin.head.bounds.min.y).Within(.001f));
        }
        [Test] public void EveryLadderGraphicHasPixelAlignedCapsAboveItsLandingFloor()
        {
            var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Testbed.unity");
            try
            {
                var visuals=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<LadderVisual2D>(true)).ToArray();Assert.AreEqual(3,visuals.Length);
                foreach(var v in visuals)
                {
                    float floor=v.name.Contains("RoofAccess")?5.4f:-7;
                    Assert.That(v.BottomCap.bounds.min.y,Is.EqualTo(floor).Within(1f/64));Assert.GreaterOrEqual(v.BottomCap.bounds.min.y,floor-.001f);
                    Assert.That(v.BottomCap.bounds.max.y,Is.EqualTo(v.Body.bounds.min.y).Within(.001f));Assert.That(v.TopCap.bounds.min.y,Is.EqualTo(v.Body.bounds.max.y).Within(.001f));
                    Assert.That(v.TopCap.bounds.max.y,Is.EqualTo(v.TopWorld).Within(.001f));Assert.That(v.BottomWorld*32,Is.EqualTo(Mathf.Round(v.BottomWorld*32)).Within(.001f));
                    Assert.IsEmpty(v.GetComponentsInChildren<Collider2D>());
                }
            }finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
