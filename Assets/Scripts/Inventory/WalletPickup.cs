using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Inventory
{
    public sealed class WalletPickup : MonoBehaviour
    {
        [SerializeField, Min(.2f)] private float pickupRadius=1.95f;
        private string rewardId;private int amount;private bool collected;private float nextCheck;
        private readonly RaycastHit2D[] hits=new RaycastHit2D[8];
        public void Initialize(string id,int value){rewardId=id;amount=Mathf.Clamp(value,0,9999);}
        public bool TryCollect(PlayerSupplies player)
        {
            if(collected||player==null||!player.CanCollect||Vector2.SqrMagnitude(player.transform.position-transform.position)>pickupRadius*pickupRadius||
                CombatVisibility2D.IsBlocked(transform.position,player.transform.position,1,hits,transform,player.transform))return false;
            if(!player.CollectWallet(rewardId,amount))return false;
            collected=true;gameObject.SetActive(false);Destroy(gameObject);return true;
        }
        private void OnTriggerEnter2D(Collider2D other)=>TryCollect(other.GetComponentInParent<PlayerSupplies>());
        private void OnTriggerStay2D(Collider2D other)
        {if(Time.time<nextCheck)return;nextCheck=Time.time+.2f;TryCollect(other.GetComponentInParent<PlayerSupplies>());}
    }
}
