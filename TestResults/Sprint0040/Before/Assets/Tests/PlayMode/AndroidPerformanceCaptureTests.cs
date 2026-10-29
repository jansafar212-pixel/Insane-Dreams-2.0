#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    // Deliberately Editor-only instrumentation, never included in the APK.
    public sealed class AndroidPerformanceCaptureTests
    {
        [Serializable] public sealed class Cost { public string name; public double msPerFrame; }
        [Serializable] public sealed class Probe { public string name; public double usPerCall,bytesPerCall; }
        [Serializable] public sealed class Capture
        {
            public string context="Controlled Unity Editor, NOT Android",phase;
            public int frames,enemyCount; public float seconds;
            public double averageFps,medianMs,p95Ms,p99Ms,onePercentLowFps,memoryBytes;
            public List<Cost> cpu=new List<Cost>(); public List<Probe> probes=new List<Probe>();
        }
        static Action Call(object target,string method)=>(Action)Delegate.CreateDelegate(typeof(Action),target,target.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance));
        static Probe Measure(string name,Action call,Action prepare=null)
        {
            for(int i=0;i<20;i++){prepare?.Invoke();call();}
            var watch=new System.Diagnostics.Stopwatch();long bytes=0;
            for(int i=0;i<200;i++)
            {
                prepare?.Invoke();long before=GC.GetAllocatedBytesForCurrentThread();watch.Start();call();watch.Stop();bytes+=GC.GetAllocatedBytesForCurrentThread()-before;
            }
            // Unity's current Mono runtime returns zero even for a known 8 KiB
            // allocation. Do not present that unsupported counter as zero GC.
            return new Probe{name=name,usPerCall=watch.Elapsed.TotalMilliseconds*1000/200,bytesPerCall=-1};
        }
        GameSession session;string save;bool background,profiling;float volume;
        [UnityTearDown] public IEnumerator Cleanup()
        {
            ProfilerDriver.enabled=profiling;Time.timeScale=1;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));
            if(session!=null)Object.Destroy(session.gameObject);yield return null;
            if(save!=null)foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);
            Application.runInBackground=background;AudioListener.volume=volume;
        }
        [UnityTest] public IEnumerator CaptureComparableWarehouseWorkload()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;profiling=ProfilerDriver.enabled;
            save=Path.Combine(Path.GetTempPath(),"AkumuPerf-"+Guid.NewGuid().ToString("N")+".json");
            session=new GameObject("PerformanceTestSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);
            session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));Assert.IsTrue(session.PrepareMission());
            UnityEngine.Random.InitState(32026);
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            var mission=Object.FindAnyObjectByType<WarehouseMission>();var player=mission.Player;player.GetComponent<Health>().ProtectFor(300);
            var input=new SerializedObject(player);input.FindProperty("readKeyboard").boolValue=false;input.ApplyModifiedPropertiesWithoutUndo();
            var hud=Object.FindAnyObjectByType<WarehouseTouchControls>();var readout=typeof(WarehouseTouchControls).GetField("nextReadout",BindingFlags.Instance|BindingFlags.NonPublic);
            var report=new Capture{phase=File.ReadAllText("TestResults/AndroidPerformance/phase.txt").Trim(),enemyCount=Object.FindObjectsByType<InsaneDreams.Enemies.EnemyCombat>(FindObjectsSortMode.None).Length};
            var rows=new List<double>(600);var totals=new Dictionary<string,double>();
            ProfilerDriver.enabled=true;
            for(int i=0;i<120;i++)yield return null;
            float began=Time.realtimeSinceStartup;
            // Same inputs, same scene and seeded decisions on both runs. No enemy cheats.
            for(int i=0;i<600;i++)
            {
                player.SetTouchAim(i%240<120?Vector2.right:Vector2.left);player.SetFireIntent(i%180<120);player.SetTouchMove(new Vector2(i%240<120?-.35f:.35f,0));
                yield return new WaitForEndOfFrame();rows.Add(Time.unscaledDeltaTime*1000);
            }
            report.memoryBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            report.seconds=Time.realtimeSinceStartup-began;
            int finalFrame=ProfilerDriver.lastFrameIndex-1;
            for(int frame=finalFrame-59;frame<=finalFrame;frame++)
            {
                using(var data=ProfilerDriver.GetRawFrameDataView(frame,0))
                {
                    if(!data.valid)continue;
                    for(int n=0;n<data.sampleCount;n++)
                    {
                        string name=data.GetSampleName(n);
                        // Inclusive samples are diagnostic categories, not additive totals.
                        if(!(name.Contains("Behaviour")||name.Contains("Physics2D")||name.Contains("UIElements")||name.Contains("Render")||name.Contains("Garbage")||name.Contains("GC.Alloc")))continue;
                        double value=data.GetSampleTimeMs(n);totals.TryGetValue(name,out double old);totals[name]=old+value;
                    }
                }
            }
            player.SetFireIntent(false);player.SetTouchMove(Vector2.zero);player.SetTouchAim(Vector2.zero);
            report.frames=rows.Count;rows.Sort();double sum=0,slow=0;foreach(double t in rows)sum+=t;
            for(int i=rows.Count-6;i<rows.Count;i++)slow+=rows[i];
            report.averageFps=1000/(sum/rows.Count);report.onePercentLowFps=1000/(slow/6);report.medianMs=rows[rows.Count/2];report.p95Ms=rows[(int)(rows.Count*.95)];report.p99Ms=rows[(int)(rows.Count*.99)];
            report.memoryBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            foreach(var pair in totals)report.cpu.Add(new Cost{name=pair.Key,msPerFrame=pair.Value/60});report.cpu.Sort((a,b)=>b.msPerFrame.CompareTo(a.msPerFrame));
            report.probes.Add(Measure("HUD steady frame",Call(hud,"Update"),()=>readout.SetValue(hud,float.MaxValue)));
            report.probes.Add(Measure("HUD unchanged readout",Call(hud,"Update"),()=>readout.SetValue(hud,0f)));
            var missionHud=Object.FindAnyObjectByType<MissionHud>();report.probes.Add(Measure("Mission unchanged readout",Call(missionHud,"Readout")));
            var visual=player.GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>();report.probes.Add(Measure("Player full pose",Call(visual,"LateUpdate")));
            var attack=player.GetComponent<AttackExecutor>();report.probes.Add(Measure("Tracer fade",Call(attack,"Update")));
            File.WriteAllText("TestResults/AndroidPerformance/"+report.phase+".json",JsonUtility.ToJson(report,true));
            Assert.AreEqual(600,report.frames);Assert.Greater(report.seconds,0);
        }
    }
}
#endif

