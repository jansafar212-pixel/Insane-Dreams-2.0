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
        [SerializeField, Min(1)] private float detectionRange = 14;
        [SerializeField, Min(0)] private float closeRange = 3;
        [SerializeField, Range(0, 45)] private float closeSpread = 2;
        [SerializeField, Range(0, 45)] private float distantSpread = 16;
        [SerializeField] private LayerMask geometry = 1;
        [SerializeField, Min(0)] private float reactionSeconds = .3f;
        [SerializeField, Min(.02f)] private float aimSampleSeconds = .18f;
        [SerializeField, Min(1)] private float aimDegreesPerSecond = 240;
        [SerializeField, Min(1)] private float effectiveRange = 10;
        [SerializeField, Min(1)] private float preferredDistance = 5;
        [SerializeField, Min(0)] private float visibilityMemory = .65f;
        [SerializeField, Min(0)] private float rangeHysteresis = 2;
        [SerializeField, Range(0, .8f)] private float ordinaryReloadFraction = .2f;
        [SerializeField, Range(0, .8f)] private float cowardReloadFraction = .4f;
        [SerializeField, Range(0, .8f)] private float overconfidentReloadFraction;
        [SerializeField, Min(.1f)] private float withdrawalDistance = 3.5f;
        [SerializeField, Min(.1f)] private float withdrawalSeconds = 1.8f;
        [SerializeField, Min(.05f)] private float tacticalSampleSeconds = .18f;
        [SerializeField, Min(0)] private float reengagementDelay = .35f;
        [SerializeField, Min(0)] private int initialReserveAmmo = 54;
        [SerializeField, Min(.1f)] private float minimumWithdrawalDistance = 2.5f;
        [SerializeField, Range(0,1)] private float ordinaryBlindChance = .7f, overconfidentBlindChance = .95f, cowardBlindChance = .4f;
        [SerializeField, Range(.1f,3)] private float blindFireSeconds = 2;
        [SerializeField, Range(.1f,1)] private float cowardBlindDuration = .45f;
        private float blindUntil, lastFiredAt = float.NegativeInfinity;
        public Vector2 LastKnownPosition => lastSeen;
        public bool IsBlindFiring => !TargetVisible && Time.time < blindUntil && weapon != null && weapon.MagazineAmmo > 0;
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
        private void Awake()
        {
            owner = GetComponent<Health>(); patrol = GetComponent<LarperBrain>();
            motor = GetComponent<PlayerMotor2D>(); executor = GetComponent<AttackExecutor>();
            route = GetComponent<AuthoredLadderRoute>();
            awareness = GetComponent<LarperAwareness>();
            if (firearm != null) weapon = new RuntimeWeapon(firearm, initialReserveAmmo);
        }
        public void BindTarget(Health value) { target = value; tracking = wasVisible = false; blindUntil=0;lastFiredAt=float.NegativeInfinity; }
        private void Update()
        {
            if (owner.IsDefeated) { blindUntil=0;withdrawing=false; weapon?.Cycle.CancelReload(); patrol.ClearPursuit(); motor.SetMoveInput(0); State = EngagementState.Defeated; return; }
            if(Time.time<panicUntil){patrol.DecisionsSuspended=true;patrol.ClearPursuit();motor.SetMoveInput(0);State=EngagementState.Panicked;return;}
            bool routing = route != null && route.IsRunning;
            Vector2 delta = target != null ? (Vector2)(target.transform.position - transform.position) : Vector2.zero;
            bool available = !routing && target != null && target.isActiveAndEnabled && !target.IsDefeated;
            float range = detectionRange + (tracking ? rangeHysteresis : 0);
            bool previouslyVisible=TargetVisible;
            TargetVisible = available && delta.sqrMagnitude <= range * range &&
                !CombatVisibility2D.IsBlocked(transform.position, target.transform.position, geometry, sightHits);
            if(previouslyVisible!=TargetVisible)VisibilityChanged?.Invoke(TargetVisible);
            // Authored traversal owns the motor. Do not resume a stale retreat after it.
            if (routing) { blindUntil=0;withdrawing=false; weapon?.Cycle.CancelReload(); patrol.ClearPursuit(); patrol.DecisionsSuspended=true; State=EngagementState.Patrol; return; }
            if (wasVisible && !TargetVisible && available && tracking && State==EngagementState.Engaging && weapon!=null && !weapon.Cycle.IsReloading &&
                weapon.MagazineAmmo>0 && Time.time-lastFiredAt<=Mathf.Max(.35f,weapon.Definition.FiringInterval*1.5f))
            {
                var disposition=awareness!=null&&awareness.Personality!=null?awareness.Personality.Disposition:LarperDisposition.Ordinary;
                float chance=disposition==LarperDisposition.Overconfident?overconfidentBlindChance:disposition==LarperDisposition.Cowardly?cowardBlindChance:ordinaryBlindChance;
                if(Random.value<chance)blindUntil=Time.time+blindFireSeconds*(disposition==LarperDisposition.Cowardly?cowardBlindDuration:1);
            }
            if (TargetVisible)
            {
                blindUntil=0;
                if (!wasVisible) { readyAt = Time.time + reactionSeconds; nextAimSample = 0; }
                if (!tracking) AimDirection = delta.normalized;
                tracking = true; lastSeenAt = Time.time; lastSeen = target.transform.position;
            }
            wasVisible = TargetVisible;
            // A committed reload finishes even if sight is lost or the target leaves.
            // Tick is the sole ammunition transfer authority; defeat/panic cancel first.
            bool wasReloading = weapon != null && weapon.Cycle.IsReloading;
            weapon?.Cycle.Tick(Time.time);
            if (wasReloading && !weapon.Cycle.IsReloading) reengageAt = Time.time + reengagementDelay;
            if (weapon != null && weapon.Cycle.IsReloading)
            { blindUntil=0;patrol.DecisionsSuspended=true; patrol.SetPursuit(0); State=EngagementState.Reloading; return; }
            if (withdrawing)
            {
                patrol.DecisionsSuspended=true; State=EngagementState.Withdrawing;
                float travelled = Mathf.Abs(transform.position.x-withdrawalX);
                if (!withdrawalAnnounced && travelled >= .25f) { withdrawalAnnounced=true; WithdrawalStarted?.Invoke(); }
                if (Time.time >= nextTacticalSample)
                {
                    nextTacticalSample = Time.time + tacticalSampleSeconds;
                    bool reached = travelled >= withdrawalGoal;
                    // Breaking sight is a sufficient local improvement; blocked/timeout
                    // paths fall back to reloading in place instead of searching forever.
                    if (reached || !TargetVisible && travelled >= minimumWithdrawalDistance || Time.time-withdrawalStarted >= withdrawalSeconds || !patrol.CanWithdraw(retreatDirection))
                    { if (!reached && travelled < minimumWithdrawalDistance) WithdrawalFailed?.Invoke(); BeginReload(); return; }
                    patrol.SetPursuit(retreatDirection, false);
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
                Vector2.Distance(lastSeen,transform.position) <= effectiveRange)
            { StartWithdrawal(); return; }
            if (!available || delta.sqrMagnitude > range * range || !TargetVisible && (!tracking || Time.time - lastSeenAt > visibilityMemory))
            {
                tracking = wasVisible = false; patrol.ClearPursuit();
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
            if (delta.magnitude > effectiveRange) { State = EngagementState.Approaching; return; }
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
            float spread = SpreadAtDistance(distance);
            Vector2 direction = Quaternion.Euler(0, 0, Random.Range(-spread, spread)) * AimDirection;
            // Test the actual imperfect firing direction, not just the direct visibility ray.
            if (!blind && CombatVisibility2D.IsBlocked(transform.position,(Vector2)transform.position+direction*Mathf.Min(distance,weapon.Definition.AttackRange),geometry,sightHits)) return;
            if(executor.Attack(weapon, direction, Time.time))lastFiredAt=Time.time;
        }
        private void BeginReload()
        {
            blindUntil=0;
            withdrawing=false; patrol.SetPursuit(0); motor.SetMoveInput(0);
            if (weapon.Cycle.TryReload(Time.time)) ReloadStarted?.Invoke();
            State=weapon.Cycle.IsReloading?EngagementState.Reloading:EngagementState.Searching;
        }
        private void StartWithdrawal()
        {
            blindUntil=0;
            float dx = lastSeen.x-transform.position.x;
            retreatDirection = Mathf.Abs(dx) > .1f ? -Mathf.Sign(dx) : -Mathf.Sign(AimDirection.x);
            withdrawalGoal = patrol.WithdrawalReach(retreatDirection, withdrawalDistance);
            if (withdrawalGoal < minimumWithdrawalDistance) { WithdrawalFailed?.Invoke(); BeginReload(); return; }
            withdrawing=true; withdrawalAnnounced=false; withdrawalStarted=Time.time; withdrawalX=transform.position.x;
            nextTacticalSample=Time.time+tacticalSampleSeconds; State=EngagementState.Withdrawing; patrol.SetPursuit(retreatDirection,false);
        }
        private void OnDisable() { blindUntil=0;withdrawing=false; tracking = wasVisible = TargetVisible = false; if (patrol != null) { patrol.DecisionsSuspended = false; patrol.ClearPursuit(); } weapon?.Cycle.CancelReload(); }
    }
}
