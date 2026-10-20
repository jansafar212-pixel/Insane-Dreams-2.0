using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using UnityEngine;
namespace InsaneDreams.Visuals
{
    public sealed class MovementAudioHooks : MonoBehaviour
    {
        [SerializeField] private AudioClip footstep, landing, ladderStep;
        [SerializeField, Min(.1f)] private float stepInterval = .35f;
        private AudioSource source;
        private Rigidbody2D body;
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private float nextStep;
        private bool grounded;
        private InsaneDreams.Combat.Health health;
        private PlayerPosture2D posture;
        [SerializeField, Min(.1f)] private float runningSpeed=5;
        [SerializeField, Range(.05f,1)] private float crouchNoiseMultiplier=.25f;
        private void Awake() { source = GetComponent<AudioSource>(); body = GetComponent<Rigidbody2D>(); motor = GetComponent<PlayerMotor2D>(); traversal = GetComponent<PlayerTraversal2D>();health=GetComponent<InsaneDreams.Combat.Health>();posture=GetComponent<PlayerPosture2D>(); }
        private void Update()
        {
            if (body == null || motor == null) return;
            if (!grounded && motor.IsGrounded && landing != null&&source!=null) source.PlayOneShot(landing, .4f);
            grounded = motor.IsGrounded;
            bool climbing = traversal != null && traversal.IsClimbing;
            AudioClip clip = climbing ? ladderStep : footstep;
            float speed=climbing?Mathf.Abs(body.linearVelocity.y):Mathf.Abs(body.linearVelocity.x);
            if (Time.time >= nextStep && (climbing ? speed > .3f : grounded && speed > .3f))
            {
                if(clip!=null&&source!=null)source.PlayOneShot(clip,.3f);nextStep=Time.time+stepInterval;
                if(health!=null&&health.Team==InsaneDreams.Combat.CombatTeam.Player&&!health.IsDefeated)
                {
                    float intensity=Mathf.Lerp(.35f,1,Mathf.Clamp01(speed/runningSpeed));if(posture!=null&&posture.IsCrouched)intensity*=crouchNoiseMultiplier;
                    InsaneDreams.Combat.CombatNoise.Report(new InsaneDreams.Combat.CombatNoiseEvent(gameObject,health.Team,InsaneDreams.Combat.CombatNoiseKind.Movement,body.position,body.position,intensity));
                }
            }
        }
        private void OnDisable() { grounded = false; nextStep = 0; }
    }
}
