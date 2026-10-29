using InsaneDreams.CameraSystem;
using NUnit.Framework;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class DevelopmentFrameStatsTests
    {
        [Test] public void RollingFrameTimesReportPercentilesAndExpireOldSpikes()
        {
            var go=new GameObject("FrameStatsTest");try
            {
                var meter=go.AddComponent<DevelopmentFrameOverlay>();for(int i=0;i<120;i++)meter.RecordFrame(1f/60);var stable=meter.ReadStats();Assert.AreEqual(60,stable.fps,.01f);Assert.AreEqual(16.6667f,stable.p95Ms,.01f);
                for(int i=0;i<12;i++)meter.RecordFrame(.05f);var spike=meter.ReadStats();Assert.Greater(spike.p95Ms,49);Assert.Greater(spike.p99Ms,49);Assert.Less(spike.fps,60);
                for(int i=0;i<120;i++)meter.RecordFrame(1f/60);meter.RecordFrame(float.NaN);Assert.AreEqual(60,meter.ReadStats().fps,.01f);
            }finally{Object.DestroyImmediate(go);}
        }
    }
}
