using UnityEngine;

namespace InsaneDreams.Traversal
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class LadderVolume : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float alignmentSpeed = 8f;
        [SerializeField] private float topExitHorizontalOffset = 1.8f;
        private BoxCollider2D volume;
        public float CenterX => transform.position.x;
        public float Bottom => Volume.bounds.min.y;
        public float Top => Volume.bounds.max.y;
        public float AlignmentSpeed => alignmentSpeed;
        public float TopExitHorizontalOffset => topExitHorizontalOffset;
        private BoxCollider2D Volume => volume != null ? volume : volume = GetComponent<BoxCollider2D>();
        private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;
    }
}
