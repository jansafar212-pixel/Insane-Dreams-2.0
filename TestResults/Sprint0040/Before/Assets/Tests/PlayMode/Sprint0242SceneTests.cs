#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Missions;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0242SceneTests
    {
        private PlayerActions player;private Rigidbody2D body;private PlayerTraversal2D traversal;private bool background;
        [UnitySetUp] public IEnumerator Setup()
        {background=Application.runInBackground;Application.runInBackground=true;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;player=Object.FindAnyObjectByType<WarehouseMission>().Player;body=player.GetComponent<Rigidbody2D>();traversal=player.GetComponent<PlayerTraversal2D>();var input=new UnityEditor.SerializedObject(player);input.FindProperty("readKeyboard").boolValue=false;input.ApplyModifiedPropertiesWithoutUndo();player.GetComponent<Health>().ProtectFor(120);yield return new WaitForSeconds(.4f);}
        [UnityTearDown] public IEnumerator Cleanup(){yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));Application.runInBackground=background;}
        private void Place(Vector2 p){player.SetTouchMove(Vector2.zero);traversal.ResetTraversal();body.position=p;body.linearVelocity=Vector2.zero;player.transform.position=p;Physics2D.SyncTransforms();}
        [UnityTest] public IEnumerator RepairedBasementLadderPassesBothDirectionsAndLandsWithoutDisablingPlatforms()
        {
            Place(new Vector2(12,-6.2f));yield return new WaitForSeconds(.4f);player.SetTouchMove(Vector2.up);
            for(int i=0;i<450&&(body.position.y<6||body.position.x<13.4f);i++)yield return new WaitForFixedUpdate();
            Assert.Greater(body.position.y,6,"Climb must pass the former y=2 obstruction: "+body.position+", climbing="+traversal.IsClimbing);
            player.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.2f);
            Assert.IsFalse(traversal.IsClimbing);Assert.Greater(body.position.x,13);Assert.IsTrue(player.GetComponent<PlayerMotor2D>().enabled);Assert.AreEqual(3,body.gravityScale);
            Place(new Vector2(12,6.1f));yield return new WaitForSeconds(.4f);player.SetTouchMove(Vector2.down);
            for(int i=0;i<450&&body.position.y>-6.15f;i++)yield return new WaitForFixedUpdate();
            Assert.Less(body.position.y,-6.1f);yield return new WaitForSeconds(.4f);Assert.IsFalse(traversal.IsClimbing);Assert.IsTrue(player.GetComponent<PlayerMotor2D>().IsGrounded);Assert.AreEqual(3,body.gravityScale);
            var step=SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Collider2D>()).Single(c=>c.name=="AlternateGapStep");Assert.IsTrue(step.enabled);Assert.IsFalse(step.isTrigger);Assert.Less(step.bounds.max.x,11.2f);
        }
        [UnityTest] public IEnumerator RealLadderAllowsDesktopAndTouchJumpAwayAndOneRemainingAirJump()
        {
            foreach(bool touch in new[]{false,true})
            {
                Place(new Vector2(12,1));yield return new WaitForSeconds(.4f);player.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.15f);Assert.IsTrue(traversal.IsClimbing);
                var button=Object.FindAnyObjectByType<UIDocument>().rootVisualElement.Q<VisualElement>("jumpButton");
                void Tap(bool down)
                {
                    if(down){using(var e=PointerDownEvent.GetPooled(new Touch{fingerId=3,phase=TouchPhase.Began,position=button.worldBound.center},EventModifiers.None)){e.target=button;button.SendEvent(e);}}
                    else{using(var e=PointerUpEvent.GetPooled(new Touch{fingerId=3,phase=TouchPhase.Ended,position=button.worldBound.center},EventModifiers.None)){e.target=button;button.SendEvent(e);}}
                }
                if(touch)Tap(true);else player.Jump();yield return null;yield return new WaitForFixedUpdate();
                Assert.IsFalse(traversal.IsClimbing);Assert.AreEqual(1,player.GetComponent<PlayerMotor2D>().JumpsUsed);Assert.Greater(body.linearVelocity.y,10);Assert.AreEqual(3,body.gravityScale);
                if(touch){Tap(false);yield return null;Tap(true);}else player.Jump();yield return null;yield return new WaitForFixedUpdate();Assert.AreEqual(2,player.GetComponent<PlayerMotor2D>().JumpsUsed);
                if(touch){Tap(false);yield return null;Tap(true);}else player.Jump();yield return null;yield return new WaitForFixedUpdate();Assert.AreEqual(2,player.GetComponent<PlayerMotor2D>().JumpsUsed);if(touch)Tap(false);
            }
        }
    }
}
#endif
