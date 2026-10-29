using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    [DefaultExecutionOrder(250)]
    public sealed class OfficerShotTelegraph : MonoBehaviour
    {
        [SerializeField,Min(.2f)] private float windupSeconds=1;
        [SerializeField,Range(.05f,.5f)] private float commitSeconds=.2f;
        [SerializeField,Min(.1f)] private float shotCooldown=2.4f, supportedExtraCooldown=.8f;
        [SerializeField,Min(0)] private float retreatDistance=6;
        [SerializeField] private Vector2 retreatCooldown=new Vector2(2.5f,4);
        [SerializeField] private Health[] nearbyAllies=System.Array.Empty<Health>();
        [SerializeField,Min(1)] private float allyRange=14;
        [SerializeField] private LayerMask geometry=1;
        [SerializeField,Min(.02f)] private float beamWidth=.055f;
        [Header("Optional authored defensive position")]
        [SerializeField] private Transform bunkerPeek;
        [SerializeField] private Collider2D bunkerCover;
        [SerializeField,Min(1)] private float bunkerReach=10;
        [SerializeField,Range(0,1)] private float bunkerRetreatChance=.8f;
        [SerializeField,Range(.08f,1)] private float bunkerHopRunUp=.65f;
        public Transform BunkerPeek=>bunkerPeek;
        public Collider2D BunkerCover=>bunkerCover;
        public bool NearBunker=>bunkerPeek!=null&&bunkerCover!=null&&bunkerCover.isActiveAndEnabled&&Vector2.SqrMagnitude(transform.position-bunkerPeek.position)<=bunkerReach*bunkerReach;
        public bool RetreatToBunkerAfterShot=>NearBunker&&Random.value<bunkerRetreatChance;
        public float CoverStandOff(Collider2D cover)=>NearBunker&&cover==bunkerCover?bunkerHopRunUp:.08f;
        [Header("Concrete pillbox (optional scene anchors)")]
        [SerializeField] private Transform bunkerHide;
        [SerializeField] private Transform bunkerExit;
        public Transform BunkerExit=>bunkerExit;
        [SerializeField] private Vector2 embrasureHeight=new Vector2(2.5f,3.4f);
        public bool UsesEmbrasure=>bunkerHide!=null&&bunkerPeek!=null&&bunkerCover!=null;
        public Vector2 HidePosition=>bunkerHide.position;
        public bool AllowsBunkerRay(Vector2 origin,Vector2 aim)
        {
            if(!UsesEmbrasure)return true;
            // The rear door is a real exit; shots there still use normal solid geometry.
            if(aim.x>=-.01f)return bunkerExit!=null&&aim.x>.01f&&origin.x>=bunkerHide.position.x;
            // Test both faces, including a muzzle protruding through the slit.
            // Extrapolation checks the same barrel line rather than allowing a
            // muzzle outside concrete to bypass the authored aperture.
            var b=bunkerCover.bounds;
            float a=origin.y+(b.min.x-origin.x)*aim.y/aim.x;
            float z=origin.y+(b.max.x-origin.x)*aim.y/aim.x;
            return Mathf.Min(a,z)>embrasureHeight.x+.02f&&Mathf.Max(a,z)<embrasureHeight.y-.02f;
        }
        private float startedAt,lastRequested,nextAimAt,errorAngle,range;
        private bool winding;
        private Vector2 direction=Vector2.right;
        private AttackExecutor attacks;private EnemyCombat combat;private Health owner;private LineRenderer laser;private Material ownedMaterial;
        private readonly RaycastHit2D[] laserHits=new RaycastHit2D[8];
        public bool Winding=>winding;
        public bool Locked=>winding&&Time.time>=startedAt+Mathf.Max(0,windupSeconds-commitSeconds);
        public Vector2 Direction=>direction;
        public bool LaserVisible=>laser!=null&&laser.enabled;
        public float WarningStartedAt=>startedAt;
        public Vector2 LaserEnd=>laser!=null?(Vector2)laser.GetPosition(1):Vector2.zero;
        public float RetreatDistance=>retreatDistance;
        [Header("Finite ballistic vest and local fear reactions")]
        [SerializeField,Min(0)] private float bodyProtection=50;
        [SerializeField,Range(0,1)] private float bodyAbsorption=.65f;
        [SerializeField,Min(.5f)] private float hitRetreatCooldown=2.5f;
        [SerializeField,Range(.2f,1.2f)] private float closeHideSeconds=.45f;
        private float damageRetreatUntil,nextDamageRetreat;
        public float CloseHideSeconds=>closeHideSeconds;
        public void ResetProtection()
        {
            GetComponent<BodyArmor>()?.Restore(bodyProtection,Mathf.Max(1,bodyProtection),bodyAbsorption);
            damageRetreatUntil=nextDamageRetreat=0;
        }
        private void OnEnable(){ResetProtection();owner.Damaged+=Damaged;}
        private void Damaged(DamageInfo hit)
        {
            if(!UsesEmbrasure||owner.IsDefeated||Time.time<nextDamageRetreat)return;
            nextDamageRetreat=Time.time+hitRetreatCooldown;damageRetreatUntil=Time.time+.5f;Cancel();
        }
        public bool TakeDamageRetreat()
        {bool requested=Time.time<damageRetreatUntil;damageRetreatUntil=0;return requested;}
        public float NextRetreatDelay=>Random.Range(Mathf.Max(.5f,retreatCooldown.x),Mathf.Max(retreatCooldown.x,retreatCooldown.y));
        public bool AlliesAlive{get{foreach(var ally in nearbyAllies)if(ally!=null&&ally.isActiveAndEnabled&&!ally.IsDefeated&&((Vector2)(ally.transform.position-transform.position)).sqrMagnitude<=allyRange*allyRange)return true;return false;}}
        private void Awake()
        {
            attacks=GetComponent<AttackExecutor>();combat=GetComponent<EnemyCombat>();owner=GetComponent<Health>();
            var g=new GameObject("OfficerRedLaser");g.transform.SetParent(transform,false);laser=g.AddComponent<LineRenderer>();laser.useWorldSpace=true;laser.positionCount=2;laser.startWidth=laser.endWidth=beamWidth;laser.sortingOrder=28;laser.startColor=laser.endColor=new Color(1,.07f,.04f,1);laser.enabled=false;
            laser.sharedMaterial=attacks!=null?attacks.AimMaterial:null;if(laser.sharedMaterial==null)laser.sharedMaterial=ownedMaterial=new Material(Shader.Find("Sprites/Default"));
        }
        public bool AllowShot(Vector2 aim,float spread,float distance,out Vector2 shot)
        {
            shot=direction;if(Time.time<nextAimAt)return false;lastRequested=Time.time;range=distance;
            if(!winding){winding=true;startedAt=Time.time;errorAngle=Random.Range(-spread,spread);direction=Quaternion.Euler(0,0,errorAngle)*aim.normalized;}
            if(!Locked)direction=Quaternion.Euler(0,0,errorAngle)*aim.normalized;
            shot=direction;return Time.time-startedAt>=windupSeconds;
        }
        public void ShotFired(){Cancel();nextAimAt=Time.time+shotCooldown+(AlliesAlive?supportedExtraCooldown:0);combat.RepositionAfterOfficerShot();}
        public void Cancel(){winding=false;if(laser!=null)laser.enabled=false;}
        private void LateUpdate()
        {
            if(!winding)return;if(owner.IsDefeated||combat==null||!combat.isActiveAndEnabled||!combat.MayInitiateFire||combat.Weapon.Cycle.IsReloading||Time.time-lastRequested>Mathf.Max(.15f,2*Time.deltaTime)){Cancel();return;}
            Vector2 origin=attacks.Origin;if(!AllowsBunkerRay(origin,direction)){Cancel();return;}float beamRange=ScreenRange(combat.GameplayCamera,origin,range);
            // Navigation triggers are not cover. Match real bullet/sight queries,
            // including the roof-access ladder beside the enlarged pillbox.
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(geometry);
            int count=Physics2D.Raycast(origin,direction,filter,laserHits,beamRange);
            Vector2 end=count>0?laserHits[0].point:origin+direction*beamRange;
            laser.enabled=true;laser.SetPosition(0,new Vector3(origin.x,origin.y,-.1f));laser.SetPosition(1,new Vector3(end.x,end.y,-.1f));
        }
        // Covers the whole viewport even on ultrawide phones or during camera zoom.
        // No viewport arrays, scene scans, or additional physics queries.
        public static float ScreenRange(Camera camera,Vector2 origin,float weaponRange)
        {
            if(camera==null)return weaponRange;
            float depth=camera.transform.InverseTransformPoint(new Vector3(origin.x,origin.y,0)).z;
            Vector2 low=camera.ViewportToWorldPoint(new Vector3(0,0,depth)),high=camera.ViewportToWorldPoint(new Vector3(1,1,depth));
            return Mathf.Max(weaponRange,Vector2.Distance(origin,(low+high)*.5f)+Vector2.Distance(low,high)+.5f);
        }
        private void OnDisable(){owner.Damaged-=Damaged;damageRetreatUntil=0;Cancel();nextAimAt=0;}
        private void OnDestroy(){if(ownedMaterial!=null)Destroy(ownedMaterial);}
    }
}
