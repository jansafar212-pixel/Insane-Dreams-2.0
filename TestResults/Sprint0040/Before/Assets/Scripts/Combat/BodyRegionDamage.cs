using InsaneDreams.Visuals;
using UnityEngine;
namespace InsaneDreams.Combat
{
    // Accepted hit locations accumulate independently of weapon identity. Living
    // animation stays intact; the existing corpse receives these regional totals.
    [DefaultExecutionOrder(-80)]
    public sealed class BodyRegionDamage : MonoBehaviour
    {
        [SerializeField,Min(1)] private float headThreshold=80,limbThreshold=60,torsoThreshold=150;
        [SerializeField,Min(1)] private float armThreshold=105;
        private readonly float[] damage=new float[6];private SpriteRenderer[] renderers;private int[] regions;private Health health;
        public float DamageAt(int region)=>damage[region];
        public float Threshold(int region)=>region==0?torsoThreshold:region==1?headThreshold:region==2||region==3?armThreshold:limbThreshold;
        private void Awake()
        {
            health=GetComponent<Health>();var rig=GetComponentInChildren<CutoutRig>(true);if(rig==null)return;var baseParts=rig.BodyParts;var extra=rig.Segmented!=null?rig.Segmented.ExtraParts:null;renderers=new SpriteRenderer[extra!=null?14:6];regions=new int[renderers.Length];for(int i=0;i<6;i++){renderers[i]=baseParts[i].GetComponent<SpriteRenderer>();regions[i]=i;}if(extra!=null)for(int i=0;i<8;i++){renderers[i+6]=extra[i].GetComponent<SpriteRenderer>();regions[i+6]=2+i/2;}
        }
        private void OnEnable(){Clear();health.Damaged+=Hit;var recovery=GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>();if(recovery!=null)recovery.Recovered+=Clear;var revive=GetComponent<PlayerRevive>();if(revive!=null)revive.Revived+=Clear;}
        private void OnDisable(){health.Damaged-=Hit;var recovery=GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>();if(recovery!=null)recovery.Recovered-=Clear;var revive=GetComponent<PlayerRevive>();if(revive!=null)revive.Revived-=Clear;}
        public void Clear(){for(int i=0;i<6;i++)damage[i]=0;}
        private void Hit(DamageInfo hit)
        {
            if(!hit.Impact.HasValue||renderers==null)return;damage[RegionAt(hit.Impact.Value.Point)]+=hit.Amount;
        }
        public int RegionAt(Vector2 point)
        {
            if(renderers==null)return 0;float best=float.PositiveInfinity;int region=0;
            for(int i=0;i<renderers.Length;i++){var b=renderers[i].bounds;float distance=((Vector2)b.ClosestPoint(point)-point).sqrMagnitude+((Vector2)b.center-point).sqrMagnitude*.001f;if(distance<best){best=distance;region=regions[i];}}
            return region;
        }
    }
}
