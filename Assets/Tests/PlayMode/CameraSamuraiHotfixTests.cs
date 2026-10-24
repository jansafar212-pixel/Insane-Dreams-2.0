#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Prototype;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace InsaneDreams.Tests
{
    public sealed class CameraSamuraiHotfixTests
    {
        private bool background;private SamuraiCombat samurai;private EnemyCombat combat;private Health target;private Rigidbody2D body,playerBody;private Camera camera;
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.2f);
            var p=Object.FindFirstObjectByType<InsaneDreams.Missions.WarehouseMission>().Player;p.ResetActions();p.enabled=false;target=p.GetComponent<Health>();target.ProtectFor(120);playerBody=p.GetComponent<Rigidbody2D>();playerBody.constraints=RigidbodyConstraints2D.FreezeAll;
            samurai=Object.FindObjectsByType<SamuraiCombat>().First();combat=samurai.GetComponent<EnemyCombat>();body=samurai.GetComponent<Rigidbody2D>();
            foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;if(e!=combat)e.gameObject.SetActive(false);}
            body.position=new Vector2(23,.53f);body.linearVelocity=Vector2.zero;playerBody.position=new Vector2(25.5f,.85f);
            camera=new GameObject("HotfixGameplayCamera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=8;camera.transform.position=new Vector3(23,4,-10);combat.BindGameplayCamera(camera);combat.BindTarget(target);Physics2D.SyncTransforms();yield return new WaitForSeconds(.2f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(camera!=null)Object.Destroy(camera.gameObject);Application.runInBackground=background;yield return null;}
        private void Run(){samurai.GetComponent<LarperBrain>().enabled=true;combat.enabled=true;combat.SetEncounterEnabled(true);}
        [UnityTest] public IEnumerator SmallBodyDetectionWorksAtDifferentElevationsAndSolidWallsOcclude()
        {
            body.constraints=RigidbodyConstraints2D.FreezeAll;Run();
            foreach(float y in new[]{.85f,2.8f,4.2f})
            {playerBody.position=new Vector2(25.5f,y);Physics2D.SyncTransforms();yield return new WaitForSeconds(.2f);Assert.IsTrue(combat.TargetVisible,"Small Samurai should see clear target elevation "+y);}
            var wall=new GameObject("HotfixSightWall");wall.transform.position=new Vector2(24.2f,4);wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,8);Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.2f);Assert.IsFalse(combat.TargetVisible);Object.Destroy(wall);
            var box=samurai.GetComponent<BoxCollider2D>();Assert.AreEqual(.7539605f,box.size.x,.001f);Assert.AreEqual(1.028464f,box.size.y,.001f);
        }
        [UnityTest] public IEnumerator VisibleEntryStartsPursuitAndCameraExitDoesNotClearAggro()
        {
            camera.transform.position=new Vector3(-20,4,-10);Run();float x=body.position.x;yield return new WaitForSeconds(.4f);Assert.Less(Mathf.Abs(body.position.x-x),.1f,"Unseen Samurai must not begin an aggressive approach");
            camera.transform.position=new Vector3(23,4,-10);yield return new WaitForSeconds(.25f);Assert.Greater(Mathf.Abs(body.position.x-x),.2f,"Visible clear opportunity should trigger immediate chase");Assert.IsTrue(combat.HasEngaged);
            camera.transform.position=new Vector3(-20,4,-10);int strikes=samurai.Strikes;yield return new WaitForSeconds(.7f);Assert.AreEqual(strikes,samurai.Strikes,"No offscreen attack initiation");
            body.constraints=RigidbodyConstraints2D.FreezeAll;float away=Mathf.Sign(playerBody.position.x-body.position.x);var wall=new GameObject("HotfixMemoryWall");wall.transform.position=new Vector2((body.position.x+playerBody.position.x)*.5f,3);wall.AddComponent<BoxCollider2D>().size=new Vector2(.1f,6);Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.2f);Assert.IsFalse(combat.TargetVisible,$"body={body.position} player={playerBody.position} wall={wall.transform.position}");Vector2 memory=combat.LastKnownPosition;playerBody.position+=Vector2.right*away;Physics2D.SyncTransforms();
            yield return new WaitForSeconds(3.5f);Assert.IsFalse(combat.TargetVisible,$"body={body.position} player={playerBody.position} wall={wall.transform.position}");Assert.IsTrue(combat.HasEngaged,"Camera/occlusion must not reset acquired aggro");Assert.AreEqual(memory,combat.LastKnownPosition,"Hidden player motion must not grant omniscient pursuit");Object.Destroy(wall);
            combat.ResetEncounter(target);Assert.IsTrue(samurai.GetComponent<LarperBrain>().DecisionsSuspended);yield return new WaitForSeconds(.2f);Assert.IsFalse(combat.HasEngaged,"An offscreen replay/reset must require a new visible opportunity");
        }
        [UnityTest] public IEnumerator AutonomousSmallSamuraiCrossesActualCoverAndKeepsMeleeTelegraph()
        {
            body.position=new Vector2(5.5f,.53f);playerBody.position=new Vector2(10,.85f);camera.transform.position=new Vector3(8,4,-10);Physics2D.SyncTransforms();Run();
            float end=Time.time+18;var trace=new System.Text.StringBuilder();float sample=0;
            while(Time.time<end&&samurai.Strikes==0)
            {yield return new WaitForFixedUpdate();if(Time.time>=sample){sample=Time.time+.5f;trace.Append($"{body.position} see={combat.TargetVisible} {combat.State} {samurai.FailedTransitionReason}; ");}}
            Assert.Greater(body.position.x,8.4f,trace.ToString());Assert.Greater(samurai.Strikes,0,trace.ToString());Assert.IsTrue(combat.HasEngaged);Assert.Greater(samurai.PlatformJumps,0);
        }
        [UnityTest] public IEnumerator RemovedObstructionAllowsReplanningInsteadOfPermanentStationaryFailure()
        {
            body.position=new Vector2(1.5f,5.93f);playerBody.position=new Vector2(4.3f,8.15f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.8f);
            var brain=samurai.GetComponent<LarperBrain>();brain.DecisionsSuspended=true;brain.enabled=true;
            samurai.Tick(target,target.transform.position,true,true,true);Assert.IsTrue(samurai.PlatformJumpActive);
            var wall=new GameObject("TemporaryWarehouseObstruction");wall.transform.position=new Vector2(2.2f,8);wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,6);Physics2D.SyncTransforms();
            float end=Time.time+4;while(Time.time<end){samurai.Tick(target,target.transform.position,true,true,true);yield return new WaitForFixedUpdate();}
            Assert.AreEqual(1,samurai.PlatformJumps,"No repeated blocked launches");Object.Destroy(wall);yield return null;
            end=Time.time+7;while(Time.time<end){samurai.Tick(target,target.transform.position,true,true,true);yield return new WaitForFixedUpdate();if(body.position.y>7.7f&&samurai.GetComponent<PlayerMotor2D>().IsGrounded)break;}
            Assert.Greater(samurai.PlatformJumps,1,samurai.FailedTransitionReason);Assert.Greater(body.position.y,7.7f);
        }
        [UnityTest] public IEnumerator AutonomousPursuitReachesElevatedWarehouseStepAfterVisibleAcquisition()
        {
            body.position=new Vector2(1.5f,5.93f);playerBody.position=new Vector2(4.3f,8.15f);camera.transform.position=new Vector3(4,9,-10);Physics2D.SyncTransforms();Run();
            float end=Time.time+12;bool saw=false;var trace=new System.Text.StringBuilder();float sample=0;
            while(Time.time<end)
            {yield return new WaitForFixedUpdate();saw|=combat.TargetVisible;if(Time.time>=sample){sample=Time.time+.5f;trace.Append($"{body.position} see={combat.TargetVisible} {combat.State} {samurai.FailedTransitionReason}; ");}if(body.position.y>7.7f&&samurai.GetComponent<PlayerMotor2D>().IsGrounded)break;}
            Assert.IsTrue(saw,trace.ToString());Assert.IsTrue(combat.HasEngaged,trace.ToString());Assert.Greater(samurai.PlatformJumps,0,trace.ToString());Assert.Greater(body.position.y,7.7f,trace.ToString());
        }
        [UnityTest] public IEnumerator BriefDecisionGapPreservesCommittedPhysicalFlight()
        {
            body.position=new Vector2(1.5f,5.93f);playerBody.position=new Vector2(4.3f,8.15f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.8f);
            var brain=samurai.GetComponent<LarperBrain>();brain.DecisionsSuspended=true;brain.enabled=true;samurai.Tick(target,target.transform.position,true,true,true);Assert.IsTrue(samurai.PlatformJumpActive);
            yield return new WaitForSeconds(.22f);samurai.Tick(target,target.transform.position,true,true,true);Assert.IsTrue(samurai.PlatformJumpActive,"A short gap must not cancel a valid airborne plan");
            float end=Time.time+2;while(Time.time<end){samurai.Tick(target,target.transform.position,true,true,true);yield return new WaitForFixedUpdate();if(!samurai.PlatformJumpActive&&samurai.GetComponent<PlayerMotor2D>().IsGrounded)break;}
            Assert.AreEqual(1,samurai.PlatformJumps);Assert.AreEqual(7.3f+samurai.GetComponent<BoxCollider2D>().size.y*.5f,body.position.y,.15f);
        }
    }
}
#endif
