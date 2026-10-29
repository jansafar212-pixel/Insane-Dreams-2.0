using UnityEngine;
namespace InsaneDreams.Combat
{
    // Presentation pressure only: consumes real, clipped hostile shot segments.
    [RequireComponent(typeof(Health),typeof(AudioSource))]
    public sealed class PlayerSuppression : MonoBehaviour
    {
        [SerializeField,Range(.2f,3)] private float nearMissRadius=1.2f;
        [SerializeField,Range(.01f,.5f)] private float closeGain=.22f;
        [SerializeField,Range(.1f,2)] private float decayPerSecond=.45f;
        [SerializeField,Range(.03f,.3f)] private float gainInterval=.08f;
        [SerializeField,Range(0,.2f)] private float maximumVolume=.07f;
        [SerializeField] private AudioClip nearMissSound;
        private Health health;private Collider2D box;private AudioSource audioSource;
        private float pressure,nextGain,nextSound;
        private readonly RaycastHit2D[] occlusion=new RaycastHit2D[8];
        public float Pressure=>pressure;
        private void Awake(){health=GetComponent<Health>();box=GetComponent<Collider2D>();audioSource=GetComponent<AudioSource>();}
        private void OnEnable()=>CombatNoise.Reported+=Hear;
        private void OnDisable(){CombatNoise.Reported-=Hear;pressure=0;}
        private void Update(){pressure=health.IsDefeated?0:Mathf.MoveTowards(pressure,0,decayPerSecond*Time.deltaTime);}
        private static Vector2 Nearest(Vector2 point,Vector2 a,Vector2 segment)
        {return a+segment*Mathf.Clamp01(Vector2.Dot(point-a,segment)/segment.sqrMagnitude);}
        private void Hear(CombatNoiseEvent shot)
        {
            if(health.IsDefeated||box==null||shot.Kind!=CombatNoiseKind.Gunfire||shot.Team!=CombatTeam.Enemy||shot.HitTarget==health||Time.time<nextGain)return;
            Vector2 segment=shot.End-shot.Position;if(segment.sqrMagnitude<.001f)return;
            var bounds=box.bounds;Vector2 center=bounds.center,head=new Vector2(center.x,bounds.max.y-.1f);
            Vector2 near=Nearest(head,shot.Position,segment),bodyNear=Nearest(center,shot.Position,segment);
            if(Vector2.Distance(bodyNear,center)<Vector2.Distance(near,head))near=bodyNear;
            Vector2 edge=box.ClosestPoint(near);float distance=Vector2.Distance(near,edge);
            if(distance>nearMissRadius||CombatVisibility2D.IsBlocked(near,edge,1,occlusion,shot.Source!=null?shot.Source.transform:null,transform))return;
            nextGain=Time.time+gainInterval;pressure=Mathf.Clamp01(pressure+closeGain*Mathf.Lerp(.15f,1,1-distance/nearMissRadius));
            if(nearMissSound!=null&&Time.time>=nextSound){nextSound=Time.time+.3f;audioSource.PlayOneShot(nearMissSound,maximumVolume*Mathf.Lerp(.3f,1,pressure));}
        }
    }
}
