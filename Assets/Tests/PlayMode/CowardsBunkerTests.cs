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
    public sealed class CowardsBunkerTests
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
        readonly RaycastHit2D[] solidHits=new RaycastHit2D[8];
        RaycastHit2D SolidRay(Vector2 from,Vector2 direction,float distance){var f=new ContactFilter2D{useTriggers=false};f.SetLayerMask(1);return Physics2D.Raycast(from,direction.normalized,f,solidHits,distance)>0?solidHits[0]:default;}
        IEnumerator CrossEnlargedBunker()
        {
            var player=mission.Player;var box=player.GetComponent<BoxCollider2D>();var ladder=Object.FindObjectsByType<LadderVolume>().Single(l=>l.name=="PillboxRoofAccessLadder");float deadline=Time.time+14;
            while(player.transform.position.x<ladder.CenterX-.1f&&Time.time<deadline){player.SetTouchMove(Vector2.right);yield return new WaitForFixedUpdate();}
            player.SetTouchMove(Vector2.up);
            while((box.bounds.min.y<7.8f||player.GetComponent<PlayerTraversal2D>().IsClimbing)&&Time.time<deadline)yield return new WaitForFixedUpdate();
            Assert.Greater(box.bounds.min.y,7.7f,"Existing ladder must exit onto the doubled-height roof");
            while(player.transform.position.x<150&&Time.time<deadline){player.SetTouchMove(Vector2.right);yield return new WaitForFixedUpdate();}player.SetTouchMove(Vector2.zero);Assert.GreaterOrEqual(player.transform.position.x,150);
        }
        void Capture(string name,float x,float y,float zoom=6)
        {
            var c=Camera.main;var at=c.transform.position;float size=c.orthographicSize;c.transform.position=new Vector3(x,y,-10);c.orthographicSize=zoom;var old=RenderTexture.active;var previous=c.targetTexture;var rt=new RenderTexture(960,540,24){filterMode=FilterMode.Point};c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(960,540,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,960,540),0,0);t.Apply();File.WriteAllBytes("TestResults/ConcreteBunker/"+name+".png",t.EncodeToPNG());c.targetTexture=previous;RenderTexture.active=old;Object.Destroy(t);Object.Destroy(rt);c.transform.position=at;c.orthographicSize=size;
        }
        [UnityTest]public IEnumerator ConcreteShellBlocksEveryExteriorBoundaryExceptTheEmbrasureAndOfficerCanBeShot()
        {
            for(float y=1.65f;y<7.8f;y+=.1f){var hit=SolidRay(new Vector2(139,y),Vector2.right,1.5f);bool open=y>2.5f&&y<3.9f;Assert.AreEqual(!open,hit.collider!=null,"Front y="+y);if(hit.collider!=null)Assert.AreEqual(ImpactMaterial.Concrete,ImpactSurface2D.Classify(hit.collider));}
            Assert.IsNull(SolidRay(new Vector2(150,3),Vector2.left,2).collider,"Rear door is a real opening");Assert.NotNull(SolidRay(new Vector2(150,4),Vector2.left,2).collider);Assert.NotNull(Physics2D.Raycast(new Vector2(144.5f,9),Vector2.down,2,1).collider);
            Move(officer,141.05f,1.6f);Move(mission.Player.gameObject,137,1.6f);officer.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;mission.Player.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;yield return new WaitForSeconds(.2f);
            var hp=officer.GetComponent<Health>();var attack=mission.Player.GetComponent<AttackExecutor>();var gun=new InsaneDreams.Inventory.RuntimeWeapon(AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"));float start=hp.Current;
            var enemyAttack=officer.GetComponent<AttackExecutor>();var enemyGun=new InsaneDreams.Inventory.RuntimeWeapon(combat.Weapon.Definition);Assert.IsTrue(enemyAttack.Attack(enemyGun,new Vector2(-1,-.5f).normalized,Time.time));Assert.AreEqual(ImpactMaterial.Concrete,enemyAttack.LastImpactMaterial);Assert.Less(enemyAttack.LastShotEnd.x,140.5f);
            for(int i=0;i<4&&!hp.IsDefeated;i++){yield return new WaitForSeconds(.21f);Assert.IsTrue(attack.Attack(gun,((Vector2)officer.transform.position+Vector2.up*.4f-attack.Origin).normalized,Time.time));}
            Assert.Less(hp.Current,start,"Ordinary player fire can hit exposed Officer through hole");Assert.IsTrue(hp.IsDefeated);Assert.NotNull(officer.GetComponent<InsaneDreams.Visuals.RagdollOnDefeat>().LastRagdoll);Assert.AreEqual(MissionStage.Escape,mission.Stage);
            Capture("ConcretePillbox",144.5f,4.7f,5.5f);
            var player=mission.Player;player.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeRotation;yield return CrossEnlargedBunker();float deadline=Time.time+12;while(!mission.AttemptEnded&&Time.time<deadline){player.SetTouchMove(Vector2.right);yield return new WaitForFixedUpdate();}player.SetTouchMove(Vector2.zero);Assert.AreEqual(MissionStage.Complete,mission.Stage,"Actual carbine defeat followed by physical ladder/roof traversal must reach Results");
        }
        [UnityTest]public IEnumerator ConfirmedRearFlankDrawsOfficerThroughRealDoorWithoutTeleporting()
        {
            Move(officer,146.5f,1.6f);Move(mission.Player.gameObject,153,1.6f);
            mission.Player.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            Camera.main.transform.position=new Vector3(147,3.5f,-10);
            combat.enabled=true;brain.enabled=true;combat.SetEncounterEnabled(true);combat.BindTarget(mission.Player.GetComponent<Health>());
            float deadline=Time.time+8,old=officer.transform.position.x;bool confirmed=false;
            while(officer.transform.position.x<149.5f&&Time.time<deadline)
            {
                yield return new WaitForFixedUpdate();confirmed|=combat.TargetVisible;
                Assert.Less(Mathf.Abs(officer.transform.position.x-old),.7f);old=officer.transform.position.x;
            }
            Assert.IsTrue(confirmed);Assert.GreaterOrEqual(officer.transform.position.x,149.5f,"Must walk through rear opening");
            Assert.IsTrue(warning.AllowsBunkerRay(officer.GetComponent<AttackExecutor>().Origin,Vector2.right));
        }
        [UnityTest]public IEnumerator OfficerPeeksTelegraphsFiresHidesAndReturnsInsideConcreteWithoutDeadlock()
        {
            Move(mission.Player.gameObject,137,1.6f);mission.Player.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;yield return new WaitForSeconds(.2f);combat.enabled=true;brain.enabled=true;combat.SetEncounterEnabled(true);combat.BindTarget(mission.Player.GetComponent<Health>());
            var attack=officer.GetComponent<AttackExecutor>();bool laser=false,hidden=false;float oldX=officer.transform.position.x;int hiddenFrames=0;int checkedShots=0;var rows=new List<string>();
            for(int i=0;i<1100;i++)
            {yield return new WaitForFixedUpdate();float x=officer.transform.position.x;Assert.Less(Mathf.Abs(x-oldX),.7f,"No teleport");oldX=x;Assert.That(x,Is.InRange(140.8f,148.2f),"Stay inside enlarged shell");if(warning.LaserVisible){Assert.IsTrue(warning.AllowsBunkerRay(attack.Origin,warning.Direction));if(!laser){Capture("OfficerAtEmbrasure",144.5f,4.7f,5.5f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("TestResults/Sprint0033/OfficerExposedHUD.png");}laser=true;}
                if(attack.ShotsFired>checkedShots){Assert.GreaterOrEqual(Time.time-warning.WarningStartedAt,.49f);Assert.IsTrue(warning.AllowsBunkerRay(attack.LastShotOrigin,attack.LastShotDirection));checkedShots=attack.ShotsFired;}
                if(checkedShots>0&&combat.IsHoldingCover&&officer.GetComponent<PlayerPosture2D>().IsCrouched){if(++hiddenFrames>10){if(!hidden){Capture("OfficerHidden",144.5f,4.7f,5.5f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("TestResults/Sprint0033/OfficerHiddenHUD.png");}hidden=true;}var b=officer.GetComponent<BoxCollider2D>().bounds;Assert.NotNull(Physics2D.Linecast(new Vector2(137,2.3f),new Vector2(b.center.x,b.max.y-.05f),1).collider,"Concrete protects crouched head");}
                if(i%10==0)rows.Add(Time.time+" "+officer.transform.position+" "+combat.State+" visible="+combat.TargetVisible+" shots="+attack.ShotsFired+" muzzle="+attack.Origin+" aim="+combat.AimDirection);
                if(attack.ShotsFired>=2&&hidden)break;
            }
            File.WriteAllLines("TestResults/ConcreteBunker/OfficerTrace.txt",rows);Assert.IsTrue(laser);Assert.GreaterOrEqual(attack.ShotsFired,2);Assert.IsTrue(hidden,"Must physically crouch deeper inside between shots");
            Move(mission.Player.gameObject,130,0);int before=attack.ShotsFired;float until=Time.time+12;while(attack.ShotsFired<=before&&Time.time<until)yield return new WaitForFixedUpdate();Assert.Greater(attack.ShotsFired,before,"Can engage the actual ground-level approach through the slit");
            combat.Weapon.SetAmmunition(0,7);until=Time.time+12;bool reload=false;while(Time.time<until){reload|=combat.Weapon.Cycle.IsReloading;if(reload&&combat.Weapon.MagazineAmmo>0)break;yield return new WaitForFixedUpdate();}Assert.IsTrue(reload);Assert.Greater(combat.Weapon.MagazineAmmo,0);Assert.AreEqual(0,combat.Weapon.ReserveAmmo);
        }
        [UnityTest]public IEnumerator LaserClipsToConcreteAndOnlyPassesThroughFiringHole()
        {
            Move(officer,141.05f,1.6f);officer.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;combat.enabled=true;combat.BindTarget(null);typeof(EnemyCombat).GetProperty("AimDirection").SetValue(combat,Vector2.left);yield return new WaitForSeconds(.2f);
            var wall=new GameObject("LaserTestConcrete");wall.transform.position=new Vector2(137,3);wall.AddComponent<BoxCollider2D>().size=new Vector2(.3f,3);Physics2D.SyncTransforms();
            for(int i=0;i<4;i++){warning.AllowShot(Vector2.left,0,20,out _);yield return new WaitForEndOfFrame();}Assert.IsTrue(warning.LaserVisible);Assert.AreEqual(137.15f,warning.LaserEnd.x,.03f);Assert.IsFalse(warning.AllowsBunkerRay(new Vector2(142,2.2f),Vector2.left));Assert.IsFalse(warning.AllowsBunkerRay(new Vector2(142,4.1f),Vector2.left));Assert.IsFalse(warning.AllowsBunkerRay(new Vector2(142,3),Vector2.right));Object.Destroy(wall);
        }
        [UnityTest]public IEnumerator PhysicalGrenadeRoofCollisionAndRealPlayerRoofAccessRouteStayValid()
        {
            var p=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Sprint0027/Throwable.prefab")).GetComponent<ThrowableProjectile2D>();p.Launch(mission.Player.gameObject,ThrowableKind.Fragmentation,new Vector2(144.5f,9),Vector2.zero,3);yield return new WaitForSeconds(.7f);Assert.Greater(p.GetComponent<Rigidbody2D>().position.y,7.82f);float hp=officer.GetComponent<Health>().Current;p.Launch(mission.Player.gameObject,ThrowableKind.Fragmentation,new Vector2(139,2),Vector2.zero,.01f);yield return new WaitForSeconds(.1f);Assert.AreEqual(hp,officer.GetComponent<Health>().Current,"Closed concrete shields external frag blast");Object.Destroy(p.gameObject);
            Move(mission.Player.gameObject,137,1.6f);yield return CrossEnlargedBunker();
        }
    }
}
#endif

