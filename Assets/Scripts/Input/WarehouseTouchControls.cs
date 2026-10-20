using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneDreams.Input
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class WarehouseTouchControls : MonoBehaviour
    {
        [SerializeField] private PlayerActions actions;
        [SerializeField, Range(.75f, 1)] private float fireThreshold = .85f;
        [SerializeField, Range(.4f, .74f)] private float fireReleaseThreshold = .7f;
        private bool firingZone;
        private Stick moveStick;
        private Stick aimStick;
        private VisualElement parachute;
        private VisualElement root;
        private int chutePointer = -1;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;
        private PlayerActions subscribedActions;
        private InsaneDreams.Inventory.PlayerLoadout loadout;
        private Label loadoutLabel;
        private float nextReadout;
        private InsaneDreams.Combat.Health health;
        private InsaneDreams.Combat.PlayerHealing healing;
        private InsaneDreams.Traversal.PlayerTraversal2D traversal;
        private Label chuteLabel;
        private Button healButton;
        public bool IsBound { get; private set; }

        public void Bind(PlayerActions target)
        {
            if (subscribedActions != null) subscribedActions.InputReset -= ClearPointers;
            ClearPointers();
            if (actions != null && actions != target) actions.ResetActions();
            actions = target;
            subscribedActions = target;
            loadout = target != null ? target.GetComponent<InsaneDreams.Inventory.PlayerLoadout>() : null;
            health = target != null ? target.GetComponent<InsaneDreams.Combat.Health>() : null;
            healing = target != null ? target.GetComponent<InsaneDreams.Combat.PlayerHealing>() : null;
            traversal = target != null ? target.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>() : null;
            nextReadout = 0;
            if (subscribedActions != null) subscribedActions.InputReset += ClearPointers;
            IsBound = actions != null && moveStick != null;
        }

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            loadoutLabel = root.Q<Label>("loadout");
            root.Q<Button>("slot1")?.RegisterCallback<ClickEvent>(SelectFirst);
            root.Q<Button>("slot2")?.RegisterCallback<ClickEvent>(SelectSecond);
            root.Q<Button>("melee")?.RegisterCallback<ClickEvent>(SelectMelee);
            root.Q<Button>("reload")?.RegisterCallback<ClickEvent>(Reload);
            healButton=root.Q<Button>("heal");healButton?.RegisterCallback<ClickEvent>(Heal);
            moveStick = new Stick(root.Q<VisualElement>("moveStick"), value => { if (actions != null) actions.SetTouchMove(value); });
            aimStick = new Stick(root.Q<VisualElement>("aimStick"), value =>
            {
                if (actions == null) return;
                actions.SetTouchAim(value);
                float magnitude = value.magnitude;
                if (magnitude >= fireThreshold) firingZone = true;
                if (magnitude <= fireReleaseThreshold) firingZone = false;
                actions.SetFireIntent(firingZone);
            });
            parachute = root.Q<VisualElement>("parachuteButton");
            chuteLabel = parachute.Q<Label>();
            parachute.RegisterCallback<PointerDownEvent>(ChuteDown);
            parachute.RegisterCallback<PointerUpEvent>(ChuteUp);
            parachute.RegisterCallback<PointerCancelEvent>(ChuteCancel);
            parachute.RegisterCallback<PointerCaptureOutEvent>(ChuteCaptureLost);
            Bind(actions);
            lastScreen = Vector2Int.zero;
        }

        private void SelectFirst(ClickEvent evt) => actions?.SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Firearm1);
        private void SelectSecond(ClickEvent evt) => actions?.SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Firearm2);
        private void SelectMelee(ClickEvent evt) => actions?.SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Melee);
        private void Reload(ClickEvent evt) => actions?.RequestReload();
        private void Heal(ClickEvent evt) => actions?.RequestHeal();

        private void ChuteDown(PointerDownEvent evt)
        {
            if (chutePointer != -1) return;
            chutePointer = evt.pointerId;
            parachute.CapturePointer(chutePointer);
            if (actions != null) actions.SetTouchParachute(true);
            evt.StopPropagation();
        }
        private void ChuteUp(PointerUpEvent evt) { if (evt.pointerId == chutePointer) ReleaseChute(); }
        private void ChuteCancel(PointerCancelEvent evt) { if (evt.pointerId == chutePointer) ReleaseChute(); }
        private void ChuteCaptureLost(PointerCaptureOutEvent evt) { if (evt.pointerId == chutePointer) ReleaseChute(); }
        private void ReleaseChute()
        {
            int pointer = chutePointer;
            chutePointer = -1;
            if (pointer >= 0 && parachute.HasPointerCapture(pointer)) parachute.ReleasePointer(pointer);
            if (actions != null) actions.SetTouchParachute(false);
        }

        private void Update()
        {
            bool deployed = traversal != null && traversal.IsParachuting;
            parachute.EnableInClassList("deployed", deployed);
            string caption = deployed ? "RETRACT\nCHUTE" : "DEPLOY\nCHUTE";
            if (chuteLabel != null && chuteLabel.text != caption) chuteLabel.text = caption;
            if (loadoutLabel != null && Time.unscaledTime >= nextReadout)
            {
                nextReadout = Time.unscaledTime + .2f;
                var weapon = loadout != null ? loadout.SelectedWeapon : null;
                loadoutLabel.text = loadout == null ? "Loadout: unbound" : weapon == null ? $"{loadout.SelectedSlot}: empty" :
                    $"{loadout.SelectedSlot}: {weapon.Definition.DisplayName}\n" +
                    (weapon.UsesAmmunition ? $"Ammo {weapon.MagazineAmmo} / reserve {weapon.ReserveAmmo}" : "Melee / unlimited uses") +
                    (weapon.Cycle.IsReloading ? "\nRELOADING" : "") + (health != null ? $"\nHealth {health.Current:0}/{health.Maximum:0}" : "") +
                    (healing==null?"":Time.time<healing.FeedbackUntil?$"\nHEALED +{healing.LastRestored:0}":healing.CooldownRemaining>0?$"\nHeal {healing.CooldownRemaining:0.0}s":"\nH — HEAL");
                loadoutLabel.style.color=healing!=null&&Time.time<healing.FeedbackUntil?new Color(.35f,1,.55f):Color.white;
                if(healButton!=null)healButton.text=healing!=null&&healing.CooldownRemaining>0?$"HEAL\n{healing.CooldownRemaining:0.0}s":"HEAL";
            }
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (lastSafeArea == Screen.safeArea && lastScreen == screen) return;
            lastSafeArea = Screen.safeArea;
            lastScreen = screen;
            VisualElement safe = root.Q<VisualElement>("safeArea");
            Vector4 insets = SafeAreaInsets(lastSafeArea, screen);
            safe.style.left = Length.Percent(insets.x * 100);
            safe.style.right = Length.Percent(insets.y * 100);
            safe.style.top = Length.Percent(insets.z * 100);
            safe.style.bottom = Length.Percent(insets.w * 100);
        }

        public static Vector4 SafeAreaInsets(Rect safeArea, Vector2 screen)
        {
            float width = Mathf.Max(1, screen.x), height = Mathf.Max(1, screen.y);
            return new Vector4(Mathf.Clamp01(safeArea.xMin / width), Mathf.Clamp01((width - safeArea.xMax) / width),
                Mathf.Clamp01((height - safeArea.yMax) / height), Mathf.Clamp01(safeArea.yMin / height));
        }

        private void ClearPointers()
        {
            moveStick?.Release(); aimStick?.Release();
            firingZone = false;
            if (parachute != null) ReleaseChute();
        }
        private void CancelInput(bool resetDeployment = true)
        {
            ClearPointers();
            if (actions != null) { if (resetDeployment) actions.ResetActions(); else actions.SuspendInput(); }
        }
        private void OnApplicationFocus(bool focused) { if (!focused) CancelInput(false); }
        private void OnApplicationPause(bool paused) { if (paused) CancelInput(false); }
        private void OnDisable()
        {
            CancelInput();
            if (subscribedActions != null) subscribedActions.InputReset -= ClearPointers;
            subscribedActions = null;
            moveStick?.Dispose(); aimStick?.Dispose();
            if (root == null) return;
            root.Q<Button>("slot1")?.UnregisterCallback<ClickEvent>(SelectFirst);
            root.Q<Button>("slot2")?.UnregisterCallback<ClickEvent>(SelectSecond);
            root.Q<Button>("melee")?.UnregisterCallback<ClickEvent>(SelectMelee);
            root.Q<Button>("reload")?.UnregisterCallback<ClickEvent>(Reload);
            healButton?.UnregisterCallback<ClickEvent>(Heal);
            parachute?.UnregisterCallback<PointerDownEvent>(ChuteDown);
            parachute?.UnregisterCallback<PointerUpEvent>(ChuteUp);
            parachute?.UnregisterCallback<PointerCancelEvent>(ChuteCancel);
            parachute?.UnregisterCallback<PointerCaptureOutEvent>(ChuteCaptureLost);
            IsBound = false;
        }

        private sealed class Stick
        {
            private readonly VisualElement pad;
            private readonly VisualElement knob;
            private readonly System.Action<Vector2> changed;
            private int pointer = -1;
            public Stick(VisualElement element, System.Action<Vector2> callback)
            {
                pad = element; knob = pad.Q<VisualElement>(className: "knob"); changed = callback;
                pad.RegisterCallback<PointerDownEvent>(Down);
                pad.RegisterCallback<PointerMoveEvent>(Move);
                pad.RegisterCallback<PointerUpEvent>(Up);
                pad.RegisterCallback<PointerCancelEvent>(Cancel);
                pad.RegisterCallback<PointerCaptureOutEvent>(CaptureLost);
            }
            private void Down(PointerDownEvent evt)
            {
                if (pointer != -1) return;
                pointer = evt.pointerId; pad.CapturePointer(pointer); SetPosition(evt.position); evt.StopPropagation();
            }
            private void Move(PointerMoveEvent evt) { if (evt.pointerId == pointer) { SetPosition(evt.position); evt.StopPropagation(); } }
            private void Up(PointerUpEvent evt) { if (evt.pointerId == pointer) Release(); }
            private void Cancel(PointerCancelEvent evt) { if (evt.pointerId == pointer) Release(); }
            private void CaptureLost(PointerCaptureOutEvent evt) { if (evt.pointerId == pointer) Release(); }
            private void SetPosition(Vector2 position)
            {
                Vector2 local = pad.WorldToLocal(position) - pad.contentRect.center;
                float radius = Mathf.Max(1, pad.contentRect.width * 0.35f);
                Vector2 axis = Vector2.ClampMagnitude(local / radius, 1);
                knob.style.translate = new Translate(axis.x * radius, axis.y * radius);
                changed(new Vector2(axis.x, -axis.y));
            }
            public void Release()
            {
                int old = pointer; pointer = -1;
                if (old >= 0 && pad.HasPointerCapture(old)) pad.ReleasePointer(old);
                knob.style.translate = new Translate(0, 0); changed(Vector2.zero);
            }
            public void Dispose()
            {
                pad.UnregisterCallback<PointerDownEvent>(Down); pad.UnregisterCallback<PointerMoveEvent>(Move);
                pad.UnregisterCallback<PointerUpEvent>(Up); pad.UnregisterCallback<PointerCancelEvent>(Cancel);
                pad.UnregisterCallback<PointerCaptureOutEvent>(CaptureLost);
            }
        }
    }
}
