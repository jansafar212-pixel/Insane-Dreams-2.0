#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0224Tests
    {
        private const float X=26000;
        private readonly List<GameObject> objects=new List<GameObject>();
        private GameObject player;private PlayerActions actions;private PlayerMotor2D motor;private Rigidbody2D body;private Camera view;
        private bool background;private Random.State random;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Float(Object target,string name,float value){var so=new SerializedObject(target);so.FindProperty(name).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private Collider2D Solid(Vector2 point,Vector2 size){var go=Keep(new GameObject("S0224Solid"));go.transform.position=point;var box=go.AddComponent<BoxCollider2D>();box.size=size;Physics2D.SyncTransforms();return box;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(224);
            Solid(new Vector2(X,-.5f),new Vector2(160,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X-8,.85f),Quaternion.identity));
            actions=player.GetComponent<PlayerActions>();motor=player.GetComponent<PlayerMotor2D>();body=player.GetComponent<Rigidbody2D>();player.GetComponent<Health>().ProtectFor(60);
            var so=new SerializedObject(actions);so.FindProperty("readKeyboard").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();
            view=Keep(new GameObject("S0224Camera")).AddComponent<Camera>();view.orthographic=true;view.orthographicSize=12;view.aspect=2;view.transform.position=new Vector3(X,2,-10);
            yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Random.state=random;Application.runInBackground=background;
        }
        private EnemyCombat Enemy(float duck=0,float retreat=0)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X,.71f),Quaternion.identity));
            go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(view);Float(e,"defensiveCrouchProbability",duck);Float(e,"suppressionRetreatChance",retreat);return e;
        }
        private void Overhead(EnemyCombat e,float height=1.9f)
        {e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,new Vector2(X-8,height),new Vector2(X+8,height),1));}
        [UnityTest] public IEnumerator SingleThenSecondJumpCapsVelocityPreservesMomentumRejectsThirdAndResetsOnLanding()
        {
            actions.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.2f);float horizontal=body.linearVelocity.x;
            actions.Jump();yield return new WaitForFixedUpdate();Assert.AreEqual(1,motor.JumpsUsed);Assert.Greater(body.linearVelocity.y,11);
            yield return new WaitForSeconds(.2f);actions.Jump();yield return new WaitForFixedUpdate();
            Assert.AreEqual(2,motor.JumpsUsed);Assert.That(body.linearVelocity.y,Is.InRange(9.5f,10.3f));Assert.AreEqual(horizontal,body.linearVelocity.x,.01f);
            float second=body.linearVelocity.y;actions.Jump();yield return new WaitForFixedUpdate();Assert.Less(body.linearVelocity.y,second);Assert.AreEqual(2,motor.JumpsUsed);
            actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(1.6f);Assert.IsTrue(motor.IsGrounded);Assert.AreEqual(0,motor.JumpsUsed);
            actions.Jump();yield return new WaitForFixedUpdate();Assert.AreEqual(1,motor.JumpsUsed);Assert.Greater(body.linearVelocity.y,11);
        }
        [UnityTest] public IEnumerator AndroidHeldGestureDoesNotAutoDoubleJumpButFreshGestureDoes()
        {
            actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.25f);Assert.AreEqual(1,motor.JumpsUsed);
            yield return new WaitForSeconds(.12f);Assert.AreEqual(1,motor.JumpsUsed);
            actions.SetTouchMove(Vector2.zero);yield return null;actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.06f);Assert.AreEqual(2,motor.JumpsUsed);Assert.Greater(body.linearVelocity.y,7);
            actions.SetTouchMove(Vector2.zero);yield return null;actions.SetTouchMove(Vector2.up);float before=body.linearVelocity.y;yield return new WaitForSeconds(.06f);Assert.AreEqual(2,motor.JumpsUsed);Assert.Less(body.linearVelocity.y,before);
        }
        [UnityTest] public IEnumerator WallContactAndMotorReenableCannotRefillAirJump()
        {
            Solid(new Vector2(X-6.9f,3),new Vector2(.3f,6));actions.Jump();yield return new WaitForSeconds(.15f);actions.Jump();yield return new WaitForSeconds(.08f);
            actions.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.2f);Assert.AreEqual(2,motor.JumpsUsed);
            motor.enabled=false;motor.enabled=true;actions.Jump();float before=body.linearVelocity.y;yield return new WaitForFixedUpdate();Assert.AreEqual(2,motor.JumpsUsed);Assert.Less(body.linearVelocity.y,before);
        }
        [UnityTest] public IEnumerator DoubleJumpFromCoverKeepsSmoothRootWeaponAndReadableExistingRig()
        {
            Solid(new Vector2(X-6.9f,.45f),new Vector2(1,.9f));actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.25f);Assert.IsTrue(player.GetComponent<PlayerCover2D>().IsCovered);
            actions.SetTouchMove(Vector2.zero);actions.Jump();yield return new WaitForSeconds(.18f);actions.Jump();yield return new WaitForSeconds(.06f);
            var visual=player.GetComponent<CharacterVisualDriver>();Assert.AreEqual(2,motor.JumpsUsed);Assert.IsFalse(player.GetComponent<PlayerCover2D>().IsCovered);Assert.AreEqual(CharacterPose.Jump,visual.CurrentPose);
            Assert.Less(Vector3.Distance(visual.Rig.transform.position,player.transform.position),.001f);Assert.AreEqual(visual.Rig.skin.Face(FaceExpression.Surprised),visual.Rig.face.sprite);
            Assert.Less(Vector2.Distance(visual.Rig.rightArm.TransformPoint(Vector3.down*visual.Rig.skin.arm.bounds.size.y),visual.Rig.weaponMount.position),.01f);
        }
        [UnityTest] public IEnumerator RespawnRestoresBudgetWhileEnemyMotorRetainsSingleJump()
        {
            actions.Jump();yield return new WaitForSeconds(.15f);actions.Jump();yield return new WaitForSeconds(.05f);Assert.AreEqual(2,motor.JumpsUsed);
            player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Respawn();yield return new WaitForSeconds(.15f);Assert.AreEqual(0,motor.JumpsUsed);
            var e=Enemy();e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;e.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeRotation;yield return new WaitForSeconds(.15f);
            var other=e.GetComponent<PlayerMotor2D>();other.RequestJump();yield return new WaitForSeconds(.15f);other.RequestJump();float before=e.GetComponent<Rigidbody2D>().linearVelocity.y;yield return new WaitForFixedUpdate();Assert.AreEqual(1,other.JumpsUsed);Assert.Less(e.GetComponent<Rigidbody2D>().linearVelocity.y,before);
        }
        [UnityTest] public IEnumerator RealHitscanShortBurstDucksAndBuildsPressureWithoutDamageThenRecovers()
        {
            var e=Enemy(1,0);yield return new WaitForSeconds(.2f);var attack=player.GetComponent<AttackExecutor>();var weapon=player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon;
            float hp=e.GetComponent<Health>().Current;float aimY=e.GetComponent<BoxCollider2D>().bounds.max.y+.35f;
            bool ducked=false,posture=false;
            for(int i=0;i<3;i++){Assert.IsTrue(attack.Attack(weapon,(new Vector2(X,aimY)-attack.Origin).normalized,Time.time));yield return new WaitForSeconds(weapon.Definition.FiringInterval+.02f);ducked|=e.IsDefensivelyCrouching;posture|=e.GetComponent<PlayerPosture2D>().IsCrouched;}
            // Sustained evasion may replace the duck at the third shot's frame boundary.
            Assert.AreEqual(3,attack.ShotsFired);Assert.AreEqual(hp,e.GetComponent<Health>().Current);Assert.Greater(e.Suppression,55);Assert.IsTrue(ducked,"A real initial duck must be observed");Assert.IsTrue(posture,"The collider posture must actually crouch");Assert.IsTrue(e.IsDefensivelyCrouching||e.State==EngagementState.Withdrawing);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().PanicReactions);
            yield return new WaitForSeconds(10);Assert.AreEqual(0,e.Suppression);Assert.IsFalse(e.IsDefensivelyCrouching);
        }
        [UnityTest] public IEnumerator OverheadRegistrationRemainsConsistentAfterDuckingAndFarOrShieldedShotsAreRejected()
        {
            var e=Enemy(1,0);yield return new WaitForSeconds(.2f);Overhead(e);float standing=e.Suppression;Assert.Greater(standing,18);
            yield return new WaitForFixedUpdate();Assert.IsTrue(e.GetComponent<PlayerPosture2D>().IsCrouched);Overhead(e);Assert.AreEqual(Mathf.Min(100,standing*2),e.Suppression,.15f);
            e.ResetEncounter(null);Overhead(e,4);Assert.AreEqual(0,e.Suppression);
            Solid(new Vector2(X,1.65f),new Vector2(4,.2f));Overhead(e);Assert.AreEqual(0,e.Suppression);
        }
        [UnityTest] public IEnumerator FailedDuckCanRetryDuringSameBurstOnlyAfterCooldown()
        {
            var e=Enemy();Float(e,"panicEscapeChance",0);Float(e,"sustainedSuppressionSeconds",.45f); // Isolate duck retry from the earlier Sprint0026 moving escape.
            var so=new SerializedObject(e);so.FindProperty("defensiveCrouchCooldown").vector2Value=new Vector2(.3f,.3f);so.ApplyModifiedPropertiesWithoutUndo();yield return new WaitForSeconds(.2f);
            Overhead(e);Float(e,"defensiveCrouchProbability",1);yield return new WaitForSeconds(.1f);Overhead(e);Assert.IsFalse(e.IsDefensivelyCrouching);
            yield return new WaitForSeconds(.25f);Overhead(e);Assert.IsTrue(e.IsDefensivelyCrouching,"A continuous burst must not suppress all further defensive opportunities.");
        }
        [UnityTest] public IEnumerator FewCloseMissesCanInterruptAdvancingAndCommitRetreatWithoutFullPanic()
        {
            var e=Enemy(0,1);e.BindTarget(player.GetComponent<Health>());yield return new WaitForSeconds(.5f);
            Overhead(e);yield return null;Assert.That(e.State,Is.EqualTo(EngagementState.Reacting).Or.EqualTo(EngagementState.Withdrawing));
            Overhead(e);Overhead(e);Assert.AreEqual(EngagementState.Withdrawing,e.State);Assert.Greater(e.RetreatDestination.x,e.transform.position.x);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().PanicReactions);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().CollapseReactions);
        }
    }
}
#endif
