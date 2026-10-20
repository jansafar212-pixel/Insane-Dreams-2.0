using UnityEngine;

namespace InsaneDreams.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(150)]
    public sealed class WarehouseCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 levelMin = new Vector2(-28, -9);
        [SerializeField] private Vector2 levelMax = new Vector2(28, 20);
        [SerializeField] private Vector2 offset = new Vector2(0, 1.2f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.16f;
        [SerializeField, Min(1)] private float orthographicSize = 7.2f;
        [SerializeField, Min(0)] private int visualPixelsPerUnit = 32;
        [SerializeField, Range(1,1.25f)] private float androidCharacterScale=1.125f;
        [SerializeField] private bool previewAndroidFraming;
        private Camera view;
        private Vector3 velocity;
        private Vector3 followPosition;
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
            // Apply after the integer zoom calculation: otherwise rounding can
            // silently erase a modest Android framing adjustment.
            if(Application.platform==RuntimePlatform.Android||Application.isEditor&&previewAndroidFraming)
                view.orthographicSize/=Mathf.Clamp(androidCharacterScale,1,1.25f);
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
            followPosition=FramedPosition();transform.position = PixelPosition(followPosition);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = FramedPosition();
            // The target transform is Rigidbody2D's interpolated presentation position.
            // Never feed the pixel-rounded render output back into the smoothing state.
            Vector3 position = Vector3.SmoothDamp(followPosition, desired, ref velocity, smoothTime);
            float halfY = view.orthographicSize, halfX = halfY * view.aspect;
            float x=Mathf.Clamp(position.x,levelMin.x+halfX,levelMax.x-halfX),y=Mathf.Clamp(position.y,levelMin.y+halfY,levelMax.y-halfY);
            if(x!=position.x)velocity.x=0;if(y!=position.y)velocity.y=0;position.x=x;position.y=y;followPosition=position;
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
