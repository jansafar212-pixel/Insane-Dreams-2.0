using InsaneDreams.Visuals;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    // Damage follows the visible body envelope, excluding the sword. The movement
    // shape stays stable while limbs animate, avoiding animation-driven physics jitter.
    [DefaultExecutionOrder(175)]
    public sealed class SamuraiBodyHitbox : MonoBehaviour
    {
        [SerializeField] private CutoutRig rig;
        [SerializeField] private BoxCollider2D movementCollider,visualCollider;
        private SpriteRenderer[] parts;
        public BoxCollider2D Collider=>visualCollider;
        public Bounds VisibleBounds{get;private set;}
        public bool HasVisibleBody=>parts!=null&&parts.Length>0;
        public void Configure(CutoutRig value,BoxCollider2D movement,BoxCollider2D visual)
        {rig=value;movementCollider=movement;visualCollider=visual;Cache();RefreshBounds();}
        private void Awake(){Cache();RefreshBounds();}
        private void Cache()
        {
            if(rig==null||visualCollider==null)return;
            var roots=rig.BodyParts;var extra=rig.Segmented!=null?rig.Segmented.ExtraParts:System.Array.Empty<Transform>();
            parts=new SpriteRenderer[roots.Length+extra.Length];
            for(int i=0;i<roots.Length;i++)parts[i]=roots[i].GetComponent<SpriteRenderer>();
            for(int i=0;i<extra.Length;i++)parts[roots.Length+i]=extra[i].GetComponent<SpriteRenderer>();
        }
        private void LateUpdate()=>RefreshBounds();
        public void RefreshBounds()
        {
            if(parts==null||parts.Length==0||visualCollider==null)return;
            var bounds=parts[0].bounds;for(int i=1;i<parts.Length;i++)bounds.Encapsulate(parts[i].bounds);
            VisibleBounds=bounds;
            var localCenter=transform.InverseTransformPoint(bounds.center);
            var scale=transform.lossyScale;var size=new Vector2(bounds.size.x/Mathf.Abs(scale.x),bounds.size.y/Mathf.Abs(scale.y));
            if((visualCollider.offset-(Vector2)localCenter).sqrMagnitude>.00000001f)visualCollider.offset=localCenter;
            if((visualCollider.size-size).sqrMagnitude>.00000001f)visualCollider.size=size;
        }
        // Queries use the animated body, never the separate locomotion envelope.
        public static bool IsMovementShape(Collider2D collider)=>collider.TryGetComponent<SamuraiBodyHitbox>(out var body)&&body.isActiveAndEnabled&&body.movementCollider==collider;
    }
}
