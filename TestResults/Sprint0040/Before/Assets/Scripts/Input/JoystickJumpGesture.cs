using System;
using UnityEngine;

namespace InsaneDreams.Input
{
    [Serializable]
    public sealed class JoystickJumpGesture
    {
        [SerializeField, Range(0.4f, 1)] private float upwardThreshold = 0.65f;
        [SerializeField, Range(0, 0.35f)] private float rearmThreshold = 0.25f;
        [SerializeField, Range(0.2f, 0.8f)] private float diagonalThreshold = 0.45f;
        [SerializeField, Min(1)] private float climbDominance = 1.15f;
        private bool armed = true;
        public bool ClimbSuppressed { get; private set; }
        public bool NeedsDecision(Vector2 axis) => armed && axis.y >= upwardThreshold;

        public bool Evaluate(Vector2 axis, bool usableLadder, bool climbing, bool grounded)
        {
            if (axis.y <= rearmThreshold)
            {
                armed = true;
                ClimbSuppressed = false;
            }
            if (!armed || axis.y < upwardThreshold) return false;
            armed = false; // Consume even a rejected airborne or ladder gesture.
            if (climbing) return false;
            bool diagonal = Mathf.Abs(axis.x) >= diagonalThreshold && axis.y < Mathf.Abs(axis.x) * climbDominance;
            if (usableLadder && (!diagonal || !grounded)) return false;
            ClimbSuppressed = true; // Do not attach later from this already-held gesture.
            return true;
        }

        public void Reset() { armed = false; ClimbSuppressed = false; }
    }
}
