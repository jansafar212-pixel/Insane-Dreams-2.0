using UnityEngine;

namespace InsaneDreams.Combat
{
    // A physical outcome, independent of health, factions, ammunition and defeat events.
    public readonly struct PhysicalImpact
    {
        public readonly Vector2 Point, Direction;
        public readonly float Impulse;
        public readonly Rigidbody2D Body;
        public readonly float ReactionStrength;
        public PhysicalImpact(Vector2 point, Vector2 direction, float impulse, Rigidbody2D body = null,float reactionStrength=1)
        { Point = point; Direction = direction.normalized; Impulse = impulse; Body = body;ReactionStrength=reactionStrength; }
        public bool IsValid => !float.IsNaN(Point.x) && !float.IsNaN(Point.y) && !float.IsInfinity(Point.x) && !float.IsInfinity(Point.y) &&
            !float.IsNaN(Direction.x) && !float.IsNaN(Direction.y) && !float.IsInfinity(Direction.x) && !float.IsInfinity(Direction.y) &&
            !float.IsNaN(Impulse) && !float.IsInfinity(Impulse) && Impulse > 0 && Direction.sqrMagnitude > .5f;
    }
    public interface IPhysicalImpactReceiver { void ReceiveImpact(PhysicalImpact impact); }
    public enum ShotSurface { None, Geometry, Character, Ragdoll }
}
