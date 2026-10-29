using UnityEngine;

namespace InsaneDreams.Prototype
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        [SerializeField, Min(0)] private float moveSpeed = 7f;
        [SerializeField, Min(0)] private float acceleration = 65f;
        [SerializeField, Min(0)] private float deceleration = 85f;
        [SerializeField, Min(0)] private float jumpSpeed = 12f;
        [SerializeField] private bool allowAirJump;
        [SerializeField, Range(.5f,1)] private float airJumpSpeedMultiplier=.85f;
        [SerializeField,Min(1)] private float airDiveSpeed=14;
        private bool diveRequested;
        public void RequestAirDive(){if(allowAirJump&&jumpsUsed==2)diveRequested=true;}
        public void CancelAirDive()=>diveRequested=false;
        private int jumpsUsed;
        public int JumpsUsed=>jumpsUsed;
        public float JumpSpeed=>jumpSpeed;
        public float AirJumpSpeed=>jumpSpeed*airJumpSpeedMultiplier;
        public float MoveSpeed=>moveSpeed;
        public event System.Action AirJumped;
        public void ResetJumpBudget()=>jumpsUsed=0;
        public void ConsumeTraversalJump()=>jumpsUsed=1;
        [SerializeField, Min(0)] private float groundProbeDistance = 0.08f;
        [SerializeField] private LayerMask groundLayers = 1;
        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
        private Rigidbody2D body;
        private BoxCollider2D box;
        private float moveInput;
        private bool jumpRequested;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
        }

        // Desktop and future touch adapters share this input boundary.
        public void SetMoveInput(float value) => moveInput = Mathf.Clamp(value, -1f, 1f);
        public void RequestJump() => jumpRequested = true;

        private void FixedUpdate()
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(groundLayers);
            filter.useTriggers = false;
            int count = box.Cast(Vector2.down, filter, groundHits, groundProbeDistance);
            IsGrounded = false;
            for (int i = 0; i < count; i++)
                if (groundHits[i].normal.y > 0.65f && body.linearVelocity.y <= 0.1f)
                    IsGrounded = true;

            Vector2 velocity = body.linearVelocity;
            float rate = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput * moveSpeed, rate * Time.fixedDeltaTime);
            if(IsGrounded)jumpsUsed=0;
            if (jumpRequested && (IsGrounded || allowAirJump&&jumpsUsed<2))
            {
                bool airborne=!IsGrounded;
                velocity.y = jumpSpeed*(airborne?airJumpSpeedMultiplier:1);
                jumpsUsed=airborne?2:1;
                IsGrounded = false;
                if(airborne)AirJumped?.Invoke();
            }
            jumpRequested = false;
            if(diveRequested&&!IsGrounded&&jumpsUsed==2)velocity.y=Mathf.Min(velocity.y,-airDiveSpeed);
            diveRequested=false;
            body.linearVelocity = velocity;
        }

        private void OnDisable()
        {
            moveInput = 0;
            jumpRequested = false;
            diveRequested=false;
        }
    }
}
