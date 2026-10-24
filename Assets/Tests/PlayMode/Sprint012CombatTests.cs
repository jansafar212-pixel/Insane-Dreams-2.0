#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint012CombatTests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private GameObject player;private PlayerActions actions;private bool background;private Camera camera;private UIDocument document;
        private T Asset<T>(string path)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path);
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private GameObject Solid(string name,Vector2 p,Vector2 size){var go=Keep(new GameObject(name));go.transform.position=p;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;LarperAwareness.ResetEncounterSpeech();
            Solid("S012Floor",new Vector2(180,-.5f),new Vector2(60,1));player=Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(180,.85f),Quaternion.identity));
            actions=player.GetComponent<PlayerActions>();actions.ResetActions();
            camera=Keep(new GameObject("S012Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=8;camera.transform.position=new Vector3(182,3,-10);
            document=Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();document.GetComponent<WarehouseTouchControls>().Bind(actions);yield return new WaitForSeconds(.3f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var d in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))d.Retire();foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;}
        private EnemyCombat Enemy()
        {
            player.GetComponent<Health>().ProtectFor(60);
            var go=Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(184,.85f),Quaternion.identity));go.GetComponent<LarperBrain>().Configure(Asset<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            go.GetComponent<LarperAwareness>().Configure(Asset<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Ordinary.asset"));go.GetComponent<LarperSpeechBubble>().Bind(camera,document);
            var e=go.GetComponent<EnemyCombat>();e.BindTarget(player.GetComponent<Health>());e.BindGameplayCamera(camera);return e;
        }
        private void BlindPolicy(EnemyCombat e,float chance=1,float seconds=2)
        {var s=new SerializedObject(e);foreach(string key in new[]{"ordinaryBlindChance","cowardBlindChance","overconfidentBlindChance"})s.FindProperty(key).floatValue=chance;s.FindProperty("blindFireSeconds").floatValue=seconds;s.ApplyModifiedPropertiesWithoutUndo();}
        private IEnumerator Engage(EnemyCombat e){for(int i=0;i<60&&e.GetComponent<AttackExecutor>().ShotsFired<2;i++)yield return new WaitForFixedUpdate();Assert.GreaterOrEqual(e.GetComponent<AttackExecutor>().ShotsFired,2);}
        [UnityTest] public IEnumerator FirstSightReactionIsSlowerButEstablishedAutomaticFireKeepsItsCadence()
        {
            var e=Enemy();var a=e.GetComponent<AttackExecutor>();float until=Time.time+2;
            while(!e.TargetVisible&&Time.time<until)yield return null;
            Assert.IsTrue(e.TargetVisible);float seen=Time.time;
            while(a.ShotsFired==0&&Time.time<until)yield return null;
            Assert.Greater(a.ShotsFired,0);Assert.That(Time.time-seen,Is.InRange(.52f,1.4f));
            int first=a.ShotsFired;float fired=Time.time;
            while(a.ShotsFired==first&&Time.time<until)yield return null;
            Assert.Greater(a.ShotsFired,first);Assert.Less(Time.time-fired,.25f);
        }
        [UnityTest] public IEnumerator BlindBurstFreezesMemoryAndConsumesRealAmmoIntoCover()
        {
            var e=Enemy();BlindPolicy(e);yield return Engage(e);var a=e.GetComponent<AttackExecutor>();Vector2 known=e.LastKnownPosition;int shots=a.ShotsFired,ammo=e.Weapon.MagazineAmmo;
            Solid("BlindCover",new Vector2(182,1),new Vector2(.3f,8));player.GetComponent<Rigidbody2D>().position=new Vector2(176,3);player.transform.position=new Vector3(176,3);Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.45f);Assert.IsTrue(e.IsBlindFiring);Assert.AreEqual(known,e.LastKnownPosition);Assert.Greater(a.ShotsFired,shots);Assert.AreEqual(ammo-(a.ShotsFired-shots),e.Weapon.MagazineAmmo);Assert.AreEqual(ShotSurface.Geometry,a.LastSurface);
            Assert.Less(Vector2.Angle(e.AimDirection,known-(Vector2)e.transform.position),2);
        }
        [UnityTest] public IEnumerator BlindTimeoutStopsWithoutRefreshingOrGrantingAmmo()
        {
            var e=Enemy();BlindPolicy(e,1,.3f);yield return Engage(e);Solid("TimeoutCover",new Vector2(182,1),new Vector2(.3f,8));yield return new WaitForSeconds(.5f);
            Assert.IsFalse(e.IsBlindFiring);Assert.Greater(e.Weapon.MagazineAmmo,0);int shots=e.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(.4f);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator EmptyBlindMagazineTransitionsToExistingRetreatAndReload()
        {
            var e=Enemy();BlindPolicy(e);yield return Engage(e);var settings=new SerializedObject(e);foreach(var key in new[]{"ordinaryReloadFraction","cowardReloadFraction","overconfidentReloadFraction"})settings.FindProperty(key).floatValue=0;settings.ApplyModifiedPropertiesWithoutUndo();e.Weapon.SetAmmunition(3,36);Solid("EmptyCover",new Vector2(182,1),new Vector2(.3f,8));
            for(int i=0;i<110&&e.State!=EngagementState.Reloading;i++)yield return new WaitForFixedUpdate();
            Assert.AreEqual(EngagementState.Reloading,e.State);Assert.AreEqual(0,e.Weapon.MagazineAmmo);Assert.IsFalse(e.IsBlindFiring);Assert.Greater(e.transform.position.x,186.4f);
            yield return new WaitForSeconds(e.Weapon.Definition.ReloadDuration*1.05f+.2f);Assert.AreEqual(27,e.Weapon.MagazineAmmo);Assert.AreEqual(9,e.Weapon.ReserveAmmo);Assert.IsFalse(e.IsBlindFiring);
        }
        [UnityTest] public IEnumerator NeverSeenAndDeclinedBlindDecisionCannotStartSpraying()
        {
            var wall=Solid("UndetectedCover",new Vector2(182,1),new Vector2(.3f,8));var e=Enemy();BlindPolicy(e,0);yield return new WaitForSeconds(.5f);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);
            Object.Destroy(wall);yield return null;yield return Engage(e);int shots=e.GetComponent<AttackExecutor>().ShotsFired;Solid("DeclinedCover",new Vector2(182,1),new Vector2(.3f,8));yield return new WaitForSeconds(.3f);Assert.IsFalse(e.IsBlindFiring);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator LowCoverBlocksBothCrouchDirectionsButStandingAndUncoveredIncomingShotsCanHit()
        {
            var cover=Solid("RepresentativeLowCover",new Vector2(182,.45f),new Vector2(1.6f,.9f));
            var target=Solid("CoverTarget",new Vector2(185,1),new Vector2(.8f,1.4f)).AddComponent<Health>();
            var attack=player.GetComponent<AttackExecutor>();var weapon=player.GetComponent<PlayerLoadout>().SelectedWeapon;float now=Time.time;
            foreach(float sign in new[]{1f,-1f})
            {
                cover.transform.position=new Vector3(180+sign*2,.45f);target.transform.position=new Vector3(180+sign*5,1);Physics2D.SyncTransforms();target.Restore();
                actions.SetTouchMove(Vector2.zero);actions.SetTouchAim(Vector2.right*sign);yield return new WaitForSeconds(.15f);
                float standBy=Time.realtimeSinceStartup+1;
                while((player.GetComponent<PlayerPosture2D>().IsCrouched||attack.Origin.y<=cover.GetComponent<Collider2D>().bounds.max.y+.03f)&&Time.realtimeSinceStartup<standBy)yield return null;
                Assert.IsTrue(attack.Attack(weapon,Vector2.right*sign,now+=1));Assert.Less(target.Current,target.Maximum);
                float health=target.Current;actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.15f);
                float crouchBy=Time.realtimeSinceStartup+1;
                while((!player.GetComponent<PlayerPosture2D>().IsCrouched||attack.Origin.y>=.9f)&&Time.realtimeSinceStartup<crouchBy)yield return null;
                Assert.IsTrue(attack.Attack(weapon,new Vector2(sign,.15f),now+=1));Assert.AreEqual(health,target.Current);Assert.AreEqual(ShotSurface.Geometry,attack.LastSurface);
                Assert.Less(attack.LastShotOrigin.y,.9f);
            }
            actions.SetTouchAim(Vector2.up);yield return new WaitForSeconds(.1f);Assert.IsTrue(attack.Attack(weapon,Vector2.up,now+=1));
            Assert.Greater(attack.LastShotOrigin.y,.1f);Assert.Less(attack.LastShotOrigin.y,.8f);
            var rig=player.GetComponent<CharacterVisualDriver>().Rig;Assert.Less(Vector3.Distance(rig.rightArm.TransformPoint(Vector3.down*rig.skin.arm.bounds.size.y),rig.weaponMount.position),.002f);
            target.gameObject.SetActive(false);cover.transform.position=new Vector3(182,.45f);Physics2D.SyncTransforms();
            var shooter=Keep(new GameObject("IncomingPistol"));shooter.transform.position=new Vector3(185,.55f);shooter.AddComponent<Health>();var incoming=shooter.AddComponent<AttackExecutor>();var pistol=new RuntimeWeapon(Asset<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset"));
            float before=player.GetComponent<Health>().Current;incoming.Attack(pistol,Vector2.left,now+=1);Assert.AreEqual(before,player.GetComponent<Health>().Current);
            Object.Destroy(cover);yield return null;Physics2D.SyncTransforms();incoming.Attack(pistol,Vector2.left,now+=1);Assert.Less(player.GetComponent<Health>().Current,before);
        }
        [UnityTest] public IEnumerator FullPistolBurstCommitsToRealRetreatThroughLostSightAndReloadsOnce()
        {
            var e=Enemy();var reliability=new SerializedObject(e);reliability.FindProperty("unreliableWeapon").boolValue=false;reliability.ApplyModifiedPropertiesWithoutUndo(); // Uninterrupted-burst baseline; Sprint021 covers actual malfunction/recovery.
            int movedEvents=0,reloads=0,retreatLines=0;e.WithdrawalStarted+=()=>movedEvents++;e.ReloadStarted+=()=>reloads++;
            e.GetComponent<LarperAwareness>().LineDelivered+=c=>{if(c==LarperSpeechEvent.Withdraw)retreatLines++;};float start=e.transform.position.x;
            for(int i=0;i<180&&e.State!=EngagementState.Withdrawing;i++)yield return new WaitForFixedUpdate();
            Assert.AreEqual(EngagementState.Withdrawing,e.State);Assert.Greater(e.GetComponent<AttackExecutor>().ShotsFired,10);
            Solid("SightLostMidRetreat",new Vector2(182,1),new Vector2(.2f,4));
            for(int i=0;i<100&&e.State!=EngagementState.Reloading;i++)yield return new WaitForFixedUpdate();
            Assert.AreEqual(EngagementState.Reloading,e.State);Assert.GreaterOrEqual(e.transform.position.x-start,2.5f);Assert.AreEqual(1,movedEvents);Assert.AreEqual(1,reloads);
            for(int i=0;i<80&&retreatLines==0;i++)yield return new WaitForFixedUpdate();Assert.AreEqual(1,retreatLines,"Meaningful dialogue waits for the previous readable bubble.");
            int shots=e.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(.4f);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);Assert.AreEqual(1,reloads);
        }
        [UnityTest] public IEnumerator OversizedMagazineFumblesVisiblyAndDeathCannotFinishReload()
        {
            var e=Enemy();e.BindTarget(null);e.Weapon.SetAmmunition(0,36);Assert.IsTrue(e.Weapon.Cycle.TryReload(Time.time));
            yield return new WaitForSeconds(e.Weapon.Definition.ReloadDuration*.25f);var v=e.GetComponent<CharacterVisualDriver>();var rig=v.Rig;
            Assert.IsTrue(v.ReloadPoseActive);Assert.IsTrue(rig.weapon.enabled);Assert.IsTrue(rig.magazine.enabled);
            // The accepted 0025.3 exchange replaced the old levitating fumble.
            Assert.Greater(Vector2.Distance(rig.magazine.transform.localPosition,e.Weapon.Definition.MagazineMountOffset),.01f);
            var hand=rig.Segmented.leftHand.TransformPoint(Vector3.down*rig.Segmented.hand.bounds.size.y*.55f);
            Assert.Less(Vector3.Distance(hand,rig.magazine.transform.position),.055f);Assert.Less(Vector3.Distance(rig.weaponMount.position,rig.Segmented.grip.position),.01f);Assert.AreEqual(0,e.Weapon.MagazineAmmo);
            float deadline=e.Weapon.Cycle.ReloadEndsAt;e.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));e.Weapon.Cycle.Tick(deadline+1);
            Assert.AreEqual(0,e.Weapon.MagazineAmmo);Assert.AreEqual(36,e.Weapon.ReserveAmmo);
        }
        [UnityTest] public IEnumerator Sprint0122DistantVisibleTargetDrawsInaccuratePrematureFire()
        {
            var e=Enemy();player.GetComponent<Rigidbody2D>().position=new Vector2(163,.85f);Physics2D.SyncTransforms();
            var s=new SerializedObject(e);s.FindProperty("preferredDistance").floatValue=30;s.ApplyModifiedPropertiesWithoutUndo();
            s.FindProperty("engagementRange").floatValue=24;s.FindProperty("detectionRange").floatValue=28;s.ApplyModifiedPropertiesWithoutUndo(); // Preserve coverage of configurable premature range beyond the new default.
            yield return Engage(e);Assert.IsTrue(e.TargetVisible);Assert.Greater(Vector2.Distance(e.transform.position,player.transform.position),20);
            Assert.AreEqual(40,e.SpreadAtDistance(21));Assert.AreEqual(3,e.SpreadAtDistance(2));Assert.AreEqual(28,e.Weapon.Definition.AttackRange);
            Assert.Less(e.Weapon.MagazineAmmo,27);
        }
        [UnityTest] public IEnumerator Sprint0122PointBlankBurstStillDealsRapidRealDamage()
        {
            var e=Enemy();player.GetComponent<Rigidbody2D>().position=new Vector2(182,.85f);player.GetComponent<Health>().ProtectFor(0);Physics2D.SyncTransforms();
            yield return new WaitForSeconds(1.1f);Assert.GreaterOrEqual(e.GetComponent<AttackExecutor>().ShotsFired,5);
            Assert.LessOrEqual(player.GetComponent<Health>().Current,75);
        }
        [UnityTest] public IEnumerator Sprint0122FallbackRetreatTravelsSixUnitsAndReloadCannotFinishEarly()
        {
            var e=Enemy();e.Weapon.SetAmmunition(0,36);float start=e.transform.position.x;
            for(int i=0;i<200&&e.State!=EngagementState.Reloading;i++)yield return new WaitForFixedUpdate();
            Assert.AreEqual(EngagementState.Reloading,e.State);Assert.IsNull(e.SelectedCover);Assert.That(e.transform.position.x-start,Is.InRange(5f,7f));
            float duration=e.Weapon.Cycle.ReloadEndsAt-e.Weapon.Cycle.ReloadStartedAt;Assert.That(duration,Is.InRange(3.8f,4.2f));
            e.BindTarget(null); // A committed reload survives target loss; do not sample after a fresh shot.
            int shots=e.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(3.5f);
            Assert.IsTrue(e.Weapon.Cycle.IsReloading);Assert.AreEqual(0,e.Weapon.MagazineAmmo);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);
            yield return new WaitForSeconds(.8f);Assert.AreEqual(27,e.Weapon.MagazineAmmo);Assert.AreEqual(9,e.Weapon.ReserveAmmo);
        }
        [UnityTest] public IEnumerator Sprint0122ReachableLowCoverIsPreferredReachedAndCrouchedDuringReload()
        {
            var cover=Solid("RetreatLowCover",new Vector2(188.5f,.45f),new Vector2(1.2f,.9f));var e=Enemy();e.Weapon.SetAmmunition(0,36);
            Solid("CheckpointTriggerBeyondCover",new Vector2(190.5f,1.5f),new Vector2(2,3)).GetComponent<Collider2D>().isTrigger=true;
            float start=e.transform.position.x;bool selected=false,jumped=false;
            for(int i=0;i<300&&!e.IsReloadingCrouched;i++)
            {selected|=e.SelectedCover==cover.GetComponent<Collider2D>();jumped|=e.GetComponent<Rigidbody2D>().linearVelocity.y>1;yield return new WaitForFixedUpdate();}
            Assert.IsTrue(selected,"A reachable crate must win over plain retreat.");Assert.IsTrue(jumped,"Existing motor must physically jump the low crate.");
            Assert.That(e.transform.position.x-start,Is.InRange(5f,7f));Assert.IsTrue(e.IsReloadingCrouched,"Must arrive and crouch behind real cover.");
            Assert.Less(Mathf.Abs(e.transform.position.y-.71f),.25f,"Perching on the crate is not protected cover.");
            var hits=new RaycastHit2D[8];Assert.IsTrue(CombatVisibility2D.IsBlocked(new Vector2(180,.65f),e.GetComponent<BoxCollider2D>().bounds.center,1,hits));
            Assert.IsFalse(CombatVisibility2D.IsBlocked(new Vector2(180,1.25f),new Vector2(e.transform.position.x,1.25f),1,hits));
            yield return new WaitForSeconds(4.5f);Assert.IsFalse(e.GetComponent<InsaneDreams.Traversal.PlayerPosture2D>().IsCrouched);Assert.Greater(e.Weapon.MagazineAmmo,0);
        }
        [UnityTest] public IEnumerator Sprint0122UnreachableCoverRejectsCeilingAndFallsBackWithoutLooping()
        {
            Solid("UnsafeCover",new Vector2(188.5f,.45f),new Vector2(1.2f,.9f));Solid("HopCeiling",new Vector2(188.5f,2.1f),new Vector2(2,.3f));
            var e=Enemy();e.Weapon.SetAmmunition(0,36);
            for(int i=0;i<220&&e.State!=EngagementState.Reloading;i++)yield return new WaitForFixedUpdate();
            Assert.IsNull(e.SelectedCover);Assert.AreEqual(EngagementState.Reloading,e.State);Assert.IsTrue(e.Weapon.Cycle.IsReloading);
            Assert.Less(e.transform.position.x,188);
        }
        [UnityTest] public IEnumerator BlockedRetreatReloadsInPlaceWithoutFalseMovementCalloutOrRepeatedEvents()
        {
            Solid("RetreatWall",new Vector2(184.7f,1),new Vector2(.2f,3));var e=Enemy();e.Weapon.SetAmmunition(0,36);int moves=0,reloads=0,lines=0;
            e.WithdrawalStarted+=()=>moves++;e.ReloadStarted+=()=>reloads++;e.GetComponent<LarperAwareness>().LineDelivered+=c=>{if(c==LarperSpeechEvent.Withdraw)lines++;};
            // Wait through the existing .55s reaction plus jitter before checking blocked reload.
            float until=Time.time+1.5f;while(e.State!=EngagementState.Reloading&&Time.time<until)yield return new WaitForFixedUpdate();
            Assert.AreEqual(EngagementState.Reloading,e.State);Assert.AreEqual(0,moves);Assert.AreEqual(0,lines);Assert.AreEqual(1,reloads);
            yield return new WaitForSeconds(.4f);Assert.AreEqual(1,reloads);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);
        }
    }
}
#endif
