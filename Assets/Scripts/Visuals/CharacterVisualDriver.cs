using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using InsaneDreams.Checkpoints;
using UnityEngine;

namespace InsaneDreams.Visuals
{
    [DefaultExecutionOrder(100)]
    public sealed class CharacterVisualDriver : MonoBehaviour
    {
        [SerializeField] private CutoutRig rig;
        [SerializeField, Min(1)] private float pixelsPerUnit = 32;
        [SerializeField, Min(.01f)] private float expressionSeconds = 1.4f;
        [SerializeField, Range(.04f,.2f)] private float flinchSeconds=.1f;
        [SerializeField, Range(0,6)] private float flinchDegrees=3;
        [SerializeField, Min(.01f)] private float walkStartSpeed=.3f;
        [SerializeField, Min(0)] private float walkStopSpeed=.1f;
        private int lastPoseFrame=-1;
        private float flinchUntil,flinchSign;
        private float airJumpUntil;
        private void AirJump(){airJumpUntil=Time.time+.18f;if(rig!=null&&rig.Segmented!=null)rig.Segmented.AirJump();}
        private Rigidbody2D body;
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private PlayerPosture2D posture;
        private PlayerCover2D cover;
        private PlayerActions player;
        private PistolMagazineReload pistolReload;
        private OfficerWeaponPresentation officerWeapon;
        private PlayerHealingPresentation healingVisual;
        private WeaponCycle observedReload;
        private void MagazineInserted()=>rig.FinishMagazineExchange();
        private EnemyCombat enemy;
        private SamuraiCombat samurai;
        private FireworksCombat fireworks;
        private LarperAwareness awareness;
        private AttackExecutor attacks;
        private Health health;
        private HitReaction2D hitReaction;
        private PlayerRecovery recovery;
        private InsaneDreams.Inventory.PlayerLoadout loadout;
        private float phase, facing = 1, expressionUntil, recoilUntil;
        private float shotPoseUntil;
        private Vector2 shotAim;
        private float meleeUntil;
        private Vector2 meleeAim;
        public bool ReloadPoseActive { get; private set; }
        private FaceExpression reaction;
        public CutoutRig Rig => rig;
        public CharacterPose CurrentPose { get; private set; }
        public FaceExpression Expression { get; private set; }
        public float Facing => facing;
        public Vector2 Aim { get; private set; } = Vector2.right;
        private void Awake()
        {
            if (rig == null) rig = GetComponentInChildren<CutoutRig>(true);
            body = GetComponent<Rigidbody2D>(); motor = GetComponent<PlayerMotor2D>(); traversal = GetComponent<PlayerTraversal2D>(); posture = GetComponent<PlayerPosture2D>();
            player = GetComponent<PlayerActions>(); enemy = GetComponent<EnemyCombat>(); awareness = GetComponent<LarperAwareness>(); attacks = GetComponent<AttackExecutor>(); health = GetComponent<Health>(); recovery = GetComponent<PlayerRecovery>();
            loadout = GetComponent<InsaneDreams.Inventory.PlayerLoadout>();
            cover = GetComponent<PlayerCover2D>();
            hitReaction=GetComponent<HitReaction2D>();
            samurai=GetComponent<SamuraiCombat>();
            fireworks=GetComponent<FireworksCombat>();
        }
        private void OnEnable()
        {
            pistolReload=GetComponent<PistolMagazineReload>();
            officerWeapon=GetComponent<OfficerWeaponPresentation>();
            healingVisual=GetComponent<PlayerHealingPresentation>();
            if (attacks != null) attacks.Fired += Fired;
            if(motor!=null)motor.AirJumped+=AirJump;
            if (health != null) health.Damaged += Hurt;
            if (awareness != null) { awareness.Reaction += React; awareness.LineDelivered += Line; }
            if (recovery != null) recovery.Recovered += ResetVisuals;
            ResetVisuals();
            var source = GetComponent<SpriteRenderer>(); if (source != null && rig != null) source.enabled = false;
        }
        private void OnDisable()
        {
            if(rig!=null){rig.PoseReloadShell(null,false,0);rig.FinishMagazineExchange();}
            if(observedReload!=null)observedReload.MagazineInserted-=MagazineInserted;observedReload=null;
            if (attacks != null) attacks.Fired -= Fired;
            if(motor!=null)motor.AirJumped-=AirJump;
            if (health != null) health.Damaged -= Hurt;
            if (awareness != null) { awareness.Reaction -= React; awareness.LineDelivered -= Line; }
            if (recovery != null) recovery.Recovered -= ResetVisuals;
        }
        private void ResetVisuals() { lastPoseFrame=-1; phase = recoilUntil = expressionUntil = shotPoseUntil = airJumpUntil = meleeUntil = 0; ReloadPoseActive = false; reaction = Expression = FaceExpression.Neutral; }
        private void Fired(Vector2 direction, Vector2 end, bool hit)
        {
            if(attacks.LastAttackUsesAmmunition)recoilUntil=Time.time+RecoilSeconds;
            else if(player!=null){meleeAim=direction;meleeUntil=Time.time+.22f;}
        }
        private float RecoilSeconds=>loadout!=null&&loadout.SelectedWeapon!=null&&loadout.SelectedWeapon.Definition.FireMode==InsaneDreams.Inventory.WeaponFireMode.ReleaseToFire?.35f:.12f;
        private void Hurt(DamageInfo damage)
        {
            SetReaction(FaceExpression.Stunned);flinchUntil=hitReaction!=null&&hitReaction.UsesLivingPhysics?0:Time.time+flinchSeconds;
            flinchSign=damage.Impact.HasValue?Mathf.Sign(damage.Impact.Value.Direction.x):1;
        }
        private void SetReaction(FaceExpression value) { reaction = value; expressionUntil = Time.time + expressionSeconds; }
        private void React(LarperSpeechEvent context)
        {
            if(context==LarperSpeechEvent.Suppressed)
            {
                SetReaction(FaceExpression.Scared);flinchUntil=Time.time+flinchSeconds*.6f;
                flinchSign=enemy!=null?Mathf.Sign(enemy.AimDirection.x):1;return;
            }
            SetReaction(context == LarperSpeechEvent.Panic || context == LarperSpeechEvent.Withdraw ? FaceExpression.Scared : context == LarperSpeechEvent.MissStreak || context == LarperSpeechEvent.Reload || context == LarperSpeechEvent.Blocked ? FaceExpression.Angry : context == LarperSpeechEvent.Detected || context == LarperSpeechEvent.WitnessDefeat ? FaceExpression.Surprised : FaceExpression.Neutral);
        }
        private void Line(LarperSpeechEvent context)
        {
            if (context != LarperSpeechEvent.Panic && context != LarperSpeechEvent.MissStreak && awareness.Personality != null && awareness.Personality.Disposition == LarperDisposition.Overconfident) SetReaction(FaceExpression.Smug);
        }
        public static CharacterPose ResolvePose(bool defeated, bool panic, bool climbing, bool parachuting, bool crouched, bool grounded, Vector2 velocity, float walkThreshold=.15f)
        {
            if (defeated) return CharacterPose.Defeated;
            if (panic) return CharacterPose.Panic;
            if (climbing) return CharacterPose.Climb;
            if (parachuting) return CharacterPose.Parachute;
            if (crouched) return Mathf.Abs(velocity.x) > walkThreshold ? CharacterPose.CrouchWalk : CharacterPose.Crouch;
            if (!grounded) return velocity.y > .1f ? CharacterPose.Jump : CharacterPose.Fall;
            return Mathf.Abs(velocity.x) > walkThreshold ? CharacterPose.Walk : CharacterPose.Idle;
        }
        public void PrepareShot(Vector2 direction)
        {
            shotAim = direction; shotPoseUntil = Time.time + .06f;
            RefreshPose();
        }
        private void LateUpdate() => RefreshPose();
        private void RefreshPose()
        {
            if (rig == null || !rig.IsBound || body == null) return;
            bool advance=lastPoseFrame!=Time.frameCount;lastPoseFrame=Time.frameCount;
            bool walking=CurrentPose==CharacterPose.Walk||CurrentPose==CharacterPose.CrouchWalk;
            float threshold=player!=null?(walking?Mathf.Min(walkStopSpeed,walkStartSpeed):walkStartSpeed):.15f;
            CurrentPose = ResolvePose(health != null && health.IsDefeated, enemy != null && enemy.State == EngagementState.Panicked,
                traversal != null && traversal.IsClimbing, traversal != null && traversal.ParachuteFlightActive, posture != null && posture.IsCrouched||player!=null&&player.IsAirSliding, motor != null && motor.IsGrounded, body.linearVelocity,threshold);
            // Contact impulses near zero are not a new player-facing command. Keep
            // independent aiming, and leave enemy velocity-driven facing unchanged.
            if(player!=null){if(Mathf.Abs(player.Move.x)>.1f)facing=Mathf.Sign(player.Move.x);}
            else if(Mathf.Abs(body.linearVelocity.x)>.15f)facing=Mathf.Sign(body.linearVelocity.x);
            if(samurai!=null&&samurai.StrikePoseActive&&Mathf.Abs(samurai.CommittedAim.x)>.01f)facing=Mathf.Sign(samurai.CommittedAim.x);
            Aim = player != null ? player.AimDirection : enemy != null && (enemy.TargetVisible || enemy.IsBlindFiring) ? enemy.AimDirection : new Vector2(facing, 0);
            if(officerWeapon!=null)Aim=officerWeapon.DisplayAim(Aim);
            bool shotPose = Time.time < shotPoseUntil;
            if (shotPose) Aim = shotAim;
            if (advance) phase += Time.deltaTime * (CurrentPose == CharacterPose.Climb ? Mathf.Abs(body.linearVelocity.y) * 3 : Mathf.Abs(body.linearVelocity.x) * 3);
            Expression = CurrentPose == CharacterPose.Panic ? FaceExpression.Scared : Time.time < expressionUntil ? reaction : FaceExpression.Neutral;
            var definition = loadout != null ? loadout.SelectedWeapon?.Definition : enemy != null ? enemy.Weapon?.Definition : null;
            // Resolve once: assigning a fallback and then its equipment override each
            // frame dirties sprite geometry/batching even though the final art is unchanged.
            var weaponSprite = loadout != null ? loadout.SelectedSlot == InsaneDreams.Inventory.EquipmentSlot.Melee ? rig.skin.meleeWeapon :
                loadout.SelectedSlot == InsaneDreams.Inventory.EquipmentSlot.Firearm2 && rig.skin.secondaryWeapon != null ? rig.skin.secondaryWeapon : rig.skin.weapon : rig.weapon.sprite;
            if (definition != null && definition.EquippedSprite != null) weaponSprite = definition.EquippedSprite;
            if (rig.weapon.sprite != weaponSprite) rig.weapon.sprite = weaponSprite;
            rig.SetMagazine(definition != null ? definition.MagazineSprite : null,definition?.MagazineMountOffset,definition!=null&&definition.PhysicalMagazineReload);
            if(samurai!=null)rig.weapon.color=samurai.Winding?new Color(1,.45f,.25f):Color.white;
            if(fireworks!=null)rig.weapon.color=fireworks.Preparing?Color.yellow:Color.white;
            var cycle = loadout != null ? loadout.SelectedWeapon?.Cycle : enemy != null ? enemy.Weapon?.Cycle : null;
            if(cycle!=observedReload){if(observedReload!=null)observedReload.MagazineInserted-=MagazineInserted;observedReload=cycle;if(cycle!=null)cycle.MagazineInserted+=MagazineInserted;}
            ReloadPoseActive = cycle != null && cycle.IsReloading && health != null && !health.IsDefeated;
            rig.Pose(CurrentPose, phase, Aim, facing, Mathf.Clamp01((recoilUntil - Time.time) / RecoilSeconds)*(definition!=null?definition.RecoilScale:1), Expression,
                shotPose || (player != null ? player.HasAimIntent || player.FireHeld : enemy != null && (enemy.TargetVisible || enemy.IsBlindFiring)), loadout == null || loadout.SelectedWeapon != null,
                ReloadPoseActive, cycle != null ? cycle.ReloadProgress(Time.time) : 0,
                cover != null && cover.IsCovered ? cover.VisualCrouchAmount : -1,
                cover != null && cover.IsCovered ? cover.UpperBodyTuck : 0, cover != null && cover.RaiseWeapon,advance);
            if(player!=null&&definition!=null&&loadout.SelectedSlot==InsaneDreams.Inventory.EquipmentSlot.Melee&&Time.time<meleeUntil)
            {
                float baseAngle=Mathf.Atan2(meleeAim.y,meleeAim.x*facing)*Mathf.Rad2Deg;
                float progress=Mathf.Clamp01(1-(meleeUntil-Time.time)/.22f);
                rig.PoseSamurai(baseAngle+Mathf.Lerp(60,-50,Mathf.SmoothStep(0,1,progress)));
            }
            // Deployment remains authoritative even while grounded or climbing. Only the
            rig.PoseReloadShell(definition?.ShellSprite,ReloadPoseActive&&definition!=null&&definition.LoadsIndividually,cycle!=null?cycle.ReloadProgress(Time.time):0);
            if(pistolReload!=null)pistolReload.Pose(rig,loadout!=null?loadout.SelectedWeapon:enemy!=null?enemy.Weapon:null,ReloadPoseActive,cycle!=null?cycle.ReloadProgress(Time.time):0);
            if(player==null&&enemy!=null&&ReloadPoseActive&&pistolReload!=null&&pistolReload.Handles(definition)&&rig.Segmented!=null)
                rig.Segmented.PosePistolExchange(cycle.ReloadProgress(Time.time));
            float flinch=Mathf.Clamp01((flinchUntil-Time.time)/flinchSeconds)*flinchDegrees*flinchSign*facing;
            rig.torso.localRotation*=Quaternion.Euler(0,0,-flinch);rig.head.localRotation*=Quaternion.Euler(0,0,flinch);
            if(samurai!=null&&rig.Segmented==null&&CurrentPose!=CharacterPose.Defeated&&CurrentPose!=CharacterPose.Climb&&CurrentPose!=CharacterPose.Panic)
                rig.PoseSamurai((samurai.StrikePoseActive?Mathf.Atan2(samurai.CommittedAim.y,samurai.CommittedAim.x*facing)*Mathf.Rad2Deg:0)+
                    (CurrentPose==CharacterPose.Jump&&!samurai.StrikePoseActive?-35:samurai.BladeAngle));
            if(fireworks!=null)
            {
                rig.PoseFireworks(fireworks.HandAngle,fireworks.HoldingVisual);
                if(fireworks.Surprised){Expression=FaceExpression.Surprised;rig.SetExpression(Expression);}
            }
            if(hitReaction!=null)rig.transform.localRotation=Quaternion.Euler(0,0,-hitReaction.Lean*hitReaction.Direction*facing);
            if(player!=null&&Time.time<airJumpUntil&&CurrentPose==CharacterPose.Jump)
            {
                float kick=Mathf.Sin(Mathf.PI*Mathf.Clamp01((airJumpUntil-Time.time)/.18f))*14;
                rig.leftLeg.localRotation*=Quaternion.Euler(0,0,kick);
                rig.rightLeg.localRotation*=Quaternion.Euler(0,0,-kick);
                rig.SetExpression(FaceExpression.Surprised);
            }
            // airborne pose raises the arms; normal grounded locomotion remains available.
            if (traversal != null) rig.parachute.enabled = traversal.IsParachuting && !health.IsDefeated;
            // The player already has an interpolated Rigidbody presentation. Rounding
            // it again produces 1/32-unit stalls/steps, most obvious on reversals.
            // Keep the existing pixel grid for enemies and the warehouse camera.
            Vector3 p = transform.position;
            rig.transform.position = player!=null?p:new Vector3(Mathf.Round(p.x * pixelsPerUnit) / pixelsPerUnit, Mathf.Round(p.y * pixelsPerUnit) / pixelsPerUnit, p.z);
            rig.transform.position+=new Vector3(rig.PresentationOffset.x*facing,rig.PresentationOffset.y+rig.GroundAlignmentOffset,0);
            if(rig.Segmented!=null&&samurai!=null&&CurrentPose!=CharacterPose.Defeated)
                rig.Segmented.Pose(CurrentPose,phase,(samurai.StrikePoseActive?Mathf.Atan2(samurai.CommittedAim.y,samurai.CommittedAim.x*facing)*Mathf.Rad2Deg:0)+samurai.BladeAngle,motor.IsGrounded,advance?Time.deltaTime:0);
            if(definition!=null&&definition.CustomMuzzle)rig.ApplyWeaponMuzzle(definition.MuzzleOffset,(CurrentPose==CharacterPose.Crouch||CurrentPose==CharacterPose.CrouchWalk)&&!(cover!=null&&cover.RaiseWeapon));
            if (cover != null && cover.RaiseWeapon && !ReloadPoseActive) rig.RaiseWeaponOverCover(cover.CoverTop, cover.NearEdge);
            officerWeapon?.Pose(rig,ReloadPoseActive,CurrentPose);
            healingVisual?.Pose(rig);
            hitReaction?.ApplyPose(facing,ReloadPoseActive);
            attacks?.ConstrainMuzzle();
        }
    }
}
