#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint015Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();
        private Camera camera;private GameObject player;private bool background;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private GameObject Solid(string name,Vector2 p,Vector2 size){var go=Keep(new GameObject(name));go.transform.position=p;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        private void Setting(Object target,string key,float value){var s=new SerializedObject(target);s.FindProperty(key).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private void Flag(Object target,string key,bool value){var s=new SerializedObject(target);s.FindProperty(key).boolValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;
            Solid("S015Floor",new Vector2(550,-.5f),new Vector2(120,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(540,.85f),Quaternion.identity));
            player.GetComponent<PlayerHealthRecovery>().enabled=false;
            camera=Keep(new GameObject("S015Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4;camera.aspect=1;camera.transform.position=new Vector3(540,1,-10);
            yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;}
        private EnemySpawnPoint Point(float x)
        {
            var go=Keep(new GameObject("S015Point"));go.transform.position=new Vector3(x,.85f);var p=go.AddComponent<EnemySpawnPoint>();
            var s=new SerializedObject(p);s.FindProperty("archetype").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset");s.FindProperty("spawnOnStart").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();
            p.BindGameplayCamera(camera);return p;
        }
        private EnemyCombat Enemy()
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(546,.85f),Quaternion.identity));
            go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);e.BindTarget(player.GetComponent<Health>());player.GetComponent<Health>().ProtectFor(60);return e;
        }
        private IEnumerator Engage(EnemyCombat e){for(int i=0;i<80&&e.GetComponent<AttackExecutor>().ShotsFired<2;i++)yield return new WaitForFixedUpdate();Assert.GreaterOrEqual(e.GetComponent<AttackExecutor>().ShotsFired,2);}
        [UnityTest] public IEnumerator SpawnWaitsForSafeViewThenPersistsAcrossCameraAbsence()
        {
            var p=Point(542);var actor=p.Spawn();objects.Add(actor.gameObject);Assert.IsTrue(p.IsPending);Assert.IsFalse(actor.gameObject.activeSelf);
            camera.transform.position=new Vector3(530,1,-10);yield return new WaitForSeconds(.25f);Assert.IsTrue(actor.gameObject.activeInHierarchy);Assert.IsFalse(p.IsPending);
            camera.transform.position=new Vector3(542,1,-10);yield return new WaitForSeconds(.25f);Assert.AreSame(actor,p.Spawn());Assert.IsTrue(actor.gameObject.activeInHierarchy);
            camera.transform.position=new Vector3(510,1,-10);yield return new WaitForSeconds(3.5f);Assert.IsTrue(actor.gameObject.activeInHierarchy);Assert.IsFalse(actor.GetComponent<Health>().IsDefeated);
            camera.transform.position=new Vector3(actor.transform.position.x,1,-10);yield return null;Assert.AreSame(actor,p.Spawn());Assert.IsTrue(actor.gameObject.activeInHierarchy);
        }
        [UnityTest] public IEnumerator BlockedUnsupportedAndOccupiedSpawnsPostponeWithoutRelocating()
        {
            var p=Point(552);var wall=Solid("S015Wall",new Vector2(552,1),new Vector2(2,3));var actor=p.Spawn();objects.Add(actor.gameObject);actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;Assert.IsTrue(p.IsPending);Assert.IsFalse(actor.gameObject.activeSelf);
            Object.Destroy(wall);yield return new WaitForSeconds(.25f);Assert.IsTrue(actor.gameObject.activeSelf);Assert.AreEqual(552,actor.transform.position.x,.05f);
            actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var other=Point(552);var duplicate=other.Spawn();objects.Add(duplicate.gameObject);Assert.IsTrue(other.IsPending);Assert.IsFalse(duplicate.gameObject.activeSelf);
            var unsupported=Point(620);var floating=unsupported.Spawn();objects.Add(floating.gameObject);yield return new WaitForSeconds(.25f);Assert.IsTrue(unsupported.IsPending);Assert.IsFalse(floating.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator SpawnMarginSupportsWideAndPortraitCamerasAndMultiplePoints()
        {
            camera.aspect=2;var p=Point(549);var actor=p.Spawn();objects.Add(actor.gameObject);Assert.IsFalse(actor.gameObject.activeSelf,"The safety margin extends beyond the visible edge.");
            camera.aspect=.5f;yield return new WaitForSeconds(.25f);Assert.IsTrue(actor.gameObject.activeSelf);
            var q=Point(556);var second=q.Spawn();objects.Add(second.gameObject);yield return null;Assert.IsTrue(second.gameObject.activeSelf);Assert.AreNotSame(actor,second);
            camera.transform.position=new Vector3(556,1,-10);yield return null;Assert.IsTrue(second.gameObject.activeSelf);Assert.AreSame(second,q.Spawn());
        }
        [UnityTest] public IEnumerator OffscreenAwarenessPersistsButCameraExitCancelsEveryShot()
        {
            var e=Enemy();yield return new WaitForSeconds(.6f);Assert.IsTrue(e.TargetVisible);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);
            camera.transform.position=new Vector3(544,1,-10);yield return Engage(e);int shots=e.GetComponent<AttackExecutor>().ShotsFired;var known=e.LastKnownPosition;
            camera.transform.position=new Vector3(525,1,-10);yield return new WaitForSeconds(.6f);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);Assert.IsTrue(e.TargetVisible);Assert.IsTrue(e.gameObject.activeInHierarchy);
            Solid("S015SightBlock",new Vector2(543,1),new Vector2(.2f,5));player.GetComponent<Rigidbody2D>().position=new Vector2(539,1.5f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.3f);
            Assert.AreEqual(known,e.LastKnownPosition);Assert.IsFalse(e.IsBlindFiring);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);
            yield return new WaitForSeconds(3.2f);Assert.AreEqual(EngagementState.Patrol,e.State);Assert.AreEqual(known,e.LastKnownPosition);
        }
        [UnityTest] public IEnumerator FinalCameraMovementAndMissingCameraCannotExposeANewSpawn()
        {
            var p=Point(552);var actor=p.Spawn();objects.Add(actor.gameObject);
            camera.transform.position=new Vector3(552,1,-10);yield return null;Assert.IsTrue(p.IsPending);Assert.IsFalse(actor.gameObject.activeSelf);
            camera.enabled=false;yield return new WaitForSeconds(.25f);Assert.IsTrue(p.IsPending);
            camera.enabled=true;camera.transform.position=new Vector3(530,1,-10);yield return new WaitForSeconds(.25f);Assert.IsTrue(actor.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator BlockedRetreatReloadsOffscreenThenHandlesEmptyReserves()
        {
            var e=Enemy();Solid("S015RetreatWall",new Vector2(547,1.5f),new Vector2(.6f,3));e.Weapon.SetAmmunition(0,27);
            for(int i=0;i<60&&!e.Weapon.Cycle.IsReloading;i++)yield return new WaitForFixedUpdate();Assert.IsTrue(e.Weapon.Cycle.IsReloading);var destination=e.RetreatDestination;
            camera.transform.position=new Vector3(520,1,-10);yield return new WaitForSeconds(4.3f);Assert.IsFalse(e.Weapon.Cycle.IsReloading);Assert.AreEqual(27,e.Weapon.MagazineAmmo);Assert.AreEqual(0,e.Weapon.ReserveAmmo);Assert.AreEqual(destination,e.RetreatDestination);
            e.Weapon.SetAmmunition(0,0);int shots=e.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(.4f);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);Assert.IsFalse(e.Weapon.Cycle.IsReloading);
        }
        [UnityTest] public IEnumerator ExplicitOffscreenCapabilityStillRequiresRealDetectionAndSight()
        {
            var e=Enemy();Flag(e,"allowOffscreenFire",true);yield return Engage(e);Assert.IsFalse(e.InGameplayViewport);
            Solid("S015CapabilityWall",new Vector2(543,1),new Vector2(.3f,6));Setting(e,"ordinaryBlindChance",0);Setting(e,"cowardBlindChance",0);Setting(e,"overconfidentBlindChance",0);
            yield return new WaitForSeconds(.2f);int shots=e.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(.3f);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator OnscreenMemoryBurstExpiresWithoutTrackingThroughCover()
        {
            var e=Enemy();camera.transform.position=new Vector3(544,1,-10);foreach(var k in new[]{"ordinaryBlindChance","cowardBlindChance","overconfidentBlindChance"})Setting(e,k,1);
            Setting(e,"blindFireSeconds",.4f);yield return Engage(e);var known=e.LastKnownPosition;
            Solid("S015MemoryWall",new Vector2(543,1),new Vector2(.3f,6));player.GetComponent<Rigidbody2D>().position=new Vector2(538,2);Physics2D.SyncTransforms();int shots=e.GetComponent<AttackExecutor>().ShotsFired;
            yield return new WaitForSeconds(.3f);Assert.IsTrue(e.IsBlindFiring);Assert.AreEqual(known,e.LastKnownPosition);Assert.Greater(e.GetComponent<AttackExecutor>().ShotsFired,shots);
            yield return new WaitForSeconds(.3f);Assert.IsFalse(e.IsBlindFiring);shots=e.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(.25f);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator HealingCapsRepeatsHonorsCooldownAndCannotResurrect()
        {
            var h=player.GetComponent<Health>();var healing=player.GetComponent<PlayerHealing>();Assert.IsNotNull(healing);Assert.IsFalse(healing.TryHeal());Setting(healing,"damageInterruptionSeconds",0);
            h.ApplyDamage(new DamageInfo(80,CombatTeam.Enemy));player.GetComponent<PlayerActions>().RequestHeal();Assert.AreEqual(20,h.Current,"Held intent must not heal instantly.");
            yield return new WaitForSeconds(.4f);Assert.That(h.Current,Is.InRange(28,35));Assert.Greater(healing.FeedbackUntil,Time.time);
            yield return new WaitForSeconds(3.05f);Assert.AreEqual(100,h.Current);Assert.AreEqual(120,healing.DuctTape,.01f);Assert.IsFalse(healing.TryHeal());
            h.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));yield return new WaitForSeconds(.1f);Assert.IsTrue(h.IsDefeated);Assert.IsFalse(healing.TryHeal());Assert.AreEqual(0,h.Current);
        }
        [UnityTest] public IEnumerator LimitedHealingDoesNotConsumeUsesOnRejectedAttempts()
        {
            var healing=player.GetComponent<PlayerHealing>();Setting(healing,"initialDuctTape",5);Setting(healing,"damageInterruptionSeconds",0);healing.ResetSupply();
            Assert.IsFalse(healing.TryHeal());Assert.AreEqual(5,healing.DuctTape);player.GetComponent<Health>().ApplyDamage(new DamageInfo(70,CombatTeam.Enemy));Assert.IsTrue(healing.TryHeal());Assert.AreEqual(30,player.GetComponent<Health>().Current);
            yield return new WaitForSeconds(.4f);Assert.AreEqual(35,player.GetComponent<Health>().Current,.01f);Assert.AreEqual(0,healing.DuctTape,.001f);Assert.IsFalse(healing.TryHeal());
        }
        [UnityTest] public IEnumerator FiveFullReloadsAndReserveDepletionRemainFinite()
        {
            var e=Enemy();e.BindTarget(null);Assert.AreEqual(27,e.Weapon.MagazineAmmo);Assert.AreEqual(135,e.Weapon.ReserveAmmo);
            for(int reload=0;reload<5;reload++)
            {
                float t=Time.time+reload*10;for(int shot=0;shot<27;shot++)Assert.IsTrue(e.Weapon.Cycle.TryAttack(t+shot*.11f));Assert.IsFalse(e.Weapon.Cycle.TryAttack(t+3));
                Assert.IsTrue(e.Weapon.Cycle.TryReload(t+3));Assert.IsFalse(e.Weapon.Cycle.TryAttack(t+4));e.Weapon.Cycle.Tick(t+7.01f);Assert.AreEqual(27,e.Weapon.MagazineAmmo);Assert.AreEqual(135-(reload+1)*27,e.Weapon.ReserveAmmo);
            }
            e.Weapon.SetAmmunition(0,0);Assert.IsFalse(e.Weapon.Cycle.TryReload(Time.time+60));Assert.IsFalse(e.Weapon.Cycle.TryAttack(Time.time+60));yield return null;
        }
        [UnityTest] public IEnumerator CrouchKeepsRigidProportionsGroundedFeetAndSafeCeilingTransitions()
        {
            var actions=player.GetComponent<PlayerActions>();var rig=player.GetComponent<CharacterVisualDriver>().Rig;var box=player.GetComponent<BoxCollider2D>();float feet=box.bounds.min.y;
            actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.25f);Assert.IsTrue(player.GetComponent<PlayerPosture2D>().IsCrouched);
            foreach(var part in new[]{rig.head,rig.torso,rig.leftArm,rig.leftLeg,rig.rightLeg})Assert.AreEqual(Vector3.one,part.localScale);
            Assert.AreEqual(feet,box.bounds.min.y,.03f);Assert.AreEqual(feet,rig.leftLeg.GetComponent<SpriteRenderer>().bounds.min.y,.04f);Assert.LessOrEqual(rig.head.GetComponent<SpriteRenderer>().bounds.max.y,box.bounds.max.y+.05f);
            var ceiling=Solid("S015LowCeiling",new Vector2(540,1.3f),new Vector2(3,.5f));actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.2f);Assert.IsTrue(player.GetComponent<PlayerPosture2D>().IsCrouched);Assert.AreEqual(feet,box.bounds.min.y,.03f);
            Object.Destroy(ceiling);yield return new WaitForSeconds(.25f);Assert.IsFalse(player.GetComponent<PlayerPosture2D>().IsCrouched);Assert.AreEqual(feet,box.bounds.min.y,.03f);
            actions.SetTouchMove(Vector2.down);actions.SetTouchAim(Vector2.right);actions.SetFireIntent(true);yield return new WaitForSeconds(.25f);Assert.Greater(player.GetComponent<AttackExecutor>().ShotsFired,0);Assert.IsTrue(player.GetComponent<PlayerPosture2D>().IsCrouched);Assert.AreEqual(1,rig.torso.localScale.y);
        }
    }
}
#endif
