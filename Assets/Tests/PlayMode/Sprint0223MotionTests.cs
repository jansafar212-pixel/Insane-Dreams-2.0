#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Input;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    [DefaultExecutionOrder(300)]
    public sealed class RenderMotionProbe : MonoBehaviour
    {
        public CharacterVisualDriver driver;public Camera view;
        public int samples;public float maximumLateRenderShift,maximumBackwardStep,maximumPresentationError;
        private Vector3 lateRig,previousRig;private float previousVelocity;private int lateFrame=-1;
        private void OnEnable()=>RenderPipelineManager.beginCameraRendering+=Render;
        private void OnDisable()=>RenderPipelineManager.beginCameraRendering-=Render;
        private void LateUpdate(){if(driver==null)return;lateRig=driver.Rig.transform.position;lateFrame=Time.frameCount;}
        private void Render(ScriptableRenderContext context,Camera camera)
        {
            if(camera!=view||driver==null||lateFrame!=Time.frameCount)return;
            Vector3 p=driver.Rig.transform.position;float velocity=driver.GetComponent<Rigidbody2D>().linearVelocity.x;
            maximumLateRenderShift=Mathf.Max(maximumLateRenderShift,Vector3.Distance(p,lateRig));
            maximumPresentationError=Mathf.Max(maximumPresentationError,Vector3.Distance(p,driver.transform.position));
            if(samples>0&&velocity>1&&previousVelocity>1)maximumBackwardStep=Mathf.Max(maximumBackwardStep,previousRig.x-p.x);
            previousRig=p;previousVelocity=velocity;samples++;
        }
    }
    public sealed class Sprint0223MotionTests
    {
        [UnityTest] public IEnumerator RenderedMotionDoesNotRewindOrMoveAfterLateUpdate()
        {
            bool background=Application.runInBackground;Application.runInBackground=true;
            var floor=new GameObject("S0223Floor");floor.transform.position=new Vector2(23000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(80,1);
            var player=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(23000,.85f),Quaternion.identity);
            var cameraObject=new GameObject("S0223RenderCamera");var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=6;camera.transform.position=new Vector3(23000,2,-10);
            var probe=player.AddComponent<RenderMotionProbe>();probe.driver=player.GetComponent<CharacterVisualDriver>();probe.view=camera;
            var actions=player.GetComponent<PlayerActions>();var settings=new SerializedObject(actions);settings.FindProperty("readKeyboard").boolValue=false;settings.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                yield return new WaitForSeconds(.25f);
                actions.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.5f);
                for(int i=0;i<8;i++){actions.SetTouchMove(i%2==0?Vector2.left:Vector2.right);yield return new WaitForSeconds(.12f);}
                var result=new MotionResult{samples=probe.samples,lateRenderShift=probe.maximumLateRenderShift,backwardStep=probe.maximumBackwardStep,presentationError=probe.maximumPresentationError};
                System.IO.File.WriteAllText("TestResults/Sprint0223MotionTrace.json",JsonUtility.ToJson(result,true));
                Assert.Greater(probe.samples,10);Assert.Less(probe.maximumLateRenderShift,.001f);Assert.Less(probe.maximumBackwardStep,.04f);
                Assert.Less(probe.maximumPresentationError,.001f,"The interpolated player presentation must not be rounded onto a separate coarse grid every frame.");
            }
            finally{Object.Destroy(player);Object.Destroy(floor);Object.Destroy(cameraObject);Application.runInBackground=background;}
            yield return null;
        }
        [System.Serializable] private sealed class MotionResult{public int samples;public float lateRenderShift,backwardStep,presentationError;}
    }
}
#endif
