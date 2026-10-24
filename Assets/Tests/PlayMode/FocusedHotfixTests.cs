#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Prototype;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace InsaneDreams.Tests
{
    public sealed class FocusedHotfixTests
    {
        private bool background;
        [UnitySetUp] public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;yield return null;}
        [UnityTearDown] public IEnumerator Cleanup(){Application.runInBackground=background;yield return null;}
        [UnityTest] public IEnumerator SamuraiPhysicalJumpApexIncreasesModeratelyAndResetsOnLanding()
        {
            var floor=new GameObject("HotfixJumpFloor");floor.transform.position=new Vector2(64000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(30,1);
            var actor=Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(64000,.53f),Quaternion.identity);
            try
            {
                actor.GetComponent<EnemyCombat>().enabled=false;actor.GetComponent<LarperBrain>().enabled=false;
                var body=actor.GetComponent<Rigidbody2D>();var motor=actor.GetComponent<PlayerMotor2D>();float configured=motor.JumpSpeed;
                float[] rises=new float[2];
                for(int i=0;i<2;i++)
                {
                    var so=new UnityEditor.SerializedObject(motor);so.FindProperty("jumpSpeed").floatValue=i==0?12:configured;so.ApplyModifiedPropertiesWithoutUndo();
                    yield return new WaitForSeconds(.3f);float start=body.position.y;motor.RequestJump();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
                    float deadline=Time.time+2;while(Time.time<deadline){rises[i]=Mathf.Max(rises[i],body.position.y-start);yield return new WaitForFixedUpdate();if(motor.IsGrounded&&rises[i]>.5f)break;}
                    Assert.IsTrue(motor.IsGrounded);Assert.AreEqual(0,motor.JumpsUsed);
                }
                Assert.That(rises[1]/rises[0],Is.InRange(1.20f,1.25f));
            }
            finally{Object.Destroy(actor);Object.Destroy(floor);}
        }
        [UnityTest] public IEnumerator SamuraiCrossesActualWarehouseCrateCoverAndUpperSteps()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitForSeconds(.3f);
            var mission=Object.FindFirstObjectByType<InsaneDreams.Missions.WarehouseMission>();
            var player=mission.Player;player.ResetActions();player.enabled=false;var target=player.GetComponent<Health>();target.ProtectFor(120);
            var pb=player.GetComponent<Rigidbody2D>();pb.constraints=RigidbodyConstraints2D.FreezeAll;
            var s=Object.FindObjectsByType<SamuraiCombat>().First();
            foreach(var e in Object.FindObjectsByType<EnemyCombat>()){e.enabled=false;e.GetComponent<LarperBrain>().enabled=false;if(e.gameObject!=s.gameObject)e.gameObject.SetActive(false);}
            var body=s.GetComponent<Rigidbody2D>();var motor=s.GetComponent<PlayerMotor2D>();var brain=s.GetComponent<LarperBrain>();
            foreach(var pair in new[]{new Vector4(-5.5f,.53f,0,.85f),new Vector4(5.5f,.53f,10,.85f),new Vector4(1.5f,5.93f,15,12.85f)})
            {
                s.enabled=false;brain.enabled=false;motor.SetMoveInput(0);body.position=new Vector2(pair.x,pair.y);body.linearVelocity=Vector2.zero;
                pb.position=new Vector2(pair.z,pair.w);Physics2D.SyncTransforms();s.enabled=true;
                yield return new WaitForSeconds(.8f);brain.DecisionsSuspended=true;brain.enabled=true;
                float end=Time.time+18;float previous=body.position.x;int jumps=s.PlatformJumps;var trace=new System.Text.StringBuilder();float sample=0;
                while(Time.time<end)
                {
                    s.Tick(target,target.transform.position,true,true,true);yield return new WaitForFixedUpdate();
                    if(Time.time>=sample){sample=Time.time+.5f;trace.Append($"{body.position} {brain.State} {s.FailedTransitionReason}; ");}
                    if(Mathf.Abs(body.position.x-pair.z)<1.7f&&Mathf.Abs(body.position.y-(pair.w-.85f+.514f))<.3f&&motor.IsGrounded)break;
                }
                Assert.Less(Mathf.Abs(body.position.x-pair.z),1.7f,pair+" "+trace);
                Assert.AreEqual(pair.w-.85f+.514f,body.position.y,.3f,pair+" "+trace);
                Assert.Greater(s.PlatformJumps,jumps,pair+" must traverse physically");
            }
        }
    }
}
#endif
