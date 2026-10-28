using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class EnemyWalletDrop : MonoBehaviour
    {
        [SerializeField] private WalletPickup walletPrefab;
        [SerializeField] private Vector2Int valueRange=new Vector2Int(5,20);
        [SerializeField, Range(0,1)] private float dropChance=1;
        private Health health;private bool dropped;private WalletPickup wallet;
        private string rewardId;
        public WalletPickup Wallet=>wallet;
        private void Awake(){health=GetComponent<Health>();rewardId=System.Guid.NewGuid().ToString("N");}
        private void OnEnable()=>health.Defeated+=Defeated;
        private void OnDisable()=>health.Defeated-=Defeated;
        private void Defeated(DamageInfo _)
        {
            // Health's invocation snapshot still calls this if another death
            // listener has already deactivated the actor. Never reset on enable.
            if(dropped||health.Team!=CombatTeam.Enemy)return;dropped=true;
            if(walletPrefab==null||Random.value>=dropChance)return;
            wallet=Instantiate(walletPrefab,transform.position+Vector3.down*.45f,Quaternion.identity);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(wallet.gameObject,gameObject.scene);
            int minimum=Mathf.Clamp(valueRange.x,1,9999),maximum=Mathf.Clamp(valueRange.y,minimum,9999);
            wallet.Initialize(rewardId,Random.Range(minimum,maximum+1));
        }
        public void ResetDrop(){if(wallet!=null)Destroy(wallet.gameObject);wallet=null;dropped=false;}
        // A defeated actor may be culled before its reward is collected. The pickup
        // belongs to the scene; actor cleanup must not silently erase earned money.
    }
}
