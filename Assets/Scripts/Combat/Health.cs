using System;
using UnityEngine;

namespace InsaneDreams.Combat
{
    public enum CombatTeam { Neutral, Player, Enemy }
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly CombatTeam Team;
        public readonly GameObject Source;
        public readonly PhysicalImpact? Impact;
        public DamageInfo(float amount, CombatTeam team, GameObject source = null, PhysicalImpact? impact = null)
        { Amount = amount; Team = team; Source = source; Impact = impact; }
    }
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] private float maximum = 100;
        [SerializeField] private CombatTeam team;
        [SerializeField] private bool allowFriendlyFire;
        [SerializeField] private float current;
        private float protectedUntil;
        public float Maximum => maximum;
        public float Current => current;
        public CombatTeam Team => team;
        public bool IsDefeated { get; private set; }
        public event Action<DamageInfo> Damaged;
        public event Action<DamageInfo> Defeated;
        private void Awake() => Restore();
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
            if (!allowFriendlyFire && team != CombatTeam.Neutral && damage.Team == team) return false;
            current = Mathf.Max(0, current - damage.Amount);
            bool defeated = current <= 0;
            if (defeated) IsDefeated = true;
            Damaged?.Invoke(damage);
            if (defeated) Defeated?.Invoke(damage);
            return true;
        }
    }
}
