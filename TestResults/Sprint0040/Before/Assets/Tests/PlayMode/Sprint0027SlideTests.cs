#if UNITY_EDITOR
using System.Collections;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0027SlideTests
    {
        GameObject floor,player;PlayerActions actions;PlayerMotor2D motor;Rigidbody2D body;bool background;
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;floor=new GameObject("SlideFloor");floor.transform.position=new Vector2(29000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(100,1);player=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(29000,.85f),Quaternion.identity);actions=player.GetComponent<PlayerActions>();motor=player.GetComponent<PlayerMotor2D>();body=player.GetComponent<Rigidbody2D>();yield return new WaitForSeconds(.1f);}
        [UnityTearDown]public IEnumerator Cleanup(){Object.Destroy(player);Object.Destroy(floor);yield return null;Application.runInBackground=background;}
        [UnityTest]public IEnumerator RunningDownGestureSlidesWithFiniteMomentumAndReducedCollider()
        {actions.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.4f);float speed=body.linearVelocity.x,height=player.GetComponent<BoxCollider2D>().size.y;actions.SetTouchMove(new Vector2(1,-1));yield return null;Assert.IsTrue(actions.IsLandingSliding);yield return new WaitForFixedUpdate();Assert.Less(player.GetComponent<BoxCollider2D>().size.y,height);Assert.Greater(body.linearVelocity.x,speed*.5f);Assert.LessOrEqual(body.linearVelocity.x,motor.MoveSpeed+.1f);actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.5f);Assert.IsFalse(actions.IsLandingSliding);Assert.IsFalse(player.GetComponent<PlayerPosture2D>().IsCrouched);}
        [UnityTest]public IEnumerator DoubleJumpDownRequestSurvivesReleaseUntilLanding()
        {actions.SetTouchMove(Vector2.right);actions.Jump();yield return new WaitForSeconds(.15f);actions.Jump();yield return new WaitForSeconds(.1f);actions.SetTouchMove(new Vector2(1,-1));yield return null;yield return new WaitForFixedUpdate();Assert.IsTrue(actions.IsAirSliding);actions.SetTouchMove(Vector2.right);float until=Time.time+3;while(!actions.IsLandingSliding&&Time.time<until)yield return null;Assert.IsTrue(actions.IsLandingSliding);Assert.IsTrue(player.GetComponent<PlayerPosture2D>().IsCrouched);yield return new WaitForSeconds(.5f);Assert.IsFalse(actions.IsLandingSliding);Assert.AreEqual(0,motor.JumpsUsed);}
    }
}
#endif
