using InsaneDreams.Checkpoints;
using UnityEngine;

namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(Health), typeof(PlayerRecovery))]
    public sealed class PlayerHealthRecovery : MonoBehaviour
    {
        [SerializeField, Min(0)] private float recoveryProtection = 2;
        private Health health;
        private PlayerRecovery recovery;
        private PlayerRevive revive;
        private void Awake() { health = GetComponent<Health>(); recovery = GetComponent<PlayerRecovery>();revive=GetComponent<PlayerRevive>(); }
        private void OnEnable() { health.Defeated += Defeated; recovery.Recovered += Recovered; }
        private void OnDisable() { health.Defeated -= Defeated; recovery.Recovered -= Recovered; }
        private void Defeated(DamageInfo damage) {if(revive==null||!revive.CanOfferRevive){var death=GetComponent<Visuals.PlayerDeathRagdoll>();if(death==null||!death.BeginAutomaticRecovery())recovery.Respawn();}}
        private void Recovered() { health.Restore(); health.ProtectFor(recoveryProtection); }
    }
}
