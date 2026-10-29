using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Enemies;
using UnityEngine;
namespace InsaneDreams.Combat
{
    // Extends the existing pose/recovery owner. Six reusable angular joints supply
    // secondary motion; they have no colliders and never push the gameplay motor.
    [RequireComponent(typeof(Health))]
    public sealed class HitReaction2D : MonoBehaviour
    {
        [SerializeField,Min(0),Tooltip("Legacy impulse thresholds; in living mode: heavy-response strength and accumulated balance threshold.")] private float stumbleImpulse=2.2f,knockdownImpulse=3.5f;
        [SerializeField,Range(0,1)] private float knockdownChance=.15f;
        [SerializeField,Min(.1f)] private float recoverySeconds=.65f,cooldown=3;
        [Header("Living hybrid response")]
        [SerializeField] private bool livingPhysics;
        [SerializeField,Range(.1f,2)] private float susceptibility=1;
        [SerializeField,Min(.1f)] private float balanceDecay=1.15f,heavyCooldown=.55f,recoveryGrace=1.2f;
        [SerializeField,Range(2,40)] private float maximumJointAngle=26;
        [SerializeField,Range(20,400)] private float impulseDegrees=115;
        [SerializeField,Min(1)] private float springReturn=18;
        [SerializeField,Min(.01f)] private float springTorque=.25f;
        [SerializeField,Min(0)] private float jointDamping=5;
        [SerializeField,Range(.5f,2)] private float springDamping=1.5f;
        [SerializeField,Range(100,600)] private float maximumAngularSpeed=320;
        [SerializeField,Range(4,30)] private float maximumTorsoAngle=12;
        [SerializeField,Range(.3f,2)] private float responseSeconds=.8f;
        [SerializeField,Range(0,1)] private float torsoSpill=.25f,heavyTorsoSpill=.55f;
        private Visuals.CutoutRig rig;
        private BodyRegionDamage regions;
        private Enemies.AuthoredLadderRoute route;
        private Traversal.PlayerTraversal2D traversal;
        private PlayerHealing healing;
        private PlayerCover2D cover;
        private Transform[] poseParts;
        private Rigidbody2D[] springs;
        private HingeJoint2D[] joints;
        private GameObject physicsRoot;
        private Rigidbody2D anchor;
        private bool jointsAwake;
        private readonly float[] pendingRegions=new float[6];
        private readonly Vector2[] regionPoints=new Vector2[6],regionDirections=new Vector2[6];
        private float pendingStrength,awakeUntil,rapidLoad,nextHeavy,immuneUntil;
        private Vector2 pendingDirection;
        public bool UsesLivingPhysics=>livingPhysics;
        public float Balance{get;private set;}
        public float LastIntensity{get;private set;}
        public int LastRegion{get;private set;}
        public int PhysicalReactions{get;private set;}
        public int HeavyReactions{get;private set;}
        public int Falls{get;private set;}
        public int JointCount=>springs!=null?springs.Length:0;
        public float RegionAngle(int region)=>springs!=null&&springs[region]!=null?Mathf.Clamp(Mathf.DeltaAngle(0,springs[region].rotation),-maximumJointAngle,maximumJointAngle):0;
        private Health health;private PlayerMotor2D motor;private PlayerActions player;private PlayerCombat playerCombat;private EnemyCombat enemy;private LarperBrain brain;
        private bool playerWasEnabled,combatWasEnabled,enemyWasEnabled,brainWasSuspended,locked;
        private float until,nextReaction,stumbleUntil;
        private InsaneDreams.Checkpoints.PlayerRecovery recovery;
        public bool KnockedDown=>locked&&Time.time<until;
        public float Lean=>KnockedDown?70*Mathf.Sin(Mathf.PI*Mathf.Clamp01((until-Time.time)/recoverySeconds)):
            !livingPhysics&&Time.time<stumbleUntil?12*Mathf.Clamp01((stumbleUntil-Time.time)/.3f):0;
        public float Direction{get;private set;}=1;
        private void Awake()
        {
            health=GetComponent<Health>();motor=GetComponent<PlayerMotor2D>();player=GetComponent<PlayerActions>();playerCombat=GetComponent<PlayerCombat>();enemy=GetComponent<EnemyCombat>();brain=GetComponent<LarperBrain>();recovery=GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>();
            rig=GetComponentInChildren<Visuals.CutoutRig>(true);regions=GetComponent<BodyRegionDamage>();route=GetComponent<Enemies.AuthoredLadderRoute>();traversal=GetComponent<Traversal.PlayerTraversal2D>();healing=GetComponent<PlayerHealing>();cover=GetComponent<PlayerCover2D>();
            if(livingPhysics&&rig!=null){poseParts=rig.BodyParts;CreateJoints();}
        }
        private void OnEnable(){health.Damaged+=Hurt;health.DamageResolved+=Resolved;if(recovery!=null)recovery.Recovered+=Recovered;}
        private void Recovered()=>CancelReaction();
        public void CancelReaction(bool restoreControls=true)
        {if(restoreControls&&!health.IsDefeated)Release();else locked=false;stumbleUntil=until=nextReaction=nextHeavy=immuneUntil=0;Balance=pendingStrength=rapidLoad=0;pendingDirection=Vector2.zero;for(int i=0;i<6;i++){pendingRegions[i]=0;regionPoints[i]=regionDirections[i]=Vector2.zero;}SleepJoints();}
        private void Hurt(DamageInfo hit)
        {
            if(livingPhysics)return;
            if(health.IsDefeated||!hit.Impact.HasValue||!hit.Impact.Value.IsValid||Time.time<nextReaction)return;
            var impact=hit.Impact.Value;if(impact.Impulse<stumbleImpulse)return;
            var traversal=GetComponent<Traversal.PlayerTraversal2D>();if(traversal!=null&&traversal.IsClimbing)return;
            Direction=Mathf.Sign(impact.Direction.x);stumbleUntil=Time.time+.3f;nextReaction=Time.time+cooldown;
            if(impact.Impulse<knockdownImpulse||motor==null||!motor.IsGrounded||Random.value>=knockdownChance)return;
            locked=true;until=Time.time+recoverySeconds;motor.SetMoveInput(0);
            if(player!=null){playerWasEnabled=player.enabled;player.SuspendInput();player.enabled=false;}
            if(playerCombat!=null){combatWasEnabled=playerCombat.enabled;playerCombat.enabled=false;}
            if(enemy!=null){enemyWasEnabled=enemy.enabled;enemy.enabled=false;}
            if(brain!=null){brainWasSuspended=brain.DecisionsSuspended;brain.DecisionsSuspended=true;brain.ClearPursuit();}
        }
        private void CreateJoints()
        {
            physicsRoot=new GameObject(name+"_LivingImpactJoints");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(physicsRoot,gameObject.scene);
            anchor=physicsRoot.AddComponent<Rigidbody2D>();anchor.bodyType=RigidbodyType2D.Kinematic;anchor.simulated=false;
            springs=new Rigidbody2D[6];joints=new HingeJoint2D[6];
            for(int i=0;i<6;i++)
            {
                var g=new GameObject("ImpactJoint"+i);g.transform.SetParent(physicsRoot.transform,false);
                var b=springs[i]=g.AddComponent<Rigidbody2D>();b.gravityScale=0;b.mass=.1f;b.inertia=.015f;b.angularDamping=jointDamping;b.constraints=RigidbodyConstraints2D.FreezePosition;b.simulated=false;
                var j=joints[i]=g.AddComponent<HingeJoint2D>();j.connectedBody=anchor;j.autoConfigureConnectedAnchor=false;j.anchor=j.connectedAnchor=Vector2.zero;j.enableCollision=false;j.useLimits=false;j.useMotor=false;
            }
        }
        private void Resolved(DamageOutcome outcome)
        {
            var hit=outcome.Hit;
            if(!livingPhysics||health.IsDefeated||springs==null||!hit.Impact.HasValue||!hit.Impact.Value.IsValid)return;
            var impact=hit.Impact.Value;float strength=impact.ReactionStrength;
            if(float.IsNaN(strength)||float.IsInfinity(strength)||strength<=0)return;
            float armorFactor=Mathf.Lerp(.65f,1,Mathf.Clamp01(outcome.HealthLost/Mathf.Max(.01f,hit.Amount)));
            strength=Mathf.Clamp(strength*armorFactor,0,4);
            LastRegion=regions!=null?regions.RegionAt(impact.Point):0;
            pendingRegions[LastRegion]+=strength;pendingStrength+=strength;pendingDirection+=impact.Direction*strength;regionPoints[LastRegion]+=impact.Point*strength;regionDirections[LastRegion]+=impact.Direction*strength;
        }
        private void FixedUpdate()
        {
            if(!livingPhysics||springs==null||anchor==null)return;
            Balance=Mathf.MoveTowards(Balance,0,Time.fixedDeltaTime*balanceDecay);
            rapidLoad=Mathf.MoveTowards(rapidLoad,0,Time.fixedDeltaTime*3);
            if(health.IsDefeated){pendingStrength=0;SleepJoints();return;}
            if(pendingStrength>0)
            {
                // One physics response for the batch, including all shotgun pellets.
                float strength=Mathf.Min(3.2f,pendingStrength)*susceptibility;
                float diminishing=1/(1+rapidLoad*.25f);rapidLoad=Mathf.Min(4,rapidLoad+1);
                Vector2 direction=pendingDirection.sqrMagnitude>.0001f?pendingDirection.normalized:Vector2.right;
                LastIntensity=strength;PhysicalReactions++;Direction=Mathf.Abs(direction.x)>.05f?Mathf.Sign(direction.x):Mathf.Sign(direction.y);
                Vector2 hip=rig.transform.TransformPoint((rig.leftLeg.localPosition+rig.rightLeg.localPosition)*.5f);
                bool heavy=!locked&&Time.time>=immuneUntil&&strength>=stumbleImpulse&&Time.time>=nextHeavy;
                if(heavy){HeavyReactions++;nextHeavy=Time.time+heavyCooldown;}
                bool waking=!jointsAwake;anchor.simulated=true;jointsAwake=true;awakeUntil=Time.time+responseSeconds;
                for(int i=0;i<6;i++)
                {
                    var b=springs[i];b.simulated=true;
                    if(waking){b.rotation=0;b.angularVelocity=0;b.linearVelocity=Vector2.zero;b.WakeUp();}
                    float share=pendingRegions[i]/pendingStrength;
                    float turn=i<2?-Direction:Direction;
                    if(pendingRegions[i]>0)
                    {
                        Vector2 lever=regionPoints[i]/pendingRegions[i]-(i==0?hip:(Vector2)poseParts[i].position);
                        Vector2 incoming=regionDirections[i].normalized;
                        float torque=lever.x*incoming.y-lever.y*incoming.x;
                        if(Mathf.Abs(torque)>.01f)turn=Mathf.Sign(torque);
                    }
                    float response=Mathf.Lerp(i==0?(heavy?heavyTorsoSpill:torsoSpill):heavy?.2f:.08f,1,share);
                    float kick=(player!=null?-Direction:turn)*impulseDegrees*strength*diminishing*response*(player!=null?.3f:1);
                    b.AddTorque(Mathf.Clamp(kick,-maximumAngularSpeed,maximumAngularSpeed)*Mathf.Deg2Rad*b.inertia,ForceMode2D.Impulse);pendingRegions[i]=0;regionPoints[i]=regionDirections[i]=Vector2.zero;
                }
                pendingStrength=0;pendingDirection=Vector2.zero;
                if(!locked&&Time.time>=immuneUntil)
                {
                    Balance=Mathf.Min(knockdownImpulse*1.5f,Balance+strength*diminishing);
                    // Player mobility is never disabled by this living response.
                    if(player==null&&Time.time>=nextReaction&&Balance>=knockdownImpulse&&motor!=null&&motor.IsGrounded&&(route==null||!route.IsRunning))
                    {
                        nextReaction=Time.time+cooldown;Balance=0;
                        if(Random.value<knockdownChance){locked=true;until=Time.time+recoverySeconds;Falls++;motor.SetMoveInput(0);if(brain!=null){brainWasSuspended=brain.DecisionsSuspended;brain.DecisionsSuspended=true;brain.ClearPursuit();}}
                    }
                }
            }
            if(Time.time>=awakeUntil){SleepJoints();return;}
            for(int i=0;i<6;i++)
            {
                var b=springs[i];float angle=Mathf.DeltaAngle(0,b.rotation);
                if(Mathf.Abs(angle)>maximumJointAngle){b.rotation=Mathf.Clamp(angle,-maximumJointAngle,maximumJointAngle);b.angularVelocity=0;}
                b.angularVelocity=Mathf.Clamp(b.angularVelocity,-maximumAngularSpeed,maximumAngularSpeed);
                // World-angle spring avoids HingeJoint motor sign/reference-angle
                // changes when a pooled response is reset between opposite hits.
                float acceleration=-angle*springReturn*springReturn-b.angularVelocity*springReturn*springDamping;
                b.AddTorque(Mathf.Clamp(acceleration*Mathf.Deg2Rad*b.inertia,-springTorque,springTorque),ForceMode2D.Force);
            }
        }
        private void SleepJoints()
        {
            if(springs==null||!jointsAwake)return;
            // Scene unload can destroy the sibling physics root before this actor.
            for(int i=0;i<6;i++){var b=springs[i];if(b==null)continue;b.angularVelocity=0;b.rotation=0;b.Sleep();b.simulated=false;}
            if(anchor!=null)anchor.simulated=false;jointsAwake=false;
        }
        public void ApplyPose(float facing,bool reloading)
        {
            if(!livingPhysics||springs==null||rig==null||health.IsDefeated)return;
            if(!jointsAwake&&!KnockedDown)return;
            bool constrained=reloading||healing!=null&&healing.IsHealing||traversal!=null&&(traversal.IsClimbing||traversal.ParachuteFlightActive)||cover!=null&&cover.IsCovered;
            Vector3 oldHand=rig.rightArm.TransformPoint(Vector3.down*rig.skin.arm.bounds.size.y);Quaternion barrel=rig.weaponMount.rotation;bool flip=rig.weapon.flipY;
            Vector3 hip=(rig.leftLeg.localPosition+rig.rightLeg.localPosition)*.5f;
            float torsoAngle=Mathf.Clamp(RegionAngle(0)*facing,-maximumTorsoAngle,maximumTorsoAngle)*(player!=null?.65f:1)*(constrained?(player!=null?.2f:.45f):1);
            Quaternion upper=Quaternion.Euler(0,0,torsoAngle);
            rig.torso.localPosition=hip+upper*(rig.torso.localPosition-hip);
            for(int i=0;i<6;i++)
            {
                if(constrained&&i>=2)continue;
                if(i==1||i==2||i==3)poseParts[i].localPosition=hip+upper*(poseParts[i].localPosition-hip);
                float angle=i==0?torsoAngle:RegionAngle(i)*facing*(player!=null?.65f:1)+(i<4?torsoAngle:0);
                poseParts[i].localRotation*=Quaternion.Euler(0,0,angle);
            }
            if(!constrained)
            {
                if(rig.Segmented!=null)rig.Segmented.FollowImpactGrip();
                else rig.weaponMount.position+=rig.rightArm.TransformPoint(Vector3.down*rig.skin.arm.bounds.size.y)-oldHand;
                rig.weaponMount.rotation=barrel;
                rig.weapon.flipY=flip;
            }
            if(KnockedDown)
            {
                float foot=Mathf.Min(rig.leftLeg.GetComponent<SpriteRenderer>().bounds.min.y,rig.rightLeg.GetComponent<SpriteRenderer>().bounds.min.y);
                if(rig.Segmented!=null)foot=Mathf.Min(rig.Segmented.leftFoot.GetComponent<SpriteRenderer>().bounds.min.y,rig.Segmented.rightFoot.GetComponent<SpriteRenderer>().bounds.min.y);
                var box=GetComponent<Collider2D>();if(box!=null)rig.transform.position+=Vector3.up*Mathf.Clamp(box.bounds.min.y-foot,-.65f,.2f);
            }
        }
        private void Update(){if(locked&&Time.time>=until){Release();immuneUntil=Time.time+recoveryGrace;Balance=0;}}
        private void Release()
        {if(!locked)return;locked=false;if(!livingPhysics){if(player!=null)player.enabled=playerWasEnabled;if(playerCombat!=null)playerCombat.enabled=combatWasEnabled;if(enemy!=null)enemy.enabled=enemyWasEnabled;}if(brain!=null)brain.DecisionsSuspended=brainWasSuspended;}
        private void OnDisable(){health.Damaged-=Hurt;health.DamageResolved-=Resolved;if(recovery!=null)recovery.Recovered-=Recovered;CancelReaction(!health.IsDefeated);}
        private void OnDestroy(){if(physicsRoot!=null)Destroy(physicsRoot);}
    }
}
