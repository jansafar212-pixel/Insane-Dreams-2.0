using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Prototype;
using UnityEngine;

namespace InsaneDreams.Enemies
{
    public enum EngagementState { Patrol, Engaging, Reloading, Defeated, Approaching, Searching, Reacting, Panicked, Withdrawing, BlindFiring }
    public enum ThreatAwareness { Unaware, Suspicious, Alerted, UnderFire }
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
        public event System.Action ThreatNoticed;
        [SerializeField, Min(.8f)] private float minimumCombatSpacing=1.75f;
        [SerializeField, Min(.05f)] private float positionSampleSeconds=.25f;
        [Tooltip("Under fire, reconsider a moving target after this fraction of the normal displacement. Stable positions still remain valid.")]
        [SerializeField, Range(.8f,1)] private float suppressedRepositionDistanceMultiplier=.8f;
        private float nextPositionSample;
        private Vector2 positionAnchor;
        [SerializeField] private Vector2 combatRepositionSeconds=new Vector2(8,13);
        [SerializeField, Range(0,1)] private float combatRepositionChance=.2f;
        private float nextCombatReposition,repositionStandOff,coverHoldUntil;
        public int TacticalRepositions { get; private set; }
        public bool IsHoldingCover=>Time.time<coverHoldUntil&&selectedCover!=null&&selectedCover.isActiveAndEnabled;
        private bool retreatReservation;
        private float nextCrowdCheck,crowdDirection;
        private bool crowdBlocked;
        public bool CanAdvance(float direction)
        {
            if(!motor.IsGrounded||route!=null&&route.IsRunning)return true;
            direction=Mathf.Sign(direction);
            if(Time.time>=nextCrowdCheck||direction!=crowdDirection)
            {
                nextCrowdCheck=Time.time+.1f;crowdDirection=direction;
                crowdBlocked=!CombatPositionReservations.CanAdvance(this,direction,crowdBlocked);
            }
            return !crowdBlocked;
        }
        internal float MinimumCombatSpacing=>minimumCombatSpacing;
        internal bool PositionActive=>isActiveAndEnabled&&owner!=null&&!owner.IsDefeated;
        public bool HasCombatPosition=>CombatPositionReservations.TryGet(this,out _);
        public Vector2 CombatPosition=>CombatPositionReservations.TryGet(this,out var point)?point:(Vector2)transform.position;
        private void ReleasePosition(){CombatPositionReservations.Release(this);retreatReservation=false;nextPositionSample=0;}
        private void MoveToCombatPosition(Vector2 focus,float standOff)
        {
            // Briefly hold ground while returning inaccurate fire. Keep any existing
            // reservation; critical reload/retreat paths cancel the reaction first.
            if(IsDefensivelyCrouching){patrol.SetPursuit(0);motor.SetMoveInput(0);return;}
            Vector2 delta=focus-(Vector2)transform.position;
            if(Mathf.Abs(delta.y)>=3){patrol.SetPursuit(0);ReleasePosition();return;}
            if(Time.time>=nextPositionSample)
            {
                nextPositionSample=Time.time+positionSampleSeconds;
                bool has=CombatPositionReservations.TryGet(this,out var held);
                bool reached=has&&Mathf.Abs(held.x-transform.position.x)<.2f;
                if(TargetVisible&&motor.IsGrounded&&Time.time>=readyAt)
                {
                    if(nextCombatReposition==0)nextCombatReposition=Time.time+Random.Range(combatRepositionSeconds.x,combatRepositionSeconds.y);
                    else if(Time.time>=nextCombatReposition&&(!has||reached))
                    {
                        nextCombatReposition=Time.time+Mathf.Max(1,Random.Range(combatRepositionSeconds.x,combatRepositionSeconds.y));
                        bool pressured=Suppression>=moderateSuppression||Time.time-lastFiredAt>2.5f;
                        if(Random.value<combatRepositionChance*(pressured?2:1))
                        {
                            var disposition=awareness!=null&&awareness.Personality!=null?awareness.Personality.Disposition:LarperDisposition.Ordinary;
                            float advanceChance=disposition==LarperDisposition.Overconfident?.65f:disposition==LarperDisposition.Cowardly?.1f:.3f;
                            repositionStandOff=Random.value<advanceChance?-1.25f:1.75f;has=false;TacticalRepositions++;
                        }
                    }
                }
                standOff=Mathf.Max(1,standOff+repositionStandOff);
                float reconsiderDistance=minimumCombatSpacing*2*(Time.time<underFireUntil?Mathf.Clamp(suppressedRepositionDistanceMultiplier,.8f,1):1);
                if(!has||retreatReservation||Mathf.Abs(focus.x-positionAnchor.x)>reconsiderDistance||!CombatPositionReservations.IsFree(this,held)||reached&&Mathf.Abs(delta.x)>standOff+.3f)
                {
                    CombatPositionReservations.Release(this);retreatReservation=false;
                    float sign=Mathf.Abs(delta.x)>.01f?Mathf.Sign(delta.x):1;
                    float desired=Mathf.Abs(delta.x)>standOff||repositionStandOff!=0?focus.x-sign*standOff:transform.position.x;
                    desired=transform.position.x+Mathf.Clamp(desired-transform.position.x,-3,3);
                    for(int i=0;i<7;i++)
                    {
                        float offset=i==0?0:((i+1)/2)*minimumCombatSpacing*(i%2==1?-sign:sign);
                        Vector2 candidate=new Vector2(desired+offset,transform.position.y);float travel=candidate.x-transform.position.x;
                        if(Mathf.Abs(travel)>.2f&&!patrol.CanReachCover(Mathf.Sign(travel),Mathf.Abs(travel),out _))continue;
                        if(CombatPositionReservations.TryClaim(this,candidate)){positionAnchor=focus;break;}
                    }
                    if(!HasCombatPosition&&CombatPositionReservations.TryClaim(this,transform.position))positionAnchor=focus;
                }
            }
            float dx=CombatPosition.x-transform.position.x;patrol.SetPursuit(HasCombatPosition&&Mathf.Abs(dx)>.18f?Mathf.Sign(dx):0);
        }
        [SerializeField, Min(0)] private float movementHearingRange=11;
        [SerializeField, Min(0)] private float gunfireHearingRange=24;
        [SerializeField, Range(.05f,1)] private float obstructedHearingMultiplier=.45f;
        [SerializeField, Min(.1f)] private float hearingMemorySeconds=4;
        [SerializeField, Min(.1f)] private float soundPositionUncertainty=2;
        [SerializeField, Min(.1f)] private float nearMissDistance=1.25f;
        [SerializeField, Min(.05f)] private float gunfireHearingInterval=.25f;
        private Vector2 heardPosition;
        private float heardUntil,underFireUntil,nextGunfireHearing,hesitateUntil,nextHesitation;
        [Header("Defensive crouch (near-miss suppression)")]
        [SerializeField, Range(0,1)] private float defensiveCrouchProbability=.48f;
        [SerializeField] private Vector2 defensiveCrouchSeconds=new Vector2(.8f,2);
        [SerializeField] private Vector2 defensiveCrouchCooldown=new Vector2(5,8);
        [Tooltip("A continuous burst is one episode; only a quiet gap permits another decision.")]
        [SerializeField, Min(.1f)] private float suppressionQuietSeconds=.65f;
        private float defensiveUntil,nextDefensiveDecision,suppressionUntil;
        [Header("Accumulated near-miss suppression")]
        [SerializeField, Min(1)] private float maximumSuppression=100;
        [SerializeField, Min(0)] private float nearMissSuppression=31.5f;
        [SerializeField, Min(0)] private float suppressionDecayPerSecond=18;
        [SerializeField, Min(0)] private float moderateSuppression=25;
        [SerializeField, Min(0)] private float highSuppression=60;
        [SerializeField, Range(0,1)] private float suppressionRetreatChance=.55f;
        [SerializeField, Min(.1f)] private float suppressionReactionCooldown=6;
        private SuppressionMeter suppression;
        private float nextSuppressionReaction;
        private bool suppressionWithdrawal;
        [Header("Basic Larper equipment reliability")]
        [SerializeField] private bool unreliableWeapon;
        [SerializeField, Range(0,.01f)] private float malfunctionChance=.0075f;
        [SerializeField, Range(.1f,5)] private float malfunctionRecoverySeconds=1.4f;
        [SerializeField, Min(1)] private float malfunctionCooldown=18;
        private float nextMalfunction;
        public int Malfunctions { get; private set; }
        public event System.Action WeaponMalfunctioned;
        private Vector2 withdrawalThreat;
        public float Suppression=>suppression.Sample(Time.time,suppressionDecayPerSecond,suppressionQuietSeconds);
        private float SuppressionSusceptibility=>awareness!=null&&awareness.Personality!=null?
            awareness.Personality.Disposition switch {LarperDisposition.Cowardly=>1.25f,LarperDisposition.Overconfident=>.75f,_=>1}:1;
        private float moraleUntil,morale;
        public float MoraleInfluence=>Time.time<moraleUntil?morale:0;
        public bool CanReactToAllyDefeat=>isActiveAndEnabled&&owner!=null&&!owner.IsDefeated&&Time.time>=panicUntil&&
            !withdrawing&&!IsHoldingCover&&weapon!=null&&!weapon.Cycle.IsReloading&&
            (route==null||!route.IsRunning)&&(traversal==null||!traversal.IsClimbing&&!traversal.ParachuteFlightActive);
        public bool ApplyAllyMorale(bool fearful,float seconds)
        {
            if(!CanReactToAllyDefeat)return false;
            morale=fearful?-1:1;moraleUntil=Time.time+Mathf.Clamp(seconds,.1f,8);
            if(fearful)hesitateUntil=Mathf.Max(hesitateUntil,Time.time+Random.Range(.15f,.45f));
            return true;
        }
        public void ExpressAllyFear()
        {
            if(!CanReactToAllyDefeat)return;
            if(TargetVisible&&motor.IsGrounded&&weapon.ReserveAmmo>0&&weapon.MagazineAmmo<=weapon.Definition.MagazineCapacity*.55f&&Random.value<.5f)
            {StartWithdrawal();return;}
            TryDefensiveCrouch(defensiveCrouchProbability*1.25f);
            hesitateUntil=Mathf.Max(hesitateUntil,Mathf.Min(moraleUntil,Time.time+Random.Range(.4f,.8f)));
        }
        private InsaneDreams.Traversal.PlayerTraversal2D traversal;
        public bool IsDefensivelyCrouching=>Time.time<defensiveUntil;
        private bool CanDefensiveCrouch()=>posture!=null&&motor.IsGrounded&&!owner.IsDefeated&&Time.time>=panicUntil&&
            !withdrawing&&weapon!=null&&!weapon.Cycle.IsReloading&&
            (route==null||!route.IsRunning)&&(traversal==null||!traversal.IsClimbing&&!traversal.ParachuteFlightActive);
        private void Suppressed(float proximity,Vector2 threat)
        {
            float previous=Suppression;
            float influence=SuppressionSusceptibility*(MoraleInfluence<0?1.15f:MoraleInfluence>0?.9f:1);
            float level=suppression.Add(nearMissSuppression*Mathf.Lerp(.4f,1.45f,proximity)*influence,Time.time,maximumSuppression,suppressionDecayPerSecond,suppressionQuietSeconds);
            bool fresh=Time.time>=suppressionUntil;
            suppressionUntil=Time.time+Mathf.Max(.1f,suppressionQuietSeconds);
            if(fresh)TryDefensiveCrouch(defensiveCrouchProbability*(MoraleInfluence<0?1.25f:MoraleInfluence>0?.9f:1));
            if(!CanReactToAllyDefeat||!motor.IsGrounded)return;
            if(level>=highSuppression&&Time.time>=nextSuppressionReaction)
            {
                // Failed rolls also consume the window. Bursts cannot reroll every
                // bullet until everyone retreats; this emits no morale events.
                nextSuppressionReaction=Time.time+Mathf.Max(.1f,suppressionReactionCooldown);
                if(Random.value<Mathf.Clamp01(suppressionRetreatChance*influence))StartWithdrawal(true,threat);
            }
            else if(previous<moderateSuppression&&level>=moderateSuppression&&Time.time>=nextHesitation)
            {hesitateUntil=Mathf.Max(hesitateUntil,Time.time+.25f);nextHesitation=Time.time+1;}
        }
        private void TryDefensiveCrouch(float probability)
        {
            if(Time.time<nextDefensiveDecision||!CanDefensiveCrouch())return;
            // Failed rolls also consume the cooldown; automatic fire cannot reroll
            // until success, even with short pauses between bullets.
            float cooldownMin=Mathf.Max(.1f,defensiveCrouchCooldown.x);
            nextDefensiveDecision=Time.time+Random.Range(cooldownMin,Mathf.Max(cooldownMin,defensiveCrouchCooldown.y));
            if(Random.value>=Mathf.Clamp01(probability))return;
            float durationMin=Mathf.Max(.1f,defensiveCrouchSeconds.x);
            defensiveUntil=Time.time+Random.Range(durationMin,Mathf.Max(durationMin,defensiveCrouchSeconds.y));
            posture.SetInput(Vector2.down);
        }
        private void EndDefensiveCrouch(){defensiveUntil=0;posture?.SetInput(Vector2.zero);posture?.TryStand();}
        public Vector2 HeardPosition=>heardPosition;
        public ThreatAwareness Awareness=>Time.time<underFireUntil?ThreatAwareness.UnderFire:TargetVisible||tracking&&Time.time-lastSeenAt<=visibilityMemory?ThreatAwareness.Alerted:Time.time<heardUntil?ThreatAwareness.Suspicious:ThreatAwareness.Unaware;
        public void Hear(CombatNoiseEvent noise)
        {
            if(owner==null||owner.IsDefeated||noise.Source==gameObject||noise.Team!=CombatTeam.Player)return;
            bool shot=noise.Kind==CombatNoiseKind.Gunfire;
            Vector2 origin=actorBox.bounds.center,segment=noise.End-noise.Position;
            float t=segment.sqrMagnitude>.0001f?Mathf.Clamp01(Vector2.Dot(origin-noise.Position,segment)/segment.sqrMagnitude):0;
            Vector2 closest=noise.Position+segment*t;
            float missDistance=Vector2.Distance(origin,closest);
            bool near=shot&&noise.HitTarget!=owner&&segment.sqrMagnitude>.0001f&&missDistance<=nearMissDistance&&
                !CombatVisibility2D.IsBlocked(closest,origin,geometry,sightHits,transform,noise.Source!=null?noise.Source.transform:null);
            if(near)
            {
                Suppressed(1-missDistance/Mathf.Max(.1f,nearMissDistance),noise.Position);
                underFireUntil=Time.time+.6f;
                if(!TargetVisible&&Time.time>=nextHesitation){hesitateUntil=Time.time+.15f;nextHesitation=Time.time+1;}
            }
            if(shot&&Time.time<nextGunfireHearing)return;
            float range=(shot?gunfireHearingRange:movementHearingRange)*Mathf.Clamp01(noise.Intensity);
            if(!near&&Vector2.SqrMagnitude(origin-noise.Position)>range*range)return;
            if(CombatVisibility2D.IsBlocked(origin,noise.Position,geometry,sightHits,transform,noise.Source!=null?noise.Source.transform:null))range*=obstructedHearingMultiplier;
            if(!near&&Vector2.SqrMagnitude(origin-noise.Position)>range*range)return;
            if(shot)nextGunfireHearing=Time.time+gunfireHearingInterval;
            // Quantization is deterministic uncertainty, not a live target reference.
            heardPosition=new Vector2(Mathf.Round(noise.Position.x/soundPositionUncertainty),Mathf.Round(noise.Position.y/soundPositionUncertainty))*soundPositionUncertainty;
            heardUntil=Time.time+hearingMemorySeconds;
            if(!encounterEnabled){encounterEnabled=true;ThreatNoticed?.Invoke();}
        }
        private float nextCameraLookup;
        private Transform visibleBody;
        public bool HasEngaged => engagementAuthorized;
        // Checkpoint/replay boundary only; normal combat still owns its state machine.
        public void ResetEncounter(Health value)
        {
            nextCombatReposition=repositionStandOff=coverHoldUntil=0;
            nextMalfunction=Time.time+2;
            suppression=default;nextSuppressionReaction=0;suppressionWithdrawal=false;
            nextCrowdCheck=0;crowdBlocked=false;moraleUntil=morale=0;defensiveUntil=nextDefensiveDecision=suppressionUntil=0;
            ReleasePosition();panicUntil=blindUntil=reengageAt=readyAt=nextAimSample=heardUntil=underFireUntil=hesitateUntil=nextGunfireHearing=0;withdrawing=false;selectedCover=null;
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
        [Tooltip("Briefly favor an earlier low-ammo withdrawal under incoming fire. A zero threshold stays zero.")]
        [SerializeField, Range(1,1.25f)] private float suppressedReloadFractionMultiplier=1.2f;
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
        public float ReloadFraction
        {
            get
            {
                float baseline=awareness != null && awareness.Personality != null ? awareness.Personality.Disposition switch
                { LarperDisposition.Cowardly => cowardReloadFraction, LarperDisposition.Overconfident => overconfidentReloadFraction, _ => ordinaryReloadFraction } : ordinaryReloadFraction;
                float social=MoraleInfluence<0?1.25f:MoraleInfluence>0?.85f:1;
                return Mathf.Min(.8f,baseline*social*(Time.time<underFireUntil?Mathf.Clamp(suppressedReloadFractionMultiplier,1,1.25f):1));
            }
        }
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
            coverHoldUntil=0;
            EndDefensiveCrouch();
            hesitateUntil=0;ReleasePosition();panicUntil=Mathf.Max(panicUntil,Time.time+seconds);blindUntil=0;withdrawing=false;weapon?.Cycle.CancelReload();return true;
        }
        public bool TargetVisible { get; private set; }
        public float SpreadAtDistance(float distance) => Mathf.Lerp(closeSpread, distantSpread, Mathf.InverseLerp(closeRange, effectiveRange, distance));
        public EngagementState State { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        private void OnEnable() {nextMalfunction=Time.time+2;nextCombatReposition=repositionStandOff=coverHoldUntil=0;suppression=default;nextSuppressionReaction=0;suppressionWithdrawal=false;engagementAuthorized=false;CombatPositionReservations.Register(this);}
        private void Awake()
        {
            owner = GetComponent<Health>(); patrol = GetComponent<LarperBrain>();
            motor = GetComponent<PlayerMotor2D>(); executor = GetComponent<AttackExecutor>();
            route = GetComponent<AuthoredLadderRoute>();
            awareness = GetComponent<LarperAwareness>();
            actorBox = GetComponent<BoxCollider2D>(); posture = GetComponent<InsaneDreams.Traversal.PlayerPosture2D>();
            traversal = GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>();
            var rig=GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true);visibleBody=rig!=null?rig.torso:transform;
            if (firearm != null) weapon = new RuntimeWeapon(firearm, initialReserveAmmo);
        }
        public void BindTarget(Health value) { nextPerception=0;TargetVisible=false;engagementAuthorized=false;target = value; targetCollider=value!=null?value.GetComponent<Collider2D>():null;tracking = wasVisible = false; blindUntil=0;lastFiredAt=float.NegativeInfinity; }
        private void Update()
        {
            // The existing posture controller owns collider changes and safe standing.
            // Timed suppression reactions share the same posture as cover reloads.
            if(IsDefensivelyCrouching&&!CanDefensiveCrouch())EndDefensiveCrouch();
            bool coverReload=weapon!=null&&weapon.Cycle.IsReloading&&Time.time>=panicUntil&&selectedCover!=null&&selectedCover.isActiveAndEnabled&&
                motor.IsGrounded&&Mathf.Abs(transform.position.y-withdrawalY)<.25f&&
                Mathf.Abs(transform.position.x-RetreatDestination.x)<.65f&&CoveredAt(transform.position.x);
            posture?.SetInput(coverReload||IsDefensivelyCrouching||IsHoldingCover?Vector2.down:Vector2.zero);
            if (owner.IsDefeated) { ReleasePosition();blindUntil=0;withdrawing=false; weapon?.Cycle.CancelReload(); patrol.ClearPursuit(); motor.SetMoveInput(0); State = EngagementState.Defeated; return; }
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
                TargetVisible=delta.sqrMagnitude<=range*range&&!CombatVisibility2D.IsBlocked(SightOrigin,SightTarget,geometry,sightHits,transform,target.transform);
                if(TargetVisible){lastSeenAt=Time.time;lastSeen=target.transform.position;}
            }
            if(previouslyVisible!=TargetVisible)VisibilityChanged?.Invoke(TargetVisible);
            if(TargetVisible&&!encounterEnabled){encounterEnabled=true;ThreatNoticed?.Invoke();}
            if(!encounterEnabled){ReleasePosition();patrol.DecisionsSuspended=true;patrol.ClearPursuit();motor.SetMoveInput(0);State=EngagementState.Patrol;return;}
            // Authored traversal owns the motor. Do not resume a stale retreat after it.
            if (routing) { ReleasePosition();blindUntil=0;withdrawing=false; weapon?.Cycle.CancelReload(); patrol.ClearPursuit(); patrol.DecisionsSuspended=true; State=EngagementState.Patrol; return; }
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
            if (wasReloading && !weapon.Cycle.IsReloading) { ReleasePosition();reengageAt = Time.time + reengagementDelay; posture?.SetInput(Vector2.zero); posture?.TryStand(); }
            if (weapon != null && weapon.Cycle.IsReloading)
            { blindUntil=0;patrol.DecisionsSuspended=true; patrol.SetPursuit(0); State=EngagementState.Reloading; return; }
            if(weapon!=null&&weapon.Cycle.IsMalfunctioning(Time.time))
            {patrol.DecisionsSuspended=true;patrol.SetPursuit(0);State=EngagementState.Reacting;return;}
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
                    {
                        if (!reached && travelled < minimumWithdrawalDistance) WithdrawalFailed?.Invoke();
                        if(suppressionWithdrawal&&weapon.MagazineAmmo>Mathf.FloorToInt(weapon.Definition.MagazineCapacity*ReloadFraction))
                        {
                            withdrawing=suppressionWithdrawal=false;patrol.SetPursuit(0);reengageAt=Time.time+reengagementDelay;State=EngagementState.Searching;
                            if(reached&&selectedCover!=null&&CoveredAt(transform.position.x))coverHoldUntil=Time.time+Random.Range(1.2f,2.4f);
                            else ReleasePosition();
                        }
                        else BeginReload();
                        return;
                    }
                    patrol.SetPursuit(retreatDirection, withdrawalAllowsHop, true);
                }
                return;
            }
            if(IsHoldingCover)
            {
                if(Time.time>=nextPositionSample){nextPositionSample=Time.time+positionSampleSeconds;if(!motor.IsGrounded||!CoveredAt(transform.position.x))coverHoldUntil=0;}
                if(IsHoldingCover){patrol.DecisionsSuspended=true;patrol.SetPursuit(0);State=EngagementState.Reacting;return;}
            }
            if(coverHoldUntil>0){coverHoldUntil=0;selectedCover=null;ReleasePosition();posture?.SetInput(Vector2.zero);}
            if(Time.time<hesitateUntil){patrol.DecisionsSuspended=true;patrol.SetPursuit(0);State=EngagementState.Reacting;return;}
            if (blindUntil>0)
            {
                if (available && Time.time<blindUntil && weapon.MagazineAmmo>0)
                {
                    ReleasePosition();patrol.DecisionsSuspended=true;patrol.SetPursuit(0);State=EngagementState.BlindFiring;
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
                if(Time.time<heardUntil)
                {
                    Vector2 investigate=heardPosition-(Vector2)transform.position;patrol.DecisionsSuspended=true;
                    MoveToCombatPosition(heardPosition,1.2f);
                    if(investigate.sqrMagnitude>.01f)AimDirection=investigate.normalized;State=EngagementState.Searching;return;
                }
                ReleasePosition();if (!routing) patrol.DecisionsSuspended = false;
                State = EngagementState.Patrol; return;
            }
            patrol.DecisionsSuspended = true;
            Vector2 rememberedDelta = lastSeen - (Vector2)transform.position;
            MoveToCombatPosition(lastSeen,preferredDistance*(MoraleInfluence>0?.9f:1));
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
            if(!blind&&CombatVisibility2D.IsBlocked(SightOrigin,SightTarget,geometry,sightHits,transform,target.transform))return;
            float spread = SpreadAtDistance(distance) + (blind ? blindAimUncertainty : 0);
            Vector2 direction = Quaternion.Euler(0, 0, Random.Range(-spread, spread)) * AimDirection;
            // Test the actual imperfect firing direction, not just the direct visibility ray.
            if (!blind && CombatVisibility2D.IsBlocked(SightOrigin,SightOrigin+direction*Mathf.Min(distance,weapon.Definition.AttackRange),geometry,sightHits,transform)) return;
            if(unreliableWeapon&&Time.time>=nextMalfunction&&Random.value<malfunctionChance&&weapon.Cycle.TryMalfunction(Time.time,malfunctionRecoverySeconds))
            {
                nextMalfunction=Time.time+Mathf.Max(1,malfunctionCooldown);Malfunctions++;State=EngagementState.Reacting;
                if(Random.value<.35f)TryDefensiveCrouch(defensiveCrouchProbability);
                WeaponMalfunctioned?.Invoke();return;
            }
            if(executor.Attack(weapon, direction, Time.time)){lastFiredAt=Time.time;engagementAuthorized=true;}
        }
        private void BeginReload()
        {
            suppressionWithdrawal=false;
            EndDefensiveCrouch();
            blindUntil=0;
            withdrawing=false; patrol.SetPursuit(0); motor.SetMoveInput(0);
            if (weapon.Cycle.TryReload(Time.time,Random.Range(1-reloadDurationVariation,1+reloadDurationVariation))) ReloadStarted?.Invoke();
            State=weapon.Cycle.IsReloading?EngagementState.Reloading:EngagementState.Searching;
        }
        private void StartWithdrawal(bool fromSuppression=false,Vector2 threat=default)
        {
            coverHoldUntil=0;
            suppressionWithdrawal=fromSuppression;
            withdrawalThreat=fromSuppression?threat:lastSeen+Vector2.up*.35f;
            EndDefensiveCrouch();
            ReleasePosition();
            blindUntil=0;
            float dx = (fromSuppression?threat.x:lastSeen.x)-transform.position.x;
            retreatDirection = Mathf.Abs(dx) > .1f ? -Mathf.Sign(dx) : -Mathf.Sign(AimDirection.x);
            withdrawalX=transform.position.x;withdrawalY=transform.position.y;selectedCover=null;withdrawalAllowsHop=false;
            if(!PickReachableCover())withdrawalGoal = patrol.WithdrawalReach(retreatDirection, withdrawalDistance);
            while(withdrawalGoal>=minimumWithdrawalDistance&&!CombatPositionReservations.TryClaim(this,RetreatDestination))withdrawalGoal-=minimumCombatSpacing;
            retreatReservation=HasCombatPosition;
            if (withdrawalGoal < minimumWithdrawalDistance) { WithdrawalFailed?.Invoke(); BeginReload(); return; }
            withdrawing=true; withdrawalAnnounced=false; withdrawalStarted=Time.time; withdrawalX=transform.position.x;
            nextTacticalSample=Time.time+tacticalSampleSeconds; State=EngagementState.Withdrawing; patrol.SetPursuit(retreatDirection,withdrawalAllowsHop,true);
        }
        private bool CoveredAt(float x)
        {
            float height=posture!=null?posture.StandingWorldHeight:actorBox.bounds.size.y;
            Vector2 crouchedCentre=new Vector2(x,withdrawalY+height*((posture!=null?posture.CrouchedHeightRatio:.7f)-.5f)-.15f);
            return CombatVisibility2D.IsBlocked(withdrawalThreat,crouchedCentre,geometry,sightHits);
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
                if(distance<minimumWithdrawalDistance||distance>coverSearchDistance||!CoveredAt(x)||!CombatPositionReservations.IsFree(this,new Vector2(x,withdrawalY))||!CombatPositionReservations.IsPathFree(this,new Vector2(x,withdrawalY))||!patrol.CanReachCover(retreatDirection,distance,out bool hop))continue;
                float score=coverSearchDistance-distance-(hop?.75f:0)+Random.Range(-.25f,.25f);
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
            Gizmos.color=Color.cyan;Gizmos.DrawWireSphere(box.bounds.center,detectionRange);
            Gizmos.color=new Color(1,.7f,.2f);Gizmos.DrawWireSphere(box.bounds.center,movementHearingRange);
            if(Application.isPlaying&&HasCombatPosition){Gizmos.color=Color.magenta;Gizmos.DrawWireSphere(CombatPosition,minimumCombatSpacing*.5f);Gizmos.DrawLine(box.bounds.center,CombatPosition);}
            if(Application.isPlaying&&tracking){Gizmos.color=Color.yellow;Gizmos.DrawLine(box.bounds.center,lastSeen);Gizmos.DrawWireSphere(lastSeen,.15f);}
        }
        private void OnDisable() { moraleUntil=morale=0;defensiveUntil=nextDefensiveDecision=suppressionUntil=0;CombatPositionReservations.Remove(this);heardUntil=underFireUntil=hesitateUntil=0;posture?.SetInput(Vector2.zero);posture?.TryStand();selectedCover=null;blindUntil=0;withdrawing=false; tracking = wasVisible = TargetVisible = false; if (patrol != null) { patrol.DecisionsSuspended = false; patrol.ClearPursuit(); } weapon?.Cycle.CancelReload(); }
    }
}
