using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using UnityEngine;
namespace InsaneDreams.Inventory
{
    [RequireComponent(typeof(PlayerActions),typeof(PlayerHealing),typeof(PlayerLoadout))]
    public sealed class PlayerSupplies : MonoBehaviour
    {
        private readonly HashSet<SupplyCrate2D> nearby=new HashSet<SupplyCrate2D>();
        private readonly HashSet<string> wallets=new HashSet<string>();
        private PlayerActions actions;private Health health;
        public PlayerHealing Healing{get;private set;}
        public PlayerLoadout Loadout{get;private set;}
        public int Earnings{get;private set;}
        public string Notification=>Time.unscaledTime<noticeUntil?notice:string.Empty;
        private float noticeUntil;private string notice;
        public bool CanCollect=>isActiveAndEnabled&&actions.isActiveAndEnabled&&!health.IsDefeated;
        public SupplyCrate2D NearbyCrate
        {
            get
            {
                SupplyCrate2D best=null;float distance=float.PositiveInfinity;
                foreach(var crate in nearby)
                {if(crate==null||!crate.CanUse(this))continue;float d=Vector2.SqrMagnitude(crate.transform.position-transform.position);if(d<distance){best=crate;distance=d;}}
                return best;
            }
        }
        private void Awake(){actions=GetComponent<PlayerActions>();health=GetComponent<Health>();Healing=GetComponent<PlayerHealing>();Loadout=GetComponent<PlayerLoadout>();}
        private void OnEnable()=>actions.InteractionRequested+=Interact;
        private void OnDisable(){actions.InteractionRequested-=Interact;nearby.Clear();}
        private void Interact(){var crate=NearbyCrate;if(crate!=null&&crate.TrySupply(this))Notify("Supplies replenished");}
        public void Register(SupplyCrate2D crate)=>nearby.Add(crate);
        public void Unregister(SupplyCrate2D crate)=>nearby.Remove(crate);
        public bool CollectWallet(string id,int amount)
        {
            if(!CanCollect||string.IsNullOrEmpty(id)||amount<=0||!wallets.Add(id))return false;
            Earnings=(int)System.Math.Min(int.MaxValue,(long)Earnings+amount);Notify("Wallet collected");return true;
        }
        private void Notify(string message){notice=message;noticeUntil=Time.unscaledTime+2.5f;}
        public void ResetMission()
        {
            wallets.Clear();Earnings=0;noticeUntil=0;Healing.ResetSupply();
            GetComponent<Combat.PlayerRevive>()?.ResetInventory();
            // Only at an explicit full replay; checkpoint deaths preserve finite
            // supplies and reward claims. No scene search occurs during gameplay.
            foreach(var root in gameObject.scene.GetRootGameObjects())
            {
                foreach(var crate in root.GetComponentsInChildren<SupplyCrate2D>(true))crate.ResetContents();
                foreach(var drop in root.GetComponentsInChildren<InsaneDreams.Enemies.EnemyWalletDrop>(true))drop.ResetDrop();
            }
        }
    }
}
