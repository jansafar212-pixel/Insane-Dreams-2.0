#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class AirborneLadderTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private Rigidbody2D body;
        private PlayerActions actions;
        private PlayerTraversal2D traversal;
        private PlayerMotor2D motor;
        private bool background;
        private GameObject Keep(GameObject go) { objects.Add(go); return go; }
        private GameObject Box(string name, Vector2 position, Vector2 size, bool trigger = false)
        {
            var go = Keep(new GameObject(name)); go.transform.position = position;
            var box = go.AddComponent<BoxCollider2D>(); box.size = size; box.isTrigger = trigger;
            return go;
        }
        private LadderVolume Ladder(float x, float bottom = -1) =>
            Box("AirborneCaptureLadder", new Vector2(x, bottom + 6), new Vector2(1.8f, 12), true).AddComponent<LadderVolume>();
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            Box("AirborneTestFloor", new Vector2(4000, -.5f), new Vector2(50, 1));
            var player = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab")));
            body = player.GetComponent<Rigidbody2D>(); actions = player.GetComponent<PlayerActions>();
            traversal = player.GetComponent<PlayerTraversal2D>(); motor = player.GetComponent<PlayerMotor2D>();
            body.position = new Vector2(4000, .85f); actions.ResetActions(); Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.35f); Assert.IsTrue(motor.IsGrounded);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            objects.Clear(); yield return null; Application.runInBackground = background;
        }
        [UnityTest] public IEnumerator HeldUpBeforeVerticalCaptureJumpsThenGrabsWithoutAnotherGesture()
        {
            Ladder(4000, 3); Physics2D.SyncTransforms(); Assert.IsFalse(traversal.CanUseLadder);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.06f);
            Assert.IsFalse(motor.IsGrounded); Assert.IsFalse(traversal.IsClimbing); Assert.Greater(body.linearVelocity.y, 5);
            yield return new WaitForSeconds(.2f);
            Assert.IsTrue(traversal.IsClimbing); Assert.AreEqual(0, body.gravityScale); Assert.IsFalse(motor.enabled);
            Assert.That(body.linearVelocity.y, Is.EqualTo(4.5f).Within(.02f), "Attachment must replace jump velocity, never queue another jump");
            float height = body.position.y; yield return new WaitForSeconds(.15f);
            Assert.Greater(body.position.y, height + .4f); Assert.IsTrue(traversal.IsClimbing);
        }
        [UnityTest] public IEnumerator JumpFromLeftHoldsUpwardDiagonalBeforeHorizontalCapture() => HeldApproach(1);
        [UnityTest] public IEnumerator JumpFromRightHoldsUpwardDiagonalBeforeHorizontalCapture() => HeldApproach(-1);
        private IEnumerator HeldApproach(float direction)
        {
            // Start outside capture, with enough flight time for input sampling under Editor load.
            float axis = 4000 + direction * 3f; Ladder(axis); Physics2D.SyncTransforms();
            Assert.IsFalse(traversal.CanUseLadder);
            actions.SetTouchMove(new Vector2(direction * .7f, .7f)); yield return new WaitForSeconds(.12f);
            Assert.IsFalse(motor.IsGrounded); Assert.IsFalse(traversal.CanUseLadder);
            actions.SetTouchMove(new Vector2(direction * .35f, .9f));
            yield return null;
            float previousX = body.position.x; bool attached = false;
            for (int i = 0; i < 35; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.Less(Mathf.Abs(body.position.x - previousX), .19f, "Capture must align by bounded motion, not teleport");
                previousX = body.position.x; attached |= traversal.IsClimbing;
            }
            Assert.IsTrue(attached, $"No airborne grab: position={body.position}, velocity={body.linearVelocity}, grounded={motor.IsGrounded}, usable={traversal.CanUseLadder}, move={actions.Move}");
            Assert.IsTrue(traversal.IsClimbing); Assert.AreEqual(0, body.gravityScale);
            Assert.That(body.position.x, Is.EqualTo(axis).Within(.2f));
            Assert.Greater(body.linearVelocity.y, 0); Assert.LessOrEqual(body.linearVelocity.y, 4.51f);
        }
        [UnityTest] public IEnumerator CaptureZoneEnteredBeforeUpwardPressGrabsWhileFalling()
        {
            Ladder(4003); Physics2D.SyncTransforms();
            actions.SetTouchMove(new Vector2(.7f, .7f)); yield return new WaitForSeconds(.05f);
            actions.SetTouchMove(Vector2.right); yield return new WaitForSeconds(.45f);
            Assert.IsFalse(traversal.IsClimbing); Assert.IsFalse(motor.IsGrounded);
            Assert.IsTrue(traversal.CanUseLadder); Assert.Less(body.linearVelocity.y, 0);
            actions.SetTouchMove(new Vector2(.2f, .9f)); yield return new WaitForSeconds(.08f);
            Assert.IsTrue(traversal.IsClimbing); Assert.AreEqual(0, body.gravityScale);
            Assert.That(body.linearVelocity.y, Is.EqualTo(4.05f).Within(.03f));
        }
        [UnityTest] public IEnumerator LateralJumpPastLadderWithoutClimbIntentKeepsNormalAirborneMovement()
        {
            Ladder(4000); Physics2D.SyncTransforms();
            actions.SetTouchMove(new Vector2(.7f, .7f)); yield return new WaitForSeconds(.15f);
            Assert.IsFalse(traversal.IsClimbing); Assert.Greater(body.linearVelocity.y, 3);
            actions.SetTouchMove(Vector2.right); yield return new WaitForSeconds(.2f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(3, body.gravityScale); Assert.Greater(body.linearVelocity.x, 6);
        }
        [UnityTest] public IEnumerator BlockedAndDistantLaddersDoNotCaptureHeldAirborneUpInput()
        {
            Ladder(4002); Ladder(4010);
            Box("CaptureBlockingWall", new Vector2(4001, 4), new Vector2(.2f, 8)); Physics2D.SyncTransforms();
            Assert.IsFalse(traversal.CanUseLadder);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.35f);
            Assert.IsFalse(motor.IsGrounded); Assert.IsFalse(traversal.IsClimbing);
            Assert.IsFalse(traversal.CanUseLadder); Assert.AreEqual(3, body.gravityScale);
            Assert.Less(body.position.x, 4000.1f);
        }
        [UnityTest] public IEnumerator DisabledCandidateCanBecomeAvailableDuringHeldUpAndDestructionRestoresGravity()
        {
            var ladder = Ladder(4000, 3); ladder.enabled = false; Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.18f);
            Assert.IsFalse(traversal.IsClimbing); Assert.IsFalse(traversal.CanUseLadder);
            ladder.enabled = true; yield return new WaitForSeconds(.08f);
            Assert.IsTrue(traversal.IsClimbing); Assert.AreEqual(0, body.gravityScale);
            Object.Destroy(ladder.gameObject); yield return new WaitForSeconds(.08f);
            Assert.IsFalse(traversal.IsClimbing); Assert.IsTrue(motor.enabled); Assert.AreEqual(3, body.gravityScale);
            Assert.Less(body.linearVelocity.y, 4.5f);
        }
        [UnityTest] public IEnumerator CandidateDestroyedBeforeCaptureLeavesTheHeldJumpAirborne()
        {
            var ladder = Ladder(4000, 3); Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.06f);
            Assert.IsFalse(motor.IsGrounded); Assert.IsFalse(traversal.IsClimbing);
            Object.Destroy(ladder.gameObject); yield return new WaitForSeconds(.2f);
            Assert.IsFalse(traversal.CanUseLadder); Assert.IsFalse(traversal.IsClimbing);
            Assert.IsTrue(motor.enabled); Assert.AreEqual(3, body.gravityScale);
            Assert.Greater(body.position.y, 1.5f); Assert.Less(body.linearVelocity.y, 6);
        }
        [UnityTest] public IEnumerator AirborneDismountStillRequiresLockoutAndFreshGestureThenCanGrabAgain()
        {
            var ladder = Ladder(4000); Physics2D.SyncTransforms();
            actions.SetTouchMove(new Vector2(.7f, .7f)); yield return new WaitForSeconds(.15f);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.8f);
            Assert.IsTrue(traversal.IsClimbing);
            actions.SetTouchMove(Vector2.left); yield return new WaitForSeconds(.06f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(3, body.gravityScale);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.1f);
            Assert.IsFalse(traversal.IsClimbing, "The airborne exception must not bypass the delay");
            yield return new WaitForSeconds(.25f);
            Assert.IsFalse(traversal.IsClimbing, "Up pressed during lockout remains consumed");
            Assert.IsFalse(motor.IsGrounded);
            actions.SetTouchMove(Vector2.zero); yield return new WaitForSeconds(.04f);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.08f);
            Assert.IsTrue(traversal.IsClimbing); Assert.AreEqual(0, body.gravityScale);
            ladder.enabled = false; yield return new WaitForSeconds(.08f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(3, body.gravityScale); Assert.IsTrue(motor.enabled);
        }
    }
}
#endif
