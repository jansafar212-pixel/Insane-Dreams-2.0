using UnityEngine;
namespace InsaneDreams.Combat
{
    // Event-driven, component-independent storage: no Update or allocation.
    public struct SuppressionMeter
    {
        private float level,lastShot;
        public float Sample(float now,float decayPerSecond,float quietSeconds) =>
            Mathf.Max(0,level-Mathf.Max(0,now-lastShot-Mathf.Max(0,quietSeconds))*Mathf.Max(0,decayPerSecond));
        public float Add(float amount,float now,float maximum,float decayPerSecond,float quietSeconds)
        {
            level=Mathf.Clamp(Sample(now,decayPerSecond,quietSeconds)+Mathf.Max(0,amount),0,Mathf.Max(0,maximum));
            lastShot=now;return level;
        }
    }
}
