using InsaneDreams.Prototype;
using UnityEngine;

namespace InsaneDreams.Traversal
{
    [DefaultExecutionOrder(10)]
    [RequireComponent(typeof(PlayerMotor2D))]
    public sealed class PlayerTraversal2D : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float climbSpeed = 4.5f;
        [SerializeField, Min(0)] private float ladderJumpHorizontalSpeed = 7f;
        [SerializeField, Min(0)] private float reattachDelay = 0.3f;
        [SerializeField, Min(0)] private float captureWidthMultiplier = 1.25f;
        [SerializeField, Min(0)] private float bottomCaptureHeightMultiplier = .35f;
        [SerializeField, Range(.1f, 1)] private float entryThreshold = .35f;
        [SerializeField, Range(0, .3f)] private float entryReleaseThreshold = .2f;
        [SerializeField, Min(1)] private float entryDominance = 1.15f;
        [SerializeField, Range(.2f, 1)] private float dismountThreshold = .6f;
        [SerializeField, Min(1)] private float dismountDominance = 1.2f;
        [SerializeField, Min(0)] private float topLandingDrop = 2.5f;
        [SerializeField, Range(.02f,.1f)] private float topExitClearance = .04f;
        [SerializeField, Min(0)] private float sideLandingDrop = .8f;
        [SerializeField] private LayerMask solidLayers = 1;
        [SerializeField, Min(0.1f)] private float parachuteFallSpeed = 2.8f;
        [SerializeField, Range(0.01f, 1)] private float parachuteGravityMultiplier = 0.2f;
        [SerializeField, Range(.1f, 1)] private float parachuteHorizontalMultiplier = .4f;
        [SerializeField, Min(0)] private float parachuteRedeployDelay = .2f;
        [SerializeField] private bool allowAscendingDeployment = true;
        [SerializeField] private GameObject parachuteVisual;
        private Collider2D[] overlaps = new Collider2D[16];
        private PlayerMotor2D motor;
        private Rigidbody2D body;
        private BoxCollider2D playerCollider;
        private PlayerPosture2D posture;
        private InsaneDreams.Combat.PlayerCover2D cover;
        private float normalGravity;
        private float nextAttachTime;
        private float climbInput;
        private float horizontalInput;
        private bool parachuteHeld;
        private float nextParachuteDeploy;
        private bool ownsMotion;
        private bool requireClimbRelease;
        private LadderVolume ladder;
        private bool exiting;
        private float exitX;
        private readonly RaycastHit2D[] solidHits = new RaycastHit2D[16];
        public bool IsClimbing => ladder != null;
        public bool IsParachuting { get; private set; }
        public bool ParachuteFlightActive => IsParachuting && !IsClimbing && motor != null && !motor.IsGrounded;
        public float HorizontalSpeedMultiplier => ParachuteFlightActive ? parachuteHorizontalMultiplier : 1;
        public bool CanUseLadder => FindUsableLadder() != null;
        public bool HasUpwardClimbIntent(Vector2 move) => move.y >= entryThreshold && move.y >= Mathf.Abs(move.x) * entryDominance;
        public bool CanEnterFromTop(Vector2 move)
        {
            if (cover != null && cover.OwnsDownwardInput(move)) return false;
            if (move.y > -entryThreshold || -move.y < Mathf.Abs(move.x) * entryDominance) return false;
            var candidate = FindUsableLadder();
            float reachHeight = posture != null ? posture.StandingWorldHeight : playerCollider.bounds.size.y;
            return candidate != null && body.position.y >= candidate.Top - reachHeight * 1.25f;
        }

        private void Awake()
        {
            motor = GetComponent<PlayerMotor2D>();
            motor.AirJumped+=ClearParachute;
            body = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<BoxCollider2D>();
            posture = GetComponent<PlayerPosture2D>();
            cover = GetComponent<InsaneDreams.Combat.PlayerCover2D>();
            normalGravity = body.gravityScale;
            SetParachute(false);
        }

        public void SetInput(Vector2 move, bool parachute)
        {
            horizontalInput = move.x;
            climbInput = move.y;
            if (parachute && !parachuteHeld) ToggleParachute();
            parachuteHeld = parachute;
            if (Mathf.Abs(move.y) < entryReleaseThreshold) requireClimbRelease = false;
        }

        public void Jump()
        {
            if (!IsClimbing) { motor.RequestJump(); return; }
            LeaveLadder(true);
            motor.ConsumeTraversalJump();
            body.linearVelocity = new Vector2(horizontalInput * ladderJumpHorizontalSpeed, motor.JumpSpeed);
        }

        public bool ToggleParachute()
        {
            if (IsParachuting) { ClearParachute(); return true; }
            if (!isActiveAndEnabled || IsClimbing || Time.time < nextParachuteDeploy ||
                !allowAscendingDeployment && body.linearVelocity.y > 0 || SafeTravel(Vector2.down, .1f) < .06f) return false;
            SetParachute(true); return true;
        }
        public void ClearParachute() { SetParachute(false); nextParachuteDeploy = Time.time + parachuteRedeployDelay; }

