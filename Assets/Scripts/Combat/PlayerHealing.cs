using InsaneDreams.Input;
using UnityEngine;
namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(Health),typeof(PlayerActions))]
    public sealed class PlayerHealing : MonoBehaviour
    {
        [SerializeField, Min(1)] private float amount=35;
        [SerializeField, Min(0)] private float cooldownSeconds=3;
        [SerializeField] private bool unlimitedUses=true;
        [SerializeField, Min(0)] private int remainingUses=3;
        private Health health;private PlayerActions actions;private float nextUse;
        public float LastRestored{get;private set;}
        public float FeedbackUntil{get;private set;}
        public float CooldownRemaining=>Mathf.Max(0,nextUse-Time.time);
        public event System.Action<float> Healed;
        private void Awake(){health=GetComponent<Health>();actions=GetComponent<PlayerActions>();}
        private void OnEnable()=>actions.HealRequested+=Requested;
        private void OnDisable()=>actions.HealRequested-=Requested;
        private void Requested()=>TryHeal();
        public bool TryHeal()
        {
            if(!isActiveAndEnabled||!actions.isActiveAndEnabled||Time.time<nextUse||!unlimitedUses&&remainingUses<=0)return false;
            float restored=health.Heal(amount);if(restored<=0)return false;
            if(!unlimitedUses)remainingUses--;nextUse=Time.time+cooldownSeconds;
            LastRestored=restored;FeedbackUntil=Time.time+1.2f;Healed?.Invoke(restored);return true;
        }
    }
}
