using InsaneDreams.CameraSystem;
using InsaneDreams.Input;
using InsaneDreams.Traversal;
using UnityEngine;

namespace InsaneDreams.Checkpoints
{
    [RequireComponent(typeof(PlayerActions))]
    public sealed class PlayerRecovery : MonoBehaviour
    {
        [SerializeField] private Transform initialSpawn;
        [SerializeField] private WarehouseCameraFollow followCamera;
        [SerializeField] private float fallThreshold = -11f;
        private Rigidbody2D body;
        private PlayerActions actions;
        private PlayerTraversal2D traversal;
        private Vector2 checkpoint;
        private CheckpointMarker current;
        public Vector2 CurrentSpawn => checkpoint;
        public event System.Action Recovered;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            actions = GetComponent<PlayerActions>();
            traversal = GetComponent<PlayerTraversal2D>();
            checkpoint = initialSpawn != null ? (Vector2)initialSpawn.position : body.position;
        }
        private void Update() { if (body.position.y < fallThreshold) Respawn(); }
        public void Bind(Transform spawn, WarehouseCameraFollow camera)
        {
            if (current != null) current.SetLit(false);
            current = null; initialSpawn = spawn; followCamera = camera;
            checkpoint = spawn != null ? (Vector2)spawn.position : body.position;
        }

        public void Activate(CheckpointMarker marker, Vector2 point)
        {
            if (current != null) current.SetLit(false);
            current = marker;
            checkpoint = point;
            current.SetLit(true);
        }

        public void Respawn()
        {
            actions.ResetActions();
            traversal.ResetTraversal();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0;
            body.rotation = 0;
            body.position = checkpoint;
            transform.position = new Vector3(checkpoint.x, checkpoint.y, transform.position.z);
            Physics2D.SyncTransforms();
            GetComponent<PlayerPosture2D>()?.ResetPosture();
            if (followCamera != null) followCamera.SnapToTarget();
            Recovered?.Invoke();
        }
    }
}
