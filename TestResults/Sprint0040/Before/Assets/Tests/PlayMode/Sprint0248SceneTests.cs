#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Missions;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0248SceneTests
    {
        private bool previousBackground;
        [UnityTearDown]public IEnumerator Cleanup(){Application.runInBackground=previousBackground;yield return null;}
        [UnityTest]public IEnumerator ThreeSamuraiSurviveResetOnGroundThenIndividuallyDetectPursueAndStrike()
        {
            bool background=previousBackground=Application.runInBackground;Application.runInBackground=true;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.3f);
            var mission=Object.FindFirstObjectByType<WarehouseMission>();var player=mission.Player;var health=player.GetComponent<Health>();health.ProtectFor(120);player.ResetActions();player.enabled=false;
            var samurai=Object.FindObjectsByType<SamuraiCombat>();Assert.AreEqual(3,samurai.Length);Assert.AreEqual(17,mission.Encounters.Sum(e=>e.Count));
            foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;}
            var camera=Camera.main;camera.GetComponent<InsaneDreams.CameraSystem.WarehouseCameraFollow>().enabled=false;camera.orthographicSize=5;
            foreach(var s in samurai)
            {
                Assert.IsTrue(s.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().IsGrounded,s.name);float originalX=s.transform.position.x;
                var body=player.GetComponent<Rigidbody2D>();body.position=(Vector2)s.transform.position+(s.transform.position.x<-2?Vector2.left:Vector2.right)*2.5f;body.linearVelocity=Vector2.zero;player.transform.position=body.position;Physics2D.SyncTransforms();
                camera.transform.position=s.transform.position+new Vector3(0,1,-10);var e=s.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);e.BindTarget(health);e.SetEncounterEnabled(true);s.GetComponent<LarperBrain>().enabled=true;e.enabled=true;
                float deadline=Time.time+4;while(s.Strikes==0&&Time.time<deadline)yield return null;
                Assert.IsTrue(e.TargetVisible,s.name+" must see an unobstructed local player");Assert.Greater(Mathf.Abs(s.transform.position.x-originalX),.2f,s.name+" must pursue");Assert.Greater(s.Strikes,0,s.name+" must engage");
                e.enabled=false;s.GetComponent<LarperBrain>().enabled=false;s.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);
            }
            mission.Replay();yield return new WaitForSeconds(.3f);foreach(var p in mission.Encounters.SelectMany(e=>e.Points).Where(p=>p.Spawned.GetComponent<SamuraiCombat>()!=null)){Assert.Less(Vector2.Distance(p.transform.position,p.Spawned.transform.position),.1f);Assert.IsTrue(p.Spawned.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().IsGrounded);}
            Application.runInBackground=background;
        }
    }
}
#endif
