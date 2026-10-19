#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class Sprint011CombatTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private GameObject player;
        private PlayerActions actions;
        private PlayerLoadout loadout;
        private AttackExecutor attacks;
        private CharacterVisualDriver visual;
        private bool background;
        private T Asset<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
        private GameObject Keep(GameObject go) { objects.Add(go); return go; }
        private GameObject Solid(string name, Vector2 position, Vector2 size)
        {
            var go = Keep(new GameObject(name)); go.transform.position = position;
            go.AddComponent<BoxCollider2D>().size = size; Physics2D.SyncTransforms(); return go;
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            Solid("S011Floor", new Vector2(120, -.5f), new Vector2(80, 1));
            player = Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"), new Vector3(120, .85f), Quaternion.identity));
            actions = player.GetComponent<PlayerActions>(); actions.ResetActions();
            loadout = player.GetComponent<PlayerLoadout>(); attacks = player.GetComponent<AttackExecutor>(); visual = player.GetComponent<CharacterVisualDriver>();
            player.GetComponent<Health>().ProtectFor(60);
            yield return new WaitForSeconds(.3f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var d in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None)) d.Retire();
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            objects.Clear(); yield return null; Application.runInBackground = background;
        }
        private EnemyCombat Enemy()
        {
            var go = Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"), new Vector3(124, .85f), Quaternion.identity));
            go.GetComponent<LarperBrain>().Configure(Asset<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            var combat = go.GetComponent<EnemyCombat>(); combat.BindTarget(player.GetComponent<Health>());CombatTestView.Frame(combat); return combat;
        }
        [UnityTest] public IEnumerator EveryAimAndFacingUsesTransformedMuzzleAndExactShotLineWithBoundedTrails()
        {
            Assert.IsFalse(player.transform.Find("AimDirectionMarker").gameObject.activeInHierarchy);
            float now = Time.time;
            foreach (float facing in new[] { -1f, 1f })
            foreach (var aim in new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down, new Vector2(1, 1).normalized, new Vector2(-1, -1).normalized })
            {
                player.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(facing, 0);
                actions.SetTouchAim(aim); yield return null;
                loadout.SelectedWeapon.SetAmmunition(12, 36);
                Assert.IsTrue(attacks.Attack(loadout.SelectedWeapon, aim, now += 1));
                Assert.Less(Vector2.Distance(attacks.LastShotOrigin, visual.Rig.MuzzlePosition), .002f);
                var gun = visual.Rig.weapon;
                Vector2 visibleBarrel = gun.transform.TransformPoint(new Vector3(gun.sprite.bounds.max.x, (gun.flipY ? -1f : 1f) / 32, 0));
                Assert.Less(Vector2.Distance(visibleBarrel, attacks.LastShotOrigin), .002f, "Mirrored pixels and child anchor must agree");
                Assert.Greater(Vector2.Dot(visual.Rig.BarrelDirection, aim), .9999f);
                Assert.Greater(Vector2.Dot((attacks.LastShotEnd - attacks.LastShotOrigin).normalized, aim), .9999f);
                var lines = player.GetComponentsInChildren<LineRenderer>(); Assert.AreEqual(6, lines.Length);
                Assert.IsTrue(lines.Any(l => l.enabled && Vector2.Distance(l.GetPosition(0), attacks.LastShotOrigin) < .002f && Vector2.Distance(l.GetPosition(1), attacks.LastShotEnd) < .002f));
            }
            yield return new WaitForSeconds(.4f);
            Assert.IsFalse(player.GetComponentsInChildren<LineRenderer>().Any(l => l.enabled));
        }
        [UnityTest] public IEnumerator BarrelClippedIntoCoverRetractsAndCannotDamageTargetBehindFirstWall()
        {
            var wall = Solid("NearBarrel", new Vector2(120.55f, 1), new Vector2(.1f, 3));
            Solid("FarWall", new Vector2(122, 1), new Vector2(.2f, 3));
            var target = Solid("CoveredTarget", new Vector2(124, 1), new Vector2(1, 2)).AddComponent<Health>();
            Assert.IsTrue(attacks.Attack(loadout.SelectedWeapon, Vector2.right, Time.time));
            Assert.AreEqual(ShotSurface.Geometry, attacks.LastSurface); Assert.AreEqual(target.Maximum, target.Current);
            Assert.Less(attacks.LastShotOrigin.x, wall.GetComponent<Collider2D>().bounds.min.x);
            Assert.That(attacks.LastShotEnd.x, Is.EqualTo(wall.GetComponent<Collider2D>().bounds.min.x).Within(.003f));
            Assert.Less(Vector2.Distance(attacks.LastShotOrigin, visual.Rig.MuzzlePosition), .002f);
            yield return null;
        }
        [UnityTest] public IEnumerator PlayerReloadPoseUsesRealCycleWhileMovingAndSwitchCancelsWithoutAmmoTransfer()
        {
            var weapon = loadout.SelectedWeapon; weapon.SetAmmunition(1, 20); actions.SetTouchAim(Vector2.up);
            actions.RequestReload(); actions.SetTouchMove(Vector2.right);
            yield return new WaitForSeconds(.25f);
            Assert.IsTrue(visual.ReloadPoseActive); Assert.IsTrue(visual.Rig.weapon.enabled);
            Assert.Less(Vector2.Dot(visual.Rig.BarrelDirection, Vector2.up), .999f);
            int shots = attacks.ShotsFired; actions.SetFireIntent(true); yield return new WaitForSeconds(.15f);
            Assert.AreEqual(shots, attacks.ShotsFired); Assert.AreEqual(1, weapon.MagazineAmmo);
            actions.SetFireIntent(false); loadout.Select(EquipmentSlot.Firearm2); yield return null; yield return null;
            Assert.IsFalse(weapon.Cycle.IsReloading); Assert.IsFalse(visual.ReloadPoseActive); Assert.AreEqual(1, weapon.MagazineAmmo); Assert.AreEqual(20, weapon.ReserveAmmo);
        }
        [UnityTest] public IEnumerator FocusLossPreservesDeploymentButExplicitRecoveryClearsIt()
        {
            var controls = Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<WarehouseTouchControls>();
            controls.Bind(actions); yield return null;
            var body = player.GetComponent<Rigidbody2D>(); body.position = new Vector2(120, 4); Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate(); actions.SetTouchParachute(true); actions.SetTouchParachute(false);
            Assert.IsTrue(player.GetComponent<PlayerTraversal2D>().IsParachuting);
            player.SendMessage("OnApplicationFocus", false); controls.SendMessage("OnApplicationFocus", false); yield return null;
            Assert.IsTrue(player.GetComponent<PlayerTraversal2D>().IsParachuting);
            player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Respawn(); yield return null;
            Assert.IsFalse(player.GetComponent<PlayerTraversal2D>().IsParachuting); Assert.AreEqual(3, body.gravityScale);
        }
        [UnityTest] public IEnumerator TunedImpulsesIncreaseMomentumByThirtyPercentAndKeepExistingCaps()
        {
            var actor = Enemy(); actor.enabled = false;
            var doll = Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab"))).GetComponent<CartoonRagdoll>();
            doll.Activate(actor.GetComponent<CharacterVisualDriver>().Rig, Vector2.zero);
            var part = doll.parts[0]; foreach (var p in doll.parts) { p.linearVelocity = Vector2.zero; p.angularVelocity = 0; }
            doll.ReceiveImpact(new PhysicalImpact(part.worldCenterOfMass, Vector2.right, .9f, part));
            Assert.That(part.linearVelocity.x, Is.EqualTo(.9f * 1.56f / part.mass).Within(.001f));
            part.linearVelocity = Vector2.zero;
            doll.ReceiveDefeatImpact(new PhysicalImpact(part.worldCenterOfMass, Vector2.right, .9f, part));
            Assert.That(part.linearVelocity.x, Is.EqualTo(.9f * 2.6f / part.mass).Within(.001f));
            doll.ReceiveImpact(new PhysicalImpact(part.worldCenterOfMass, Vector2.right, 10000, part));
            Assert.LessOrEqual(part.linearVelocity.magnitude, 12.001f); Assert.AreEqual(45, doll.Lifetime);
            yield return null;
        }
        [UnityTest] public IEnumerator EnemyWithdrawsReloadsThroughLostSightAndReengagesWithFiniteAmmo()
        {
            var enemy = Enemy(); enemy.Weapon.SetAmmunition(0, 24);
            float initialX = enemy.transform.position.x; int shots = enemy.GetComponent<AttackExecutor>().ShotsFired;
            for (int i = 0; i < 100 && enemy.State != EngagementState.Reloading; i++) yield return new WaitForFixedUpdate();
            Assert.AreEqual(EngagementState.Reloading, enemy.State); Assert.Greater(enemy.transform.position.x - initialX, .1f);
            yield return null; Assert.IsTrue(enemy.GetComponent<CharacterVisualDriver>().ReloadPoseActive);
            Assert.IsTrue(enemy.GetComponent<CharacterVisualDriver>().Rig.weapon.enabled);
            var wall = Solid("LostSightDuringReload", new Vector2(122, 1), new Vector2(.3f, 4));
            yield return new WaitForSeconds(enemy.Weapon.Definition.ReloadDuration * 1.05f + .2f);
            Assert.IsFalse(enemy.Weapon.Cycle.IsReloading); Assert.Greater(enemy.Weapon.MagazineAmmo, 0); Assert.Less(enemy.Weapon.ReserveAmmo, 24);
            Assert.AreEqual(shots, enemy.GetComponent<AttackExecutor>().ShotsFired); Assert.IsFalse(enemy.TargetVisible);
            Object.Destroy(wall); yield return new WaitForSeconds(1);
            Assert.Greater(enemy.GetComponent<AttackExecutor>().ShotsFired, shots);
            var e = enemy.GetComponent<AttackExecutor>(); Assert.Greater(Vector2.Dot((e.LastShotEnd-e.LastShotOrigin).normalized, e.LastShotDirection), .999f);
        }
        [UnityTest] public IEnumerator BlockedWithdrawalReloadsInPlaceAndPanicOrDefeatCannotCompleteCancelledReload()
        {
            Solid("RetreatBlocked", new Vector2(124.65f, 1), new Vector2(.25f, 3));
            var enemy = Enemy(); enemy.Weapon.SetAmmunition(0, 24);
            yield return new WaitForSeconds(.55f); Assert.AreEqual(EngagementState.Reloading, enemy.State);
            Assert.IsTrue(enemy.PanicFor(.2f)); Assert.IsFalse(enemy.Weapon.Cycle.IsReloading);
            Assert.AreEqual(0, enemy.Weapon.MagazineAmmo);
            yield return new WaitForSeconds(.35f); Assert.IsTrue(enemy.Weapon.Cycle.IsReloading);
            var cycle = enemy.Weapon.Cycle; float deadline = cycle.ReloadEndsAt;
            enemy.GetComponent<Health>().ApplyDamage(new DamageInfo(1000, CombatTeam.Player));
            Assert.IsFalse(cycle.IsReloading); cycle.Tick(deadline + 10);
            Assert.AreEqual(0, enemy.Weapon.MagazineAmmo); Assert.AreEqual(24, enemy.Weapon.ReserveAmmo);
        }
        [UnityTest] public IEnumerator EmptyReservesNeverCreateAmmoAndPersonalityControlsReloadThreshold()
        {
            var enemy = Enemy(); enemy.Weapon.SetAmmunition(0, 0);
            var awareness = enemy.GetComponent<LarperAwareness>();
            var fractions = new Dictionary<LarperDisposition,float>();
            foreach (var path in AssetDatabase.FindAssets("t:LarperPersonalityProfile").Select(AssetDatabase.GUIDToAssetPath))
            { awareness.Configure(Asset<LarperPersonalityProfile>(path)); fractions[awareness.Personality.Disposition] = enemy.ReloadFraction; }
            Assert.Greater(fractions[LarperDisposition.Cowardly], fractions[LarperDisposition.Ordinary]);
            Assert.Greater(fractions[LarperDisposition.Ordinary], fractions[LarperDisposition.Overconfident]);
            yield return new WaitForSeconds(1.1f);
            Assert.IsFalse(enemy.Weapon.Cycle.IsReloading); Assert.AreEqual(0, enemy.Weapon.MagazineAmmo); Assert.AreEqual(0, enemy.Weapon.ReserveAmmo);
            Assert.AreEqual(0, enemy.GetComponent<AttackExecutor>().ShotsFired); Assert.AreNotEqual(EngagementState.Withdrawing, enemy.State);
        }
        [UnityTest] public IEnumerator PanicInterruptsWithdrawalAndAuthoredLadderRoutePreemptsReloadWithoutStalePursuit()
        {
            var enemy = Enemy(); enemy.Weapon.SetAmmunition(0, 24);
            for (int i = 0; i < 30 && enemy.State != EngagementState.Withdrawing; i++) yield return new WaitForFixedUpdate();
            Assert.AreEqual(EngagementState.Withdrawing, enemy.State);
            Assert.IsTrue(enemy.PanicFor(.2f)); yield return null;
            Assert.AreEqual(EngagementState.Panicked, enemy.State); Assert.IsFalse(enemy.Weapon.Cycle.IsReloading);
            Solid("BlockAfterPanic", (Vector2)enemy.transform.position + Vector2.right * .65f, new Vector2(.25f, 3));
            yield return new WaitForSeconds(.35f); Assert.IsTrue(enemy.Weapon.Cycle.IsReloading);
            var ladderObject = Solid("TacticalRouteLadder", new Vector2(124, 4), new Vector2(1.8f, 12));
            ladderObject.GetComponent<BoxCollider2D>().isTrigger = true;
            var ladder = ladderObject.AddComponent<LadderVolume>();
            enemy.GetComponent<AuthoredLadderRoute>().Configure(ladder, 6.6f); yield return null; yield return null;
            Assert.IsFalse(enemy.Weapon.Cycle.IsReloading); Assert.AreEqual(0, enemy.Weapon.MagazineAmmo); Assert.AreEqual(24, enemy.Weapon.ReserveAmmo);
            Assert.IsTrue(enemy.GetComponent<AuthoredLadderRoute>().IsRunning);
            Assert.AreNotEqual(EngagementState.Withdrawing, enemy.State); Assert.IsFalse(enemy.PanicFor(1));
        }
    }
}
#endif
