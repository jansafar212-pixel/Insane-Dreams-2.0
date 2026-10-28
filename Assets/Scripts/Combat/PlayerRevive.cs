using InsaneDreams.Checkpoints;
using InsaneDreams.Input;
using UnityEngine;
namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(Health),typeof(PlayerRecovery),typeof(PlayerActions))]
    public sealed class PlayerRevive : MonoBehaviour
    {
        public const int Capacity=2;
        [SerializeField,Range(0,2)] private int initialInjections;
        [SerializeField] private bool automaticRevival=true;
        [SerializeField,Range(.1f,1)] private float revivalDelay=.35f;
        private Coroutine pendingRevival;
        public bool IsAutomatic=>automaticRevival;
        private Health health;private PlayerActions actions;private PlayerCombat combat;
        private Rigidbody2D body;private PlayerRecovery recovery;
        private bool waiting,wasSimulated,chargeReserved;private float protectionUntil;
        public int Injections{get;private set;}
        public bool CanOfferRevive=>isActiveAndEnabled&&health!=null&&health.IsDefeated&&(chargeReserved||Injections>0);
        public float ProtectionRemaining=>Mathf.Max(0,protectionUntil-Time.time);
        public event System.Action<int> InventoryChanged;
        public event System.Action Revived;
        private void Awake(){health=GetComponent<Health>();actions=GetComponent<PlayerActions>();combat=GetComponent<PlayerCombat>();body=GetComponent<Rigidbody2D>();recovery=GetComponent<PlayerRecovery>();Injections=Mathf.Clamp(initialInjections,0,Capacity);}
        private void OnEnable(){health.Defeated+=Defeated;recovery.Recovered+=CheckpointRecovered;}
        private void OnDisable(){health.Defeated-=Defeated;recovery.Recovered-=CheckpointRecovered;CancelPending();chargeReserved=false;if(waiting){body.simulated=wasSimulated;waiting=false;}}
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
            Injections--;chargeReserved=true;InventoryChanged?.Invoke(Injections);
            // Let all death/game-over observers see the still-available charge first.
            if(automaticRevival)pendingRevival=StartCoroutine(AutomaticRevival());
        }
        private System.Collections.IEnumerator AutomaticRevival(){yield return new WaitForSeconds(revivalDelay);pendingRevival=null;TryRevive();}
        private void CancelPending(){if(pendingRevival!=null)StopCoroutine(pendingRevival);pendingRevival=null;}
        private void ResumeControls()
        {if(waiting)body.simulated=wasSimulated;waiting=false;actions.ResetActions();actions.enabled=true;combat.enabled=true;}
        public bool TryRevive()
        {
            if(!waiting||!CanOfferRevive)return false;
            CancelPending();
            chargeReserved=false;health.Restore();health.ProtectFor(2);protectionUntil=Time.time+2;
            ResumeControls();GetComponent<Visuals.PlayerDeathRagdoll>()?.ReturnToLife();Revived?.Invoke();return true;
        }
        public bool ReturnToCheckpoint()
        {if(!waiting||!health.IsDefeated)return false;CancelPending();chargeReserved=false;ResumeControls();recovery.Respawn();return true;}
        private void CheckpointRecovered(){CancelPending();chargeReserved=false;if(waiting)ResumeControls();protectionUntil=0;}
    }
}
