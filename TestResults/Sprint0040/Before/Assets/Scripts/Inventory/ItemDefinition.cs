using InsaneDreams.Data;
using UnityEngine;

namespace InsaneDreams.Inventory
{
    public enum ItemCategory { Utility, Firearm, Melee }
    [CreateAssetMenu(menuName = "Insane Dreams/Item Definition")]
    public class ItemDefinition : DefinitionAsset
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemCategory category;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public virtual ItemCategory Category => category;
    }
}
