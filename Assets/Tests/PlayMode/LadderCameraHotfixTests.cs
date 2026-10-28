#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.CameraSystem;
using InsaneDreams.Input;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class LadderCameraHotfixTests
    {
        private bool background;private readonly List<GameObject> owned=new List<GameObject>();
        private GameObject Keep(GameObject go){owned.Add(go);return go;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;}
        [UnityTest]public IEnumerator ClimbingCentersSmoothlyRejectsAimChangesAndRestoresDirectionalComposition()
        {
            // Keep the isolation offset below the range where float precision stalls camera damping.
            var floor=Keep(new GameObject("LadderCameraFloor"));floor.transform.position=new Vector2(7100,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(100,1);
            var p=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(7100,.85f),Quaternion.identity));
            var actions=p.GetComponent<PlayerActions>();var so=new SerializedObject(actions);so.FindProperty("readKeyboard").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();
            var c=Keep(new GameObject("LadderCamera")).AddComponent<Camera>();c.orthographic=true;c.aspect=16f/9;var follow=c.gameObject.AddComponent<WarehouseCameraFollow>();so=new SerializedObject(follow);so.FindProperty("levelMin").vector2Value=new Vector2(7000,-20);so.FindProperty("levelMax").vector2Value=new Vector2(7200,80);so.FindProperty("visualPixelsPerUnit").intValue=0;so.ApplyModifiedPropertiesWithoutUndo();follow.Bind(p.transform);
            yield return new WaitForSeconds(.3f);Assert.AreEqual(1f/3,c.WorldToViewportPoint(p.transform.position).x,.01f);
            var ladder=Keep(new GameObject("CameraTestLadder"));ladder.transform.position=new Vector2(7100,10);var box=ladder.AddComponent<BoxCollider2D>();box.isTrigger=true;box.size=new Vector2(1.8f,30);ladder.AddComponent<LadderVolume>();
            p.GetComponent<Rigidbody2D>().position=new Vector2(7100,3);Physics2D.SyncTransforms();actions.SetTouchMove(Vector2.up);
            float end=Time.time+2.8f;float previous=c.WorldToViewportPoint(p.transform.position).x,maxStep=0;
            while(Time.time<end){yield return null;float x=c.WorldToViewportPoint(p.transform.position).x;maxStep=Mathf.Max(maxStep,Mathf.Abs(x-previous));previous=x;}
            Assert.IsTrue(p.GetComponent<PlayerTraversal2D>().IsClimbing);Assert.AreEqual(.5f,previous,.008f);Assert.Less(maxStep,.06f);
            var aimDirections=new[]{Vector2.left,Vector2.right,Vector2.up,Vector2.down};
            for(int i=0;i<8;i++){actions.SetTouchAim(aimDirections[i%4]);yield return new WaitForSeconds(.06f);Assert.AreEqual(.5f,c.WorldToViewportPoint(p.transform.position).x,.008f);Assert.That(c.WorldToViewportPoint(p.transform.position).y,Is.InRange(.15f,.85f));}
            actions.SetTouchMove(Vector2.right);actions.SetTouchAim(Vector2.right);actions.Jump();yield return new WaitForSeconds(.1f);Assert.IsFalse(p.GetComponent<PlayerTraversal2D>().IsClimbing);
            yield return new WaitForSeconds(3.4f);Assert.AreEqual(1f/3,c.WorldToViewportPoint(p.transform.position).x,.012f);
        }
    }
}
#endif
