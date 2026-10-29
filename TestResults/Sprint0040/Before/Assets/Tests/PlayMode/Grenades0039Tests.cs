#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class Grenades0039Tests
    {
        const float X=112000;
        readonly List<GameObject> owned=new List<GameObject>();bool background;float volume;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;AudioListener.volume=volume;}
        Health Dummy(float x,float y=1,CombatTeam team=CombatTeam.Player)
        {
            var g=Keep(new GameObject("HazardVictim"));g.layer=2;g.transform.position=new Vector2(x,y);g.AddComponent<BoxCollider2D>().size=new Vector2(.3f,1.4f);
            var body=g.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.useFullKinematicContacts=true;
            var health=g.AddComponent<Health>();var s=new SerializedObject(health);s.FindProperty("maximum").floatValue=500;s.FindProperty("team").enumValueIndex=(int)team;s.ApplyModifiedPropertiesWithoutUndo();health.Restore();return health;
        }
        Landmine2D Mine(float x)
        {
            return Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Hazards/Landmine.prefab"),new Vector3(x,0),Quaternion.identity)).GetComponent<Landmine2D>();
        }
        [UnityTest]public IEnumerator InventoryBoundariesAndSwitchOrCapacityChangesCancelAimWithoutLoss()
        {
            var player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,1),Quaternion.identity));player.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;
            var input=player.GetComponent<PlayerActions>();var settings=new SerializedObject(input);settings.FindProperty("readKeyboard").boolValue=false;settings.ApplyModifiedPropertiesWithoutUndo();
            var load=player.GetComponent<PlayerLoadout>();var stock=player.GetComponent<PlayerThrowables>();yield return null;
            stock.RestoreStock(new[]{2,1});Assert.AreEqual(2,stock.Count(ThrowableKind.Fragmentation));Assert.AreEqual(1,stock.Count(ThrowableKind.Flashbang));Assert.AreEqual(0,stock.Count(ThrowableKind.Smoke));Assert.AreEqual(0,stock.Count((ThrowableKind)99));
            var box=new[]{int.MaxValue,2};Assert.AreEqual(5,stock.TakeSupply(box));Assert.AreEqual(5,stock.Count(ThrowableKind.Fragmentation));Assert.AreEqual(3,stock.Count(ThrowableKind.Flashbang));Assert.AreEqual(int.MaxValue-3,box[0]);Assert.AreEqual(0,box[1]);
            load.Select(EquipmentSlot.Melee);stock.Select(ThrowableKind.Fragmentation);input.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.12f);Assert.IsTrue(stock.IsAiming);
            stock.ConfigureCapacity(3);Assert.IsFalse(stock.IsAiming);input.ResetActions();yield return null;Assert.AreEqual(0,stock.Throws);Assert.AreEqual(3,stock.Count(ThrowableKind.Fragmentation));
            input.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.12f);load.Select(EquipmentSlot.Firearm1);input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(0,stock.Throws);Assert.AreEqual(3,stock.Count(ThrowableKind.Fragmentation));
            load.Select(EquipmentSlot.Melee);stock.Select(ThrowableKind.Flashbang);input.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.12f);stock.RestoreStock(new[]{3,0,1});input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(0,stock.Throws);Assert.AreEqual(0,stock.Count(ThrowableKind.Flashbang));
            stock.Select(ThrowableKind.Smoke);input.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.12f);input.SetTouchAim(Vector2.zero);yield return new WaitForSeconds(.05f);Assert.AreEqual(1,stock.Throws);Assert.AreEqual(0,stock.Count(ThrowableKind.Smoke));input.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.5f);input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(1,stock.Throws);Assert.AreEqual(0,stock.Count(ThrowableKind.Smoke));
        }
        [UnityTest]public IEnumerator FragmentationIsStrongerWithFalloffAndCoverAndOtherTypesKeepTheirSettings()
        {
            var near=Dummy(X+1);var middle=Dummy(X+2.5f);var far=Dummy(X+4.3f);var behind=Dummy(X-2);
            var wall=Keep(new GameObject("BlastCover"));wall.transform.position=new Vector2(X-1,1);wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,4);
            var duplicate=new GameObject("SecondHitbox");duplicate.layer=2;duplicate.transform.SetParent(near.transform,false);duplicate.AddComponent<BoxCollider2D>().size=new Vector2(.2f,.4f);int hits=0;near.Damaged+=_=>hits++;
            var projectile=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Sprint0027/Throwable.prefab"))).GetComponent<ThrowableProjectile2D>();
            var s=new SerializedObject(projectile);Assert.AreEqual(145,s.FindProperty("fragmentationDamage").floatValue);Assert.AreEqual(4.5f,s.FindProperty("blastRadius").floatValue);Assert.AreEqual(6,s.FindProperty("flashRadius").floatValue);Assert.AreEqual(1.6f,s.FindProperty("flashSeconds").floatValue);Assert.AreEqual(3.5f,s.FindProperty("smokeRadius").floatValue);Assert.AreEqual(8,s.FindProperty("smokeSeconds").floatValue);
            Physics2D.SyncTransforms();projectile.Burst(null,new Vector2(X,1));Assert.Greater(500-near.Current,110);Assert.Greater(500-middle.Current,60);Assert.Greater(far.Current,middle.Current);Assert.Less(far.Current,500);Assert.AreEqual(500,behind.Current);Assert.AreEqual(1,hits);Assert.AreEqual(1,projectile.Detonations);
            Assert.IsFalse(projectile.GetComponent<Rigidbody2D>().simulated);yield return new WaitForSeconds(1.05f);Assert.IsFalse(projectile.Busy);
        }
        [UnityTest]public IEnumerator MinePressureUsesOneDelayedExistingBurstForPlayerAndEnemyWithoutRecursing()
        {
            var mine=Mine(X);var second=Mine(X+1);var player=Dummy(X,1.2f);var enemy=Dummy(X+1,1.2f,CombatTeam.Enemy);Physics2D.SyncTransforms();yield return new WaitForSeconds(.5f);
            Assert.IsFalse(mine.Primed);player.transform.position=new Vector2(X,.6f);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.IsTrue(mine.Primed);Assert.AreEqual(0,mine.Detonations);
            yield return new WaitForSeconds(.26f);Assert.AreEqual(1,mine.Detonations);Assert.Less(player.Current,500);Assert.Less(enemy.Current,500);Assert.IsFalse(mine.Explosion.GetComponent<Rigidbody2D>().simulated);
            // Explosion can arm another mine, but detonation waits its own warning interval.
            yield return new WaitForSeconds(.3f);Assert.AreEqual(1,second.Detonations);Assert.AreEqual(1,mine.Detonations);yield return new WaitForSeconds(1);Assert.IsFalse(mine.gameObject.activeSelf);Assert.IsFalse(second.gameObject.activeSelf);
        }
        [UnityTest]public IEnumerator ShootingMineFromOutsideBlastIsSafeAndNeverInstallsMinesInLevelOne()
        {
            var mine=Mine(X);var shooter=Dummy(X-6,.11f);var executor=shooter.gameObject.AddComponent<AttackExecutor>();var weapon=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset"));
            Physics2D.SyncTransforms();Assert.IsTrue(executor.Attack(weapon,Vector2.right,Time.time));Assert.IsTrue(mine.Primed);Assert.AreEqual(0,mine.Detonations);yield return new WaitForSeconds(.3f);Assert.AreEqual(1,mine.Detonations);Assert.AreEqual(500,shooter.Current);
            var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Demo.unity");try{foreach(var root in scene.GetRootGameObjects())Assert.AreEqual(0,root.GetComponentsInChildren<Landmine2D>(true).Length);}finally{UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
#endif
