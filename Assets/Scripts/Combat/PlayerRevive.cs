using InsaneDreams.Checkpoints;
using InsaneDreams.Input;
using UnityEngine;
namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(Health),typeof(PlayerRecovery),typeof(PlayerActions))]
    public sealed class PlayerRevive : MonoBehaviour
    {
        public const int Capacity=2;
        [SerializeField,Range(0,2)] private int initialInjections=2;
        private Health health;private PlayerActions actions;private PlayerCombat combat;
        private Rigidbody2D body;private PlayerRecovery recovery;
        private bool waiting,wasSimulated;private float protectionUntil;
        public int Injections{get;private set;}
        public bool CanOfferRevive=>isActiveAndEnabled&&health!=null&&health.IsDefeated&&Injections>0;
        public float ProtectionRemaining=>Mathf.Max(0,protectionUntil-Time.time);
        public event System.Action<int> InventoryChanged;
        private void Awake(){health=GetComponent<Health>();actions=GetComponent<PlayerActions>();combat=GetComponent<PlayerCombat>();body=GetComponent<Rigidbody2D>();recovery=GetComponent<PlayerRecovery>();Injections=Mathf.Clamp(initialInjections,0,Capacity);}
        private void OnEnable(){health.Defeated+=Defeated;recovery.Recovered+=CheckpointRecovered;}
        private void OnDisable(){health.Defeated-=Defeated;recovery.Recovered-=CheckpointRecovered;if(waiting){body.simulated=wasSimulated;waiting=false;}}
        public void ResetInventory()=>RestoreInventory(initialInjections);
        public void RestoreInventory(int count){Injections=Mathf.Clamp(count,0,Capacity);InventoryChanged?.Invoke(Injections);}
        public int AddInjections(int count){int old=Injections;RestoreInventory(Injections+Mathf.Max(0,Mathf.Min(Capacity,count)));return Injections-old;}
        private void Defeated(DamageInfo _)
        {
            if(!CanOfferRevive||waiting)return;
            GetComponent<HitReaction2D>()?.CancelReaction();
            actions.SuspendInput();actions.enabled=false;combat.Cancel();combat.enabled=false;
            GetComponent<Traversal.PlayerTraversal2D>()?.ResetTraversal();
            GetComponent<Prototype.PlayerMotor2D>()?.SetMoveInput(0);
            body.linearVelocity=Vector2.zero;wasSimulated=body.simulated;body.simulated=false;waiting=true;
        }
        private void ResumeControls()
        {if(waiting)body.simulated=wasSimulated;waiting=false;actions.ResetActions();actions.enabled=true;combat.enabled=true;}
        public bool TryRevive()
        {
            if(!waiting||!CanOfferRevive)return false;
            Injections--;health.Restore();health.ProtectFor(2);protectionUntil=Time.time+2;
            ResumeControls();InventoryChanged?.Invoke(Injections);return true;
        }
        public bool ReturnToCheckpoint()
        {if(!waiting||!health.IsDefeated)return false;ResumeControls();recovery.Respawn();return true;}
        private void CheckpointRecovered(){if(waiting)ResumeControls();protectionUntil=0;}
    }
}
