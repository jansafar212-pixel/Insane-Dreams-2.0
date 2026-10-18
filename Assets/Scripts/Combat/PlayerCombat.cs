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
        public event System.Action ReloadStarted;
        private void Awake() { actions = GetComponent<PlayerActions>(); loadout = GetComponent<PlayerLoadout>(); executor = GetComponent<AttackExecutor>(); }
        private void OnEnable() { actions.ReloadRequested += Reload; actions.InputReset += Cancel; loadout.SelectionChanging += Cancel; }
        private void OnDisable() { actions.ReloadRequested -= Reload; actions.InputReset -= Cancel; loadout.SelectionChanging -= Cancel; Cancel(); }
        public void Reload() { if (loadout.SelectedWeapon != null && loadout.SelectedWeapon.Cycle.TryReload(Time.time)) ReloadStarted?.Invoke(); }
        public void Cancel() { previous?.Cycle.CancelReload(); loadout.SelectedWeapon?.Cycle.CancelReload(); }
        private void LateUpdate()
        {
            var current = loadout.SelectedWeapon;
            if (current != previous) { previous?.Cycle.CancelReload(); previous = current; }
            if (current == null) return;
            current.Cycle.Tick(Time.time);
            if (actions.FireHeld) executor.Attack(current, actions.AimDirection, Time.time);
        }
    }
}
