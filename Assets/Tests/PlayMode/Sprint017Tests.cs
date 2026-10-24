#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint017Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private GameObject player;private Camera camera;private bool background;private Random.State random;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private GameObject Solid(Vector2 position,Vector2 size){var go=Keep(new GameObject("S017Geometry"));go.transform.position=position;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        private void Float(Object target,string key,float value){var s=new SerializedObject(target);s.FindProperty(key).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private void Range(Object target,string key,float min,float max){var s=new SerializedObject(target);s.FindProperty(key).vector2Value=new Vector2(min,max);s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;random=Random.state;
            Solid(new Vector2(10000,-.5f),new Vector2(120,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(10000,.85f),Quaternion.identity));player.GetComponent<Health>().ProtectFor(60);
            camera=Keep(new GameObject("S017Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=20;camera.aspect=1;camera.transform.position=new Vector3(10006,2,-10);yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Random.state=random;Application.runInBackground=background;}
        private EnemyCombat Enemy(float x=10008,bool forced=true)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(x,.71f),Quaternion.identity));go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);e.BindTarget(player.GetComponent<Health>());
            // Isolate defensive-crouch decisions; Sprint0202Tests covers accumulated-suppression retreats.
            Float(e,"suppressionRetreatChance",0);if(forced)Float(e,"defensiveCrouchProbability",1);return e;
        }
        private void Near(EnemyCombat e)=>e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,new Vector2(e.transform.position.x-4,1.7f),new Vector2(e.transform.position.x+4,1.7f),1));
        [UnityTest] public IEnumerator NearMissReusesGoalkeeperPoseGroundedColliderAndSafeFiringMuzzle()
        {
            var e=Enemy();Range(e,"defensiveCrouchSeconds",1.5f,1.5f);yield return new WaitForSeconds(.3f);var box=e.GetComponent<BoxCollider2D>();float feet=box.bounds.min.y,height=box.size.y;var rig=e.GetComponent<CharacterVisualDriver>().Rig;
            var incoming=player.GetComponent<AttackExecutor>();float hp=e.GetComponent<Health>().Current;
            Assert.IsTrue(incoming.Attack(player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon,(new Vector2(10008,1.75f)-incoming.Origin).normalized,Time.time));Assert.AreEqual(hp,e.GetComponent<Health>().Current,"A real hitscan near miss should trigger the reaction without damage.");
            yield return new WaitForSeconds(.25f);Assert.IsTrue(e.IsDefensivelyCrouching);Assert.IsTrue(e.GetComponent<PlayerPosture2D>().IsCrouched);Assert.AreEqual(height*.7f,box.size.y,.001f);Assert.AreEqual(feet,box.bounds.min.y,.01f);Assert.AreEqual(CharacterPose.Crouch,e.GetComponent<CharacterVisualDriver>().CurrentPose);
            foreach(var part in new[]{rig.head,rig.torso,rig.leftArm,rig.leftLeg,rig.rightLeg})Assert.AreEqual(Vector3.one,part.localScale);
            Assert.AreEqual(feet,rig.leftLeg.GetComponent<SpriteRenderer>().bounds.min.y,.05f);Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(0,rig.leftLeg.localEulerAngles.z)),40);
            var attack=e.GetComponent<AttackExecutor>();int shots=attack.ShotsFired;yield return new WaitForSeconds(.3f);Assert.Greater(attack.ShotsFired,shots);Assert.Greater(attack.LastShotOrigin.y,feet+.1f);Assert.LessOrEqual(attack.LastShotOrigin.y,box.bounds.max.y+.05f);Assert.That(e.SpreadAtDistance(12),Is.InRange(40,50));
            camera.transform.position=new Vector3(9900,2,-10);yield return new WaitForSeconds(.1f);shots=attack.ShotsFired;yield return new WaitForSeconds(.25f);Assert.AreEqual(shots,attack.ShotsFired);
        }
        [UnityTest] public IEnumerator ContinuousBurstDoesNotExtendCrouchOrRerollAndCooldownRequiresNewEpisode()
        {
            var e=Enemy();Range(e,"defensiveCrouchSeconds",.2f,.2f);Range(e,"defensiveCrouchCooldown",.8f,.8f);Float(e,"suppressionQuietSeconds",.15f);yield return new WaitForSeconds(.2f);
            Near(e);yield return new WaitForSeconds(.08f);Assert.IsTrue(e.IsDefensivelyCrouching);
            for(int i=0;i<6;i++){Near(e);yield return new WaitForSeconds(.1f);}Assert.IsFalse(e.IsDefensivelyCrouching);Assert.IsFalse(e.GetComponent<PlayerPosture2D>().IsCrouched);
            yield return new WaitForSeconds(.2f);Near(e);yield return new WaitForFixedUpdate();Assert.IsTrue(e.IsDefensivelyCrouching);
        }
        [UnityTest] public IEnumerator FailedProbabilityRollIsNotRepeatedForEveryBullet()
        {
            var e=Enemy();Range(e,"defensiveCrouchCooldown",.8f,.8f);Float(e,"suppressionQuietSeconds",.1f);Float(e,"defensiveCrouchProbability",0);yield return new WaitForSeconds(.2f);Near(e);Assert.IsFalse(e.IsDefensivelyCrouching);
            Float(e,"defensiveCrouchProbability",1);yield return new WaitForSeconds(.2f);Near(e);Assert.IsFalse(e.IsDefensivelyCrouching,"Even a fresh episode must respect a failed decision cooldown.");
            yield return new WaitForSeconds(.7f);Near(e);Assert.IsTrue(e.IsDefensivelyCrouching);
        }
        [UnityTest] public IEnumerator ExpiryRespectsLowCeilingThenRestoresStandingWithoutAnotherTrigger()
        {
            var e=Enemy();Range(e,"defensiveCrouchSeconds",.25f,.25f);yield return new WaitForSeconds(.2f);var box=e.GetComponent<BoxCollider2D>();var size=box.size;var offset=box.offset;Near(e);yield return new WaitForSeconds(.1f);
            var ceiling=Solid(new Vector2(10008,1.4f),new Vector2(2,.2f));yield return new WaitForSeconds(.3f);Assert.IsFalse(e.IsDefensivelyCrouching);Assert.IsTrue(e.GetComponent<PlayerPosture2D>().IsCrouched);Assert.IsFalse(e.GetComponent<PlayerPosture2D>().TryStand());
            Object.Destroy(ceiling);yield return new WaitForSeconds(.2f);Assert.IsFalse(e.GetComponent<PlayerPosture2D>().IsCrouched);Assert.AreEqual(size,box.size);Assert.AreEqual(offset,box.offset);
        }
        [UnityTest] public IEnumerator WithdrawalReloadPanicAirborneAndDisableTakePriority()
        {
            var e=Enemy();yield return new WaitForSeconds(.4f);Near(e);yield return new WaitForSeconds(.1f);Assert.IsTrue(e.IsDefensivelyCrouching);e.Weapon.SetAmmunition(0,27);yield return new WaitForSeconds(.2f);Assert.IsFalse(e.IsDefensivelyCrouching);Assert.IsTrue(e.State==EngagementState.Withdrawing||e.State==EngagementState.Reloading);Near(e);Assert.IsFalse(e.IsDefensivelyCrouching);
            e.ResetEncounter(player.GetComponent<Health>());Near(e);yield return new WaitForFixedUpdate();Assert.IsTrue(e.IsDefensivelyCrouching);Assert.IsTrue(e.PanicFor(.3f));Assert.IsFalse(e.IsDefensivelyCrouching);Near(e);Assert.IsFalse(e.IsDefensivelyCrouching);
            yield return new WaitForSeconds(.4f);e.ResetEncounter(player.GetComponent<Health>());e.GetComponent<Rigidbody2D>().position=new Vector2(10008,5);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Near(e);Assert.IsFalse(e.IsDefensivelyCrouching);
            e.GetComponent<Rigidbody2D>().position=new Vector2(10008,.71f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.15f);e.ResetEncounter(player.GetComponent<Health>());Near(e);yield return new WaitForFixedUpdate();Assert.IsTrue(e.IsDefensivelyCrouching);e.gameObject.SetActive(false);Assert.IsFalse(e.IsDefensivelyCrouching);e.gameObject.SetActive(true);yield return new WaitForSeconds(.1f);Assert.IsFalse(e.GetComponent<PlayerPosture2D>().IsCrouched);
        }
        [UnityTest] public IEnumerator CrouchedSightCannotTrackThroughNewSolidCoverAndDeathStillCreatesRagdoll()
        {
            var e=Enemy();Float(e,"ordinaryBlindChance",0);Float(e,"cowardBlindChance",0);Float(e,"overconfidentBlindChance",0);yield return new WaitForSeconds(.4f);Near(e);yield return new WaitForSeconds(.2f);Assert.IsTrue(e.IsDefensivelyCrouching);var known=e.LastKnownPosition;
            Solid(new Vector2(10004,2),new Vector2(.3f,6));player.GetComponent<Rigidbody2D>().position=new Vector2(9999,1);Physics2D.SyncTransforms();yield return new WaitForSeconds(.2f);Assert.IsFalse(e.TargetVisible);Assert.AreEqual(known,e.LastKnownPosition);var attack=e.GetComponent<AttackExecutor>();int shots=attack.ShotsFired;yield return new WaitForSeconds(.2f);Assert.AreEqual(shots,attack.ShotsFired);
            e.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;Assert.IsTrue(e.GetComponent<Health>().IsDefeated);Assert.IsFalse(e.IsDefensivelyCrouching);Assert.IsFalse(e.HasCombatPosition);Assert.IsFalse(e.gameObject.activeInHierarchy);
            var ragdoll=e.GetComponent<RagdollOnDefeat>();Assert.AreEqual(1,ragdoll.SpawnCount);Assert.IsNotNull(ragdoll.LastRagdoll);Keep(ragdoll.LastRagdoll.gameObject);
        }
        [UnityTest] public IEnumerator LadderTraversalCancelsDefensiveCrouchAndRejectsNewSuppressionDecisions()
        {
            var e=Enemy();yield return new WaitForSeconds(.2f);Near(e);yield return new WaitForSeconds(.1f);Assert.IsTrue(e.IsDefensivelyCrouching);
            var ladder=Solid(new Vector2(10008,5),new Vector2(2,12));ladder.GetComponent<BoxCollider2D>().isTrigger=true;ladder.AddComponent<LadderVolume>();Physics2D.SyncTransforms();
            var traversal=e.GetComponent<PlayerTraversal2D>();traversal.SetInput(Vector2.up,false);yield return new WaitForSeconds(.25f);Assert.IsTrue(traversal.IsClimbing);Assert.IsFalse(e.IsDefensivelyCrouching);Near(e);Assert.IsFalse(e.IsDefensivelyCrouching);
        }
        [UnityTest] public IEnumerator Sprint018SuppressionModifiesExistingAmmoAndMovingTargetDecisionsOnlyBriefly()
        {
            var e=Enemy();Float(e,"defensiveCrouchProbability",0);yield return new WaitForSeconds(.4f);Assert.AreEqual(.2f,e.ReloadFraction,.001f);Assert.IsTrue(e.HasCombatPosition);var held=e.CombatPosition;
            player.GetComponent<Rigidbody2D>().position=new Vector2(10003.1f,.71f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.3f);Assert.Less(Vector2.Distance(held,e.CombatPosition),.01f,"Without suppression, keep the valid lease below the normal 3.5-unit retarget threshold.");
            e.Weapon.SetAmmunition(6,135);Near(e);Assert.AreEqual(.24f,e.ReloadFraction,.001f);yield return new WaitForSeconds(.3f);Assert.IsTrue(e.State==EngagementState.Withdrawing||e.State==EngagementState.Reloading,"Six rounds now qualify for an earlier ordinary retreat under fire (previously five).");
            e.ResetEncounter(player.GetComponent<Health>());player.GetComponent<Rigidbody2D>().position=new Vector2(10000,.71f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.4f);held=e.CombatPosition;
            // Allow the .1-second perception and .25-second tactical samples to both run,
            // while still checking inside the existing .6-second suppression window.
            player.GetComponent<Rigidbody2D>().position=new Vector2(10003.1f,.71f);Physics2D.SyncTransforms();Near(e);yield return new WaitForSeconds(.5f);Assert.Greater(Vector2.Distance(held,e.CombatPosition),1,"Suppression lowers the existing retarget threshold from 3.5 to 2.8, without forcing a new action sequence.");
            yield return new WaitForSeconds(.4f);Assert.AreEqual(.2f,e.ReloadFraction,.001f);
            Float(e,"ordinaryReloadFraction",0);Near(e);Assert.AreEqual(0,e.ReloadFraction,"A zero aggressive reload threshold stays zero.");
        }
        [UnityTest] public IEnumerator Sprint018CrouchTuningIncreasesSeededOpportunitiesWithoutMakingThemMandatory()
        {
            var e=Enemy(forced:false);yield return new WaitForSeconds(.2f);Assert.AreEqual(.70f,new SerializedObject(e).FindProperty("defensiveCrouchProbability").floatValue,.001f);
            int before=0,after=0,calibrated=0,current=0;for(int i=0;i<2000;i++)foreach(float chance in new[]{.3f,.36f,.48f,.70f})
            {
                e.ResetEncounter(player.GetComponent<Health>());Float(e,"defensiveCrouchProbability",chance);Random.InitState(18000+i);Near(e);
                if(e.IsDefensivelyCrouching){if(chance==.3f)before++;else if(chance==.36f)after++;else if(chance==.48f)calibrated++;else current++;}
            }
            TestContext.WriteLine("Matched seeded episodes: Sprint017="+before+", Sprint018="+after+", Sprint020.1="+calibrated);
            Assert.Greater(after,before*1.1f);Assert.Less(after,before*1.35f);Assert.Less(after,1000,"Most eligible episodes still choose other behavior.");
            Assert.Greater(calibrated,after*1.25f);Assert.Less(calibrated,after*1.4f);Assert.Less(calibrated,1200,"Calibration must leave substantial opportunities to stand and fight.");
            Assert.Greater(current,calibrated*1.3f);Assert.Less(current,calibrated*1.6f);Assert.Less(current,1800,"Sprint022.4 must still leave ground-holding decisions.");
        }
        [UnityTest] public IEnumerator SeveralLarpersMakeVariedBoundedDecisionsOnTheSameIncomingSegment()
        {
            var actors=Enumerable.Range(0,8).Select(i=>Enemy(10004+i*2,false)).ToArray();foreach(var e in actors){e.BindTarget(null);Range(e,"defensiveCrouchSeconds",.8f,2);}
            yield return new WaitForSeconds(.2f);Random.InitState(17017);var noise=new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,new Vector2(10000,1.7f),new Vector2(10022,1.7f),1);foreach(var e in actors)e.Hear(noise);
            int crouched=actors.Count(e=>e.IsDefensivelyCrouching);Assert.Greater(crouched,0);Assert.Less(crouched,actors.Length);yield return new WaitForSeconds(.25f);Assert.AreEqual(crouched,actors.Count(e=>e.GetComponent<PlayerPosture2D>().IsCrouched));
            yield return new WaitForSeconds(2);Assert.IsTrue(actors.All(e=>!e.IsDefensivelyCrouching&&!e.GetComponent<PlayerPosture2D>().IsCrouched));
        }
    }
}
#endif
