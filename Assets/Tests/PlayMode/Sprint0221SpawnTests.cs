#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class Sprint0221SpawnTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private Camera camera;
        private Health player;
        private bool background;
        private const float X = 19000;
        private GameObject Keep(GameObject go) { objects.Add(go); return go; }
        private BoxCollider2D Solid(Vector2 position, Vector2 size)
        {
            var go = Keep(new GameObject("S0221Geometry")); go.transform.position = position;
            var box = go.AddComponent<BoxCollider2D>(); box.size = size; Physics2D.SyncTransforms(); return box;
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            Solid(new Vector2(X, -.5f), new Vector2(100, 1));
            camera = Keep(new GameObject("S0221Camera")).AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 4; camera.aspect = 1;
            camera.transform.position = new Vector3(X, 1, -10);
            var go = Keep(new GameObject("S0221Player")); go.layer = 2; go.transform.position = new Vector2(X, .85f);
            go.AddComponent<BoxCollider2D>().size = new Vector2(.8f, 1.4f); player = go.AddComponent<Health>();
            var data = new SerializedObject(player); data.FindProperty("team").enumValueIndex = (int)CombatTeam.Player; data.ApplyModifiedPropertiesWithoutUndo();
            player.ProtectFor(60); Physics2D.SyncTransforms(); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            objects.Clear(); yield return null; Application.runInBackground = background;
        }
        private EnemySpawnPoint Point(float offset, float y = .85f, bool reverse = false)
        {
            var go = Keep(new GameObject("S0221Point")); go.transform.position = new Vector2(X + offset, y);
            var point = go.AddComponent<EnemySpawnPoint>(); var data = new SerializedObject(point);
            data.FindProperty("archetype").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset");
            data.FindProperty("spawnOnStart").boolValue = false;
            if (reverse) data.FindProperty("progressionDirection").vector2Value = Vector2.left;
            data.ApplyModifiedPropertiesWithoutUndo(); point.BindTarget(player); point.BindGameplayCamera(camera);
            var actor = point.Spawn(); Keep(actor.gameObject); actor.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            return point;
        }
        private void Move(float playerOffset, float cameraOffset)
        {
            player.transform.position = new Vector2(X + playerOffset, .85f);
            camera.transform.position = new Vector3(X + cameraOffset, 1, -10); Physics2D.SyncTransforms();
        }
        [UnityTest] public IEnumerator ForwardPointSpawnsBehindScreenMarginWhileRearPointStaysDeferred()
        {
            var front = Point(12); var rear = Point(-12); yield return new WaitForSeconds(.3f);
            Assert.IsTrue(front.Spawned.gameObject.activeSelf); Assert.IsFalse(front.IsPending);
            Assert.IsFalse(rear.Spawned.gameObject.activeSelf); Assert.IsTrue(rear.IsProgressionBlocked); Assert.IsTrue(rear.IsPending);
            Assert.Greater(front.Spawned.transform.position.x, camera.ViewportToWorldPoint(new Vector3(1, .5f, 10)).x + 1.25f);
        }
        [UnityTest] public IEnumerator TurningAndBacktrackingCannotRevivePassedOrDefeatedPoints()
        {
            var rear = Point(-12); yield return new WaitForSeconds(.25f);
            player.transform.localScale = new Vector3(-1, 1, 1); Move(-25, -25);
            for (int i = 0; i < 4; i++) { Assert.AreSame(rear.Spawned, rear.Spawn()); player.transform.localScale *= -1; }
            yield return new WaitForSeconds(.3f); Assert.IsFalse(rear.Spawned.gameObject.activeSelf); Assert.IsTrue(rear.IsProgressionBlocked); Assert.IsTrue(rear.IsPending);
            var forward = Point(-10); yield return new WaitForSeconds(.25f); Assert.IsTrue(forward.Spawned.gameObject.activeSelf);
            forward.Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(1000, CombatTeam.Player));
            var ragdoll = forward.Spawned.GetComponent<InsaneDreams.Visuals.RagdollOnDefeat>().LastRagdoll; if (ragdoll != null) Keep(ragdoll.gameObject);
            forward.Spawn(); yield return new WaitForSeconds(.25f);
            Assert.IsTrue(forward.Spawned.GetComponent<Health>().IsDefeated); Assert.IsFalse(forward.IsPending);
        }
        [UnityTest] public IEnumerator PendingVisiblePointCannotMaterializeBehindPlayerAfterCameraPassesIt()
        {
            var point = Point(3); yield return new WaitForSeconds(.25f); Assert.IsTrue(point.IsPending);
            Move(15, 15); yield return null; Assert.IsTrue(point.IsProgressionBlocked);
            Move(-10, -10); point.Spawn(); yield return new WaitForSeconds(.3f);
            Assert.IsFalse(point.Spawned.gameObject.activeSelf); Assert.IsTrue(point.IsPending); Assert.AreEqual(SpawnBlockReason.PassedProgress, point.BlockReason);
        }
        [UnityTest] public IEnumerator AuthoredReverseRouteDoesNotFollowPlayerFacing()
        {
            player.transform.localScale = new Vector3(1, 1, 1);
            var ahead = Point(-12, reverse: true); var behind = Point(12, reverse: true);
            yield return new WaitForSeconds(.3f); Assert.IsTrue(ahead.Spawned.gameObject.activeSelf); Assert.IsTrue(behind.IsProgressionBlocked);
        }
        [UnityTest] public IEnumerator CameraMovementAndNearPlayerLocationPostponeInsteadOfRelocating()
        {
            var point = Point(12); camera.transform.position = new Vector3(X + 12, 1, -10);
            yield return new WaitForSeconds(.3f); Assert.IsFalse(point.Spawned.gameObject.activeSelf);
            camera.transform.position = new Vector3(X, 1, -10); yield return new WaitForSeconds(.3f);
            Assert.IsTrue(point.Spawned.gameObject.activeSelf); Assert.AreEqual(X + 12, point.Spawned.transform.position.x, .01f);
            // Even when the camera pans away, proximity cannot reveal a new actor.
            Move(20, 0); var near = Point(22); yield return new WaitForSeconds(.3f);
            Assert.IsFalse(near.Spawned.gameObject.activeSelf); Assert.IsTrue(near.IsPending);
        }
        [UnityTest] public IEnumerator InvalidSupportBlockedTerrainAndCrowdedPointsWaitAtAuthoredLocations()
        {
            var first = Point(12); var crowd = Point(13); var unsupported = Point(60);
            var block = Solid(new Vector2(X + 20, 1), new Vector2(2, 3)); var blocked = Point(20);
            yield return new WaitForSeconds(.3f);
            Assert.IsTrue(first.Spawned.gameObject.activeSelf); Assert.IsTrue(crowd.IsPending);
            Assert.IsTrue(unsupported.IsPending); Assert.IsTrue(blocked.IsPending);
            Object.Destroy(block.gameObject); first.Spawned.GetComponent<Rigidbody2D>().position = new Vector2(X + 10, .85f); Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.3f); Assert.IsTrue(blocked.Spawned.gameObject.activeSelf); Assert.IsTrue(crowd.Spawned.gameObject.activeSelf);
            Assert.AreEqual(X + 13, crowd.Spawned.transform.position.x, .01f); Assert.IsFalse(unsupported.Spawned.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator RevealedEnemyPersistsBehindPlayerWithAmmoAndNoOffscreenRegularShots()
        {
            var point = Point(12); yield return new WaitForSeconds(.3f); var actor = point.Spawned;
            var combat = actor.GetComponent<EnemyCombat>(); combat.Weapon.SetAmmunition(19, 99);
            Move(25, 25); yield return new WaitForSeconds(.6f); Assert.IsTrue(actor.gameObject.activeSelf);
            Assert.AreSame(actor, point.Spawn()); Assert.AreEqual(19, combat.Weapon.MagazineAmmo); Assert.AreEqual(99, combat.Weapon.ReserveAmmo);
            Assert.AreEqual(0, actor.GetComponent<AttackExecutor>().ShotsFired); Assert.IsFalse(point.IsProgressionBlocked);
        }
        [UnityTest] public IEnumerator UnbindingPlayerPostponesAndRebindingCannotForgetVisitedProgress()
        {
            var point = Point(12); camera.transform.position = new Vector3(X + 12, 1, -10);
            yield return new WaitForSeconds(.25f); point.BindTarget(null); camera.transform.position = new Vector3(X, 1, -10);
            yield return new WaitForSeconds(.3f); Assert.IsFalse(point.Spawned.gameObject.activeSelf);
            point.BindTarget(player); Move(20, 20); yield return null;
            point.BindTarget(null); Move(0, 0); point.BindTarget(player); point.Spawn(); yield return new WaitForSeconds(.3f);
            Assert.IsTrue(point.IsProgressionBlocked); Assert.IsTrue(point.IsPending); Assert.IsFalse(point.Spawned.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator OnlyExplicitEncounterResetReopensPassedPointAndCompletedResetStaysClosed()
        {
            var point = Point(3); Move(15, 15); yield return null; Assert.IsTrue(point.IsProgressionBlocked);
            var actor = point.Spawned; Move(-10, -10); point.ResetEncounter(false, true);
            point.Spawn(); yield return new WaitForSeconds(.25f); Assert.IsFalse(actor.gameObject.activeSelf);
            point.ResetEncounter(true); yield return new WaitForSeconds(.25f);
            Assert.AreSame(actor, point.Spawned); Assert.IsTrue(actor.gameObject.activeSelf); Assert.IsFalse(point.IsProgressionBlocked);
        }
    }
}
#endif
