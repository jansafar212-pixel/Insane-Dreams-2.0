using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Prototype;
using UnityEngine;

namespace InsaneDreams.Enemies
{
    public enum EngagementState { Patrol, Engaging, Reloading, Defeated, Approaching, Searching, Reacting, Panicked, Withdrawing, BlindFiring }
    [DefaultExecutionOrder(-20)]
    [RequireComponent(typeof(Health), typeof(AttackExecutor), typeof(LarperBrain))]
    public sealed class EnemyCombat : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition firearm;
        [SerializeField, Min(1)] private float detectionRange = 22;
        [SerializeField, Min(1)] private float engagementRange = 18;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private bool allowOffscreenFire;
        [SerializeField, Min(.02f)] private float perceptionSeconds = .1f;
        private float nextPerception;
        public bool MayInitiateFire => allowOffscreenFire || InGameplayViewport;
        private bool cameraBound, engagementAuthorized;
        private bool encounterEnabled=true;
        public void SetEncounterEnabled(bool value)=>encounterEnabled=value;
        private float nextCameraLookup;
        private Transform visibleBody;
        public bool HasEngaged => engagementAuthorized;
        // Checkpoint/replay boundary only; normal combat still owns its state machine.
        public void ResetEncounter(Health value)
        {
            panicUntil=blindUntil=reengageAt=readyAt=nextAimSample=0;withdrawing=false;selectedCover=null;
            weapon?.Cycle.CancelReload();if(firearm!=null)weapon=new RuntimeWeapon(firearm,initialReserveAmmo);
            posture?.ResetPosture();patrol.DecisionsSuspended=false;patrol.ClearPursuit();
            AimDirection=Vector2.right;State=EngagementState.Patrol;BindTarget(value);
        }
        public void BindGameplayCamera(Camera value) { gameplayCamera=value;cameraBound=true; }
        public bool InGameplayViewport
        {
            get
            {
                if(!cameraBound&&(gameplayCamera==null||!gameplayCamera.isActiveAndEnabled||!gameplayCamera.CompareTag("MainCamera"))&&Time.unscaledTime>=nextCameraLookup)
                {gameplayCamera=Camera.main;nextCameraLookup=Time.unscaledTime+.5f;}
                return InsaneDreams.CameraSystem.GameplayViewport.Overlaps(gameplayCamera,actorBox.bounds,visibleBody!=null?visibleBody.gameObject.layer:gameObject.layer);
            }
        }
        [SerializeField, Min(0)] private float closeRange = 3;
        [SerializeField, Range(0, 45)] private float closeSpread = 3;
        [SerializeField, Range(0, 45)] private float distantSpread = 40;
        [SerializeField] private LayerMask geometry = 1;
        [SerializeField, Min(0)] private float reactionSeconds = .3f;
        [SerializeField, Min(.02f)] private float aimSampleSeconds = .18f;
        [SerializeField, Min(1)] private float aimDegreesPerSecond = 240;
        [SerializeField, Min(1)] private float effectiveRange = 10;
        [SerializeField, Min(1)] private float preferredDistance = 5;
        [SerializeField, Min(0)] private float visibilityMemory = 3;
        [SerializeField, Min(0)] private float rangeHysteresis = 2;
        [SerializeField, Range(0, .8f)] private float ordinaryReloadFraction = .2f;
        [SerializeField, Range(0, .8f)] private float cowardReloadFraction = .4f;
        [SerializeField, Range(0, .8f)] private float overconfidentReloadFraction;
        [SerializeField, Min(.1f)] private float withdrawalDistance = 6;
        [SerializeField, Min(.1f)] private float withdrawalSeconds = 4.5f;
        [SerializeField, Min(.05f)] private float tacticalSampleSeconds = .18f;
        [SerializeField, Min(0)] private float reengagementDelay = .35f;
        [SerializeField, Min(0)] private int initialReserveAmmo = 135;
        [SerializeField, Min(.1f)] private float minimumWithdrawalDistance = 2.5f;
        [SerializeField, Range(0,.1f)] private float reloadDurationVariation = .05f;
        [SerializeField, Min(0)] private float reactionJitterSeconds = .05f;
        [SerializeField, Range(0,10)] private float blindAimUncertainty = 3;
        [SerializeField, Range(1,10)] private float coverSearchDistance = 7;
        private readonly Collider2D[] coverCandidates = new Collider2D[32];
        private Collider2D selectedCover, targetCollider;
        private BoxCollider2D actorBox;
        private InsaneDreams.Traversal.PlayerPosture2D posture;
        private bool withdrawalAllowsHop;
        private float withdrawalY;
        public Collider2D SelectedCover => selectedCover;
        public Vector2 RetreatDestination => new Vector2(withdrawalX+retreatDirection*withdrawalGoal,withdrawalY);
        public bool IsReloadingCrouched => weapon != null && weapon.Cycle.IsReloading && posture != null && posture.IsCrouched;
        private Vector2 SightOrigin => new Vector2(actorBox.bounds.center.x, actorBox.bounds.max.y-.15f);
        private Vector2 SightTarget => targetCollider != null ? new Vector2(targetCollider.bounds.center.x,targetCollider.bounds.max.y-.15f) : (Vector2)target.transform.position+Vector2.up*.4f;
        [SerializeField, Range(0,1)] private float ordinaryBlindChance = .7f, overconfidentBlindChance = .95f, cowardBlindChance = .4f;
        [SerializeField, Range(.1f,3)] private float blindFireSeconds = 2;
        [SerializeField, Range(.1f,1)] private float cowardBlindDuration = .45f;
        private float blindUntil, lastFiredAt = float.NegativeInfinity;
        public Vector2 LastKnownPosition => lastSeen;
        public bool IsBlindFiring => !TargetVisible && Time.time < blindUntil && weapon != null && weapon.MagazineAmmo > 0 && MayInitiateFire;
        public event System.Action WithdrawalStarted;
        public event System.Action WithdrawalFailed;
        private float withdrawalGoal;
        private bool withdrawalAnnounced;
        private Health owner, target;
        private LarperBrain patrol;
        private PlayerMotor2D motor;
        private AttackExecutor executor;
        private RuntimeWeapon weapon;
        private AuthoredLadderRoute route;
        private LarperAwareness awareness;
        private bool withdrawing;
        private float withdrawalStarted, withdrawalX, retreatDirection, nextTacticalSample, reengageAt;
        public RuntimeWeapon Weapon => weapon;
        public float ReloadFraction => awareness != null && awareness.Personality != null ? awareness.Personality.Disposition switch
        { LarperDisposition.Cowardly => cowardReloadFraction, LarperDisposition.Overconfident => overconfidentReloadFraction, _ => ordinaryReloadFraction } : ordinaryReloadFraction;
        private bool tracking, wasVisible;
        private float readyAt, lastSeenAt, nextAimSample;
        private Vector2 lastSeen, sampledAim;
        private float panicUntil;
        private readonly RaycastHit2D[] sightHits=new RaycastHit2D[8];
        public event System.Action ReloadStarted;
        public event System.Action<bool> VisibilityChanged;
        public bool PanicFor(float seconds)
        {
            if(owner.IsDefeated||route!=null&&route.IsRunning)return false;
            panicUntil=Mathf.Max(panicUntil,Time.time+seconds);blindUntil=0;withdrawing=false;weapon?.Cycle.CancelReload();return true;
        }
        public bool TargetVisible { get; private set; }
        public float SpreadAtDistance(float distance) => Mathf.Lerp(closeSpread, distantSpread, Mathf.InverseLerp(closeRange, effectiveRange, distance));
        public EngagementState State { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        private void OnEnable() => engagementAuthorized=false;
        private void Awake()
        {
            owner = GetComponent<Health>(); patrol = GetComponent<LarperBrain>();
            motor = GetComponent<PlayerMotor2D>(); executor = GetComponent<AttackExecutor>();
            route = GetComponent<AuthoredLadderRoute>();
            awareness = GetComponent<LarperAwareness>();
            actorBox = GetComponent<BoxCollider2D>(); posture = GetComponent<InsaneDreams.Traversal.PlayerPosture2D>();
            var rig=GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true);visibleBody=rig!=null?rig.torso:transform;
            if (firearm != null) weapon = new RuntimeWeapon(firearm, initialReserveAmmo);
        }
        public void BindTarget(Health value) { nextPerception=0;TargetVisible=false;engagementAuthorized=false;target = value; targetCollider=value!=null?value.GetComponent<Collider2D>():null;tracking = wasVisible = false; blindUntil=0;lastFiredAt=float.NegativeInfinity; }
        private void Update()
        {
            // Authored encounter staging is independent of camera visibility. Scene
            // occupants hold their posts until activated; active combat never sleeps offscreen.
            if(!encounterEnabled){patrol.DecisionsSuspended=true;patrol.ClearPursuit();motor.SetMoveInput(0);State=EngagementState.Patrol;return;}
            // The existing posture controller owns collider changes and safe standing.
            // Combat only requests crouch during a reload at a verified cover endpoint.
            bool coverReload=weapon!=null&&weapon.Cycle.IsReloading&&Time.time>=panicUntil&&selectedCover!=null&&selectedCover.isActiveAndEnabled&&
                motor.IsGrounded&&Mathf.Abs(transform.position.y-withdrawalY)<.25f&&
                Mathf.Abs(transform.position.x-RetreatDestination.x)<.65f&&CoveredAt(transform.position.x);
            posture?.SetInput(coverReload?Vector2.down:Vector2.zero);
            if (owner.IsDefeated) { blindUntil=0;withdrawing=false; weapon?.Cycle.CancelReload(); patrol.ClearPursuit(); motor.SetMoveInput(0); State = EngagementState.Defeated; return; }
            if(Time.time<panicUntil){patrol.DecisionsSuspended=true;patrol.ClearPursuit();motor.SetMoveInput(0);State=EngagementState.Panicked;return;}
            bool routing = route != null && route.IsRunning;
            Vector2 delta = target != null ? (Vector2)(target.transform.position - transform.position) : Vector2.zero;
            bool available = !routing && target != null && target.isActiveAndEnabled && !target.IsDefeated;
            float range = detectionRange + (tracking ? rangeHysteresis : 0);
            bool previouslyVisible=TargetVisible;
            if(!available)TargetVisible=false;
            else if(Time.time>=nextPerception)
            {
                nextPerception=Time.time+perceptionSeconds;
                TargetVisible=delta.sqrMagnitude<=range*range&&!CombatVisibility2D.IsBlocked(SightOrigin,SightTarget,geometry,sightHits);
                if(TargetVisible){lastSeenAt=Time.time;lastSeen=target.transform.position;}
            }
            if(previouslyVisible!=TargetVisible)VisibilityChanged?.Invoke(TargetVisible);
            // Authored traversal owns the motor. Do not resume a stale retreat after it.
            if (routing) { blindUntil=0;withdrawing=false; weapon?.Cycle.CancelReload(); patrol.ClearPursuit(); patrol.DecisionsSuspended=true; State=EngagementState.Patrol; return; }
            if (wasVisible && !TargetVisible && available && tracking && State==EngagementState.Engaging && weapon!=null && !weapon.Cycle.IsReloading &&
                weapon.MagazineAmmo>0 && Time.time-lastFiredAt<=Mathf.Max(.35f,weapon.Definition.FiringInterval*1.5f))
            {
                var disposition=awareness!=null&&awareness.Personality!=null?awareness.Personality.Disposition:LarperDisposition.Ordinary;
                float chance=disposition==LarperDisposition.Overconfident?overconfidentBlindChance:disposition==LarperDisposition.Cowardly?cowardBlindChance:ordinaryBlindChance;
                if(Random.value<chance)blindUntil=Mathf.Min(lastSeenAt+visibilityMemory,Time.time+blindFireSeconds*(disposition==LarperDisposition.Cowardly?cowardBlindDuration:1));
            }
            if (TargetVisible)
            {
                blindUntil=0;
                if (!wasVisible) { readyAt = Time.time + reactionSeconds + Random.Range(0,reactionJitterSeconds); nextAimSample = 0; }
                if (!tracking) AimDirection = delta.normalized;
                tracking = true;
            }
            wasVisible = TargetVisible;
            // Camera permission is independent of awareness and is checked every frame.
            // Leaving the view cancels panic fire; a previous shot grants no exemption.
            if(!MayInitiateFire)blindUntil=0;
            // A committed reload finishes even if sight is lost or the target leaves.
            // Tick is the sole ammunition transfer authority; defeat/panic cancel first.
            bool wasReloading = weapon != null && weapon.Cycle.IsReloading;
            weapon?.Cycle.Tick(Time.time);
            if (wasReloading && !weapon.Cycle.IsReloading) { reengageAt = Time.time + reengagementDelay; posture?.SetInput(Vector2.zero); posture?.TryStand(); }
            if (weapon != null && weapon.Cycle.IsReloading)
            { blindUntil=0;patrol.DecisionsSuspended=true; patrol.SetPursuit(0); State=EngagementState.Reloading; return; }
            if (withdrawing)
            {
                patrol.DecisionsSuspended=true; State=EngagementState.Withdrawing;
                float travelled = Mathf.Max(0,(transform.position.x-withdrawalX)*retreatDirection);
                // Stop at the endpoint between tactical samples; inertia still belongs
                // to the motor. Otherwise a sample interval can overshoot low cover.
                if(travelled>=withdrawalGoal)patrol.SetPursuit(0);
                if (!withdrawalAnnounced && travelled >= .25f) { withdrawalAnnounced=true; WithdrawalStarted?.Invoke(); }
                if (Time.time >= nextTacticalSample)
                {
                    nextTacticalSample = Time.time + tacticalSampleSeconds;
                    bool reached = travelled >= withdrawalGoal;
                    // Settle from a planned hop before crouching/reloading. A finite
                    // timeout and local safety checks still terminate changing routes.
                    bool settling=!motor.IsGrounded||selectedCover!=null&&Mathf.Abs(transform.position.y-withdrawalY)>.25f;
                    if (reached && settling && Time.time-withdrawalStarted<withdrawalSeconds) { patrol.SetPursuit(0); return; }
                    if (reached || Time.time-withdrawalStarted >= withdrawalSeconds || !patrol.CanWithdraw(retreatDirection,withdrawalAllowsHop))
                    { if (!reached && travelled < minimumWithdrawalDistance) WithdrawalFailed?.Invoke(); BeginReload(); return; }
                    patrol.SetPursuit(retreatDirection, withdrawalAllowsHop, true);
                }
                return;
            }
            if (blindUntil>0)
            {
                if (available && Time.time<blindUntil && weapon.MagazineAmmo>0)
                {
                    patrol.DecisionsSuspended=true;patrol.SetPursuit(0);State=EngagementState.BlindFiring;
                    AimAtMemory();FireAtMemory(Vector2.Distance(lastSeen,transform.position),true);return;
                }
                blindUntil=0;
                if(available&&weapon.MagazineAmmo==0&&weapon.ReserveAmmo>0){StartWithdrawal();return;}
            }
            // Recent last-seen information may still justify a low-ammo withdrawal.
            // Do not abandon the decision merely because the next sight sample is blocked.
            if (available && tracking && Time.time-lastSeenAt <= visibilityMemory && Time.time >= Mathf.Max(readyAt,reengageAt) &&
                weapon != null && weapon.ReserveAmmo > 0 && weapon.MagazineAmmo <= Mathf.FloorToInt(weapon.Definition.MagazineCapacity * ReloadFraction) &&
                Vector2.Distance(lastSeen,transform.position) <= engagementRange)
            { StartWithdrawal(); return; }
            if (!available || !TargetVisible && (!tracking || Time.time - lastSeenAt > visibilityMemory))
            {
                engagementAuthorized=false;tracking = wasVisible = false; patrol.ClearPursuit();
                if (!routing) patrol.DecisionsSuspended = false;
                State = EngagementState.Patrol; return;
            }
            patrol.DecisionsSuspended = true;
            Vector2 rememberedDelta = lastSeen - (Vector2)transform.position;
            bool approach = Mathf.Abs(rememberedDelta.x) > preferredDistance && Mathf.Abs(rememberedDelta.y) < 3;
            patrol.SetPursuit(approach ? Mathf.Sign(rememberedDelta.x) : 0);
            AimAtMemory();
            if (!TargetVisible) { State = EngagementState.Searching; return; }
            if (Time.time < Mathf.Max(readyAt, reengageAt)) { State = EngagementState.Reacting; return; }
            if (delta.magnitude > engagementRange) { State = EngagementState.Approaching; return; }
            if(!MayInitiateFire){State=EngagementState.Approaching;return;}
            State = EngagementState.Engaging;
            if (weapon == null || weapon.MagazineAmmo == 0) return;
            FireAtMemory(delta.magnitude,false);
        }
        private void AimAtMemory()
        {
            Vector2 rememberedDelta=lastSeen-(Vector2)transform.position;
            if (Time.time >= nextAimSample)
            { sampledAim = rememberedDelta.normalized; nextAimSample = Time.time + aimSampleSeconds; }
            float angle = Mathf.MoveTowardsAngle(Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg,
                Mathf.Atan2(sampledAim.y, sampledAim.x) * Mathf.Rad2Deg, aimDegreesPerSecond * Time.deltaTime);
            AimDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }
        private void FireAtMemory(float distance,bool blind)
        {
            if(Time.time<weapon.Cycle.NextAttackAt||weapon.Cycle.IsReloading)return;
            if(!MayInitiateFire)return;
            // A bounded perception cache never authorizes a shot through newly placed cover.
            if(!blind&&CombatVisibility2D.IsBlocked(SightOrigin,SightTarget,geometry,sightHits))return;
            float spread = SpreadAtDistance(distance) + (blind ? blindAimUncertainty : 0);
            Vector2 direction = Quaternion.Euler(0, 0, Random.Range(-spread, spread)) * AimDirection;
            // Test the actual imperfect firing direction, not just the direct visibility ray.
            if (!blind && CombatVisibility2D.IsBlocked(SightOrigin,SightOrigin+direction*Mathf.Min(distance,weapon.Definition.AttackRange),geometry,sightHits)) return;
            if(executor.Attack(weapon, direction, Time.time)){lastFiredAt=Time.time;engagementAuthorized=true;}
        }
        private void BeginReload()
        {
            blindUntil=0;
            withdrawing=false; patrol.SetPursuit(0); motor.SetMoveInput(0);
            if (weapon.Cycle.TryReload(Time.time,Random.Range(1-reloadDurationVariation,1+reloadDurationVariation))) ReloadStarted?.Invoke();
            State=weapon.Cycle.IsReloading?EngagementState.Reloading:EngagementState.Searching;
        }
        private void StartWithdrawal()
        {
            blindUntil=0;
            float dx = lastSeen.x-transform.position.x;
            retreatDirection = Mathf.Abs(dx) > .1f ? -Mathf.Sign(dx) : -Mathf.Sign(AimDirection.x);
            withdrawalX=transform.position.x;withdrawalY=transform.position.y;selectedCover=null;withdrawalAllowsHop=false;
            if(!PickReachableCover())withdrawalGoal = patrol.WithdrawalReach(retreatDirection, withdrawalDistance);
            if (withdrawalGoal < minimumWithdrawalDistance) { WithdrawalFailed?.Invoke(); BeginReload(); return; }
            withdrawing=true; withdrawalAnnounced=false; withdrawalStarted=Time.time; withdrawalX=transform.position.x;
            nextTacticalSample=Time.time+tacticalSampleSeconds; State=EngagementState.Withdrawing; patrol.SetPursuit(retreatDirection,withdrawalAllowsHop,true);
        }
        private bool CoveredAt(float x)
        {
            float height=posture!=null?posture.StandingWorldHeight:actorBox.bounds.size.y;
            Vector2 crouchedCentre=new Vector2(x,withdrawalY-height*(1-(posture!=null?posture.CrouchedHeightRatio:.7f))*.5f);
            return CombatVisibility2D.IsBlocked(lastSeen+Vector2.up*.35f,crouchedCentre,geometry,sightHits);
        }
        private bool PickReachableCover()
        {
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(geometry);
            int count=Physics2D.OverlapBox((Vector2)transform.position+Vector2.right*retreatDirection*coverSearchDistance*.5f,new Vector2(coverSearchDistance+2,3),0,filter,coverCandidates);
            if(count==coverCandidates.Length)return false; // Saturated queries fall back safely.
            float best=float.NegativeInfinity;
            for(int i=0;i<count;i++)
            {
                var c=coverCandidates[i];if(c==null||c.transform.IsChildOf(transform))continue;var b=c.bounds;
                if(b.size.y<.55f||b.size.x>1.8f||Mathf.Abs(b.min.y-actorBox.bounds.min.y)>.2f)continue;
                float x=(retreatDirection>0?b.max.x:b.min.x)+retreatDirection*(actorBox.bounds.extents.x+.25f);
                float distance=(x-withdrawalX)*retreatDirection;
                if(distance<minimumWithdrawalDistance||distance>coverSearchDistance||!CoveredAt(x)||!patrol.CanReachCover(retreatDirection,distance,out bool hop))continue;
                float score=coverSearchDistance-distance;
                if(score<=best)continue;best=score;selectedCover=c;withdrawalGoal=distance;withdrawalAllowsHop=hop;
            }
            return selectedCover!=null;
        }
        private void OnDrawGizmosSelected()
        {
            var box=GetComponent<BoxCollider2D>();if(box==null)return;
            var camera=gameplayCamera!=null?gameplayCamera:Camera.main;
            Gizmos.color=allowOffscreenFire||InsaneDreams.CameraSystem.GameplayViewport.Overlaps(camera,box.bounds,gameObject.layer,true)?Color.green:Color.red;
            Gizmos.DrawWireCube(box.bounds.center,box.bounds.size);
            if(Application.isPlaying&&tracking){Gizmos.color=Color.yellow;Gizmos.DrawLine(box.bounds.center,lastSeen);Gizmos.DrawWireSphere(lastSeen,.15f);}
        }
        private void OnDisable() { posture?.SetInput(Vector2.zero);posture?.TryStand();selectedCover=null;blindUntil=0;withdrawing=false; tracking = wasVisible = TargetVisible = false; if (patrol != null) { patrol.DecisionsSuspended = false; patrol.ClearPursuit(); } weapon?.Cycle.CancelReload(); }
    }
}
