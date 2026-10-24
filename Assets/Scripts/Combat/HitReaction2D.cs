using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Enemies;
using UnityEngine;
namespace InsaneDreams.Combat
{
    // Optional demo component; the existing motor and visual driver retain ownership.
    [RequireComponent(typeof(Health))]
    public sealed class HitReaction2D : MonoBehaviour
    {
        [SerializeField,Min(0)] private float stumbleImpulse=2.2f,knockdownImpulse=3.5f;
        [SerializeField,Range(0,1)] private float knockdownChance=.15f;
        [SerializeField,Min(.1f)] private float recoverySeconds=.65f,cooldown=3;
        private Health health;private PlayerMotor2D motor;private PlayerActions player;private PlayerCombat playerCombat;private EnemyCombat enemy;private LarperBrain brain;
        private bool playerWasEnabled,combatWasEnabled,enemyWasEnabled,brainWasSuspended,locked;
        private float until,nextReaction,stumbleUntil;
        private InsaneDreams.Checkpoints.PlayerRecovery recovery;
        public bool KnockedDown=>locked&&Time.time<until;
        public float Lean=>KnockedDown?70*Mathf.Sin(Mathf.PI*Mathf.Clamp01((until-Time.time)/recoverySeconds)):
            Time.time<stumbleUntil?12*Mathf.Clamp01((stumbleUntil-Time.time)/.3f):0;
        public float Direction{get;private set;}=1;
        private void Awake()
        {health=GetComponent<Health>();motor=GetComponent<PlayerMotor2D>();player=GetComponent<PlayerActions>();playerCombat=GetComponent<PlayerCombat>();enemy=GetComponent<EnemyCombat>();brain=GetComponent<LarperBrain>();recovery=GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>();}
        private void OnEnable(){health.Damaged+=Hurt;if(recovery!=null)recovery.Recovered+=Recovered;}
        private void Recovered()=>CancelReaction();
        public void CancelReaction(bool restoreControls=true){if(restoreControls)Release();else locked=false;stumbleUntil=until=nextReaction=0;}
        private void Hurt(DamageInfo hit)
        {
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
        private void Update(){if(locked&&Time.time>=until)Release();}
        private void Release()
        {if(!locked)return;locked=false;if(player!=null)player.enabled=playerWasEnabled;if(playerCombat!=null)playerCombat.enabled=combatWasEnabled;if(enemy!=null)enemy.enabled=enemyWasEnabled;if(brain!=null)brain.DecisionsSuspended=brainWasSuspended;}
        private void OnDisable(){health.Damaged-=Hurt;if(recovery!=null)recovery.Recovered-=Recovered;Release();stumbleUntil=nextReaction=0;}
    }
}
