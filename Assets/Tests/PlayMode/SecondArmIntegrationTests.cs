#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Visuals;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Checkpoints;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class SecondArmIntegrationTests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private bool background;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        [UnitySetUp] public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;var floor=Keep(new GameObject("SecondArmFloor"));floor.transform.position=new Vector3(15000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(40,1);yield return null;}
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;}
        [UnityTest] public IEnumerator MirroredRagdollCopiesBothArmsAndDepthWithoutChangingPhysicsJoints()
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(15000,.71f),Quaternion.identity));go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;yield return new WaitForSeconds(.2f);go.GetComponent<CharacterVisualDriver>().enabled=false;var r=go.GetComponent<CharacterVisualDriver>().Rig;
            r.Pose(CharacterPose.Crouch,1,Vector2.left,-1,0,FaceExpression.Neutral);var rearPosition=r.leftArm.position;var frontPosition=r.rightArm.position;var rearDirection=r.leftArm.TransformVector(Vector3.down).normalized;
            go.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));var d=go.GetComponent<RagdollOnDefeat>().LastRagdoll;Keep(d.gameObject);
            Assert.AreEqual(6,d.parts.Length);Assert.AreEqual(14,d.Bodies.Length);Assert.AreEqual(13,d.GetComponentsInChildren<HingeJoint2D>().Length);Assert.Less(Vector3.Distance(rearPosition,d.parts[2].position),.001f);Assert.Less(Vector3.Distance(frontPosition,d.parts[3].position),.001f);Assert.Greater(Vector3.Dot(rearDirection,d.parts[2].transform.TransformVector(Vector3.down).normalized),.999f);
            Assert.AreEqual(r.RearArmTint,d.parts[2].GetComponent<SpriteRenderer>().color);Assert.IsTrue(d.parts[2].GetComponent<SpriteRenderer>().flipX);Assert.AreEqual(r.leftArm.GetComponent<SpriteRenderer>().sortingOrder,d.parts[2].GetComponent<SpriteRenderer>().sortingOrder);
            for(int i=2;i<=3;i++){var joint=d.parts[i].GetComponent<HingeJoint2D>();Assert.AreSame(d.parts[0],joint.connectedBody);Assert.IsTrue(joint.useLimits);Assert.IsFalse(joint.enableCollision);Assert.IsTrue(d.parts[i].GetComponent<SpriteRenderer>().enabled);Assert.AreEqual(r.Segmented.upperArm,d.parts[i].GetComponent<SpriteRenderer>().sprite);}
            yield return new WaitForSeconds(.5f);foreach(var part in d.parts){Assert.IsFalse(float.IsNaN(part.position.x));Assert.LessOrEqual(part.linearVelocity.magnitude,12.01f);}
        }
        [UnityTest] public IEnumerator PlayerReloadAndRecoveryRestoreVisibleRearArmAndDepth()
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(15000,.85f),Quaternion.identity));yield return new WaitForSeconds(.25f);var visual=go.GetComponent<CharacterVisualDriver>();var r=visual.Rig;var weapon=go.GetComponent<PlayerLoadout>().SelectedWeapon;
            weapon.SetAmmunition(weapon.Definition.MagazineCapacity-1,108);Assert.IsTrue(weapon.Cycle.TryReload(Time.time));yield return new WaitForSeconds(.3f);Assert.IsTrue(visual.ReloadPoseActive);Assert.Greater(r.leftArm.GetComponent<SpriteRenderer>().sortingOrder,r.torso.GetComponent<SpriteRenderer>().sortingOrder);Assert.Less(r.leftArm.GetComponent<SpriteRenderer>().sortingOrder,r.rightArm.GetComponent<SpriteRenderer>().sortingOrder);
            go.GetComponent<PlayerRecovery>().Respawn();yield return new WaitForSeconds(.25f);Assert.IsFalse(visual.ReloadPoseActive);Assert.IsTrue(r.IsBound);Assert.IsTrue(r.leftArm.GetComponent<SpriteRenderer>().enabled);Assert.IsTrue(r.rightArm.GetComponent<SpriteRenderer>().enabled);Assert.Less(r.leftArm.GetComponent<SpriteRenderer>().sortingOrder,r.torso.GetComponent<SpriteRenderer>().sortingOrder);Assert.Greater(Quaternion.Angle(r.leftArm.localRotation,r.rightArm.localRotation),30);
        }
    }
}
#endif
