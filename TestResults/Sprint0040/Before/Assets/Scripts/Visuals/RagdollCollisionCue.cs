using UnityEngine;
namespace InsaneDreams.Visuals
{
    public sealed class RagdollCollisionCue : MonoBehaviour
    {
        private CartoonRagdoll owner;
        private void Awake() => owner = GetComponentInParent<CartoonRagdoll>();
        private void OnCollisionEnter2D(Collision2D hit) { if (owner != null) owner.CollisionCue(hit.relativeVelocity.magnitude); }
    }
}
