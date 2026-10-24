#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using InsaneDreams.CameraSystem;
using InsaneDreams.Checkpoints;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Level;
using InsaneDreams.Missions;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace InsaneDreams.Tests
{
    public sealed class Sprint014MissionTests
    {
        private WarehouseMission mission;private MissionTrigger[] zones;private bool background;
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Mission01.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;
            var roots=SceneManager.GetActiveScene().GetRootGameObjects();mission=roots.SelectMany(g=>g.GetComponentsInChildren<WarehouseMission>()).Single();zones=roots.SelectMany(g=>g.GetComponentsInChildren<MissionTrigger>()).ToArray();mission.Player.GetComponent<Health>().ProtectFor(60);
            // These cases exercise checkpoint fallback; in-place injection revival has separate coverage.
            mission.Player.GetComponent<PlayerRevive>()?.RestoreInventory(0);
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Application.runInBackground=background;}
        private bool Enter(MissionZone kind)
        {
            var zone=zones.Single(z=>z.Zone==kind);var p=mission.Player;var body=p.GetComponent<Rigidbody2D>();body.position=zone.Region.bounds.center;body.linearVelocity=Vector2.zero;p.transform.position=body.position;Physics2D.SyncTransforms();return mission.EnterZone(kind,zone.Region);
        }
        private void Defeat(int group)
        {foreach(var point in mission.Encounters[group].Points)point.Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player,mission.Player.gameObject));Assert.IsTrue(mission.Encounters[group].Cleared);}
        private void StorageCheckpoint(){Assert.IsTrue(Enter(MissionZone.Entrance));Defeat(0);Assert.IsTrue(Enter(MissionZone.Storage));Defeat(1);Assert.IsTrue(Enter(MissionZone.Checkpoint));}
        private void Records(){StorageCheckpoint();Assert.IsTrue(Enter(MissionZone.Processing));Defeat(2);Assert.IsTrue(Enter(MissionZone.OfficeCheckpoint));Assert.IsTrue(Enter(MissionZone.Records));Assert.IsTrue(mission.TryRecoverRecords());}
        private EnemyCombat[] Actors()=>SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<EnemyCombat>(true)).ToArray();
        [UnityTest] public IEnumerator DisabledOfficeMarkerCannotStealCheckpointBeforeRecordsAreAuthorized()
        {
            StorageCheckpoint();var recovery=mission.Player.GetComponent<PlayerRecovery>();var saved=recovery.CurrentSpawn;
            Assert.IsFalse(Enter(MissionZone.Records));yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            Assert.AreEqual(saved,recovery.CurrentSpawn,"Disabled marker callbacks must not bypass the mission commit.");
            var health=mission.Player.GetComponent<Health>();health.ProtectFor(0);health.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));
            Assert.AreEqual(saved,(Vector2)mission.Player.transform.position);Assert.AreEqual(MissionStage.ReachProcessing,mission.Stage);Assert.AreEqual(100,health.Current);
        }
        [UnityTest] public IEnumerator InitializationHasSafeSpawnRealHudAndOnlyTenPrewarmedInactiveEnemies()
        {
            Assert.AreEqual(MissionStage.EnterWarehouse,mission.Stage);Assert.AreEqual(new Vector2(-24,.85f),mission.Player.GetComponent<PlayerRecovery>().CurrentSpawn);Assert.AreEqual(100,mission.Player.GetComponent<Health>().Current);
            Assert.AreEqual(10,Actors().Length);Assert.AreEqual(10,Actors().Count(a=>a.gameObject.activeInHierarchy));Assert.AreEqual(0,mission.Deaths);Assert.AreEqual(0,mission.Kills);Assert.IsFalse(mission.HasRecords);
            Assert.AreEqual(24,mission.Player.GetComponent<PlayerLoadout>().SelectedWeapon.MagazineAmmo);Assert.IsFalse(mission.Player.GetComponent<PlayerLoadout>().SelectedWeapon.Cycle.IsReloading);
            var root=mission.GetComponentInChildren<UIDocument>();var doc=SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<UIDocument>()).Single();Assert.AreEqual(mission.Objective,doc.rootVisualElement.Q<Label>("objective").text);
            Assert.IsTrue(SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<LevelEntry>()).Single().IsBound);yield return new WaitForSeconds(1);
            Assert.AreEqual(10,Actors().Length,"Natural perception must not spawn duplicate scene occupants.");
            mission.Encounters[1].Points[0].Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(1,CombatTeam.Player,mission.Player.gameObject));Assert.IsTrue(mission.Encounters[1].IsActive,"A real player hit must alert visible occupants before their region trigger.");Assert.AreEqual(MissionStage.EnterWarehouse,mission.Stage,"A legitimate alert must not bypass objective gates.");
        }
        [UnityTest] public IEnumerator PhysicalColliderEdgeEntryActivatesBeforePivotReachesTriggerInterior()
        {
            mission.Player.SetTouchMove(new Vector2(1,-1));
            for(int i=0;i<160&&mission.Stage==MissionStage.EnterWarehouse;i++)yield return new WaitForFixedUpdate();
            mission.Player.SetTouchMove(Vector2.zero);Assert.AreEqual(MissionStage.ClearStorage,mission.Stage);Assert.IsTrue(mission.Encounters[0].IsActive);
            Assert.IsFalse(zones.Single(z=>z.Zone==MissionZone.Entrance).Region.OverlapPoint(mission.Player.GetComponent<BoxCollider2D>().bounds.center),"Initial collider contact occurs before center entry.");
        }
        [UnityTest] public IEnumerator MissionZonesDoNotObstructRealWestLadderMezzanineLanding()
        {
            yield return new WaitForSeconds(.35f); // Preserve the existing recovery/reattachment lockout.
            var p=mission.Player;var body=p.GetComponent<Rigidbody2D>();body.position=new Vector2(-12,.85f);p.transform.position=body.position;Physics2D.SyncTransforms();p.SetTouchMove(Vector2.up);
            for(int i=0;i<110&&body.position.y<6.15f;i++)yield return new WaitForFixedUpdate();
            Assert.IsTrue(p.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>().IsClimbing);p.SetTouchMove(Vector2.right);
            for(int i=0;i<35;i++)yield return new WaitForFixedUpdate();p.SetTouchMove(Vector2.zero);
            Assert.IsFalse(p.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>().IsClimbing);Assert.Greater(body.position.x,-10.3f);Assert.That(body.position.y,Is.InRange(5.95f,6.3f));
        }
        [UnityTest] public IEnumerator ZoneProgressionRequiresRealRegionAndCannotSkipGuardOrRepeatActivation()
        {
            var entrance=zones.Single(z=>z.Zone==MissionZone.Entrance);Assert.IsFalse(mission.EnterZone(MissionZone.Entrance,entrance.Region),"Remote calls must not activate encounters.");
            Assert.IsFalse(Enter(MissionZone.Storage));Assert.AreEqual(MissionStage.EnterWarehouse,mission.Stage);Assert.IsTrue(Enter(MissionZone.Entrance));Assert.IsFalse(Enter(MissionZone.Entrance));
            Assert.IsTrue(mission.Encounters[0].IsActive);Assert.IsFalse(Enter(MissionZone.Storage),"Natural awareness must not bypass the guard objective.");Defeat(0);Assert.IsTrue(Enter(MissionZone.Storage));Assert.IsFalse(Enter(MissionZone.Storage));
            Assert.AreEqual(9,Actors().Count(a=>a.gameObject.activeInHierarchy));Defeat(1);Assert.AreEqual(MissionStage.ReachProcessing,mission.Stage);Assert.IsFalse(Enter(MissionZone.Processing));Assert.IsTrue(Enter(MissionZone.Checkpoint));Assert.AreEqual(1,mission.CheckpointCommits);Assert.IsFalse(Enter(MissionZone.Checkpoint));yield return null;
        }
        [UnityTest] public IEnumerator ExitAndRemoteRecordsStayLockedUntilBothEncountersAndInteractionComplete()
        {
            Assert.IsFalse(Enter(MissionZone.Exit));Assert.AreEqual(0,mission.Completions);Assert.IsFalse(mission.TryRecoverRecords());
            StorageCheckpoint();Assert.IsTrue(Enter(MissionZone.Processing));Assert.IsFalse(Enter(MissionZone.Exit));Assert.IsFalse(Enter(MissionZone.Records));Assert.IsFalse(mission.TryRecoverRecords());
            Assert.IsFalse(Enter(MissionZone.Exit));Defeat(2);Assert.IsFalse(mission.TryRecoverRecords());var body=mission.Player.GetComponent<Rigidbody2D>();body.position=new Vector2(18,6.25f);mission.Player.transform.position=body.position;Physics2D.SyncTransforms();Assert.IsFalse(mission.TryRecoverRecords(),"Records need a close approach on the same floor.");Assert.IsTrue(Enter(MissionZone.Records));mission.Player.RequestInteraction();Assert.IsTrue(mission.HasRecords);Assert.AreEqual(MissionStage.Escape,mission.Stage);Assert.IsFalse(mission.TryRecoverRecords());Assert.AreEqual(1,mission.CheckpointCommits,"Collecting evidence must not also activate a checkpoint.");yield return null;
        }
        [UnityTest] public IEnumerator CheckpointRollbackPreservesClearedGroupsRestoresAmmoAndNeverDuplicatesActors()
        {
            var before=Actors();StorageCheckpoint();var weapon=mission.Player.GetComponent<PlayerLoadout>().SelectedWeapon;int ammo=weapon.MagazineAmmo,reserve=weapon.ReserveAmmo;
            Assert.IsTrue(Enter(MissionZone.Processing));foreach(var point in mission.Encounters[2].Points.Take(2))point.Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player,mission.Player.gameObject));Assert.AreEqual(7,mission.Kills);weapon.SetAmmunition(0,0);
            for(int attempt=0;attempt<2;attempt++)
            {
                var health=mission.Player.GetComponent<Health>();health.ProtectFor(0);Assert.IsTrue(health.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy)));Assert.AreEqual(attempt+1,mission.Deaths);Assert.AreEqual(100,health.Current);
                Assert.AreEqual(MissionStage.ReachProcessing,mission.Stage);Assert.AreEqual(5,mission.Kills);Assert.AreEqual(ammo,weapon.MagazineAmmo);Assert.AreEqual(reserve,weapon.ReserveAmmo);Assert.IsTrue(mission.Encounters[0].Cleared);Assert.IsTrue(mission.Encounters[1].Cleared);Assert.AreEqual(0,mission.Encounters[2].DefeatedCount);Assert.IsFalse(mission.Encounters[2].IsActive);
                Assert.AreEqual(7,mission.TotalDefeats,"Results retain real defeat events even when encounter progress rolls back.");
                Assert.Less(Vector2.Distance(mission.Player.transform.position,new Vector2(1,6.25f)),.02f);Assert.AreEqual(10,Actors().Length);CollectionAssert.AreEquivalent(before,Actors());yield return null;
            }
        }
        [UnityTest] public IEnumerator RecordsCheckpointCompletionResultsAndReplayReflectActualEventsExactlyOnce()
        {
            var actors=Actors();var supplies=mission.Player.GetComponent<PlayerSupplies>();Assert.IsTrue(supplies.CollectWallet("s023-results",42));Records();var health=mission.Player.GetComponent<Health>();health.ProtectFor(0);health.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));Assert.AreEqual(1,mission.Deaths);Assert.IsTrue(mission.HasRecords);Assert.AreEqual(MissionStage.Escape,mission.Stage);Assert.AreEqual(42,mission.Earnings);Assert.IsFalse(supplies.CollectWallet("s023-results",42));
            Assert.IsTrue(Enter(MissionZone.Exit));Assert.IsFalse(Enter(MissionZone.Exit));Assert.AreEqual(1,mission.Completions);Assert.AreEqual(10,mission.Kills);float time=mission.Elapsed;yield return new WaitForSeconds(.2f);Assert.AreEqual(time,mission.Elapsed);Assert.IsFalse(mission.Player.enabled);
            var doc=SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<UIDocument>()).Single();var root=doc.rootVisualElement;Assert.IsFalse(root.Q("missionResults").ClassListContains("hidden"));StringAssert.Contains("ENEMIES DEFEATED   10",root.Q<Label>("resultStats").text);StringAssert.Contains("DEATHS   1",root.Q<Label>("resultStats").text);
            StringAssert.Contains("MISSION EARNINGS   $42",root.Q<Label>("resultStats").text);Assert.IsFalse(supplies.CollectWallet("after-completion",99));Assert.AreEqual(42,mission.Earnings);
            var button=root.Q<Button>("replayMission");using(var evt=NavigationSubmitEvent.GetPooled()){evt.target=button;button.SendEvent(evt);}yield return null;
            Assert.AreEqual(MissionStage.EnterWarehouse,mission.Stage);Assert.AreEqual(0,mission.Deaths);Assert.AreEqual(0,mission.Kills);Assert.AreEqual(0,mission.TotalDefeats);Assert.IsFalse(mission.HasRecords);Assert.Less(mission.Elapsed,.3f);Assert.IsTrue(mission.Player.enabled);Assert.AreEqual(10,Actors().Count(a=>!a.GetComponent<Health>().IsDefeated));Assert.AreEqual(10,Actors().Count(a=>a.gameObject.activeInHierarchy));Assert.AreEqual(DisplayStyle.Flex,root.Q("missionResetCover").resolvedStyle.display);CollectionAssert.AreEquivalent(actors,Actors());Assert.IsTrue(root.Q("missionResults").ClassListContains("hidden"));
            Assert.AreEqual(0,mission.Earnings);Assert.AreEqual(200,mission.Player.GetComponent<PlayerHealing>().DuctTape);Assert.IsFalse(root.Q<Label>("resultStats").text.Contains("MISSION EARNINGS"));
        }
        [UnityTest] public IEnumerator MissionEnemyKeepsCameraGateHitscanAndFourSecondReloadRules()
        {
            Assert.IsTrue(Enter(MissionZone.Entrance));var actor=mission.Encounters[0].Points[0].Spawned;var ai=actor.GetComponent<EnemyCombat>();var body=actor.GetComponent<Rigidbody2D>();body.constraints=RigidbodyConstraints2D.FreezeAll;
            var camera=Camera.main;var follow=camera.GetComponent<WarehouseCameraFollow>();follow.enabled=false;camera.transform.position=new Vector3(-26,1,-10);camera.orthographicSize=1;ai.BindGameplayCamera(camera);
            yield return new WaitForSeconds(.65f);Assert.IsFalse(ai.InGameplayViewport);Assert.AreEqual(0,actor.GetComponent<AttackExecutor>().ShotsFired);
            camera.transform.position=new Vector3(-16,1,-10);camera.orthographicSize=7;mission.Player.GetComponent<Rigidbody2D>().position=new Vector2(-15,.71f);Physics2D.SyncTransforms();
            for(int i=0;i<70&&actor.GetComponent<AttackExecutor>().ShotsFired==0;i++)yield return new WaitForFixedUpdate();Assert.Greater(actor.GetComponent<AttackExecutor>().ShotsFired,0);
            ai.BindTarget(null);ai.Weapon.SetAmmunition(0,36);Assert.IsTrue(ai.Weapon.Cycle.TryReload(Time.time));int shots=actor.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(3.6f);Assert.IsTrue(ai.Weapon.Cycle.IsReloading);Assert.AreEqual(0,ai.Weapon.MagazineAmmo);Assert.AreEqual(shots,actor.GetComponent<AttackExecutor>().ShotsFired);yield return new WaitForSeconds(.5f);Assert.AreEqual(27,ai.Weapon.MagazineAmmo);Assert.AreEqual(9,ai.Weapon.ReserveAmmo);
        }
    }
}
#endif
