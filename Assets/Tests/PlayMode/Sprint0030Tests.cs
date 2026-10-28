#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0030Tests
    {
        [Serializable]class Sample{public float x,y,time;public string state,cover;public bool visible,crouched;public int shots;}
        [Serializable]class Trace{public float minX,maxX;public bool laser,retreated,crouched;public int ammo,shots;public List<Sample> samples;}
        GameSession session;string save;WarehouseMission mission;GameObject officer;EnemyCombat combat;LarperBrain brain;OfficerShotTelegraph warning;bool background;float volume;UnityEngine.Random.State random;
        [UnitySetUp]public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;random=UnityEngine.Random.state;UnityEngine.Random.InitState(2808);
            save=Path.Combine(Path.GetTempPath(),"AkumuBunker-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("BunkerTestSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.PrepareMission();yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            mission=Object.FindAnyObjectByType<WarehouseMission>();mission.Player.GetComponent<Health>().ProtectFor(120);foreach(var p in mission.Encounters.SelectMany(e=>e.Points)){p.Spawned.GetComponent<EnemyCombat>().enabled=false;p.Spawned.enabled=false;p.Spawned.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}
            officer=mission.Encounters[4].Points[0].Spawned.gameObject;combat=officer.GetComponent<EnemyCombat>();brain=officer.GetComponent<LarperBrain>();warning=officer.GetComponent<OfficerShotTelegraph>();Camera.main.GetComponent<InsaneDreams.CameraSystem.WarehouseCameraFollow>().enabled=false;Camera.main.transform.position=new Vector3(140,3.5f,-10);Camera.main.orthographicSize=6;
        }
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Object.Destroy(session.gameObject);yield return null;foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);Application.runInBackground=background;AudioListener.volume=volume;UnityEngine.Random.state=random;}
        void Move(GameObject go,float x,float feet){var b=go.GetComponent<BoxCollider2D>();var body=go.GetComponent<Rigidbody2D>();body.position=new Vector2(x,feet+b.size.y*.5f-b.offset.y);go.transform.position=body.position;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
        void Capture(string name,float x,float y,float zoom=6)
        {
            var c=Camera.main;var at=c.transform.position;float size=c.orthographicSize;c.transform.position=new Vector3(x,y,-10);c.orthographicSize=zoom;var old=RenderTexture.active;var previous=c.targetTexture;var rt=new RenderTexture(960,540,24){filterMode=FilterMode.Point};c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(960,540,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,960,540),0,0);t.Apply();File.WriteAllBytes("TestResults/Sprint0030/"+name+".png",t.EncodeToPNG());c.targetTexture=previous;RenderTexture.active=old;Object.Destroy(t);Object.Destroy(rt);c.transform.position=at;c.orthographicSize=size;
        }
        static System.Reflection.FieldInfo Field(object obj,string name)=>obj.GetType().GetField(name,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        [UnityTest]public IEnumerator OfficerVestDoublesBodyShotDurabilityWithoutExtraHealthAndHeadAndMeleeRemainWeaknesses()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/FirstOperation/Officer.prefab");
            foreach(bool melee in new[]{false,true})
            {var a=Object.Instantiate(prefab,new Vector2(23000,3),Quaternion.identity);a.GetComponent<EnemyCombat>().enabled=false;a.GetComponent<LarperBrain>().enabled=false;a.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;var hp=a.GetComponent<Health>();var vest=a.GetComponent<BodyArmor>();Assert.AreEqual(50,hp.Maximum);Assert.AreEqual(50,vest.Condition);int hits=0;var point=a.GetComponent<BoxCollider2D>().bounds.center;
                while(!hp.IsDefeated&&hits<20){Assert.IsTrue(hp.ApplyDamage(new DamageInfo(10,CombatTeam.Player,null,new PhysicalImpact(point,Vector2.right,0),false,melee?DamageKind.Melee:DamageKind.Ballistic)));hits++;}Assert.AreEqual(melee?5:10,hits);Assert.AreEqual(melee?50:0,vest.Condition);hp.Restore();a.GetComponent<EnemyCombat>().ResetEncounter(null);Assert.AreEqual(50,vest.Condition);Object.Destroy(a);yield return null;}
            var head=Object.Instantiate(prefab,new Vector2(23004,3),Quaternion.identity);head.GetComponent<EnemyCombat>().enabled=false;head.GetComponent<LarperBrain>().enabled=false;var h=head.GetComponent<Health>();var b=head.GetComponent<BoxCollider2D>();var p=new Vector2(b.bounds.center.x,b.bounds.max.y-.02f);h.ApplyDamage(new DamageInfo(10,CombatTeam.Player,null,new PhysicalImpact(p,Vector2.right,0)));Assert.AreEqual(40,h.Current);Assert.AreEqual(50,head.GetComponent<BodyArmor>().Condition);Object.Destroy(head);yield return null;
        }
        [UnityTest]public IEnumerator DirectHitCancelsWarningTriggersDebouncedPhysicalRetreatAndCombatResumes()
        {
            Move(mission.Player.gameObject,137,1.6f);mission.Player.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;combat.enabled=true;brain.enabled=true;combat.SetEncounterEnabled(true);combat.BindTarget(mission.Player.GetComponent<Health>());float end=Time.time+5;while(!warning.Winding&&Time.time<end)yield return null;Assert.IsTrue(warning.Winding);Capture("ArmoredOfficer",142,3.5f,4);
            var hp=officer.GetComponent<Health>();hp.ApplyDamage(new DamageInfo(10,CombatTeam.Player,mission.Player.gameObject,new PhysicalImpact(officer.GetComponent<BoxCollider2D>().bounds.center,Vector2.right,0)));Assert.IsFalse(warning.Winding);float cooldown=(float)Field(warning,"nextDamageRetreat").GetValue(warning);yield return null;yield return null;Assert.AreEqual(EngagementState.Withdrawing,combat.State);
            hp.ApplyDamage(new DamageInfo(1,CombatTeam.Player,mission.Player.gameObject));Assert.AreEqual(cooldown,(float)Field(warning,"nextDamageRetreat").GetValue(warning));bool hidden=false;end=Time.time+16;while(Time.time<end){hidden|=combat.IsHoldingCover;if(officer.GetComponent<AttackExecutor>().ShotsFired>0&&hidden)break;yield return new WaitForFixedUpdate();}Assert.IsTrue(hidden);Assert.Greater(officer.GetComponent<AttackExecutor>().ShotsFired,0);Assert.That(officer.transform.position.x,Is.InRange(140.8f,148.2f));
        }
        [UnityTest]public IEnumerator ConfirmedClosePlayerShortensHidingWithoutOmniscientOrPermanentRetreat()
        {
            Move(officer,143.25f,1.6f);Move(mission.Player.gameObject,138.5f,1.6f);mission.Player.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;combat.enabled=true;brain.enabled=true;combat.SetEncounterEnabled(true);combat.BindTarget(mission.Player.GetComponent<Health>());yield return new WaitForSeconds(.2f);Assert.IsTrue(combat.TargetVisible);Move(officer,143.25f,1.6f);Field(combat,"selectedCover").SetValue(combat,warning.BunkerCover);Field(combat,"coverHoldUntil").SetValue(combat,Time.time+2.4f);yield return new WaitForSeconds(.1f);float remaining=(float)Field(combat,"coverHoldUntil").GetValue(combat)-Time.time;Assert.LessOrEqual(remaining,.46f);Assert.Greater(remaining,0);
            Move(mission.Player.gameObject,130,0);combat.BindTarget(mission.Player.GetComponent<Health>());Field(combat,"selectedCover").SetValue(combat,warning.BunkerCover);Field(combat,"coverHoldUntil").SetValue(combat,Time.time+2.4f);yield return new WaitForSeconds(.1f);Assert.Greater((float)Field(combat,"coverHoldUntil").GetValue(combat)-Time.time,1.8f,"Unconfirmed distant target does not force exposure");
        }
        [UnityTest]public IEnumerator PlayerVestAndHelmetDisappearImmediatelyWhenBrokenAndRemovingGearDoesNotStackPenalties()
        {
            session.Data.bodyArmorTier=2;session.Data.armor=90;session.Data.helmetTier=2;session.Data.helmet=35;var bridge=mission.GetComponent<CampaignMissionBridge>();bridge.ApplyEquipment();var armor=mission.Player.GetComponent<BodyArmor>();var visual=mission.Player.GetComponent<InsaneDreams.Visuals.ArmorVisual>();Assert.IsTrue(visual.overlay.enabled);Assert.IsTrue(visual.helmetOverlay.enabled);armor.Absorb(new DamageInfo(1000,CombatTeam.Enemy));Assert.IsFalse(visual.overlay.enabled);Assert.IsNull(visual.overlay.sprite);var box=mission.Player.GetComponent<BoxCollider2D>();armor.Absorb(new DamageInfo(1000,CombatTeam.Enemy,null,new PhysicalImpact(new Vector2(box.bounds.center.x,box.bounds.max.y-.01f),Vector2.left,0)));Assert.IsFalse(visual.helmetOverlay.enabled);Assert.IsNull(visual.helmetOverlay.sprite);session.Data.bodyArmorTier=session.Data.helmetTier=0;session.Data.armor=session.Data.helmet=0;bridge.ApplyEquipment();bridge.ApplyEquipment();Assert.AreEqual(432,mission.Player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().MaximumReserveAmmo);Assert.AreEqual(1,mission.Player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon.ReloadMultiplier);Assert.IsFalse(visual.overlay.enabled);Assert.IsFalse(visual.helmetOverlay.enabled);yield return null;
        }
    }
}
#endif
