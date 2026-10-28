#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
namespace InsaneDreams.CameraSystem
{
    // Playtest instrumentation only. It never changes frame caps or input.
    public sealed class DevelopmentFrameOverlay : MonoBehaviour
    {
        private readonly float[] samples=new float[120],sorted=new float[120];
        private int cursor,count;private float nextReadout;private Label label;
        public struct FrameStats { public float fps,meanMs,p95Ms,p99Ms; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
        private static void Loaded(Scene scene,LoadSceneMode mode)
        {
            if(scene.name=="Warehouse_Demo"||scene.name=="Warehouse_Testbed"||scene.name=="Warehouse_Mission01")
                new GameObject("DevelopmentFrameTimes").AddComponent<DevelopmentFrameOverlay>();
        }
        private IEnumerator Start()
        {
            yield return null;yield return null;
            foreach(var doc in FindObjectsByType<UIDocument>())
            {
                if(doc.gameObject.scene!=gameObject.scene)continue;
                var root=doc.rootVisualElement;var safe=root.Q("safeArea");if(safe==null)continue;
                label=new Label("FRAME TIMES / WARMING UP"){name="developmentFrameTimes",pickingMode=PickingMode.Ignore};
                label.style.position=Position.Absolute;label.style.left=Length.Percent(30);label.style.top=70;label.style.width=Length.Percent(40);
                label.style.fontSize=11;label.style.color=new Color(.9f,1,.75f);label.style.backgroundColor=new Color(.08f,.1f,.1f,.8f);
                safe.Add(label);break;
            }
        }
        public void RecordFrame(float seconds)
        {if(seconds<=0||float.IsNaN(seconds)||float.IsInfinity(seconds))return;samples[cursor]=seconds*1000;cursor=(cursor+1)%samples.Length;count=Mathf.Min(count+1,samples.Length);}
        public FrameStats ReadStats()
        {
            if(count==0)return default;float total=0;for(int i=0;i<count;i++){sorted[i]=samples[i];total+=samples[i];}Array.Sort(sorted,0,count);
            float mean=total/count;return new FrameStats{fps=1000/mean,meanMs=mean,p95Ms=sorted[Mathf.CeilToInt(count*.95f)-1],p99Ms=sorted[Mathf.CeilToInt(count*.99f)-1]};
        }
        private void Update()
        {
            if(Time.timeScale>0)RecordFrame(Time.unscaledDeltaTime);
            if(label==null||Time.unscaledTime<nextReadout)return;nextReadout=Time.unscaledTime+1;
            var s=ReadStats();label.text=$"FPS {s.fps:0} / {s.meanMs:0.0} ms / p95 {s.p95Ms:0.0} / p99 {s.p99Ms:0.0}";
        }
        private void OnDestroy()=>label?.RemoveFromHierarchy();
    }
}
#endif
