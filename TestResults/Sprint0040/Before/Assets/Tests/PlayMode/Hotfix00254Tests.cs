#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace InsaneDreams.Tests
{
 public sealed class Hotfix00254Tests
 {
  const float X=6500;readonly List<GameObject> owned=new List<GameObject>();readonly List<Object> data=new List<Object>();bool background;Random.State random;GameObject player;Camera camera;
  GameObject Keep(GameObject g){owned.Add(g);return g;}
  GameObject Solid(float x,float y,Vector2 size){var g=Keep(new GameObject("H254Solid"));g.transform.position=new Vector2(X+x,y);g.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return g;}
  void Float(Object o,string name,float value){var s=new SerializedObject(o);s.FindProperty(name).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
  void Private(EnemyCombat e,string name,object value)=>typeof(EnemyCombat).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(e,value);
  bool Escape(EnemyCombat e){Private(e,"nextPanicEscape",0f);Float(e,"panicEscapeChance",1);Random.InitState(1);return (bool)typeof(EnemyCombat).GetMethod("TryPanicEscape",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(e,new object[]{100f,new Vector2(X-8,1.8f)});}
  [UnitySetUp]public IEnumerator Setup()
  {
   background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(254);Solid(0,-.5f,new Vector2(100,1));
   player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X-8,.71f),Quaternion.identity));player.GetComponent<Health>().ProtectFor(120);
   camera=Keep(new GameObject("H254Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=12;camera.aspect=2;camera.transform.position=new Vector3(X,2,-10);yield return new WaitForSeconds(.2f);
  }
  [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);foreach(var d in data)if(d!=null)Object.Destroy(d);owned.Clear();data.Clear();yield return null;Application.runInBackground=background;Random.state=random;}
  EnemyCombat Enemy(string type="Larper",string personality="Ordinary")
  {
   var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/"+type+".prefab"),new Vector3(X,type=="Samurai"?.53f:.71f),Quaternion.identity));
   g.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/"+type+".asset"));
   var e=g.GetComponent<EnemyCombat>();e.BindTarget(player.GetComponent<Health>());e.BindGameplayCamera(camera);g.GetComponent<LarperAwareness>().Configure(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+personality+".asset"));return e;
  }
  IEnumerator Finish(EnemyCombat e){float end=Time.time+4;while(e.IsPanicEscaping&&Time.time<end)yield return new WaitForFixedUpdate();Assert.IsFalse(e.IsPanicEscaping,"Bounded escape duration");}
  [UnityTest]public IEnumerator RealSustainedNearMissesCauseMovementThenHideAndCombatRecoveryWithoutCollapse()
  {
   var e=Enemy();yield return new WaitForSeconds(.2f);Float(e,"suppressionRetreatChance",0);Random.InitState(254);
   var source=Keep(new GameObject("H254RealShots"));source.transform.position=new Vector2(X-8,1.8f);var hp=source.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)CombatTeam.Player;s.ApplyModifiedPropertiesWithoutUndo();
   var attack=source.AddComponent<AttackExecutor>();var gun=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),72);
   bool pending=false;for(int i=0;i<8;i++){Random.InitState(1);Assert.IsTrue(attack.Attack(gun,Vector2.right,Time.time));yield return new WaitForSeconds(.21f);pending|=e.IsPanicEscaping&&e.PanicEscapes==0;}
   Assert.GreaterOrEqual(e.NearMissEvents,6);Assert.IsTrue(pending,"A start must not spend the charge");Assert.Greater(e.transform.position.x-X,1);
   yield return Finish(e);Assert.AreEqual(1,e.PanicEscapes);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().CollapseReactions);
   int shots=e.GetComponent<AttackExecutor>().ShotsFired;camera.transform.position+=Vector3.right*100;yield return new WaitForSeconds(1);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired,"No off-screen firing");
   camera.transform.position-=Vector3.right*100;yield return new WaitForSeconds(10);Assert.AreEqual(0,e.Suppression);Assert.Greater(e.GetComponent<AttackExecutor>().ShotsFired,shots);Assert.IsTrue(e.gameObject.activeInHierarchy);
  }
  [UnityTest]public IEnumerator TwoSuccessfulEscapesCapThirdButFearAndLocalMovementRemainAndOnlyEncounterResetClears()
  {
   var e=Enemy();yield return new WaitForSeconds(.2f);Assert.IsTrue(Escape(e));Assert.AreEqual(0,e.PanicEscapes);yield return Finish(e);Assert.AreEqual(1,e.PanicEscapes);
   yield return new WaitForSeconds(.9f);Assert.IsTrue(Escape(e));yield return Finish(e);Assert.AreEqual(2,e.PanicEscapes);Assert.IsFalse(Escape(e));
   e.enabled=false;e.enabled=true;e.BindGameplayCamera(camera);e.BindTarget(player.GetComponent<Health>());Assert.AreEqual(2,e.PanicEscapes);
   Float(e,"defensiveCrouchProbability",1);Float(e,"suppressionRetreatChance",0);
   Vector2 point=(Vector2)e.GetComponent<BoxCollider2D>().bounds.center+Vector2.up*.2f;
   e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,point-Vector2.right*16,point+Vector2.right*8,1));
   yield return new WaitForFixedUpdate();Assert.Greater(e.Suppression,0);Assert.IsTrue(e.IsDefensivelyCrouching);Assert.AreEqual(2,e.PanicEscapes);
   Private(e,"suppressionEpisodeStart",Time.time-1);for(int i=0;i<4;i++)e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,point-Vector2.right*16,point+Vector2.right*8,1));
   Assert.IsFalse(e.IsPanicEscaping);if(e.State==EngagementState.Withdrawing)Assert.LessOrEqual(Vector2.Distance(e.RetreatDestination,e.transform.position),1.6f);
   e.ResetEncounter(player.GetComponent<Health>());Assert.AreEqual(0,e.PanicEscapes);
  }
  [UnityTest]public IEnumerator FailedAndInterruptedRoutesDoNotConsumeQuotaOrRunTowardShooter()
  {
   var wall=Solid(1.2f,2,new Vector2(.3f,4));var e=Enemy();yield return new WaitForSeconds(.2f);
   Assert.IsFalse(Escape(e));Assert.AreEqual(0,e.PanicEscapes);Assert.IsFalse(e.IsPanicEscaping);Object.Destroy(wall);yield return null;
   Assert.IsTrue(Escape(e));yield return new WaitForSeconds(.12f);Assert.AreEqual(0,e.PanicEscapes);float x=e.transform.position.x-X;
   var block=Solid(x+.8f,2,new Vector2(.3f,4));yield return new WaitForSeconds(4);Assert.AreEqual(0,e.PanicEscapes);Assert.IsFalse(e.IsPanicEscaping);
   Object.Destroy(block);yield return null;Assert.IsTrue(Escape(e));yield return Finish(e);Assert.AreEqual(1,e.PanicEscapes);
  }
  [UnityTest]public IEnumerator ReachableLowBoxCoverUsesPhysicalHopAndRejectsRepeatedDestination()
  {
   Solid(3.5f,.5f,Vector2.one);var e=Enemy();yield return new WaitForSeconds(.2f);Assert.IsTrue(Escape(e));var destination=e.RetreatDestination;
   Assert.Greater(destination.x,X+2);float previous=e.transform.position.x;bool rose=false;float end=Time.time+4;
   while(e.IsPanicEscaping&&Time.time<end){yield return new WaitForFixedUpdate();Assert.Less(Mathf.Abs(e.transform.position.x-previous),.35f,"No snapping");previous=e.transform.position.x;rose|=e.transform.position.y>1.3f;}
   Assert.IsTrue(rose,"position="+e.transform.position+" dest="+destination+" state="+e.State+" malfunctions="+e.Malfunctions+" brain="+e.GetComponent<LarperBrain>().State);Assert.AreEqual(1,e.PanicEscapes);yield return new WaitForSeconds(.9f);
   if(Escape(e)){Assert.Greater(Vector2.Distance(destination,e.RetreatDestination),1.25f);Assert.LessOrEqual(Vector2.Distance(new Vector2(X,.71f),e.RetreatDestination),12.01f);}
  }
  [UnityTest]public IEnumerator ExplicitArchetypeOptInPreservesSpecialistsAndFutureOfficer()
  {
   var e=Enemy();yield return new WaitForSeconds(.2f);Assert.IsTrue(e.UsesPanicEscapes);
   var officer=Object.Instantiate(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));officer.name="DistinctOfficerTestArchetype";data.Add(officer);e.GetComponent<LarperBrain>().Configure(officer);
   Assert.IsFalse(e.UsesPanicEscapes);Assert.IsFalse(Escape(e));Private(e,"<PanicEscapes>k__BackingField",2);
   bool legacy=(bool)typeof(EnemyCombat).GetMethod("StartWithdrawal",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(e,new object[]{true,new Vector2(X-8,1.8f),false});Assert.IsTrue(legacy);Assert.Greater(e.RetreatDestination.x-e.transform.position.x,2);
   foreach(var name in new[]{"Samurai","FireworksExpert"}){var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/"+name+".prefab");Assert.AreNotEqual(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"),new SerializedObject(p.GetComponent<LarperBrain>()).FindProperty("archetype").objectReferenceValue);}
  }
  [UnityTest]public IEnumerator PersonalityAndMoraleAdjustHighEscapeProbabilityWithoutChangingAccumulation()
  {
   var e=Enemy();yield return new WaitForSeconds(.2f);Assert.AreEqual(.9f,e.PanicEscapeProbability,.001f);Assert.IsTrue(e.ApplyAllyMorale(true,3));Assert.AreEqual(.98f,e.PanicEscapeProbability,.001f);
   e.ResetEncounter(player.GetComponent<Health>());e.GetComponent<LarperAwareness>().Configure(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Overconfident.asset"));Assert.AreEqual(.675f,e.PanicEscapeProbability,.001f);
   e.GetComponent<LarperAwareness>().Configure(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Cowardly.asset"));Assert.AreEqual(.98f,e.PanicEscapeProbability,.001f);
   Assert.AreEqual(63.8f,new SerializedObject(e).FindProperty("nearMissSuppression").floatValue,.001f);
  }
 }
}
#endif

