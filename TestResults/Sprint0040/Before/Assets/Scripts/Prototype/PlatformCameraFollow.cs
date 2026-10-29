using UnityEngine;

namespace InsaneDreams.Prototype
{
    [RequireComponent(typeof(Camera))]
    public sealed class PlatformCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, -10);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
        private Vector3 velocity;

        private void Start()
        {
            GetComponent<Camera>().orthographic = true;
            if (target != null) transform.position = target.position + offset;
        }

        private void LateUpdate()
        {
            if (target != null)
                transform.position = Vector3.SmoothDamp(transform.position,
                    target.position + offset, ref velocity, smoothTime);
        }
    }
}
