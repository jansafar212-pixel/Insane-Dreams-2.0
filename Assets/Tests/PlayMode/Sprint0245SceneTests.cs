#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Missions;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0245SceneTests
    {
        private WarehouseMission mission;private bool background;
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.2f);mission=Object.FindAnyObjectByType<WarehouseMission>();mission.Player.GetComponent<Health>().ProtectFor(120);}
        [UnityTearDown]public IEnumerator Cleanup(){yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Application.runInBackground=background;}
        private bool Enter(MissionZone zone)
        {var z=Object.FindObjectsByType<MissionTrigger>().Single(t=>t.Zone==zone);var b=mission.Player.GetComponent<Rigidbody2D>();b.position=z.Region.bounds.center;b.linearVelocity=Vector2.zero;mission.Player.transform.position=b.position;Physics2D.SyncTransforms();return mission.EnterZone(zone,z.Region);}
        private void Defeat(int i){foreach(var p in mission.Encounters[i].Points)p.Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));}
        [UnityTest]public IEnumerator EvidenceOfficeCheckpointAndExitAreIndependentAndStillCompleteMission()
        {
            var zones=Object.FindObjectsByType<MissionTrigger>();var record=zones.Single(z=>z.Zone==MissionZone.Records);var checkpoint=zones.Single(z=>z.Zone==MissionZone.OfficeCheckpoint);Assert.IsNull(record.GetComponent<InsaneDreams.Checkpoints.CheckpointMarker>());Assert.IsFalse(record.Region.bounds.Intersects(checkpoint.Region.bounds));Assert.Greater(record.Region.bounds.min.x-checkpoint.Region.bounds.max.x,.9f);
            Assert.IsTrue(Enter(MissionZone.Entrance));Defeat(0);Assert.IsTrue(Enter(MissionZone.Storage));Defeat(1);Assert.IsTrue(Enter(MissionZone.Checkpoint));Assert.IsTrue(Enter(MissionZone.Processing));Defeat(2);
            Assert.IsTrue(Enter(MissionZone.OfficeCheckpoint));Assert.IsFalse(mission.CanInteract);Assert.AreEqual(2,mission.CheckpointCommits);Assert.IsFalse(Enter(MissionZone.OfficeCheckpoint));Assert.IsTrue(Enter(MissionZone.Records));Assert.AreEqual(2,mission.CheckpointCommits);Assert.IsTrue(mission.TryRecoverRecords());Assert.AreEqual(2,mission.CheckpointCommits);Assert.IsTrue(Enter(MissionZone.Exit));Assert.AreEqual(1,mission.Completions);yield return null;
        }
        [UnityTest]public IEnumerator RealPlayerArmorAndUnarmoredResidentsUseActualEquipment()
        {var armor=mission.Player.GetComponent<BodyArmor>();var visual=mission.Player.GetComponent<ArmorVisual>();Assert.IsNotNull(visual);armor.Restore(0);Assert.IsFalse(visual.overlay.enabled);armor.Restore(60);Assert.IsTrue(visual.overlay.enabled);Assert.AreSame(visual.intact,visual.overlay.sprite);armor.Restore(20);Assert.AreSame(visual.damaged,visual.overlay.sprite);foreach(var enemy in Object.FindObjectsByType<InsaneDreams.Enemies.EnemyCombat>(FindObjectsInactive.Include))Assert.IsNull(enemy.GetComponent<BodyArmor>());Assert.AreEqual(17,mission.Encounters.Sum(e=>e.Count));yield return null;}
        [UnityTest]public IEnumerator WarehouseLowCoverAndWestLadderDoNotStealDownwardInput()
        {
            var a=mission.Player;var data=new UnityEditor.SerializedObject(a);data.FindProperty("readKeyboard").boolValue=false;data.ApplyModifiedPropertiesWithoutUndo();
            var body=a.GetComponent<Rigidbody2D>();var traversal=a.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>();
            void Place(Vector2 p){a.SetTouchMove(Vector2.zero);traversal.ResetTraversal();a.GetComponent<InsaneDreams.Traversal.PlayerPosture2D>().ResetPosture();body.position=p;body.linearVelocity=Vector2.zero;a.transform.position=p;Physics2D.SyncTransforms();}
            // Real crate: LowCover_Sprint012 (-14,.45), west ladder x=-12.
            Place(new Vector2(-15.7f,.85f));a.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.35f);a.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.3f);Assert.IsTrue(a.GetComponent<PlayerCover2D>().IsCovered);Assert.IsFalse(traversal.IsClimbing);
            Place(new Vector2(-13.65f,1.75f));yield return new WaitForSeconds(.35f);a.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.2f);Assert.IsFalse(traversal.IsClimbing);Assert.IsTrue(a.GetComponent<InsaneDreams.Traversal.PlayerPosture2D>().IsCrouched);
            Place(new Vector2(-12.75f,1.75f));a.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.3f);Assert.IsFalse(traversal.IsClimbing,"Downward cover/crouch input must not magnetize a falling player to the mid-ladder capture zone.");Assert.AreEqual(3,body.gravityScale);Assert.Less(Mathf.Abs(body.position.x+12.75f),.05f);
        }
    }
}
#endif
