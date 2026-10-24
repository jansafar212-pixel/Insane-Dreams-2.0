#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0222MovementTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private GameObject player;
        private PlayerActions actions;
        private CharacterVisualDriver visual;
        private Rigidbody2D body;
        private bool background;
        private const float X = 21000;
        private GameObject Keep(GameObject go) { objects.Add(go); return go; }
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            var floor = Keep(new GameObject("S0222Floor")); floor.transform.position = new Vector2(X, -.5f);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100, 1);
            player = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"), new Vector3(X, .85f), Quaternion.identity));
            actions = player.GetComponent<PlayerActions>(); visual = player.GetComponent<CharacterVisualDriver>(); body = player.GetComponent<Rigidbody2D>();
            var data = new SerializedObject(actions); data.FindProperty("readKeyboard").boolValue = false; data.ApplyModifiedPropertiesWithoutUndo();
            yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            objects.Clear(); yield return null; Application.runInBackground = background;
        }
        [UnityTest] public IEnumerator PhysicsMicroReversalsCannotFlipPlayerAgainstHeldMovementIntent()
        {
            actions.SetTouchMove(Vector2.right); yield return new WaitForSeconds(.15f);
            player.GetComponent<PlayerMotor2D>().enabled = false; body.gravityScale = 0;
            for (int i = 0; i < 8; i++)
            {
                body.linearVelocity = new Vector2(i % 2 == 0 ? -.2f : .2f, 0);
                yield return null;
                Assert.AreEqual(1, visual.Facing, "A subpixel contact reversal must not mirror the whole character against held right input.");
            }
        }
        [UnityTest] public IEnumerator MicroSpeedNoiseDoesNotAlternateIdleAndWalkAtOldThreshold()
        {
            player.GetComponent<PlayerMotor2D>().enabled = false; body.gravityScale = 0;
            for (int i = 0; i < 8; i++)
            {
                body.linearVelocity = new Vector2(i % 2 == 0 ? .14f : .16f, 0); yield return null;
                Assert.AreEqual(CharacterPose.Idle, visual.CurrentPose, "Noise around the old .15 threshold must not restart the leg pose.");
            }
        }
        [UnityTest] public IEnumerator RepeatedShotPreparationWithinOneFrameCannotAdvancePoseBlends()
        {
            actions.SetTouchMove(Vector2.down); yield return new WaitForFixedUpdate(); yield return null;
            var settings = new SerializedObject(visual.Rig); settings.FindProperty("crouchTransitionSeconds").floatValue = 10; settings.ApplyModifiedPropertiesWithoutUndo();
            typeof(CutoutRig).GetField("crouchBlend", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(visual.Rig, 0f);
            visual.PrepareShot(Vector2.right); var head = visual.Rig.head.localPosition;
            visual.PrepareShot(Vector2.right);
            Assert.Less(Vector3.Distance(head, visual.Rig.head.localPosition), .00001f, "Firing pose evaluation must be idempotent within its render frame.");
        }
        private void AssertRigAnchored()
        {
            var rig=visual.Rig;
            Assert.Less(Vector2.Distance(rig.transform.position,player.transform.position),.03f);
            Assert.IsTrue(rig.leftArm.GetComponent<SpriteRenderer>().enabled); Assert.IsTrue(rig.rightArm.GetComponent<SpriteRenderer>().enabled);
            Assert.Less(Vector2.Distance(rig.rightArm.TransformPoint(Vector3.down*rig.skin.arm.bounds.size.y),rig.weaponMount.position),.01f);
            Assert.AreEqual(RigidbodyInterpolation2D.Interpolate,body.interpolation);
            Assert.IsEmpty(rig.GetComponentsInChildren<Rigidbody2D>());
        }
        [UnityTest] public IEnumerator FastReversalsStopsShootingReloadAndDamageKeepOneResponsiveRig()
        {
            actions.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.4f);Assert.Greater(body.linearVelocity.x,6.5f);
            actions.SetTouchAim(Vector2.right);actions.SetFireIntent(true);yield return new WaitForSeconds(.25f);
            Assert.Greater(player.GetComponent<AttackExecutor>().ShotsFired,0);AssertRigAnchored();actions.SetFireIntent(false);
            actions.RequestReload();Assert.IsTrue(player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon.Cycle.IsReloading);
            for(int i=0;i<6;i++)
            {
                float direction=i%2==0?-1:1;actions.SetTouchMove(Vector2.right*direction);yield return null;yield return null;
                Assert.AreEqual(direction,visual.Facing);yield return new WaitForSeconds(.16f);Assert.AreEqual(direction,visual.Facing);AssertRigAnchored();
            }
            actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.2f);Assert.Less(Mathf.Abs(body.linearVelocity.x),.1f);Assert.AreEqual(CharacterPose.Idle,visual.CurrentPose);
            var health=player.GetComponent<Health>();health.ApplyDamage(new DamageInfo(10,CombatTeam.Enemy));yield return null;yield return null;Assert.AreEqual(FaceExpression.Stunned,visual.Expression);AssertRigAnchored();
            player.GetComponent<PlayerRevive>()?.RestoreInventory(0); // Checkpoint fallback, separate from injection revival.
            health.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));yield return new WaitForSeconds(.2f);Assert.IsFalse(health.IsDefeated);AssertRigAnchored();
        }
        [UnityTest] public IEnumerator CratePassCoverCrouchAndJumpLandingHaveStableOwnership()
        {
            var crate=Keep(new GameObject("S0222Crate"));crate.transform.position=new Vector2(X+1.1f,.45f);crate.AddComponent<BoxCollider2D>().size=new Vector2(1,.9f);Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.25f);var cover=player.GetComponent<PlayerCover2D>();Assert.IsTrue(cover.IsCovered);
            for(int i=0;i<12;i++){yield return null;Assert.IsTrue(cover.IsCovered);Assert.AreEqual(CharacterPose.Crouch,visual.CurrentPose);AssertRigAnchored();}
            actions.SetTouchMove(new Vector2(-1,-1));yield return new WaitForSeconds(.4f);Assert.IsFalse(cover.IsCovered);Assert.AreEqual(CharacterPose.CrouchWalk,visual.CurrentPose);AssertRigAnchored();
            actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.15f);actions.Jump();actions.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.12f);
            Assert.IsFalse(cover.IsCovered);Assert.AreEqual(CharacterPose.Jump,visual.CurrentPose);AssertRigAnchored();
            yield return new WaitForSeconds(.9f);actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.2f);
            Assert.IsFalse(cover.IsCovered);Assert.AreEqual(CharacterPose.Idle,visual.CurrentPose);AssertRigAnchored();
        }
    }
}
#endif
