#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class GrenadeResupplyHotfixTests
    {
        GameSession session;string path;bool background;float volume;WarehouseMission mission;
        [UnitySetUp]public IEnumerator Setup(){Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;path=Path.Combine(Path.GetTempPath(),"GrenadeHotfix-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("GrenadeHotfixSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),path);session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.PrepareMission();yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;mission=Object.FindAnyObjectByType<WarehouseMission>();mission.Player.GetComponent<Health>().ProtectFor(120);foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;e.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}}
        [UnityTearDown]public IEnumerator Cleanup(){yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Object.Destroy(session.gameObject);yield return null;foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);Application.runInBackground=background;AudioListener.volume=volume;}
        [UnityTest]public IEnumerator ExistingBoxRestocksEveryTypeWithPartialAcceptanceFiniteStockImmediateHudAndSave()
        {
            var crates=Object.FindObjectsByType<SupplyCrate2D>().OrderBy(c=>c.transform.position.x).ToArray();Assert.AreEqual(5,crates.Length);foreach(var c in crates)foreach(ThrowableKind kind in Enum.GetValues(typeof(ThrowableKind)))Assert.AreEqual(2,c.GrenadesLeft(kind));
            var player=mission.Player;var body=player.GetComponent<Rigidbody2D>();body.position=new Vector2(crates[0].transform.position.x-.8f,.72f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return new WaitForSeconds(.1f);var t=player.GetComponent<PlayerThrowables>();t.RestoreStock(new[]{4,5,0});var loadout=player.GetComponent<PlayerLoadout>();var gun=loadout.Get(EquipmentSlot.Firearm1);gun.SetAmmunition(5,0);loadout.Select(EquipmentSlot.Melee);yield return new WaitForSeconds(.25f);
            var controls=Object.FindAnyObjectByType<WarehouseTouchControls>();typeof(WarehouseTouchControls).GetField("nextReadout",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(controls,float.MaxValue);int originalAmmo=crates[0].AmmoLeft;Assert.IsTrue(crates[0].TrySupply(player.GetComponent<PlayerSupplies>()));Assert.AreEqual(5,t.Count(ThrowableKind.Fragmentation));Assert.AreEqual(5,t.Count(ThrowableKind.Flashbang));Assert.AreEqual(2,t.Count(ThrowableKind.Smoke));Assert.AreEqual(Mathf.Min(originalAmmo,loadout.ReserveCapacityFor(gun.Definition)),gun.ReserveAmmo);Assert.AreEqual(originalAmmo-gun.ReserveAmmo,crates[0].AmmoLeft);Assert.AreEqual(5,gun.MagazineAmmo);Assert.AreEqual(1,crates[0].GrenadesLeft(ThrowableKind.Fragmentation));Assert.AreEqual(2,crates[0].GrenadesLeft(ThrowableKind.Flashbang));yield return null;yield return new WaitForEndOfFrame();Assert.AreEqual("5/5",controls.GetComponent<UIDocument>().rootVisualElement.Q<Label>("ammoValue").text);
            var saved=new ProgressStore(path).Load(()=>new GameProgress());CollectionAssert.AreEqual(new[]{5,5,2},session.Data.grenades);CollectionAssert.AreEqual(new[]{0,0,0},saved.grenades,"Carried stock remains reserved on disk until mission settlement");
            t.RestoreStock(new[]{0,0,0});Assert.IsTrue(crates[0].TrySupply(player.GetComponent<PlayerSupplies>()));CollectionAssert.AreEqual(new[]{1,2,0},new[]{t.Count(ThrowableKind.Fragmentation),t.Count(ThrowableKind.Flashbang),t.Count(ThrowableKind.Smoke)});crates[0].gameObject.SetActive(false);crates[0].gameObject.SetActive(true);Assert.IsFalse(crates[0].TrySupply(player.GetComponent<PlayerSupplies>()));Assert.AreEqual(0,crates[0].GrenadesLeft(ThrowableKind.Fragmentation));Assert.AreEqual(0,crates[0].GrenadesLeft(ThrowableKind.Flashbang));Assert.AreEqual(0,crates[0].GrenadesLeft(ThrowableKind.Smoke));Assert.AreEqual(4,t.TakeSupply(new[]{int.MaxValue,0,0}));Assert.AreEqual(5,t.Count(ThrowableKind.Fragmentation));Assert.AreEqual(0,t.TakeSupply(new[]{1,0,0}));
        }
        ThrowableProjectile2D Projectile()=>Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Sprint0027/Throwable.prefab")).GetComponent<ThrowableProjectile2D>();
        Health Dummy(float x){var g=new GameObject("FragTestTarget");g.layer=2;g.transform.position=new Vector2(x,.8f);g.AddComponent<BoxCollider2D>().size=new Vector2(.1f,1.4f);var hp=g.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("maximum").floatValue=500;s.FindProperty("team").enumValueIndex=(int)CombatTeam.Player;s.ApplyModifiedPropertiesWithoutUndo();hp.Restore();return hp;}
        static void Capture(Vector2 center,string name)
        {
            var camera=new GameObject("ControlledFragCapture").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4;camera.aspect=16f/9;camera.transform.position=new Vector3(center.x,center.y+1,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.16f,.18f,.2f);var old=RenderTexture.active;var rt=new RenderTexture(960,540,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(960,540,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,960,540),0,0);tex.Apply();File.WriteAllBytes("TestResults/GrenadeResupplyHotfix/"+name+".png",tex.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;rt.Release();Object.Destroy(rt);Object.Destroy(tex);Object.Destroy(camera.gameObject);
        }
        [UnityTest]public IEnumerator FragUsesLargerRealFalloffFriendlyCoverAndPooledChunkyEffectsWithoutChangingOtherTypes()
        {
            var owner=Dummy(60000);var near=Dummy(60001);var far=Dummy(60003.5f);var outside=Dummy(60004.3f);var shielded=Dummy(59998);var wall=new GameObject("FragShield");wall.transform.position=new Vector2(59999,1);wall.AddComponent<BoxCollider2D>().size=new Vector2(.15f,4);Physics2D.SyncTransforms();var p=Projectile();int count=p.GetComponentsInChildren<SpriteRenderer>(true).Length;var s=new SerializedObject(p);Assert.AreEqual(4.1f,s.FindProperty("blastRadius").floatValue);Assert.AreEqual(110.5f,s.FindProperty("fragmentationDamage").floatValue);Assert.AreEqual(6,s.FindProperty("flashRadius").floatValue);Assert.AreEqual(1.6f,s.FindProperty("flashSeconds").floatValue);Assert.AreEqual(3.5f,s.FindProperty("smokeRadius").floatValue);Assert.AreEqual(8,s.FindProperty("smokeSeconds").floatValue);
            p.Launch(owner.gameObject,ThrowableKind.Fragmentation,new Vector2(60000,.8f),Vector2.zero,.01f);yield return new WaitForSeconds(.06f);Assert.AreEqual(1,p.Detonations);Assert.That(500-owner.Current,Is.InRange(108,111));Assert.Less(near.Current,far.Current);Assert.Less(far.Current,500);Assert.AreEqual(500,outside.Current);Assert.AreEqual(500,shielded.Current);Assert.IsTrue(p.GetComponent<AudioSource>().isPlaying);Assert.IsTrue(p.transform.Find("FragFire0").GetComponent<SpriteRenderer>().enabled);Capture(p.transform.position,"FragFireball");yield return new WaitForSeconds(.2f);Assert.IsTrue(p.transform.Find("FragSmoke0").GetComponent<SpriteRenderer>().enabled);Capture(p.transform.position,"FragSmokeDebris");yield return new WaitForSeconds(.8f);Assert.IsFalse(p.Busy);
            for(int i=0;i<2;i++){p.Launch(null,i==0?ThrowableKind.Flashbang:ThrowableKind.Fragmentation,new Vector2(60010,.8f),Vector2.zero,.01f);yield return new WaitForSeconds(.12f);Assert.AreEqual(count,p.GetComponentsInChildren<SpriteRenderer>(true).Length);if(i==0){Assert.IsFalse(p.transform.Find("FragFire0").GetComponent<SpriteRenderer>().enabled);Assert.IsFalse(p.GetComponent<AudioSource>().isPlaying);}yield return new WaitForSeconds(1);}
        }
        [UnityTest]public IEnumerator OrdinaryLocalizedDeathKeepsArmsUntilOneRegionAccumulates105AndLegThresholdStays60()
        {
            var actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(64000,.72f),Quaternion.identity);actor.GetComponent<EnemyCombat>().enabled=false;actor.GetComponent<LarperBrain>().enabled=false;actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;yield return null;var regions=actor.GetComponent<BodyRegionDamage>();Assert.AreEqual(105,regions.Threshold(2));Assert.AreEqual(105,regions.Threshold(3));Assert.AreEqual(60,regions.Threshold(4));Assert.AreEqual(80,regions.Threshold(1));var rig=actor.GetComponent<CharacterVisualDriver>().Rig;var hp=actor.GetComponent<Health>();
            for(int i=0;i<2;i++)hp.ApplyDamage(new DamageInfo(25,CombatTeam.Player,null,new PhysicalImpact(rig.rightArm.GetComponent<SpriteRenderer>().bounds.center,Vector2.right,.1f)));Assert.IsTrue(hp.IsDefeated);var corpse=actor.GetComponent<RagdollOnDefeat>().LastRagdoll;Assert.NotNull(corpse);Assert.AreEqual(50,corpse.RegionDamage(3));Assert.IsFalse(corpse.IsRegionSeparated(2));Assert.IsFalse(corpse.IsRegionSeparated(3));var body=corpse.Bodies[3];var hit=new PhysicalImpact(body.position,Vector2.right,.2f,body);corpse.ReceiveRegionDamage(54,hit);Assert.IsFalse(corpse.IsRegionSeparated(3));corpse.ReceiveRegionDamage(1,hit);Assert.IsTrue(corpse.IsRegionSeparated(3));Assert.IsFalse(corpse.IsRegionSeparated(2));body=corpse.Bodies[4];corpse.ReceiveRegionDamage(60,new PhysicalImpact(body.position,Vector2.down,.2f,body));Assert.IsTrue(corpse.IsRegionSeparated(4));
            corpse.ReceiveImpact(new PhysicalImpact(body.position,Vector2.right,1000,body));for(int i=0;i<12;i++){yield return new WaitForFixedUpdate();foreach(var part in corpse.Bodies)Assert.LessOrEqual(part.linearVelocity.magnitude,12.5f);}
        }
        [UnityTest]public IEnumerator FragShakeIsDistanceBoundedExpiresAndActualEnemyDeathStillCreatesPhysicsCorpse()
        {
            var follow=Camera.main.GetComponent<InsaneDreams.CameraSystem.WarehouseCameraFollow>();Vector2 origin=mission.Player.transform.position;var p=Projectile();p.Launch(mission.Player.gameObject,ThrowableKind.Fragmentation,origin+Vector2.right*2,Vector2.zero,.01f);yield return new WaitForSeconds(.05f);Assert.Greater(follow.ShakeMagnitude,0);Assert.LessOrEqual(follow.ShakeMagnitude,.08f);yield return new WaitForSeconds(.25f);Assert.AreEqual(0,follow.ShakeMagnitude);
            var actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(62001,.72f),Quaternion.identity);actor.GetComponent<EnemyCombat>().enabled=false;actor.GetComponent<LarperBrain>().enabled=false;actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;p.Launch(mission.Player.gameObject,ThrowableKind.Fragmentation,new Vector2(62000,.72f),Vector2.zero,.01f);yield return new WaitForSeconds(.08f);Assert.AreEqual(0,follow.ShakeMagnitude);Assert.IsTrue(actor.GetComponent<Health>().IsDefeated);Assert.NotNull(actor.GetComponent<RagdollOnDefeat>().LastRagdoll);Assert.AreEqual(1,actor.GetComponent<RagdollOnDefeat>().SpawnCount);
        }
    }
}
#endif
