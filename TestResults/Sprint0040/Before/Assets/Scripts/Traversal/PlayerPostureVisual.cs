using UnityEngine;

namespace InsaneDreams.Traversal
{
    [DefaultExecutionOrder(-25)]
    [RequireComponent(typeof(PlayerPosture2D), typeof(SpriteRenderer))]
    public sealed class PlayerPostureVisual : MonoBehaviour
    {
        private PlayerPosture2D posture;
        private SpriteRenderer source, pose;
        private void Awake()
        {
            posture = GetComponent<PlayerPosture2D>(); source = GetComponent<SpriteRenderer>();
            var go = new GameObject("PostureVisual"); go.transform.SetParent(transform, false);
            pose = go.AddComponent<SpriteRenderer>(); pose.sprite = source.sprite; pose.color = source.color;
            pose.drawMode = source.drawMode; pose.size = source.size; pose.flipX = source.flipX; pose.flipY = source.flipY;
            pose.sharedMaterial = source.sharedMaterial; pose.sortingLayerID = source.sortingLayerID; pose.sortingOrder = source.sortingOrder;
        }
        private void OnEnable() { source.enabled = false; pose.enabled = GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true) == null; }
        private void LateUpdate()
        {
            pose.transform.localScale = new Vector3(1, posture.HeightRatio, 1);
            pose.transform.localPosition = posture.LocalCenter;
        }
        private void OnDisable() { if (source != null) source.enabled = true; if (pose != null) pose.enabled = false; }
    }
}
