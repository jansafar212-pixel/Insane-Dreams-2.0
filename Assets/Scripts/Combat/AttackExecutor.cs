using InsaneDreams.Inventory;
using UnityEngine;

namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(Health))]
    public sealed class AttackExecutor : MonoBehaviour
    {
        [SerializeField] private LayerMask hitLayers = 5;
        [SerializeField] private LayerMask physicalImpactLayers = 2;
        [SerializeField] private LineRenderer tracer;
        [SerializeField, Range(.02f, 1)] private float tracerSeconds = .35f;
        [SerializeField, Min(.001f)] private float tracerWidth = .035f;
        [SerializeField, Range(1, 16)] private int maximumTracers = 6;
        [SerializeField] private Color friendlyTracer = new Color(1, .9f, .45f, .85f);
        [SerializeField] private Color enemyTracer = new Color(1, .35f, .25f, .85f);
        [SerializeField] private LayerMask muzzleBlockingLayers = 1;
        private readonly System.Collections.Generic.List<RaycastHit2D> hits = new System.Collections.Generic.List<RaycastHit2D>(64);
        private Health owner;
        private InsaneDreams.Traversal.PlayerPosture2D posture;
        private InsaneDreams.Visuals.CharacterVisualDriver visual;
        private InsaneDreams.Visuals.CutoutRig rig;
        private Vector2 BodyOrigin => posture != null ? posture.WorldCenter : (Vector2)transform.position;
        public Vector2 Origin => rig != null && rig.muzzle != null ? rig.MuzzlePosition : BodyOrigin;
        public Vector2 LastShotOrigin { get; private set; }
        public Vector2 LastShotDirection { get; private set; }
        public Vector2 LastShotEnd { get; private set; }
        private LineRenderer[] trails;
        private float[] expires;
        private int nextTrail;
        private Material ownedMaterial;
        public int ShotsFired { get; private set; }
        public event System.Action<Vector2, Vector2, bool> Fired;
        public event System.Action<Vector2, ShotSurface> SurfaceHit;
        public ShotSurface LastSurface { get; private set; }
        public event System.Action EmptyAttempt;
        private float nextEmptyCue;
        private void Awake()
        {
            owner = GetComponent<Health>(); posture = GetComponent<InsaneDreams.Traversal.PlayerPosture2D>();
            visual = GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>(); rig = GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true);
            rig?.EnsureMuzzle();
            trails = new LineRenderer[Mathf.Clamp(maximumTracers, 1, 16)]; expires = new float[trails.Length];
            Material material = tracer != null ? tracer.sharedMaterial : null;
            if (material == null) material = ownedMaterial = new Material(Shader.Find("Sprites/Default"));
            for (int i = 0; i < trails.Length; i++)
            {
                var line = i == 0 && tracer != null ? tracer : new GameObject("Tracer_" + i).AddComponent<LineRenderer>();
                if (line.transform.parent == null) line.transform.SetParent(transform, false);
                line.sharedMaterial = material; line.useWorldSpace = true; line.positionCount = 2;
                line.startWidth = line.endWidth = tracerWidth; line.sortingOrder = 29; line.enabled = false;
                trails[i] = line;
            }
        }
        public void ConstrainMuzzle()
        {
            if (rig == null || rig.muzzle == null) return;
            // Check the held path through the shoulder. A torso-to-tip diagonal
            // incorrectly blocks a standing gun above otherwise valid low cover.
            Vector2 shoulder = rig.rightArm.position;
            if (ClipHeldSegment(BodyOrigin, shoulder, out var safe) || ClipHeldSegment(shoulder, rig.MuzzlePosition, out safe)) rig.RetractMuzzle(safe);
        }
        private bool ClipHeldSegment(Vector2 start, Vector2 end, out Vector2 safePosition)
        {
            Vector2 segment = end-start; safePosition=end;
            if (segment.sqrMagnitude < .000001f) return false;
            var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(muzzleBlockingLayers);
            int count = Physics2D.Raycast(start, segment.normalized, filter, hits, segment.magnitude);
            float safe = segment.magnitude;
            for (int i = 0; i < count; i++)
                if (!hits[i].collider.transform.IsChildOf(transform)) safe = Mathf.Min(safe, Mathf.Max(0, hits[i].distance - .015f));
            safePosition=start+segment.normalized*safe; return safe<segment.magnitude;
        }
        public bool Attack(RuntimeWeapon weapon, Vector2 aim, float now)
        {
            if (weapon == null || owner.IsDefeated || float.IsNaN(now) || float.IsInfinity(now) || float.IsNaN(aim.x) || float.IsNaN(aim.y) ||
                float.IsInfinity(aim.x) || float.IsInfinity(aim.y) || aim.sqrMagnitude < .001f) return false;
            if (!weapon.Cycle.TryAttack(now))
            {
                if (!weapon.Cycle.IsReloading && weapon.Definition.WeaponCategory == WeaponCategory.Firearm && weapon.MagazineAmmo == 0 && Time.time >= nextEmptyCue)
                { nextEmptyCue = Time.time + .3f; EmptyAttempt?.Invoke(); }
                return false;
            }
            Vector2 direction = aim.normalized;
            // Enemy spread and current input are posed synchronously, before reading the
            // anchor. Update/LateUpdate ordering cannot leave a previous-frame muzzle.
            if (weapon.UsesAmmunition) visual?.PrepareShot(direction);
            ConstrainMuzzle();
            Vector2 origin = weapon.UsesAmmunition ? Origin : BodyOrigin;
            var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(hitLayers.value | physicalImpactLayers.value);
            int count = Physics2D.Raycast(origin, direction, filter, hits, weapon.Definition.AttackRange);
            RaycastHit2D closest = default; float distance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider.transform.IsChildOf(transform)) continue;
                if (hits[i].distance < distance) { closest = hits[i]; distance = hits[i].distance; }
            }
            Vector2 end = origin + direction * weapon.Definition.AttackRange;
            bool damaged = false;
            LastSurface = ShotSurface.None;
            if (closest.collider != null)
            {
                end = closest.point;
                var impact = new PhysicalImpact(end, direction, weapon.Definition.ImpactImpulse, closest.rigidbody);
                var health = closest.collider.GetComponentInParent<Health>();
                if (health != null)
                {
                    LastSurface = ShotSurface.Character;
                    damaged = health.ApplyDamage(new DamageInfo(weapon.Definition.Damage, owner.Team, gameObject, impact));
                }
                else
                {
                    var receiver = closest.collider.GetComponentInParent<IPhysicalImpactReceiver>();
                    LastSurface = receiver != null ? ShotSurface.Ragdoll : ShotSurface.Geometry;
                    receiver?.ReceiveImpact(impact);
                }
                SurfaceHit?.Invoke(end, LastSurface);
            }
            ShotsFired++;
            LastShotOrigin = origin; LastShotDirection = direction; LastShotEnd = end;
            Fired?.Invoke(direction, end, damaged);
            if (weapon.UsesAmmunition && trails != null)
            {
                var line = trails[nextTrail]; line.enabled = true;
                line.SetPosition(0, new Vector3(origin.x, origin.y, -.1f)); line.SetPosition(1, new Vector3(end.x, end.y, -.1f));
                expires[nextTrail] = Time.time + tracerSeconds;
                nextTrail = (nextTrail + 1) % trails.Length;
                FadeTrails();
            }
            return true;
        }
        private void Update() => FadeTrails();
        private void FadeTrails()
        {
            if (trails == null) return;
            Color color = owner.Team == CombatTeam.Enemy ? enemyTracer : friendlyTracer;
            for (int i = 0; i < trails.Length; i++)
            {
                if (!trails[i].enabled) continue;
                float remaining = expires[i] - Time.time;
                if (remaining <= 0) { trails[i].enabled = false; continue; }
                Color faded = color; faded.a *= Mathf.Clamp01(remaining / Mathf.Max(.02f, tracerSeconds));
                trails[i].startColor = trails[i].endColor = faded;
            }
        }
        private void OnDisable() { if (trails != null) foreach (var line in trails) if (line != null) line.enabled = false; }
        private void OnDestroy() { if (ownedMaterial != null) Destroy(ownedMaterial); }
    }
}
