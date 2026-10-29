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
        [SerializeField] private Material tracerMaterial;
        [SerializeField, Range(.02f, 1)] private float tracerSeconds = .17f;
        [SerializeField, Min(.001f)] private float tracerWidth = .035f;
        [SerializeField, Range(1, 16)] private int maximumTracers = 6;
        [SerializeField] private Color friendlyTracer = new Color(1, .9f, .45f, .85f);
        [SerializeField] private Color enemyTracer = new Color(1, .35f, .25f, .85f);
        [SerializeField] private LayerMask muzzleBlockingLayers = 1;
        private readonly System.Collections.Generic.List<RaycastHit2D> hits = new System.Collections.Generic.List<RaycastHit2D>(64);
        private Health owner;
        private PlayerHealing healing;
        private InsaneDreams.Traversal.PlayerPosture2D posture;
        private InsaneDreams.Visuals.CharacterVisualDriver visual;
        private InsaneDreams.Visuals.CutoutRig rig;
        private PlayerCover2D playerCover;
        private Vector2 BodyOrigin => posture != null ? posture.WorldCenter : (Vector2)transform.position;
        public Vector2 Origin => rig != null && rig.muzzle != null ? rig.MuzzlePosition : BodyOrigin;
        public Vector2 LastShotOrigin { get; private set; }
        public Vector2 LastShotDirection { get; private set; }
        public Vector2 LastShotEnd { get; private set; }
        public Vector2 LastDamagePoint { get; private set; }
        public Vector2 LastHitNormal { get; private set; }
        public ImpactMaterial LastImpactMaterial { get; private set; }
        private LineRenderer[] trails;
        private float[] expires;
        private Color[] trailColors;
        private int nextTrail;
        private Material ownedMaterial;
        private Material aimMaterial;
        public int ShotsFired { get; private set; }
        public bool LastAttackUsesAmmunition { get; private set; }
        public event System.Action<Vector2, Vector2, bool> Fired;
        public event System.Action<Vector2, ShotSurface> SurfaceHit;
        public ShotSurface LastSurface { get; private set; }
        public event System.Action EmptyAttempt;
        private float nextEmptyCue;
        private RuntimeWeapon bloomWeapon;
        private float bloom,lastSpreadShot;
        public float SustainedSpread=>bloom;
        public Material TracerMaterial=>trails!=null&&trails.Length>0?trails[0].sharedMaterial:null;
        public Material AimMaterial=>aimMaterial;
        public Vector2 PreviewEnd(Vector2 direction,float range)
        {
            Vector2 start=Origin;var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(hitLayers.value|physicalImpactLayers.value);
            int count=Physics2D.Raycast(start,direction,filter,hits,range);float nearest=range;
            for(int i=0;i<count;i++)if(!hits[i].collider.transform.IsChildOf(transform)&&!InsaneDreams.Enemies.SamuraiBodyHitbox.IsMovementShape(hits[i].collider))nearest=Mathf.Min(nearest,hits[i].distance);
            return start+direction*nearest;
        }
        private void Awake()
        {
            owner = GetComponent<Health>(); posture = GetComponent<InsaneDreams.Traversal.PlayerPosture2D>();
            healing=GetComponent<PlayerHealing>();
            visual = GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>(); rig = GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true);
            rig?.EnsureMuzzle();
            playerCover = GetComponent<PlayerCover2D>();
            trails = new LineRenderer[Mathf.Clamp(maximumTracers, 1, 16)]; expires = new float[trails.Length];
            trailColors=new Color[trails.Length];
            Material material = tracer != null ? tracer.sharedMaterial : null;
            if (material == null) material = ownedMaterial = new Material(Shader.Find("Sprites/Default"));
            aimMaterial=material;
            if(tracerMaterial!=null)material=tracerMaterial;
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
            bool raised = playerCover != null && playerCover.RaiseWeapon;
            if (ClipHeldSegment(BodyOrigin, shoulder, out var safe) ||
                (raised ? ClipHeldSegment(shoulder, rig.weaponMount.position, out safe) || ClipHeldSegment(rig.weaponMount.position, rig.MuzzlePosition, out safe) :
                ClipHeldSegment(shoulder, rig.MuzzlePosition, out safe))) rig.RetractMuzzle(safe);
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
            if(healing!=null&&healing.IsHealing)return false;
            if (weapon == null || owner.IsDefeated || float.IsNaN(now) || float.IsInfinity(now) || float.IsNaN(aim.x) || float.IsNaN(aim.y) ||
                float.IsInfinity(aim.x) || float.IsInfinity(aim.y) || aim.sqrMagnitude < .001f) return false;
            if (!weapon.Cycle.TryAttack(now))
            {
                if (!weapon.Cycle.IsReloading && weapon.Definition.WeaponCategory == WeaponCategory.Firearm && weapon.MagazineAmmo == 0 && Time.time >= nextEmptyCue)
                { nextEmptyCue = Time.time + .3f; EmptyAttempt?.Invoke(); }
                return false;
            }
            Vector2 direction = aim.normalized;
            LastAttackUsesAmmunition=weapon.UsesAmmunition;
            // Enemy spread and current input are posed synchronously, before reading the
            // anchor. Update/LateUpdate ordering cannot leave a previous-frame muzzle.
            if (weapon.UsesAmmunition) visual?.PrepareShot(direction);
            ConstrainMuzzle();
            Vector2 origin = weapon.UsesAmmunition ? Origin : BodyOrigin;
            if(bloomWeapon!=weapon){bloomWeapon=weapon;bloom=0;}
            bloom=Mathf.MoveTowards(bloom,0,Mathf.Max(0,now-lastSpreadShot)*weapon.Definition.MaximumBloom*.35f);
            float spread=weapon.Definition.SpreadDegrees+(weapon.Definition.Pellets==1?bloom:0);
            if(weapon.Definition.Pellets==1&&spread>0)direction=Quaternion.Euler(0,0,Random.Range(-spread,spread))*direction;
            bool damaged=TracePellet(weapon,origin,direction);
            var centerEnd=LastShotEnd;var centerNormal=LastHitNormal;var centerMaterial=LastImpactMaterial;var centerSurface=LastSurface;
            for(int i=1;i<weapon.Definition.Pellets;i++)
            {
                int slot=(i+1)/2*(i%2==1?1:-1);
                float angle=slot*spread/Mathf.Max(1,weapon.Definition.Pellets-1);
                damaged|=TracePellet(weapon,origin,Quaternion.Euler(0,0,angle)*direction);
            }
            bloom=Mathf.Min(weapon.Definition.MaximumBloom,bloom+weapon.Definition.BloomPerShot);lastSpreadShot=now;
            LastShotOrigin=origin;LastShotDirection=direction;LastShotEnd=centerEnd;LastHitNormal=centerNormal;LastImpactMaterial=centerMaterial;LastSurface=centerSurface;
            ShotsFired++;Fired?.Invoke(direction,centerEnd,damaged);
            return true;
        }
        private bool TracePellet(RuntimeWeapon weapon,Vector2 origin,Vector2 direction)
        {
            var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(hitLayers.value | physicalImpactLayers.value);
            // A short player contact sweep is forgiving across the visible wrench
            // swing, while its farthest extent remains the configured melee range.
            float contactRadius=Mathf.Min(.16f,weapon.Definition.AttackRange*.25f);
            int count = !weapon.UsesAmmunition&&playerCover!=null?
                Physics2D.CircleCast(origin,contactRadius,direction,filter,hits,Mathf.Max(0,weapon.Definition.AttackRange-contactRadius)):
                Physics2D.Raycast(origin, direction, filter, hits, weapon.Definition.AttackRange);
            RaycastHit2D closest = default; float distance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider.transform.IsChildOf(transform)) continue;
                if(!weapon.UsesAmmunition&&Vector2.Dot(hits[i].point-origin,direction)<-.01f)continue;
                if(InsaneDreams.Enemies.SamuraiBodyHitbox.IsMovementShape(hits[i].collider))continue;
                if (hits[i].distance < distance) { closest = hits[i]; distance = hits[i].distance; }
            }
            Vector2 end = origin + direction * weapon.Definition.AttackRange;
            bool damaged = false;
            Health hitTarget = null;
            LastSurface = ShotSurface.None;
            LastImpactMaterial=ImpactMaterial.Generic;LastHitNormal=-direction;
            if (closest.collider != null)
            {
                end = closest.point;
                LastHitNormal=closest.normal;LastImpactMaterial=ImpactSurface2D.Classify(closest.collider);
                float reaction=weapon.Definition.HitReactionStrength*weapon.Definition.DamageAtDistance(distance)/Mathf.Max(.01f,weapon.Definition.Damage);
                var impact = new PhysicalImpact(end, direction, weapon.Definition.ImpactImpulse, closest.rigidbody,reaction);
                var health = closest.collider.GetComponentInParent<Health>();
                hitTarget = health;
                if (health != null)
                {
                    LastSurface = ShotSurface.Character;
                    damaged = health.ApplyDamage(new DamageInfo(weapon.Definition.DamageAtDistance(distance), owner.Team, gameObject, impact,false,weapon.UsesAmmunition?DamageKind.Ballistic:DamageKind.Melee));
                    if(damaged)LastDamagePoint=end;
                }
                else
                {
                    var receiver = closest.collider.GetComponentInParent<IPhysicalImpactReceiver>();
                    LastSurface = receiver != null ? ShotSurface.Ragdoll : ShotSurface.Geometry;
                    receiver?.ReceiveImpact(impact);
                    if(receiver is Visuals.CartoonRagdoll corpse)corpse.ReceiveRegionDamage(weapon.Definition.DamageAtDistance(distance),impact);
                }
                SurfaceHit?.Invoke(end, LastSurface);
            }
            LastShotOrigin = origin; LastShotDirection = direction; LastShotEnd = end;
            if(weapon.UsesAmmunition)CombatNoise.Report(new CombatNoiseEvent(gameObject,owner.Team,CombatNoiseKind.Gunfire,origin,end,1,hitTarget));
            if (weapon.UsesAmmunition && trails != null)
            {
                var line = trails[nextTrail]; line.enabled = true;
                Color color=weapon.Definition.CustomTracer?weapon.Definition.TracerColor:owner.Team==CombatTeam.Enemy?enemyTracer:friendlyTracer;
                trailColors[nextTrail]=Color.Lerp(color,Color.white,Random.Range(.05f,.25f));
                line.startWidth=tracerWidth*weapon.Definition.TracerScale*Random.Range(.85f,1.15f);line.endWidth=line.startWidth*.65f;
                line.SetPosition(0, new Vector3(origin.x, origin.y, -.1f)); line.SetPosition(1, new Vector3(end.x, end.y, -.1f));
                expires[nextTrail] = Time.time + tracerSeconds;
                nextTrail = (nextTrail + 1) % trails.Length;
                FadeTrails();
            }
            return damaged;
        }
        private void Update() => FadeTrails();
        private void FadeTrails()
        {
            if (trails == null) return;
            for (int i = 0; i < trails.Length; i++)
            {
                if (!trails[i].enabled) continue;
                float remaining = expires[i] - Time.time;
                if (remaining <= 0) { trails[i].enabled = false; continue; }
                Color faded = trailColors[i]; faded.a *= Mathf.Clamp01(remaining / Mathf.Max(.02f, tracerSeconds));
                trails[i].startColor=faded;faded.a*=.4f;trails[i].endColor=faded;
            }
        }
        private void OnDisable() { if (trails != null) foreach (var line in trails) if (line != null) line.enabled = false; }
        private void OnDestroy() { if (ownedMaterial != null) Destroy(ownedMaterial); }
    }
}
