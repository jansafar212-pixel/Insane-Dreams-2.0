using UnityEngine;
namespace InsaneDreams.Combat
{
    // Accepted damage only. Four bounded direction pulses and one edge pulse;
    // no full-screen material, spawned indicators or input/animation ownership.
    [RequireComponent(typeof(Health))]
    public sealed class PlayerDamageFeedback : MonoBehaviour
    {
        [SerializeField,Min(.05f)] private float indicatorSeconds=.55f,edgeSeconds=.32f,healthFlashSeconds=.13f;
        [SerializeField,Range(0,.3f)] private float maximumEdgeAlpha=.18f;
        [SerializeField,Range(0,1)] private float maximumIndicatorAlpha=.85f;
        [SerializeField,Min(1)] private float seriousDamage=18;
        [SerializeField,Range(0,.05f)] private float cameraShakeStrength=.025f;
        [SerializeField,Range(0,1)] private float cameraShakeScale=1;
        [SerializeField,Range(.02f,.15f)] private float cameraShakeSeconds=.09f;
        [SerializeField,Min(.2f)] private float cameraShakeCooldown=.65f;
        private Health health;
        private CameraSystem.WarehouseCameraFollow cameraFollow;
        private readonly float[] until=new float[4],intensity=new float[4];
        private float edgeUntil,edgeIntensity,flashUntil,nextShake;
        public uint Revision {get;private set;}
        public float LastSeverity {get;private set;}
        public int LastDirection {get;private set;}=-1; // left,right,above,below
        public int CameraImpulses {get;private set;}
        public float CameraShakeScale {get=>cameraShakeScale;set=>cameraShakeScale=Mathf.Clamp01(value);}
        public bool HealthFlashActive=>Time.time<flashUntil;
        public float EdgeAlpha=>maximumEdgeAlpha*edgeIntensity*Fade(edgeUntil,edgeSeconds);
        public float IndicatorAlpha(int side)=>maximumIndicatorAlpha*intensity[side]*Fade(until[side],indicatorSeconds);
        private static float Fade(float end,float seconds){float t=Mathf.Clamp01((end-Time.time)/seconds);return t*t*(3-2*t);}
        private void Awake()=>health=GetComponent<Health>();
        private void Start(){var c=Camera.main;if(c!=null)cameraFollow=c.GetComponent<CameraSystem.WarehouseCameraFollow>();}
        public void BindCamera(CameraSystem.WarehouseCameraFollow value)=>cameraFollow=value;
        private void OnEnable(){health.DamageResolved+=Hurt;var revive=GetComponent<PlayerRevive>();if(revive!=null)revive.Revived+=Clear;var recovery=GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>();if(recovery!=null)recovery.Recovered+=Clear;}
        private void OnDisable(){health.DamageResolved-=Hurt;var revive=GetComponent<PlayerRevive>();if(revive!=null)revive.Revived-=Clear;var recovery=GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>();if(recovery!=null)recovery.Recovered-=Clear;Clear();}
        public void Clear(){edgeUntil=flashUntil=0;for(int i=0;i<4;i++)until[i]=intensity[i]=0;LastSeverity=0;LastDirection=-1;Revision++;}
        private void Hurt(DamageOutcome outcome)
        {
            if(outcome.HealthLost<=0&&outcome.ArmorAbsorbed<=0)return;
            var hit=outcome.Hit;
            LastSeverity=Mathf.Clamp01((outcome.HealthLost+outcome.ArmorAbsorbed*.12f)/Mathf.Max(1,health.Maximum*.35f));
            float pulse=Mathf.Lerp(.25f,1,LastSeverity);
            edgeIntensity=Mathf.Max(EdgeAlpha/Mathf.Max(.001f,maximumEdgeAlpha),pulse);edgeUntil=Time.time+edgeSeconds;
            flashUntil=Time.time+healthFlashSeconds;Revision++;
            LastDirection=-1;
            bool directional=hit.Kind!=DamageKind.Environment&&!(hit.Team==CombatTeam.Neutral&&hit.Source==null);
            Vector2 source=Vector2.zero;
            if(directional&&hit.Impact.HasValue&&hit.Impact.Value.IsValid)source=-hit.Impact.Value.Direction;
            else if(directional&&hit.Source!=null)source=(Vector2)(hit.Source.transform.position-transform.position);
            if(source.sqrMagnitude>.01f)
            {
                int side=Mathf.Abs(source.x)>=Mathf.Abs(source.y)?(source.x<0?0:1):(source.y>0?2:3);
                intensity[side]=Mathf.Max(IndicatorAlpha(side)/Mathf.Max(.001f,maximumIndicatorAlpha),Mathf.Lerp(.45f,1,LastSeverity));until[side]=Time.time+indicatorSeconds;LastDirection=side;
            }
            if(outcome.HealthLost>=seriousDamage&&cameraShakeScale>0&&Time.time>=nextShake&&cameraFollow!=null&&cameraFollow.Target==transform)
            {cameraFollow.AddExplosionShake(transform.position,cameraShakeStrength*cameraShakeScale, cameraShakeSeconds,1);nextShake=Time.time+cameraShakeCooldown;CameraImpulses++;}
        }
    }
}
