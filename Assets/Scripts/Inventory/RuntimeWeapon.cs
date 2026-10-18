using System;
using UnityEngine;

namespace InsaneDreams.Inventory
{
    [Serializable]
    public sealed class WeaponSnapshot
    {
        public string definitionId;
        public string instanceId;
        public int magazineAmmo;
        public int reserveAmmo;
    }

    [Serializable]
    public sealed class RuntimeWeapon
    {
        [SerializeField] private WeaponDefinition definition;
        [SerializeField] private string instanceId;
        [SerializeField] private int magazineAmmo;
        [SerializeField] private int reserveAmmo;
        [NonSerialized] private InsaneDreams.Combat.WeaponCycle cycle;
        public InsaneDreams.Combat.WeaponCycle Cycle => cycle ??= new InsaneDreams.Combat.WeaponCycle(this);
        public WeaponDefinition Definition => definition;
        public string InstanceId => instanceId;
        public int MagazineAmmo => magazineAmmo;
        public int ReserveAmmo => reserveAmmo;
        public bool UsesAmmunition => definition != null && definition.WeaponCategory == WeaponCategory.Firearm;

        public RuntimeWeapon(WeaponDefinition definition, int reserveAmmo = 0)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            this.definition = definition;
            instanceId = Guid.NewGuid().ToString("N");
            SetAmmunition(definition.MagazineCapacity, reserveAmmo);
        }

        public void SetAmmunition(int magazine, int reserve)
        {
            magazineAmmo = UsesAmmunition ? Mathf.Clamp(magazine, 0, definition.MagazineCapacity) : 0;
            reserveAmmo = UsesAmmunition ? Mathf.Max(0, reserve) : 0;
        }

        public WeaponSnapshot Capture() => new WeaponSnapshot
        {
            definitionId = definition.StableId, instanceId = instanceId,
            magazineAmmo = magazineAmmo, reserveAmmo = reserveAmmo
        };

        public static RuntimeWeapon Restore(WeaponSnapshot data, WeaponDefinition definition)
        {
            if (data == null || definition == null || !definition.HasValidIdentity ||
                data.definitionId != definition.StableId || !Guid.TryParse(data.instanceId, out _))
                throw new ArgumentException("Snapshot requires a matching stable definition and valid instance identity.");
            var result = new RuntimeWeapon(definition) { instanceId = data.instanceId };
            result.SetAmmunition(data.magazineAmmo, data.reserveAmmo);
            return result;
        }
    }
}
