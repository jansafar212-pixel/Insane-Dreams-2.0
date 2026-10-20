using System.Collections.Generic;
using UnityEngine;
using InsaneDreams.Combat;

namespace InsaneDreams.Visuals
{
    public sealed class CartoonRagdoll : MonoBehaviour, IPhysicalImpactReceiver
    {
        [SerializeField, Min(1)] private int maximumActive = 4;
        [SerializeField, Min(.1f)] private float lifetime = 45;
        [SerializeField, Min(.1f)] private float restSeconds = 1.5f;
        [SerializeField, Min(0)] private float initialDefeatMultiplier = 2.99f;
        [SerializeField, Min(0)] private float postDefeatMultiplier = 1.794f;
        [SerializeField, Min(0)] private float maximumImpulse = 2.875f;
        [SerializeField, Min(0)] private float maximumVelocity = 12;
        [SerializeField, Min(0)] private float maximumAngularVelocity = 720;
        [SerializeField, Min(0)] private float linearDamping = .5f, angularDamping = 1.2f;
        [SerializeField, Range(5, 120)] private float limbAngularLimit = 80;
        [SerializeField] private AudioClip collisionSound;
        [SerializeField, Min(.1f)] private float collisionSoundCooldown = .3f;
        private AudioSource audioSource;
        private float nextCollisionSound;
        public int ImpactCount { get; private set; }
        public Vector2 LastImpactPoint { get; private set; }
        public Rigidbody2D[] parts;
        public SpriteRenderer face;
        private static readonly List<CartoonRagdoll> active = new List<CartoonRagdoll>();
        private float age, resting;
        public static int ActiveCount => active.Count;
        public bool IsResting { get; private set; }
        public float Lifetime { get => lifetime; set => lifetime = Mathf.Max(.1f, value); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetRegistry() => active.Clear();
        public void Activate(CutoutRig source, Vector2 momentum)
        {
            if (active.Contains(this)) return;
            while (active.Count >= maximumActive) active[0].Retire();
            active.Add(this);
            var originals = source.BodyParts;
            // Place every body before connecting and enabling simulation: no joint spawn explosion.
            for (int i = 0; i < parts.Length; i++)
            {
                var from = originals[i]; var to = parts[i];
                to.simulated = false;
                Vector3 down = from.TransformVector(Vector3.down);
                float angle = Mathf.Atan2(down.y, down.x) * Mathf.Rad2Deg + 90;
                to.transform.SetPositionAndRotation(from.position, Quaternion.Euler(0, 0, angle));
                Vector3 scale = from.lossyScale;
                to.transform.localScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                var sprite = to.GetComponent<SpriteRenderer>(); sprite.sprite = from.GetComponent<SpriteRenderer>().sprite;
                sprite.flipX = source.transform.lossyScale.x < 0;
                sprite.color = i == 0 ? source.skin.clothing : Color.white;
            }
            face.sprite = source.skin.Face(FaceExpression.Stunned); face.flipX = source.transform.lossyScale.x < 0;
            for (int i = 1; i < parts.Length; i++)
            {
                var joint = parts[i].GetComponent<HingeJoint2D>();
                joint.anchor = i == 1 ? new Vector2(0, source.skin.head.bounds.min.y) : Vector2.zero;
                joint.connectedBody = parts[0]; joint.autoConfigureConnectedAnchor = true;
                float limit = i == 1 ? 35 : limbAngularLimit;
                joint.limits = new JointAngleLimits2D { min = -limit, max = limit };
            }
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].simulated = true;
                parts[i].centerOfMass = parts[i].GetComponent<Collider2D>().offset;
                parts[i].linearDamping = linearDamping; parts[i].angularDamping = angularDamping;
                parts[i].linearVelocity = Vector2.ClampMagnitude(momentum, 8) + Vector2.up * .6f;
                parts[i].angularVelocity = i % 2 == 0 ? 35 : -35;
            }
        }
        public void ReceiveImpact(PhysicalImpact impact) => ApplyImpact(impact, postDefeatMultiplier);
        public void ReceiveDefeatImpact(PhysicalImpact impact) => ApplyImpact(impact, initialDefeatMultiplier);
        private void ApplyImpact(PhysicalImpact impact, float multiplier)
        {
            if (!impact.IsValid || !isActiveAndEnabled || parts == null || parts.Length == 0) return;
            Rigidbody2D target = null; float nearest = float.PositiveInfinity;
            foreach (var part in parts)
            {
                if (part == impact.Body) { target = part; break; }
                float distance = (part.GetComponent<Collider2D>().ClosestPoint(impact.Point) - impact.Point).sqrMagnitude;
                if (distance < nearest) { nearest = distance; target = part; }
            }
            if (target == null) return;
            foreach (var part in parts) part.WakeUp();
            IsResting = false; resting = 0;
            Vector2 point = target.GetComponent<Collider2D>().ClosestPoint(impact.Point);
            target.AddForceAtPosition(impact.Direction * Mathf.Min(maximumImpulse, impact.Impulse * multiplier), point, ForceMode2D.Impulse);
            ClampVelocities(); ImpactCount++; LastImpactPoint = point;
        }
        private void ClampVelocities()
        {
            foreach (var part in parts)
            {
                if (maximumVelocity > 0) part.linearVelocity = Vector2.ClampMagnitude(part.linearVelocity, maximumVelocity);
                if (maximumAngularVelocity > 0) part.angularVelocity = Mathf.Clamp(part.angularVelocity, -maximumAngularVelocity, maximumAngularVelocity);
            }
        }
        private void FixedUpdate() { if (!IsResting && parts != null) ClampVelocities(); }
        public void CollisionCue(float speed)
        {
            if (collisionSound == null || speed < 1 || Time.time < nextCollisionSound) return;
            nextCollisionSound = Time.time + collisionSoundCooldown;
            if (audioSource == null) { audioSource = gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake = false; }
            audioSource.PlayOneShot(collisionSound, Mathf.Clamp01(speed / 12));
        }
        private void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime) { Retire(); return; }
            if (parts == null) return;
            if (IsResting)
            {
                bool awake = false; foreach (var part in parts) if (part.IsAwake()) { awake = true; break; }
                if (!awake) return;
                IsResting = false; resting = 0;
            }
            bool still = true;
            foreach (var part in parts) if (part != null && (part.linearVelocity.sqrMagnitude > .025f || Mathf.Abs(part.angularVelocity) > 8)) { still = false; break; }
            resting = still ? resting + Time.deltaTime : 0;
            if (resting >= restSeconds)
            {
                IsResting = true;
                foreach (var part in parts) if (part != null) { part.linearVelocity = Vector2.zero; part.angularVelocity = 0; part.Sleep(); }
            }
        }
        public void Retire() { active.Remove(this); gameObject.SetActive(false); Destroy(gameObject); }
        private void OnDisable() => active.Remove(this);
        private void OnDestroy() => active.Remove(this);
    }
}
