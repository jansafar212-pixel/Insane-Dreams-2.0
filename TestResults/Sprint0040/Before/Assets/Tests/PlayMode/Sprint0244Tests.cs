#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Prototype;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0244Tests
    {
        private readonly List<GameObject> owned=new List<GameObject>();private bool background;private Random.State random;
        private GameObject Keep(GameObject go){owned.Add(go);return go;}
        private void Solid(float x,float top,float width)
        {var go=Keep(new GameObject("S0244Platform"));go.transform.position=new Vector2(41000+x,top-.25f);go.AddComponent<BoxCollider2D>().size=new Vector2(width,.5f);}
        private SamuraiCombat Actor(float x=0,float y=.71f)
        {var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(41000+x,y),Quaternion.identity));go.GetComponent<EnemyCombat>().enabled=false;go.GetComponent<LarperBrain>().enabled=false;return go.GetComponent<SamuraiCombat>();}
        private Health Target(float x,float y)
        {var go=Keep(new GameObject("S0244Target"));go.layer=2;go.transform.position=new Vector2(41000+x,y);return go.AddComponent<Health>();}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(244);yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var go in owned)if(go!=null)Object.Destroy(go);owned.Clear();yield return null;Random.state=random;Application.runInBackground=background;}
        [UnityTest]public IEnumerator PlannedDoubleJumpPhysicallyReachesHigherPlatformAndResets()
        {
            Solid(0,0,3);Solid(4,3.3f,3);var s=Actor();var t=Target(4,4.01f);var motor=s.GetComponent<PlayerMotor2D>();yield return new WaitForSeconds(.9f);
            int impulses=0;motor.AirJumped+=()=>impulses++;int maximum=0;float end=Time.time+4;bool launched=false;
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);if(s.PlatformJumpActive&&!launched)t.transform.position=new Vector2(41010,.71f);yield return new WaitForFixedUpdate();maximum=Mathf.Max(maximum,motor.JumpsUsed);launched|=s.PlatformJumpActive;if(launched&&!s.PlatformJumpActive&&motor.IsGrounded)break;}
            Assert.IsTrue(launched,s.FailedTransitionReason);Assert.AreEqual(1,impulses);Assert.AreEqual(2,maximum);Assert.AreEqual(3.3f+s.GetComponent<BoxCollider2D>().size.y*.5f,s.transform.position.y,.18f);Assert.AreEqual(0,motor.JumpsUsed);
        }
        [UnityTest]public IEnumerator OrdinaryReachablePlatformDoesNotSpendSecondJump()
        {
            Solid(0,0,3);Solid(3,1.4f,3);var s=Actor();var t=Target(3,2.11f);int extra=0;s.GetComponent<PlayerMotor2D>().AirJumped+=()=>extra++;yield return new WaitForSeconds(.9f);
            float end=Time.time+3;while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return null;if(s.PlatformJumps>0&&!s.PlatformJumpActive)break;}
            Assert.AreEqual(1,s.PlatformJumps);Assert.AreEqual(0,extra);Assert.AreEqual(1.4f+s.GetComponent<BoxCollider2D>().size.y*.5f,s.transform.position.y,.18f);
        }
        [UnityTest]public IEnumerator MotorRejectsThirdImpulseUntilConfirmedLanding()
        {
            Solid(0,0,40);var s=Actor();var m=s.GetComponent<PlayerMotor2D>();yield return new WaitForSeconds(.2f);m.RequestJump();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            m.RequestJump();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.AreEqual(2,m.JumpsUsed);float before=s.GetComponent<Rigidbody2D>().linearVelocity.y;
            m.RequestJump();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.AreEqual(2,m.JumpsUsed);Assert.Less(s.GetComponent<Rigidbody2D>().linearVelocity.y,before);
        }
        [UnityTest]public IEnumerator SegmentChainsAndSwordRemainConnectedAcrossPosesAndFacing()
        {
            Solid(0,0,40);var s=Actor();var rig=s.GetComponentInChildren<CutoutRig>();var joints=rig.Segmented;yield return null;Assert.IsTrue(joints.IsBound);
            foreach(float facing in new[]{-1f,1f})foreach(var pose in new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Fall,CharacterPose.Crouch,CharacterPose.Climb})
            for(int i=0;i<12;i++)
            {
                rig.Pose(pose,i*.3f,Vector2.right,facing,0,FaceExpression.Neutral,true,true);joints.Pose(pose,i*.3f,i<6?65:-35,pose!=CharacterPose.Jump&&pose!=CharacterPose.Fall,.016f);
                Assert.AreSame(rig.leftArm,joints.leftElbow.parent);Assert.AreSame(joints.rightElbow,joints.rightHand.parent);Assert.AreSame(joints.leftKnee,joints.leftFoot.parent);
                Assert.Less(Vector3.Distance(joints.grip.position,rig.weaponMount.position),.001f,pose.ToString());Assert.AreEqual(Vector3.one,rig.rightArm.localScale);
                Assert.Greater(Vector3.Dot(rig.BarrelDirection,joints.rightHand.TransformVector(Vector3.down).normalized),.99f);
                Assert.That(Mathf.DeltaAngle(0,joints.leftKnee.localEulerAngles.z),Is.InRange(-120.1f,0));Assert.That(Mathf.DeltaAngle(0,joints.rightElbow.localEulerAngles.z),Is.InRange(7.9f,135.1f));
            }
            Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(0,joints.leftKnee.localEulerAngles.z)),1);
        }
        [UnityTest]public IEnumerator RouteCanInitiallyMoveAwayFromTargetAndBlacklistFailedEdge()
        {
            Solid(0,0,3);Solid(-3,2,3);Solid(0,4,3);var s=Actor();var routes=new SamuraiPlatformRoutes();yield return new WaitForSeconds(.2f);
            Assert.IsTrue(routes.Select(s.transform.position,new Vector2(41000,4.71f),.7f,1,2.3f,3.9f,8.5f,6.5f));
            var from=routes.Source;var next=routes.Destination;Assert.Less(next.bounds.center.x,41000);
            routes.Fail(from,next,5,"Controlled failure");Assert.IsFalse(routes.Select(s.transform.position,new Vector2(41000,4.71f),.7f,1,2.3f,3.9f,8.5f,6.5f));Assert.AreEqual("No local connected route",routes.LastFailure);
        }
        [UnityTest]public IEnumerator CeilingRejectsBothArcsWithoutRepeatedLaunches()
        {
            Solid(0,0,3);Solid(3,3.3f,3);Solid(0,2.1f,8);var s=Actor();var t=Target(3,4.01f);yield return new WaitForSeconds(.9f);float end=Time.time+2;
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(0,s.PlatformJumps);Assert.AreEqual(s.GetComponent<BoxCollider2D>().size.y*.5f,s.transform.position.y,.15f);
        }
        [UnityTest]public IEnumerator DoubleJumpCrossesWideGapWithoutAirChaining()
        {
            Solid(0,0,3);Solid(7.3f,0,3);var s=Actor();var t=Target(7.3f,.71f);int extra=0;s.GetComponent<PlayerMotor2D>().AirJumped+=()=>extra++;yield return new WaitForSeconds(.9f);
            float end=Time.time+4;while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return null;if(s.PlatformJumps>0&&!s.PlatformJumpActive)break;}
            Assert.AreEqual(1,extra);Assert.AreEqual(s.GetComponent<BoxCollider2D>().size.y*.5f,s.transform.position.y,.15f);Assert.Greater(s.transform.position.x,41005.8f);Assert.IsTrue(s.GetComponent<PlayerMotor2D>().IsGrounded);
        }
        [UnityTest]public IEnumerator DestroyedDestinationEndsPlanWithoutExtraAirJumps()
        {
            Solid(0,0,3);Solid(4,3.3f,3);var s=Actor();var t=Target(4,4.01f);int extra=0;s.GetComponent<PlayerMotor2D>().AirJumped+=()=>extra++;yield return new WaitForSeconds(.9f);
            s.Tick(t,t.transform.position,true,true,true);Assert.IsTrue(s.PlatformJumpActive);Object.Destroy(owned[1]);float end=Time.time+3;
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return null;Assert.LessOrEqual(s.GetComponent<PlayerMotor2D>().JumpsUsed,2);}
            Assert.LessOrEqual(extra,1);Assert.AreEqual(1,s.PlatformJumps);Assert.IsFalse(s.PlatformJumpActive);
        }
        [UnityTest]public IEnumerator ExistingLadderCanExitAtSelectedIntermediatePlatform()
        {
            Solid(0,0,14);Solid(5,5.4f,4);var ladder=Keep(new GameObject("S0244Ladder"));ladder.transform.position=new Vector2(41002,4);var volume=ladder.AddComponent<BoxCollider2D>();volume.isTrigger=true;volume.size=new Vector2(1.8f,9.6f);ladder.AddComponent<InsaneDreams.Traversal.LadderVolume>();
            var s=Actor(2);var t=Target(5,6.11f);var route=s.GetComponent<AuthoredLadderRoute>();yield return new WaitForSeconds(.9f);float end=Time.time+8;bool used=false;
            while(Time.time<end){if(!route.IsRunning)s.Tick(t,t.transform.position,true,true,true);used|=route.IsRunning;yield return null;if(used&&route.State==LadderRouteState.Complete)break;}
            Assert.IsTrue(used,s.FailedTransitionReason);Assert.AreEqual(LadderRouteState.Complete,route.State);Assert.AreEqual(5.4f+s.GetComponent<BoxCollider2D>().size.y*.5f,s.transform.position.y,.2f);
        }
        [UnityTest]public IEnumerator SegmentedRagdollKeepsFourteenBodiesAndLimitedConnections()
        {
            Solid(0,0,40);var s=Actor();yield return new WaitForSeconds(.2f);var death=s.GetComponent<RagdollOnDefeat>();s.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;
            var rag=death.LastRagdoll;Assert.IsNotNull(rag);owned.Add(rag.gameObject);Assert.AreEqual(14,rag.Bodies.Length);yield return new WaitForSeconds(.35f);
            foreach(var joint in rag.GetComponentsInChildren<HingeJoint2D>()){Assert.IsNotNull(joint.connectedBody);Assert.IsTrue(joint.useLimits);Assert.Less(Vector2.Distance(joint.transform.TransformPoint(joint.anchor),joint.connectedBody.transform.TransformPoint(joint.connectedAnchor)),.25f);}
            foreach(var body in rag.Bodies)Assert.LessOrEqual(body.linearVelocity.magnitude,12.1f);
        }
        [UnityTest]public IEnumerator WarehouseStepsOfferPhysicalJumpRoutesWithoutLoops()
        {
            Solid(-3.3f,5.4f,14.6f);Solid(4.3f,7.3f,2.6f);Solid(7.5f,9,2.6f);Solid(10.3f,10.7f,2.2f);Solid(16.8f,12,9.6f);
            var s=Actor(1.5f,6.11f);var t=Target(15,12.71f);var brain=s.GetComponent<LarperBrain>();brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Samurai.asset"));
            yield return new WaitForSeconds(.9f);brain.DecisionsSuspended=true;brain.enabled=true;float end=Time.time+16;bool single=false,twice=false;int last=0;string trace="";
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);single|=s.PlatformJumpActive&&!s.PlannedDoubleJump;twice|=s.PlatformJumpActive&&s.PlannedDoubleJump;if(last!=s.PlatformJumps){last=s.PlatformJumps;trace+=$"{s.Transition} {s.PlannedLaunch}->{s.PlannedLanding}; ";}yield return null;if(s.transform.position.y>12.6f&&s.GetComponent<PlayerMotor2D>().IsGrounded)break;}
            Assert.IsTrue(single||twice,"No physical route: "+trace);Assert.IsTrue(twice,$"No double jump: {trace} position={s.transform.position} reason={s.FailedTransitionReason}");Assert.Greater(s.transform.position.y,12.5f);Assert.Less(s.PlatformJumps,8,"No failed jump loop; stronger jump can skip an ordinary step");
        }
    }
}
#endif
