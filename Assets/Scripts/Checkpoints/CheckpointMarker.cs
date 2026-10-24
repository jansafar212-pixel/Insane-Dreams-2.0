using UnityEngine;

namespace InsaneDreams.Checkpoints
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class CheckpointMarker : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private SpriteRenderer markerVisual;
        [SerializeField] private Color inactiveColor = new Color(0.3f, 0.65f, 0.85f);
        [SerializeField] private Color activeColor = new Color(0.3f, 1f, 0.5f);
        private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;
        public bool IsLit{get;private set;}
        public void SetLit(bool lit) { IsLit=lit;if (markerVisual != null) markerVisual.color = lit ? activeColor : inactiveColor; }
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Physics2D also delivers triggers to disabled MonoBehaviours. Mission
            // markers are disabled because objective-gated commits own activation.
            if(!isActiveAndEnabled)return;
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out PlayerRecovery player))
                player.Activate(this, spawnPoint != null ? (Vector2)spawnPoint.position : (Vector2)transform.position);
        }
    }
}
