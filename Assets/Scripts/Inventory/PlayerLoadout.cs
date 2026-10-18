using InsaneDreams.Input;
using UnityEngine;

namespace InsaneDreams.Inventory
{
    public enum EquipmentSlot { Firearm1, Firearm2, Melee }
    public sealed class PlayerLoadout : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition initialFirearm1;
        [SerializeField] private WeaponDefinition initialFirearm2;
        [SerializeField] private WeaponDefinition initialMelee;
        [SerializeField, Min(0)] private int initialReserveAmmo = 36;
        [SerializeField] private RuntimeWeapon firearm1;
        [SerializeField] private RuntimeWeapon firearm2;
        [SerializeField] private RuntimeWeapon melee;
        [SerializeField] private EquipmentSlot selectedSlot;
        private PlayerActions actions;
        public event System.Action SelectionChanging;
        public EquipmentSlot SelectedSlot => selectedSlot;
        public RuntimeWeapon SelectedWeapon => Get(selectedSlot);
        public RuntimeWeapon Get(EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.Firearm1 => firearm1, EquipmentSlot.Firearm2 => firearm2,
            EquipmentSlot.Melee => melee, _ => null
        };

        private void Awake()
        {
            if (initialFirearm1 != null) Equip(EquipmentSlot.Firearm1, new RuntimeWeapon(initialFirearm1, initialReserveAmmo));
            if (initialFirearm2 != null) Equip(EquipmentSlot.Firearm2, new RuntimeWeapon(initialFirearm2, initialReserveAmmo));
            if (initialMelee != null) Equip(EquipmentSlot.Melee, new RuntimeWeapon(initialMelee));
        }
        private void OnEnable()
        {
            actions = GetComponent<PlayerActions>();
            if (actions != null) actions.WeaponSelectionRequested += Select;
        }
        private void OnDisable() { if (actions != null) actions.WeaponSelectionRequested -= Select; }
        public void Select(EquipmentSlot slot)
        {
            if (!System.Enum.IsDefined(typeof(EquipmentSlot), slot) || slot == selectedSlot) return;
            SelectionChanging?.Invoke(); selectedSlot = slot;
        }
        public bool Equip(EquipmentSlot slot, RuntimeWeapon weapon)
        {
            if (!System.Enum.IsDefined(typeof(EquipmentSlot), slot)) return false;
            if (weapon == null) { Unequip(slot); return true; }
            bool wantsMelee = slot == EquipmentSlot.Melee;
            if (weapon.Definition == null || wantsMelee != (weapon.Definition.WeaponCategory == WeaponCategory.Melee)) return false;
            if (firearm1 == weapon || firearm2 == weapon || melee == weapon) return Get(slot) == weapon;
            if (slot == selectedSlot) SelectionChanging?.Invoke();
            switch (slot)
            {
                case EquipmentSlot.Firearm1: firearm1 = weapon; break;
                case EquipmentSlot.Firearm2: firearm2 = weapon; break;
                case EquipmentSlot.Melee: melee = weapon; break;
            }
            return true;
        }
        public RuntimeWeapon Unequip(EquipmentSlot slot)
        {
            RuntimeWeapon result = Get(slot);
            if (slot == selectedSlot && result != null) SelectionChanging?.Invoke();
            switch (slot)
            {
                case EquipmentSlot.Firearm1: firearm1 = null; break;
                case EquipmentSlot.Firearm2: firearm2 = null; break;
                case EquipmentSlot.Melee: melee = null; break;
            }
            return result;
        }
    }
}
