using System;
using InsaneDreams.Data;
using UnityEngine;

namespace InsaneDreams.Enemies
{
    [Flags]
    public enum MovementCapabilities { None = 0, Walk = 1, Climb = 2, Jump = 4, Drop = 8, Stationary = 16 }
    [CreateAssetMenu(menuName = "Insane Dreams/Enemy Archetype")]
    public sealed class EnemyArchetype : DefinitionAsset
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private MovementCapabilities capabilities = MovementCapabilities.Walk | MovementCapabilities.Jump;
        [SerializeField, Range(0, 1)] private float moveInput = .6f;
        [SerializeField, Min(1)] private float patrolHalfWidth = 7;
        [SerializeField, Min(.05f)] private float blockedPause = .5f;
        public GameObject Prefab => prefab;
        public MovementCapabilities Capabilities => capabilities;
        public float MoveInput => moveInput;
        public float PatrolHalfWidth => patrolHalfWidth;
        public float BlockedPause => blockedPause;
        public bool Allows(MovementCapabilities capability) => (capabilities & capability) == capability;
        public string ValidationError => !HasValidIdentity ? "A stable GUID is required." :
            prefab == null ? "An enemy prefab is required." :
            prefab.GetComponent<LarperBrain>() == null ? "Prefab requires LarperBrain." : null;
    }
}
