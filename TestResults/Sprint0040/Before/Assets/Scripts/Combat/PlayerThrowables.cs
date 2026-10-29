using InsaneDreams.Input;
using InsaneDreams.Inventory;
using UnityEngine;
namespace InsaneDreams.Combat
{
    [DefaultExecutionOrder(80)]
    public sealed class PlayerThrowables : MonoBehaviour
    {
        public const int Capacity=5;
        public int MaximumCapacity{get;private set;}=Capacity;
        public void ConfigureCapacity(int value){Cancel();MaximumCapacity=Mathf.Clamp(value,3,Capacity);for(int i=0;i<3;i++)stock[i]=Mathf.Min(stock[i],MaximumCapacity);InventoryChanged?.Invoke();}
        [SerializeField] private ThrowableProjectile2D projectile;
        [SerializeField] private Sprite[] icons;
        [SerializeField,Min(1)] private float throwSpeed=12;
        [SerializeField,Min(.1f)] private float throwCooldown=.4f;
        private readonly ThrowableProjectile2D[] pool=new ThrowableProjectile2D[6];private readonly int[] stock=new int[3];
        private readonly RaycastHit2D[] geometryHits=new RaycastHit2D[8];
        private PlayerActions actions;private PlayerLoadout loadout;private Health health;private PlayerHealing healing;private LineRenderer preview;private Material material;
        private bool aiming;private Vector2 rememberedAim;private float nextThrow,nextPreview;
        private Collider2D playerBox;private PlayerCover2D cover;
        public const float ClearanceRadius=.12f;
        public Vector2 PreviewOrigin { get; private set; }
        public ThrowableKind Selected{get;private set;}
        public bool IsSelected=>loadout!=null&&loadout.SelectedSlot==EquipmentSlot.Melee;
        public bool IsAiming=>aiming;
        public int Throws{get;private set;}
        public event System.Action InventoryChanged;
        public int Count(ThrowableKind kind)=>(int)kind>=0&&(int)kind<stock.Length?stock[(int)kind]:0;
        public int TakeSupply(int[] available)
        {
            if(available==null)return 0;int total=0;
            for(int i=0;i<Mathf.Min(stock.Length,available.Length);i++){int added=Mathf.Clamp(available[i],0,MaximumCapacity-stock[i]);stock[i]+=added;available[i]-=added;total+=added;}
            if(total>0)InventoryChanged?.Invoke();return total;
        }
        public Sprite Icon=>icons!=null&&icons.Length==3?icons[(int)Selected]:null;
        public void Select(ThrowableKind kind){if((int)kind<0||(int)kind>2)return;Cancel();Selected=kind;}
        public void RestoreStock(int[] values){Cancel();for(int i=0;i<3;i++)stock[i]=Mathf.Clamp(values!=null&&values.Length>i?values[i]:0,0,MaximumCapacity);InventoryChanged?.Invoke();}
        private void Awake()
        {
            actions=GetComponent<PlayerActions>();loadout=GetComponent<PlayerLoadout>();health=GetComponent<Health>();healing=GetComponent<PlayerHealing>();
            playerBox=GetComponent<Collider2D>();cover=GetComponent<PlayerCover2D>();
            if(projectile!=null)for(int i=0;i<pool.Length;i++){pool[i]=Instantiate(projectile);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(pool[i].gameObject,gameObject.scene);pool[i].gameObject.SetActive(false);}
            preview=new GameObject("ThrowableTrajectory").AddComponent<LineRenderer>();preview.transform.SetParent(transform,false);preview.useWorldSpace=true;preview.positionCount=13;preview.startWidth=preview.endWidth=.035f;preview.sortingOrder=25;preview.startColor=preview.endColor=new Color(1,.8f,.2f,.7f);material=new Material(Shader.Find("Sprites/Default"));preview.sharedMaterial=material;preview.enabled=false;
        }
        private void AimEnded(Vector2 direction,bool valid){if(!valid)Cancel();}
        private void OnEnable(){actions.InputReset+=Cancel;actions.AimCancelled+=Cancel;actions.AimReleased+=AimEnded;loadout.SelectionChanging+=Cancel;}
        private void OnDisable(){actions.InputReset-=Cancel;actions.AimCancelled-=Cancel;actions.AimReleased-=AimEnded;loadout.SelectionChanging-=Cancel;Cancel();}
        public void Cancel(){aiming=false;if(preview!=null)preview.enabled=false;}
        public bool TryReleaseOrigin(Vector2 aim,out Vector2 origin)
        {
            Bounds b=playerBox.bounds;Vector2 center=b.center;
            if(cover!=null&&cover.IsCovered)
            {
                // Lift on the player's side of the solid face, then move the hand out.
                // Both legs are swept; a low ceiling refuses the throw rather than crossing it.
                Vector2 raised=new Vector2(center.x,Mathf.Max(b.max.y,cover.CoverTop)+ClearanceRadius+.04f);
                if(Blocked(new Vector2(center.x,b.max.y+ClearanceRadius),raised,out origin))return false;
                Vector2 end=raised+new Vector2(aim.x*.45f,Mathf.Max(0,aim.y)*.2f);
                if(Blocked(raised,end,out origin))return false;origin=end;return true;
            }
            Vector2 goal=center+aim*(b.extents.magnitude+ClearanceRadius+.04f);
            if(Blocked(center,goal,out var hit)){origin=hit-aim*.04f;return Vector2.Distance(center,origin)>b.extents.magnitude;}
            origin=goal;return true;
        }
        private bool Blocked(Vector2 from,Vector2 to,out Vector2 point){var f=new ContactFilter2D{useTriggers=false};f.SetLayerMask(1);Vector2 d=to-from;int n=Physics2D.CircleCast(from,ClearanceRadius,d.normalized,f,geometryHits,d.magnitude);point=n>0?geometryHits[0].centroid:to;return n>0;}
        private Vector2 Velocity(Vector2 aim)=>aim*throwSpeed+Vector2.up*2;
        private void Update()
        {
            if(!IsSelected||!actions.isActiveAndEnabled||health.IsDefeated||healing!=null&&healing.IsHealing){Cancel();return;}
            bool intent=actions.HasAimIntent; // Right-stick drag / desktop aim hold, never a fire-button tap.
            if(intent&&Count(Selected)>0&&Time.time>=nextThrow)
            {
                aiming=true;rememberedAim=actions.AimDirection.normalized;
                if(Time.time>=nextPreview){nextPreview=Time.time+.1f;preview.enabled=TryReleaseOrigin(rememberedAim,out var origin);PreviewOrigin=origin;Vector2 v=Velocity(rememberedAim),last=origin;bool blocked=false;for(int i=0;i<13;i++){float t=i*.1f;Vector2 at=origin+v*t+Physics2D.gravity*1.2f*(t*t*.5f);if(blocked)at=last;else if(i>0&&Blocked(last,at,out var point)){at=point;blocked=true;}preview.SetPosition(i,new Vector3(at.x,at.y,-.2f));last=at;}}
            }
            else if(aiming&&!intent)
            {
                aiming=false;preview.enabled=false;
                // Inventory or equipment can change between preview and release.
                // Only a successfully launched cached projectile consumes one item.
                if(Count(Selected)<=0||Time.time<nextThrow||!TryReleaseOrigin(rememberedAim,out var origin))return;
                foreach(var p in pool)if(p!=null&&!p.Busy){p.Launch(gameObject,Selected,origin,Velocity(rememberedAim));cover?.ExposeThrow();stock[(int)Selected]--;Throws++;nextThrow=Time.time+throwCooldown;InventoryChanged?.Invoke();break;}
            }
        }
        private void OnDestroy(){foreach(var p in pool)if(p!=null)Destroy(p.gameObject);if(material!=null)Destroy(material);}
    }
}
