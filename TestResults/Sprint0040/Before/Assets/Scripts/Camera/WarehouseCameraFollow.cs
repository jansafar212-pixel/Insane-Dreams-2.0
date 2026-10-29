using InsaneDreams.Input;
using InsaneDreams.Traversal;
using UnityEngine;

namespace InsaneDreams.CameraSystem
{
    [RequireComponent(typeof(Camera)), DefaultExecutionOrder(150)]
    public sealed class WarehouseCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 levelMin=new Vector2(-28,-9),levelMax=new Vector2(28,20);
        [SerializeField] private Vector2 offset=new Vector2(0,1.2f);
        [Header("Tracking / attention (seconds to approximately 95% settled)")]
        [SerializeField,Min(.01f)] private float smoothTime=.16f;
        [SerializeField,Min(.01f)] private float combatSmoothTime=.11f;
        [SerializeField,Min(.01f)] private float directionTransitionSeconds=.5f;
        [SerializeField,Min(0)] private float directionCommitSeconds=.12f;
        [SerializeField,Min(0)] private float combatAttentionSeconds=1;
        [SerializeField,Range(.1f,.8f)] private float aimDeadZone=.3f;
        [SerializeField,Range(0,.3f)] private float horizontalLookAhead=1f/6;
        [SerializeField,Min(.01f)] private float ladderRecenterSeconds=.4f;
        [Header("Vertical / comfort")]
        [SerializeField,Min(.01f)] private float verticalSmoothTime=.24f;
        [SerializeField,Min(0)] private float verticalDeadZone=.25f;
        [SerializeField,Min(0)] private float fallingLookAhead=1.6f;
        [SerializeField,Min(0)] private float verticalAimLookAhead=1.2f;
        [SerializeField,Min(1)] private float fallingLeadSpeed=14;
        [SerializeField] private Vector2 playerScreenMargins=new Vector2(.18f,.18f);
        [SerializeField,Min(.1f)] private float boundaryEaseDistance=2;
        [SerializeField,Min(.5f)] private float closeThreatRadius=6;
        [SerializeField,Range(0,.6f)] private float closeThreatLeadReduction=.45f;
        [Header("Existing pixel-art projection")]
        [SerializeField,Min(1)] private float orthographicSize=7.2f;
        [SerializeField,Range(1,1.5f)] private float gameplayZoomMultiplier=1.155f;
        [SerializeField] private Rect gameplayViewport=new Rect(0,0,1,1);
        [SerializeField,Min(0)] private int visualPixelsPerUnit=32;
        [SerializeField,Range(1,1.25f)] private float androidCharacterScale=1.125f;
        [SerializeField] private bool previewAndroidFraming;
        private Camera view;
        private PlayerActions actions;
        private PlayerTraversal2D traversal;
        private Rigidbody2D body;
        private Vector3 followPosition,velocity;
        private Vector2 lastTargetPosition;
        private float clock,facing=1,pendingFacing=1,pendingSince,combatDirection=1,combatUntil,combatVertical;
        private float framing=1,framingVelocity,ladderUntil,fallLead,fallLeadVelocity,nextThreatSample,pressure;
        private bool meaningfulAim;
        private float shakeUntil,shakeDuration,shakeStrength;
        public Transform Target=>target;
        public float AttentionDirection=>facing;
        public bool CombatAttentionActive=>clock<combatUntil;
        public float NearbyThreatPressure=>pressure;
        public float DesiredPlayerViewportX=>gameplayViewport.center.x-framing*gameplayViewport.width*Mathf.Min(horizontalLookAhead,.5f-Mathf.Clamp(playerScreenMargins.x,.05f,.4f));
        public float ShakeMagnitude=>shakeStrength*Mathf.Clamp01((shakeUntil-Time.time)/Mathf.Max(.01f,shakeDuration));
        public void AddExplosionShake(Vector2 origin,float amplitude,float seconds,float range)
        {
            if(!isActiveAndEnabled||range<=0||seconds<=0)return;
            float weight=1-Vector2.Distance(target!=null?(Vector2)target.position:(Vector2)transform.position,origin)/range;if(weight<=0)return;
            shakeStrength=Mathf.Max(ShakeMagnitude,Mathf.Clamp(amplitude,0,.12f)*weight);shakeDuration=Mathf.Clamp(seconds,.01f,.25f);shakeUntil=Time.time+shakeDuration;
        }
        private void Awake()=>view=GetComponent<Camera>();
        private void Start(){if(target!=null&&actions==null)SnapToTarget();}
        public void Bind(Transform value){target=value;if(target!=null)SnapToTarget();}
        // Spawn/checkpoint/replay only. Ordinary follow never resets its damping state.
        public void SnapToTarget()
        {
            if(target==null)return;if(view==null)view=GetComponent<Camera>();
            actions=target.GetComponent<PlayerActions>();traversal=target.GetComponent<PlayerTraversal2D>();body=target.GetComponent<Rigidbody2D>();
            var visual=target.GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>();
            facing=pendingFacing=combatDirection=visual!=null?visual.Facing:1;
            clock=pendingSince=combatVertical=0;combatUntil=ladderUntil=-1;meaningfulAim=false;
            framing=traversal!=null&&traversal.IsClimbing?0:facing;
            framingVelocity=fallLead=fallLeadVelocity=pressure=nextThreatSample=0;velocity=Vector3.zero;shakeUntil=shakeStrength=0;
            lastTargetPosition=target.position;Projection();followPosition=DesiredPosition(target.position,0);transform.position=followPosition;
        }
        private void Projection()
        {
            view.orthographic=true;Vector2 size=levelMax-levelMin;
            float aspect=Mathf.Max(.1f,view.aspect),maximum=Mathf.Max(.1f,Mathf.Min(size.y*.5f,size.x/(2*aspect)));
            float zoomed=Mathf.Min(orthographicSize,maximum);
            if(visualPixelsPerUnit>0&&view.pixelHeight>0)
            {
                int zoom=Mathf.Max(1,Mathf.RoundToInt(view.pixelHeight/(2*zoomed*visualPixelsPerUnit)));
                zoom=Mathf.Max(zoom,Mathf.CeilToInt(view.pixelHeight/(2*maximum*visualPixelsPerUnit)));
                zoomed=view.pixelHeight/(2f*visualPixelsPerUnit*zoom);
            }
            zoomed/=Mathf.Clamp(gameplayZoomMultiplier,1,1.5f);
            if(Application.platform==RuntimePlatform.Android||Application.isEditor&&previewAndroidFraming)zoomed/=Mathf.Clamp(androidCharacterScale,1,1.25f);
            if(view.orthographicSize!=zoomed)view.orthographicSize=zoomed;
        }
        // Exact critically damped solution for a held target. Unlike a polynomial
        // SmoothDamp approximation, the state remains consistent at variable dt.
        private static float Damp(float current,float desired,ref float speed,float settle,float dt)
        {
            float omega=4.744f/Mathf.Max(.01f,settle),difference=current-desired;
            float decay=Mathf.Exp(-omega*dt),step=(speed+omega*difference)*dt;
            speed=(speed-omega*step)*decay;return desired+(difference+step)*decay;
        }
        private float Attention(float dt)
        {
            float strength=actions!=null?actions.AimIntentStrength:0;
            meaningfulAim=actions!=null&&actions.HasAimIntent&&strength>=(meaningfulAim?aimDeadZone*.8f:aimDeadZone);
            bool combat=meaningfulAim||actions!=null&&actions.FireHeld;
            if(combat&&clock>=combatUntil){combatDirection=facing;combatVertical=0;}
            float move=actions!=null?actions.Move.x:(target.position.x-lastTargetPosition.x)/Mathf.Max(.001f,dt);
            float candidate=combat?(Mathf.Abs(actions.AimDirection.x)<.12f?0:actions.AimDirection.x):Mathf.Abs(move)>.15f?Mathf.Sign(move):facing;
            if(!combat&&clock<combatUntil)return combatDirection;
            float sign=Mathf.Abs(candidate)<.01f?0:Mathf.Sign(candidate);
            if(sign!=pendingFacing){pendingFacing=sign;pendingSince=clock;}
            if(clock-pendingSince>=directionCommitSeconds)
            {
                if(sign!=0)facing=sign;
                if(combat){combatDirection=candidate;combatVertical=actions.AimDirection.y;}
            }
            if(combat){combatUntil=clock+combatAttentionSeconds;return combatDirection;}
            return facing;
        }
        private static float SoftBound(float value,float low,float high,float width)
        {
            width=Mathf.Min(Mathf.Max(.01f,width),(high-low)*.5f);value=Mathf.Clamp(value,low,high);
            if(width<=0)return value;
            if(value<low+width){float t=(value-low)/width;return low+width*t*t*(2-t);}
            if(value>high-width){float t=(high-value)/width;return high-width*t*t*(2-t);}
            return value;
        }
        private Vector3 DesiredPosition(Vector2 at,float downwardLead)
        {
            float halfY=view.orthographicSize,halfX=halfY*view.aspect;
            float x=at.x+offset.x+(.5f-DesiredPlayerViewportX)*2*halfX;
            float y=at.y+offset.y+downwardLead;
            return new Vector3(SoftBound(x,levelMin.x+halfX,levelMax.x-halfX,boundaryEaseDistance),SoftBound(y,levelMin.y+halfY,levelMax.y-halfY,boundaryEaseDistance),-10);
        }
        private void LateUpdate()=>StepCamera(Time.deltaTime);
        private void StepCamera(float dt)
        {
            if(target==null||dt<=0)return;
            clock+=dt;Projection();Vector2 at=target.position;float attention=Attention(dt);
            if(clock>=nextThreatSample){nextThreatSample=clock+.15f;pressure=InsaneDreams.Enemies.CombatPositionReservations.LocalThreatPressure(at,closeThreatRadius);}
            bool climbing=traversal!=null&&traversal.IsClimbing;if(climbing)ladderUntil=clock+.12f;
            bool centered=climbing||clock<ladderUntil;
            framing=Damp(framing,centered?0:attention*(1-closeThreatLeadReduction*pressure),ref framingVelocity,centered?ladderRecenterSeconds:directionTransitionSeconds,dt);
            float descent=body!=null?Mathf.Max(0,-body.linearVelocity.y):Mathf.Max(0,(lastTargetPosition.y-at.y)/dt);
            float verticalBias=-fallingLookAhead*Mathf.Clamp01(descent/fallingLeadSpeed)+(CombatAttentionActive&&!centered?combatVertical*verticalAimLookAhead:0);
            fallLead=Damp(fallLead,verticalBias,ref fallLeadVelocity,verticalSmoothTime,dt);
            var desired=DesiredPosition(at,fallLead);float halfY=view.orthographicSize,halfX=halfY*view.aspect;
            float low=levelMin.x+halfX,high=levelMax.x-halfX;
            // Retain responsive interpolated travel without speed-dependent camera
            // lag. Fade this feed-forward before a boundary, so it can ease to rest.
            float edge=Mathf.Max(0,Mathf.Min(followPosition.x-low,high-followPosition.x));
            followPosition.x+=(at.x-lastTargetPosition.x)*Mathf.SmoothStep(0,1,edge/boundaryEaseDistance);
            float horizontalTime=CombatAttentionActive?combatSmoothTime:smoothTime;
            float horizontalComfort=halfX*(1-2*Mathf.Clamp(playerScreenMargins.x,.05f,.4f));
            if(Mathf.Abs(at.x-followPosition.x)>horizontalComfort)horizontalTime=Mathf.Min(horizontalTime,combatSmoothTime);
            followPosition.x=Damp(followPosition.x,desired.x,ref velocity.x,horizontalTime,dt);
            float error=desired.y-followPosition.y;
            float verticalTarget=followPosition.y+Mathf.Sign(error)*Mathf.Max(0,Mathf.Abs(error)-verticalDeadZone);
            float verticalTime=Mathf.Abs(at.y-followPosition.y)>halfY*(1-2*Mathf.Clamp(playerScreenMargins.y,.05f,.4f))?combatSmoothTime:verticalSmoothTime;
            followPosition.y=Damp(followPosition.y,verticalTarget,ref velocity.y,verticalTime,dt);
            float x=Mathf.Clamp(followPosition.x,low,high),y=Mathf.Clamp(followPosition.y,levelMin.y+halfY,levelMax.y-halfY);
            if(x!=followPosition.x)velocity.x=0;if(y!=followPosition.y)velocity.y=0;
            followPosition=new Vector3(x,y,-10);lastTargetPosition=at;
            float shake=ShakeMagnitude;var render=followPosition;if(shake>0)render+=new Vector3(Mathf.Sin(Time.time*76),Mathf.Sin(Time.time*103),0)*shake;
            // Continuous interpolated camera position: never quantize the follow
            // transform to one art pixel or feed rounded output back into damping.
            transform.position=render;
        }
    }
}
