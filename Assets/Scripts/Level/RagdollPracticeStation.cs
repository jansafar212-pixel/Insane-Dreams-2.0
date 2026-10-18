using InsaneDreams.Enemies;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace InsaneDreams.Level
{
    // Opt-in practice configuration. No automatic spawning or changes to the regular encounter.
    public sealed class RagdollPracticeStation : MonoBehaviour
    {
        [SerializeField] private GameObject larperPrefab;
        [SerializeField] private Transform groundTarget, dropTarget;
        [SerializeField] private UIDocument document;
        private GameObject current;
        private VisualElement panel;
        private void Start()
        {
            if (document == null || document.rootVisualElement == null) return;
            panel = new VisualElement { name = "ragdollPractice" };
            panel.style.position = Position.Absolute; panel.style.left = 18; panel.style.top = 260; panel.style.width = 250;
            var ground = new Button(SpawnGroundTarget) { text = "RAGDOLL BAY: SPAWN (F6)" };
            var drop = new Button(SpawnDropTarget) { text = "DROP TARGET ONTO PLATFORM" };
            ground.style.height = drop.style.height = 32;
            ground.style.backgroundColor = drop.style.backgroundColor = new Color(.14f,.19f,.24f,.96f);
            ground.style.color = drop.style.color = new Color(.95f,.83f,.5f);
            panel.Add(ground); panel.Add(drop); document.rootVisualElement.Add(panel);
        }
        private void Update() { if (Application.isFocused && Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame) SpawnGroundTarget(); }
        public void SpawnGroundTarget() => Spawn(groundTarget);
        public void SpawnDropTarget() => Spawn(dropTarget);
        private void Spawn(Transform point)
        {
            if (!Application.isPlaying || larperPrefab == null || point == null) return;
            if (current != null) { current.SetActive(false); Destroy(current); }
            current = Instantiate(larperPrefab, point.position, Quaternion.identity);
            current.name = "PracticeLarper";
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(current, gameObject.scene);
            // This target is intentionally passive; live regular Larpers retain their own AI.
            current.GetComponent<LarperBrain>().enabled = false;
            current.GetComponent<EnemyCombat>().enabled = false;
        }
        private void OnDestroy() { panel?.RemoveFromHierarchy(); }
    }
}
