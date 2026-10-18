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
        private Rigidbody2D body;
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private PlayerPosture2D posture;
        private PlayerActions player;
        private EnemyCombat enemy;
        private LarperAwareness awareness;
        private AttackExecutor attacks;
        private Health health;
        private PlayerRecovery recovery;
        private InsaneDreams.Inventory.PlayerLoadout loadout;
        private float phase, facing = 1, expressionUntil, recoilUntil;
        private float shotPoseUntil;
        private Vector2 shotAim;
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
        }
        private void OnEnable()
        {
            if (attacks != null) attacks.Fired += Fired;
            if (health != null) health.Damaged += Hurt;
            if (awareness != null) { awareness.Reaction += React; awareness.LineDelivered += Line; }
            if (recovery != null) recovery.Recovered += ResetVisuals;
            ResetVisuals();
            var source = GetComponent<SpriteRenderer>(); if (source != null && rig != null) source.enabled = false;
        }
        private void OnDisable()
        {
            if (attacks != null) attacks.Fired -= Fired;
            if (health != null) health.Damaged -= Hurt;
            if (awareness != null) { awareness.Reaction -= React; awareness.LineDelivered -= Line; }
            if (recovery != null) recovery.Recovered -= ResetVisuals;
        }
        private void ResetVisuals() { phase = recoilUntil = expressionUntil = shotPoseUntil = 0; ReloadPoseActive = false; reaction = Expression = FaceExpression.Neutral; }
        private void Fired(Vector2 direction, Vector2 end, bool hit) => recoilUntil = Time.time + .12f;
        private void Hurt(DamageInfo _) => SetReaction(FaceExpression.Stunned);
        private void SetReaction(FaceExpression value) { reaction = value; expressionUntil = Time.time + expressionSeconds; }
        private void React(LarperSpeechEvent context)
        {
            SetReaction(context == LarperSpeechEvent.Panic || context == LarperSpeechEvent.Withdraw ? FaceExpression.Scared : context == LarperSpeechEvent.MissStreak || context == LarperSpeechEvent.Reload || context == LarperSpeechEvent.Blocked ? FaceExpression.Angry : context == LarperSpeechEvent.Detected || context == LarperSpeechEvent.WitnessDefeat ? FaceExpression.Surprised : FaceExpression.Neutral);
        }
        private void Line(LarperSpeechEvent context)
        {
            if (context != LarperSpeechEvent.Panic && context != LarperSpeechEvent.MissStreak && awareness.Personality != null && awareness.Personality.Disposition == LarperDisposition.Overconfident) SetReaction(FaceExpression.Smug);
        }
        public static CharacterPose ResolvePose(bool defeated, bool panic, bool climbing, bool parachuting, bool crouched, bool grounded, Vector2 velocity)
        {
            if (defeated) return CharacterPose.Defeated;
            if (panic) return CharacterPose.Panic;
            if (climbing) return CharacterPose.Climb;
            if (parachuting) return CharacterPose.Parachute;
            if (crouched) return Mathf.Abs(velocity.x) > .15f ? CharacterPose.CrouchWalk : CharacterPose.Crouch;
            if (!grounded) return velocity.y > .1f ? CharacterPose.Jump : CharacterPose.Fall;
            return Mathf.Abs(velocity.x) > .15f ? CharacterPose.Walk : CharacterPose.Idle;
        }
        public void PrepareShot(Vector2 direction)
        {
            shotAim = direction; shotPoseUntil = Time.time + .06f;
            RefreshPose(false);
        }
        private void LateUpdate() => RefreshPose(true);
        private void RefreshPose(bool advance)
        {
            if (rig == null || !rig.IsBound || body == null) return;
            CurrentPose = ResolvePose(health != null && health.IsDefeated, enemy != null && enemy.State == EngagementState.Panicked,
                traversal != null && traversal.IsClimbing, traversal != null && traversal.ParachuteFlightActive, posture != null && posture.IsCrouched, motor != null && motor.IsGrounded, body.linearVelocity);
            if (Mathf.Abs(body.linearVelocity.x) > .15f) facing = Mathf.Sign(body.linearVelocity.x);
            Aim = player != null ? player.AimDirection : enemy != null && (enemy.TargetVisible || enemy.IsBlindFiring) ? enemy.AimDirection : new Vector2(facing, 0);
            bool shotPose = Time.time < shotPoseUntil;
            if (shotPose) Aim = shotAim;
            if (advance) phase += Time.deltaTime * (CurrentPose == CharacterPose.Climb ? Mathf.Abs(body.linearVelocity.y) * 3 : Mathf.Abs(body.linearVelocity.x) * 3);
            Expression = CurrentPose == CharacterPose.Panic ? FaceExpression.Scared : Time.time < expressionUntil ? reaction : FaceExpression.Neutral;
            if (loadout != null)
                rig.weapon.sprite = loadout.SelectedSlot == InsaneDreams.Inventory.EquipmentSlot.Melee ? rig.skin.meleeWeapon :
                    loadout.SelectedSlot == InsaneDreams.Inventory.EquipmentSlot.Firearm2 && rig.skin.secondaryWeapon != null ? rig.skin.secondaryWeapon : rig.skin.weapon;
            var definition = loadout != null ? loadout.SelectedWeapon?.Definition : enemy != null ? enemy.Weapon?.Definition : null;
            if (definition != null && definition.EquippedSprite != null) rig.weapon.sprite = definition.EquippedSprite;
            rig.SetMagazine(definition != null ? definition.MagazineSprite : null);
            var cycle = loadout != null ? loadout.SelectedWeapon?.Cycle : enemy != null ? enemy.Weapon?.Cycle : null;
            ReloadPoseActive = cycle != null && cycle.IsReloading && health != null && !health.IsDefeated;
            rig.Pose(CurrentPose, phase, Aim, facing, Mathf.Clamp01((recoilUntil - Time.time) / .12f), Expression,
                shotPose || (player != null ? player.HasAimIntent || player.FireHeld : enemy != null && (enemy.TargetVisible || enemy.IsBlindFiring)), loadout == null || loadout.SelectedWeapon != null,
                ReloadPoseActive, cycle != null ? cycle.ReloadProgress(Time.time) : 0);
            // Deployment remains authoritative even while grounded or climbing. Only the
            // airborne pose raises the arms; normal grounded locomotion remains available.
            if (traversal != null) rig.parachute.enabled = traversal.IsParachuting && !health.IsDefeated;
            // Snap only the visual child; physics and independent aiming retain full precision.
            Vector3 p = transform.position;
            rig.transform.position = new Vector3(Mathf.Round(p.x * pixelsPerUnit) / pixelsPerUnit, Mathf.Round(p.y * pixelsPerUnit) / pixelsPerUnit, p.z);
            attacks?.ConstrainMuzzle();
        }
    }
}