        private void FixedUpdate()
        {
            if (ownsMotion && (ladder == null || !ladder.isActiveAndEnabled)) LeaveLadder(true);
            if (Mathf.Abs(climbInput) < entryReleaseThreshold) requireClimbRelease = false;
            var move = new Vector2(horizontalInput, climbInput);
            bool coverOwns = cover != null && (cover.IsCovered || cover.OwnsDownwardInput(move));
            bool entryIntent = !coverOwns && (HasUpwardClimbIntent(move) || CanEnterFromTop(move));
            if (!IsClimbing && entryIntent && Time.time < nextAttachTime) requireClimbRelease = true;
            if (!IsClimbing && !requireClimbRelease && entryIntent && Time.time >= nextAttachTime)
            {
                LadderVolume closest = FindUsableLadder();
                if (closest != null)
                {
                    ladder = closest;
                    ownsMotion = true;
                    motor.enabled = false; // The existing motor remains the sole walking/jumping implementation.
                    body.gravityScale = 0;
                }
            }

            if (IsClimbing)
            {
                if (exiting)
                {
                    float dx = exitX - body.position.x;
                    float speed = Mathf.Clamp(dx / Time.fixedDeltaTime, -ladder.AlignmentSpeed, ladder.AlignmentSpeed);
                    float safe = SafeTravel(Vector2.right * Mathf.Sign(dx), Mathf.Abs(speed) * Time.fixedDeltaTime);
                    if (Mathf.Abs(dx) < .06f || safe < .001f || horizontalInput * dx < -.3f)
                    { body.linearVelocity = Vector2.zero; LeaveLadder(true); }
                    else { body.linearVelocity = new Vector2(Mathf.Sign(dx) * safe / Time.fixedDeltaTime, 0); return; }
                }
                if (!IsClimbing) return;
                bool leaveSide = Mathf.Abs(horizontalInput) >= dismountThreshold && Mathf.Abs(horizontalInput) > Mathf.Abs(climbInput) * dismountDominance;
                // Reach the endpoint and clear the contact skin before moving
                // sideways. The revised decks meet the feet at the marker itself.
                float climbTop=ladder.Top+topExitClearance;
                bool leaveTop = body.position.y >= climbTop - .005f && climbInput > 0;
                bool leaveBottom = climbInput < -entryThreshold && (body.position.y <= ladder.Bottom + 0.1f ||
                    Mathf.Abs(body.position.x - ladder.CenterX) < .1f && SafeTravel(Vector2.down, .08f) < .04f);
                if (leaveTop)
                {
                    body.linearVelocity = Vector2.zero;
                    float preferred = Mathf.Abs(horizontalInput) > .2f ? Mathf.Sign(horizontalInput) : Mathf.Sign(ladder.TopExitHorizontalOffset);
                    if (TryLanding(preferred, topLandingDrop) || TryLanding(-preferred, topLandingDrop)) return;
                    // No landing: hold the endpoint, still allowing descent or a deliberate side exit.
                    if (!leaveSide) return;
                }
                if (leaveSide)
                {
                    float direction = Mathf.Sign(horizontalInput);
                    if (TryLanding(direction, sideLandingDrop)) return;
                    LeaveLadder(true);
                    body.linearVelocity = new Vector2(direction * ladderJumpHorizontalSpeed, 0);
                }
                else if (leaveBottom) { body.linearVelocity = Vector2.zero; LeaveLadder(true); }
                else if (IsClimbing)
                {
                    body.gravityScale = 0;
                    float alignment = Mathf.Clamp((ladder.CenterX - body.position.x) / Time.fixedDeltaTime,
                        -ladder.AlignmentSpeed, ladder.AlignmentSpeed);
                    float dy = Mathf.Clamp(climbInput * climbSpeed * Time.fixedDeltaTime,
                        ladder.Bottom - body.position.y, Mathf.Max(0, climbTop - body.position.y));
                    body.linearVelocity = new Vector2(
                        Mathf.Sign(alignment) * SafeTravel(Vector2.right * Mathf.Sign(alignment), Mathf.Abs(alignment) * Time.fixedDeltaTime) / Time.fixedDeltaTime,
                        Mathf.Sign(dy) * SafeTravel(Vector2.up * Mathf.Sign(dy), Mathf.Abs(dy)) / Time.fixedDeltaTime);
                    return;
                }
            }

            bool controlledDescent = ParachuteFlightActive && body.linearVelocity.y <= 0;
            body.gravityScale = controlledDescent ? normalGravity * parachuteGravityMultiplier : normalGravity;
            if (controlledDescent)
            {
                // Account for the gravity step that follows FixedUpdate.
                float limitBeforeGravity = -parachuteFallSpeed - Physics2D.gravity.y * body.gravityScale * Time.fixedDeltaTime;
                body.linearVelocity = new Vector2(body.linearVelocity.x,
                    Mathf.Max(body.linearVelocity.y, limitBeforeGravity));
            }
        }

