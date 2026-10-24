using System.Collections;
using InsaneDreams.Checkpoints;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using UnityEngine;
namespace InsaneDreams.Visuals
{
    [RequireComponent(typeof(Health),typeof(CharacterVisualDriver),typeof(PlayerRecovery))]
    public sealed class PlayerDeathRagdoll : MonoBehaviour
    {
        [SerializeField] private CartoonRagdoll prefab;
        [SerializeField,Min(.1f)] private float automaticRecoverySeconds=1.2f;
        private Health health;private CharacterVisualDriver visual;private PlayerRecovery recovery;
        private PlayerActions actions;private PlayerCombat combat;private Rigidbody2D body;
        private bool recovering,wasSimulated,actionsEnabled,combatEnabled;
        private Coroutine automaticRecovery;
        public CartoonRagdoll Corpse{get;private set;}
        public int SpawnCount{get;private set;}
        private void Awake(){health=GetComponent<Health>();visual=GetComponent<CharacterVisualDriver>();recovery=GetComponent<PlayerRecovery>();actions=GetComponent<PlayerActions>();combat=GetComponent<PlayerCombat>();body=GetComponent<Rigidbody2D>();}
        private void OnEnable(){health.Damaged+=Damaged;recovery.Recovered+=ReturnToLife;}
        private void OnDisable(){health.Damaged-=Damaged;recovery.Recovered-=ReturnToLife;ReturnToLife();}
        private void Damaged(DamageInfo damage)
        {
            if(!health.IsDefeated||Corpse!=null||prefab==null||visual.Rig==null||!visual.Rig.IsBound)return;
            // Damage notification precedes all defeat/respawn observers: copy the actual final pose.
            Corpse=Instantiate(prefab,transform.position,Quaternion.identity);Corpse.name="Player_CartoonRagdoll";
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(Corpse.gameObject,gameObject.scene);
            Corpse.Activate(visual.Rig,body.linearVelocity);if(damage.Impact.HasValue)Corpse.ReceiveDefeatImpact(damage.Impact.Value);
            SpawnCount++;visual.Rig.gameObject.SetActive(false);
        }
        public bool BeginAutomaticRecovery()
        {
            if(Corpse==null)return false;if(recovering)return true;
            recovering=true;wasSimulated=body.simulated;actionsEnabled=actions.enabled;combatEnabled=combat.enabled;
            actions.SuspendInput();actions.enabled=false;combat.Cancel();combat.enabled=false;
            GetComponent<Traversal.PlayerTraversal2D>()?.ResetTraversal();GetComponent<Prototype.PlayerMotor2D>()?.SetMoveInput(0);
            GetComponent<HitReaction2D>()?.CancelReaction();body.linearVelocity=Vector2.zero;body.simulated=false;
            automaticRecovery=StartCoroutine(Recover());return true;
        }
        private IEnumerator Recover(){yield return new WaitForSeconds(automaticRecoverySeconds);automaticRecovery=null;if(health.IsDefeated)recovery.Respawn();}
        public void ReturnToLife()
        {
            if(automaticRecovery!=null){StopCoroutine(automaticRecovery);automaticRecovery=null;}
            if(Corpse!=null)Corpse.Retire();Corpse=null;
            if(visual!=null&&visual.Rig!=null)visual.Rig.gameObject.SetActive(true);
            if(recovering){body.simulated=wasSimulated;actions.ResetActions();actions.enabled=actionsEnabled;combat.enabled=combatEnabled;recovering=false;}
        }
    }
}
