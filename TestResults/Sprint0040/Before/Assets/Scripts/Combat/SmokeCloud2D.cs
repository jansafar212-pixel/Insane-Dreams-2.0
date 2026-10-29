using System.Collections.Generic;
using UnityEngine;
namespace InsaneDreams.Combat
{
    public sealed class SmokeCloud2D : MonoBehaviour
    {
        static readonly List<SmokeCloud2D> active=new List<SmokeCloud2D>(12);
        private float radius,until;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset()=>active.Clear();
        public void Activate(float size,float seconds){radius=size;until=Time.time+seconds;enabled=true;if(!active.Contains(this))active.Add(this);}
        private void OnDisable()=>active.Remove(this);
        public static bool Blocks(Vector2 origin,Vector2 end)
        {
            Vector2 delta=end-origin;float length=delta.sqrMagnitude;if(length<.001f)return false;
            for(int i=0;i<active.Count;i++){var cloud=active[i];if(cloud==null||Time.time>=cloud.until)continue;Vector2 center=cloud.transform.position;float t=Mathf.Clamp01(Vector2.Dot(center-origin,delta)/length);if((origin+delta*t-center).sqrMagnitude<cloud.radius*cloud.radius)return true;}return false;
        }
    }
}
