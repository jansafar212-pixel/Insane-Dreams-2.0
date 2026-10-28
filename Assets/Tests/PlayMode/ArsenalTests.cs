#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class ArsenalTests
    {
        const float X=12000;readonly List<GameObject> owned=new List<GameObject>();GameObject player;PlayerActions input;PlayerCombat combat;PlayerLoadout loadout;AttackExecutor attack;ArsenalCatalog catalog;bool background;float volume;UnityEngine.Random.State random;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;random=UnityEngine.Random.state;UnityEngine.Random.InitState(33033);
            var floor=Keep(new GameObject("ArsenalFloor"));floor.transform.position=new Vector2(X,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(160,1);
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,.72f),Quaternion.identity));input=player.GetComponent<PlayerActions>();combat=player.GetComponent<PlayerCombat>();loadout=player.GetComponent<PlayerLoadout>();attack=player.GetComponent<AttackExecutor>();
            var s=new SerializedObject(input);s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();player.GetComponent<Health>().ProtectFor(300);catalog=Resources.Load<ArsenalCatalog>("ArsenalCatalog");yield return null;yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;foreach(var g in owned)if(g!=null)Object.Destroy(g);foreach(var d in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))d.Retire();owned.Clear();yield return null;Application.runInBackground=background;AudioListener.volume=volume;UnityEngine.Random.state=random;}
        RuntimeWeapon Equip(int index){input.SuspendInput();var w=new RuntimeWeapon(catalog.weapons[index],180);Assert.IsTrue(loadout.Equip(EquipmentSlot.Firearm1,w));loadout.Select(EquipmentSlot.Firearm1);return w;}
        IEnumerator Hold(Vector2 aim){input.BeginTouchAim();input.SetTouchAim(aim);input.SetFireIntent(true);yield return null;yield return null;}
        void Release(bool valid){input.EndTouchAim(valid);input.SetTouchAim(Vector2.zero);input.SetFireIntent(false);}
        [UnityTest] public IEnumerator SniperHoldCancelReleaseAndAutomaticRecoveryNeverDoubleFire()
        {
            var w=Equip(6);yield return null;yield return Hold(Vector2.right);Assert.IsTrue(combat.SniperAiming);Assert.IsTrue(combat.AimLaser.enabled);var beam=combat.AimLaser;Assert.Greater(Vector3.Distance(beam.GetPosition(0),beam.GetPosition(1)),20,"Unobstructed aiming laser extends across the sightline");Assert.AreEqual(0,attack.ShotsFired);Release(true);yield return null;Assert.AreEqual(1,attack.ShotsFired);Assert.AreEqual(4,w.MagazineAmmo);Assert.IsTrue(w.Cycle.IsRecovering(Time.time));Assert.IsFalse(combat.AimLaser.enabled);
            Release(true);yield return null;Assert.AreEqual(1,attack.ShotsFired);yield return new WaitForSeconds(1.05f);
            yield return Hold(Vector2.up);input.SetTouchAim(Vector2.zero);Release(true);yield return null;Assert.AreEqual(1,attack.ShotsFired,"Center-release cancels");
            yield return Hold(Vector2.right);Release(false);yield return null;Assert.AreEqual(1,attack.ShotsFired,"Capture loss/cancel never shoots");
            yield return Hold(Vector2.left);Release(true);yield return null;Assert.AreEqual(2,attack.ShotsFired);Assert.AreSame(beam,combat.AimLaser,"Repeated aim reuses one renderer");
        }
        [UnityTest] public IEnumerator SniperSwitchPauseReloadResetDeathAndRevivalCannotReleaseAnAccidentalShot()
        {
            Equip(6);yield return null;yield return Hold(Vector2.right);loadout.Equip(EquipmentSlot.Firearm2,new RuntimeWeapon(catalog.weapons[0],20));loadout.Select(EquipmentSlot.Firearm2);Release(true);yield return null;Assert.AreEqual(0,attack.ShotsFired);
            Equip(6);yield return null;var grenades=player.GetComponent<PlayerThrowables>();grenades.RestoreStock(new[]{1,0,0});yield return Hold(Vector2.right);input.SetTouchMove(Vector2.left);yield return null;loadout.Select(EquipmentSlot.Melee);yield return null;Release(true);yield return null;Assert.AreEqual(0,grenades.Throws,"Old sniper hold cannot become a grenade throw");Assert.Less(input.Move.x,0,"Cancelling weapon aim must preserve movement");
            Equip(6);yield return null;yield return Hold(Vector2.right);Time.timeScale=0;Release(true);input.SuspendInput();Time.timeScale=1;yield return null;Assert.AreEqual(0,attack.ShotsFired);
            yield return Hold(Vector2.right);input.SuspendInput();Release(true);yield return null;Assert.AreEqual(0,attack.ShotsFired);
            var w=loadout.SelectedWeapon;w.SetAmmunition(2,20);yield return Hold(Vector2.right);combat.Reload();Release(true);yield return null;Assert.AreEqual(0,attack.ShotsFired);Assert.IsTrue(w.Cycle.IsReloading);combat.Cancel();Assert.AreEqual(2,w.MagazineAmmo);
            var revive=player.GetComponent<PlayerRevive>();revive.RestoreInventory(1);yield return Hold(Vector2.right);var hp=player.GetComponent<Health>();hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy,null));Release(true);yield return null;Assert.IsTrue(hp.IsDefeated);Assert.AreEqual(0,attack.ShotsFired);Assert.IsTrue(revive.TryRevive());input.SuspendInput();yield return null;Release(true);yield return null;Assert.AreEqual(0,attack.ShotsFired);
        }
        [UnityTest] public IEnumerator ShotgunPelletsConsumeOneRoundAndDoNotDuplicateColliderDamage()
        {
            var w=Equip(4);yield return null;var target=Keep(new GameObject("DoubleColliderTarget"));target.layer=2;target.transform.position=new Vector2(X+3,1);target.AddComponent<BoxCollider2D>().size=new Vector2(1,5);target.AddComponent<BoxCollider2D>().size=new Vector2(.9f,4.9f);
            var hp=target.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)CombatTeam.Enemy;s.FindProperty("maximum").floatValue=1000;s.ApplyModifiedPropertiesWithoutUndo();hp.Restore();Physics2D.SyncTransforms();int events=0,shots=0;hp.Damaged+=_=>events++;attack.Fired+=(a,b,c)=>shots++;
            Assert.IsTrue(attack.Attack(w,Vector2.right,Time.time));Assert.AreEqual(5,w.MagazineAmmo);Assert.AreEqual(7,events);Assert.AreEqual(944,hp.Current,.1f);Assert.AreEqual(1,shots);Assert.AreEqual(1,attack.ShotsFired);
        }
        [UnityTest] public IEnumerator EagleUsesNormalHeldFireWhileMovingAndJumpingAndMuzzleRemainsAligned()
        {
            var w=Equip(1);yield return null;input.SetTouchMove(Vector2.right);input.SetTouchAim(Vector2.right);input.SetFireIntent(true);input.RequestTouchJump();yield return new WaitForSeconds(.8f);Assert.GreaterOrEqual(attack.ShotsFired,2);Assert.IsFalse(combat.SniperAiming);Assert.IsNull(combat.AimLaser);Assert.Less(w.MagazineAmmo,7);
            input.SetFireIntent(false);float shotTime=Time.time+10;foreach(var aim in new[]{Vector2.left,Vector2.right,Vector2.up,Vector2.down})
            {input.SetTouchAim(aim);yield return null;float now=shotTime+=5;w.Cycle.Tick(now);Assert.IsTrue(attack.Attack(w,aim,now));Assert.Greater(Vector2.Dot(attack.LastShotDirection,aim),.99f);Assert.Less(Vector2.Distance(attack.LastShotOrigin,player.GetComponentInChildren<CutoutRig>().MuzzlePosition),.01f);w.Cycle.TryReload(now);w.Cycle.Tick(now+4);}
        }
        [UnityTest] public IEnumerator SustainedAutomaticAndPelletFireKeepTheEffectPoolBounded()
        {
            int pool=player.GetComponentsInChildren<LineRenderer>(true).Length;var watch=System.Diagnostics.Stopwatch.StartNew();
            foreach(int index in new[]{2,4,5}){var w=Equip(index);yield return null;for(int i=0;i<120;i++){w.SetAmmunition(w.Definition.MagazineCapacity,180);Assert.IsTrue(attack.Attack(w,Vector2.right,Time.time+20+i*2));}Assert.AreEqual(pool,player.GetComponentsInChildren<LineRenderer>(true).Length);}
            watch.Stop();System.IO.File.WriteAllText("TestResults/Arsenal/Stress.txt","Controlled Editor 360 trigger pulls / 1080 hitscan segments; bounded line pool="+pool+"; elapsed including yielded frames ms="+watch.Elapsed.TotalMilliseconds);Assert.AreEqual(360,attack.ShotsFired);
        }
        [UnityTest] public IEnumerator AllWeaponsDamageAllFourArchetypesThroughActualHitboxes()
        {
            foreach(var definition in catalog.weapons)foreach(var path in new[]{"Assets/Prefabs/Enemies/Larper.prefab","Assets/Prefabs/Enemies/Samurai.prefab","Assets/Prefabs/Enemies/FireworksExpert.prefab","Assets/Data/FirstOperation/Officer.prefab"})
            {
                var w=new RuntimeWeapon(definition,180);loadout.Equip(EquipmentSlot.Firearm1,w);loadout.Select(EquipmentSlot.Firearm1);
                var enemy=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),new Vector3(X+4,.72f),Quaternion.identity));enemy.GetComponent<Enemies.EnemyCombat>().enabled=false;enemy.GetComponent<Enemies.LarperBrain>().enabled=false;yield return null;
                var hp=enemy.GetComponent<Health>();float before=hp.Current;var hitbox=enemy.GetComponent<Enemies.SamuraiBodyHitbox>();Vector2 center=hitbox!=null?hitbox.VisibleBounds.center:enemy.GetComponent<Collider2D>().bounds.center;
                Vector2 aim=Vector2.right;for(int n=0;n<3;n++){aim=(center-attack.Origin).normalized;player.GetComponent<CharacterVisualDriver>().PrepareShot(aim);}Physics2D.SyncTransforms();Assert.IsTrue(attack.Attack(w,aim,Time.time+10));Assert.Less(hp.Current,before,definition.name+" -> "+path+" / "+attack.LastSurface);Object.Destroy(enemy);foreach(var corpse in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))corpse.Retire();yield return null;
            }
        }
        [UnityTest] public IEnumerator SustainedLmgBloomBuildsAndRecoversWithoutMovementPenalty()
        {
            var w=Equip(5);yield return null;float speed=player.GetComponent<Prototype.PlayerMotor2D>().MoveSpeed,now=Time.time+10;
            for(int i=0;i<20;i++){Assert.IsTrue(attack.Attack(w,Vector2.right,now));now+=w.Definition.FiringInterval+.001f;}
            Assert.Greater(attack.SustainedSpread,5);Assert.IsTrue(attack.Attack(w,Vector2.right,now+20));Assert.LessOrEqual(attack.SustainedSpread,w.Definition.BloomPerShot+.01f);
            Assert.AreEqual(speed,player.GetComponent<Prototype.PlayerMotor2D>().MoveSpeed);
        }
        [UnityTest] public IEnumerator EveryGunFiresWhileMovingAirborneAndCrouchingAndReloadSwitchConservesAmmo()
        {
            var motor=player.GetComponent<Prototype.PlayerMotor2D>();float speed=motor.MoveSpeed;
            for(int i=0;i<catalog.weapons.Length;i++)
            {
                var w=Equip(i);yield return null;
                float deadline=Time.time+3;while(!motor.IsGrounded&&Time.time<deadline)yield return null;Assert.IsTrue(motor.IsGrounded);
                float x=player.transform.position.x;input.SetTouchMove(Vector2.right);input.RequestTouchJump();yield return Hold(Vector2.right);
                if(w.Definition.FireMode==WeaponFireMode.ReleaseToFire)Release(true);
                yield return new WaitForSeconds(.1f);Assert.IsFalse(motor.IsGrounded);Assert.Less(w.MagazineAmmo,w.Definition.MagazineCapacity,w.Definition.name);Assert.Greater(player.transform.position.x,x);Assert.AreEqual(speed,motor.MoveSpeed);
                input.SuspendInput();deadline=Time.time+3;while(!motor.IsGrounded&&Time.time<deadline)yield return null;
                input.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.2f);Assert.IsTrue(player.GetComponent<Traversal.PlayerPosture2D>().IsCrouched);
                yield return new WaitForSeconds(Mathf.Max(.01f,w.Cycle.NextAttackAt-Time.time+.02f));int before=attack.ShotsFired;yield return Hold(Vector2.right);if(w.Definition.FireMode==WeaponFireMode.ReleaseToFire)Release(true);yield return new WaitForSeconds(.1f);Assert.Greater(attack.ShotsFired,before,"Crouched "+w.Definition.name);
                input.SuspendInput();int total=w.MagazineAmmo+w.ReserveAmmo;combat.Reload();Assert.IsTrue(w.Cycle.IsReloading);loadout.Select(EquipmentSlot.Firearm2);yield return null;Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(total,w.MagazineAmmo+w.ReserveAmmo);loadout.Select(EquipmentSlot.Firearm1);combat.Reload();w.Cycle.Tick(w.Cycle.ReloadStartedAt+w.Definition.ReloadDuration*w.ReloadMultiplier+.01f);Assert.AreEqual(w.Definition.MagazineCapacity,w.MagazineAmmo);Assert.AreEqual(total,w.MagazineAmmo+w.ReserveAmmo);
            }
        }
    }
}
#endif
