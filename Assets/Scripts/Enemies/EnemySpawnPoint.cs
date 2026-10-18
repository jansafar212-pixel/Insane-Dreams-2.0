using UnityEngine;

namespace InsaneDreams.Enemies
{
    public sealed class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private EnemyArchetype archetype;
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private Transform spawnedParent;
        [SerializeField] private InsaneDreams.Traversal.LadderVolume ladderRoute;
        [SerializeField] private float routeTargetHeight = 6.6f;
        [SerializeField] private LarperPersonalityProfile personality;
        private InsaneDreams.Combat.Health target;
        public void BindTarget(InsaneDreams.Combat.Health value) { target = value; if (Spawned != null) Spawned.GetComponent<EnemyCombat>()?.BindTarget(value); }
        public EnemyArchetype Archetype => archetype;
        public LarperBrain Spawned { get; private set; }
        public string ValidationError => archetype == null ? "Assign an enemy archetype." : archetype.ValidationError;
        private void Start() { if (spawnOnStart) Spawn(); }
        public LarperBrain Spawn()
        {
            if (Spawned != null) return Spawned;
            if (ValidationError != null) { Debug.LogWarning($"{name}: {ValidationError}", this); return null; }
            var actor = Instantiate(archetype.Prefab, transform.position, transform.rotation, spawnedParent);
            Spawned = actor.GetComponent<LarperBrain>(); Spawned.Configure(archetype);
            actor.GetComponent<EnemyCombat>()?.BindTarget(target);
            if(personality!=null)actor.GetComponent<LarperAwareness>()?.Configure(personality);
            if (ladderRoute != null) actor.GetComponent<AuthoredLadderRoute>()?.Configure(ladderRoute, routeTargetHeight);
            return Spawned;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = ValidationError == null ? Color.yellow : Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(.8f, 1.4f, .1f));
        }
    }
}
