using UnityEngine;
namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(Collider2D),typeof(SpriteRenderer))]
    public sealed class EnvironmentalMine2D : MonoBehaviour
    {
        [SerializeField,Min(.1f)] private float damage=75,radius=2.5f,impulse=2,armingSeconds=.3f;
        private Collider2D trigger;private SpriteRenderer visual;private LineRenderer burst;private Material material;private float armedAt,hideAt;
        private readonly AreaCombat2D area=new AreaCombat2D();
        public bool Activated{get;private set;}
        public int Activations{get;private set;}
        private void Awake(){trigger=GetComponent<Collider2D>();visual=GetComponent<SpriteRenderer>();burst=new GameObject("MineBurst").AddComponent<LineRenderer>();burst.transform.SetParent(transform,false);burst.useWorldSpace=true;burst.positionCount=17;burst.startWidth=burst.endWidth=.07f;burst.sortingOrder=25;material=new Material(Shader.Find("Sprites/Default"));burst.sharedMaterial=material;burst.enabled=false;}
        private void OnEnable()=>armedAt=Time.time+armingSeconds;
        private void OnTriggerEnter2D(Collider2D other)=>Contact(other);
        private void OnTriggerStay2D(Collider2D other)=>Contact(other);
        private void Contact(Collider2D other)
        {
            if(Activated||Time.time<armedAt)return;var hp=other.GetComponentInParent<Health>();if(hp==null||hp.IsDefeated)return;
            Activated=true;Activations++;trigger.enabled=false;visual.color=new Color(.4f,.35f,.3f);hideAt=Time.time+.35f;
            Vector2 center=transform.position;for(int i=0;i<17;i++){float a=i*Mathf.PI/8;burst.SetPosition(i,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}burst.enabled=true;
            area.Damage(gameObject,CombatTeam.Enemy,center,radius,damage,impulse);
        }
        private void Update(){if(!burst.enabled)return;float alpha=Mathf.Clamp01((hideAt-Time.time)/.35f);burst.startColor=burst.endColor=new Color(1,.6f,.15f,alpha);if(alpha<=0)burst.enabled=false;}
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
