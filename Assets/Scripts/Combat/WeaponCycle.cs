using InsaneDreams.Inventory;
using UnityEngine;

namespace InsaneDreams.Combat
{
    // Caller-supplied game time keeps cadence and reload rules deterministic.
    public sealed class WeaponCycle
    {
        private readonly RuntimeWeapon weapon;
        private float stepDuration;
        public bool IsReloading { get; private set; }
        public event System.Action MagazineInserted;
        public float ReloadEndsAt { get; private set; }
        public float ReloadStartedAt { get; private set; }
        public float ReloadProgress(float now) => IsReloading ? Mathf.InverseLerp(ReloadStartedAt, ReloadEndsAt, now) : 0;
        public float NextAttackAt { get; private set; }
        public bool IsRecovering(float now)=>weapon.Definition.FireMode==WeaponFireMode.ReleaseToFire&&now<NextAttackAt;
        private float malfunctionUntil;
        public bool IsMalfunctioning(float now)=>now<malfunctionUntil;
        public bool TryMalfunction(float now,float seconds)
        {
            if(float.IsNaN(now)||float.IsInfinity(now)||float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0||
                !weapon.UsesAmmunition||weapon.MagazineAmmo<=0||IsReloading||now<NextAttackAt||IsMalfunctioning(now))return false;
            malfunctionUntil=now+Mathf.Clamp(seconds,.1f,5);return true;
        }
        public void CancelMalfunction()=>malfunctionUntil=0;
        public WeaponCycle(RuntimeWeapon weapon) => this.weapon = weapon;
        public void Tick(float now)
        {
            if (!IsReloading || now < ReloadEndsAt || float.IsNaN(now) || float.IsInfinity(now)) return;
            if(weapon.Definition.LoadsIndividually)
            {
                // Bounded by capacity, including a delayed frame; each completed step owns one shell.
                for(int i=0;i<weapon.Definition.MagazineCapacity&&IsReloading&&now>=ReloadEndsAt;i++)
                {
                    if(weapon.ReserveAmmo<=0||weapon.MagazineAmmo>=weapon.Definition.MagazineCapacity){IsReloading=false;break;}
                    weapon.SetAmmunition(weapon.MagazineAmmo+1,weapon.ReserveAmmo-1);
                    MagazineInserted?.Invoke();
                    if(weapon.ReserveAmmo<=0||weapon.MagazineAmmo>=weapon.Definition.MagazineCapacity)IsReloading=false;
                    else{ReloadStartedAt=ReloadEndsAt;ReloadEndsAt+=stepDuration;}
                }
                return;
            }
            IsReloading = false;
            int transfer = Mathf.Min(weapon.Definition.MagazineCapacity - weapon.MagazineAmmo, weapon.ReserveAmmo);
            weapon.SetAmmunition(weapon.MagazineAmmo + transfer, weapon.ReserveAmmo - transfer);
            if(weapon.Definition.PhysicalMagazineReload)MagazineInserted?.Invoke();
        }
        public bool TryAttack(float now)
        {
            if (float.IsNaN(now) || float.IsInfinity(now)) return false;
            Tick(now);
            if(IsReloading&&weapon.Definition.LoadsIndividually&&weapon.MagazineAmmo>0&&now>=NextAttackAt&&!IsMalfunctioning(now))CancelReload();
            if (IsReloading || IsMalfunctioning(now) || now < NextAttackAt) return false;
            if (weapon.UsesAmmunition)
            {
                if (weapon.MagazineAmmo <= 0) return false;
                weapon.SetAmmunition(weapon.MagazineAmmo - 1, weapon.ReserveAmmo);
            }
            NextAttackAt = now + Mathf.Max(.01f, weapon.Definition.FiringInterval);
            return true;
        }
        public bool TryReload(float now, float durationScale = 1)
        {
            if (float.IsNaN(now) || float.IsInfinity(now) || float.IsNaN(durationScale) || float.IsInfinity(durationScale) || durationScale <= 0) return false;
            Tick(now);
            if (!weapon.UsesAmmunition || IsReloading || IsRecovering(now) || IsMalfunctioning(now) || weapon.ReserveAmmo <= 0 || weapon.MagazineAmmo >= weapon.Definition.MagazineCapacity) return false;
            stepDuration=Mathf.Max(.05f,weapon.Definition.ReloadStepDuration*durationScale*weapon.ReloadMultiplier);
            IsReloading = true; ReloadStartedAt = now; ReloadEndsAt = now + stepDuration; return true;
        }
        public void CancelReload() => IsReloading = false;
    }
}
