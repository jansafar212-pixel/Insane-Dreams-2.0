using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Inventory
{
    public sealed class WalletPickup : MonoBehaviour
    {
        [SerializeField, Min(.2f)] private float pickupRadius=1.95f;
        private string rewardId;private int amount;private bool collected;private float nextCheck;
        private readonly RaycastHit2D[] hits=new RaycastHit2D[8];
        private readonly Collider2D[] clearance=new Collider2D[12];
        private Vector2 dropAnchor;private int recoveries;private float recoverAt;
        public int Relocations { get; private set; }
        public void Initialize(string id,int value)
        {
            rewardId=id;amount=Mathf.Clamp(value,0,9999);dropAnchor=transform.position;recoveries=0;
            RecoverPlacement();recoverAt=Time.time+.6f;
        }
        private bool Free(Vector2 foot)
        {
            var f=new ContactFilter2D{useTriggers=false};f.SetLayerMask(1);
            // Space for a real crouched player, not merely the decorative wallet pixels.
            return Physics2D.OverlapBox(foot+Vector2.up*.6f,new Vector2(.65f,1.05f),0,f,clearance)==0;
        }
        public bool RecoverPlacement()
        {
            var f=new ContactFilter2D{useTriggers=false};f.SetLayerMask(1);
            Vector2 anchor=dropAnchor;
            Vector2 best=anchor;float bestCost=float.PositiveInfinity;
            // Static trigger pickup: there is deliberately no falling rigidbody. Find
            // walkable support once, with two bounded follow-ups for moving geometry.
            for(int i=0;i<19;i++)
            {
                float dx=i==0?0:((i+1)/2)*.5f*(i%2==1?-1:1);
                Vector2 start=anchor+new Vector2(dx,.8f);
                int n=Physics2D.Raycast(start,Vector2.down,f,hits,32);
                if(n==hits.Length)continue;
                for(int j=0;j<n;j++)
                {
                    if(hits[j].distance<.01f||hits[j].normal.y<.7f)continue;
                    Vector2 point=hits[j].point+Vector2.up*.14f;
                    if(!Free(point))continue;
                    float cost=Mathf.Abs(point.y-anchor.y)*2+Mathf.Abs(dx);
                    if(cost<bestCost){bestCost=cost;best=point;}
                }
                if(bestCost<.5f)break;
            }
            if(float.IsPositiveInfinity(bestCost))return false;
            if(Vector2.Distance(transform.position,best)>.02f){transform.position=best;Relocations++;}
            return true;
        }
        private void Update()
        {
            if(recoveries>=2||Time.time<recoverAt||collected)return;
            recoveries++;recoverAt=Time.time+1.4f;
            var f=new ContactFilter2D{useTriggers=false};f.SetLayerMask(1);
            int count=Physics2D.Raycast(transform.position,Vector2.down,f,hits,.3f);
            bool supported=false;for(int i=0;i<count;i++)if(hits[i].normal.y>.7f)supported=true;
            if(!supported||!Free(transform.position))RecoverPlacement();
        }
        public bool TryCollect(PlayerSupplies player)
        {
            if(collected||player==null||!player.CanCollect)return false;
            var body=player.GetComponent<Collider2D>();
            Vector2 target=body!=null?body.ClosestPoint(transform.position):(Vector2)player.transform.position;
            if(Vector2.SqrMagnitude(target-(Vector2)transform.position)>pickupRadius*pickupRadius||
                CombatVisibility2D.IsBlocked(transform.position,target,1,hits,transform,player.transform,false))return false;
            if(!player.CollectWallet(rewardId,amount))return false;
            collected=true;gameObject.SetActive(false);Destroy(gameObject);return true;
        }
        private void OnTriggerEnter2D(Collider2D other)=>TryCollect(other.GetComponentInParent<PlayerSupplies>());
        private void OnTriggerStay2D(Collider2D other)
        {if(Time.time<nextCheck)return;nextCheck=Time.time+.2f;TryCollect(other.GetComponentInParent<PlayerSupplies>());}
    }
}
