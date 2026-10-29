#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsaneDreams.CameraSystem;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Visuals;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Level1CombatHotfixTests
    {
        GameSession session;string save;bool background;float volume;WarehouseMission mission;UnityEngine.Random.State random;
        [UnitySetUp]public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;random=UnityEngine.Random.state;UnityEngine.Random.InitState(2815);
            save=Path.Combine(Path.GetTempPath(),"AkumuCombat-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("CombatTestSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.PrepareMission();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;mission=Object.FindAnyObjectByType<WarehouseMission>();mission.Player.GetComponent<Health>().ProtectFor(120);
            foreach(var point in mission.Encounters.SelectMany(g=>g.Points)){point.Spawned.GetComponent<EnemyCombat>().enabled=false;point.Spawned.enabled=false;point.Spawned.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Object.Destroy(session.gameObject);yield return null;foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);AudioListener.volume=volume;Application.runInBackground=background;UnityEngine.Random.state=random;}
        void PlayerAt(float x,float y)
        {var b=mission.Player.GetComponent<Rigidbody2D>();b.position=new Vector2(x,y);b.linearVelocity=Vector2.zero;mission.Player.transform.position=b.position;Physics2D.SyncTransforms();}
        void CameraAt(float x,float y)
        {var c=Camera.main;c.GetComponent<WarehouseCameraFollow>().enabled=false;c.transform.position=new Vector3(x,y,-10);c.orthographicSize=7;}
        OfficerShotTelegraph Officer(bool frozen=true)
        {
            var a=mission.Encounters[4].Points[0].Spawned;a.GetComponent<EnemyCombat>().enabled=true;a.enabled=!frozen;if(frozen)a.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;var o=a.GetComponent<OfficerShotTelegraph>();
            if(frozen){a.GetComponent<Rigidbody2D>().position=o.BunkerPeek.position;a.transform.position=o.BunkerPeek.position;Physics2D.SyncTransforms();var s=new SerializedObject(o);s.FindProperty("retreatDistance").floatValue=0;s.FindProperty("bunkerRetreatChance").floatValue=0;s.ApplyModifiedPropertiesWithoutUndo();}return o;
        }
        [UnityTest]public IEnumerator EveryOfficerShotHasHalfSecondWarningAndLockedFinalDirection()
        {
            var o=Officer();PlayerAt(137,2.35f);mission.Player.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;CameraAt(141,4);var attacks=o.GetComponent<AttackExecutor>();var warnings=new List<float>();var times=new List<float>();var dirs=new List<Vector2>();attacks.Fired+=(a,b,c)=>{warnings.Add(Time.time-o.WarningStartedAt);times.Add(Time.time);dirs.Add(attacks.LastShotDirection);};
            float end=Time.time+10;while(!o.Winding&&Time.time<end)yield return null;Assert.IsTrue(o.Winding);yield return new WaitForSeconds(.15f);yield return new WaitForEndOfFrame();Assert.Greater(Vector2.Dot(o.GetComponent<CharacterVisualDriver>().Rig.BarrelDirection,o.Direction),.9999f,"Tracking-phase barrel must follow the laser's actual sampled aim");while(Time.time<o.WarningStartedAt+.35f&&Time.time<end)yield return null;Assert.IsTrue(o.Locked);Assert.IsTrue(o.LaserVisible);Assert.AreEqual(0,attacks.ShotsFired);Vector2 locked=o.Direction;
            PlayerAt(137,3.05f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(Application.dataPath),"TestResults/Level1CombatHotfix/OfficerLaser.png"));
            while(warnings.Count<2&&Time.time<end)yield return null;Assert.AreEqual(2,warnings.Count);foreach(float duration in warnings)Assert.GreaterOrEqual(duration,.49f);Assert.GreaterOrEqual(times[1]-times[0],2.89f);Assert.Greater(Vector2.Dot(locked,dirs[0]),.99999f,"The fired direction must match the pre-dodge laser lock");
        }
        [UnityTest]public IEnumerator LaserClipsAtSolidCoverAndCancelsOffscreen()
        {
            var o=Officer();var combat=o.GetComponent<EnemyCombat>();combat.BindTarget(null);typeof(EnemyCombat).GetProperty("AimDirection").SetValue(combat,Vector2.left);CameraAt(141,4);var wall=new GameObject("TestLaserBlock");float wallX=o.GetComponent<AttackExecutor>().Origin.x-2;wall.transform.position=new Vector2(wallX,2.5f);wall.AddComponent<BoxCollider2D>().size=new Vector2(.3f,5);Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.25f); // Sprint0032 camera-entry grace precedes the isolated beam check.
            for(int i=0;i<4;i++){o.AllowShot(Vector2.left,0,20,out _);yield return new WaitForEndOfFrame();}Assert.IsTrue(o.LaserVisible);Assert.AreEqual(wallX+.15f,o.LaserEnd.x,.02f);Assert.AreEqual(0,o.GetComponent<AttackExecutor>().ShotsFired);
            CameraAt(30,4);yield return null;yield return new WaitForEndOfFrame();Assert.IsFalse(o.LaserVisible);Assert.IsFalse(o.Winding);
        }
        [UnityTest]public IEnumerator OfficerRetreatsWithoutTeleportingAndOptionalAllyDoesNotGateExit()
        {
            var o=Officer(false);var ally=Target("OptionalSupportFixture",new Vector2(146,2.3f),CombatTeam.Enemy);var support=new SerializedObject(o);var allies=support.FindProperty("nearbyAllies");allies.arraySize=1;allies.GetArrayElementAtIndex(0).objectReferenceValue=ally;support.ApplyModifiedPropertiesWithoutUndo();bool sawSupport=false;float maxExcursion=0;PlayerAt(137,2.35f);CameraAt(141,4);float start=o.transform.position.x,end=Time.time+6;bool retreat=false;var trace=new List<string>();o.GetComponent<EnemyCombat>().WithdrawalFailed+=()=>trace.Add("WITHDRAWAL FAILED min="+new SerializedObject(o.GetComponent<EnemyCombat>()).FindProperty("minimumWithdrawalDistance").floatValue+" leftReach="+o.GetComponent<LarperBrain>().WithdrawalReach(-1,7)+" rightReach="+o.GetComponent<LarperBrain>().WithdrawalReach(1,7));while(Time.time<end){var c=o.GetComponent<EnemyCombat>();retreat|=c.State==EngagementState.Withdrawing;sawSupport|=o.AlliesAlive;maxExcursion=Mathf.Max(maxExcursion,Mathf.Abs(o.transform.position.x-start));trace.Add(c.State+" visible="+c.TargetVisible+" position="+o.transform.position+" grounded="+o.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().IsGrounded+" player="+mission.Player.transform.position+" retreat="+o.RetreatDistance);yield return new WaitForFixedUpdate();}File.WriteAllLines("TestResults/Level1CombatHotfix/OfficerRetreat.txt",trace);Assert.IsTrue(retreat,"Officer must choose withdrawal on a visible close approach; inspect OfficerRetreat.txt");Assert.IsTrue(sawSupport,"Assigned command guard is recognized when the Officer approaches within the original support radius");Assert.Greater(maxExcursion,.2f);Assert.Less(Mathf.Abs(o.transform.position.x-start),12);
            o.GetComponent<Health>().ApplyDamage(new DamageInfo(10000,CombatTeam.Player,mission.Player.gameObject));Assert.IsFalse(ally.IsDefeated);Assert.AreEqual(MissionStage.Escape,mission.Stage);Assert.IsTrue(mission.Encounters[4].Cleared);
            var exit=Object.FindObjectsByType<MissionTrigger>().Single(z=>z.Zone==MissionZone.Exit);PlayerAt(exit.Region.bounds.center.x,1);Assert.IsTrue(mission.EnterZone(MissionZone.Exit,exit.Region));Assert.AreEqual(MissionStage.Complete,mission.Stage);
        }
        [UnityTest]public IEnumerator CommittedThrowSurvivesJumpingSightLossAndTickGaps()
        {
            var a=mission.Encounters[3].Points[0].Spawned;var combat=a.GetComponent<EnemyCombat>();combat.enabled=true;a.enabled=true;var f=a.GetComponent<FireworksCombat>();var s=new SerializedObject(f);s.FindProperty("heldMalfunctionChance").floatValue=0;s.ApplyModifiedPropertiesWithoutUndo();PlayerAt(90,.75f);CameraAt(91,4);
            float end=Time.time+6;while(!f.Preparing&&Time.time<end)yield return null;Assert.IsTrue(f.Preparing);var rocket=Object.FindObjectsByType<FireworkProjectile>().Single(p=>p.Busy&&!p.Launched);Vector2 selected=rocket.Destination;float fuse=rocket.DetonationDelay;mission.Player.Jump();end=Time.time+2;
            while(f.Launches==0&&Time.time<end)
            {mission.Player.SetTouchMove(new Vector2(Mathf.Sin(Time.time*12),0));f.Tick(mission.Player.GetComponent<Health>(),mission.Player.transform.position,false,true,false);if(!rocket.Launched){Assert.Less(Vector2.Distance(selected,rocket.Destination),.001f);Assert.AreEqual(fuse,rocket.DetonationDelay);}yield return new WaitForFixedUpdate();}
            mission.Player.SetTouchMove(Vector2.zero);Assert.AreEqual(1,f.Launches);Assert.IsFalse(f.Preparing);Assert.AreEqual(2,f.PoolSize);
        }
        Health Target(string name,Vector2 p,CombatTeam team,bool armor=false)
        {var g=new GameObject(name);g.layer=2;g.transform.position=p;g.AddComponent<BoxCollider2D>().size=Vector2.one*.1f;if(armor)g.AddComponent<BodyArmor>().Restore(60);var h=g.AddComponent<Health>();var s=new SerializedObject(h);s.FindProperty("maximum").floatValue=200;s.FindProperty("team").enumValueIndex=(int)team;s.ApplyModifiedPropertiesWithoutUndo();h.Restore();return h;}
        [UnityTest]public IEnumerator ActualRadialBlastPreservesFriendlySelfArmorAndOcclusionRules()
        {
            var center=new Vector2(50,8);var self=Target("Self",center,CombatTeam.Enemy);var enemy=Target("Ally",center+Vector2.right*.7f,CombatTeam.Enemy);var mid=Target("PlayerMid",center+Vector2.right*1.4f,CombatTeam.Player);var far=Target("PlayerFar",center+Vector2.right*2.1f,CombatTeam.Player);var edge=Target("AtBoundary",center+Vector2.right*2.8f,CombatTeam.Player);var outside=Target("Outside",center+Vector2.right*3,CombatTeam.Player);var armor=Target("Armor",center+Vector2.left*.7f,CombatTeam.Player,true);var shielded=Target("Covered",center+Vector2.up*1.4f,CombatTeam.Player);
            var wall=new GameObject("BlastShield");wall.transform.position=center+Vector2.up*.7f;wall.AddComponent<BoxCollider2D>().size=new Vector2(1,.1f);var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/FirstOperation/OperationFirework.prefab"));var p=go.GetComponent<FireworkProjectile>();Physics2D.SyncTransforms();p.Prepare(self.gameObject,center,Color.yellow,true);p.ExplodeHeld(center);
            Assert.AreEqual(100,self.Current,.1f);Assert.AreEqual(115,enemy.Current,.1f);Assert.AreEqual(150,mid.Current,.1f);Assert.AreEqual(180,far.Current,.1f);Assert.AreEqual(200,edge.Current);Assert.AreEqual(200,outside.Current);Assert.AreEqual(200,shielded.Current);Assert.AreEqual(115,armor.Current,.15f,"Ballistic armor does not absorb explosions");Assert.AreEqual(60,armor.GetComponent<BodyArmor>().Condition);yield return null;
        }
    }
}
#endif
