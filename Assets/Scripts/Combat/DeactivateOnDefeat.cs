using UnityEngine;

namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(Health))]
    public sealed class DeactivateOnDefeat : MonoBehaviour
    {
        private Health health;
        private void Awake() => health = GetComponent<Health>();
        private void OnEnable() => health.Defeated += Defeated;
        private void OnDisable() => health.Defeated -= Defeated;
        private void Defeated(DamageInfo damage) => gameObject.SetActive(false);
    }
}
