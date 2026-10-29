using UnityEngine;

namespace InsaneDreams.Combat
{
    // Deliberately placed future-level hazard. Never installed by a level-scanning system.
    [RequireComponent(typeof(Health),typeof(SpriteRenderer))]
    public sealed class Landmine2D : MonoBehaviour
    {
        [SerializeField] private ThrowableProjectile2D explosion;
        [SerializeField,Min(0)] private float armingSeconds=.4f;
        [SerializeField,Min(.05f)] private float warningSeconds=.2f;
        private ThrowableProjectile2D burst;
        private Health health;
        private SpriteRenderer visual;
        private Collider2D[] contacts;
        private float armedAt,detonateAt;
        public bool Primed{get;private set;}
        public bool Spent{get;private set;}
        public int Detonations{get;private set;}
        public ThrowableProjectile2D Explosion=>burst;
        private void Awake()
        {
            health=GetComponent<Health>();visual=GetComponent<SpriteRenderer>();contacts=GetComponentsInChildren<Collider2D>();
            if(explosion!=null){burst=Instantiate(explosion);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(burst.gameObject,gameObject.scene);burst.gameObject.SetActive(false);}
        }
        private void OnEnable(){armedAt=Time.time+armingSeconds;health.Defeated+=Shot;}
        private void OnDisable(){health.Defeated-=Shot;if(burst!=null)burst.gameObject.SetActive(false);}
        private void Shot(DamageInfo hit)
        {
            if(hit.Kind==DamageKind.Ballistic||hit.Kind==DamageKind.Explosion)Prime();
        }
        private void OnTriggerEnter2D(Collider2D other)=>Pressure(other);
        private void OnTriggerStay2D(Collider2D other)=>Pressure(other);
        private void Pressure(Collider2D other)
        {
            if(Time.time<armedAt||Primed||Spent)return;
            var actor=other.GetComponentInParent<Health>();
            if(actor!=null&&actor!=health&&!actor.IsDefeated)Prime();
        }
        private void Prime()
        {
            if(Primed||Spent||burst==null)return;
            Primed=true;detonateAt=Time.time+warningSeconds;visual.color=new Color(1,.3f,.2f);
        }
        private void Update()
        {
            if(Primed&&!Spent&&Time.time>=detonateAt)
            {
                // Mark spent and remove contacts BEFORE damage dispatch. Nearby mines
                // arm for their own next update, so chains cannot recurse through damage.
                Spent=true;Detonations++;foreach(var c in contacts)c.enabled=false;visual.enabled=false;
                burst.Burst(gameObject,transform.position);
            }
            if(Spent&&burst!=null&&!burst.Busy)gameObject.SetActive(false);
        }
        private void OnDestroy(){if(burst!=null)Destroy(burst.gameObject);}
    }
}
