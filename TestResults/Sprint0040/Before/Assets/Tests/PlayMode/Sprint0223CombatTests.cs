#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    public sealed class Sprint0223CombatTests
    {
        private const float X=24000;
        private readonly List<GameObject> objects=new List<GameObject>();
        private readonly List<Object> data=new List<Object>();
        private bool background;private Random.State random;private Camera view;private GameObject source;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Float(Object target,string name,float value){var so=new SerializedObject(target);so.FindProperty(name).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private void Private(object target,string name,object value)=>target.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(target,value);
        private object Reservation(string method,params object[] args)=>typeof(EnemyCombat).Assembly.GetType("InsaneDreams.Enemies.CombatPositionReservations").GetMethod(method).Invoke(null,args);
        private Collider2D Solid(Vector2 point,Vector2 size){var go=Keep(new GameObject("S0223Solid"));go.transform.position=point;var box=go.AddComponent<BoxCollider2D>();box.size=size;Physics2D.SyncTransforms();return box;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(223);
            Solid(new Vector2(X,-.5f),new Vector2(100,1));source=Keep(new GameObject("S0223Shooter"));
            view=Keep(new GameObject("S0223Camera")).AddComponent<Camera>();view.orthographic=true;view.orthographicSize=12;view.aspect=2;view.transform.position=new Vector3(X,2,-10);yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var go in objects)if(go!=null)Object.Destroy(go);foreach(var o in data)if(o!=null)Object.Destroy(o);
            objects.Clear();data.Clear();yield return null;Random.state=random;Application.runInBackground=background;
        }
        private EnemyCombat Enemy(float offset=0,string personality="Ordinary")
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X+offset,.71f),Quaternion.identity));
            go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            var profile=Object.Instantiate(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+personality+".asset"));data.Add(profile);Float(profile,"panicChance",0);
            go.GetComponent<LarperAwareness>().Configure(profile);var combat=go.GetComponent<EnemyCombat>();combat.BindGameplayCamera(view);Float(combat,"defensiveCrouchProbability",0);Float(combat,"suppressionRetreatChance",0);return combat;
        }
        private void Near(EnemyCombat e,float offset=.1f)
        {
            Vector2 p=(Vector2)e.GetComponent<BoxCollider2D>().bounds.center+Vector2.up*offset;
            e.Hear(new CombatNoiseEvent(source,CombatTeam.Player,CombatNoiseKind.Gunfire,p-Vector2.right*8,p+Vector2.right*8,1));
        }
        [UnityTest] public IEnumerator SmallCoverHasOneLeaseAcrossBothFacesAndReleasesOnDisable()
        {
            var left=Enemy(-4);var right=Enemy(4);var crate=Solid(new Vector2(X,.45f),new Vector2(1.2f,.9f));yield return new WaitForSeconds(.15f);
            Assert.IsTrue((bool)Reservation("TryClaimCover",left,crate,new Vector2(X-1.2f,.71f)));
            Assert.IsFalse((bool)Reservation("TryClaimCover",right,crate,new Vector2(X+1.2f,.71f)),"Opposite sides of the same narrow box cannot attract two retreat leases.");
            left.gameObject.SetActive(false);Assert.IsTrue((bool)Reservation("TryClaimCover",right,crate,new Vector2(X+1.2f,.71f)));
            right.ResetEncounter(null);Assert.IsTrue((bool)Reservation("IsCoverFree",left,crate));
        }
        [UnityTest] public IEnumerator AirborneAndCrateHeightNeighborsCannotBypassSeparationButOtherFloorsCan()
        {
            var left=Enemy();var right=Enemy(1.2f);yield return new WaitForSeconds(.15f);
            left.GetComponent<PlayerMotor2D>().enabled=false;typeof(PlayerMotor2D).GetProperty("IsGrounded").SetValue(left.GetComponent<PlayerMotor2D>(),false);
            right.GetComponent<Rigidbody2D>().position=new Vector2(X+1.2f,1.71f);Physics2D.SyncTransforms();
            Assert.IsFalse(left.CanAdvance(1),"A low crate hop must retain the neighboring actor's space.");
            Solid(new Vector2(X+1.2f,2.5f),new Vector2(4,1));right.GetComponent<Rigidbody2D>().position=new Vector2(X+1.2f,3.71f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.12f);
            Assert.IsTrue(left.CanAdvance(1),"Separate floors must remain independent.");
        }
        [UnityTest] public IEnumerator PressureAddsImprecisionThenExpiresAndConfidentActorsRecoverFaster()
        {
            var ordinary=Enemy();var confident=Enemy(5,"Overconfident");yield return new WaitForSeconds(.15f);
            float spread=ordinary.SpreadAtDistance(5);for(int i=0;i<4;i++){Near(ordinary);Near(confident);}
            Assert.AreEqual(100,ordinary.Suppression);Assert.Greater(ordinary.SpreadAtDistance(5),spread);Assert.AreEqual(100,confident.Suppression);
            yield return new WaitForSeconds(1.5f);Assert.Less(confident.Suppression,ordinary.Suppression);
            yield return new WaitForSeconds(8);Assert.AreEqual(0,ordinary.Suppression);Assert.AreEqual(spread,ordinary.SpreadAtDistance(5),.001f);
        }
        [UnityTest] public IEnumerator ReservedCoverImpactCreatesPressureWithoutThroughWallOrParallelShotLeakage()
        {
            var e=Enemy();var crate=Solid(new Vector2(X-1.2f,.45f),new Vector2(1.2f,.9f));yield return new WaitForSeconds(.15f);
            Private(e,"selectedCover",crate);Vector2 end=new Vector2(crate.bounds.min.x,.7f);Vector2 start=end-Vector2.right*8;
            e.Hear(new CombatNoiseEvent(source,CombatTeam.Player,CombatNoiseKind.Gunfire,start,end,1));Assert.Greater(e.Suppression,0);
            e.ResetEncounter(null);Private(e,"selectedCover",crate);
            e.Hear(new CombatNoiseEvent(source,CombatTeam.Player,CombatNoiseKind.Gunfire,start+Vector2.up,end+Vector2.up,1));Assert.AreEqual(0,e.Suppression,"A parallel path above cover is not an impact.");
            Solid(new Vector2(X-.25f,1),new Vector2(.1f,2));
            e.Hear(new CombatNoiseEvent(source,CombatTeam.Player,CombatNoiseKind.Gunfire,start,end,1));Assert.AreEqual(0,e.Suppression,"A separate solid barrier still blocks pressure.");
        }
        [UnityTest] public IEnumerator PsychologicalOptOutPreservesHearingWithoutFearOrSpreadPenalty()
        {
            var e=Enemy();var so=new SerializedObject(e);so.FindProperty("susceptibleToSuppression").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();yield return new WaitForSeconds(.15f);
            float spread=e.SpreadAtDistance(5);for(int i=0;i<10;i++)Near(e);
            Assert.AreEqual(0,e.Suppression);Assert.AreEqual(spread,e.SpreadAtDistance(5));Assert.IsFalse(e.IsDefensivelyCrouching);Assert.IsFalse(e.ApplyAllyMorale(true,5));Assert.AreEqual(ThreatAwareness.Suspicious,e.Awareness);
        }
        [UnityTest] public IEnumerator WitnessInCoverReceivesFiniteInfluenceWithoutInterruptingItsCoverHold()
        {
            var e=Enemy();var ally=Enemy(3);var crate=Solid(new Vector2(X-3,.45f),new Vector2(1,.9f));yield return new WaitForSeconds(.15f);
            Private(e,"selectedCover",crate);Private(e,"coverHoldUntil",Time.time+3);Float(e.GetComponent<LarperAwareness>(),"socialFearChance",1);
            Assert.IsTrue(ally.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player)));
            var doll=ally.GetComponent<RagdollOnDefeat>().LastRagdoll;if(doll!=null)Keep(doll.gameObject);
            Assert.AreEqual(-1,e.MoraleInfluence);Assert.AreEqual(1,e.GetComponent<LarperAwareness>().SocialReactions);Assert.IsTrue(e.IsHoldingCover);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().PanicReactions);
            Assert.AreEqual(45,e.GetComponent<LarperAwareness>().Personality.MoraleCooldown);
            yield return new WaitForSeconds(5.2f);Assert.AreEqual(0,e.MoraleInfluence);
        }
        [UnityTest] public IEnumerator SaturatedProlongedFireCanPanicOnceWithoutStartingAMoraleChain()
        {
            var e=Enemy();yield return new WaitForSeconds(.15f);Float(e.GetComponent<LarperAwareness>().Personality,"panicChance",1);
            for(int i=0;i<4;i++)Near(e);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().PanicReactions,"An isolated instantaneous burst must not roll full panic.");
            Private(e,"suppressionEpisodeStart",Time.time-2);Private(e,"nextSuppressionReaction",0f);Near(e);
            Assert.AreEqual(1,e.GetComponent<LarperAwareness>().PanicReactions);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().CollapseReactions);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().SocialReactions);
            Private(e,"panicUntil",0f);Private(e,"nextSuppressionReaction",0f);Near(e);Assert.AreEqual(1,e.GetComponent<LarperAwareness>().PanicReactions,"The long cooldown also applies to continuous fire.");
        }
    }
}
#endif
