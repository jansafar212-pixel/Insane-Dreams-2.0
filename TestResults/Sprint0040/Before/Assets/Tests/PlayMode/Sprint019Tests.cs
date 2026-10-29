#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint019Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private Camera camera;private LarperPersonalityProfile profile;private bool background;private Random.State random;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Float(Object target,string key,float value){var s=new SerializedObject(target);s.FindProperty(key).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private GameObject Solid(Vector2 p,Vector2 size){var go=Keep(new GameObject("S019Geometry"));go.transform.position=p;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;random=Random.state;
            Solid(new Vector2(11000,-.5f),new Vector2(120,1));camera=Keep(new GameObject("S019Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=12;camera.aspect=2;camera.transform.position=new Vector3(11004,2,-10);
            profile=Object.Instantiate(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Ordinary.asset"));Float(profile,"panicChance",0);Float(profile,"moraleCooldown",1);yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();Object.Destroy(profile);yield return null;Random.state=random;Application.runInBackground=background;}
        private EnemyCombat Enemy(float x=11000,float fear=1,float confidence=0)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(x,.71f),Quaternion.identity));go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var a=go.GetComponent<LarperAwareness>();a.Configure(profile);a.GetComponent<LarperSpeechBubble>().Bind(camera,null);Float(a,"socialFearChance",fear);Float(a,"socialConfidenceChance",confidence);Float(a,"socialInfluenceSeconds",.35f);
            var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);return e;
        }
        private void Defeat(EnemyCombat e){Assert.IsTrue(e.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player)));var doll=e.GetComponent<RagdollOnDefeat>().LastRagdoll;if(doll!=null)Keep(doll.gameObject);}
        [UnityTest] public IEnumerator RealWitnessedDefeatCausesFiniteFearWithoutInventingPlayerKnowledgeOrChains()
        {
            var witness=Enemy();var source=Enemy(11003);yield return new WaitForSeconds(.15f);Defeat(source);Assert.AreEqual(-1,witness.MoraleInfluence);Assert.AreEqual(.25f,witness.ReloadFraction,.001f);Assert.AreEqual(1,witness.GetComponent<LarperAwareness>().SocialReactions);Assert.AreEqual(Vector2.zero,witness.LastKnownPosition);Assert.IsFalse(witness.TargetVisible);
            yield return new WaitForSeconds(.1f);Assert.AreEqual(EngagementState.Reacting,witness.State);yield return new WaitForSeconds(.5f);Assert.AreEqual(0,witness.MoraleInfluence);Assert.AreEqual(.2f,witness.ReloadFraction,.001f);Assert.AreEqual(EngagementState.Patrol,witness.State);Assert.AreEqual(1,witness.GetComponent<LarperAwareness>().SocialReactions);
        }
        [UnityTest] public IEnumerator WallDistanceAndRearFieldRejectUnobservedDefeats()
        {
            var witness=Enemy();var wall=Solid(new Vector2(11001.5f,2),new Vector2(.3f,5));var blocked=Enemy(11003);yield return new WaitForSeconds(.15f);Defeat(blocked);Assert.AreEqual(0,witness.MoraleInfluence);
            Object.Destroy(wall);yield return null;var distant=Enemy(11020);Defeat(distant);Assert.AreEqual(0,witness.MoraleInfluence);var behind=Enemy(10997);Defeat(behind);Assert.AreEqual(0,witness.MoraleInfluence);Assert.AreEqual(0,witness.GetComponent<LarperAwareness>().SocialReactions);
        }
        [UnityTest] public IEnumerator EventWatermarkAndCooldownPreventRepeatedOrStackingReactions()
        {
            var witness=Enemy();var source=Enemy(11003);yield return new WaitForSeconds(.15f);Defeat(source);var a=witness.GetComponent<LarperAwareness>();var eventField=typeof(LarperAwareness).GetField("lastDefeatEvent",BindingFlags.Instance|BindingFlags.NonPublic);ulong id=(ulong)eventField.GetValue(a);
            var again=Enemy(11004);Defeat(again);Assert.AreEqual(1,a.SocialReactions);yield return new WaitForSeconds(1.1f);
            typeof(LarperAwareness).GetMethod("Witness",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(a,new object[]{source.GetComponent<LarperAwareness>(),new Vector2(11003,1),id});Assert.AreEqual(1,a.SocialReactions,"An old notification stays deduplicated even after cooldown.");
            var fresh=Enemy(11003.5f);Defeat(fresh);Assert.AreEqual(2,a.SocialReactions);Assert.AreEqual(-1,witness.MoraleInfluence);Assert.AreEqual(.25f,witness.ReloadFraction,.001f);
            witness.ResetEncounter(null);Assert.AreEqual(0,witness.MoraleInfluence);Assert.AreEqual(.2f,witness.ReloadFraction,.001f);
        }
        [UnityTest] public IEnumerator ConfidenceRetainsImperfectFireAndNormalAmmoAfterExpiry()
        {
            var witness=Enemy(11000,0,1);var source=Enemy(11003);yield return new WaitForSeconds(.15f);Defeat(source);Assert.AreEqual(1,witness.MoraleInfluence);Assert.AreEqual(.17f,witness.ReloadFraction,.001f);Assert.AreEqual(40,witness.SpreadAtDistance(12));Assert.IsFalse(witness.IsBlindFiring);Assert.IsFalse(witness.TargetVisible);
            yield return new WaitForSeconds(.5f);Assert.AreEqual(0,witness.MoraleInfluence);Assert.AreEqual(.2f,witness.ReloadFraction,.001f);
        }
        [UnityTest] public IEnumerator ReloadingAndLadderRouteRejectSocialInterruptionAndDisableClearsInfluence()
        {
            var witness=Enemy();Assert.IsTrue(witness.Weapon.Cycle.TryReload(Time.time)==false,"A full magazine does not reload.");witness.Weapon.SetAmmunition(0,135);Assert.IsTrue(witness.Weapon.Cycle.TryReload(Time.time));var source=Enemy(11003);yield return new WaitForSeconds(.1f);Defeat(source);Assert.AreEqual(0,witness.MoraleInfluence);Assert.IsTrue(witness.Weapon.Cycle.IsReloading);
            witness.ResetEncounter(null);var route=witness.GetComponent<AuthoredLadderRoute>();route.Configure(null,6);Assert.IsFalse(witness.ApplyAllyMorale(true,4));yield return new WaitForFixedUpdate();
            Assert.IsTrue(witness.ApplyAllyMorale(true,4));witness.gameObject.SetActive(false);Assert.AreEqual(0,witness.MoraleInfluence);
        }
        [UnityTest] public IEnumerator SeveralWitnessesMakeDifferentBoundedDecisionsWithoutPropagatingEvents()
        {
            var defaults=new SerializedObject(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab").GetComponent<LarperAwareness>());
            Assert.AreEqual(.60f,defaults.FindProperty("socialFearChance").floatValue,.001f);Assert.AreEqual(.08f,defaults.FindProperty("collapseChance").floatValue,.001f);Assert.AreEqual(.35f,defaults.FindProperty("collapseSpreadChance").floatValue,.001f);
            var witnesses=Enumerable.Range(0,8).Select(i=>Enemy(10996+i*.7f,.60f,.15f)).ToArray();foreach(var e in witnesses)Float(e.GetComponent<LarperAwareness>(),"socialInfluenceSeconds",.8f);
            var source=Enemy(11003);yield return new WaitForSeconds(.2f);Random.InitState(19019);Defeat(source);var decisions=witnesses.Select(e=>e.MoraleInfluence).ToArray();Assert.Greater(decisions.Distinct().Count(),1);Assert.IsTrue(decisions.Any(v=>v<0));Assert.IsTrue(decisions.Any(v=>v>0),"The higher fear band must still allow confident reactions in the same group.");
            int reactions=witnesses.Sum(e=>e.GetComponent<LarperAwareness>().SocialReactions);yield return new WaitForSeconds(1);Assert.AreEqual(reactions,witnesses.Sum(e=>e.GetComponent<LarperAwareness>().SocialReactions));Assert.IsTrue(witnesses.All(e=>e.MoraleInfluence==0));
        }
    }
}
#endif
