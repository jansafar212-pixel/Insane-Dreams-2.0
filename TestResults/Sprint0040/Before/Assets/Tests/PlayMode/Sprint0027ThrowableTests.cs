#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0027ThrowableTests
    {
        readonly List<GameObject> objects=new List<GameObject>();bool background;float volume;
        GameObject Keep(GameObject o){objects.Add(o);return o;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;var floor=Keep(new GameObject("GrenadeFloor"));floor.transform.position=new Vector2(31000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(100,1);yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in objects)if(g!=null)Object.Destroy(g);objects.Clear();yield return null;Application.runInBackground=background;AudioListener.volume=volume;}
        ThrowableProjectile2D Projectile()=>Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Sprint0027/Throwable.prefab"))).GetComponent<ThrowableProjectile2D>();
        Health Dummy(float x,CombatTeam team){var g=Keep(new GameObject("GrenadeVictim"));g.layer=2;g.transform.position=new Vector2(x,.8f);g.AddComponent<BoxCollider2D>().size=new Vector2(.4f,1.4f);var hp=g.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)team;s.ApplyModifiedPropertiesWithoutUndo();return hp;}
        [UnityTest]public IEnumerator PlayerAimsThenReleasesOneFiniteGrenadeAndResetCancels()
        {
            var player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(31000,.85f),Quaternion.identity));yield return new WaitForEndOfFrame();var t=player.GetComponent<PlayerThrowables>();t.RestoreStock(new[]{1,1,1});player.GetComponent<PlayerLoadout>().Select(EquipmentSlot.Melee);var a=player.GetComponent<PlayerActions>();a.SetTouchAim(new Vector2(1,.4f));yield return new WaitForSeconds(.15f);Assert.IsTrue(t.IsAiming,"Initial aim: selected="+t.IsSelected+" intent="+a.HasAimIntent+" enabled="+a.enabled+" stock="+t.Count(t.Selected));Assert.AreEqual(0,t.Throws);Assert.IsNull(player.GetComponent<PlayerLoadout>().SelectedWeapon);a.SetTouchAim(Vector2.zero);yield return new WaitForSeconds(.1f);Assert.AreEqual(1,t.Throws);Assert.AreEqual(0,t.Count(ThrowableKind.Fragmentation));yield return new WaitForSeconds(.5f);t.Select(ThrowableKind.Flashbang);a.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.1f);Assert.IsTrue(t.IsAiming,"Second selected-type aim");a.ResetActions();yield return null;Assert.AreEqual(1,t.Throws);Assert.AreEqual(1,t.Count(ThrowableKind.Flashbang));
        }
        [UnityTest]public IEnumerator FragmentationUsesRadialFriendlySelfDamageDeduplicationAndSolidCover()
        {
            var owner=Dummy(31010,CombatTeam.Player);var enemy=Dummy(31011,CombatTeam.Enemy);var shielded=Dummy(31012.5f,CombatTeam.Enemy);int hits=0;enemy.Damaged+=_=>hits++;var child=new GameObject("DuplicateHitbox");child.layer=2;child.transform.SetParent(enemy.transform,false);child.AddComponent<BoxCollider2D>();var wall=Keep(new GameObject("BlastCover"));wall.transform.position=new Vector2(31011.8f,1);wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,4);Physics2D.SyncTransforms();var p=Projectile();p.Launch(owner.gameObject,ThrowableKind.Fragmentation,new Vector2(31010,.8f),Vector2.zero,.05f);yield return new WaitForSeconds(.15f);Assert.Less(owner.Current,100);Assert.Less(enemy.Current,100);Assert.AreEqual(100,shielded.Current);Assert.AreEqual(1,hits);Assert.AreEqual(1,p.Detonations);
        }
        [UnityTest]public IEnumerator SmokeBlocksSensesNotPhysicalBulletsAndExpiresAndFlashIsTemporary()
        {
            var p=Projectile();var s=new SerializedObject(p);s.FindProperty("smokeSeconds").floatValue=.35f;s.ApplyModifiedPropertiesWithoutUndo();p.Launch(null,ThrowableKind.Smoke,new Vector2(31010,1),Vector2.zero,.02f);yield return new WaitForSeconds(.1f);var buffer=new RaycastHit2D[8];Assert.IsTrue(CombatVisibility2D.IsBlocked(new Vector2(31006,1),new Vector2(31014,1),1,buffer));Assert.IsFalse(CombatVisibility2D.IsBlocked(new Vector2(31006,1),new Vector2(31014,1),1,buffer,null,null,false));Assert.IsNull(Physics2D.Raycast(new Vector2(31006,1),Vector2.right,8,1).collider);yield return new WaitForSeconds(.4f);Assert.IsFalse(CombatVisibility2D.IsBlocked(new Vector2(31006,1),new Vector2(31014,1),1,buffer));
            var enemy=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(31012,.72f),Quaternion.identity));enemy.GetComponent<LarperBrain>().enabled=false;var combat=enemy.GetComponent<EnemyCombat>();combat.BindTarget(null);p=Projectile();s=new SerializedObject(p);s.FindProperty("flashSeconds").floatValue=.35f;s.ApplyModifiedPropertiesWithoutUndo();p.Launch(null,ThrowableKind.Flashbang,new Vector2(31010,1),Vector2.zero,.02f);yield return new WaitForSeconds(.12f);Assert.IsTrue(combat.SensesDisrupted);yield return new WaitForSeconds(.45f);Assert.IsFalse(combat.SensesDisrupted);
        }
        [UnityTest]public IEnumerator GrenadeShopPurchasesAreBoundedAndPersistWithoutChangingMedicalStock()
        {
            Assert.IsNull(GameSession.Instance);string path=Path.Combine(Path.GetTempPath(),"GrenadeShop-"+Guid.NewGuid().ToString("N")+".json");var session=Keep(new GameObject("GrenadeShopSession")).AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),path);session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));session.Data.balance=1000;int medical=session.Data.faintanyl;
            try{for(int i=0;i<5;i++)Assert.IsTrue(session.Buy("frag-grenade",i));Assert.IsFalse(session.CanBuy("frag-grenade"));Assert.AreEqual(5,session.Data.grenades[0]);Assert.AreEqual(1000-5*session.Catalog.Find("frag-grenade").price,session.Data.balance);Assert.AreEqual(medical,session.Data.faintanyl);var loaded=new ProgressStore(path).Load(()=>new GameProgress());Assert.AreEqual(5,loaded.grenades[0]);Assert.AreEqual(0,loaded.grenades[1]);}finally{foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);}yield return null;
        }
    }
}
#endif
