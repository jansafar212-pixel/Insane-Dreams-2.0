using InsaneDreams.Input;
using InsaneDreams.Inventory;
using UnityEngine;

namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(PlayerActions), typeof(PlayerLoadout), typeof(AttackExecutor))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        private PlayerActions actions;
        private PlayerLoadout loadout;
        private AttackExecutor executor;
        private RuntimeWeapon previous;
        private PlayerCover2D cover;
        private Visuals.PlayerSyringeVisual syringe;
        public event System.Action ReloadStarted;
        private void Awake() { actions = GetComponent<PlayerActions>(); loadout = GetComponent<PlayerLoadout>(); executor = GetComponent<AttackExecutor>(); cover = GetComponent<PlayerCover2D>(); syringe=GetComponent<Visuals.PlayerSyringeVisual>(); }
        private void OnEnable() { actions.ReloadRequested += Reload; actions.InputReset += Cancel; loadout.SelectionChanging += Cancel; }
        private void OnDisable() { actions.ReloadRequested -= Reload; actions.InputReset -= Cancel; loadout.SelectionChanging -= Cancel; Cancel(); }
        public void Reload() { if ((syringe==null||!syringe.IsPlaying)&&loadout.SelectedWeapon != null && loadout.SelectedWeapon.Cycle.TryReload(Time.time)) ReloadStarted?.Invoke(); }
        public void Cancel() { previous?.Cycle.CancelReload(); loadout.SelectedWeapon?.Cycle.CancelReload(); }
        private void LateUpdate()
        {
            var current = loadout.SelectedWeapon;
            if (current != previous) { previous?.Cycle.CancelReload(); previous = current; }
            if (current == null) return;
            current.Cycle.Tick(Time.time);
            if(syringe!=null&&syringe.IsPlaying)return;
            if (!actions.FireHeld || cover != null && cover.IsCovered && actions.HasAimIntent && !cover.PeekReady) return;
            Vector2 aim = actions.AimDirection;
            // Roll only on an eligible shot, preserving ordinary aiming and all cycle rules.
            if (cover != null && cover.IsBlindFiring && !current.Cycle.IsReloading && !current.Cycle.IsMalfunctioning(Time.time) && current.MagazineAmmo > 0 && Time.time >= current.Cycle.NextAttackAt)
                aim = Quaternion.Euler(0, 0, Random.Range(-cover.BlindSpread(0), cover.BlindSpread(0))) * aim;
            executor.Attack(current, aim, Time.time);
        }
    }
}
