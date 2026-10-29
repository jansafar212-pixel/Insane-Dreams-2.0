using InsaneDreams.Combat;
using UnityEngine;

namespace InsaneDreams.Visuals
{
    [RequireComponent(typeof(Health), typeof(CharacterVisualDriver))]
    public sealed class RagdollOnDefeat : MonoBehaviour
    {
        [SerializeField] private CartoonRagdoll prefab;
        private Health health;
        private CharacterVisualDriver visual;
        private Rigidbody2D body;
        private Vector2 momentum;
        private bool spawned;
        public int SpawnCount { get; private set; }
        public CartoonRagdoll LastRagdoll { get; private set; }
        private void Awake() { health = GetComponent<Health>(); visual = GetComponent<CharacterVisualDriver>(); body = GetComponent<Rigidbody2D>(); }
        private void OnEnable() { spawned = false; health.Damaged += CaptureMomentum; health.Defeated += Defeat; }
        private void OnDisable() { health.Damaged -= CaptureMomentum; health.Defeated -= Defeat; }
        private void CaptureMomentum(DamageInfo _) { if (body != null) momentum = body.linearVelocity; }
        private void Defeat(DamageInfo _)
        {
            // Health's captured invocation list still reaches this observer after root deactivation.
            // DeactivateOnDefeat remains the single immediate gameplay shutdown path.
            if (spawned || prefab == null || visual.Rig == null || !visual.Rig.IsBound) return;
            spawned = true; SpawnCount++;
            LastRagdoll = Instantiate(prefab, transform.position, Quaternion.identity);
            LastRagdoll.name = "Larper_CartoonRagdoll";
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(LastRagdoll.gameObject, gameObject.scene);
            LastRagdoll.Activate(visual.Rig, momentum);
            if (_.Impact.HasValue) LastRagdoll.ReceiveDefeatImpact(_.Impact.Value);
        }
    }
}
