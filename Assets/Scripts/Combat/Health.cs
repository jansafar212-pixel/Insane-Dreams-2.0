using System;
using UnityEngine;

namespace InsaneDreams.Combat
{
    public enum CombatTeam { Neutral, Player, Enemy }
    public enum DamageKind { Ballistic, Melee, Explosion, Environment }
    public readonly struct DamageOutcome
    {
        public readonly DamageInfo Hit;
        public readonly float HealthLost, ArmorAbsorbed;
        public DamageOutcome(DamageInfo hit,float lost,float absorbed)
        {Hit=hit;HealthLost=lost;ArmorAbsorbed=absorbed;}
    }
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly CombatTeam Team;
        public readonly GameObject Source;
        public readonly PhysicalImpact? Impact;
        public readonly bool AllowFriendlyFire;
        public readonly DamageKind Kind;
        public DamageInfo(float amount, CombatTeam team, GameObject source = null, PhysicalImpact? impact = null,bool allowFriendlyFire=false,DamageKind kind=DamageKind.Ballistic)
        { Amount = amount; Team = team; Source = source; Impact = impact;AllowFriendlyFire=allowFriendlyFire;Kind=kind; }
    }
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] private float maximum = 100;
        [SerializeField] private CombatTeam team;
        [SerializeField] private bool allowFriendlyFire;
        [SerializeField] private float current;
        private float protectedUntil;
        private BodyArmor armor;
        public float Maximum => maximum;
        public float Current => current;
        public CombatTeam Team => team;
        public bool IsDefeated { get; private set; }
        public event Action<DamageInfo> Damaged;
        public event Action<DamageOutcome> DamageResolved;
        public event Action<DamageInfo> Defeated;
        private void Awake() {armor=GetComponent<BodyArmor>();Restore();}
        public void Restore() { current = maximum; IsDefeated = false; }
        public void ProtectFor(float seconds) => protectedUntil = Time.time + Mathf.Max(0, seconds);
        public float Heal(float amount)
        {
            if(IsDefeated||amount<=0||float.IsNaN(amount)||float.IsInfinity(amount))return 0;
            float restored=Mathf.Min(amount,maximum-current);current+=restored;return restored;
        }
        public bool ApplyDamage(DamageInfo damage)
        {
            if (IsDefeated || Time.time < protectedUntil || damage.Amount <= 0 || float.IsNaN(damage.Amount) ||
                float.IsInfinity(damage.Amount) || damage.Source == gameObject) return false;
            if (!allowFriendlyFire && !damage.AllowFriendlyFire && team != CombatTeam.Neutral && damage.Team == team) return false;
            float before=current,unarmored=armor!=null?armor.Absorb(damage):damage.Amount;
            current = Mathf.Max(0, current - unarmored);
            bool defeated = current <= 0;
            if (defeated) IsDefeated = true;
            DamageResolved?.Invoke(new DamageOutcome(damage,before-current,Mathf.Max(0,damage.Amount-unarmored)));
            Damaged?.Invoke(damage);
            if (defeated) Defeated?.Invoke(damage);
            return true;
        }
    }
}
