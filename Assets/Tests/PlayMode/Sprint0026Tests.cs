#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

namespace InsaneDreams.Tests
{
 public sealed class Sprint0026Tests
 {
  const float X=7000;readonly List<GameObject> owned=new List<GameObject>();Random.State random;bool background;GameObject player;Camera camera;
  GameObject Keep(GameObject g){owned.Add(g);return g;}
  GameObject Solid(float x,float y,Vector2 size){var g=Keep(new GameObject("S26Solid"));g.transform.position=new Vector2(X+x,y);g.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return g;}
  void Private(EnemyCombat e,string n,object v)=>typeof(EnemyCombat).GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(e,v);
  T Field<T>(EnemyCombat e,string n)=>(T)typeof(EnemyCombat).GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(e);
  bool Escape(EnemyCombat e){Private(e,"nextPanicEscape",0f);Random.InitState(1);return (bool)typeof(EnemyCombat).GetMethod("TryPanicEscape",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(e,new object[]{100f,new Vector2(X-8,1.8f)});}
  EnemyCombat Enemy(float x=0){var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X+x,.71f),Quaternion.identity));g.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));var e=g.GetComponent<EnemyCombat>();e.BindTarget(player.GetComponent<Health>());e.BindGameplayCamera(camera);g.GetComponent<LarperAwareness>().Configure(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Ordinary.asset"));return e;}
  [UnitySetUp]public IEnumerator Setup(){random=Random.state;Random.InitState(26);background=Application.runInBackground;Application.runInBackground=true;Solid(0,-.5f,new Vector2(100,1));player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X-8,.71f),Quaternion.identity));player.GetComponent<Health>().ProtectFor(120);camera=Keep(new GameObject("S26Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=12;camera.aspect=2;camera.transform.position=new Vector3(X,2,-10);yield return new WaitForSeconds(.2f);}
  [UnityTearDown]public IEnumerator Cleanup(){var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByName("Warehouse_Demo");if(scene.isLoaded)yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;Random.state=random;}
  IEnumerator Finish(EnemyCombat e){float until=Time.time+4;while(e.IsPanicEscaping&&Time.time<until)yield return new WaitForFixedUpdate();Assert.IsFalse(e.IsPanicEscaping);}

  [UnityTest]public IEnumerator TwoCloseTrajectoryMissesAfterShortBurstStartRealEscapeWithoutIncreasingGain()
  {
   var e=Enemy();yield return new WaitForSeconds(.2f);
   var s=new SerializedObject(e);Assert.AreEqual(55,s.FindProperty("panicEscapeSuppression").floatValue);Assert.AreEqual(new Vector2(2,3),s.FindProperty("panicEscapeCooldown").vector2Value);Assert.AreEqual(63.8f,s.FindProperty("nearMissSuppression").floatValue,.001f);
   for(int i=0;i<2;i++){Random.InitState(1);float y=e.GetComponent<BoxCollider2D>().bounds.max.y+.05f;e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,new Vector2(X-8,y),new Vector2(X+14,y),1));if(i==0)yield return new WaitForSeconds(.34f);}
   Assert.IsTrue(e.IsPanicEscaping);Assert.AreEqual(0,e.PanicEscapes);float start=e.transform.position.x;yield return new WaitForSeconds(.4f);Assert.Greater(e.transform.position.x-start,.4f);yield return Finish(e);Assert.AreEqual(1,e.PanicEscapes);
  }
  [UnityTest]public IEnumerator RetreatReachesPhysicalCoverHidesUnderPressureAndEventuallyReleases()
  {
   var crate=Solid(3.5f,.5f,Vector2.one);var e=Enemy();yield return new WaitForSeconds(.2f);Assert.IsTrue(Escape(e));Assert.NotNull(Field<Collider2D>(e,"selectedCover"),"Must select physical cover before moving");yield return Finish(e);Assert.AreEqual(1,e.PanicEscapes);Assert.IsTrue(e.IsHoldingCover,"Arrival="+e.transform.position+" destination="+e.RetreatDestination+" state="+e.State);
   yield return new WaitForSeconds(.15f);Assert.IsTrue(e.GetComponent<PlayerPosture2D>().IsCrouched);
   int shots=e.GetComponent<AttackExecutor>().ShotsFired;float started=Time.time;
   while(Time.time-started<3.7f)
   {
    // Controlled pressure injection keeps this test independent of bullet-blocking cover.
    // Physical occlusion and no fire through the crate are checked separately below.
    var meter=default(SuppressionMeter);meter.Add(100,Time.time,100,12,.8f);Private(e,"suppression",meter);
    yield return new WaitForSeconds(.2f);Assert.IsTrue(e.IsHoldingCover);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);
   }
   Assert.IsFalse(Escape(e),"Protected hiding must not burn another major escape");
   yield return new WaitForSeconds(3);Assert.IsFalse(e.IsHoldingCover,"Even sustained pressure has a finite concealment deadline");Assert.AreNotEqual(EngagementState.Panicked,e.State);Assert.AreEqual(1,e.PanicEscapes);
   Object.Destroy(crate);yield return new WaitForSeconds(2.5f);Assert.Greater(e.GetComponent<AttackExecutor>().ShotsFired,shots,"Recovered NPC must resume combat through a valid open sightline");
  }
  [UnityTest]public IEnumerator FlankedOrDestroyedCoverCannotLeaveEnemyHoldingAnExposedPosition()
  {
   var crate=Solid(3.5f,.5f,Vector2.one);var e=Enemy();yield return new WaitForSeconds(.2f);Assert.IsTrue(Escape(e));yield return Finish(e);Assert.IsTrue(e.IsHoldingCover);
   player.transform.position=new Vector2(X+12,.71f);player.GetComponent<Rigidbody2D>().position=player.transform.position;Physics2D.SyncTransforms();yield return new WaitForSeconds(.35f);Assert.IsTrue(e.TargetVisible);Assert.IsFalse(e.IsHoldingCover);
   Object.Destroy(crate);yield return new WaitForSeconds(.4f);Assert.IsFalse(e.IsHoldingCover);Assert.IsFalse(e.IsPanicEscaping);
  }
  [UnityTest]public IEnumerator InsufficientHeadProtectionIsRejectedAndReservationsSeparateRetreats()
  {
   var low=Solid(3.5f,.3f,new Vector2(1,.6f));var e=Enemy();yield return new WaitForSeconds(.2f);Assert.IsTrue(Escape(e));Assert.IsNull(Field<Collider2D>(e,"selectedCover"),"A blocked centre alone must not count as head protection");
   var other=Enemy(-2);yield return new WaitForSeconds(.2f);if(Escape(other))Assert.GreaterOrEqual(Vector2.Distance(e.RetreatDestination,other.RetreatDestination),1.7f);else Assert.IsFalse(other.IsPanicEscaping,"Occupied paths must postpone rather than stack");Assert.AreEqual(0,other.PanicEscapes);yield return Finish(e);Assert.IsFalse(e.IsHoldingCover);
  }
  [UnityTest]public IEnumerator AddedDemoCoverHasGroundClearanceNoObjectiveOrSpawnOverlapAndBlocksBullets()
  {
   Assert.IsNull(InsaneDreams.Progression.GameSession.Instance,"Do not run against a personal save session");
   yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Warehouse_Demo",UnityEngine.SceneManagement.LoadSceneMode.Additive);yield return null;
   // LoadScene is synchronous, but scene-owned components finish binding on the next tick.
   var loaded=UnityEngine.SceneManagement.SceneManager.GetSceneByName("Warehouse_Demo");
   var root=System.Array.Find(loaded.GetRootGameObjects(),g=>g.name=="Sprint0026_Cover");BoxCollider2D[] covers;
   if(root!=null){Assert.AreEqual(3,root.transform.childCount);covers=root.GetComponentsInChildren<BoxCollider2D>();}
   else{
    // Level1's redesign replaced the old trio with these route-side retreat covers.
    var terrain=System.Array.Find(loaded.GetRootGameObjects(),g=>g.name=="FirstOperationTerrain");Assert.NotNull(terrain);
    covers=new BoxCollider2D[3];string[] names={"ArrivalRetreatA","ArrivalRetreatB","StorageFloorCover"};for(int i=0;i<3;i++){var t=terrain.transform.Find(names[i]);Assert.NotNull(t);covers[i]=t.GetComponent<BoxCollider2D>();Assert.NotNull(covers[i]);}
   }
   foreach(var c in covers)
   {
    Assert.IsFalse(c.isTrigger);Assert.NotNull(c.GetComponent<SpriteRenderer>().sprite);var b=c.bounds;
    Assert.IsTrue(Physics2D.Raycast(new Vector2(b.center.x,b.min.y-.03f),Vector2.down,.15f,1));
    foreach(var r in loaded.GetRootGameObjects())
    {
     foreach(var trigger in r.GetComponentsInChildren<InsaneDreams.Missions.MissionTrigger>(true))Assert.IsFalse(b.Intersects(trigger.GetComponent<BoxCollider2D>().bounds),"Objective blocked by "+c.name);
     foreach(var spawn in r.GetComponentsInChildren<EnemySpawnPoint>(true))Assert.Greater(Vector2.Distance(spawn.transform.position,b.center),.95f,"Spawn overlaps "+c.name);
    }
    var hits=new RaycastHit2D[8];Assert.IsTrue(CombatVisibility2D.IsBlocked((Vector2)b.center+Vector2.left*2,(Vector2)b.center+Vector2.right*2,1,hits));
   }
   yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(loaded);
  }
  [UnityTest]public IEnumerator AnalogWalkingAndIntentionalDoubleJumpLandingSlideReuseNormalMotor()
  {
   var actions=player.GetComponent<InsaneDreams.Input.PlayerActions>();var motor=player.GetComponent<PlayerMotor2D>();var body=player.GetComponent<Rigidbody2D>();
   actions.SetTouchMove(new Vector2(.25f,0));yield return new WaitForSeconds(.3f);Assert.AreEqual(motor.MoveSpeed*.25f,body.linearVelocity.x,.15f);
   actions.SetTouchMove(Vector2.right);actions.Jump();yield return new WaitForSeconds(.18f);actions.Jump();yield return new WaitForFixedUpdate();Assert.AreEqual(2,motor.JumpsUsed);
   actions.SetTouchMove(new Vector2(1,-1));yield return null;yield return new WaitForFixedUpdate();Assert.IsTrue(actions.IsAirSliding);Assert.LessOrEqual(body.linearVelocity.y,-13.9f);Assert.AreEqual(2,motor.JumpsUsed);
   float until=Time.time+4;while(!actions.IsLandingSliding&&Time.time<until)yield return null;
   Assert.IsTrue(actions.IsLandingSliding);yield return new WaitForFixedUpdate();Assert.IsTrue(player.GetComponent<PlayerPosture2D>().IsCrouched);Assert.That(body.linearVelocity.x,Is.InRange(motor.MoveSpeed*.5f,motor.MoveSpeed+.1f));
   yield return new WaitForSeconds(.4f);Assert.IsFalse(actions.IsLandingSliding);Assert.AreEqual(0,motor.JumpsUsed);Assert.IsTrue(motor.IsGrounded);
  }
  [UnityTest]public IEnumerator SingleJumpCannotArmSlideAndInputResetCancelsDoubleJumpSlide()
  {
   var actions=player.GetComponent<InsaneDreams.Input.PlayerActions>();var motor=player.GetComponent<PlayerMotor2D>();
   actions.Jump();actions.SetTouchMove(new Vector2(1,-1));yield return new WaitForSeconds(.15f);float until=Time.time+4;
   while(!motor.IsGrounded&&Time.time<until){Assert.IsFalse(actions.IsLandingSliding);yield return null;}yield return null;Assert.IsFalse(actions.IsLandingSliding);
   actions.SetTouchMove(Vector2.right);actions.Jump();yield return new WaitForSeconds(.18f);actions.Jump();yield return new WaitForFixedUpdate();Assert.AreEqual(2,motor.JumpsUsed);
   actions.SuspendInput();actions.SetTouchMove(new Vector2(1,-1));yield return new WaitForSeconds(.1f);until=Time.time+4;
   while(!motor.IsGrounded&&Time.time<until){Assert.IsFalse(actions.IsLandingSliding);yield return null;}yield return null;Assert.IsFalse(actions.IsLandingSliding);
  }
  [UnityTest]public IEnumerator AimStickMovesLeftWithoutChangingDiameterAndBlindFireStartsHidden()
  {
   var doc=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();doc.GetComponent<InsaneDreams.Input.WarehouseTouchControls>().Bind(player.GetComponent<InsaneDreams.Input.PlayerActions>());yield return new WaitForSeconds(.25f);
   var aim=doc.rootVisualElement.Q("aimStick");var move=doc.rootVisualElement.Q("moveStick");var safe=doc.rootVisualElement.Q("safeArea");Assert.AreEqual(44,safe.worldBound.xMax-aim.worldBound.xMax,1,"Allow panel-scale pixel rounding");Assert.AreEqual(move.style.width,aim.style.width);Assert.AreEqual(move.resolvedStyle.width,aim.resolvedStyle.width,2,"Allow rounding at different pixel-aligned origins");Assert.AreEqual(DisplayStyle.None,doc.rootVisualElement.Q("blindFireButton").resolvedStyle.display);
  }
  [UnityTest]public IEnumerator StraightDownAfterDoubleJumpDivesInMidairWithoutAnExtraJump()
  {
   var a=player.GetComponent<InsaneDreams.Input.PlayerActions>();var motor=player.GetComponent<PlayerMotor2D>();a.Jump();yield return new WaitForSeconds(.18f);a.Jump();yield return new WaitForFixedUpdate();a.SetTouchMove(Vector2.down);yield return null;yield return new WaitForFixedUpdate();
   Assert.IsTrue(a.IsAirSliding);Assert.AreEqual(2,motor.JumpsUsed);Assert.Less(player.GetComponent<Rigidbody2D>().linearVelocity.y,-13.9f);Assert.AreEqual(InsaneDreams.Visuals.CharacterPose.Crouch,player.GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>().CurrentPose);
   a.Jump();yield return new WaitForFixedUpdate();Assert.AreEqual(2,motor.JumpsUsed);Assert.Less(player.GetComponent<Rigidbody2D>().linearVelocity.y,0);
  }
  [UnityTest]public IEnumerator WitnessedActualRetreatAddsOnlyBriefLocalInfluenceWithoutWallLeakOrCascade()
  {
   var observer=Enemy(-3);observer.BindTarget(null);var unseen=Enemy(-6);unseen.BindTarget(null);Solid(-4.5f,2,new Vector2(.3f,4));
   foreach(var e in new[]{observer,unseen}){var s=new SerializedObject(e.GetComponent<LarperAwareness>());s.FindProperty("witnessedRetreatChance").floatValue=1;s.ApplyModifiedPropertiesWithoutUndo();}
   var source=Enemy();yield return new WaitForSeconds(.2f);var meter=default(SuppressionMeter);meter.Add(100,Time.time,100,12,.8f);Private(source,"suppression",meter);Assert.IsTrue(Escape(source));yield return new WaitForSeconds(.3f);
   Assert.Less(observer.MoraleInfluence,0);Assert.AreEqual(0,unseen.MoraleInfluence);var awareness=observer.GetComponent<LarperAwareness>();Assert.AreEqual(1,awareness.SocialReactions);Assert.AreEqual(0,awareness.PanicReactions);Assert.AreEqual(0,awareness.CollapseReactions);
   typeof(LarperAwareness).GetMethod("Withdraw",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(source.GetComponent<LarperAwareness>(),null);Assert.AreEqual(1,awareness.SocialReactions);yield return new WaitForSeconds(2.1f);Assert.AreEqual(0,observer.MoraleInfluence);
  }
  [UnityTest]public IEnumerator WeaponMalfunctionDuringDefensiveFireMustNotFreezeRetreatMovement()
  {
   var e=Enemy();yield return new WaitForSeconds(.2f);Assert.IsTrue(Escape(e));Assert.IsTrue(e.Weapon.Cycle.TryMalfunction(Time.time,1.4f));float start=e.transform.position.x;
   yield return new WaitForSeconds(.4f);Assert.Greater(e.transform.position.x-start,.5f,"A jammed gun must not stop the legs");yield return Finish(e);Assert.AreEqual(1,e.PanicEscapes);
  }
 }
}
#endif
