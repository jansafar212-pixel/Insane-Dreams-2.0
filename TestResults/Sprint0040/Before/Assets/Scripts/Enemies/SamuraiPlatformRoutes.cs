using InsaneDreams.Traversal;
using UnityEngine;

namespace InsaneDreams.Enemies
{
    public enum SamuraiTransition { None,Walk,SingleJump,DoubleJump,Ladder }
    // A bounded, local connectivity cache. Edges are estimates; the committed arc is swept in SamuraiCombat.
    public sealed class SamuraiPlatformRoutes
    {
        private readonly Collider2D[] query=new Collider2D[64],nodes=new Collider2D[48];
        private readonly LadderVolume[] ladders=new LadderVolume[8];
        private readonly RaycastHit2D[] supportHits=new RaycastHit2D[8];
        private readonly float[] cost=new float[48],blockedUntil=new float[16];
        private readonly int[] previous=new int[48];private readonly bool[] visited=new bool[48];
        private readonly Collider2D[] blockedFrom=new Collider2D[16],blockedTo=new Collider2D[16];
        private int count,ladderCount,failedSlot;private float refreshAt;private Vector2 cachedAt;
        public Collider2D Source{get;private set;}
        public Collider2D Destination{get;private set;}
        public SamuraiTransition Kind{get;private set;}
        public LadderVolume Ladder{get;private set;}
        public string LastFailure{get;private set;}="None";
        public void Reset(){count=ladderCount=0;refreshAt=0;Source=Destination=null;for(int i=0;i<16;i++)blockedUntil[i]=0;LastFailure="None";}
        public void Fail(Collider2D from,Collider2D to,float seconds,string reason)
        {blockedFrom[failedSlot]=from;blockedTo[failedSlot]=to;blockedUntil[failedSlot]=Time.time+seconds;failedSlot=(failedSlot+1)%16;refreshAt=0;LastFailure=reason;}
        private bool Blocked(Collider2D a,Collider2D b)
        {for(int i=0;i<16;i++)if(blockedFrom[i]==a&&blockedTo[i]==b&&Time.time<blockedUntil[i])return true;return false;}
        public bool Select(Vector2 start,Vector2 focus,float half,LayerMask layers,float singleRise,float doubleRise,float horizontal,float drop)
        {
            Source=Destination=null;Kind=SamuraiTransition.None;Ladder=null;
            if(Time.time>=refreshAt||Vector2.Distance(start,cachedAt)>8)
            {
                cachedAt=start;refreshAt=Time.time+1;count=ladderCount=0;
                var filter=new ContactFilter2D{useTriggers=true};filter.SetLayerMask(layers);
                int n=Physics2D.OverlapBox(start,new Vector2(38,30),0,filter,query);
                if(n==query.Length){LastFailure="Local geometry buffer full";return false;}
                for(int i=0;i<n;i++)
                {
                    var c=query[i];if(c.isTrigger){var l=c.GetComponent<LadderVolume>();if(l!=null&&ladderCount<8)ladders[ladderCount++]=l;continue;}
                    // One-unit warehouse crates are valid intermediate footholds.
                    if(c is BoxCollider2D&&c.bounds.size.x>=.9f&&count<nodes.Length)nodes[count++]=c;
                }
            }
            Source=Support(start+Vector2.down*(half-.15f),.45f,layers);
            var goal=Support(focus+Vector2.up*.2f,5,layers);
            int source=Index(Source),target=Index(goal);Destination=null;Kind=SamuraiTransition.None;Ladder=null;
            if(source<0||target<0||source==target)return false;
            for(int i=0;i<count;i++){cost[i]=float.PositiveInfinity;previous[i]=-1;visited[i]=false;}cost[source]=0;
            for(int iteration=0;iteration<count;iteration++)
            {
                int u=-1;float cheapest=float.PositiveInfinity;
                for(int i=0;i<count;i++)if(!visited[i]&&cost[i]<cheapest){cheapest=cost[i];u=i;}
                if(u<0||u==target)break;visited[u]=true;
                if(!Valid(nodes[u]))continue;
                for(int v=0;v<count;v++)
                {
                    if(v==u||visited[v]||!Valid(nodes[v])||Blocked(nodes[u],nodes[v]))continue;
                    var kind=Connection(nodes[u],nodes[v],singleRise,doubleRise,horizontal,drop,out _);if(kind==SamuraiTransition.None)continue;
                    var a=nodes[u].bounds;var b=nodes[v].bounds;
                    float gap=Mathf.Max(0,Mathf.Max(a.min.x-b.max.x,b.min.x-a.max.x));
                    float next=cost[u]+.85f+gap*.15f+Mathf.Abs(a.max.y-b.max.y)*.35f+(kind==SamuraiTransition.DoubleJump?.15f:kind==SamuraiTransition.Ladder?3:0);
                    if(next<cost[v]){cost[v]=next;previous[v]=u;}
                }
            }
            if(previous[target]<0){LastFailure="No local connected route";return false;}
            int nextNode=target;for(int i=0;i<count&&previous[nextNode]!=source;i++){nextNode=previous[nextNode];if(nextNode<0)return false;}
            Destination=nodes[nextNode];Kind=Connection(Source,Destination,singleRise,doubleRise,horizontal,drop,out var ladder);Ladder=ladder;
            return true;
        }
        private Collider2D Support(Vector2 origin,float distance,LayerMask layers)
        {
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(layers);
            int n=Physics2D.Raycast(origin,Vector2.down,filter,supportHits,distance);
            return n>0&&n<supportHits.Length?supportHits[0].collider:null;
        }
        private int Index(Collider2D c){if(c==null)return -1;for(int i=0;i<count;i++)if(nodes[i]==c)return i;return -1;}
        private static bool Valid(Collider2D c)=>c!=null&&c.enabled&&c.gameObject.activeInHierarchy;
        private SamuraiTransition Connection(Collider2D from,Collider2D to,float singleRise,float doubleRise,float reach,float drop,out LadderVolume ladder)
        {
            ladder=null;var a=from.bounds;var b=to.bounds;float dy=b.max.y-a.max.y;
            float gap=Mathf.Max(0,Mathf.Max(a.min.x-b.max.x,b.min.x-a.max.x))+.9f;
            if(Mathf.Abs(dy)<.15f&&gap<1)return SamuraiTransition.Walk;
            if(dy<=singleRise&&dy>=-drop&&gap<=reach*.65f)return SamuraiTransition.SingleJump;
            if(dy<=doubleRise&&dy>=-drop&&gap<=reach)return SamuraiTransition.DoubleJump;
            for(int i=0;i<ladderCount;i++)
            {
                var l=ladders[i];if(l==null||!l.isActiveAndEnabled)continue;
                if(l.Bottom>Mathf.Min(a.max.y,b.max.y)+.8f||l.Top<Mathf.Max(a.max.y,b.max.y)+.6f)continue;
                if(Mathf.Abs(Mathf.Clamp(l.CenterX,a.min.x,a.max.x)-l.CenterX)>2||Mathf.Abs(Mathf.Clamp(l.CenterX,b.min.x,b.max.x)-l.CenterX)>2)continue;
                ladder=l;return SamuraiTransition.Ladder;
            }
            return SamuraiTransition.None;
        }
    }
}
