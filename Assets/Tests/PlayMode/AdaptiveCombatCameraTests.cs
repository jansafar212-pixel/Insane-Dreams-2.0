#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using InsaneDreams.CameraSystem;
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
    public sealed class AdaptiveCombatCameraTests
    {
        const float X=5000;
        readonly List<GameObject> owned=new List<GameObject>();bool background;float volume;GameObject player;PlayerActions actions;Rigidbody2D body;Camera camera;WarehouseCameraFollow follow;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        void Value(UnityEngine.Object o,string n,float v){var s=new SerializedObject(o);s.FindProperty(n).floatValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;
            var floor=Keep(new GameObject("AdaptiveFloor"));floor.transform.position=new Vector2(X,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(1000,1);
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,.72f),Quaternion.identity));actions=player.GetComponent<PlayerActions>();body=player.GetComponent<Rigidbody2D>();player.GetComponent<InsaneDreams.Combat.Health>().ProtectFor(120);
            var input=new SerializedObject(actions);input.FindProperty("readKeyboard").boolValue=false;input.ApplyModifiedPropertiesWithoutUndo();
            camera=Keep(new GameObject("AdaptiveCamera")).AddComponent<Camera>();camera.aspect=16f/9;
            follow=camera.gameObject.AddComponent<WarehouseCameraFollow>();var s=new SerializedObject(follow);s.FindProperty("levelMin").vector2Value=new Vector2(X-400,-20);s.FindProperty("levelMax").vector2Value=new Vector2(X+400,160);s.ApplyModifiedPropertiesWithoutUndo();follow.Bind(player.transform);yield return new WaitForSeconds(.2f);
        }
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;AudioListener.volume=volume;}
        [UnityTest]public IEnumerator AimOverridesRetreatBothWaysAndDoesNotRequireFiring()
        {
            actions.SetTouchMove(Vector2.left);actions.SetTouchAim(Vector2.right);float start=body.position.x;yield return new WaitForSeconds(.85f);
            Assert.Less(body.position.x,start-1);Assert.IsFalse(actions.FireHeld);Assert.IsTrue(follow.CombatAttentionActive);Assert.Less(camera.WorldToViewportPoint(player.transform.position).x,.39f);
            actions.SetTouchMove(Vector2.right);actions.SetTouchAim(Vector2.left);start=body.position.x;yield return new WaitForSeconds(.85f);Assert.Greater(body.position.x,start+1);Assert.Greater(camera.WorldToViewportPoint(player.transform.position).x,.61f);
        }
        [UnityTest]public IEnumerator ReleasedAimKeepsCombatMemoryThenReturnsToMovement()
        {
            actions.SetTouchMove(Vector2.left);actions.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.8f);actions.SetTouchAim(Vector2.zero);yield return new WaitForSeconds(.6f);Assert.Less(follow.DesiredPlayerViewportX,.4f);Assert.IsTrue(follow.CombatAttentionActive);
            yield return new WaitForSeconds(1.2f);Assert.IsFalse(follow.CombatAttentionActive);Assert.Greater(follow.DesiredPlayerViewportX,.62f);
        }
        [UnityTest]public IEnumerator DeadZoneAndRapidReversalsCannotChatterOrStepTheCamera()
        {
            actions.SetTouchMove(Vector2.left);yield return new WaitForSeconds(.85f);actions.SetTouchMove(Vector2.zero);actions.SetTouchAim(Vector2.right*.21f);yield return new WaitForSeconds(1.1f);Assert.IsTrue(actions.HasAimIntent,"Gameplay aim threshold remains unchanged");Assert.IsFalse(follow.CombatAttentionActive,"Camera has its own noise threshold");Assert.Greater(follow.DesiredPlayerViewportX,.62f);
            float last=camera.transform.position.x,max=0;for(int i=0;i<12;i++){actions.SetTouchAim(i%2==0?Vector2.right:Vector2.left);yield return new WaitForSeconds(.04f);max=Mathf.Max(max,Mathf.Abs(camera.transform.position.x-last));last=camera.transform.position.x;}Assert.Greater(follow.DesiredPlayerViewportX,.6f);Assert.Less(max,.7f);
        }
        [UnityTest]public IEnumerator MovementRespondsWhileCameraTransitionIsStillInProgress()
        {
            actions.SetTouchMove(Vector2.right);yield return null;yield return new WaitForFixedUpdate();Assert.Greater(body.linearVelocity.x,0);Assert.Greater(body.position.x,X);
            actions.SetTouchMove(Vector2.left);actions.SetTouchAim(Vector2.left);yield return new WaitForSeconds(.12f);Assert.Less(actions.Move.x,0);Assert.Less(body.linearVelocity.x,0);Assert.That(follow.DesiredPlayerViewportX,Is.InRange(.32f,.60f),"Camera can still be transitioning after controls reverse");
        }
        [UnityTest]public IEnumerator CloseSamuraiContractsCompositionWithoutTargetLockOrJump()
        {
            actions.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.75f);float ordinary=follow.DesiredPlayerViewportX;
            var enemy=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(X+3,.53f),Quaternion.identity));var c=enemy.GetComponent<InsaneDreams.Enemies.EnemyCombat>();c.BindTarget(player.GetComponent<InsaneDreams.Combat.Health>());c.BindGameplayCamera(camera);enemy.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var behind=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(X-3,.53f),Quaternion.identity));behind.GetComponent<InsaneDreams.Enemies.EnemyCombat>().BindTarget(player.GetComponent<InsaneDreams.Combat.Health>());behind.GetComponent<InsaneDreams.Enemies.EnemyCombat>().BindGameplayCamera(camera);behind.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            yield return new WaitForSeconds(.8f);Assert.Greater(follow.NearbyThreatPressure,.4f);Assert.Greater(follow.DesiredPlayerViewportX,ordinary+.025f);Assert.Less(follow.DesiredPlayerViewportX,.5f);
            float before=camera.transform.position.x;Object.Destroy(enemy);Object.Destroy(behind);yield return null;Assert.Less(Mathf.Abs(camera.transform.position.x-before),.2f);yield return new WaitForSeconds(.8f);Assert.AreEqual(ordinary,follow.DesiredPlayerViewportX,.012f);
        }
        [UnityTest]public IEnumerator LargeFallAndParachuteChangesKeepLandingAreaAndPlayerVisible()
        {
            body.position=new Vector2(X,55);player.transform.position=body.position;body.linearVelocity=Vector2.down*22;Physics2D.SyncTransforms();follow.SnapToTarget();var traversal=player.GetComponent<PlayerTraversal2D>();bool deployed=false;
            float end=Time.time+3;while(Time.time<end){yield return new WaitForEndOfFrame();var p=camera.WorldToViewportPoint(player.transform.position);Assert.That(p.y,Is.InRange(.08f,.9f));if(!deployed&&Time.time>end-2.4f){traversal.ToggleParachute();deployed=true;}}
            Assert.IsTrue(traversal.ParachuteFlightActive);traversal.ToggleParachute();float landingDeadline=Time.time+6;
            while(!player.GetComponent<PlayerMotor2D>().IsGrounded&&Time.time<landingDeadline){yield return new WaitForEndOfFrame();Assert.That(camera.WorldToViewportPoint(player.transform.position).y,Is.InRange(.08f,.9f));}
            Assert.IsTrue(player.GetComponent<PlayerMotor2D>().IsGrounded);float last=camera.transform.position.y;
            for(int i=0;i<30;i++){yield return new WaitForEndOfFrame();Assert.Less(Mathf.Abs(camera.transform.position.y-last),.8f,"Landing recovery must not snap");last=camera.transform.position.y;}
        }
        [UnityTest]public IEnumerator BoundsEaseWithoutLeavingLevelAtPhoneAspects()
        {
            // Playable travel has the same buffer inside camera bounds as the
            // authored level. A camera cannot both contain the world and center a
            // player permitted to run arbitrarily beyond that world.
            foreach(float side in new[]{-1f,1f}){var wall=Keep(new GameObject("CameraBoundsFixtureWall"));wall.transform.position=new Vector2(X+side*20.5f,5);wall.AddComponent<BoxCollider2D>().size=new Vector2(1,12);}
            foreach(float aspect in new[]{16f/9,19.5f/9,20f/9,8f/3})
            {
                camera.aspect=aspect;var s=new SerializedObject(follow);s.FindProperty("levelMin").vector2Value=new Vector2(X-25,-10);s.FindProperty("levelMax").vector2Value=new Vector2(X+25,30);s.ApplyModifiedPropertiesWithoutUndo();body.position=new Vector2(X,.72f);player.transform.position=body.position;actions.ResetActions();follow.SnapToTarget();actions.SetTouchMove(Vector2.right);actions.SetTouchAim(Vector2.right);
                float end=Time.time+2.8f;while(Time.time<end){yield return new WaitForEndOfFrame();float half=camera.orthographicSize*camera.aspect;Assert.GreaterOrEqual(camera.transform.position.x-half,X-25-.001f);Assert.LessOrEqual(camera.transform.position.x+half,X+25+.001f);Assert.That(camera.WorldToViewportPoint(player.transform.position).x,Is.InRange(.08f,.92f));}
            }
        }
        [UnityTest]public IEnumerator DeterministicRatesAndSteadyStateAllocationsRemainBounded()
        {
            actions.enabled=false;body.simulated=false;follow.enabled=false;
            var step=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),follow,typeof(WarehouseCameraFollow).GetMethod("StepCamera",BindingFlags.Instance|BindingFlags.NonPublic));
            var results=new List<float>();
            foreach(var pattern in new[]{new[]{1f/30},new[]{1f/60},new[]{1f/120},new[]{1f/20,1f/120,1f/45}})
            {
                player.transform.position=new Vector3(X,.72f);follow.Bind(player.transform);actions.SetTouchAim(Vector2.left);typeof(PlayerActions).GetProperty("HasAimIntent").SetValue(actions,true);
                float t=0;int tick=0;while(t<2-.0001f){float d=Mathf.Min(pattern[tick++%pattern.Length],2-t);player.transform.position+=Vector3.right*(7*d);step(d);t+=d;}results.Add(camera.transform.position.x-player.transform.position.x);
            }
            Assert.Less(Mathf.Abs(results[0]-results[1]),.025f);Assert.Less(Mathf.Abs(results[1]-results[2]),.025f);Assert.Less(Mathf.Abs(results[1]-results[3]),.025f);
            long check=GC.GetAllocatedBytesForCurrentThread();var allocationProbe=new byte[8192];bool counterSupported=GC.GetAllocatedBytesForCurrentThread()>check;GC.KeepAlive(allocationProbe);
            for(int i=0;i<100;i++)step(.016f);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)step(.016f);long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
            if(counterSupported)Assert.LessOrEqual(allocated,1024,"No recurring managed allocations in camera stepping");
            System.IO.File.WriteAllText("TestResults/AdaptiveCombatCamera/AllocationBytes.txt",counterSupported?allocated.ToString():"NOT TESTED: allocation counter unsupported");
            float old=camera.transform.position.x;player.transform.position+=Vector3.right*.002f;step(.016f);Assert.That(camera.transform.position.x-old,Is.InRange(.0001f,.01f),"Sub-art-pixel movement must not stall or jump one1/32unit cell");yield return null;
        }
        [UnityTest]public IEnumerator VerticalAimBiasRemainsBounded()
        {
            body.position=new Vector2(X,30);player.transform.position=body.position;body.constraints=RigidbodyConstraints2D.FreezeAll;follow.SnapToTarget();actions.SetTouchAim(Vector2.up);yield return new WaitForSeconds(.9f);float up=camera.transform.position.y;Assert.That(camera.WorldToViewportPoint(player.transform.position).y,Is.InRange(.15f,.45f));
            actions.SetTouchAim(Vector2.down);yield return new WaitForSeconds(.9f);Assert.Less(camera.transform.position.y,up-1);Assert.That(camera.WorldToViewportPoint(player.transform.position).y,Is.InRange(.35f,.75f));
        }
    }
}
#endif

