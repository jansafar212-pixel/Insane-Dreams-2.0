#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class Sprint022CoverTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private GameObject player;
        private PlayerActions actions;
        private PlayerCover2D cover;
        private PlayerPosture2D posture;
        private BoxCollider2D box, crate;
        private Rigidbody2D body;
        private PlayerLoadout loadout;
        private AttackExecutor attacks;
        private bool background;
        private Random.State random;
        private const float X = 17000;
        private GameObject Keep(GameObject go) { objects.Add(go); return go; }
        private BoxCollider2D Solid(string name, Vector2 position, Vector2 size)
        {
            var go = Keep(new GameObject(name)); go.transform.position = position;
            var result = go.AddComponent<BoxCollider2D>(); result.size = size; Physics2D.SyncTransforms(); return result;
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            random = Random.state; Random.InitState(22022);
            Solid("CoverTestFloor", new Vector2(X, -.5f), new Vector2(40, 1));
            player = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"), new Vector3(X, .85f), Quaternion.identity));
            player.GetComponent<PlayerHealthRecovery>().enabled = false;
            actions = player.GetComponent<PlayerActions>(); cover = player.GetComponent<PlayerCover2D>();
            posture = player.GetComponent<PlayerPosture2D>(); box = player.GetComponent<BoxCollider2D>(); body = player.GetComponent<Rigidbody2D>();
            loadout = player.GetComponent<PlayerLoadout>(); attacks = player.GetComponent<AttackExecutor>();
            var data = new SerializedObject(actions); data.FindProperty("readKeyboard").boolValue = false; data.ApplyModifiedPropertiesWithoutUndo();
            crate = Solid("CoverTestCrate", new Vector2(X + 1.1f, .45f), new Vector2(1, .9f));
            yield return new WaitForSeconds(.25f);
            actions.SetTouchMove(Vector2.down); yield return new WaitForSeconds(.25f);
            Assert.IsNotNull(cover);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            objects.Clear(); yield return null; Application.runInBackground = background; Random.state = random;
        }
        private AttackExecutor Shooter(Vector2 position)
        {
            var go = Keep(new GameObject("CoverTestShooter")); go.transform.position = position;
            var h = go.AddComponent<Health>(); var data = new SerializedObject(h);
            data.FindProperty("team").enumValueIndex = (int)CombatTeam.Enemy; data.ApplyModifiedPropertiesWithoutUndo();
            return go.AddComponent<AttackExecutor>();
        }
        [UnityTest] public IEnumerator ConcealmentBlocksRealShotsButUnobstructedRearStillTakesDamage()
        {
            Assert.AreEqual(PlayerCoverStance.Concealed, cover.Stance); Assert.Less(box.bounds.max.y, crate.bounds.max.y);
            float before = player.GetComponent<Health>().Current;
            var enemy = Shooter(new Vector2(X + 4, .6f));
            Assert.IsTrue(enemy.Attack(new RuntimeWeapon(loadout.SelectedWeapon.Definition), Vector2.left, Time.time));
            Assert.AreEqual(ShotSurface.Geometry, enemy.LastSurface); Assert.AreEqual(before, player.GetComponent<Health>().Current);
            var rear = Shooter(new Vector2(X - 4, .6f));
            Assert.IsTrue(rear.Attack(new RuntimeWeapon(loadout.SelectedWeapon.Definition), Vector2.right, Time.time));
            Assert.AreEqual(ShotSurface.Character, rear.LastSurface); Assert.Less(player.GetComponent<Health>().Current, before);
            yield return null;
        }
        [UnityTest] public IEnumerator PeekExposesDamageableBodyAndReleaseConcealsWithoutMovingFeet()
        {
            float feet = box.bounds.min.y; var position = body.position;
            actions.SetTouchAim(Vector2.right); yield return new WaitForSeconds(.22f);
            Assert.IsTrue(cover.PeekReady); Assert.Greater(box.bounds.max.y, crate.bounds.max.y + .2f);
            var enemy = Shooter(new Vector2(X + 4, 1.15f)); float before = player.GetComponent<Health>().Current;
            enemy.Attack(new RuntimeWeapon(loadout.SelectedWeapon.Definition), Vector2.left, Time.time);
            Assert.AreEqual(ShotSurface.Character, enemy.LastSurface); Assert.Less(player.GetComponent<Health>().Current, before);
            actions.SetTouchAim(Vector2.zero); yield return new WaitForSeconds(.22f);
            Assert.AreEqual(PlayerCoverStance.Concealed, cover.Stance); Assert.Less(box.bounds.max.y, crate.bounds.max.y);
            Assert.AreEqual(feet, box.bounds.min.y, .015f); Assert.Less(Vector2.Distance(position, body.position), .02f);
        }
        [UnityTest] public IEnumerator BlindFireHasWideRealBarrelSpreadAndAimedFireRetainsAccuracy()
        {
            var directions = new List<float>(); var origins = new List<Vector2>();
            attacks.Fired += (direction, end, hit) => { directions.Add(Vector2.SignedAngle(Vector2.right, direction)); origins.Add(attacks.Origin); };
            int ammo = loadout.SelectedWeapon.MagazineAmmo;
            actions.SetFireIntent(true); yield return new WaitForSeconds(1.25f); actions.SetFireIntent(false); yield return null;
            Assert.GreaterOrEqual(directions.Count, 4); Assert.AreEqual(ammo - directions.Count, loadout.SelectedWeapon.MagazineAmmo);
            float min = Mathf.Min(directions.ToArray()), max = Mathf.Max(directions.ToArray()); Assert.Greater(max - min, 12);
            Assert.Less(box.bounds.max.y, crate.bounds.max.y);
            foreach (var origin in origins) { Assert.IsFalse(crate.OverlapPoint(origin)); Assert.Greater(origin.y, crate.bounds.max.y); }
            var rig = player.GetComponent<CharacterVisualDriver>().Rig;
            Assert.Greater(Vector2.Dot(rig.BarrelDirection, attacks.LastShotDirection), .99f);
            actions.SetTouchAim(Vector2.right); yield return new WaitForSeconds(.22f);
            actions.SetFireIntent(true); yield return new WaitForSeconds(.3f); actions.SetFireIntent(false); yield return null;
            Assert.IsTrue(cover.PeekReady); Assert.Greater(Vector2.Dot(Vector2.right, attacks.LastShotDirection), .9999f);
        }
        [UnityTest] public IEnumerator LeftSideCoverMirrorsArmsAndMuzzleWithoutChangingRootScale()
        {
            crate.transform.position = new Vector3(X - 1.1f, .45f); Physics2D.SyncTransforms();
            actions.SetTouchAim(Vector2.left); yield return new WaitForSeconds(.2f);
            actions.SetTouchAim(Vector2.zero); actions.SetFireIntent(true); yield return new WaitForSeconds(.35f);
            Assert.AreEqual(PlayerCoverStance.BlindFire, cover.Stance); Assert.Less(attacks.LastShotDirection.x, 0);
            Assert.Greater(attacks.LastShotOrigin.y, crate.bounds.max.y); Assert.IsFalse(crate.OverlapPoint(attacks.LastShotOrigin));
            var rig = player.GetComponent<CharacterVisualDriver>().Rig;
            Assert.IsTrue(rig.leftArm.GetComponent<SpriteRenderer>().enabled); Assert.IsTrue(rig.rightArm.GetComponent<SpriteRenderer>().enabled);
            Assert.AreEqual(Vector3.one, player.transform.localScale);
        }
        [UnityTest] public IEnumerator InvalidGeometryAndDistanceNeverBecomeCover()
        {
            crate.size = new Vector2(1, 4); crate.transform.position = new Vector3(X + 1.1f, 2); Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.15f); Assert.IsFalse(cover.IsCovered, "Tall wall");
            crate.size = new Vector2(1, .9f); crate.transform.position = new Vector3(X + 1.1f, .45f); crate.isTrigger = true; Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.15f); Assert.IsFalse(cover.IsCovered, "Trigger decoration");
            crate.isTrigger = false; crate.usedByEffector = true; crate.gameObject.AddComponent<PlatformEffector2D>(); Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.15f); Assert.IsFalse(cover.IsCovered, "One-way platform");
            crate.usedByEffector = false; crate.transform.position += Vector3.right * 3; Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.15f); Assert.IsFalse(cover.IsCovered, "Out of range / floor alone");
        }
        [UnityTest] public IEnumerator CeilingPreventsHiddenAccuratePeekAndShotsCannotStartInsideObstacle()
        {
            Solid("CoverTestCeiling", new Vector2(X, 1.15f), new Vector2(.9f, .2f));
            actions.SetTouchAim(Vector2.right); actions.SetFireIntent(true); yield return new WaitForSeconds(.3f);
            Assert.IsFalse(cover.PeekReady); Assert.AreEqual(0, attacks.ShotsFired);
            actions.SetTouchAim(Vector2.zero); yield return new WaitForSeconds(.35f);
            Assert.IsFalse(crate.OverlapPoint(attacks.LastShotOrigin));
        }
        [UnityTest] public IEnumerator ReloadHealingJumpAndRecoveryReleaseCoverWithoutColliderOrGravityLeaks()
        {
            var h = player.GetComponent<Health>(); h.ApplyDamage(new DamageInfo(40, CombatTeam.Enemy)); actions.RequestHeal();yield return new WaitForSeconds(1); Assert.Greater(h.Current, 60);actions.SetHealIntent(false);
            var weapon = loadout.SelectedWeapon; weapon.SetAmmunition(1, 108);
            actions.SetTouchAim(Vector2.right); yield return new WaitForSeconds(.2f); actions.RequestReload(); yield return new WaitForSeconds(.2f);
            Assert.IsTrue(weapon.Cycle.IsReloading); Assert.AreEqual(PlayerCoverStance.Concealed, cover.Stance);
            yield return new WaitForSeconds(weapon.Definition.ReloadDuration + .1f);
            Assert.AreEqual(weapon.Definition.MagazineCapacity, weapon.MagazineAmmo); Assert.Less(weapon.ReserveAmmo, 108);
            actions.SetTouchAim(Vector2.zero); actions.Jump(); yield return new WaitForSeconds(.12f);
            Assert.IsFalse(cover.IsCovered); Assert.IsFalse(posture.IsCrouched); Assert.AreEqual(3, body.gravityScale);
            player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Respawn(); yield return null;
            Assert.IsFalse(cover.IsCovered); Assert.AreEqual(1, posture.HeightRatio, .001f);
        }
        [UnityTest] public IEnumerator CoverRemovalMovementAndIncapacitationRestoreOrdinaryPosture()
        {
            crate.enabled = false; yield return new WaitForSeconds(.15f);
            Assert.IsFalse(cover.IsCovered); Assert.AreEqual(posture.CrouchedHeightRatio, posture.HeightRatio, .001f);
            crate.enabled = true; yield return new WaitForSeconds(.15f); Assert.IsTrue(cover.IsCovered);
            actions.SetTouchMove(new Vector2(-1, -1)); yield return new WaitForSeconds(.45f); Assert.IsFalse(cover.IsCovered);
            actions.SetTouchMove(Vector2.down); body.position = new Vector2(X, .72f); Physics2D.SyncTransforms(); yield return new WaitForSeconds(.2f);
            Assert.IsTrue(cover.IsCovered); body.simulated = false; yield return null; Assert.IsFalse(cover.IsCovered);
            body.simulated = true; yield return new WaitForSeconds(.15f); Assert.IsTrue(cover.IsCovered);
            player.GetComponent<Health>().ApplyDamage(new DamageInfo(1000, CombatTeam.Enemy)); yield return null; Assert.IsFalse(cover.IsCovered);
        }
        [UnityTest] public IEnumerator LeavingCoverUnderCeilingEventuallyRestoresCrouchHeightWhileStillHoldingDown()
        {
            var ceiling = Solid("CoverTestLowCeiling", new Vector2(X, 1.03f), new Vector2(.9f, .3f));
            crate.enabled = false; yield return new WaitForSeconds(.15f);
            Assert.IsFalse(cover.IsCovered); Assert.Less(posture.HeightRatio, posture.CrouchedHeightRatio);
            Object.Destroy(ceiling.gameObject); yield return new WaitForSeconds(.15f);
            Assert.IsTrue(posture.IsCrouched); Assert.AreEqual(posture.CrouchedHeightRatio, posture.HeightRatio, .001f);
        }
        [UnityTest] public IEnumerator CoverPoseKeepsRigidArmsAttachedFeetGroundedAndHeadWithinDamageableBody()
        {
            var rig = player.GetComponent<CharacterVisualDriver>().Rig;
            foreach (bool peek in new[] { false, true, false })
            {
                actions.SetTouchAim(peek ? Vector2.right : Vector2.zero); actions.SetFireIntent(!peek);
                yield return new WaitForSeconds(.25f);
                Assert.LessOrEqual(rig.head.GetComponent<SpriteRenderer>().bounds.max.y, box.bounds.max.y + .06f);
                Assert.AreEqual(box.bounds.min.y, rig.leftLeg.GetComponent<SpriteRenderer>().bounds.min.y, .05f);
                var hand = rig.rightArm.TransformPoint(Vector3.down * rig.skin.arm.bounds.size.y);
                Assert.Less(Vector2.Distance(hand, rig.weaponMount.position), .01f);
                Assert.LessOrEqual(rig.rightArm.localScale.y, 1.001f);
                Assert.AreEqual(Vector3.one, rig.leftArm.localScale);
                Assert.Greater(Quaternion.Angle(rig.leftArm.localRotation, rig.rightArm.localRotation), 20);
            }
        }
        private static void TouchEvent(VisualElement target, int id, Vector2 offset, bool down)
        {
            var touch = new Touch { fingerId = id, position = target.worldBound.center + offset, phase = down ? TouchPhase.Began : TouchPhase.Ended };
            if (down) { using (var e = PointerDownEvent.GetPooled(touch, EventModifiers.None)){e.target=target;target.SendEvent(e);} }
            else { using (var e = PointerUpEvent.GetPooled(touch, EventModifiers.None)){e.target=target;target.SendEvent(e);} }
        }
        [UnityTest] public IEnumerator AndroidFireButtonWorksWithoutAimAndIndependentPointersResetSafely()
        {
            var ui = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));
            ui.GetComponent<WarehouseTouchControls>().Bind(actions); yield return null; yield return null;
            var root = ui.GetComponent<UIDocument>().rootVisualElement;
            var fire = root.Q<VisualElement>("blindFireButton"); var aim = root.Q<VisualElement>("aimStick");
            TouchEvent(fire, 2, Vector2.zero, true); yield return new WaitForSeconds(.15f);
            Assert.IsTrue(actions.FireHeld); Assert.IsFalse(actions.HasAimIntent); Assert.IsTrue(cover.IsBlindFiring);
            TouchEvent(aim, 1, new Vector2(35, 0), true); yield return new WaitForSeconds(.2f);
            Assert.IsTrue(cover.PeekReady); TouchEvent(aim, 1, Vector2.zero, false); yield return new WaitForSeconds(.15f);
            Assert.IsTrue(actions.FireHeld); Assert.IsFalse(actions.HasAimIntent);
            TouchEvent(fire, 9, Vector2.zero, false); yield return null; Assert.IsTrue(actions.FireHeld, "Other fingers cannot release fire");
            actions.SuspendInput(); yield return new WaitForSeconds(.1f); Assert.IsFalse(actions.FireHeld); Assert.IsFalse(cover.IsCovered);
            TouchEvent(fire,2,Vector2.zero,false);yield return null;
            Assert.IsNull(root.Q("fireButton"));fire=root.Q<VisualElement>("aimStick"); // Joystick owns ordinary fire after cover ends.
            Assert.AreEqual(DisplayStyle.Flex,fire.resolvedStyle.display);Assert.Greater(fire.worldBound.width,0);
            TouchEvent(fire, 2, new Vector2(90,0), true);yield return new WaitForSeconds(.1f);Assert.IsTrue(actions.FireHeld,$"Normal fire must remain held; cover={cover.IsCovered}, display={fire.resolvedStyle.display}");
            TouchEvent(fire, 2, Vector2.zero, false);yield return new WaitForSeconds(.1f);Assert.IsFalse(actions.FireHeld);
        }
        [UnityTest] public IEnumerator FireButtonLayoutFitsBothControlTemplatesInLandscapeUltrawideAndPortrait()
        {
            var ui = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));
            var root = ui.GetComponent<UIDocument>().rootVisualElement;
            foreach (var template in new[] { "WarehouseControls", "MissionControls" })
            foreach (var size in new[] { new Vector2(1280, 720), new Vector2(2560, 1080), new Vector2(720, 1280), new Vector2(1080, 1920) })
            {
                var frame = new VisualElement(); frame.style.position = Position.Absolute; frame.style.width = size.x; frame.style.height = size.y;
                root.Add(frame); AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/" + template + ".uxml").CloneTree(frame);frame.Q("blindFireButton").RemoveFromClassList("hidden");
                var interact = frame.Q<VisualElement>("interact"); if (interact != null) interact.style.display = DisplayStyle.Flex;
                yield return null; yield return null;
                Assert.IsNull(frame.Q("fireButton"));var fire = frame.Q<VisualElement>("blindFireButton"); Rect bounds = frame.worldBound;
                Assert.IsTrue(bounds.Contains(fire.worldBound.min)); Assert.IsTrue(bounds.Contains(fire.worldBound.max));
                foreach (var name in new[] { "moveStick", "aimStick", "parachuteButton", "reload", "heal", "interact" })
                {
                    var other = frame.Q<VisualElement>(name);
                    if (other != null) Assert.IsFalse(fire.worldBound.Overlaps(other.worldBound), template + " " + size + " overlaps " + name);
                }
                frame.RemoveFromHierarchy();
            }
        }
    }
}
#endif