        private void LeaveLadder(bool blockUntilRelease = false)
        {
            ladder = null;
            ownsMotion = false;
            exiting = false;
            requireClimbRelease |= blockUntilRelease;
            nextAttachTime = Time.time + reattachDelay;
            if (motor != null) motor.enabled = true;
            if (body != null) body.gravityScale = normalGravity;
        }

        private LadderVolume FindUsableLadder()
        {
            if (playerCollider == null) return null;
            var filter = new ContactFilter2D { useTriggers = true };
            Bounds bounds = playerCollider.bounds;
            Vector2 size = bounds.size;
            size.x += bounds.size.x * captureWidthMultiplier * 2;
            size.y += bounds.size.y * bottomCaptureHeightMultiplier * 2;
            int count = Physics2D.OverlapBox(bounds.center, size, 0, filter, overlaps);
            while (count == overlaps.Length && overlaps.Length < 256)
            {
                System.Array.Resize(ref overlaps, overlaps.Length * 2);
                count = Physics2D.OverlapBox(bounds.center, size, 0, filter, overlaps);
            }
            if (count == overlaps.Length) return null; // Fail closed rather than choose from truncated, unordered results.
            LadderVolume closest = null;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (!overlaps[i].enabled || !overlaps[i].isTrigger ||
                    !overlaps[i].TryGetComponent(out LadderVolume candidate) || !candidate.isActiveAndEnabled) continue;
                float d = Mathf.Abs(body.position.x - candidate.CenterX);
                if (SafeTravel(Vector2.right * Mathf.Sign(candidate.CenterX - body.position.x), d) + .02f < d) continue;
                if (d < distance || d == distance && string.CompareOrdinal(HierarchyKey(candidate.transform), HierarchyKey(closest.transform)) < 0)
                { closest = candidate; distance = d; }
            }
            return closest;
        }

        private static string HierarchyKey(Transform value)
        {
            string key = value.GetSiblingIndex().ToString("D6");
            while (value.parent != null) { value = value.parent; key = value.GetSiblingIndex().ToString("D6") + "/" + key; }
            return value.gameObject.scene.path + "/" + key;
        }

        private float SafeTravel(Vector2 direction, float distance)
        {
            if (distance <= 0) return 0;
            var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(solidLayers);
            Bounds bounds = playerCollider.bounds;
            int count = Physics2D.BoxCast(bounds.center, (Vector2)bounds.size - Vector2.one * .04f,
                0, direction, filter, solidHits, distance + .02f);
            if (count == solidHits.Length) return 0;
            float safe = distance;
            for (int i = 0; i < count; i++)
            {
                if (solidHits[i].collider.transform.IsChildOf(transform)) continue;
                // Tangential floor contacts must not obstruct horizontal alignment.
                if (Vector2.Dot(solidHits[i].normal, direction) >= -.05f) continue;
                safe = Mathf.Min(safe, Mathf.Max(0, solidHits[i].distance - .02f));
            }
            return safe;
        }

        private bool TryLanding(float direction, float maxDrop)
        {
            float distance = Mathf.Max(Mathf.Abs(ladder.TopExitHorizontalOffset), playerCollider.bounds.size.x * 1.5f);
            float x = ladder.CenterX + direction * distance;
            Bounds bounds = playerCollider.bounds;
            var floor = Physics2D.Raycast(new Vector2(x, bounds.min.y + .1f), Vector2.down, maxDrop + .1f, solidLayers);
            if (!floor || floor.normal.y < .65f || floor.collider.isTrigger) return false;
            // Both feet must be supported and the whole horizontal corridor must be clear.
            float half = bounds.extents.x * .9f;
            for (int side = -1; side <= 1; side += 2)
            {
                float edge = side * half;
                var support = Physics2D.Raycast(new Vector2(x + edge, bounds.min.y + .1f), Vector2.down, maxDrop + .1f, solidLayers);
                if (!support || support.normal.y < .65f || Mathf.Abs(support.point.y - floor.point.y) > .15f) return false;
            }
            float dx = x - body.position.x;
            if (SafeTravel(Vector2.right * Mathf.Sign(dx), Mathf.Abs(dx)) + .02f < Mathf.Abs(dx)) return false;
            exitX = x; exiting = true; body.linearVelocity = Vector2.zero; return true;
        }

        private void SetParachute(bool active)
        {
            IsParachuting = active;
            if (body != null && !IsClimbing) body.gravityScale = ParachuteFlightActive && body.linearVelocity.y <= 0 ? normalGravity * parachuteGravityMultiplier : normalGravity;
            if (parachuteVisual != null && parachuteVisual.activeSelf != active) parachuteVisual.SetActive(active);
        }

        public void ResetTraversal()
        {
            LeaveLadder();
            requireClimbRelease = false;
            climbInput = horizontalInput = 0;
            parachuteHeld = false;
            SetParachute(false);
            nextParachuteDeploy = 0;
            if (motor != null)
            {
                motor.enabled = false; // Clear any queued jump before recovery.
                motor.enabled = true;
                motor.SetMoveInput(0);
            }
        }

        private void OnDisable() => ResetTraversal();
        private void OnDestroy(){if(motor!=null)motor.AirJumped-=ClearParachute;}
    }
}
