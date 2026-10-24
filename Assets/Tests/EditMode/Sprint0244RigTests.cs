#if UNITY_EDITOR
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Prototype;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0244RigTests
    {
        [Test]public void SamuraiHasCompleteSegmentChainsAndPreservesModularArtDimensions()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab");var rig=root.GetComponentInChildren<CutoutRig>(true);var joints=rig.Segmented;
            Assert.IsTrue(joints.IsBound);Assert.AreSame(rig.leftArm,joints.leftElbow.parent);Assert.AreSame(joints.rightElbow,joints.rightHand.parent);
            Assert.AreSame(rig.leftLeg,joints.leftKnee.parent);Assert.AreSame(joints.leftKnee,joints.leftFoot.parent);Assert.AreSame(joints.rightHand,joints.grip.parent);
            foreach(var s in new[]{joints.upperArm,joints.forearm,joints.hand}){Assert.AreSame(rig.skin.arm.texture,s.texture);Assert.Greater(s.rect.height,0);Assert.AreEqual(32,s.pixelsPerUnit);}
            foreach(var s in new[]{joints.thigh,joints.shin,joints.foot}){Assert.AreSame(rig.skin.leg.texture,s.texture);Assert.Greater(s.rect.height,0);Assert.AreEqual(32,s.pixelsPerUnit);}
            Assert.AreEqual("SamuraiHelmet",rig.skin.head.name);Assert.AreEqual("HugeCardboardKatana",rig.skin.weapon.name);Assert.IsTrue(rig.face.enabled);
        }
        [Test]public void OnlySamuraiOptsIntoEnemyAirJumpWhileCombatTuningIsPreserved()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab");var data=new SerializedObject(root.GetComponent<SamuraiCombat>());
            Assert.AreEqual(.4f,data.FindProperty("windupSeconds").floatValue);Assert.AreEqual(.7f,data.FindProperty("recoverySeconds").floatValue);
            var combat=new SerializedObject(root.GetComponent<EnemyCombat>());Assert.AreEqual(.04f,combat.FindProperty("reactionSeconds").floatValue);Assert.AreEqual(.2f,combat.FindProperty("reengagementDelay").floatValue);
            Assert.AreEqual(40,AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/SamuraiSword.asset").Damage);
            Assert.IsTrue(new SerializedObject(root.GetComponent<PlayerMotor2D>()).FindProperty("allowAirJump").boolValue);
            foreach(var name in new[]{"Larper","FireworksExpert"}){var other=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/"+name+".prefab");if(name=="Larper")Assert.IsTrue(other.GetComponentInChildren<SamuraiArticulatedRig>(true).firearm);else Assert.IsNull(other.GetComponentInChildren<SamuraiArticulatedRig>(true));Assert.IsFalse(new SerializedObject(other.GetComponent<PlayerMotor2D>()).FindProperty("allowAirJump").boolValue);}
        }
    }
}
#endif
