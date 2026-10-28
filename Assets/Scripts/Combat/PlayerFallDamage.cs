using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using UnityEngine;
namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(Health),typeof(PlayerMotor2D),typeof(PlayerTraversal2D))]
    public sealed class PlayerFallDamage : MonoBehaviour
    {
        [SerializeField,Min(1)] private float safeDrop=8,damagePerUnit=8,minimumImpactSpeed=12;
        private Health health;private Rigidbody2D body;private Collider2D shape;private PlayerMotor2D motor;private PlayerTraversal2D traversal;
        private bool airborne;private float highestFeet,fallSpeed,normalGravity;
        public float LastDamage{get;private set;}
        public int Landings{get;private set;}
        private void Awake(){health=GetComponent<Health>();body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();motor=GetComponent<PlayerMotor2D>();traversal=GetComponent<PlayerTraversal2D>();normalGravity=body.gravityScale;ResetFall();}
        private void OnEnable(){var recovery=GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>();if(recovery!=null)recovery.Recovered+=ResetFall;var revive=GetComponent<PlayerRevive>();if(revive!=null)revive.Revived+=ResetFall;}
        public void ResetFall(){airborne=false;fallSpeed=0;highestFeet=shape!=null?shape.bounds.min.y:transform.position.y;}
        private void FixedUpdate()
        {
            if(health.IsDefeated||!body.simulated||traversal.IsClimbing){ResetFall();return;}
            if(!motor.IsGrounded||body.linearVelocity.y<-.1f)airborne=true;
            if(airborne){highestFeet=Mathf.Max(highestFeet,shape.bounds.min.y);fallSpeed=Mathf.Max(fallSpeed,-body.linearVelocity.y);}
            else highestFeet=shape.bounds.min.y;
        }
        private void OnCollisionEnter2D(Collision2D hit)=>Contact(hit);
        private void OnCollisionStay2D(Collision2D hit)=>Contact(hit);
        private void Contact(Collision2D hit)
        {
            if(!airborne||health.IsDefeated||traversal.IsClimbing)return;
            bool ground=false;for(int i=0;i<hit.contactCount;i++)if(hit.GetContact(i).normal.y>.65f){ground=true;break;}if(!ground)return;
            Landings++;LastDamage=0;
            // Absolute landing rule. Clear the entire flight before any calculation:
            // no stored velocity/height can become delayed damage after undeployment.
            if(traversal.IsParachuting){ResetFall();return;}
            float drop=highestFeet-shape.bounds.min.y,speed=fallSpeed;ResetFall();
            float gravity=-Physics2D.gravity.y*normalGravity;
            float ordinaryJump=gravity>0?(motor.JumpSpeed*motor.JumpSpeed+motor.AirJumpSpeed*motor.AirJumpSpeed)/(2*gravity)+.5f:safeDrop;
            if(speed<minimumImpactSpeed)return;
            LastDamage=Mathf.Max(0,drop-Mathf.Max(safeDrop,ordinaryJump))*damagePerUnit;
            if(LastDamage>0)health.ApplyDamage(new DamageInfo(LastDamage,CombatTeam.Neutral));
        }
        private void OnDisable(){var recovery=GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>();if(recovery!=null)recovery.Recovered-=ResetFall;var revive=GetComponent<PlayerRevive>();if(revive!=null)revive.Revived-=ResetFall;ResetFall();}
    }
}
