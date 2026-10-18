using System.Collections.Generic;
using InsaneDreams.Data;
using InsaneDreams.Enemies;
using UnityEditor;
using UnityEngine;

namespace InsaneDreams.EditorTools
{
    public static class DefinitionValidation
    {
        public static List<string> GetErrors()
        {
            var errors = new List<string>(); var ids = new HashSet<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:DefinitionAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid); var asset = AssetDatabase.LoadAssetAtPath<DefinitionAsset>(path);
                if (!asset.HasValidIdentity || !ids.Add(asset.StableId)) errors.Add(path + ": missing/duplicate identity");
                if (asset is EnemyArchetype enemy && enemy.ValidationError != null) errors.Add(path + ": " + enemy.ValidationError);
            }
            return errors;
        }
        [MenuItem("Insane Dreams/Validate Definitions")]
        public static void Validate() { var errors = GetErrors(); if (errors.Count == 0) Debug.Log("All definition identities are valid and unique."); else foreach (var e in errors) Debug.LogError(e); }
    }
    [CustomEditor(typeof(EnemySpawnPoint))]
    public sealed class EnemySpawnPointInspector : UnityEditor.Editor
    {
        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            var root = new UnityEngine.UIElements.VisualElement();
            UnityEditor.UIElements.InspectorElement.FillDefaultInspector(root, serializedObject, this);
            var warning = new UnityEngine.UIElements.HelpBox("", UnityEngine.UIElements.HelpBoxMessageType.Warning);
            root.Add(warning);
            System.Action refresh = () =>
            {
                string error = ((EnemySpawnPoint)target).ValidationError;
                warning.text = error ?? "";
                warning.style.display = error == null ? UnityEngine.UIElements.DisplayStyle.None : UnityEngine.UIElements.DisplayStyle.Flex;
            };
            root.RegisterCallback<UnityEditor.UIElements.SerializedPropertyChangeEvent>(_ => refresh()); refresh(); return root;
        }
    }
}
