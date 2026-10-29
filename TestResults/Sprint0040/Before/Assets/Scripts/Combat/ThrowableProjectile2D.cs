using UnityEngine;
namespace InsaneDreams.Combat
{
    public enum ThrowableKind { Fragmentation,Flashbang,Smoke }
    [RequireComponent(typeof(Rigidbody2D),typeof(SpriteRenderer),typeof(SmokeCloud2D))]
    public sealed class ThrowableProjectile2D : MonoBehaviour
    {
        [SerializeField] private Sprite[] icons;
        [SerializeField] private Sprite smokeSprite;
        [SerializeField,Min(.1f)] private float fuseSeconds=1.4f,blastRadius=4.5f,fragmentationDamage=145f,blastImpulse=2;
        [SerializeField,Min(.1f)] private float flashRadius=6,flashSeconds=1.6f,smokeRadius=3.5f,smokeSeconds=8;
        [Header("Pooled fragmentation presentation")]
        [SerializeField] private Sprite burstSprite,fragmentSprite;
        [SerializeField] private AudioClip explosionSound;
        [SerializeField,Range(4,12)] private int fragmentCount=8;
        [SerializeField,Min(.2f)] private float burstSeconds=.95f,fireballDiameter=3.6f,smokeDiameter=4.8f,fragmentDistance=4.2f;
        [SerializeField,Range(0,.12f)] private float shakeAmplitude=.08f;
        [SerializeField,Range(.01f,.25f)] private float shakeSeconds=.16f;
        [SerializeField,Min(1)] private float shakeRange=12;
        private Rigidbody2D body;private SpriteRenderer visual;private SmokeCloud2D cloud;private LineRenderer ring;private Material material;
        private readonly SpriteRenderer[] fire=new SpriteRenderer[3],haze=new SpriteRenderer[3];private SpriteRenderer flash;private SpriteRenderer[] fragments;private AudioSource sound;private float explodedAt;
        private readonly AreaCombat2D area=new AreaCombat2D();private GameObject owner;private ThrowableKind kind;private float detonateAt,hideAt;private bool exploded;
        public bool Busy=>gameObject.activeSelf;
        public int Detonations{get;private set;}
        public int Affected{get;private set;}
        private void Awake()
        {
            body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();cloud=GetComponent<SmokeCloud2D>();cloud.enabled=false;
            ring=new GameObject("ThrowableBurst").AddComponent<LineRenderer>();ring.transform.SetParent(transform,false);ring.useWorldSpace=true;ring.positionCount=17;ring.startWidth=ring.endWidth=.055f;ring.sortingOrder=25;material=new Material(Shader.Find("Sprites/Default"));ring.sharedMaterial=material;ring.enabled=false;
            flash=FragVisual("FragFlash",36,burstSprite);for(int i=0;i<3;i++){fire[i]=FragVisual("FragFire"+i,32+i,burstSprite);haze[i]=FragVisual("FragSmoke"+i,29,burstSprite);}
            fragments=new SpriteRenderer[Mathf.Clamp(fragmentCount,4,12)];for(int i=0;i<fragments.Length;i++)fragments[i]=FragVisual("FragDebris"+i,35,fragmentSprite);
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.rolloffMode=AudioRolloffMode.Linear;sound.minDistance=2;sound.maxDistance=20;sound.dopplerLevel=0;sound.volume=.5f;
        }
        private SpriteRenderer FragVisual(string name,int order,Sprite sprite){var g=new GameObject(name);g.transform.SetParent(transform,false);var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;r.enabled=false;return r;}
        private void HideFrag(){if(flash==null)return;flash.enabled=false;foreach(var r in fire)r.enabled=false;foreach(var r in haze)r.enabled=false;foreach(var r in fragments)r.enabled=false;}
        private static Vector3 Pixel(Vector2 p)=>new Vector3(Mathf.Round(p.x*32)/32,Mathf.Round(p.y*32)/32,-.15f);
        private static void Size(SpriteRenderer r,float width,float height){if(r.sprite==null)return;var size=r.sprite.bounds.size;r.transform.localScale=new Vector3(width/size.x,height/size.y,1);}
        private void FragPresentation()
        {
            float age=Time.time-explodedAt,u=Mathf.Clamp01(age/burstSeconds);Vector2 center=transform.position;
            flash.enabled=age<.055f;flash.transform.position=Pixel(center);Size(flash,fireballDiameter,fireballDiameter);flash.color=new Color(1,1,.8f,.85f*Mathf.Clamp01(1-age/.055f));
            for(int i=0;i<3;i++){
                var r=fire[i];r.enabled=age<.4f;float size=fireballDiameter*(1-i*.24f)*Mathf.Lerp(.25f,1,Mathf.Clamp01(age/.14f));r.transform.position=Pixel(center+Vector2.up*(i*.08f+age*.3f));Size(r,size,size*(i==0?.85f:1));r.color=i==0?new Color(.9f,.13f,.03f):i==1?new Color(1,.43f,.03f):new Color(1,.87f,.12f);var c=r.color;c.a=Mathf.Clamp01((.4f-age)/.14f);r.color=c;
                r=haze[i];r.enabled=age>=.12f;r.transform.position=Pixel(center+new Vector2((i-1)*.65f,.25f+u*.7f));float puff=smokeDiameter*(.38f+u*.36f);Size(r,puff,puff*.8f);r.color=new Color(.32f+i*.035f,.33f+i*.035f,.3f+i*.035f,.55f*(1-u));
            }
            for(int i=0;i<fragments.Length;i++){
                var r=fragments[i];float t=Mathf.Clamp01(age/.6f),a=i*2.399963f,speed=.6f+.4f*((i*7%11)/10f);r.enabled=age<.6f;float travel=fragmentDistance*speed*(1-(1-t)*(1-t));r.transform.position=Pixel(center+new Vector2(Mathf.Cos(a)*travel,Mathf.Sin(a)*travel-age*age*2));r.transform.rotation=Quaternion.Euler(0,0,Mathf.Round(a*Mathf.Rad2Deg/45)*45);Size(r,.12f,.07f+(i%3)*.025f);r.color=i%2==0?new Color(1,.65f,.15f,1-t):new Color(.28f,.24f,.18f,1-t);
            }
        }
        public void Launch(GameObject source,ThrowableKind selected,Vector2 origin,Vector2 velocity,float fuse=-1)
        {
            owner=source;kind=selected;exploded=false;Affected=0;cloud.enabled=false;ring.enabled=false;HideFrag();sound.Stop();gameObject.SetActive(true);body.simulated=false;transform.position=origin;body.position=origin;transform.localScale=Vector3.one;visual.sprite=icons[(int)kind];visual.color=Color.white;visual.enabled=true;body.simulated=true;body.linearVelocity=velocity;body.angularVelocity=180;detonateAt=Time.time+(fuse>=0?fuse:fuseSeconds);
        }
        // Authored hazards reuse the grenade's damage, bounded queries and cached visuals.
        // Immediate detonation never adds a simulated moving mine projectile.
        public void Burst(GameObject source,Vector2 origin)
        {Launch(source,ThrowableKind.Fragmentation,origin,Vector2.zero,0);Detonate();}
        private void Update()
        {
            if(!exploded){if(Time.time>=detonateAt)Detonate();return;}
            if(kind==ThrowableKind.Smoke){var color=visual.color;color.a=.5f*Mathf.Clamp01((hideAt-Time.time)/1.5f);visual.color=color;}
            else if(kind==ThrowableKind.Fragmentation)FragPresentation();
            else{var color=ring.startColor;color.a=Mathf.Clamp01((hideAt-Time.time)/.3f);ring.startColor=ring.endColor=color;}
            if(Time.time>=hideAt)gameObject.SetActive(false);
        }
        private void Detonate()
        {
            if(exploded)return;exploded=true;Detonations++;body.simulated=false;body.linearVelocity=Vector2.zero;Vector2 center=transform.position;
            if(kind==ThrowableKind.Smoke){visual.sprite=smokeSprite;transform.localScale=Vector3.one*(smokeRadius*2/smokeSprite.bounds.size.x);visual.color=new Color(.6f,.65f,.63f,.5f);cloud.Activate(smokeRadius,smokeSeconds);hideAt=Time.time+smokeSeconds;return;}
            visual.enabled=false;hideAt=Time.time+.3f;float radius=kind==ThrowableKind.Fragmentation?blastRadius:flashRadius;
            var color=kind==ThrowableKind.Fragmentation?new Color(1,.6f,.15f,1):new Color(1,1,.8f,.45f);ring.startColor=ring.endColor=color;for(int i=0;i<17;i++){float angle=i*Mathf.PI/8;ring.SetPosition(i,center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius);}ring.enabled=true;
            var hp=owner!=null?owner.GetComponent<Health>():null;Affected=kind==ThrowableKind.Fragmentation?area.Damage(owner,hp!=null?hp.Team:CombatTeam.Neutral,center,blastRadius,fragmentationDamage,blastImpulse):area.Flash(owner,center,flashRadius,flashSeconds);
            if(kind==ThrowableKind.Fragmentation){ring.enabled=false;transform.rotation=Quaternion.identity;explodedAt=Time.time;hideAt=Time.time+burstSeconds;FragPresentation();if(explosionSound!=null)sound.PlayOneShot(explosionSound);var camera=Camera.main;if(camera!=null)camera.GetComponent<InsaneDreams.CameraSystem.WarehouseCameraFollow>()?.AddExplosionShake(center,shakeAmplitude,shakeSeconds,shakeRange);CombatNoise.Report(new CombatNoiseEvent(owner,hp!=null?hp.Team:CombatTeam.Neutral,CombatNoiseKind.Explosion,center,center,1));}
        }
        private void OnDisable(){if(cloud!=null)cloud.enabled=false;if(body!=null)body.simulated=false;if(ring!=null)ring.enabled=false;HideFrag();if(sound!=null)sound.Stop();}
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
