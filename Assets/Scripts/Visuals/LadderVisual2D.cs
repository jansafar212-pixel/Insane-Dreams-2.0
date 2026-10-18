using UnityEngine;
namespace InsaneDreams.Visuals
{
    [DisallowMultipleComponent]
    public sealed class LadderVisual2D : MonoBehaviour
    {
        [SerializeField] private float bottom, top = 5;
        [SerializeField, Min(.1f)] private float width = 1;
        [SerializeField, Min(1)] private int pixelsPerUnit = 32;
        [SerializeField] private SpriteRenderer body, bottomCap, topCap;
        public SpriteRenderer Body => body;
        public SpriteRenderer BottomCap => bottomCap;
        public SpriteRenderer TopCap => topCap;
        public float BottomWorld => Snap(transform.TransformPoint(0, bottom, 0).y);
        public float TopWorld => Snap(transform.TransformPoint(0, top, 0).y);
        private float Snap(float value) => Mathf.Round(value * pixelsPerUnit) / pixelsPerUnit;
        public void Configure(float bottomWorld, float topWorld, Sprite segments, Sprite foot, Sprite end)
        {
            bottom = transform.InverseTransformPoint(new Vector3(transform.position.x, bottomWorld, 0)).y;
            top = transform.InverseTransformPoint(new Vector3(transform.position.x, topWorld, 0)).y;
            body = Make(body, "Body", segments); bottomCap = Make(bottomCap, "BottomTermination", foot); topCap = Make(topCap, "TopTermination", end); Layout();
        }
        private SpriteRenderer Make(SpriteRenderer existing, string name, Sprite sprite)
        {
            if (existing == null) { var go = new GameObject(name); go.transform.SetParent(transform, false); existing = go.AddComponent<SpriteRenderer>(); }
            existing.sprite = sprite; existing.sortingOrder = 1; existing.drawMode = SpriteDrawMode.Tiled; return existing;
        }
        private void Start() => Layout();
        // Renderer tiling setters send messages that Unity forbids during validation.
        // Author through Configure or the context-menu refresh; runtime refresh runs in Start.
        private void OnValidate() { pixelsPerUnit = Mathf.Max(1, pixelsPerUnit); width = Mathf.Max(.1f, width); }
        [ContextMenu("Refresh visual endpoints")]
        public void Layout()
        {
            if (body == null || bottomCap == null || topCap == null || top <= bottom) return;
            float low = transform.InverseTransformPoint(new Vector3(transform.position.x, BottomWorld, 0)).y;
            float high = transform.InverseTransformPoint(new Vector3(transform.position.x, TopWorld, 0)).y;
            float cap = Mathf.Min(4f / pixelsPerUnit, (high - low) / 2);
            Place(bottomCap, low + cap / 2, cap); Place(topCap, high - cap / 2, cap);
            Place(body, (low + high) / 2, high - low - cap * 2);
        }
        private void Place(SpriteRenderer renderer, float y, float height)
        { renderer.transform.localPosition = new Vector3(0, y, 0); renderer.size = new Vector2(width, Mathf.Max(.001f, height)); }
    }
}
