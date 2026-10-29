using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    [RequireComponent(typeof(EnemyCombat))]
    [DefaultExecutionOrder(120)]
    public sealed class FireworksCombat : MonoBehaviour
    {
        [SerializeField] private FireworkProjectile projectilePrefab;
        [SerializeField,Min(.1f)] private float preparationSeconds=.85f,recoverySeconds=1.05f,preferredRange=7;
        [SerializeField,Range(0,1)] private float badMissChance=.2f;
        [SerializeField,Range(0,.2f)] private float heldMalfunctionChance=.08f;
        [SerializeField] private Vector2 heldFuseFraction=new Vector2(.35f,.8f);
        [SerializeField,Range(0,.3f)] private float recoveryVariation=.15f;
        private float RecoveryDelay=>recoverySeconds*Random.Range(1-recoveryVariation,1+recoveryVariation);
        private readonly FireworkProjectile[] pool=new FireworkProjectile[2];
        private FireworkProjectile pending;
        private Health committedTarget;
        private LarperBrain brain;private EnemyCombat combat;private Health health;
        private InsaneDreams.Visuals.CutoutRig rig;private LarperAwareness awareness;
        private float launchAt,nextAttack,preparedAt,failAt,throwPoseUntil,surprisedUntil;
        public bool Surprised=>Time.time<surprisedUntil||pending!=null&&failAt>0&&Time.time>=failAt-.18f;
        public bool HoldingVisual=>pending==null&&Time.time>=throwPoseUntil&&!health.IsDefeated;
        public float HandAngle=>Preparing?Mathf.Lerp(90,160,Mathf.SmoothStep(0,1,(Time.time-preparedAt)/preparationSeconds)):
            Time.time<throwPoseUntil?Mathf.Lerp(160,90,1-(throwPoseUntil-Time.time)/.3f):90;
        public int HeldMalfunctions{get;private set;}
        private Vector2 Hand=>rig!=null?(Vector2)rig.weaponMount.position:(Vector2)transform.position+Vector2.up*.35f;
        public bool Preparing=>pending!=null;
        public int Launches{get;private set;}
        public int PoolSize=>pool.Length;
        private void Awake()
        {
            brain=GetComponent<LarperBrain>();combat=GetComponent<EnemyCombat>();health=GetComponent<Health>();rig=GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>();awareness=GetComponent<LarperAwareness>();
            if(projectilePrefab!=null)for(int i=0;i<pool.Length;i++){pool[i]=Instantiate(projectilePrefab);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(pool[i].gameObject,gameObject.scene);pool[i].gameObject.SetActive(false);}
        }
        private void OnEnable(){nextAttack=Time.time+Random.Range(1,2);throwPoseUntil=surprisedUntil=0;health.Damaged+=Hurt;}
        private void Hurt(DamageInfo _){Cancel();nextAttack=Mathf.Max(nextAttack,Time.time+.75f);}
        private void Cancel(){if(pending!=null)pending.Cancel();pending=null;committedTarget=null;}
        private void OnDisable(){health.Damaged-=Hurt;Cancel();}
        private void OnDestroy(){foreach(var p in pool)if(p!=null)Destroy(p.gameObject);}
        private void LateUpdate(){if(pending!=null)pending.Hold(Hand);}
        private void Update(){if(pending!=null)AdvanceCommittedThrow();}
        private void AdvanceCommittedThrow()
        {
            // The destination/fuse were selected once in Prepare. Ordinary sight loss,
            // target jumps and skipped combat ticks cannot restart this animation.
            if(health.IsDefeated||!combat.isActiveAndEnabled||committedTarget==null||committedTarget.IsDefeated||!combat.MayInitiateFire)
            {Cancel();nextAttack=Time.time+RecoveryDelay;return;}
            if(failAt>0&&Time.time>=failAt)
            {
                var failed=pending;pending=null;committedTarget=null;HeldMalfunctions++;surprisedUntil=Time.time+1;throwPoseUntil=Time.time+.3f;nextAttack=Time.time+RecoveryDelay;
                awareness?.FireworkCue(true);failed.ExplodeHeld(Hand);return;
            }
            if(Time.time>=launchAt){var thrown=pending;pending=null;committedTarget=null;thrown.Launch(Hand);Launches++;throwPoseUntil=Time.time+.3f;nextAttack=Time.time+RecoveryDelay;}
        }
        public EngagementState Tick(Health target,Vector2 remembered,bool visible,bool cameraAllowed,bool ready)
        {
            if(target==null||target.IsDefeated){Cancel();brain.SetPursuit(0);return EngagementState.Searching;}
            if(pending!=null)
            {
                brain.SetPursuit(0);
                AdvanceCommittedThrow();
                return EngagementState.Reacting;
            }
            Vector2 delta=remembered-(Vector2)transform.position;
            if(!visible){brain.SetPursuit(0);return EngagementState.Searching;}
            if(delta.magnitude>preferredRange+3)
            {float sign=Mathf.Sign(delta.x);brain.SetPursuit(combat.CanAdvance(sign)?sign*.65f:0);return EngagementState.Approaching;}
            if(delta.magnitude<3.2f)
            {float sign=-Mathf.Sign(delta.x);brain.SetPursuit(combat.CanAdvance(sign)?sign*.6f:0);if(Time.time<nextAttack||!cameraAllowed||!ready)return EngagementState.Withdrawing;}
            else brain.SetPursuit(0);
            if(Time.time<nextAttack||!cameraAllowed||!ready)return EngagementState.Reacting;
            foreach(var p in pool)if(p!=null&&!p.Busy){pending=p;break;}if(pending==null)return EngagementState.Reacting;
            float spread=Random.value<badMissChance?3.2f:.65f;
            Vector2 destination=remembered+new Vector2(Random.Range(-spread,spread),-.5f);
            committedTarget=target;pending.Prepare(gameObject,destination,Color.HSVToRGB(Random.value,.8f,1),true);preparedAt=Time.time;launchAt=Time.time+preparationSeconds;
            float heldMin=Mathf.Clamp(Mathf.Min(heldFuseFraction.x,heldFuseFraction.y),.05f,.95f);
            float heldMax=Mathf.Clamp(Mathf.Max(heldFuseFraction.x,heldFuseFraction.y),heldMin,.95f);
            failAt=Random.value<heldMalfunctionChance?Time.time+preparationSeconds*Random.Range(heldMin,heldMax):0;pending.Hold(Hand);awareness?.FireworkCue(false);
            return EngagementState.Engaging;
        }
    }
}
