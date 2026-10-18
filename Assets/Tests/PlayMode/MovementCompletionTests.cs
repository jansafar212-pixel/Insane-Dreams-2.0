#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class MovementCompletionTests
    {
        private readonly List<GameObject> created = new List<GameObject>();
        private GameObject player;
        private Rigidbody2D body;
        private PlayerActions actions;
        private PlayerPosture2D posture;
        private PlayerTraversal2D traversal;
        private BoxCollider2D box;
        private bool background;
        private GameObject Keep(GameObject go) { created.Add(go); return go; }
        private GameObject Solid(string name, Vector2 position, Vector2 size)
        { var go=Keep(new GameObject(name));go.transform.position=position;go.AddComponent<BoxCollider2D>().size=size;return go; }
        private LadderVolume Ladder(float top=10)
        { var go=Solid("MovementLadder",new Vector2(5000,top-6),new Vector2(1.8f,12));go.GetComponent<BoxCollider2D>().isTrigger=true;return go.AddComponent<LadderVolume>(); }
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;
            Solid("MovementFloor",new Vector2(5000,-.5f),new Vector2(60,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab")));
            body=player.GetComponent<Rigidbody2D>();actions=player.GetComponent<PlayerActions>();posture=player.GetComponent<PlayerPosture2D>();
            traversal=player.GetComponent<PlayerTraversal2D>();box=player.GetComponent<BoxCollider2D>();
            body.position=new Vector2(5000,.85f);actions.ResetActions();Physics2D.SyncTransforms();yield return new WaitForSeconds(.35f);
            Assert.IsNotNull(posture);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        { foreach(var go in created)if(go!=null)Object.Destroy(go);created.Clear();yield return null;Application.runInBackground=background; }
        private void Air(float y=8)
        { actions.ResetActions();traversal.ResetTraversal();body.position=new Vector2(5000,y);body.linearVelocity=Vector2.down*8;Physics2D.SyncTransforms(); }
        [UnityTest] public IEnumerator CrouchAnchorsFeetAndDiagonalWalkKeepsIndependentAimFireAndLowerOrigin()
        {
            float feet=box.bounds.min.y,standing=box.size.y;
            actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.12f);
            Assert.IsTrue(posture.IsCrouched);Assert.That(box.size.y,Is.EqualTo(standing*.55f).Within(.001f));Assert.That(box.bounds.min.y,Is.EqualTo(feet).Within(.025f));
            Assert.That(player.transform.Find("PostureVisual").GetComponent<SpriteRenderer>().bounds.size.y,Is.EqualTo(box.bounds.size.y).Within(.02f));
            actions.SetTouchMove(new Vector2(.7f,-.7f));actions.SetTouchAim(Vector2.left);actions.SetFireIntent(true);yield return new WaitForSeconds(.3f);
            Assert.Greater(body.linearVelocity.x,1);Assert.Less(body.linearVelocity.x,2.3f);Assert.Less(actions.AimDirection.x,0);
            Assert.Greater(player.GetComponent<AttackExecutor>().ShotsFired,0);Assert.Less(player.GetComponent<AttackExecutor>().Origin.y,body.position.y);
            Assert.Less(Vector2.Distance(player.GetComponent<AttackExecutor>().Origin,player.GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>().Rig.MuzzlePosition),.002f);
            Assert.AreEqual(0,body.linearVelocity.y,.03f);
        }
        [UnityTest] public IEnumerator LowCeilingBlocksStandingAndJumpThenClearanceAllowsOrdinaryJump()
        {
            actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.1f);
            var ceiling=Solid("LowCeiling",new Vector2(5000,1.25f),new Vector2(3,.35f));Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.1f);Assert.IsTrue(posture.IsCrouched);
            actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.15f);
            Assert.IsTrue(posture.IsCrouched);Assert.Less(box.bounds.max.y,1.05f);Assert.LessOrEqual(body.linearVelocity.y,.01f);
            Object.Destroy(ceiling);yield return new WaitForSeconds(.1f);Assert.IsFalse(posture.IsCrouched);
            Assert.Less(body.position.y,1); // Held rejected jump must not execute after clearance returns.
            actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.04f);actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.15f);
            Assert.Greater(body.position.y,1.4f);Assert.Greater(body.linearVelocity.y,3);
        }
        [UnityTest] public IEnumerator CrouchPassageCanBeTraversedAndStandingResumesAfterLeaving()
        {
            Solid("Passage",new Vector2(5002,1.2f),new Vector2(2,.4f));Physics2D.SyncTransforms();
            actions.SetTouchMove(new Vector2(.7f,-.7f));yield return new WaitForSeconds(1.8f);
            Assert.Greater(body.position.x,5003.5f);Assert.IsTrue(posture.IsCrouched);
            actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.1f);Assert.IsFalse(posture.IsCrouched);
            Assert.That(body.position.y,Is.EqualTo(.71f).Within(.04f));
        }
        [UnityTest] public IEnumerator GroundDownCrouchesButLadderTopDownEntersAndDescends()
        {
            Ladder();Physics2D.SyncTransforms();actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.15f);
            Assert.IsTrue(posture.IsCrouched);Assert.IsFalse(traversal.IsClimbing);
            actions.ResetActions();traversal.ResetTraversal();posture.TryStand();
            Solid("TopLanding",new Vector2(5002.4f,9.85f),new Vector2(3,.3f));body.position=new Vector2(5001.8f,10.72f);body.linearVelocity=Vector2.zero;
            Physics2D.SyncTransforms();yield return new WaitForSeconds(.35f);
            actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.5f);
            Assert.IsTrue(traversal.IsClimbing);Assert.IsFalse(posture.IsCrouched);Assert.AreEqual(0,body.gravityScale);Assert.Less(body.position.y,10);
        }
        [UnityTest] public IEnumerator ParachuteTapReleaseAndSecondTapToggleOnceWithoutUpwardPropulsion()
        {
            Air();yield return new WaitForFixedUpdate();float vy=body.linearVelocity.y;
            actions.SetTouchParachute(true);yield return new WaitForSeconds(.15f);Assert.IsTrue(traversal.IsParachuting);
            actions.SetTouchParachute(false);yield return new WaitForSeconds(.12f);Assert.IsTrue(traversal.IsParachuting);Assert.Less(body.linearVelocity.y,0);
            actions.SetTouchParachute(true);yield return new WaitForSeconds(.12f);Assert.IsFalse(traversal.IsParachuting);Assert.AreEqual(3,body.gravityScale);
            actions.SetTouchParachute(true);yield return new WaitForSeconds(.12f);Assert.IsFalse(traversal.IsParachuting,"Held input must not repeat");
            Assert.Less(body.linearVelocity.y,0);
        }
        [UnityTest] public IEnumerator CrouchedTopApproachKeepsStandingReachAtHighLadderEndpoints()
        {
            Ladder();actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.12f);Assert.IsTrue(posture.IsCrouched);
            Solid("HighEndpointLanding",new Vector2(5002.4f,7.45f),new Vector2(3,.3f));
            body.position=new Vector2(5001.8f,8.32f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.5f);
            Assert.IsTrue(traversal.IsClimbing);Assert.IsFalse(posture.IsCrouched);Assert.Less(body.position.y,8.2f);Assert.AreEqual(0,body.gravityScale);
        }
        [UnityTest] public IEnumerator ParachutePenalizesBothAxesAndRapidTogglingCannotBoostOrRedeployImmediately()
        {
            Air(12);yield return new WaitForFixedUpdate();actions.SetTouchMove(Vector2.right);actions.SetTouchParachute(true);
            yield return new WaitForSeconds(.35f);Assert.IsTrue(traversal.IsParachuting);
            Assert.That(body.linearVelocity.x,Is.EqualTo(2.8f).Within(.08f));Assert.GreaterOrEqual(body.linearVelocity.y,-2.85f);Assert.Less(body.linearVelocity.y,0);
            Assert.IsTrue(traversal.ToggleParachute());float vertical=body.linearVelocity.y;
            Assert.IsFalse(traversal.ToggleParachute());Assert.AreEqual(vertical,body.linearVelocity.y);
            yield return new WaitForSeconds(.22f);Assert.IsTrue(traversal.ToggleParachute());Assert.Less(body.linearVelocity.y,0);
        }
        [UnityTest] public IEnumerator AscendingDeploymentKeepsNormalGravityAndCannotAddUpwardPropulsion()
        {
            actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.08f);Assert.Greater(body.linearVelocity.y,0);
            float rising=body.linearVelocity.y;actions.SetTouchParachute(true);actions.SetTouchParachute(false);
            Assert.IsTrue(traversal.IsParachuting);Assert.AreEqual(3,body.gravityScale);
            yield return new WaitForSeconds(.08f);Assert.Less(body.linearVelocity.y,rising);Assert.Greater(body.linearVelocity.y,0);Assert.AreEqual(3,body.gravityScale);
        }
        [UnityTest] public IEnumerator LandingClearsParachuteAndAllowsCrouchWithoutAnotherToggle()
        {
            // Legacy test name retained; Sprint 011 intentionally replaces automatic closure.
            Air(1.5f);yield return new WaitForFixedUpdate();actions.SetTouchParachute(true);actions.SetTouchParachute(false);actions.SetTouchMove(Vector2.down);
            yield return new WaitForSeconds(.6f);Assert.IsTrue(traversal.IsParachuting);Assert.AreEqual(3,body.gravityScale);Assert.IsTrue(posture.IsCrouched);Assert.AreEqual(1,traversal.HorizontalSpeedMultiplier);
            Assert.IsTrue(player.GetComponent<PlayerMotor2D>().IsGrounded);
        }
        [UnityTest] public IEnumerator LadderRecoveryAndDisableClearParachuteAndRestoreGravity()
        {
            Ladder();Air(5);yield return new WaitForFixedUpdate();actions.SetTouchParachute(true);actions.SetTouchParachute(false);Assert.IsTrue(traversal.IsParachuting);
            yield return new WaitForSeconds(.35f);actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.1f);Assert.IsTrue(traversal.IsClimbing);Assert.IsTrue(traversal.IsParachuting);Assert.AreEqual(0,body.gravityScale);Assert.AreEqual(1,traversal.HorizontalSpeedMultiplier);
            Air(8);yield return new WaitForFixedUpdate();actions.SetTouchParachute(true);actions.SetTouchParachute(false);
            player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Respawn();yield return null;
            Assert.IsFalse(traversal.IsParachuting);Assert.IsFalse(posture.IsCrouched);Assert.AreEqual(3,body.gravityScale);
            Air(8);yield return new WaitForFixedUpdate();actions.SetTouchParachute(true);player.SetActive(false);
            Assert.IsFalse(traversal.IsParachuting);Assert.AreEqual(3,body.gravityScale);
        }
        private static void TouchEvent(VisualElement target,int id,Vector2 offset,bool down)
        { var touch=new Touch{fingerId=id,position=target.worldBound.center+offset,phase=down?TouchPhase.Began:TouchPhase.Ended};if(down){using(var e=PointerDownEvent.GetPooled(touch,EventModifiers.None))target.SendEvent(e);}else{using(var e=PointerUpEvent.GetPooled(touch,EventModifiers.None))target.SendEvent(e);} }
        [UnityTest] public IEnumerator ThreeTouchPointersCrouchAimFireAndToggleUseAuthoritativeState()
        {
            var ui=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));ui.GetComponent<WarehouseTouchControls>().Bind(actions);yield return null;yield return null;
            var root=ui.GetComponent<UIDocument>().rootVisualElement;var move=root.Q<VisualElement>("moveStick");var aim=root.Q<VisualElement>("aimStick");var chute=root.Q<VisualElement>("parachuteButton");
            TouchEvent(move,0,new Vector2(-25,45),true);TouchEvent(aim,1,new Vector2(55,0),true);yield return new WaitForSeconds(.15f);
            Assert.IsTrue(posture.IsCrouched);Assert.IsTrue(actions.FireHeld);Assert.Less(actions.Move.x,0);Assert.Greater(actions.AimDirection.x,0);
            body.position=new Vector2(5000,7);body.linearVelocity=Vector2.down*5;Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();
            TouchEvent(chute,2,Vector2.zero,true);TouchEvent(chute,2,Vector2.zero,false);yield return new WaitForSeconds(.12f);
            Assert.IsTrue(traversal.IsParachuting);StringAssert.Contains("RETRACT",chute.Q<Label>().text);Assert.IsTrue(chute.ClassListContains("deployed"));
            TouchEvent(aim,9,Vector2.zero,false);yield return null;Assert.IsTrue(actions.FireHeld);Assert.Less(actions.Move.x,0);
            actions.ResetActions();yield return null;Assert.IsFalse(traversal.IsParachuting);Assert.IsFalse(actions.FireHeld);Assert.AreEqual(Vector2.zero,actions.Move);
        }
    }
}
#endif
