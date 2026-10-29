using InsaneDreams.Input;
using UnityEngine;

namespace InsaneDreams.Inventory
{
    // Preserve the numeric values already present in scenes and saves.
    public enum EquipmentSlot { Firearm1=0, Firearm2=1, Melee=2 }
    public sealed class PlayerLoadout : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition initialFirearm1;
        [SerializeField] private WeaponDefinition initialFirearm2;
        [SerializeField] private WeaponDefinition initialMelee;
        [SerializeField, Min(0)] private int initialReserveAmmo = 36;
        [SerializeField, Min(1)] private int maximumReserveAmmo=432;
        public int MaximumReserveAmmo=>maximumReserveAmmo;
        public void ConfigureReserveCapacity(int limit)=>maximumReserveAmmo=Mathf.Max(1,limit);
        private readonly System.Collections.Generic.Dictionary<WeaponDefinition,int> reserveLimits=new System.Collections.Generic.Dictionary<WeaponDefinition,int>();
        public void ClearReserveLimits()=>reserveLimits.Clear();
        public void ConfigureReserveLimit(WeaponDefinition weapon,int limit){if(weapon!=null)reserveLimits[weapon]=Mathf.Max(1,limit);}
        public int ReserveCapacityFor(WeaponDefinition weapon)=>weapon!=null&&reserveLimits.TryGetValue(weapon,out int limit)?limit:maximumReserveAmmo;
        private float equipmentReloadMultiplier=1;
        public void ConfigureReloadMultiplier(float value){equipmentReloadMultiplier=Mathf.Clamp(value,1,1.3f);firearm1?.SetReloadMultiplier(equipmentReloadMultiplier);firearm2?.SetReloadMultiplier(equipmentReloadMultiplier);}
        public int AddReserveAmmunition(int amount)
        {
            var weapon=SelectedWeapon;
            if(weapon==null||!weapon.UsesAmmunition)weapon=firearm1!=null&&firearm1.UsesAmmunition?firearm1:firearm2;
            if(weapon==null||!weapon.UsesAmmunition||amount<=0)return 0;
            int added=Mathf.Min(amount,Mathf.Max(0,ReserveCapacityFor(weapon.Definition)-weapon.ReserveAmmo));
            weapon.SetAmmunition(weapon.MagazineAmmo,weapon.ReserveAmmo+added);return added;
        }
        [SerializeField] private RuntimeWeapon firearm1;
        [SerializeField] private RuntimeWeapon firearm2;
        [SerializeField] private RuntimeWeapon melee;
        [SerializeField] private EquipmentSlot selectedSlot;
        private PlayerActions actions;
        private InsaneDreams.Combat.PlayerThrowables throwables;
        public event System.Action SelectionChanging;
        public EquipmentSlot SelectedSlot => selectedSlot;
        // Preserve serialized slot2/save data while replacing only the player's active tool slot.
        public RuntimeWeapon SelectedWeapon => throwables!=null&&selectedSlot==EquipmentSlot.Melee?null:Get(selectedSlot);
        public RuntimeWeapon Get(EquipmentSlot slot)
        {
            var weapon=slot switch{EquipmentSlot.Firearm1=>firearm1,EquipmentSlot.Firearm2=>firearm2,EquipmentSlot.Melee=>melee,_=>null};
            // Unity may materialize an empty inline serializable class for a new or
            // unequipped Inspector field. It is an empty slot, never a usable weapon.
            return weapon?.Definition!=null&&weapon.Definition.Fits(slot)?weapon:null;
        }

        private void Awake()
        {
            if(!System.Enum.IsDefined(typeof(EquipmentSlot),selectedSlot))selectedSlot=EquipmentSlot.Firearm1;
            throwables=GetComponent<InsaneDreams.Combat.PlayerThrowables>();
            if(firearm1?.Definition==null||!firearm1.Definition.Fits(EquipmentSlot.Firearm1))firearm1=null;
            if(firearm2?.Definition==null||!firearm2.Definition.Fits(EquipmentSlot.Firearm2))firearm2=null;
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
            if (weapon.Definition == null || !weapon.Definition.Fits(slot)) return false;
            foreach(var equipped in new[]{firearm1,firearm2,melee})
                if(equipped!=null&&equipped.Definition==weapon.Definition&&Get(slot)!=equipped)return false;
            if(weapon.UsesAmmunition)weapon.SetReloadMultiplier(equipmentReloadMultiplier);
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
