using InsaneDreams.Prototype;
using UnityEngine;

namespace InsaneDreams.Traversal
{
    [DefaultExecutionOrder(-30)]
    [RequireComponent(typeof(BoxCollider2D), typeof(PlayerMotor2D))]
    public sealed class PlayerPosture2D : MonoBehaviour
    {
        [SerializeField, Range(.3f, .85f)] private float heightMultiplier = .7f;
        [SerializeField, Range(.1f, 1)] private float speedMultiplier = .45f;
        [SerializeField, Range(.1f, 1)] private float downwardThreshold = .35f;
        [SerializeField, Range(0, .3f)] private float releaseThreshold = .2f;
        [SerializeField] private LayerMask solids = 1;
        private readonly Collider2D[] overlaps = new Collider2D[16];
        private BoxCollider2D box;
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private Vector2 standingSize, standingOffset, move;
        private float jumpStandUntil;
        private bool coverHeightActive;
        public bool IsCrouched { get; private set; }
        public float SpeedMultiplier => IsCrouched ? speedMultiplier : 1;
        public Vector2 WorldCenter => box.bounds.center;
        public Vector2 LocalCenter => box.offset;
        public float HeightRatio => box.size.y / standingSize.y;
        public float CurrentWorldHeight => box.size.y * Mathf.Abs(transform.lossyScale.y);
        public float CrouchedHeightRatio => heightMultiplier;
        public float StandingWorldHeight => standingSize.y * Mathf.Abs(transform.lossyScale.y);
        private void Awake()
        {
            box = GetComponent<BoxCollider2D>(); motor = GetComponent<PlayerMotor2D>(); traversal = GetComponent<PlayerTraversal2D>();
            standingSize = box.size; standingOffset = box.offset;
        }
        public void SetInput(Vector2 value) => move = value;
        private void FixedUpdate()
        {
            if (traversal != null && (traversal.IsClimbing || traversal.CanEnterFromTop(move))) { TryStand(); return; }
            if (motor.IsGrounded && move.y <= -downwardThreshold && Time.time >= jumpStandUntil) Crouch();
            else if (move.y >= -releaseThreshold || !motor.IsGrounded) TryStand();
        }
        private void Crouch()
        {
            if (IsCrouched) { if (!coverHeightActive) ClearCoverHeight(); return; }
            box.size = new Vector2(standingSize.x, standingSize.y * heightMultiplier);
            box.offset = standingOffset + Vector2.down * (standingSize.y - box.size.y) * .5f;
            IsCrouched = true;
        }
        // Cover changes the existing damageable box, keeping the feet fixed. Expansion
        // uses the same solid clearance rule as standing up; no temporary invulnerability.
        public bool TrySetCoverHeight(float worldHeight)
        {
            if (!IsCrouched) return false;
            coverHeightActive = true;
            float height = Mathf.Clamp(worldHeight / Mathf.Max(.001f, Mathf.Abs(transform.lossyScale.y)), standingSize.y * .45f, standingSize.y);
            Vector2 size = new Vector2(standingSize.x, height);
            Vector2 offset = standingOffset + Vector2.down * (standingSize.y - height) * .5f;
            if (height > box.size.y + .001f)
            {
                var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(solids);
                int count = Physics2D.OverlapBox(transform.TransformPoint(offset), Vector2.Scale(size, transform.lossyScale) - Vector2.one * .04f, 0, filter, overlaps);
                if (count == overlaps.Length) return false;
                for (int i = 0; i < count; i++) if (!overlaps[i].transform.IsChildOf(transform)) return false;
            }
            box.size = size; box.offset = offset; return true;
        }
        public void ClearCoverHeight() { if (IsCrouched) TrySetCoverHeight(StandingWorldHeight * heightMultiplier); coverHeightActive = false; }
        public bool TryStand()
        {
            if (!IsCrouched) return true;
            var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(solids);
            Vector2 size = Vector2.Scale(standingSize, transform.lossyScale) - Vector2.one * .04f;
            int count = Physics2D.OverlapBox(transform.TransformPoint(standingOffset), size, 0, filter, overlaps);
            if (count == overlaps.Length) return false;
            for (int i = 0; i < count; i++) if (!overlaps[i].transform.IsChildOf(transform)) return false;
            box.size = standingSize; box.offset = standingOffset; IsCrouched = false; coverHeightActive = false; return true;
        }
        public bool PrepareJump()
        {
            if (!TryStand()) return false;
            jumpStandUntil = Time.time + .1f; return true;
        }
        public void ResetPosture() { move = Vector2.zero; jumpStandUntil = 0; TryStand(); }
        private void OnDisable() => ResetPosture();
    }
}
