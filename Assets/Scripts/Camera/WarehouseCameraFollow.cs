using UnityEngine;

namespace InsaneDreams.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public sealed class WarehouseCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 levelMin = new Vector2(-28, -9);
        [SerializeField] private Vector2 levelMax = new Vector2(28, 20);
        [SerializeField] private Vector2 offset = new Vector2(0, 1.2f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.16f;
        [SerializeField, Min(1)] private float orthographicSize = 7.2f;
        [SerializeField, Min(0)] private int visualPixelsPerUnit = 32;
        private Camera view;
        private Vector3 velocity;
        private void Awake() => view = GetComponent<Camera>();
        private void Start() => SnapToTarget();
        public void Bind(Transform value) { target = value; velocity = Vector3.zero; if (value != null) SnapToTarget(); }
        public Transform Target => target;

        private Vector3 FramedPosition()
        {
            view.orthographic = true;
            Vector2 size = levelMax - levelMin;
            view.orthographicSize = Mathf.Min(orthographicSize, size.y * 0.5f, size.x / (2 * view.aspect));
            // Integer screen pixels per art pixel, without a low-resolution UI render target.
            // This camera already owns warehouse framing; do not add a competing size writer.
            if (visualPixelsPerUnit > 0 && view.pixelHeight > 0)
            {
                int zoom = Mathf.Max(1, Mathf.RoundToInt(view.pixelHeight / (2 * view.orthographicSize * visualPixelsPerUnit)));
                float maximum = Mathf.Min(size.y * .5f, size.x / (2 * view.aspect));
                zoom = Mathf.Max(zoom, Mathf.CeilToInt(view.pixelHeight / (2 * maximum * visualPixelsPerUnit)));
                view.orthographicSize = view.pixelHeight / (2f * visualPixelsPerUnit * zoom);
            }
            float halfY = view.orthographicSize;
            float halfX = halfY * view.aspect;
            Vector2 desired = (Vector2)target.position + offset;
            return new Vector3(Mathf.Clamp(desired.x, levelMin.x + halfX, levelMax.x - halfX),
                Mathf.Clamp(desired.y, levelMin.y + halfY, levelMax.y - halfY), -10);
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            if (view == null) view = GetComponent<Camera>();
            velocity = Vector3.zero;
            transform.position = PixelPosition(FramedPosition());
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = FramedPosition();
            Vector3 position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            float halfY = view.orthographicSize, halfX = halfY * view.aspect;
            position.x = Mathf.Clamp(position.x, levelMin.x + halfX, levelMax.x - halfX);
            position.y = Mathf.Clamp(position.y, levelMin.y + halfY, levelMax.y - halfY);
            transform.position = PixelPosition(position);
        }
        private Vector3 PixelPosition(Vector3 value)
        {
            if (visualPixelsPerUnit <= 0) return value;
            value.x = Mathf.Round(value.x * visualPixelsPerUnit) / visualPixelsPerUnit;
            value.y = Mathf.Round(value.y * visualPixelsPerUnit) / visualPixelsPerUnit;
            return value;
        }
    }
}
