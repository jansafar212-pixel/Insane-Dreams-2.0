#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Checkpoints;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class CombatIntegrationTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private GameObject player;
        private PlayerActions actions;
        private PlayerLoadout loadout;
        private AttackExecutor executor;
        private bool background;
        private GameObject Keep(GameObject go) { objects.Add(go); return go; }
        private static T Asset<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
        private static void Pin(GameObject actor)
        {
            var body = actor.GetComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            player = Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"), new Vector3(2000, 1), Quaternion.identity));
            Pin(player); actions = player.GetComponent<PlayerActions>(); loadout = player.GetComponent<PlayerLoadout>(); executor = player.GetComponent<AttackExecutor>();
            var start = Keep(new GameObject("CombatTestSpawn")).transform; start.position = player.transform.position;
            player.GetComponent<PlayerRecovery>().Bind(start, null); actions.ResetActions();
            Physics2D.SyncTransforms(); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            objects.Clear(); yield return null; Application.runInBackground = background;
        }
        private Health Target(float x)
        {
            var go = Keep(new GameObject("DamageTarget")); go.layer = 2; go.transform.position = new Vector3(x, 1);
            go.AddComponent<BoxCollider2D>().size = new Vector2(.8f, 1.4f); var health = go.AddComponent<Health>();
            var data = new SerializedObject(health); data.FindProperty("team").enumValueIndex = (int)CombatTeam.Enemy;
            data.FindProperty("maximum").floatValue = 40; data.ApplyModifiedPropertiesWithoutUndo(); health.Restore();
            Physics2D.SyncTransforms(); return health;
        }
        [UnityTest] public IEnumerator RaycastStopsAtGeometryAndDefeatsTargetOnce()
        {
            var target = Target(2004); target.gameObject.AddComponent<DeactivateOnDefeat>(); int defeated = 0; target.Defeated += _ => defeated++;
            var wall = Keep(new GameObject("BlockingWall")); wall.transform.position = new Vector3(2002, 1); wall.AddComponent<BoxCollider2D>().size = new Vector2(1, 3);
            Physics2D.SyncTransforms(); var weapon = loadout.SelectedWeapon; int initial = weapon.MagazineAmmo;
            Assert.IsTrue(executor.Attack(weapon, Vector2.right, 0)); Assert.AreEqual(40, target.Current); Assert.AreEqual(initial - 1, weapon.MagazineAmmo);
            Assert.IsFalse(executor.Attack(weapon, Vector2.right, 0));
            Object.Destroy(wall); yield return null; Physics2D.SyncTransforms();
            Assert.IsTrue(executor.Attack(weapon, Vector2.right, 1)); Assert.AreEqual(20, target.Current);
            Assert.IsTrue(executor.Attack(weapon, Vector2.right, 2)); Assert.IsTrue(target.IsDefeated);
            Assert.IsFalse(target.gameObject.activeSelf); Assert.AreEqual(1, defeated);
            Assert.IsFalse(target.ApplyDamage(new DamageInfo(100, CombatTeam.Player)));
        }
        [UnityTest] public IEnumerator BallisticCharacterCuesAreLargerForBothTeamsButGeometryIsUnchanged()
        {
            var target=Target(2004);var weapon=new RuntimeWeapon(Asset<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset"));
            loadout.Equip(EquipmentSlot.Firearm1,weapon);loadout.Select(EquipmentSlot.Firearm1);yield return null;
            var cue=player.transform.Find("HitConfirmation");
            foreach(var team in new[]{CombatTeam.Player,CombatTeam.Enemy})
            {
                var ownerData=new SerializedObject(player.GetComponent<Health>());ownerData.FindProperty("team").enumValueIndex=(int)team;ownerData.ApplyModifiedPropertiesWithoutUndo();
                var targetData=new SerializedObject(target);targetData.FindProperty("team").enumValueIndex=(int)(team==CombatTeam.Player?CombatTeam.Enemy:CombatTeam.Player);targetData.ApplyModifiedPropertiesWithoutUndo();target.Restore();
                Physics2D.SyncTransforms();Assert.IsTrue(executor.Attack(weapon,Vector2.right,Time.time));
                Assert.Less(target.Current,target.Maximum);Assert.IsTrue(cue.GetComponent<SpriteRenderer>().enabled);
                Assert.AreEqual(.216f,cue.localScale.x,.0001f);Assert.AreEqual(.216f,cue.localScale.y,.0001f);yield return new WaitForSeconds(.25f);
            }
            var wall=Keep(new GameObject("ImpactScaleWall"));wall.transform.position=new Vector3(2002,1);wall.AddComponent<BoxCollider2D>().size=new Vector2(1,3);Physics2D.SyncTransforms();
            Assert.IsTrue(executor.Attack(weapon,Vector2.right,Time.time));Assert.AreEqual(ShotSurface.Geometry,executor.LastSurface);
            var feedback=player.GetComponent<CombatFeedback>();var so=new SerializedObject(feedback);
            float originalSize=so.FindProperty("surfaceImpactSprite").objectReferenceValue!=null?1:.18f;
            Assert.AreEqual(originalSize,player.transform.Find("SurfaceImpact").localScale.x,.0001f);
        }
        [UnityTest] public IEnumerator MeleeRequiresShortUnobstructedRangeAndNoAmmo()
        {
            var target = Target(2001.2f); var weapon = loadout.Get(EquipmentSlot.Melee);
            Assert.IsTrue(executor.Attack(weapon, Vector2.right, 0)); Assert.AreEqual(15, target.Current);
            target.transform.position = new Vector3(2003, 1); Physics2D.SyncTransforms();
            Assert.IsTrue(executor.Attack(weapon, Vector2.right, 1)); Assert.AreEqual(15, target.Current);
            Assert.AreEqual(0, weapon.MagazineAmmo); Assert.AreEqual(0, weapon.ReserveAmmo); yield return null;
        }
        [UnityTest] public IEnumerator PlayerFireSwitchInterruptAndDeathRecoveryPreserveInventory()
        {
            actions.SetTouchMove(Vector2.left); actions.SetTouchAim(Vector2.right); actions.SetFireIntent(true);
            yield return null; yield return null;
            Assert.Greater(executor.ShotsFired, 0); Assert.Less(actions.Move.x, 0); Assert.Greater(actions.AimDirection.x, 0);
            actions.SetFireIntent(false); yield return null;
            var weapon = loadout.SelectedWeapon; weapon.SetAmmunition(3, 17); actions.RequestReload(); Assert.IsTrue(weapon.Cycle.IsReloading);
            loadout.Select(EquipmentSlot.Firearm2); loadout.Select(EquipmentSlot.Firearm1);
            Assert.IsFalse(weapon.Cycle.IsReloading); Assert.AreEqual(3, weapon.MagazineAmmo);
            actions.RequestReload(); var health = player.GetComponent<Health>(); health.ProtectFor(0);
            player.GetComponent<PlayerRevive>()?.RestoreInventory(0); // Exercise finite-stock checkpoint fallback.
            Assert.IsTrue(health.ApplyDamage(new DamageInfo(1000, CombatTeam.Enemy)));
            Assert.AreEqual(health.Maximum, health.Current); Assert.IsFalse(health.IsDefeated);
            Assert.AreSame(weapon, loadout.SelectedWeapon); Assert.AreEqual(3, weapon.MagazineAmmo); Assert.AreEqual(17, weapon.ReserveAmmo);
            Assert.IsFalse(weapon.Cycle.IsReloading); Assert.IsFalse(actions.FireHeld);
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(1000, CombatTeam.Enemy)));
        }
        [UnityTest] public IEnumerator LarperEngagesShootsAndReturnsToPatrolWhenTargetLeaves()
        {
            var enemy = Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"), new Vector3(2004, 1), Quaternion.identity)); Pin(enemy);
            enemy.GetComponent<LarperBrain>().Configure(Asset<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            var combat = enemy.GetComponent<EnemyCombat>(); combat.BindTarget(player.GetComponent<Health>());
            CombatTestView.Frame(combat);
            var policy=new SerializedObject(combat);foreach(string key in new[]{"ordinaryBlindChance","cowardBlindChance","overconfidentBlindChance"})policy.FindProperty(key).floatValue=0;policy.ApplyModifiedPropertiesWithoutUndo(); // Explicitly exercise the non-spray branch.
            // Allow several imperfect shots after the configurable reaction jitter.
            for(int i=0;i<60&&player.GetComponent<Health>().Current==player.GetComponent<Health>().Maximum;i++)yield return new WaitForFixedUpdate();
            Assert.AreEqual(EngagementState.Engaging, combat.State); Assert.Greater(enemy.GetComponent<AttackExecutor>().ShotsFired, 0);
            Assert.Less(player.GetComponent<Health>().Current, player.GetComponent<Health>().Maximum);
            Assert.Less(combat.AimDirection.x, 0);
            player.transform.position = new Vector3(2050, 1); Physics2D.SyncTransforms(); yield return new WaitForSeconds(3.2f);
            Assert.AreEqual(EngagementState.Patrol, combat.State); Assert.IsFalse(enemy.GetComponent<LarperBrain>().DecisionsSuspended);
        }
        [UnityTest] public IEnumerator AuthoredLadderAscentUsesExistingTraversalAndRestoresPatrol()
        {
            var landing = Keep(new GameObject("RouteLanding")); landing.transform.position = new Vector3(2012.5f, 2.9f);
            landing.AddComponent<BoxCollider2D>().size = new Vector2(3, .3f);
            var ladderObject = Keep(new GameObject("RouteLadder")); ladderObject.transform.position = new Vector3(2010, 1.5f);
            var box = ladderObject.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(2, 5);
            var ladder = ladderObject.AddComponent<LadderVolume>();
            var enemy = Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"), new Vector3(2010, 1), Quaternion.identity));
            enemy.GetComponent<LarperBrain>().Configure(Asset<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            var route = enemy.GetComponent<AuthoredLadderRoute>(); route.Configure(ladder, 4); Physics2D.SyncTransforms();
            bool climbed = false;
            for (int step = 0; step < 150 && route.State != LadderRouteState.Complete; step++)
            { yield return new WaitForFixedUpdate(); climbed |= enemy.GetComponent<PlayerTraversal2D>().IsClimbing; }
            Assert.IsTrue(climbed); Assert.AreEqual(LadderRouteState.Complete, route.State);
            Assert.AreEqual(3, enemy.GetComponent<Rigidbody2D>().gravityScale); Assert.IsFalse(enemy.GetComponent<LarperBrain>().DecisionsSuspended);
        }
        [UnityTest] public IEnumerator TwoThumbAimOnlyAndOuterFireZoneRemainIndependent()
        {
            var ui = Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))); ui.GetComponent<WarehouseTouchControls>().Bind(actions);
            yield return null; yield return null; var root = ui.GetComponent<UIDocument>().rootVisualElement;
            var move = root.Q<VisualElement>("moveStick"); var aim = root.Q<VisualElement>("aimStick");
            using(var evt=PointerDownEvent.GetPooled(new Touch{fingerId=0,position=move.worldBound.center+Vector2.left*40,phase=TouchPhase.Began},EventModifiers.None))move.SendEvent(evt);
            using(var evt=PointerDownEvent.GetPooled(new Touch{fingerId=1,position=aim.worldBound.center+Vector2.right*20,phase=TouchPhase.Began},EventModifiers.None))aim.SendEvent(evt);
            yield return null; Assert.Less(actions.Move.x,0); Assert.Greater(actions.AimDirection.x,0); Assert.IsFalse(actions.FireHeld);
            int before=executor.ShotsFired;
            using(var evt=PointerMoveEvent.GetPooled(new Touch{fingerId=1,position=aim.worldBound.center+Vector2.right*aim.worldBound.width*.35f*.95f,phase=TouchPhase.Moved},EventModifiers.None))aim.SendEvent(evt);
            yield return null; yield return null; Assert.IsTrue(actions.FireHeld); Assert.Greater(executor.ShotsFired,before); Assert.Less(actions.Move.x,0);
            using(var evt=PointerCancelEvent.GetPooled(new Touch{fingerId=1,position=aim.worldBound.center,phase=TouchPhase.Canceled},EventModifiers.None))aim.SendEvent(evt);
            yield return null; Assert.IsFalse(actions.FireHeld); Assert.Less(actions.Move.x,0);
        }
        [UnityTest] public IEnumerator InvalidAimDoesNotConsumeAmmunitionOrStartCooldown()
        {
            var weapon=loadout.SelectedWeapon;int ammo=weapon.MagazineAmmo;
            Assert.IsFalse(executor.Attack(weapon,Vector2.zero,0));
            Assert.IsFalse(executor.Attack(weapon,new Vector2(float.NaN,1),0));
            Assert.IsFalse(executor.Attack(weapon,new Vector2(1,float.PositiveInfinity),0));
            Assert.AreEqual(ammo,weapon.MagazineAmmo);Assert.IsTrue(executor.Attack(weapon,Vector2.right,0));yield return null;
        }
        [UnityTest] public IEnumerator UnavailableLadderRouteReleasesMotionAndReloadButtonIsReachable()
        {
            var ladderObject=Keep(new GameObject("UnavailableRouteLadder"));ladderObject.transform.position=new Vector3(2010,1.5f);
            var box=ladderObject.AddComponent<BoxCollider2D>();box.isTrigger=true;box.size=new Vector2(2,5);var ladder=ladderObject.AddComponent<LadderVolume>();
            var enemy=Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(2010,1),Quaternion.identity));
            enemy.GetComponent<LarperBrain>().Configure(Asset<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            var route=enemy.GetComponent<AuthoredLadderRoute>();route.Configure(ladder,4);Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();ladder.enabled=false;
            yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            Assert.AreEqual(LadderRouteState.Unavailable,route.State);Assert.IsFalse(enemy.GetComponent<PlayerTraversal2D>().IsClimbing);
            Assert.IsTrue(enemy.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().enabled);Assert.AreEqual(3,enemy.GetComponent<Rigidbody2D>().gravityScale);
            var ui=Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));ui.GetComponent<WarehouseTouchControls>().Bind(actions);
            yield return null;yield return null;var root=ui.GetComponent<UIDocument>().rootVisualElement;
            var reload=root.Q<Button>("reload");Assert.Greater(reload.worldBound.width,70);
            Assert.IsFalse(reload.worldBound.Overlaps(root.Q<VisualElement>("aimStick").worldBound));
            var weapon=loadout.SelectedWeapon;weapon.SetAmmunition(1,10);
            using(var down=PointerDownEvent.GetPooled(new Touch{fingerId=4,position=reload.worldBound.center,phase=TouchPhase.Began},EventModifiers.None))reload.SendEvent(down);
            using(var up=PointerUpEvent.GetPooled(new Touch{fingerId=4,position=reload.worldBound.center,phase=TouchPhase.Ended},EventModifiers.None))reload.SendEvent(up);
            yield return null;Assert.IsTrue(weapon.Cycle.IsReloading);
        }
    }
}
#endif
