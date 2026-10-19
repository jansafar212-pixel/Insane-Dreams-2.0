using InsaneDreams.Prototype;
using UnityEngine;

namespace InsaneDreams.Enemies
{
    public enum LocomotionState { Unconfigured, Stationary, Walking, Jumping, Blocked, Falling }
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(PlayerMotor2D), typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class LarperBrain : MonoBehaviour
    {
        [SerializeField] private EnemyArchetype archetype;
        [SerializeField] private LayerMask surfaces = 1;
        [SerializeField, Min(.05f)] private float wallProbe = .5f;
        [SerializeField, Min(.05f)] private float ledgeProbe = .45f;
        [SerializeField, Min(.1f)] private float jumpLandingDistance = 4;
        [SerializeField] private LocomotionState state;
        private PlayerMotor2D motor;
        private Rigidbody2D body;
        private BoxCollider2D box;
        private float originX;
        private float direction = 1;
        private float pauseUntil;
        public LocomotionState State => state;
        public EnemyArchetype Archetype => archetype;
        public bool DecisionsSuspended { get; set; }
        private bool pursuing;
        private float pursuitInput;
        private bool pursuitAllowsJump = true;
        private bool pursuitIsRetreat;
        [SerializeField, Min(.1f)] private float retreatHopHeight = 1.6f;
        [SerializeField, Min(.1f)] private float retreatHopWidth = 1.8f;
        private readonly RaycastHit2D[] retreatHits = new RaycastHit2D[16];
        private readonly RaycastHit2D[] probeHits = new RaycastHit2D[8];
        private readonly Collider2D[] clearanceHits = new Collider2D[16];
        private RaycastHit2D RetreatRaycast(Vector2 origin,Vector2 forward,float distance)
        {
            var filter=new ContactFilter2D {useTriggers=false};filter.SetLayerMask(surfaces);
            int count=Physics2D.Raycast(origin,forward,filter,probeHits,distance);
            return count>0?probeHits[0]:default;
        }
        public void SetPursuit(float input, bool allowJump = true, bool plannedRetreat = false) { pursuing = true; pursuitInput = Mathf.Clamp(input, -1, 1); pursuitAllowsJump = allowJump; pursuitIsRetreat = plannedRetreat; }
        public void ClearPursuit() { pursuing = false; }
        // Fallback withdrawal walks; a validated cover corridor may use low hops.
        public bool CanWithdraw(float sign, bool allowHop = false)
        {
            if (archetype == null || !archetype.Allows(MovementCapabilities.Walk) || archetype.Allows(MovementCapabilities.Stationary)) return false;
            if (!motor.IsGrounded) return allowHop;
            Vector2 forward = Vector2.right * Mathf.Sign(sign);
            Vector2 nose = (Vector2)box.bounds.center + forward * (box.bounds.extents.x + .05f);
            var wall = RetreatRaycast(nose, forward, wallProbe);
            return (!wall || allowHop && RetreatHopSafe(wall.collider, sign)) &&
                RetreatRaycast(nose + forward * ledgeProbe + Vector2.down * (box.bounds.extents.y - .1f), Vector2.down, allowHop ? retreatHopHeight + .6f : .6f);
        }
        private bool RetreatHopSafe(Collider2D obstacle, float sign)
        {
            if (archetype == null || !archetype.Allows(MovementCapabilities.Jump) || obstacle == null) return false;
            var b=obstacle.bounds;float feet=box.bounds.min.y;
            if(b.max.y-feet>retreatHopHeight || b.min.y>feet+.2f || b.size.x>retreatHopWidth)return false;
            var filter=new ContactFilter2D {useTriggers=false};filter.SetLayerMask(surfaces);
            int count=Physics2D.OverlapBox(new Vector2(b.center.x,b.max.y+box.bounds.extents.y+.08f),new Vector2(b.size.x+box.bounds.size.x,box.bounds.size.y-.06f),0,filter,clearanceHits);
            if(count==clearanceHits.Length)return false;
            for(int i=0;i<count;i++)if(!clearanceHits[i].transform.IsChildOf(transform))return false;
            float landingX=b.center.x+Mathf.Sign(sign)*(b.extents.x+box.bounds.extents.x+.2f);
            return RetreatRaycast(new Vector2(landingX,feet+.15f),Vector2.down,.6f);
        }
        // One bounded corridor check at commitment; actual movement still uses Pursue.
        public bool CanReachCover(float sign,float distance,out bool needsHop)
        {
            needsHop=false;if(!CanWithdraw(sign,true)||!motor.IsGrounded)return false;
            Vector2 forward=Vector2.right*Mathf.Sign(sign);
            Vector2 nose=(Vector2)box.bounds.center+forward*(box.bounds.extents.x+.05f);
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(surfaces);
            int count=Physics2D.Raycast(nose,forward,filter,retreatHits,distance);
            if(count==retreatHits.Length)return false;
            for(int i=0;i<count;i++)
            {
                if(retreatHits[i].collider.transform.IsChildOf(transform))continue;
                if(!RetreatHopSafe(retreatHits[i].collider,sign))return false;
                needsHop=true;
            }
            for(float d=0;d<=distance;d+=.4f)
                if(!RetreatRaycast((Vector2)box.bounds.center+forward*d+Vector2.down*(box.bounds.extents.y-.1f),Vector2.down,.6f))return false;
            Vector2 end=(Vector2)box.bounds.center+forward*distance;
            count=Physics2D.OverlapBox(end,(Vector2)box.bounds.size-Vector2.one*.06f,0,filter,clearanceHits);
            if(count==clearanceHits.Length)return false;
            for(int i=0;i<count;i++)if(!clearanceHits[i].transform.IsChildOf(transform))return false;
            return true;
        }
        public float WithdrawalReach(float sign, float wanted)
        {
            if (!CanWithdraw(sign)) return 0;
            Vector2 forward = Vector2.right * Mathf.Sign(sign);
            Vector2 nose = (Vector2)box.bounds.center + forward * (box.bounds.extents.x + .05f);
            var wall = RetreatRaycast(nose, forward, wanted);
            float reach = wall ? Mathf.Max(0, wall.distance - .15f) : wanted;
            for (float distance = .4f; distance <= reach + .01f; distance += .4f)
                if (!RetreatRaycast(nose + forward * distance + Vector2.down * (box.bounds.extents.y - .1f), Vector2.down, .6f)) return Mathf.Max(0, distance - .5f);
            return reach;
        }
        public void AnchorPatrolHere() { originX = body.position.x; pauseUntil = 0; ClearPursuit(); }
        private void Awake()
        {
            motor = GetComponent<PlayerMotor2D>(); body = GetComponent<Rigidbody2D>(); box = GetComponent<BoxCollider2D>();
            originX = body.position.x;
        }
        public void Configure(EnemyArchetype value) { archetype = value; originX = body.position.x; direction = 1; pauseUntil = 0; }
        private void FixedUpdate()
        {
            if (pursuing) { Pursue(); return; }
            if (DecisionsSuspended) return;
            if (archetype == null) { Stop(LocomotionState.Unconfigured); return; }
            if (archetype.Allows(MovementCapabilities.Stationary) || !archetype.Allows(MovementCapabilities.Walk))
            { Stop(LocomotionState.Stationary); return; }
            if (Time.time < pauseUntil) { Stop(LocomotionState.Blocked); return; }
            if (!motor.IsGrounded)
            {
                state = body.linearVelocity.y > 0 ? LocomotionState.Jumping : LocomotionState.Falling;
                motor.SetMoveInput(direction * archetype.MoveInput); return;
            }
            float halfWidth = box.bounds.extents.x, halfHeight = box.bounds.extents.y;
            Vector2 forward = Vector2.right * direction;
            Vector2 nose = body.position + forward * (halfWidth + .05f);
            bool wall = Physics2D.Raycast(nose, forward, wallProbe, surfaces);
            Vector2 aheadFoot = nose + forward * ledgeProbe + Vector2.down * (halfHeight - .1f);
            bool floorAhead = Physics2D.Raycast(aheadFoot, Vector2.down, .5f, surfaces);
            bool limit = (body.position.x - originX) * direction >= archetype.PatrolHalfWidth;
            bool headClear = !Physics2D.Raycast(nose + Vector2.up * 2, forward, wallProbe, surfaces);
            float landingX = body.position.x + direction * jumpLandingDistance;
            bool safeLanding = Mathf.Abs(landingX - originX) <= archetype.PatrolHalfWidth &&
                Physics2D.Raycast(new Vector2(landingX, aheadFoot.y), Vector2.down, .7f, surfaces);
            if (!limit && archetype.Allows(MovementCapabilities.Jump) && headClear && (wall || !floorAhead && safeLanding))
            {
                motor.SetMoveInput(direction * archetype.MoveInput); motor.RequestJump(); state = LocomotionState.Jumping; return;
            }
            if (limit || wall || !floorAhead && !archetype.Allows(MovementCapabilities.Drop))
            {
                direction = -direction; pauseUntil = Time.time + archetype.BlockedPause; Stop(LocomotionState.Blocked); return;
            }
            state = LocomotionState.Walking; motor.SetMoveInput(direction * archetype.MoveInput);
        }
        private void Pursue()
        {
            if (archetype == null || archetype.Allows(MovementCapabilities.Stationary) || !archetype.Allows(MovementCapabilities.Walk))
            { Stop(LocomotionState.Stationary); return; }
            if (Mathf.Abs(pursuitInput) < .01f) { Stop(LocomotionState.Stationary); return; }
            if (Time.time < pauseUntil) { Stop(LocomotionState.Blocked); return; }
            direction = Mathf.Sign(pursuitInput);
            Vector2 forward = Vector2.right * direction;
            Vector2 nose = (Vector2)box.bounds.center + forward * (box.bounds.extents.x + .05f);
            var wallHit = pursuitIsRetreat ? RetreatRaycast(nose, forward, wallProbe) : Physics2D.Raycast(nose, forward, wallProbe, surfaces); bool wall = wallHit;
            Vector2 aheadFoot=nose + forward * ledgeProbe + Vector2.down * (box.bounds.extents.y - .1f);
            bool floor = pursuitIsRetreat ? RetreatRaycast(aheadFoot,Vector2.down,pursuitAllowsJump?retreatHopHeight+.6f:.6f) : Physics2D.Raycast(aheadFoot,Vector2.down,.6f,surfaces);
            bool headClear = pursuitIsRetreat ? !RetreatRaycast(nose+Vector2.up*2,forward,wallProbe) : !Physics2D.Raycast(nose + Vector2.up * 2, forward, wallProbe, surfaces);
            if (pursuitAllowsJump && motor.IsGrounded && wall && headClear && archetype.Allows(MovementCapabilities.Jump) && (!pursuitIsRetreat || RetreatHopSafe(wallHit.collider,direction)))
            { motor.SetMoveInput(pursuitInput * archetype.MoveInput); motor.RequestJump(); state = LocomotionState.Jumping; return; }
            if (wall || motor.IsGrounded && !floor)
            { pauseUntil = Time.time + archetype.BlockedPause; Stop(LocomotionState.Blocked); return; }
            state = motor.IsGrounded ? LocomotionState.Walking : LocomotionState.Falling;
            motor.SetMoveInput(pursuitInput * archetype.MoveInput);
        }
        private void Stop(LocomotionState value) { state = value; motor.SetMoveInput(0); }
        private void OnDisable() { pursuing = false; if (motor != null) motor.SetMoveInput(0); }
    }
}
