using InsaneDreams.Input;
using UnityEngine;
namespace InsaneDreams.Combat
{
    [DefaultExecutionOrder(60)]
    [RequireComponent(typeof(Health),typeof(PlayerActions))]
    public sealed class PlayerHealing : MonoBehaviour
    {
        [SerializeField, Min(1)] private float healingPerSecond=25;
        [SerializeField, Min(0)] private float initialDuctTape=200;
        [SerializeField, Min(1)] private float maximumDuctTape=200;
        [SerializeField, Min(0)] private float damageInterruptionSeconds=.8f;
        [SerializeField] private bool requireReleaseAfterDamage=true;
        private Health health;private PlayerActions actions;
        private InsaneDreams.Traversal.PlayerTraversal2D traversal;
        private InsaneDreams.Prototype.PlayerMotor2D motor;
        private InsaneDreams.Inventory.PlayerLoadout loadout;
        private float blockedUntil;private bool waitingForRelease;
        public float DuctTape{get;private set;}
        public float Capacity=>maximumDuctTape;
        public bool IsHealing{get;private set;}
        public bool WaitingForRelease=>waitingForRelease;
        public float LastRestored{get;private set;}
        public float FeedbackUntil{get;private set;}
        public float CooldownRemaining=>Mathf.Max(0,blockedUntil-Time.time);
        public event System.Action<float> Healed;
        private void Awake()
        {
            health=GetComponent<Health>();actions=GetComponent<PlayerActions>();motor=GetComponent<InsaneDreams.Prototype.PlayerMotor2D>();
            traversal=GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>();loadout=GetComponent<InsaneDreams.Inventory.PlayerLoadout>();ResetSupply();
        }
        private void OnEnable()=>health.Damaged+=Damaged;
        private void OnDisable(){health.Damaged-=Damaged;IsHealing=false;}
        private void Damaged(DamageInfo _){blockedUntil=Time.time+damageInterruptionSeconds;waitingForRelease=requireReleaseAfterDamage&&actions.HealHeld;IsHealing=false;}
        public void ResetSupply(){DuctTape=Mathf.Clamp(initialDuctTape,0,maximumDuctTape);blockedUntil=0;waitingForRelease=IsHealing=false;LastRestored=FeedbackUntil=0;}
        public void RestoreSupply(float amount,float capacity)
        {maximumDuctTape=Mathf.Max(1,capacity);DuctTape=float.IsNaN(amount)||float.IsInfinity(amount)?0:Mathf.Clamp(amount,0,maximumDuctTape);IsHealing=false;}
        public float AddDuctTape(float amount)
        {
            if(float.IsNaN(amount)||float.IsInfinity(amount)||amount<=0)return 0;
            float added=Mathf.Min(amount,Mathf.Max(0,maximumDuctTape-DuctTape));DuctTape+=added;return added;
        }
        private bool CanHeal=>isActiveAndEnabled&&actions.isActiveAndEnabled&&!health.IsDefeated&&health.Current<health.Maximum&&DuctTape>0&&
            !waitingForRelease&&Time.time>=blockedUntil&&!actions.FireHeld&&
            (motor==null||motor.enabled&&motor.IsGrounded)&&(traversal==null||!traversal.IsClimbing&&!traversal.ParachuteFlightActive)&&
            (loadout==null||loadout.SelectedWeapon==null||!loadout.SelectedWeapon.Cycle.IsReloading);
        public bool TryHeal(){if(!CanHeal)return false;actions.SetHealIntent(true);return true;}
        private void Update()
        {
            IsHealing=false;
            if(!actions.HealHeld){waitingForRelease=false;return;}
            if(!CanHeal)return;
            float restored=health.Heal(Mathf.Min(DuctTape,healingPerSecond*Time.deltaTime));
            if(restored<=0)return;
            DuctTape=Mathf.Max(0,DuctTape-restored);LastRestored=restored;IsHealing=true;FeedbackUntil=Time.time+.25f;Healed?.Invoke(restored);
        }
    }
}
