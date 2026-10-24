using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using UnityEngine;

namespace InsaneDreams.Combat
{
    public enum PlayerCoverStance { None, Concealed, BlindFire, Peek }

    // Local player extension: bounded, non-allocating geometry probes at 10 Hz while
    // crouching. No cover points, root movement, AI notifications or damage immunity.
    [DefaultExecutionOrder(40)]
    [RequireComponent(typeof(PlayerPosture2D), typeof(PlayerActions))]
    public sealed class PlayerCover2D : MonoBehaviour
    {
        [SerializeField, Min(.05f)] private float interactionDistance = .55f;
        [SerializeField, Min(.1f)] private float minimumHeight = .85f;
        [SerializeField, Min(.1f)] private float maximumHeight = 1.15f;
        [SerializeField, Min(.1f)] private float peekExposureHeight = .45f;
        [SerializeField, Min(.02f)] private float transitionSeconds = .14f;
        [SerializeField, Min(1)] private float blindSpreadMultiplier = 5;
        [Tooltip("Reference spread for the currently perfectly accurate player weapons, before the multiplier.")]
        [SerializeField, Range(1, 30)] private float minimumBlindSpreadDegrees = 5;
        [SerializeField] private LayerMask solids = 1;
        private readonly Collider2D[] candidates = new Collider2D[16];
        private readonly RaycastHit2D[] samples = new RaycastHit2D[8];
        private PlayerActions actions;
        private PlayerPosture2D posture;
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private PlayerLoadout loadout;
        private Health health;
        private Rigidbody2D body;
        private BoxCollider2D box;
        private float nextProbe, side;
        private float nextIntentProbe, intentSide;
        private Collider2D intentObstacle;
        // Resolve downward input before posture/traversal acquire a nearby ladder.
        // This shares the existing bounded cover probe, cached at 10 Hz.
        public bool OwnsDownwardInput(Vector2 move)
        {
            if (!isActiveAndEnabled || move.y >= -.2f || motor == null || !motor.IsGrounded ||
                !motor.enabled || !actions.enabled || health.IsDefeated ||
                loadout?.SelectedWeapon == null || !loadout.SelectedWeapon.UsesAmmunition ||
                traversal != null && (traversal.IsClimbing || traversal.ParachuteFlightActive)) return false;
            float direction = Mathf.Abs(actions.AimDirection.x) > .1f ? Mathf.Sign(actions.AimDirection.x) : 1;
            if (Time.time >= nextIntentProbe || intentSide != direction)
            {
                intentSide = direction; nextIntentProbe = Time.time + .1f;
                float previous = side; side = direction; intentObstacle = FindCover(); side = previous;
            }
            return intentObstacle != null && intentObstacle.enabled && intentObstacle.gameObject.activeInHierarchy;
        }
        public Collider2D Obstacle { get; private set; }
        public PlayerCoverStance Stance { get; private set; }
        public bool IsCovered => Stance != PlayerCoverStance.None;
        public bool IsBlindFiring => Stance == PlayerCoverStance.BlindFire;
        public bool PeekReady => Stance == PlayerCoverStance.Peek && box.bounds.max.y >= CoverTop + Mathf.Min(.2f, peekExposureHeight * .5f);
        public bool RaiseWeapon => IsBlindFiring || Stance == PlayerCoverStance.Peek;
        public float CoverTop => Obstacle != null ? Obstacle.bounds.max.y : box.bounds.min.y;
        public float NearEdge => side > 0 ? Obstacle.bounds.min.x : Obstacle.bounds.max.x;
        public float VisualCrouchAmount => Mathf.Clamp01((1 - posture.HeightRatio) / (1 - posture.CrouchedHeightRatio));
        public float UpperBodyTuck => Mathf.Min(0, posture.CurrentWorldHeight - posture.StandingWorldHeight * posture.CrouchedHeightRatio);
        public float BlindSpread(float ordinarySpread) => Mathf.Max(minimumBlindSpreadDegrees, ordinarySpread) * blindSpreadMultiplier;
        private void Awake()
        {
            actions = GetComponent<PlayerActions>(); posture = GetComponent<PlayerPosture2D>(); motor = GetComponent<PlayerMotor2D>();
            traversal = GetComponent<PlayerTraversal2D>(); loadout = GetComponent<PlayerLoadout>(); health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>(); box = GetComponent<BoxCollider2D>();
        }
        private void OnEnable() { actions.InputReset += ResetCover; }
        private void OnDisable() { actions.InputReset -= ResetCover; ResetCover(); }
        public void ResetCover()
        {
            Obstacle = null; Stance = PlayerCoverStance.None; nextProbe = 0;
            if (posture != null) posture.ClearCoverHeight();
        }
        private bool Eligible => actions.enabled && motor != null && motor.enabled && motor.IsGrounded && posture.IsCrouched && actions.Move.y < -.2f &&
            body.simulated && body.bodyType == RigidbodyType2D.Dynamic && !health.IsDefeated &&
            (traversal == null || !traversal.IsClimbing && !traversal.ParachuteFlightActive) &&
            loadout != null && loadout.SelectedWeapon != null && loadout.SelectedWeapon.UsesAmmunition;
        private void Update()
        {
            if (!Eligible) { if (IsCovered) ResetCover(); return; }
            float direction = Mathf.Abs(actions.AimDirection.x) > .1f ? Mathf.Sign(actions.AimDirection.x) : side == 0 ? 1 : side;
            if (Time.time >= nextProbe || direction != side || Obstacle != null && (!Obstacle.enabled || !Obstacle.gameObject.activeInHierarchy))
            {
                side = direction; nextProbe = Time.time + .1f; Obstacle = FindCover();
            }
            if (Obstacle == null) { if (IsCovered) { ResetCover(); nextProbe = Time.time + .1f; } return; }
            bool reloading = loadout.SelectedWeapon.Cycle.IsReloading;
            Stance = !reloading && actions.HasAimIntent ? PlayerCoverStance.Peek :
                !reloading && actions.FireHeld ? PlayerCoverStance.BlindFire : PlayerCoverStance.Concealed;
            float height = Stance == PlayerCoverStance.Peek ? Mathf.Min(posture.StandingWorldHeight, CoverTop - box.bounds.min.y + peekExposureHeight) :
                Mathf.Min(posture.StandingWorldHeight * posture.CrouchedHeightRatio, CoverTop - box.bounds.min.y - .05f);
            posture.TrySetCoverHeight(Mathf.MoveTowards(posture.CurrentWorldHeight, height, Time.deltaTime * posture.StandingWorldHeight / transitionSeconds));
        }
        private Collider2D FindCover()
        {
            Bounds player = box.bounds;
            var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(solids);
            Vector2 center = new Vector2(player.center.x + side * (player.extents.x + interactionDistance * .5f), player.min.y + maximumHeight * .5f);
            int count = Physics2D.OverlapBox(center, new Vector2(interactionDistance, maximumHeight), 0, filter, candidates);
            Collider2D best = null; float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var candidate = candidates[i]; var bounds = candidate.bounds;
                if (candidate.transform.IsChildOf(transform) || candidate.usedByEffector && candidate.GetComponentInParent<PlatformEffector2D>() != null) continue;
                float height = bounds.max.y - player.min.y;
                float gap = side > 0 ? bounds.min.x - player.max.x : player.min.x - bounds.max.x;
                if (height < minimumHeight || height > maximumHeight || Mathf.Abs(bounds.min.y - player.min.y) > .25f || gap < -.02f || gap > interactionDistance || gap >= nearest) continue;
                // Bounds alone are insufficient for irregular/empty geometry: a real
                // solid face must intersect the incoming horizontal body-height path.
                Vector2 start = new Vector2(player.center.x, bounds.max.y - .12f);
                int hits = Physics2D.Raycast(start, Vector2.right * side, filter, samples, player.extents.x + gap + bounds.size.x + .02f);
                bool blocks = false;
                for (int h = 0; h < hits; h++) if (samples[h].collider == candidate) { blocks = true; break; }
                if (blocks) { best = candidate; nearest = gap; }
            }
            return best;
        }
    }
}
