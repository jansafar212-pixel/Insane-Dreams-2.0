#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class Sprint005Tests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private GameObject player;
        private Rigidbody2D body;
        private PlayerActions actions;
        private PlayerTraversal2D traversal;
        private bool background;
        private GameObject Keep(GameObject go) { objects.Add(go); return go; }
        private GameObject Box(string name, Vector2 position, Vector2 size, bool trigger = false)
        {
            var go = Keep(new GameObject(name)); go.transform.position = position;
            var box = go.AddComponent<BoxCollider2D>(); box.size = size; box.isTrigger = trigger; return go;
        }
        private LadderVolume Ladder(float x = 3000, float y = 4, float height = 12) =>
            Box("PolishLadder", new Vector2(x, y), new Vector2(1.8f, height), true).AddComponent<LadderVolume>();
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            Box("Floor", new Vector2(3000, -.5f), new Vector2(60, 1));
            player = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab")));
            body = player.GetComponent<Rigidbody2D>(); actions = player.GetComponent<PlayerActions>(); traversal = player.GetComponent<PlayerTraversal2D>(); var controlledInput=new SerializedObject(actions);controlledInput.FindProperty("readKeyboard").boolValue=false;controlledInput.ApplyModifiedPropertiesWithoutUndo();
            body.position = new Vector2(3000, .85f); actions.ResetActions(); Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.35f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            objects.Clear(); yield return null; Application.runInBackground = background;
        }
        private void Position(float x, float y)
        {
            actions.ResetActions(); traversal.ResetTraversal(); body.position = new Vector2(x, y);
            body.linearVelocity = Vector2.zero; Physics2D.SyncTransforms();
        }
        [UnityTest] public IEnumerator OffsetEntryFromBothSidesAcceptsUpwardDiagonalAndAlignsSmoothly()
        {
            Ladder();
            foreach (float offset in new[] { -1.8f, 1.8f })
            {
                Position(3000 + offset, .85f); yield return new WaitForSeconds(.35f);
                actions.SetTouchMove(new Vector2(.5f, .85f));
                yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                Assert.Less(Mathf.Abs(body.position.x - (3000 + offset)), .5f, "No capture teleport");
                yield return new WaitForSeconds(.3f);
                Assert.IsTrue(traversal.IsClimbing); Assert.That(body.position.x, Is.EqualTo(3000).Within(.2f));
                Assert.AreEqual(0, body.gravityScale);
            }
        }
        [UnityTest] public IEnumerator DistantAndBlockedCandidatesAreRejectedAndWalkingNeverAttaches()
        {
            Ladder(); Position(3004, .85f); yield return new WaitForSeconds(.35f);
            Assert.IsFalse(traversal.CanUseLadder);
            Position(3001.8f, .85f); Box("Wall", new Vector2(3000.9f, 2), new Vector2(.2f, 4)); Physics2D.SyncTransforms();
            Assert.IsFalse(traversal.CanUseLadder); actions.SetTouchMove(Vector2.right);
            yield return new WaitForSeconds(.4f); Assert.IsFalse(traversal.IsClimbing);
        }
        [UnityTest] public IEnumerator WalkingAcrossUnblockedLadderDoesNotMagneticallyAttach()
        {
            Ladder(); actions.SetTouchMove(Vector2.right); yield return new WaitForSeconds(.3f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(3, body.gravityScale);
        }
        [UnityTest] public IEnumerator EquidistantLaddersSelectConsistentlyAndSideWallCannotBeCrossed()
        {
            Ladder(2998.2f); Ladder(3001.8f);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.3f);
            Assert.IsTrue(traversal.IsClimbing); float selectedX = body.position.x;
            Position(3000, .85f); yield return new WaitForSeconds(.35f);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.3f);
            Assert.That(body.position.x, Is.EqualTo(selectedX).Within(.1f));
            float direction = selectedX > 3000 ? 1 : -1;
            Box("SideWall", new Vector2(selectedX + direction * .8f, 3), new Vector2(.2f, 6)); Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.right * direction); yield return new WaitForSeconds(.2f);
            Assert.IsFalse(traversal.IsClimbing); Assert.Less((body.position.x - selectedX) * direction, .5f);
            Assert.AreEqual(3, body.gravityScale);
        }
        [UnityTest] public IEnumerator SideDismountWithoutLandingRestoresGravityAndRequiresFreshGesture()
        {
            Ladder(); body.position = new Vector2(3000, 4); Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.12f); Assert.IsTrue(traversal.IsClimbing);
            actions.SetTouchMove(new Vector2(.3f, .8f)); yield return new WaitForSeconds(.1f); Assert.IsTrue(traversal.IsClimbing);
            actions.SetTouchMove(Vector2.right); yield return new WaitForSeconds(.06f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(3, body.gravityScale); Assert.Less(body.linearVelocity.y, 0);
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.1f); Assert.IsFalse(traversal.IsClimbing);
            yield return new WaitForSeconds(.25f); Assert.IsFalse(traversal.IsClimbing, "Held input cannot reattach after lockout");
            actions.SetTouchMove(Vector2.zero); yield return new WaitForSeconds(.04f); actions.SetTouchMove(Vector2.up);
            yield return new WaitForSeconds(.1f); Assert.IsTrue(traversal.IsClimbing, $"Position {body.position}, usable {traversal.CanUseLadder}, input {actions.Move}");
        }
        [UnityTest] public IEnumerator SideDismountUsesSafeAdjacentPlatformWithoutUpwardImpulse()
        {
            Ladder(); body.position = new Vector2(3000, 4); Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.12f);
            float feet = player.GetComponent<BoxCollider2D>().bounds.min.y;
            Box("SideLanding", new Vector2(3002.5f, feet - .3f), new Vector2(3, .3f)); Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.right); yield return new WaitForSeconds(.4f);
            Assert.IsFalse(traversal.IsClimbing); Assert.Greater(body.position.x, 3001.3f);
            Assert.AreEqual(3, body.gravityScale); Assert.LessOrEqual(body.linearVelocity.y, .01f);
        }
        [UnityTest] public IEnumerator MissingTopLandingHoldsSafelyAndBottomExitDoesNotLoop()
        {
            var ladder = Ladder(); body.position = new Vector2(3000, 9.8f); Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.5f);
            Assert.IsTrue(traversal.IsClimbing); Assert.LessOrEqual(body.position.y, 10.05f); Assert.AreEqual(0, body.linearVelocity.y);
            actions.SetTouchMove(Vector2.down); yield return new WaitForSeconds(.15f); Assert.Less(body.position.y, 9.8f);
            Position(3000, .8f); yield return new WaitForSeconds(.35f);
            actions.SetTouchMove(Vector2.down); yield return new WaitForSeconds(.2f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(3, body.gravityScale);
            yield return new WaitForSeconds(.4f); Assert.IsFalse(traversal.IsClimbing);
            actions.SetTouchMove(Vector2.right); yield return new WaitForSeconds(.1f); Assert.Greater(body.linearVelocity.x, 0);
        }
        [UnityTest] public IEnumerator ActualTouchPointerPathEntersAndDismountsWhileAimPointerStaysIndependent()
        {
            Ladder(); Position(2998.2f, .85f); yield return new WaitForSeconds(.35f);
            var ui = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));
            ui.GetComponent<WarehouseTouchControls>().Bind(actions); yield return new WaitForSeconds(.25f);
            var root = ui.GetComponent<UIDocument>().rootVisualElement;
            var move = root.Q<VisualElement>("moveStick"); var aim = root.Q<VisualElement>("aimStick");
            Assert.Greater(move.worldBound.width,100);Assert.Greater(aim.worldBound.width,100);Pointer(move, 0, new Vector2(move.worldBound.width*.09f, -move.worldBound.height*.24f), true); Pointer(aim, 1, new Vector2(aim.worldBound.width*.11f, 0), true);
            yield return new WaitForSeconds(.3f); Assert.IsTrue(traversal.IsClimbing); Assert.Greater(actions.AimDirection.x, .9f); Assert.IsFalse(actions.FireHeld);
            Pointer(move, 0, new Vector2(-move.worldBound.width*.32f, 0), false); yield return new WaitForSeconds(.08f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(3, body.gravityScale); Assert.Greater(actions.AimDirection.x, .9f);
            actions.ResetActions();
        }
        private static void Pointer(VisualElement element, int finger, Vector2 offset, bool down)
        {
            var touch = new Touch { fingerId = finger, position = element.worldBound.center + offset, phase = down ? TouchPhase.Began : TouchPhase.Moved };
            if (down) { using (var evt = PointerDownEvent.GetPooled(touch, EventModifiers.None)) element.SendEvent(evt); }
            else { using (var evt = PointerMoveEvent.GetPooled(touch, EventModifiers.None)) element.SendEvent(evt); }
        }
        [UnityTest] public IEnumerator FeedbackObservesExactlyOneDamageAndDoesNotChangeWeaponRules()
        {
            var feedback = player.GetComponent<CombatFeedback>(); Assert.IsNotNull(feedback);
            var target = Box("FeedbackTarget", new Vector2(3003, body.position.y), new Vector2(.8f, 1.4f)); target.layer = 2;
            var health = target.AddComponent<Health>(); var enemyFeedback = target.AddComponent<CombatFeedback>();
            var weapon = player.GetComponent<PlayerLoadout>().SelectedWeapon; int initial = weapon.MagazineAmmo;
            Physics2D.SyncTransforms(); var attack = player.GetComponent<AttackExecutor>();
            Assert.IsTrue(attack.Attack(weapon, Vector2.right, 0)); Assert.IsFalse(attack.Attack(weapon, Vector2.right, 0));
            Assert.AreEqual(initial - 1, weapon.MagazineAmmo); Assert.AreEqual(health.Maximum - weapon.Definition.Damage, health.Current);
            Assert.AreEqual(1, feedback.ShotCues); Assert.AreEqual(1, feedback.HitCues); Assert.AreEqual(1, enemyFeedback.HurtCues);
            weapon.SetAmmunition(0, 12); Assert.IsFalse(attack.Attack(weapon, Vector2.right, 2)); Assert.AreEqual(1, feedback.EmptyCues);
            player.GetComponent<PlayerCombat>().Reload(); Assert.AreEqual(1, feedback.ReloadCues); Assert.AreEqual(0, weapon.MagazineAmmo);
            yield return null;
        }
        [UnityTest] public IEnumerator LarperReactsApproachesAndStopsFiringImmediatelyWhenOccluded()
        {
            var enemy = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"), new Vector3(3008, .85f), Quaternion.identity));
            var brain = enemy.GetComponent<LarperBrain>(); brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            var combat = enemy.GetComponent<EnemyCombat>(); combat.BindTarget(player.GetComponent<Health>());
            CombatTestView.Frame(combat);
            var policy=new UnityEditor.SerializedObject(combat);foreach(string key in new[]{"ordinaryBlindChance","overconfidentBlindChance","cowardBlindChance"})policy.FindProperty(key).floatValue=0;policy.ApplyModifiedPropertiesWithoutUndo(); // Exercise the optional non-spray path explicitly.
            var attack = enemy.GetComponent<AttackExecutor>();
            yield return new WaitForSeconds(.1f); Assert.AreEqual(0, attack.ShotsFired); Assert.AreEqual(EngagementState.Reacting, combat.State);
            yield return new WaitForSeconds(1); Assert.Greater(attack.ShotsFired, 0); Assert.Less(enemy.transform.position.x, 3007.5f);
            Box("SightWall", new Vector2(3002.5f, 2), new Vector2(.3f, 4)); Physics2D.SyncTransforms(); int shots = attack.ShotsFired;
            yield return new WaitForSeconds(.3f); Assert.IsFalse(combat.TargetVisible); Assert.AreEqual(shots, attack.ShotsFired);
            Assert.AreEqual(EngagementState.Searching, combat.State);
            yield return new WaitForSeconds(4.6f); Assert.AreEqual(EngagementState.Patrol, combat.State);
        }
        [UnityTest] public IEnumerator LarperEffectiveRangeAndAccuracyAreConfigurableAndDefeatFeedbackIsSingle()
        {
            var enemy = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"), new Vector3(3012, body.position.y), Quaternion.identity));
            enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            var combat = enemy.GetComponent<EnemyCombat>(); combat.BindTarget(player.GetComponent<Health>());
            CombatTestView.Frame(combat);
            var configuredRange=new UnityEditor.SerializedObject(combat);configuredRange.FindProperty("engagementRange").floatValue=11;configuredRange.ApplyModifiedPropertiesWithoutUndo(); // The firing limit is now independent of accuracy falloff.
            float readyDeadline=Time.time+2;while(combat.State!=EngagementState.Approaching&&Time.time<readyDeadline)yield return null;
            Assert.AreEqual(EngagementState.Approaching, combat.State); Assert.AreEqual(0, enemy.GetComponent<AttackExecutor>().ShotsFired);
            Assert.Greater(combat.SpreadAtDistance(10), combat.SpreadAtDistance(3));
            combat.BindTarget(null); enemy.transform.position = new Vector3(3003, body.position.y); Physics2D.SyncTransforms();
            var health = enemy.GetComponent<Health>(); var feedback = enemy.GetComponent<CombatFeedback>();
            var weapon = player.GetComponent<PlayerLoadout>().SelectedWeapon; var attack = player.GetComponent<AttackExecutor>();
            Assert.IsTrue(attack.Attack(weapon, Vector2.right, 0)); Assert.IsTrue(attack.Attack(weapon, Vector2.right, 1)); Assert.IsTrue(attack.Attack(weapon, Vector2.right, 2));
            Assert.IsTrue(health.IsDefeated); Assert.IsFalse(enemy.activeSelf); Assert.AreEqual(1, feedback.DefeatCues); Assert.AreEqual(3, feedback.HurtCues);
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(1, CombatTeam.Player))); Assert.AreEqual(1, feedback.DefeatCues);
            var playerHealth = player.GetComponent<Health>(); float hp = playerHealth.Current;
            playerHealth.ApplyDamage(new DamageInfo(5, CombatTeam.Enemy));
            Assert.AreEqual(hp - 5, playerHealth.Current); Assert.AreEqual(1, player.GetComponent<CombatFeedback>().HurtCues);
        }
    }
}
#endif
