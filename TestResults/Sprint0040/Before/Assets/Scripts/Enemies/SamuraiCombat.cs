using InsaneDreams.Combat;
using InsaneDreams.Prototype;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    [RequireComponent(typeof(EnemyCombat))]
    public sealed class SamuraiCombat : MonoBehaviour
    {
        [SerializeField,Min(.1f)] private float windupSeconds=.4f,recoverySeconds=.7f,attackDistance=1.65f;
        [Header("Platform pursuit")]
        [SerializeField] private LayerMask platforms=1;
        [SerializeField,Min(.1f)] private float jumpSampleSeconds=.3f,jumpCooldown=1.2f,failedJumpCooldown=3;
        [SerializeField,Min(1)] private float jumpSearchDistance=6;
        [SerializeField,Min(1)] private float maximumDrop=6.5f;
        [SerializeField,Min(6)] private float doubleJumpReach=8.5f;
        [SerializeField,Range(.15f,.4f)] private float secondJumpDelay=.32f;
        [SerializeField] private bool drawNavigation;
        private readonly SamuraiPlatformRoutes routes=new SamuraiPlatformRoutes();
        private Collider2D routeTarget,flightFrom,flightTo;
        private Collider2D approachTarget,ladderFrom,ladderTo;private AuthoredLadderRoute ladderRoute;private bool ladderAttempt;
        private float secondAt,flightBegan,approachSince,approachProgress;private bool secondRequested;
        public bool PlannedDoubleJump=>secondAt>0;
        public SamuraiTransition Transition=>flying?(PlannedDoubleJump?SamuraiTransition.DoubleJump:SamuraiTransition.SingleJump):routes.Kind;
        public string FailedTransitionReason=>routes.LastFailure;
        public Vector2 PlannedLaunch=>launch;
        private EnemyCombat combat;private LarperBrain brain;private PlayerMotor2D motor;private AttackExecutor executor;private Health health;
        private float strikeAt,nextAttack,lastTick;private bool winding;
        private float swingUntil;
        private Rigidbody2D body;private BoxCollider2D box;
        private readonly Collider2D[] surfaces=new Collider2D[32];
        private readonly RaycastHit2D[] hits=new RaycastHit2D[8];
        private readonly Vector2[] candidates=new Vector2[4];
        private readonly float[] scores=new float[4];
        private float nextJumpSample,nextJump,flightUntil;private Vector2 landing,launch,failedOrigin;private float flightInput;private bool flying,leftGround,failedHere;
        private float backoffUntil,backoffDirection,failedRetryAt;private bool usedBackoff;private Vector2 backoffFrom;
        private bool approachLedge;private float approachX;
        public bool PlatformJumpActive=>flying;
        public int PlatformJumps{get;private set;}
        public Vector2 PlannedLanding=>landing;
        public float SwingAngle=>Time.time<swingUntil?Mathf.Sin(Mathf.PI*Mathf.Clamp01((swingUntil-Time.time)/.18f))*90:0;
        public bool Winding=>winding;
        public bool StrikePoseActive=>winding||Time.time<swingUntil;
        public float BladeAngle
        {
            get
            {
                if(winding)return Mathf.Lerp(15,65,Mathf.SmoothStep(0,1,1-(strikeAt-Time.time)/windupSeconds));
                if(Time.time<swingUntil)return Mathf.Lerp(65,-35,1-(swingUntil-Time.time)/.18f);
                if(swingUntil>0&&Time.time<nextAttack)return Mathf.Lerp(-35,15,Mathf.SmoothStep(0,1,(Time.time-swingUntil)/Mathf.Max(.1f,recoverySeconds-.18f)));
                return 15;
            }
        }
        public Vector2 CommittedAim{get;private set;}=Vector2.right;
        public int Strikes{get;private set;}
        public void ReconsiderBlockedPath()
        {
            if(winding||flying||!motor.IsGrounded)return;
            routes.Fail(routes.Source,routeTarget,3,"No approach progress");
            approachLedge=failedHere=false;nextJump=nextJumpSample=Time.time+.35f;
            float away=-Mathf.Sign(combat.AimDirection.x);
            if(brain.CanWithdraw(away)&&combat.CanAdvance(away)){backoffDirection=away;backoffUntil=Time.time+.35f;}
        }
        private void Awake(){combat=GetComponent<EnemyCombat>();brain=GetComponent<LarperBrain>();motor=GetComponent<PlayerMotor2D>();executor=GetComponent<AttackExecutor>();health=GetComponent<Health>();body=GetComponent<Rigidbody2D>();box=GetComponent<BoxCollider2D>();ladderRoute=GetComponent<AuthoredLadderRoute>();}
        private void OnEnable(){nextAttack=Time.time+.05f;nextJump=nextJumpSample=backoffUntil=failedRetryAt=0;ladderAttempt=approachLedge=failedHere=usedBackoff=flying=winding=false;approachTarget=null;routes.Reset();health.Damaged+=Hurt;}
        private void Hurt(DamageInfo _){if(flying)routes.Fail(flightFrom,flightTo,3,"Hit interrupted traversal");winding=false;flying=false;backoffUntil=0;nextAttack=Mathf.Max(nextAttack,Time.time+.4f);}
        private void OnDisable(){health.Damaged-=Hurt;winding=flying=false;backoffUntil=0;}
        public EngagementState Tick(Health target,Vector2 remembered,bool visible,bool cameraAllowed,bool ready)
        {
            // Brief perception/reaction gaps must not discard an in-flight plan.
            // Its existing timeout, landing and damage gates still terminate it.
            if(Time.time-lastTick>.15f){winding=false;backoffUntil=0;}lastTick=Time.time;
            Vector2 delta=remembered-(Vector2)transform.position;brain.DecisionsSuspended=true;
            if(ladderAttempt&&ladderRoute!=null&&!ladderRoute.IsRunning)
            {if(ladderRoute.State==LadderRouteState.Unavailable)routes.Fail(ladderFrom,ladderTo,8,"Ladder unavailable or timed out");ladderAttempt=false;}
            if(target==null||target.IsDefeated){winding=flying=false;backoffUntil=0;brain.SetPursuit(0);return EngagementState.Searching;}
            if(combat.IsDefensivelyCrouching){winding=false;backoffUntil=0;brain.SetPursuit(0);motor.SetMoveInput(0);return EngagementState.Reacting;}
            if(!ready&&!flying){brain.SetPursuit(0);motor.SetMoveInput(0);return EngagementState.Reacting;}
            if(Time.time<backoffUntil)
            {brain.ClearPursuit();motor.SetMoveInput(backoffDirection*.7f);return EngagementState.Approaching;}
            if(backoffUntil>0){backoffUntil=0;motor.SetMoveInput(0);}
            // Shared combat gates still own suppression, knockdown, death and morale.
            // This method never runs an independent movement update behind those gates.
            if(flying)
            {
                if(!leftGround&&!motor.IsGrounded)flightBegan=Time.time;
                leftGround|=!motor.IsGrounded;
                if(leftGround&&motor.IsGrounded||Time.time>=flightUntil)
                {
                    bool success=Vector2.Distance(body.position,landing)<.7f;
                    flying=false;nextJump=Time.time+(success?jumpCooldown:failedJumpCooldown);
                    if(!success){failedHere=true;failedOrigin=launch;failedRetryAt=Time.time+Mathf.Max(5,failedJumpCooldown);routes.Fail(flightFrom,flightTo,5,"Landing interrupted or timed out");}
                    brain.SetPursuit(0);motor.SetMoveInput(0);return EngagementState.Reacting;
                }
                brain.ClearPursuit();
                if(leftGround&&secondAt>0&&!secondRequested&&motor.JumpsUsed==1&&Time.time-flightBegan>=secondAt)
                {motor.RequestJump();secondRequested=true;}
                // Brake at the committed landing; never retarget the flight to a moving player.
                motor.SetMoveInput((landing.x-body.position.x)*Mathf.Sign(flightInput)<.12f?0:flightInput);
                return EngagementState.Approaching;
            }
            if(winding)
            {
                brain.SetPursuit(0);
                if(!cameraAllowed){winding=false;nextAttack=Time.time+recoverySeconds;return EngagementState.Reacting;}
                if(Time.time>=strikeAt){executor.Attack(combat.Weapon,CommittedAim,Time.time);Strikes++;swingUntil=Time.time+.18f;winding=false;nextAttack=Time.time+recoverySeconds;}
                return EngagementState.Reacting;
            }
            // Recovery limits the next strike, not pursuit of an escaping target.
            if(Time.time<nextAttack&&delta.magnitude<=attackDistance&&Mathf.Abs(delta.y)<=.9f){brain.SetPursuit(0);return EngagementState.Reacting;}
            if(delta.magnitude>attackDistance||Mathf.Abs(delta.y)>.9f)
            {
                float direction=Mathf.Sign(delta.x);
                if(!combat.CanAdvance(direction)){brain.SetPursuit(0);return EngagementState.Approaching;}
                if(motor.IsGrounded&&ready&&TryPlatformJump(remembered))return EngagementState.Approaching;
                // Pursue only the supplied last-seen position, never the hidden live target.
                float travel=approachLedge&&motor.IsGrounded?approachX-body.position.x:delta.x;
                direction=Mathf.Sign(travel);
                brain.SetPursuit(Mathf.Abs(travel)>.15f&&combat.CanAdvance(direction)?direction:0,false);return EngagementState.Approaching;
            }
            brain.SetPursuit(0);
            if(Time.time<nextAttack||!visible||!ready||!cameraAllowed||!motor.IsGrounded)return EngagementState.Reacting;
            CommittedAim=delta.sqrMagnitude>.01f?delta.normalized:Vector2.right;winding=true;strikeAt=Time.time+windupSeconds;return EngagementState.Engaging;
        }

        private ContactFilter2D Filter(){var f=new ContactFilter2D{useTriggers=false};f.SetLayerMask(platforms);return f;}
        private bool TryPlatformJump(Vector2 focus)
        {
            if(Time.time<nextJumpSample||Time.time<nextJump)return false;
            nextJumpSample=Time.time+Mathf.Max(.1f,jumpSampleSeconds);
            approachLedge=false;
            bool stationaryFailure=failedHere&&Time.time<failedRetryAt&&Vector2.Distance(body.position,failedOrigin)<.8f;
            if(!stationaryFailure)failedHere=false;
            float g=-Physics2D.gravity.y*body.gravityScale;
            if(g<=0||motor.MoveSpeed<=0)return false;
            float v=motor.JumpSpeed-g*Time.fixedDeltaTime*.5f,apex=v*v/(2*g);
            Vector2 start=box.bounds.center;float half=box.bounds.extents.y;
            float secondV=motor.AirJumpSpeed-g*Time.fixedDeltaTime*.5f;
            float doubleRise=v*secondJumpDelay-.5f*g*secondJumpDelay*secondJumpDelay+secondV*secondV/(2*g);
            bool routed=routes.Select(start,focus,half,platforms,apex-.1f,doubleRise-.1f,doubleJumpReach,maximumDrop);
            routeTarget=routed?routes.Destination:null;
            if(stationaryFailure&&(routeTarget==null||routeTarget==flightTo))return false;
            if(routed&&routes.Kind==SamuraiTransition.Ladder)
            {
                if(ladderRoute!=null&&routes.Ladder!=null)
                {brain.ClearPursuit();ladderFrom=routes.Source;ladderTo=routeTarget;ladderAttempt=true;ladderRoute.ConfigurePlatform(routes.Ladder,routeTarget.bounds.max.y+half,Mathf.Sign(routeTarget.bounds.center.x-routes.Ladder.CenterX));return true;}
            }
            if(routeTarget!=null)focus=new Vector2(Mathf.Clamp(focus.x,routeTarget.bounds.min.x+.55f,routeTarget.bounds.max.x-.55f),routeTarget.bounds.max.y+half+.06f);
            bool hopRight=NeedsHop(start,half,1),hopLeft=NeedsHop(start,half,-1);
            int count=Physics2D.OverlapBox(start+Vector2.up*(doubleRise-maximumDrop)*.5f,new Vector2(doubleJumpReach*2,doubleRise+maximumDrop+2),0,Filter(),surfaces);
            if(count==surfaces.Length)return false; // Truncated geometry is unsafe.
            for(int i=0;i<scores.Length;i++)scores[i]=float.PositiveInfinity;
            float currentCost=Mathf.Abs(focus.x-start.x)+Mathf.Abs(focus.y-start.y)*2;
            for(int i=0;i<count;i++)
            {
                var c=surfaces[i];if(c.transform.IsChildOf(transform))continue;
                if(routeTarget!=null&&c!=routeTarget)continue;
                var b=c.bounds;float margin=box.bounds.extents.x+.1f;
                if(b.size.x<margin*2)continue;
                float y=b.max.y+half+.06f;
                if(y-start.y>doubleRise-.1f||y-start.y< -maximumDrop)continue;
                for(int k=0;k<3;k++)
                {
                    float x=Mathf.Clamp(k==0?focus.x:start.x+(k==1?-3:3),b.min.x+margin,b.max.x-margin);
                    Vector2 end=new Vector2(x,y);float dx=x-start.x,dy=y-start.y;
                    if(Mathf.Abs(dx)<.5f||Mathf.Abs(dx)>doubleJumpReach)continue;
                    // Ordinary same-floor walking needs no jump, except across a gap or low obstacle.
                    if(Mathf.Abs(dy)<.3f&&!(dx>0?hopRight:hopLeft)&&(routeTarget==null||routes.Kind==SamuraiTransition.Walk))continue;
                    if(!Timing(start,end,v,g,false,out _)&&!Timing(start,end,v,g,true,out _))continue;
                    float cost=Mathf.Abs(focus.x-x)+Mathf.Abs(focus.y-y)*2;
                    if(cost>currentCost-.4f)continue;
                    for(int slot=0;slot<4;slot++)if(cost<scores[slot])
                    {for(int j=3;j>slot;j--){scores[j]=scores[j-1];candidates[j]=candidates[j-1];}scores[slot]=cost;candidates[slot]=end;break;}
                }
            }
            for(int i=0;i<4;i++)
            {
                if(float.IsPositiveInfinity(scores[i]))break;
                Vector2 end=candidates[i];
                if(!CombatPositionReservations.IsFree(combat,end))continue;
                if(!ValidatedFlight(start,end,v,g,out float t,out float extra))continue;
                landing=end;launch=body.position;flightUntil=Time.time+t+.5f;usedBackoff=false;
                secondAt=extra;secondRequested=false;flightFrom=routes.Source;flightTo=routeTarget;flightBegan=Time.time;
                flightInput=(end.x-start.x)/(motor.MoveSpeed*(t-.06f));flying=true;leftGround=false;PlatformJumps++;
                brain.ClearPursuit();motor.SetMoveInput(flightInput);motor.RequestJump();return true;
            }
            if(routeTarget!=null&&FindLaunch(start,focus,half,v,g,out float requiredX))
            {
                if(approachTarget!=routeTarget){approachTarget=routeTarget;approachSince=Time.time;approachProgress=body.position.x;}
                approachLedge=true;approachX=requiredX;
                if(Mathf.Abs(body.position.x-approachProgress)>.2f){approachProgress=body.position.x;approachSince=Time.time;}
                else if(Time.time-approachSince>2){routes.Fail(routes.Source,routeTarget,5,"Launch position obstructed");approachLedge=false;approachSince=Time.time;}
                return false;
            }
            // If contact brought us under a ledge before the next tactical sample,
            // create launch clearance once. Never bounce backward repeatedly at an impossible wall.
            if(usedBackoff&&Vector2.Distance(backoffFrom,body.position)>3)usedBackoff=false;
            float direction=Mathf.Sign(focus.x-start.x);
            // A body-sized short Samurai can fit below a floating step without either
            // horizontal body ray touching it. The route already identifies that ledge.
            bool overhang=routeTarget!=null&&routeTarget.bounds.min.y>box.bounds.max.y&&
                routeTarget.bounds.min.x<box.bounds.max.x+.7f&&routeTarget.bounds.max.x>box.bounds.min.x-.7f;
            float backoffDistance=overhang?1.8f:1.2f;
            if(!usedBackoff&&!float.IsPositiveInfinity(scores[0])&&combat.CanAdvance(-direction)&&
                (overhang||Physics2D.Raycast(start+Vector2.right*direction*(box.bounds.extents.x+.05f),Vector2.right*direction,.7f,platforms)||
                 Physics2D.Raycast(start+Vector2.up*(half+.1f)+Vector2.right*direction*(box.bounds.extents.x+.05f),Vector2.right*direction,.7f,platforms))&&
                Physics2D.Raycast(new Vector2(start.x-direction*backoffDistance,start.y-half+.15f),Vector2.down,.5f,platforms)&&
                !Physics2D.BoxCast(start,(Vector2)box.bounds.size-Vector2.one*.1f,0,Vector2.left*direction,backoffDistance,platforms))
            {
                usedBackoff=true;backoffFrom=body.position;backoffDirection=-direction;backoffUntil=Time.time+(overhang?.38f:.25f);
                brain.ClearPursuit();motor.SetMoveInput(backoffDirection*.7f);return true;
            }
            // Low crates can leave the player's body centre less than .9 below us.
            // Reuse the swept walk-off rather than retrying a tall arc into the ceiling.
            if(focus.y<start.y-.3f&&FindDescendingEdge(start,half,focus,out var dropLanding))
            {
                // The old inside-edge waypoint stopped before leaving the platform;
                // generic pursuit then refused the ledge. Commit a validated physical
                // walk-off using the same finite landing/recovery bookkeeping as jumps.
                landing=dropLanding;launch=body.position;
                flightUntil=Time.time+Mathf.Abs(landing.x-launch.x)/Mathf.Max(.1f,motor.MoveSpeed*.5f)+Mathf.Sqrt(2*Mathf.Max(0,launch.y-landing.y)/g)+.5f;
                flightFrom=routes.Source;flightTo=routeTarget;secondAt=0;secondRequested=false;
                flightInput=Mathf.Sign(landing.x-body.position.x)*.5f;flying=true;leftGround=false;
                brain.ClearPursuit();motor.SetMoveInput(flightInput);return true;
            }
            if(routeTarget!=null&&!approachLedge){routes.Fail(routes.Source,routeTarget,5,"No clear launch arc");}
            return false;
        }
        private bool Timing(Vector2 start,Vector2 end,float v,float g,bool twice,out float duration)
        {
            float delay=twice?secondJumpDelay:0,height=twice?v*delay-.5f*g*delay*delay:0,speed=twice?motor.AirJumpSpeed-g*Time.fixedDeltaTime*.5f:v;
            float discriminant=speed*speed-2*g*(end.y-start.y-height);duration=0;
            if(discriminant<=.05f||end.y-start.y< -maximumDrop)return false;
            duration=delay+(speed+Mathf.Sqrt(discriminant))/g;
            return duration>delay&&Mathf.Abs(end.x-start.x)<=Mathf.Min(twice?doubleJumpReach:jumpSearchDistance,motor.MoveSpeed*(duration-.12f));
        }
        private bool ValidatedFlight(Vector2 start,Vector2 end,float v,float g,out float time,out float extra)
        {
            extra=0;bool single=Timing(start,end,v,g,false,out time);
            if(single&&ArcClear(start,end,time,v,g))return true;
            // For a normal hop, first seek a better launch instead of spending the second impulse to compensate for a poor one.
            if(single&&routes.Kind==SamuraiTransition.SingleJump)return false;
            if(Timing(start,end,v,g,true,out time)&&ArcClear(start,end,time,v,g,secondJumpDelay)){extra=secondJumpDelay;return true;}
            return false;
        }
        private bool FindLaunch(Vector2 start,Vector2 goal,float half,float v,float g,out float x)
        {
            x=0;if(routes.Source==null||routeTarget==null)return false;
            var source=routes.Source.bounds;var target=routeTarget.bounds;float margin=box.bounds.extents.x+.1f;
            float best=float.PositiveInfinity;
            for(int i=0;i<4;i++)
            {
                float proposed=i==0?target.min.x-margin:i==1?target.max.x+margin:i==2?Mathf.Clamp(goal.x,start.x-3,start.x+3):start.x+Mathf.Sign(goal.x-start.x)*.35f;
                proposed=Mathf.Clamp(proposed,source.min.x+margin,source.max.x-margin);
                if(Mathf.Abs(proposed-start.x)<.2f)continue;
                Vector2 origin=new Vector2(proposed,source.max.y+half+.01f);
                Vector2 end=new Vector2(Mathf.Clamp(goal.x,target.min.x+margin,target.max.x-margin),target.max.y+half+.06f);
                if(!CombatPositionReservations.IsFree(combat,end)||!ValidatedFlight(origin,end,v,g,out _,out _))continue;
                float distance=Mathf.Abs(proposed-start.x);if(distance<best){best=distance;x=proposed;}
            }
            return !float.IsPositiveInfinity(best);
        }
        private bool FindDescendingEdge(Vector2 start,float half,Vector2 focus,out Vector2 point)
        {
            point=default;var support=Physics2D.Raycast(start+Vector2.down*(half-.15f),Vector2.down,.4f,platforms);
            if(!support||support.normal.y<.9f)return false;
            var b=support.collider.bounds;float margin=box.bounds.extents.x+.2f,best=float.PositiveInfinity;
            for(int i=0;i<2;i++)
            {
                float sign=i==0?-1:1,edge=i==0?b.min.x:b.max.x,inside=edge-sign*margin,outside=edge+sign*margin;
                if(Mathf.Abs(inside-start.x)>jumpSearchDistance)continue;
                var floor=Physics2D.Raycast(new Vector2(outside,start.y-half+.15f),Vector2.down,maximumDrop+.2f,platforms);
                if(!floor||floor.collider==support.collider||floor.normal.y<.9f||Mathf.Abs(floor.point.y+half-focus.y)>1)continue;
                float cost=Mathf.Abs(inside-start.x)+Mathf.Abs(outside-focus.x);
                var destination=new Vector2(outside,floor.point.y+half+.015f);
                if(!CombatPositionReservations.IsFree(combat,destination))continue;
                if(Physics2D.BoxCast(start,(Vector2)box.bounds.size-Vector2.one*.1f,0,Vector2.right*sign,Mathf.Abs(outside-start.x),platforms))continue;
                if(cost<best){best=cost;point=destination;}
            }
            return !float.IsPositiveInfinity(best);
        }
        private bool NeedsHop(Vector2 start,float half,float sign)
        {
            if(Physics2D.Raycast(start+Vector2.right*sign*(box.bounds.extents.x+.05f),Vector2.right*sign,.7f,platforms))return true;
            return !Physics2D.Raycast(new Vector2(start.x+sign*.9f,start.y-half+.15f),Vector2.down,.5f,platforms);
        }
        private bool ArcClear(Vector2 start,Vector2 end,float duration,float v,float g,float extra=0)
        {
            Vector2 previous=start+Vector2.up*.04f;Vector2 size=(Vector2)box.bounds.size-Vector2.one*.1f;
            int steps=extra>0?32:20;
            for(int i=1;i<=steps;i++)
            {
                float t=duration*i/steps;float y=v*t-.5f*g*t*t;
                if(extra>0&&t>extra){float u=t-extra;y=v*extra-.5f*g*extra*extra+(motor.AirJumpSpeed-g*Time.fixedDeltaTime*.5f)*u-.5f*g*u*u;}
                Vector2 p=new Vector2(Mathf.Lerp(start.x,end.x,(float)i/steps),start.y+y);
                Vector2 d=p-previous;int n=Physics2D.BoxCast(previous,size,0,d.normalized,Filter(),hits,d.magnitude);
                if(n==hits.Length)return false;
                for(int j=0;j<n;j++)if(!hits[j].collider.transform.IsChildOf(transform))return false;
                previous=p;
            }
            return true;
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if(!drawNavigation)return;Gizmos.color=Color.cyan;Gizmos.DrawWireSphere(launch,.15f);Gizmos.DrawLine(launch,landing);Gizmos.color=Color.green;Gizmos.DrawWireSphere(landing,.2f);
            UnityEditor.Handles.Label(transform.position+Vector3.up*1.2f,$"{Transition}  jumps {GetComponent<PlayerMotor2D>().JumpsUsed}/2\n{FailedTransitionReason}");
            if(routeTarget!=null)Gizmos.DrawWireCube(routeTarget.bounds.center,routeTarget.bounds.size);
        }
#endif
    }
}
