using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using UnityEngine;

namespace InsaneDreams.Enemies
{
    public enum LadderRouteState { Unconfigured, Approaching, Climbing, Complete, Unavailable }
    [DefaultExecutionOrder(-5)]
    [RequireComponent(typeof(LarperBrain), typeof(PlayerTraversal2D))]
    public sealed class AuthoredLadderRoute : MonoBehaviour
    {
        [SerializeField] private LadderVolume ladder;
        [SerializeField] private float targetHeight = 6.6f;
        [SerializeField, Min(.1f)] private float approachTolerance = .4f;
        [SerializeField, Min(1)] private float timeoutSeconds = 20;
        private LarperBrain brain;
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private float began;
        private bool platformExit,platformAttached;private float exitDirection;
        public LadderRouteState State { get; private set; }
        public bool IsRunning => State == LadderRouteState.Approaching || State == LadderRouteState.Climbing;
        private void Awake() { brain = GetComponent<LarperBrain>(); motor = GetComponent<PlayerMotor2D>(); traversal = GetComponent<PlayerTraversal2D>(); }
        public void Configure(LadderVolume value, float height) { platformExit=false;ladder = value; targetHeight = height; began = Time.time; State = LadderRouteState.Approaching; }
        public void ConfigurePlatform(LadderVolume value,float height,float direction){Configure(value,height);platformExit=true;platformAttached=false;exitDirection=direction;}
        private void Start() { began = Time.time; if (ladder != null) State = LadderRouteState.Approaching; }
        private void FixedUpdate()
        {
            if (State == LadderRouteState.Complete || State == LadderRouteState.Unavailable) return;
            if (ladder == null)
            {
                if (IsRunning) { traversal.ResetTraversal(); Finish(LadderRouteState.Unavailable); }
                else State = LadderRouteState.Unconfigured;
                return;
            }
            if (!ladder.isActiveAndEnabled || brain.Archetype == null || !brain.Archetype.Allows(MovementCapabilities.Climb) || Time.time - began > timeoutSeconds)
            { Finish(LadderRouteState.Unavailable); return; }
            brain.DecisionsSuspended = true;
            if(platformExit&&State==LadderRouteState.Climbing&&(traversal.IsClimbing||platformAttached))
            {
                platformAttached|=traversal.IsClimbing;
                motor.SetMoveInput(0);
                if(!traversal.IsClimbing){if(motor.IsGrounded)Finish(LadderRouteState.Complete);return;}
                traversal.SetInput(Mathf.Abs(transform.position.y-targetHeight)<.16f?Vector2.right*exitDirection:Vector2.up*Mathf.Sign(targetHeight-transform.position.y),false);
                return;
            }
            if (State == LadderRouteState.Climbing && !traversal.IsClimbing && transform.position.y >= targetHeight - .5f)
            {
                // Let the assisted exit settle on its landing before patrol can steer it away.
                motor.SetMoveInput(0); traversal.SetInput(Vector2.zero, false);
                if (motor.IsGrounded) Finish(LadderRouteState.Complete);
                return;
            }
            float dx = ladder.CenterX - transform.position.x;
            if (!traversal.IsClimbing && Mathf.Abs(dx) > approachTolerance)
            {
                State = LadderRouteState.Approaching; motor.SetMoveInput(Mathf.Sign(dx) * .5f); traversal.SetInput(Vector2.zero, false);
            }
            else
            {
                State = LadderRouteState.Climbing; motor.SetMoveInput(0); traversal.SetInput(Vector2.up, false);
            }
        }
        private void Finish(LadderRouteState state)
        {
            if (state == LadderRouteState.Unavailable) traversal.ResetTraversal();
            if (state == LadderRouteState.Complete) brain.AnchorPatrolHere();
            State = state; traversal.SetInput(Vector2.zero, false); motor.SetMoveInput(0); brain.DecisionsSuspended = false;
        }
        private void OnDisable() { if (brain != null) { brain.DecisionsSuspended = false; traversal.ResetTraversal(); } }
    }
}
