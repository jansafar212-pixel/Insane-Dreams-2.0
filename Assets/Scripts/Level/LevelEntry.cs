using InsaneDreams.CameraSystem;
using InsaneDreams.Checkpoints;
using InsaneDreams.Input;
using UnityEngine;

namespace InsaneDreams.Level
{
    [DefaultExecutionOrder(-100)]
    public sealed class LevelEntry : MonoBehaviour
    {
        [SerializeField] private PlayerActions player;
        [SerializeField] private Transform spawn;
        [SerializeField] private WarehouseCameraFollow followCamera;
        [SerializeField] private WarehouseTouchControls mobileControls;
        [SerializeField] private InsaneDreams.Enemies.EnemySpawnPoint[] encounters;
        private PlayerActions boundPlayer;
        public bool IsBound => boundPlayer != null;
        private void Start() => Bind();
        public bool Bind()
        {
            Unbind();
            if (player == null || spawn == null || followCamera == null || mobileControls == null)
            { Debug.LogWarning("LevelEntry requires player, spawn, camera and mobile controls.", this); return false; }
            boundPlayer = player;
            followCamera.Bind(player.transform);
            player.BindAimCamera(followCamera.GetComponent<Camera>());
            var recovery = player.GetComponent<PlayerRecovery>();
            if (recovery != null) { recovery.Bind(spawn, followCamera); recovery.Respawn(); }
            else { player.transform.position = spawn.position; player.ResetActions(); followCamera.SnapToTarget(); }
            mobileControls.Bind(player);
            if (encounters != null) foreach (var point in encounters) if (point != null) point.BindTarget(player.GetComponent<InsaneDreams.Combat.Health>());
            return true;
        }
        public void Configure(PlayerActions actor, Transform start, WarehouseCameraFollow camera, WarehouseTouchControls controls)
        {
            Unbind(); player = actor; spawn = start; followCamera = camera; mobileControls = controls;
        }
        public void Unbind()
        {
            if (boundPlayer == null) return;
            boundPlayer.ResetActions();
            boundPlayer.BindAimCamera(null);
            if (mobileControls != null) mobileControls.Bind(null);
            if (followCamera != null) followCamera.Bind(null);
            if (encounters != null) foreach (var point in encounters) if (point != null) point.BindTarget(null);
            boundPlayer = null;
        }
        private void OnDisable() => Unbind();
    }
}
