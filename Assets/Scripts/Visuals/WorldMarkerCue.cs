using InsaneDreams.Checkpoints;
using InsaneDreams.Missions;
using UnityEngine;
namespace InsaneDreams.Visuals
{
    // Presentation only. Existing checkpoint/mission components own activation.
    public sealed class WorldMarkerCue : MonoBehaviour
    {
        public CheckpointMarker checkpoint;
        public WarehouseMission mission;
        public SpriteRenderer indicator;
        private void Update()
        {
            if(indicator==null)return;
            bool open=checkpoint!=null?checkpoint.IsLit:mission!=null&&mission.Stage>=MissionStage.Escape;
            Color color=open?new Color(.3f,1,.65f):new Color(1,.72f,.25f);
            float pulse=.86f+.08f*Mathf.Sin(Time.time*2+transform.position.x);
            indicator.color=new Color(color.r*pulse,color.g*pulse,color.b*pulse,1);
        }
    }
}
