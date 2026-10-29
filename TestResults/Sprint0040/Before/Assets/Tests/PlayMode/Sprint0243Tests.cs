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
    public sealed class Sprint0243Tests
    {
        private readonly List<GameObject> owned=new List<GameObject>();
        private bool background;
        private GameObject Keep(GameObject go){owned.Add(go);return go;}
        private GameObject Solid(float x,float top,float width=3)
        {var go=Keep(new GameObject("SamuraiTestPlatform"));go.transform.position=new Vector2(10000+x,top-.25f);go.AddComponent<BoxCollider2D>().size=new Vector2(width,.5f);return go;}
        private SamuraiCombat Actor()
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(10000,.71f),Quaternion.identity));
            go.GetComponent<EnemyCombat>().enabled=false;go.GetComponent<LarperBrain>().enabled=false;return go.GetComponent<SamuraiCombat>();
        }
        private Health Target(float x,float y)
        {var go=Keep(new GameObject("SamuraiTarget"));go.layer=2;go.transform.position=new Vector2(10000+x,y);go.AddComponent<BoxCollider2D>().size=new Vector2(.5f,1.4f);return go.AddComponent<Health>();}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var go in owned)if(go!=null)Object.Destroy(go);owned.Clear();yield return null;Application.runInBackground=background;}
        private IEnumerator Traverse(float top)
        {
            Solid(0,0);Solid(3.5f,top,2.5f);var s=Actor();var target=Target(3.5f,top+.71f);
            var brain=s.GetComponent<LarperBrain>();brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Samurai.asset"));
            yield return new WaitForSeconds(.9f);brain.DecisionsSuspended=true;brain.enabled=true;float end=Time.time+2.8f;bool airborne=false;float maxStep=0;Vector2 previous=s.transform.position;var trace=new System.Text.StringBuilder();float sample=0;
            while(Time.time<end)
            {
                s.Tick(target,target.transform.position,true,true,true);yield return new WaitForFixedUpdate();
                Vector2 now=s.transform.position;maxStep=Mathf.Max(maxStep,Vector2.Distance(previous,now));previous=now;
                airborne|=s.PlatformJumpActive&&!s.GetComponent<PlayerMotor2D>().IsGrounded;
                if(Time.time>=sample){sample=Time.time+.2f;trace.Append($"{now} g={s.GetComponent<PlayerMotor2D>().IsGrounded} brain={brain.State} jump={s.PlatformJumps} land={s.PlannedLanding}; ");}
                if(airborne&&s.GetComponent<PlayerMotor2D>().IsGrounded&&!s.PlatformJumpActive)break;
            }
            Assert.IsTrue(airborne,"A physical jump must take place: "+trace);Assert.AreEqual(1,s.PlatformJumps);
            Assert.Less(Mathf.Abs(s.transform.position.x-s.PlannedLanding.x),.7f,trace.ToString());Assert.AreEqual(top+s.GetComponent<BoxCollider2D>().size.y*.5f,s.transform.position.y,.14f,trace.ToString());
            // A warehouse-height drop reaches >20 units/s: >.4 units per physical step is legitimate.
            float ballisticStep=Mathf.Sqrt(s.GetComponent<PlayerMotor2D>().JumpSpeed*s.GetComponent<PlayerMotor2D>().JumpSpeed+2*29.43f*Mathf.Abs(top))*Time.fixedDeltaTime;
            Assert.Less(maxStep,ballisticStep+.12f,"No root teleport or snap");Assert.IsTrue(s.GetComponent<PlayerMotor2D>().IsGrounded);
        }
        [UnityTest]public IEnumerator PursuesHigherPlatformWithRealPhysics(){yield return Traverse(1.5f);}
        [UnityTest]public IEnumerator CrossesGapWithOneGroundedJump(){yield return Traverse(0);}
        [UnityTest]public IEnumerator PursuesLowerPlatformWithRealPhysics(){yield return Traverse(-1.5f);}
        [UnityTest]public IEnumerator WarehouseFloorHeightDropRemainsPhysical(){yield return Traverse(-5.4f);}
        [UnityTest]public IEnumerator ImpossibleRiseAndBlockedArcDoNotCreateJumpLoops()
        {
            Solid(0,0);Solid(3.5f,4);Solid(0,2,8);var s=Actor();var t=Target(3.5f,4.71f);
            yield return new WaitForSeconds(.9f);float end=Time.time+2;
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(0,s.PlatformJumps);Assert.AreEqual(s.GetComponent<BoxCollider2D>().size.y*.5f,s.transform.position.y,.1f);
        }
        [UnityTest]public IEnumerator MovingTargetCannotRetargetCommittedFlightAndLandingRestoresBudget()
        {
            Solid(0,0);Solid(3.5f,1.5f,2.5f);var s=Actor();var t=Target(3.5f,2.21f);yield return new WaitForSeconds(.9f);
            s.Tick(t,t.transform.position,true,true,true);Assert.IsTrue(s.PlatformJumpActive);var landing=s.PlannedLanding;
            t.transform.position=new Vector2(9990,4);float end=Time.time+1.8f;
            while(Time.time<end){s.Tick(t,t.transform.position,false,true,true);Assert.AreEqual(landing,s.PlannedLanding);yield return null;if(!s.PlatformJumpActive)break;}
            Assert.AreEqual(1,s.PlatformJumps);Assert.Less(Mathf.Abs(s.transform.position.x-landing.x),.7f);
            yield return new WaitForFixedUpdate();Assert.AreEqual(0,s.GetComponent<PlayerMotor2D>().JumpsUsed);
        }
        [UnityTest]public IEnumerator StrikeDealsFortyAndHitInterruptsNextWindup()
        {
            Solid(0,0,20);var s=Actor();var t=Target(1.2f,.71f);yield return new WaitForSeconds(.9f);
            s.Tick(t,t.transform.position,true,true,true);Assert.IsTrue(s.Winding);float end=Time.time+.5f;
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(60,t.Current,.01f);Assert.AreEqual(1,s.Strikes);Assert.IsFalse(s.Winding);
            yield return new WaitForSeconds(.75f);s.Tick(t,t.transform.position,true,true,true);Assert.IsTrue(s.Winding);
            s.GetComponent<Health>().ProtectFor(0);s.GetComponent<Health>().ApplyDamage(new DamageInfo(1,CombatTeam.Player,t.gameObject));
            Assert.IsFalse(s.Winding);s.Tick(t,t.transform.position,true,true,true);Assert.AreEqual(1,s.Strikes);
        }
        [UnityTest]public IEnumerator ModularArtKeepsHandAttachedInJumpWindupAndSwing()
        {
            Solid(0,0,20);var s=Actor();var rig=s.GetComponentInChildren<CutoutRig>();yield return new WaitForSeconds(.9f);
            foreach(var pose in new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Fall,CharacterPose.Crouch})
            foreach(float angle in new[]{65f,15f,-25f})
            {
                rig.Pose(pose,1,Vector2.right,1,0,FaceExpression.Neutral,true,true);rig.PoseSamurai(angle);
                Assert.Less(Vector3.Distance(rig.Segmented.grip.position,rig.weaponMount.position),.001f);Assert.IsTrue(rig.IsBound);Assert.IsTrue(rig.Segmented.IsBound);
            }
            Assert.IsTrue(rig.face.enabled);Assert.IsNull(rig.head.Find("RedHeadband"));Assert.AreEqual("HugeCardboardKatana",rig.skin.weapon.name);
        }
        [UnityTest]public IEnumerator WarehouseAuthoredStepRouteSupportsMultipleFiniteJumps()
        {
            // Exact existing warehouse step surfaces, isolated from live encounter scripts.
            Solid(-3.3f,5.4f,14.6f);Solid(4.3f,7.3f,2.6f);Solid(7.5f,9,2.6f);Solid(10.3f,10.7f,2.2f);Solid(16.8f,12,9.6f);
            var s=Actor();s.transform.position=new Vector2(10001.5f,6.11f);var brain=s.GetComponent<LarperBrain>();brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Samurai.asset"));
            var t=Target(15,12.71f);yield return new WaitForSeconds(.9f);brain.DecisionsSuspended=true;brain.enabled=true;float end=Time.time+14;
            while(Time.time<end&&s.transform.position.y<12.6f){s.Tick(t,t.transform.position,true,true,true);yield return null;}
            Assert.GreaterOrEqual(s.PlatformJumps,2,$"position={s.transform.position}, ground={s.GetComponent<PlayerMotor2D>().IsGrounded}, brain={brain.State}");Assert.Greater(s.transform.position.y,12.5f,"Reach roof using authored steps, allowing planned double-jump shortcuts");Assert.Less(s.PlatformJumps,8,"No bounce loop");
        }
        [UnityTest]public IEnumerator ContactWithStepAllowsOneLaunchAdjustmentInsteadOfStalling()
        {
            Solid(0,0,8);Solid(3.5f,1.9f,2);var s=Actor();s.transform.position=new Vector2(10002.08f,.71f);var t=Target(3.5f,2.61f);
            yield return new WaitForSeconds(.9f);float end=Time.time+4;
            while(Time.time<end&&s.transform.position.y<2.5f){s.Tick(t,t.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(1,s.PlatformJumps);Assert.Greater(s.transform.position.y,2.5f);
        }
        [UnityTest]public IEnumerator RagdollCopiesNewPartsWithoutOldFaceOverlay()
        {
            Solid(0,0,20);var s=Actor();yield return null;var rig=s.GetComponentInChildren<CutoutRig>();var death=s.GetComponent<RagdollOnDefeat>();
            s.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;
            Assert.IsNotNull(death.LastRagdoll);owned.Add(death.LastRagdoll.gameObject);Assert.AreEqual(6,death.LastRagdoll.parts.Length);
            Assert.IsTrue(death.LastRagdoll.face.enabled);Assert.AreEqual(rig.skin.Face(FaceExpression.Stunned),death.LastRagdoll.face.sprite);
            for(int i=0;i<6;i++)Assert.AreEqual(rig.BodyParts[i].GetComponent<SpriteRenderer>().sprite,death.LastRagdoll.parts[i].GetComponent<SpriteRenderer>().sprite);
            Assert.AreEqual(14,death.LastRagdoll.Bodies.Length);Assert.AreEqual(13,death.LastRagdoll.GetComponentsInChildren<HingeJoint2D>().Length);
        }
        [UnityTest]public IEnumerator NewObstructionDuringFlightDoesNotCauseRepeatedFailedJumps()
        {
            Solid(0,0);Solid(3.5f,1.5f,2.5f);var s=Actor();var t=Target(3.5f,2.21f);yield return new WaitForSeconds(.9f);
            s.Tick(t,t.transform.position,true,true,true);Assert.IsTrue(s.PlatformJumpActive);
            var wall=Keep(new GameObject("LateJumpObstacle"));wall.transform.position=new Vector2(10000.9f,2.5f);wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,5);Physics2D.SyncTransforms();
            float end=Time.time+5;
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(1,s.PlatformJumps);Assert.IsFalse(s.PlatformJumpActive);Assert.IsTrue(s.GetComponent<PlayerMotor2D>().IsGrounded);
        }
        [UnityTest]public IEnumerator ExistingDefensiveDuckStillInterruptsSamuraiPressure()
        {
            Solid(0,0,20);var s=Actor();var t=Target(1.2f,.71f);yield return new WaitForSeconds(.9f);
            s.Tick(t,t.transform.position,true,true,true);Assert.IsTrue(s.Winding);
            typeof(EnemyCombat).GetField("defensiveUntil",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(s.GetComponent<EnemyCombat>(),Time.time+1);
            Assert.AreEqual(EngagementState.Reacting,s.Tick(t,t.transform.position,true,true,true));Assert.IsFalse(s.Winding);Assert.AreEqual(0,s.Strikes);Assert.AreEqual(0,s.PlatformJumps);
        }
        [UnityTest]public IEnumerator PlayerDirectlyBelowCausesLedgeApproachAndPhysicalDescent()
        {
            Solid(0,0,24);Solid(0,5.4f,8);var s=Actor();s.transform.position=new Vector2(10000,6.11f);var t=Target(0,.71f);
            var brain=s.GetComponent<LarperBrain>();brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Samurai.asset"));
            yield return new WaitForSeconds(.9f);brain.DecisionsSuspended=true;brain.enabled=true;float end=Time.time+6;
            bool descended=false;Vector2 previous=s.transform.position;float maxStep=0;
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return new WaitForFixedUpdate();Vector2 now=s.transform.position;maxStep=Mathf.Max(maxStep,Vector2.Distance(previous,now));previous=now;descended|=now.y<5;if(descended&&!s.PlatformJumpActive&&s.GetComponent<PlayerMotor2D>().IsGrounded)break;}
            Assert.IsTrue(descended);Assert.LessOrEqual(s.PlatformJumps,1,"A safe walk-off needs no jump impulse");Assert.Less(maxStep,.6f,"No teleport during descent");Assert.AreEqual(s.GetComponent<BoxCollider2D>().size.y*.5f,s.transform.position.y,.15f);Assert.IsTrue(s.GetComponent<PlayerMotor2D>().IsGrounded);
        }
        [UnityTest]public IEnumerator OccupiedLandingDefersJumpUntilAllyLeaves()
        {
            Solid(0,0);Solid(3.5f,1.5f,2.5f);var s=Actor();var t=Target(3.5f,2.21f);
            var ally=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),t.transform.position,Quaternion.identity));
            ally.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;ally.GetComponent<LarperBrain>().enabled=false;
            yield return new WaitForSeconds(.9f);float end=Time.time+1;
            while(Time.time<end){s.Tick(t,t.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(0,s.PlatformJumps);ally.SetActive(false);end=Time.time+1;
            while(Time.time<end&&s.PlatformJumps==0){s.Tick(t,t.transform.position,true,true,true);yield return null;}
            Assert.AreEqual(1,s.PlatformJumps);
        }
    }
}
#endif
