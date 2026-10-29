#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Input;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using InsaneDreams.CameraSystem;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class FirstOperationTests
    {
        GameSession session;string save;bool background;float volume;
        WarehouseMission mission;
        [UnitySetUp] public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;
            save=Path.Combine(Path.GetTempPath(),"Akumu28-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("OperationTestSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);
            session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));Assert.IsTrue(session.PrepareMission());
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            mission=Object.FindAnyObjectByType<WarehouseMission>();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));if(session!=null)Object.Destroy(session.gameObject);yield return null;foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);Application.runInBackground=background;AudioListener.volume=volume;}
        void Move(float x,float floor)
        {var b=mission.Player.GetComponent<Rigidbody2D>();b.position=new Vector2(x,floor+.75f);b.linearVelocity=Vector2.zero;mission.Player.transform.position=b.position;Physics2D.SyncTransforms();Camera.main.GetComponent<WarehouseCameraFollow>().SnapToTarget();}
        void Quiet()
        {foreach(var point in mission.Encounters.SelectMany(g=>g.Points)){point.Spawned.GetComponent<EnemyCombat>().enabled=false;point.Spawned.enabled=false;point.Spawned.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}}
        [UnityTest]public IEnumerator OpeningIsolatedLarperEngagesWithoutThreeEnemyCrossfire()
        {
            mission.Player.GetComponent<Health>().ProtectFor(10);
            mission.Player.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var points=mission.Encounters.SelectMany(g=>g.Points).ToArray();
            var first=mission.Encounters[0].Points[0].Spawned.GetComponent<AttackExecutor>();
            float deadline=Time.time+3;
            while(Time.time<deadline)
            {
                Assert.LessOrEqual(points.Count(p=>p.Spawned.GetComponent<EnemyCombat>().MayInitiateFire&&p.Spawned.GetComponent<EnemyCombat>().TargetVisible),1,"Opening presents one visible shooter");
                foreach(var p in points.Skip(1))Assert.AreEqual(0,p.Spawned.GetComponent<AttackExecutor>().ShotsFired,p.name);
                yield return new WaitForFixedUpdate();
            }
            Assert.Greater(first.ShotsFired,0,"Opening remains responsive");
        }
        [UnityTest] public IEnumerator AuthoredRosterSupportBulletproofPlatformsAndFiveCameraViews()
        {
            Quiet();mission.Player.GetComponent<Health>().ProtectFor(120);Assert.IsTrue(mission.FirstOperation);CollectionAssert.AreEqual(new[]{3,3,3,3,1},mission.Encounters.Select(g=>g.Count).ToArray());
            var points=mission.Encounters.SelectMany(g=>g.Points).ToArray();Assert.AreEqual(13,points.Length);Assert.AreEqual(13,points.Count(p=>p.Spawned.gameObject.activeInHierarchy));Assert.AreEqual(10,points.Count(p=>p.Archetype.name=="Larper"));
            Assert.AreEqual(1,points.Count(p=>p.Spawned.GetComponent<SamuraiCombat>()!=null));Assert.AreEqual(1,points.Count(p=>p.Spawned.GetComponent<FireworksCombat>()!=null));Assert.AreEqual(1,points.Count(p=>p.Spawned.GetComponent<OfficerShotTelegraph>()!=null));Assert.AreEqual(0,Object.FindObjectsByType<InsaneDreams.Checkpoints.CheckpointMarker>().Length);
            foreach(var p in points){var box=p.Spawned.GetComponent<BoxCollider2D>();var hit=Physics2D.Raycast(new Vector2(box.bounds.center.x,box.bounds.min.y+.06f),Vector2.down,.25f,1);Assert.NotNull(hit.collider,p.name+" supported");}
            foreach(var b in Object.FindObjectsByType<BoxCollider2D>().Where(b=>b.gameObject.layer==0&&!b.isTrigger&&b.size.y<=.6f&&b.size.x>=3&&b.bounds.max.y>.1f))
            {Vector2 below=new Vector2(b.bounds.center.x,b.bounds.min.y-.1f);var hit=Physics2D.Raycast(below,Vector2.up,b.bounds.size.y+.2f,1);Assert.NotNull(hit.collider,"Platforms must block real bullet segments from below");Assert.LessOrEqual(hit.point.y,b.bounds.max.y+.001f,"Overlapping solid foundation still stops the ray before it crosses the deck");Assert.AreEqual(0,b.GetComponents<PlatformEffector2D>().Length);}
            Move(21,0);yield return new WaitForFixedUpdate();var gun=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"));var attack=mission.Player.GetComponent<AttackExecutor>();Assert.IsTrue(attack.Attack(gun,Vector2.up,Time.time));Assert.AreEqual(2.7f,attack.LastShotEnd.y,.05f,"Actual carbine shot must stop at deck underside");
            var references=points.Select(p=>p.Spawned).ToArray();foreach(float x in new[]{-24f,10f,49f,90f,134f})
            {Move(x,0);yield return new WaitForSeconds(.15f);Quiet();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/Sprint0028/View_"+x+".png"));yield return new WaitForEndOfFrame();}
            Move(-24,0);yield return null;CollectionAssert.AreEqual(references,points.Select(p=>p.Spawned).ToArray());Assert.AreEqual(13,points.Count(p=>p.Spawned.gameObject.activeInHierarchy));
        }
        [UnityTest] public IEnumerator TwoPurchasedRevivalsThenFailureAndRetryNeverRefillThem()
        {
            Quiet();var health=mission.Player.GetComponent<Health>();var revive=mission.Player.GetComponent<PlayerRevive>();Assert.AreEqual(0,revive.Injections);session.Data.faintanyl=2;mission.GetComponent<CampaignMissionBridge>().ApplyEquipment();
            for(int i=0;i<2;i++){health.ProtectFor(0);Assert.IsTrue(health.ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy)));Assert.IsFalse(mission.AttemptFailed);Assert.IsTrue(revive.TryRevive());Assert.Greater(revive.ProtectionRemaining,1.9f);Assert.AreEqual(1-i,session.Data.faintanyl);yield return null;}
            health.ProtectFor(0);Assert.IsTrue(health.ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy)));Assert.IsTrue(mission.AttemptFailed);Assert.IsFalse(revive.CanOfferRevive);yield return new WaitForSeconds(1.4f);Assert.IsTrue(health.IsDefeated);Assert.AreEqual(0,mission.CheckpointCommits);Assert.IsFalse(mission.Player.enabled);
            mission.Replay();yield return null;Assert.IsFalse(mission.AttemptFailed);Assert.IsFalse(health.IsDefeated);Assert.AreEqual(0,revive.Injections);Assert.AreEqual(0,session.Data.faintanyl);Assert.AreEqual(-24,mission.Player.transform.position.x,.1f);
        }
        [UnityTest] public IEnumerator SamuraiUsesActualSmallColliderToPursueElevatedPlayer()
        {
            Quiet();mission.Player.GetComponent<Health>().ProtectFor(120);var p=mission.Encounters[2].Points[0].Spawned;var combat=p.GetComponent<EnemyCombat>();combat.enabled=true;p.enabled=true;
            Move(60.2f,3.2f);var follow=Camera.main.GetComponent<WarehouseCameraFollow>();follow.enabled=false;Camera.main.transform.position=new Vector3(62,4,-10);Camera.main.orthographicSize=7;
            var samurai=p.GetComponent<SamuraiCombat>();var body=p.GetComponent<Rigidbody2D>();float last=body.position.x;int jumps=0;bool moved=false,reached=false;
            for(int i=0;i<600;i++){yield return new WaitForFixedUpdate();Assert.Less(Mathf.Abs(body.position.x-last),1,"No snapping/teleporting");last=body.position.x;jumps=samurai.PlatformJumps;if(jumps>0&&!moved){Move(65,3.2f);moved=true;}if(body.position.x>60.4f&&body.position.x<72&&p.GetComponent<BoxCollider2D>().bounds.min.y>3.1f&&p.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().IsGrounded){reached=true;break;}}
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/Sprint0028/SamuraiTraversal.json"),JsonUtility.ToJson(new TraversalEvidence{jumps=jumps,x=body.position.x,y=body.position.y,collider=p.GetComponent<BoxCollider2D>().size}));Assert.Greater(jumps,0);Assert.IsTrue(reached,"Samurai must land on the intended deck using its existing navigation");
        }
        [UnityTest] public IEnumerator ContinuousLevelRouteCanReachResultsUsingRealMovementAndExistingDoubleJump()
        {
            Quiet();mission.Player.GetComponent<Health>().ProtectFor(120);mission.Encounters[4].Points[0].Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(10000,CombatTeam.Player,mission.Player.gameObject));
            var player=mission.Player;var body=player.GetComponent<Rigidbody2D>();var box=player.GetComponent<BoxCollider2D>();var motor=player.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>();float deadline=Time.realtimeSinceStartup+75,nextJump=0;
            while(!mission.AttemptEnded&&Time.realtimeSinceStartup<deadline)
            {
                // Sprint0033 doubles the shell height: use its existing-system
                // roof ladder instead of demanding an impossible double jump.
                if(body.position.x>136&&body.position.x<140&&box.bounds.min.y<7.8f)
                {player.SetTouchMove(body.position.x<138.85f?Vector2.right:Vector2.up);yield return new WaitForFixedUpdate();continue;}
                player.SetTouchMove(Vector2.right);
                bool bunkerApproach=body.position.x>136&&body.position.x<140;
                if(motor.IsGrounded&&Time.time>=nextJump&&(bunkerApproach||Physics2D.BoxCast(box.bounds.center,(Vector2)box.bounds.size*.95f,0,Vector2.right,1.7f,1))){player.Jump();nextJump=Time.time+.4f;}
                else if(!motor.IsGrounded&&motor.JumpsUsed==1&&body.position.x>136&&body.position.x<145&&Time.time>=nextJump){player.Jump();nextJump=Time.time+.4f;}
                yield return new WaitForFixedUpdate();
            }
            player.SetTouchMove(Vector2.zero);File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/Sprint0028/GroundRoute.json"),"{\"x\":"+body.position.x.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"section\":"+mission.OperationSection+",\"complete\":"+(mission.Stage==MissionStage.Complete?"true":"false")+"}");
            Assert.AreEqual(MissionStage.Complete,mission.Stage,"Ground route stalled near x="+body.position.x);Assert.AreEqual(4,mission.OperationSection);Assert.AreEqual(0,mission.CheckpointCommits);
        }
        [UnityTest] public IEnumerator OfficerTelegraphsBeforeShootingAndCannotInitiateOffscreen()
        {
            Quiet();mission.Player.GetComponent<Health>().ProtectFor(30);var actor=mission.Encounters[4].Points[0].Spawned;actor.enabled=true;var combat=actor.GetComponent<EnemyCombat>();combat.enabled=true;var peek=actor.GetComponent<OfficerShotTelegraph>().BunkerPeek.position;actor.GetComponent<Rigidbody2D>().position=peek;actor.transform.position=peek;actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;Physics2D.SyncTransforms();
            Move(137,1.6f);var follow=Camera.main.GetComponent<WarehouseCameraFollow>();follow.enabled=false;Camera.main.transform.position=new Vector3(141,3,-10);Camera.main.orthographicSize=7;var shots=actor.GetComponent<AttackExecutor>();var telegraph=actor.GetComponent<OfficerShotTelegraph>();var telegraphSettings=new SerializedObject(telegraph);telegraphSettings.FindProperty("retreatDistance").floatValue=0;telegraphSettings.ApplyModifiedPropertiesWithoutUndo();
            float deadline=Time.time+6;while(!telegraph.Winding&&Time.time<deadline)yield return null;Assert.IsTrue(telegraph.Winding);Assert.AreEqual(0,shots.ShotsFired);yield return new WaitForSeconds(.3f);Assert.AreEqual(0,shots.ShotsFired);
            while(shots.ShotsFired==0&&Time.time<deadline)yield return null;Assert.Greater(shots.ShotsFired,0);Camera.main.transform.position=new Vector3(30,3,-10);int before=shots.ShotsFired;yield return new WaitForSeconds(2.5f);Assert.IsFalse(combat.MayInitiateFire);Assert.AreEqual(before,shots.ShotsFired);
        }
        [UnityTest]public IEnumerator ExpertCanThrowAtGroundApproachFromDeckEdgeAndRespectsViewport()
        {
            Quiet();mission.Player.GetComponent<Health>().ProtectFor(30);var actor=mission.Encounters[3].Points[0].Spawned;actor.enabled=true;actor.GetComponent<EnemyCombat>().enabled=true;Move(90,0);var follow=Camera.main.GetComponent<WarehouseCameraFollow>();follow.enabled=false;Camera.main.transform.position=new Vector3(91,4,-10);Camera.main.orthographicSize=7;
            var expert=actor.GetComponent<FireworksCombat>();float deadline=Time.time+6;while(expert.Launches==0&&Time.time<deadline)yield return new WaitForFixedUpdate();Assert.Greater(expert.Launches,0,"Solid deck must not prevent the authored expert from attacking the ground approach");Assert.AreEqual(2,expert.PoolSize);Camera.main.transform.position=new Vector3(30,3,-10);int before=expert.Launches;yield return new WaitForSeconds(3);Assert.AreEqual(before,expert.Launches);
        }
        [UnityTest]public IEnumerator ReliabilityCoversThrowAtAllThreeHeightsAndWalletsRemainCollectibleAtBunkerAndBoundaries()
        {
            Quiet();mission.Player.GetComponent<Health>().ProtectFor(60);var input=mission.Player.GetComponent<PlayerActions>();var load=mission.Player.GetComponent<PlayerLoadout>();var grenades=mission.Player.GetComponent<PlayerThrowables>();var cover=mission.Player.GetComponent<PlayerCover2D>();
            var controls=new SerializedObject(input);controls.FindProperty("readKeyboard").boolValue=false;controls.ApplyModifiedPropertiesWithoutUndo();
            string[] names={"StorageEntryVerticalCover","StorageLadderRetreatCover","StorageUpperApproachCover"};float[] floors={0,3.2f,6.4f};
            for(int i=0;i<3;i++)
            {
                input.SetTouchMove(Vector2.zero);input.SetTouchAim(Vector2.zero);Move(GameObject.Find(names[i]).transform.position.x-.9f,floors[i]);load.Select(EquipmentSlot.Melee);grenades.RestoreStock(new[]{1,1,1});grenades.Select((ThrowableKind)i);input.SetTouchMove(Vector2.down);input.SetTouchAim(new Vector2(1,.5f));yield return new WaitForSeconds(.35f);
                Assert.IsTrue(cover.IsCovered,names[i]);Assert.IsTrue(grenades.TryReleaseOrigin(input.AimDirection,out var origin));Assert.Greater(origin.y,cover.CoverTop+.09f);Assert.Less(Vector2.Distance(origin,grenades.PreviewOrigin),.04f);
                yield return new WaitForEndOfFrame();Directory.CreateDirectory("TestResults/CombatReliability");ScreenCapture.CaptureScreenshot(Path.GetFullPath("TestResults/CombatReliability/StorageCover"+i+".png"));yield return new WaitForEndOfFrame();
                int before=grenades.Throws;input.SetTouchAim(Vector2.zero);yield return null;yield return null;Assert.AreEqual(before+1,grenades.Throws);yield return new WaitForSeconds(.3f);
                foreach(var p in Object.FindObjectsByType<ThrowableProjectile2D>(FindObjectsSortMode.None))p.gameObject.SetActive(false);
            }
            input.SuspendInput();var supplies=mission.Player.GetComponent<PlayerSupplies>();int index=0;
            foreach(var at in new[]{new Vector2(-27.7f,.3f),new Vector2(16,.3f),new Vector2(23.5f,3.5f),new Vector2(28,6.7f),new Vector2(146.5f,1.95f),new Vector2(158.7f,.3f)})
            {
                var g=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/WalletPickup.prefab"),at,Quaternion.identity);var wallet=g.GetComponent<WalletPickup>();wallet.Initialize("authored-safe-"+index++,20);Vector2 point=g.transform.position;Move(point.x,point.y-.14f);Physics2D.SyncTransforms();int before=supplies.Earnings;Assert.IsTrue(wallet.TryCollect(supplies),at.ToString());Assert.IsFalse(wallet.TryCollect(supplies));Assert.AreEqual(before+20,supplies.Earnings);
            }
        }
        [Serializable] sealed class TraversalEvidence{public int jumps;public float x,y;public Vector2 collider;}
    }
}
#endif







