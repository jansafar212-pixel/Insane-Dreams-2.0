using System;
using UnityEngine;

namespace InsaneDreams.Data
{
    public abstract class DefinitionAsset : ScriptableObject
    {
        [SerializeField] private string stableId;
        public string StableId => Guid.TryParse(stableId, out var id) ? id.ToString("N") : stableId;
        public bool HasValidIdentity => Guid.TryParse(stableId, out _);
        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stableId)) stableId = Guid.NewGuid().ToString("N");
        }
    }
}
