using InsaneDreams.Inventory;
using UnityEngine;

namespace InsaneDreams.Combat
{
    // Caller-supplied game time keeps cadence and reload rules deterministic.
    public sealed class WeaponCycle
    {
        private readonly RuntimeWeapon weapon;
        public bool IsReloading { get; private set; }
        public float ReloadEndsAt { get; private set; }
        public float ReloadStartedAt { get; private set; }
        public float ReloadProgress(float now) => IsReloading ? Mathf.InverseLerp(ReloadStartedAt, ReloadEndsAt, now) : 0;
        public float NextAttackAt { get; private set; }
        public WeaponCycle(RuntimeWeapon weapon) => this.weapon = weapon;
        public void Tick(float now)
        {
            if (!IsReloading || now < ReloadEndsAt || float.IsNaN(now) || float.IsInfinity(now)) return;
            IsReloading = false;
            int transfer = Mathf.Min(weapon.Definition.MagazineCapacity - weapon.MagazineAmmo, weapon.ReserveAmmo);
            weapon.SetAmmunition(weapon.MagazineAmmo + transfer, weapon.ReserveAmmo - transfer);
        }
        public bool TryAttack(float now)
        {
            if (float.IsNaN(now) || float.IsInfinity(now)) return false;
            Tick(now);
            if (IsReloading || now < NextAttackAt) return false;
            if (weapon.UsesAmmunition)
            {
                if (weapon.MagazineAmmo <= 0) return false;
                weapon.SetAmmunition(weapon.MagazineAmmo - 1, weapon.ReserveAmmo);
            }
            NextAttackAt = now + Mathf.Max(.01f, weapon.Definition.FiringInterval);
            return true;
        }
        public bool TryReload(float now)
        {
            if (float.IsNaN(now) || float.IsInfinity(now)) return false;
            Tick(now);
            if (!weapon.UsesAmmunition || IsReloading || weapon.ReserveAmmo <= 0 || weapon.MagazineAmmo >= weapon.Definition.MagazineCapacity) return false;
            IsReloading = true; ReloadStartedAt = now; ReloadEndsAt = now + weapon.Definition.ReloadDuration; return true;
        }
        public void CancelReload() => IsReloading = false;
    }
}
