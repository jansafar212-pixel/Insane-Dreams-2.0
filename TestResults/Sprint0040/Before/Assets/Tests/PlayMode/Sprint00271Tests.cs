#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using InsaneDreams.Traversal;
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
    public sealed class Sprint00271Tests
    {
        GameSession session;string save;bool background;float volume;WarehouseMission mission;UnityEngine.Random.State random;
        [UnitySetUp]public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;random=UnityEngine.Random.state;UnityEngine.Random.InitState(271);
            save=Path.Combine(Path.GetTempPath(),"Akumu271-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("Hotfix271Session").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.PrepareMission();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            mission=Object.FindAnyObjectByType<WarehouseMission>();mission.Player.GetComponent<Health>().ProtectFor(120);
            foreach(var point in mission.Encounters.SelectMany(e=>e.Points)){point.Spawned.GetComponent<EnemyCombat>().enabled=false;point.Spawned.enabled=false;point.Spawned.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));if(session!=null)Object.Destroy(session.gameObject);yield return null;
            foreach(var suffix in new[]{"",".bak",".tmp"})if(save!=null&&File.Exists(save+suffix))File.Delete(save+suffix);Application.runInBackground=background;AudioListener.volume=volume;UnityEngine.Random.state=random;
        }
        [UnityTest]public IEnumerator EveryRevisedWarehouseLadderAutomaticallyExitsWithHeldTouchUp()
        {
            var actions=mission.Player;var traversal=actions.GetComponent<PlayerTraversal2D>();var body=actions.GetComponent<Rigidbody2D>();var box=actions.GetComponent<BoxCollider2D>();var motor=actions.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>();var trace=new System.Text.StringBuilder();
            var ladders=Object.FindObjectsByType<LadderVolume>().OrderBy(l=>l.name).ToArray();Assert.AreEqual(7,ladders.Length);
            foreach(var ladder in ladders)
            {
                actions.ResetActions();traversal.ResetTraversal();body.position=new Vector2(ladder.CenterX,ladder.Bottom+box.size.y*.5f+.02f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return new WaitForSeconds(.4f);
                actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.2f);Assert.IsTrue(traversal.IsClimbing,ladder.name);float before=body.position.y;actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.12f);Assert.Less(body.position.y,before-.2f,"Normal downward climbing: "+ladder.name);actions.SetTouchMove(Vector2.up);
                float end=Time.time+5;bool attached=true;while(Time.time<end){yield return new WaitForFixedUpdate();if(!traversal.IsClimbing)break;}
                trace.AppendLine($"{ladder.name} attached={attached} climbing={traversal.IsClimbing} x={body.position.x:F3} y={body.position.y:F3} top={ladder.Top:F3} feet={box.bounds.min.y:F3}");File.WriteAllText("TestResults/Sprint00271/Ladders.txt",trace.ToString());
                Assert.IsTrue(attached,ladder.name);Assert.IsFalse(traversal.IsClimbing,trace.ToString());Assert.IsTrue(motor.enabled);Assert.AreEqual(3,body.gravityScale);
                yield return new WaitForSeconds(.4f);Assert.IsFalse(traversal.IsClimbing,"Held upward input must not immediately reattach: "+ladder.name);Assert.IsTrue(motor.IsGrounded,ladder.name);Assert.Greater(Mathf.Abs(body.position.x-ladder.CenterX),.8f);Assert.Less(Mathf.Abs(box.bounds.min.y-(ladder.Top-.7f)),.06f,ladder.name);
            }
        }
        [UnityTest]public IEnumerator SeededOfficerHitsStationaryTargetAtTenUnitsWithoutPerfectAimOrExtraShots()
        {
            Time.timeScale=3;var floor=new GameObject("OfficerAccuracyFloor");floor.transform.position=new Vector2(50000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(40,1);
            var actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/FirstOperation/Officer.prefab"),new Vector3(50000,.7f),Quaternion.identity);actor.GetComponent<LarperBrain>().enabled=false;actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var victim=new GameObject("AccuracyTarget");victim.layer=2;victim.transform.position=new Vector2(50010,.7f);victim.AddComponent<BoxCollider2D>().size=new Vector2(.8f,1.4f);var hp=victim.AddComponent<Health>();var settings=new SerializedObject(hp);settings.FindProperty("team").enumValueIndex=(int)CombatTeam.Player;settings.FindProperty("maximum").floatValue=10000;settings.ApplyModifiedPropertiesWithoutUndo();hp.Restore();
            var camera=new GameObject("OfficerAccuracyCamera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=7;camera.aspect=16f/9;camera.transform.position=new Vector3(50005,2,-10);var combat=actor.GetComponent<EnemyCombat>();combat.BindTarget(hp);combat.BindGameplayCamera(camera);var laser=actor.GetComponent<OfficerShotTelegraph>();int hits=0,shots=0;actor.GetComponent<AttackExecutor>().Fired+=(a,b,hit)=>{shots++;if(hit)hits++;Assert.GreaterOrEqual(Time.time-laser.WarningStartedAt,.49f);Assert.Greater(Vector2.Dot(a,laser.Direction),.9999f);};
            float end=Time.time+32;while(shots<6&&Time.time<end)yield return null;
            File.WriteAllText("TestResults/Sprint00271/Accuracy.txt",$"seed271; range10; shots={shots}; hits={hits}; HP={hp.Current}");Assert.AreEqual(6,shots);Assert.GreaterOrEqual(hits,4,"Six real hitscan shots against a stationary player-sized target");Assert.Greater(combat.SpreadAtDistance(10),0);Assert.Less(combat.SpreadAtDistance(10),5);Assert.AreEqual(10000-hits*55,hp.Current,.1f);Assert.AreEqual(1,combat.Weapon.MagazineAmmo);Assert.IsFalse(combat.Weapon.Cycle.IsReloading);
        }
        static void Click(Button button){Assert.NotNull(button);using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}}
        [UnityTest]public IEnumerator SlimPauseRetainsTouchAreaAndOneMedicalTabPurchasesBothOriginalResources()
        {
            var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;var pause=root.Q<Button>("campaignPause");Assert.AreEqual(52,root.Q("pauseVisual").worldBound.width,1);Assert.AreEqual(40,root.Q("pauseVisual").worldBound.height,1);Assert.GreaterOrEqual(pause.worldBound.width,100);Assert.GreaterOrEqual(pause.worldBound.height,52);Assert.AreEqual(root.Q("safeArea").worldBound.center.x,pause.worldBound.center.x,1);Click(pause);yield return null;Assert.AreEqual(0,Time.timeScale);Object.FindAnyObjectByType<CampaignMissionBridge>().SetPaused(false);Assert.AreEqual(1,Time.timeScale);
            session.Data.balance=1000;session.Data.tape=50;session.Data.faintanyl=0;session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            Assert.AreEqual(1,root.Query<Button>().ToList().Count(b=>b.text=="Medical"));Assert.IsFalse(root.Query<Button>().ToList().Any(b=>b.text=="Duct tape"||b.text=="Faintanyl"));Click(root.Query<Button>().ToList().Single(b=>b.text=="Medical"));yield return null;
            var medical=root.Q("medicalShop");Assert.AreEqual(2,medical.childCount);Assert.AreEqual("ductTapeShop",medical[0].name);Assert.AreEqual("faintanylShop",medical[1].name);Assert.IsTrue(medical.Query<Label>().ToList().Any(l=>l.text=="Duct Tape"));Assert.IsTrue(medical.Query<Label>().ToList().Any(l=>l.text=="Faintanyl"));Assert.IsTrue(medical.Query<Label>().ToList().Any(l=>l.text=="Relax, it's not addictive. It's just habit forming."));
            Click(root.Q<Button>("procureDuctTape"));Assert.AreEqual(100,session.Data.tape);Assert.AreEqual(990,session.Data.balance);yield return new WaitForSecondsRealtime(.3f);Click(root.Q<Button>("procureFaintanyl"));Assert.AreEqual(1,session.Data.faintanyl);Assert.AreEqual(915,session.Data.balance);var stored=new ProgressStore(save).Load(()=>new GameProgress());Assert.AreEqual(100,stored.tape);Assert.AreEqual(1,stored.faintanyl);
            foreach(string category in new[]{"Weapons","Ammunition","Body armor"}){Click(root.Query<Button>().ToList().Single(b=>b.text==category));yield return null;Assert.IsNull(root.Q("medicalShop"));Assert.IsNull(root.Q("procureDuctTape"));Assert.IsNull(root.Q("procureFaintanyl"));}
        }
    }
}
#endif
