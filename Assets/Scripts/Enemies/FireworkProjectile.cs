using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    // Two prewarmed instances per specialist; no global projectile or effect manager.
    [RequireComponent(typeof(Rigidbody2D),typeof(CircleCollider2D))]
    public sealed class FireworkProjectile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer rocket;
        [SerializeField] private LineRenderer warning;
        [SerializeField,Min(.1f)] private float radius=2.7f,flightSeconds=.95f,arcHeight=2.6f,damage=24,impulse=4;
        [SerializeField] private Vector2 normalFuse=new Vector2(.8f,1.5f),shortFuse=new Vector2(.25f,.45f),longFuse=new Vector2(2,3.2f);
        [SerializeField, Range(0,1)] private float shortFuseChance=.18f,longFuseChance=.18f;
        [SerializeField, Min(.1f)] private float burstSeconds=.8f;
        [SerializeField] private Sprite effectSprite;
        [SerializeField] private Sprite flashSprite,glowSprite;
        [SerializeField] private AudioClip burstSound;
        [SerializeField] private LayerMask geometry=1;
        private readonly Collider2D[] overlaps=new Collider2D[32];
        private readonly Health[] affected=new Health[32];
        private readonly RaycastHit2D[] hits=new RaycastHit2D[8];
        private GameObject source;private Vector2 end;private Color tint;
        private bool launched,exploded,landed;
        private Rigidbody2D body;private AudioSource sound;
        private readonly SpriteRenderer[] sparks=new SpriteRenderer[24],secondary=new SpriteRenderer[6],haze=new SpriteRenderer[3];
        private SpriteRenderer flash,glow;
        public int PooledVisualCount=>sparks.Length+secondary.Length+haze.Length+2;
        private float started,blastAt,hideAt;
        public bool Busy=>gameObject.activeSelf;
        public bool Launched=>launched;
        public bool Landed=>landed;
        public bool Exploded=>exploded;
        public float DetonationDelay=>selectedFuse;
        private float selectedFuse;
        public Vector2 BundlePosition=>rocket.transform.position;
        public int LastAffected{get;private set;}
        public float Radius=>radius;
        public Vector2 Destination=>end;
        private void Awake()
        {
            body=GetComponent<Rigidbody2D>();body.simulated=false;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            body.excludeLayers=~geometry.value;body.constraints=RigidbodyConstraints2D.FreezeRotation;
            GetComponent<CircleCollider2D>().radius=.4f;
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.volume=.22f;sound.dopplerLevel=0;
            for(int i=0;i<sparks.Length;i++)sparks[i]=Visual("CartoonBurst"+i,32);
            for(int i=0;i<secondary.Length;i++)secondary[i]=Visual("SecondarySpark"+i,33);
            for(int i=0;i<haze.Length;i++){haze[i]=Visual("FireworkHaze"+i,29);if(glowSprite!=null)haze[i].sprite=glowSprite;}
            flash=Visual("FireworkFlash",34);glow=Visual("FireworkGlow",28);if(flashSprite!=null)flash.sprite=flashSprite;if(glowSprite!=null)glow.sprite=glowSprite;
        }
        private SpriteRenderer Visual(string name,int order)
        {var g=new GameObject(name);g.transform.SetParent(transform,false);var r=g.AddComponent<SpriteRenderer>();r.sprite=effectSprite;r.sortingOrder=order;r.enabled=false;return r;}
        private void HideEffects(){foreach(var r in sparks)r.enabled=false;foreach(var r in secondary)r.enabled=false;foreach(var r in haze)r.enabled=false;flash.enabled=glow.enabled=false;}
        public void Prepare(GameObject owner,Vector2 destination,Color color,bool friendlyFire)
        {
            source=owner;end=destination;tint=color;launched=exploded=landed=false;LastAffected=0;blastAt=0;
            float roll=Random.value;
            var range=roll<shortFuseChance?shortFuse:roll<Mathf.Clamp01(shortFuseChance+longFuseChance)?longFuse:normalFuse;
            selectedFuse=Random.Range(Mathf.Max(.1f,range.x),Mathf.Max(.1f,Mathf.Max(range.x,range.y)));
            gameObject.SetActive(true);body.simulated=false;rocket.enabled=true;rocket.color=Color.white;warning.enabled=true;warning.startColor=warning.endColor=new Color(color.r,color.g,color.b,.65f);
            HideEffects();
            Ring(radius);started=Time.time;
        }
        public void Hold(Vector2 hand){if(!launched&&!exploded){transform.position=hand;rocket.transform.position=hand;}}
        private void Ring(float size)
        {
            warning.positionCount=33;
            for(int i=0;i<=32;i++){float angle=i*Mathf.PI/16;warning.SetPosition(i,new Vector3(end.x+Mathf.Cos(angle)*size,end.y+Mathf.Sin(angle)*size,-.15f));}
        }
        public void Launch(Vector2 origin)
        {
            started=Time.time;launched=true;transform.position=origin;rocket.transform.localPosition=Vector3.zero;body.position=origin;
            body.gravityScale=8*arcHeight/(flightSeconds*flightSeconds*Mathf.Max(.1f,-Physics2D.gravity.y));body.simulated=true;
            body.linearVelocity=(end-origin)/flightSeconds-Physics2D.gravity*body.gravityScale*(.5f*flightSeconds);
            blastAt=Time.time+selectedFuse;
        }
        public void Cancel(){if(!launched&&!exploded)gameObject.SetActive(false);}
        private void OnCollisionEnter2D(Collision2D hit){if(launched&&!landed&&!exploded&&((1<<hit.gameObject.layer)&geometry.value)!=0)Land();}
        private void Land(){landed=true;end=body.position;body.linearVelocity=Vector2.zero;body.simulated=false;Ring(radius);}
        public void ExplodeHeld(Vector2 hand){Hold(hand);end=hand;Detonate();}
        private void Update()
        {
            if(launched&&!landed&&!exploded&&Time.time-started>5)Land();
            if(blastAt>0&&!exploded&&Time.time>=blastAt)Detonate();
            if(exploded)
            {
                float alpha=Mathf.Clamp01((hideAt-Time.time)/burstSeconds);warning.startColor=warning.endColor=new Color(tint.r,tint.g,tint.b,alpha*alpha*.05f);
                float t=1-alpha,seconds=t*burstSeconds;
                flash.enabled=seconds<.075f;flash.transform.position=(Vector3)end+Vector3.back*.25f;flash.transform.localScale=Vector3.one*Mathf.Lerp(.45f,1.6f,Mathf.Clamp01(seconds/.075f));flash.color=new Color(1,1,.9f,.85f*Mathf.Clamp01(1-seconds/.075f));
                // A restrained world-space wash, not bloom or a full-screen flash.
                glow.enabled=seconds<.14f;glow.transform.position=(Vector3)end+Vector3.back*.1f;glow.transform.localScale=new Vector3(radius*1.5f,radius*.8f,1);glow.color=new Color(1,.8f,.45f,.07f*Mathf.Clamp01(1-seconds/.14f));
                for(int i=0;i<sparks.Length;i++)
                {
                    var r=sparks[i];float a=i*2.399963f+tint.r*.7f;
                    float speed=.55f+.45f*((i*7%23)/22f),travel=radius*speed*(1-Mathf.Pow(1-t,3));
                    r.transform.position=(Vector3)end+new Vector3(Mathf.Cos(a)*travel,Mathf.Sin(a)*travel-.7f*t*t,-.2f);
                    r.transform.localScale=new Vector3((.12f+.32f*speed)*Mathf.Sqrt(alpha),.035f+.035f*alpha,1);
                    r.transform.rotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg);
                    var color=Color.HSVToRGB(Mathf.Repeat(i*.137f+tint.g*.2f,1),.7f,1);color.a=alpha*alpha;r.color=Color.Lerp(color,new Color(1,.72f,.25f,color.a),t*.6f);
                }
                for(int i=0;i<secondary.Length;i++)
                {
                    float delay=i<3?.22f:.4f,age=seconds-delay,life=.28f,u=Mathf.Clamp01(age/life),a=i*Mathf.PI/3+tint.b;
                    var r=secondary[i];r.enabled=age>=0&&age<life;
                    r.transform.position=(Vector3)end+new Vector3(Mathf.Cos(a)*radius*(.45f+u*.22f),Mathf.Sin(a)*radius*(.45f+u*.22f)-u*u*.2f,-.22f);
                    float size=.05f+.13f*Mathf.Sin(u*Mathf.PI);r.transform.localScale=Vector3.one*size;r.transform.rotation=Quaternion.Euler(0,0,45+u*90);
                    var color=Color.HSVToRGB(Mathf.Repeat(i*.21f+tint.r,1),.5f,1);color.a=1-u;r.color=color;
                }
                for(int i=0;i<haze.Length;i++)
                {
                    var r=haze[i];r.enabled=t>.12f;r.transform.position=(Vector3)end+new Vector3((i-1)*.4f,.2f+t*.45f,-.05f);
                    r.transform.localScale=Vector3.one*(.3f+t*.55f);r.color=new Color(.65f,.62f,.72f,.055f*Mathf.Sin(Mathf.PI*t));
                }
                if(Time.time>=hideAt)gameObject.SetActive(false);
            }
            else {warning.widthMultiplier=.045f+.015f*Mathf.Sin(Time.time*(landed?16:8));if(landed)rocket.color=Color.Lerp(Color.white,new Color(1,.65f,.3f),.25f+.2f*Mathf.Sin(Time.time*16));}
        }
        public void Detonate()
        {
            if(exploded)return;if(launched)end=body.position;exploded=true;body.simulated=false;rocket.enabled=false;hideAt=Time.time+burstSeconds;warning.widthMultiplier=.08f;Ring(radius);LastAffected=0;
            HideEffects();foreach(var r in sparks){r.enabled=true;r.transform.position=(Vector3)end+Vector3.back*.2f;r.color=tint;}
            flash.enabled=true;flash.transform.position=(Vector3)end+Vector3.back*.25f;flash.transform.localScale=Vector3.one*.45f;flash.color=new Color(1,1,.9f,.85f);
            if(burstSound!=null)sound.PlayOneShot(burstSound);
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(~0);
            int count=Physics2D.OverlapCircle(end,radius,filter,overlaps);
            if(count==overlaps.Length)return; // Saturated query fails closed.
            for(int i=0;i<count;i++)
            {
                if(SamuraiBodyHitbox.IsMovementShape(overlaps[i]))continue;
                var hp=overlaps[i].GetComponentInParent<Health>();if(hp==null||hp.IsDefeated)continue;
                bool duplicate=false;for(int j=0;j<LastAffected;j++)if(affected[j]==hp)duplicate=true;if(duplicate)continue;
                Vector2 point=overlaps[i].bounds.center;
                if(CombatVisibility2D.IsBlocked(end,point,geometry,hits,source!=null?source.transform:null,hp.transform))continue;
                var direction=point-end;float distance=direction.magnitude;if(direction.sqrMagnitude<.01f)direction=Vector2.up;
                float weight=1-.5f*Mathf.Clamp01(distance/radius);
                var impact=new PhysicalImpact(point,direction,impulse*weight,hp.GetComponent<Rigidbody2D>());
                bool self=hp.gameObject==source;
                if(hp.ApplyDamage(new DamageInfo(damage*weight,CombatTeam.Enemy,self?null:source,impact,true)))
                {
                    affected[LastAffected++]=hp;
                    if(hp.Team==CombatTeam.Enemy&&!hp.IsDefeated)
                    {var combat=hp.GetComponent<EnemyCombat>();if(combat!=null&&combat.ApplyAllyMorale(true,3))combat.ExpressAllyFear();hp.GetComponent<LarperAwareness>()?.FriendlyFireShock();}
                }
            }
        }
    }
}
