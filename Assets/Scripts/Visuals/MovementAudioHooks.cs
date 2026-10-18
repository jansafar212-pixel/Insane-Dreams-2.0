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
        private void Awake() { source = GetComponent<AudioSource>(); body = GetComponent<Rigidbody2D>(); motor = GetComponent<PlayerMotor2D>(); traversal = GetComponent<PlayerTraversal2D>(); }
        private void Update()
        {
            if (source == null || body == null || motor == null) return;
            if (!grounded && motor.IsGrounded && landing != null) source.PlayOneShot(landing, .4f);
            grounded = motor.IsGrounded;
            bool climbing = traversal != null && traversal.IsClimbing;
            AudioClip clip = climbing ? ladderStep : footstep;
            if (clip != null && Time.time >= nextStep && (climbing ? Mathf.Abs(body.linearVelocity.y) > .3f : grounded && Mathf.Abs(body.linearVelocity.x) > 1))
            { source.PlayOneShot(clip, .3f); nextStep = Time.time + stepInterval; }
        }
        private void OnDisable() { grounded = false; nextStep = 0; }
    }
}
