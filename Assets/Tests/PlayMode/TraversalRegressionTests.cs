#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using InsaneDreams.Checkpoints;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class TraversalRegressionTests
    {
        private GameObject player;
        private Rigidbody2D body;
        private PlayerActions actions;
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private readonly List<GameObject> created = new List<GameObject>();
        private bool background;
        private GameObject Keep(GameObject go) { created.Add(go); return go; }
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            var ground = Keep(new GameObject("TestFloor")); ground.transform.position = new Vector3(1000, -.5f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(60, 1);
            player = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab")));
            player.transform.position = new Vector3(1000, .85f);
            body = player.GetComponent<Rigidbody2D>(); actions = player.GetComponent<PlayerActions>();
            motor = player.GetComponent<PlayerMotor2D>(); traversal = player.GetComponent<PlayerTraversal2D>();
            actions.ResetActions(); Physics2D.SyncTransforms(); yield return new WaitForSeconds(.3f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in created) if (go != null) Object.Destroy(go);
            created.Clear(); yield return null; Application.runInBackground = background;
        }
        private LadderVolume Ladder()
        {
            var go = Keep(new GameObject("TestLadder")); go.transform.position = new Vector3(1000, 1.5f);
            var box = go.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(2, 5);
            Physics2D.SyncTransforms(); return go.AddComponent<LadderVolume>();
        }
        [UnityTest] public IEnumerator DiagonalJumpIsSingleUntilNeutralAndAirborneRequestsAreRejected()
        {
            Assert.IsTrue(motor.IsGrounded); float initial = body.position.y;
            actions.SetTouchMove(new Vector2(.7f, .7f)); yield return new WaitForSeconds(.15f);
            Assert.Greater(body.position.y, initial + .5f); Assert.Greater(body.linearVelocity.x, 0);
            actions.SetTouchMove(Vector2.zero); yield return null;
            actions.SetTouchMove(Vector2.up); float rising = body.linearVelocity.y;
            yield return new WaitForSeconds(.08f); Assert.Less(body.linearVelocity.y, rising);
            yield return new WaitForSeconds(1f); Assert.IsTrue(motor.IsGrounded);
            yield return new WaitForSeconds(.2f); Assert.Less(body.position.y, 1f);
            actions.SetTouchMove(Vector2.zero); yield return null;
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.15f);
            Assert.Greater(body.position.y, 1.2f);
        }
        [UnityTest] public IEnumerator LadderTakesPriorityAndDisabledOrDestroyedLadderRestoresGravity()
        {
            var ladder = Ladder(); actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.2f);
            Assert.IsTrue(traversal.IsClimbing); Assert.AreEqual(0, body.gravityScale);
            ladder.enabled = false; yield return new WaitForSeconds(.1f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(3, body.gravityScale);
            actions.SetTouchMove(Vector2.zero); traversal.ResetTraversal(); body.position = new Vector2(1000, 1);
            ladder.enabled = true; Physics2D.SyncTransforms(); yield return new WaitForSeconds(.35f);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.15f); Assert.IsTrue(traversal.IsClimbing);
            Object.Destroy(ladder.gameObject); yield return new WaitForSeconds(.1f);
            Assert.IsFalse(traversal.IsClimbing); Assert.IsTrue(motor.enabled); Assert.AreEqual(3, body.gravityScale);
        }
        [UnityTest] public IEnumerator LadderTopAndDiagonalArbitrationDoNotGenerateExtraJump()
        {
            // Top assistance now requires an actual landing rather than teleporting into empty space.
            var landing = Keep(new GameObject("TopLanding")); landing.transform.position = new Vector3(1002.5f, 2.9f);
            landing.AddComponent<BoxCollider2D>().size = new Vector2(3, .3f);
            var ladder = Ladder(); actions.SetTouchMove(new Vector2(.7f, .7f));
            yield return new WaitForSeconds(.15f); Assert.IsFalse(traversal.IsClimbing); Assert.Greater(body.linearVelocity.y, 3);
            actions.ResetActions(); traversal.ResetTraversal(); body.position = new Vector2(1000, 3.7f); body.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms(); yield return new WaitForSeconds(.35f);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.9f); // Includes the new smooth horizontal exit.
            Assert.IsFalse(traversal.IsClimbing); Assert.Greater(body.position.x, 1001);
            Assert.LessOrEqual(body.linearVelocity.y, 0); Assert.AreEqual(3, body.gravityScale);
        }
        [UnityTest] public IEnumerator ParachuteCapReleaseAndCheckpointReset()
        {
            body.position = new Vector2(1000, 12); body.linearVelocity = Vector2.down * 15;
            actions.SetTouchParachute(true); yield return new WaitForSeconds(.2f);
            Assert.IsTrue(traversal.IsParachuting); Assert.GreaterOrEqual(body.linearVelocity.y, -2.85f);
            actions.SetTouchParachute(false); yield return new WaitForSeconds(.2f);
            Assert.IsTrue(traversal.IsParachuting, "Release no longer retracts a toggle parachute");
            actions.SetTouchParachute(true); actions.SetTouchParachute(false); yield return new WaitForSeconds(.2f);
            Assert.IsFalse(traversal.IsParachuting); Assert.AreEqual(3, body.gravityScale); Assert.Less(body.linearVelocity.y, -5);
            actions.SetTouchMove(Vector2.right); actions.SetTouchParachute(true);
            player.GetComponent<PlayerRecovery>().Respawn(); yield return null;
            Assert.AreEqual(Vector2.zero, actions.Move); Assert.IsFalse(traversal.IsParachuting);
            Assert.AreEqual(3, body.gravityScale); Assert.Less(body.position.y, 1);
        }
        [UnityTest] public IEnumerator AimAndFireIntentionAreIndependentFromMovement()
        {
            actions.SetTouchMove(Vector2.left); actions.SetTouchAim(new Vector2(1, 1)); actions.SetFireIntent(true);
            yield return null; Assert.Less(actions.Move.x, 0); Assert.Greater(actions.AimDirection.x, 0);
            Assert.Greater(actions.AimDirection.y, 0); Assert.IsTrue(actions.FireHeld);
            actions.SetTouchAim(Vector2.zero); yield return null; Assert.Greater(actions.AimDirection.x, 0);
            actions.ResetActions(); yield return null; Assert.IsFalse(actions.FireHeld);
        }
        private static Touch TouchData(int finger, Vector2 position, TouchPhase phase) =>
            new Touch { fingerId = finger, position = position, phase = phase };
        private static void Down(VisualElement target, int finger, Vector2 position)
        {
            using (var evt = PointerDownEvent.GetPooled(TouchData(finger, position, TouchPhase.Began), EventModifiers.None)) target.SendEvent(evt);
        }
        private static void Up(VisualElement target, int finger)
        {
            using (var evt = PointerUpEvent.GetPooled(TouchData(finger, target.worldBound.center, TouchPhase.Ended), EventModifiers.None)) target.SendEvent(evt);
        }
        [UnityTest] public IEnumerator IndependentTouchPointersCancellationFocusAndRespawnClearOwnership()
        {
            var ui = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));
            var controls = ui.GetComponent<WarehouseTouchControls>(); controls.Bind(actions);
            yield return null; yield return null;
            var root = ui.GetComponent<UIDocument>().rootVisualElement;
            Assert.IsNull(root.Q<VisualElement>("jumpButton"));
            var move = root.Q<VisualElement>("moveStick"); var aim = root.Q<VisualElement>("aimStick");
            var chute = root.Q<VisualElement>("parachuteButton");
            Down(move, 0, move.worldBound.center + Vector2.right * 40);
            Down(move, 1, move.worldBound.center + Vector2.left * 40); // Must not hijack finger zero.
            Down(aim, 2, aim.worldBound.center + new Vector2(35, -35));
            Down(chute, 3, chute.worldBound.center);
            yield return null;
            Assert.Greater(actions.Move.x, .4f); Assert.Greater(actions.AimDirection.y, .4f); Assert.IsTrue(actions.FireHeld);
            Up(move, 1); Up(aim, 2); yield return null;
            Assert.Greater(actions.Move.x, .4f); Assert.IsFalse(actions.FireHeld);
            Assert.IsTrue(chute.HasPointerCapture(4));
            using (var evt = PointerCancelEvent.GetPooled(TouchData(0, move.worldBound.center, TouchPhase.Canceled), EventModifiers.None)) move.SendEvent(evt);
            yield return null; Assert.AreEqual(Vector2.zero, actions.Move); Assert.IsTrue(chute.HasPointerCapture(4));
            Down(move, 0, move.worldBound.center + Vector2.right * 40); yield return null;
            player.GetComponent<PlayerRecovery>().Respawn(); yield return null;
            Assert.IsFalse(move.HasPointerCapture(1)); Assert.IsFalse(chute.HasPointerCapture(4));
            Assert.AreEqual(Vector2.zero, actions.Move);
            Down(move, 0, move.worldBound.center + Vector2.right * 40); yield return null;
            controls.SendMessage("OnApplicationFocus", false); yield return null;
            Assert.AreEqual(Vector2.zero, actions.Move); Assert.IsFalse(move.HasPointerCapture(1));
        }
        [UnityTest] public IEnumerator LevelEntryRebindsSpawnCameraAndControlsWithoutSceneNames()
        {
            var ui = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));
            var controls = ui.GetComponent<WarehouseTouchControls>(); yield return null;
            var cameraObject = Keep(new GameObject("TestCamera")); cameraObject.AddComponent<Camera>();
            var follow = cameraObject.AddComponent<InsaneDreams.CameraSystem.WarehouseCameraFollow>();
            var spawnA = Keep(new GameObject("SpawnA")).transform; spawnA.position = new Vector3(1003, .85f);
            var spawnB = Keep(new GameObject("SpawnB")).transform; spawnB.position = new Vector3(1005, .85f);
            var entry = Keep(new GameObject("Entry")).AddComponent<InsaneDreams.Level.LevelEntry>();
            entry.Configure(actions, spawnA, follow, controls); Assert.IsTrue(entry.Bind()); yield return null;
            Assert.AreSame(player.transform, follow.Target); Assert.IsTrue(controls.IsBound);
            Assert.That(body.position.x, Is.EqualTo(1003).Within(.05f));
            entry.Configure(actions, spawnB, follow, controls); Assert.IsTrue(entry.Bind()); yield return null;
            Assert.That(body.position.x, Is.EqualTo(1005).Within(.05f));
            Assert.AreEqual((Vector2)spawnB.position, player.GetComponent<PlayerRecovery>().CurrentSpawn);
            entry.Unbind(); Assert.IsNull(follow.Target); Assert.IsFalse(controls.IsBound);
        }
        [UnityTest] public IEnumerator TypedSpawnResolvesArchetypeAndMissingOptionalReferencesAreSafe()
        {
            var archetype = AssetDatabase.LoadAssetAtPath<InsaneDreams.Enemies.EnemyArchetype>("Assets/Data/Enemies/Larper.asset");
            var marker = Keep(new GameObject("TypedSpawn")); marker.transform.position = new Vector3(1000, .85f);
            var point = marker.AddComponent<InsaneDreams.Enemies.EnemySpawnPoint>();
            var serialized = new SerializedObject(point); serialized.FindProperty("archetype").objectReferenceValue = archetype;
            serialized.ApplyModifiedPropertiesWithoutUndo(); Assert.IsNull(point.ValidationError);
            var enemy = point.Spawn(); Keep(enemy.gameObject); Assert.AreSame(enemy, point.Spawn());
            yield return new WaitForSeconds(.4f); Assert.AreSame(archetype, enemy.Archetype);
            Assert.AreEqual(InsaneDreams.Enemies.LocomotionState.Walking, enemy.State);
            Assert.Greater(enemy.GetComponent<Rigidbody2D>().linearVelocity.x, 0);
            var blank = Keep(new GameObject("UnconfiguredEnemy")); blank.layer = 2;
            blank.transform.position = new Vector3(1010, .85f); var safe = blank.AddComponent<InsaneDreams.Enemies.LarperBrain>();
            yield return new WaitForSeconds(.1f); Assert.AreEqual(InsaneDreams.Enemies.LocomotionState.Unconfigured, safe.State);
        }
        [UnityTest] public IEnumerator GroundOnlyEnemyStopsAtUnsafeLedgeAndStationaryCapabilityWins()
        {
            var profile = Object.Instantiate(AssetDatabase.LoadAssetAtPath<InsaneDreams.Enemies.EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            var settings = new SerializedObject(profile); settings.FindProperty("capabilities").intValue = (int)InsaneDreams.Enemies.MovementCapabilities.Walk;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var enemy = Keep(Object.Instantiate(profile.Prefab, new Vector3(1029, .85f), Quaternion.identity));
            var brain = enemy.GetComponent<InsaneDreams.Enemies.LarperBrain>(); brain.Configure(profile);
            yield return new WaitForSeconds(.9f);
            Assert.Less(enemy.transform.position.x, 1029.7f); Assert.Greater(enemy.transform.position.y, 0);
            settings.Update(); settings.FindProperty("capabilities").intValue = (int)(InsaneDreams.Enemies.MovementCapabilities.Walk | InsaneDreams.Enemies.MovementCapabilities.Stationary);
            settings.ApplyModifiedPropertiesWithoutUndo(); yield return new WaitForSeconds(.3f);
            Assert.AreEqual(InsaneDreams.Enemies.LocomotionState.Stationary, brain.State);
            Assert.That(enemy.GetComponent<Rigidbody2D>().linearVelocity.x, Is.EqualTo(0).Within(.05f));
            Object.Destroy(profile);
        }
        [UnityTest] public IEnumerator LarperWalksAndUsesPermittedJumpAcrossObstacle()
        {
            var obstacle = Keep(new GameObject("JumpObstacle")); obstacle.transform.position = new Vector3(1003, .5f);
            obstacle.AddComponent<BoxCollider2D>().size = Vector2.one;
            var profile = AssetDatabase.LoadAssetAtPath<InsaneDreams.Enemies.EnemyArchetype>("Assets/Data/Enemies/Larper.asset");
            var enemy = Keep(Object.Instantiate(profile.Prefab, new Vector3(1000, .85f), Quaternion.identity));
            var brain = enemy.GetComponent<InsaneDreams.Enemies.LarperBrain>(); brain.Configure(profile);
            bool walked = false, jumped = false; float maxHeight = 0;
            for (float time = 0; time < 2; time += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                walked |= brain.State == InsaneDreams.Enemies.LocomotionState.Walking;
                jumped |= brain.State == InsaneDreams.Enemies.LocomotionState.Jumping;
                maxHeight = Mathf.Max(maxHeight, enemy.transform.position.y);
            }
            Assert.IsTrue(walked); Assert.IsTrue(jumped); Assert.Greater(maxHeight, 1.7f);
            Assert.Greater(enemy.transform.position.x, 1004);
        }
        [UnityTest] public IEnumerator EquipmentButtonsReadRuntimeLoadoutAndPreserveAmmo()
        {
            var ui = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));
            ui.GetComponent<WarehouseTouchControls>().Bind(actions); yield return null; yield return null;
            var root = ui.GetComponent<UIDocument>().rootVisualElement;
            var loadout = player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>();
            var first = loadout.Get(InsaneDreams.Inventory.EquipmentSlot.Firearm1); first.SetAmmunition(3, 17);
            var second = root.Q<Button>("slot2"); Down(second, 5, second.worldBound.center); Up(second, 5);
            yield return null; Assert.AreEqual(InsaneDreams.Inventory.EquipmentSlot.Firearm2, loadout.SelectedSlot);
            actions.SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Melee); yield return new WaitForSeconds(.25f);
            StringAssert.Contains("unlimited", root.Q<Label>("loadout").text);
            actions.SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Firearm1); yield return new WaitForSeconds(.25f);
            Assert.AreEqual(3, loadout.SelectedWeapon.MagazineAmmo); StringAssert.Contains("Ammo 3", root.Q<Label>("loadout").text);
        }
        [UnityTest] public IEnumerator MobileControlsFitLandscapeAndWideLandscapeTargets()
        {
            var ui = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));
            var document = ui.GetComponent<UIDocument>();
            var original = document.panelSettings; var settings = Object.Instantiate(original); document.panelSettings = settings;
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(2560, 1080) })
            {
                var target = new RenderTexture(size.x, size.y, 0); target.Create(); settings.targetTexture = target;
                yield return null; yield return null; yield return null;
                var root = document.rootVisualElement; Rect viewport = root.worldBound;
                var move = root.Q<VisualElement>("moveStick"); var aim = root.Q<VisualElement>("aimStick");
                var chute = root.Q<VisualElement>("parachuteButton");
                Assert.Greater(move.worldBound.width, 50); Assert.Greater(aim.worldBound.width, 50);
                Assert.IsFalse(move.worldBound.Overlaps(aim.worldBound)); Assert.IsFalse(chute.worldBound.Overlaps(aim.worldBound));
                foreach (var element in new[] { move, aim, chute, root.Q<VisualElement>(className: "equipment") })
                {
                    Assert.GreaterOrEqual(element.worldBound.xMin, viewport.xMin - 1);
                    Assert.LessOrEqual(element.worldBound.xMax, viewport.xMax + 1);
                    Assert.GreaterOrEqual(element.worldBound.yMin, viewport.yMin - 1);
                    Assert.LessOrEqual(element.worldBound.yMax, viewport.yMax + 1);
                }
                settings.targetTexture = null; target.Release(); Object.Destroy(target);
            }
            document.panelSettings = original; Object.Destroy(settings);
        }
    }
}
#endif
