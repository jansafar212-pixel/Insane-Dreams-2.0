#if UNITY_EDITOR
using System.Linq;
using InsaneDreams.Visuals;
using InsaneDreams.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0245RigTests
    {
        [Test]public void FullPanicUsesDistinctFaceWithoutChangingFallbackExpressionContract()
        {var g=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"));try{var r=g.GetComponentInChildren<CutoutRig>();r.Pose(CharacterPose.Panic,0,Vector2.right,1,0,FaceExpression.Scared);Assert.IsNotNull(r.skin.panicFace);Assert.IsTrue(r.skin.panicFace==r.face.sprite,"Compare native Unity sprite identity across asset reloads");Assert.IsFalse(r.skin.Face(FaceExpression.Scared)==r.face.sprite);Assert.IsTrue(r.skin.IsComplete);}finally{Object.DestroyImmediate(g);}}
        [TestCase("Larper")][TestCase("Samurai")]
        public void CandidateHasOneCompleteSegmentedRigWithConsistentDensity(string name)
        {var go=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/"+name+".prefab");var r=go.GetComponentInChildren<CutoutRig>(true);Assert.IsTrue(r.IsBound);Assert.IsTrue(r.Segmented.IsBound);Assert.AreEqual(1,r.GetComponents<SamuraiArticulatedRig>().Length);foreach(var limb in new[]{r.leftArm,r.rightArm,r.leftLeg,r.rightLeg})Assert.AreEqual(1,limb.childCount,"One segment chain per limb");Assert.AreEqual(32,r.skin.head.pixelsPerUnit);Assert.AreEqual(name=="Samurai"?new Vector2(14,10):new Vector2(12,11),r.skin.head.rect.size);Assert.AreEqual(name=="Samurai"?new Vector2(10,11):new Vector2(12,17),r.skin.torso.rect.size);Assert.IsTrue(r.skin.provenance.Contains(name=="Samurai"?"awaiting artistic playtest review":"awaiting artistic approval"));Assert.IsNull(go.GetComponent<BodyArmor>());Assert.IsEmpty(r.GetComponentsInChildren<Collider2D>());}
        [Test]public void CandidateUsesEightFlatColorsWithUniformSkinAndSixDistinctFaces()
        {var r=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab").GetComponentInChildren<CutoutRig>(true);Assert.LessOrEqual(r.skin.head.texture.GetPixels32().Where(c=>c.a>0).Distinct().Count(),8);Assert.AreEqual(6,r.skin.faces.Distinct().Count());var rect=r.skin.arm.rect;for(int y=0;y<12;y++)for(int x=0;x<5;x++){var c=r.skin.arm.texture.GetPixel((int)rect.x+x,(int)rect.y+y);Assert.AreEqual((Color)new Color32(255,214,165,255),c);} }
    }
}
#endif
