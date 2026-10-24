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
        [SerializeField, Range(.75f, 1.5f)] private float joystickScale = 1.15f;
        private VisualElement jumpButton;
        private int jumpPointer = -1;
        private VisualElement safeArea;
        private bool firingZone;
        private VisualElement fireButton;
        private int firePointer = -1;
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
        private InsaneDreams.Combat.BodyArmor armor;
        private InsaneDreams.Combat.PlayerCover2D cover;
        private InsaneDreams.Combat.PlayerSuppression suppression;
        private VisualElement pressureFrame;
        private InsaneDreams.Combat.PlayerRevive revive;
        private VisualElement reviveOverlay;
        private Button reviveButton;
        private VisualElement blindFireButton,activeFireControl;
        private InsaneDreams.Traversal.PlayerTraversal2D traversal;
        private Label chuteLabel;
        private Button healButton;
        private int healPointer=-1;
        private Button supplyButton;
        private Label suppliesNotice;
        private InsaneDreams.Inventory.PlayerSupplies supplies;
        public bool IsBound { get; private set; }

        public void Bind(PlayerActions target)
        {
            if (subscribedActions != null) subscribedActions.InputReset -= ClearPointers;
            ClearPointers();
            if (actions != null && actions != target) actions.ResetActions();
            actions = target;
            cover=target!=null?target.GetComponent<InsaneDreams.Combat.PlayerCover2D>():null;
            suppression=target!=null?target.GetComponent<InsaneDreams.Combat.PlayerSuppression>():null;
            revive=target!=null?target.GetComponent<InsaneDreams.Combat.PlayerRevive>():null;
            subscribedActions = target;
            loadout = target != null ? target.GetComponent<InsaneDreams.Inventory.PlayerLoadout>() : null;
            health = target != null ? target.GetComponent<InsaneDreams.Combat.Health>() : null;
            healing = target != null ? target.GetComponent<InsaneDreams.Combat.PlayerHealing>() : null;
            armor = target != null ? target.GetComponent<InsaneDreams.Combat.BodyArmor>() : null;
            supplies=target!=null?target.GetComponent<InsaneDreams.Inventory.PlayerSupplies>():null;
            traversal = target != null ? target.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>() : null;
            nextReadout = 0;
            if (subscribedActions != null) subscribedActions.InputReset += ClearPointers;
            IsBound = actions != null && moveStick != null;
        }

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            safeArea = root.Q<VisualElement>("safeArea");
            safeArea.RegisterCallback<GeometryChangedEvent>(LayoutControls);
            jumpButton = root.Q<VisualElement>("jumpButton");
            jumpButton?.RegisterCallback<PointerDownEvent>(JumpDown);
            jumpButton?.RegisterCallback<PointerUpEvent>(JumpUp);
            jumpButton?.RegisterCallback<PointerCancelEvent>(JumpCancel);
            jumpButton?.RegisterCallback<PointerCaptureOutEvent>(JumpLost);
            loadoutLabel = root.Q<Label>("loadout");
            root.Q<Button>("slot1")?.RegisterCallback<ClickEvent>(SelectFirst);
            root.Q<Button>("slot2")?.RegisterCallback<ClickEvent>(SelectSecond);
            root.Q<Button>("melee")?.RegisterCallback<ClickEvent>(SelectMelee);
            root.Q<Button>("reload")?.RegisterCallback<ClickEvent>(Reload);
            healButton=root.Q<Button>("heal");
            healButton?.RegisterCallback<PointerDownEvent>(HealDown,TrickleDown.TrickleDown);
            healButton?.RegisterCallback<PointerUpEvent>(HealUp,TrickleDown.TrickleDown);
            healButton?.RegisterCallback<PointerCancelEvent>(HealCancel);
            healButton?.RegisterCallback<PointerCaptureOutEvent>(HealLost);
            supplyButton=root.Q<Button>("supplyInteract");supplyButton?.RegisterCallback<ClickEvent>(Supply);
            suppliesNotice=root.Q<Label>("suppliesNotice");
            moveStick = new Stick(root.Q<VisualElement>("moveStick"), value => { if (actions != null) actions.SetTouchMove(value); });
            aimStick = new Stick(root.Q<VisualElement>("aimStick"), value =>
            {
                if (actions == null) return;
                actions.SetTouchAim(value);
                float magnitude = value.magnitude;
                if (magnitude >= fireThreshold) firingZone = true;
                if (magnitude <= fireReleaseThreshold) firingZone = false;
                UpdateFireIntent();
            });
            parachute = root.Q<VisualElement>("parachuteButton");
            chuteLabel = parachute.Q<Label>();
            fireButton = root.Q<VisualElement>("fireButton");
            blindFireButton=root.Q<VisualElement>("blindFireButton");
            blindFireButton?.RegisterCallback<PointerDownEvent>(FireDown);
            blindFireButton?.RegisterCallback<PointerUpEvent>(FireUp);
            blindFireButton?.RegisterCallback<PointerCancelEvent>(FireCancel);
            blindFireButton?.RegisterCallback<PointerCaptureOutEvent>(FireCaptureLost);
            pressureFrame=new VisualElement{name="incomingFirePressure",pickingMode=PickingMode.Ignore};
            pressureFrame.style.position=Position.Absolute;pressureFrame.style.left=pressureFrame.style.right=pressureFrame.style.top=pressureFrame.style.bottom=0;
            pressureFrame.style.borderLeftWidth=pressureFrame.style.borderRightWidth=pressureFrame.style.borderTopWidth=pressureFrame.style.borderBottomWidth=3;
            root.Add(pressureFrame);
            reviveOverlay=new VisualElement{name="reviveOverlay"};
            reviveOverlay.style.position=Position.Absolute;
            reviveOverlay.style.left=reviveOverlay.style.right=reviveOverlay.style.top=reviveOverlay.style.bottom=0;
            reviveOverlay.style.alignItems=Align.Center;reviveOverlay.style.justifyContent=Justify.Center;
            reviveOverlay.style.backgroundColor=new Color(0,0,0,.65f);
            reviveOverlay.style.color=Color.white;reviveOverlay.style.fontSize=20;
            reviveOverlay.style.display=DisplayStyle.None;
            reviveOverlay.Add(new Label("YOU'RE NOT QUITE DONE YET."));
            reviveButton=new Button(()=>revive?.TryRevive()){name="reviveButton",text="INJECT FAINTANYL — REVIVE"};
            reviveButton.style.minWidth=300;reviveButton.style.minHeight=64;
            reviveButton.style.fontSize=22;reviveButton.style.color=Color.white;reviveButton.style.backgroundColor=new Color(.12f,.32f,.28f);
            reviveOverlay.Add(reviveButton);
            var checkpoint=new Button(()=>revive?.ReturnToCheckpoint()){name="reviveCheckpoint",text="RETURN TO CHECKPOINT"};
            checkpoint.style.minHeight=48;reviveOverlay.Add(checkpoint);root.Add(reviveOverlay);
            checkpoint.style.color=Color.white;checkpoint.style.backgroundColor=new Color(.16f,.19f,.23f);
            fireButton?.RegisterCallback<PointerDownEvent>(FireDown);
            fireButton?.RegisterCallback<PointerUpEvent>(FireUp);
            fireButton?.RegisterCallback<PointerCancelEvent>(FireCancel);
            fireButton?.RegisterCallback<PointerCaptureOutEvent>(FireCaptureLost);
            parachute.RegisterCallback<PointerDownEvent>(ChuteDown);
            parachute.RegisterCallback<PointerUpEvent>(ChuteUp);
            parachute.RegisterCallback<PointerCancelEvent>(ChuteCancel);
            parachute.RegisterCallback<PointerCaptureOutEvent>(ChuteCaptureLost);
            Bind(actions);
            lastScreen = Vector2Int.zero;
            ApplyJoystickLayout();
        }

        private void LayoutControls(GeometryChangedEvent evt) => ApplyJoystickLayout();
        public void SetJoystickScale(float value){joystickScale=Mathf.Clamp(value,.75f,1.5f);if(root!=null)ApplyJoystickLayout();}
        private void ApplyJoystickLayout()
        {
            float diameter = 187.5f * Mathf.Clamp(joystickScale, .75f, 1.5f);
            // Leave room for each action column and a central gap inside the safe area.
            if (safeArea.contentRect.width > 0)
                diameter = Mathf.Min(diameter, Mathf.Max(100, safeArea.contentRect.width * .5f - 144));
            float scale = diameter / 187.5f;
            foreach (string name in new[] { "moveStick", "aimStick" })
            {
                var pad = root.Q<VisualElement>(name);
                pad.style.width = pad.style.height = diameter;
                var knob = pad.Q<VisualElement>(className: "knob");
                knob.style.width = knob.style.height = 77.5f * scale;
            }
            float inset = Mathf.Max(244, 28 + diameter + 18);
            if (safeArea.contentRect.width > 0)
                inset = Mathf.Min(inset, safeArea.contentRect.width * .5f - 98);
            root.Q<VisualElement>("heal").style.left = inset;
            root.Q<VisualElement>("supplyInteract").style.left = inset;
            root.Q<VisualElement>("reload").style.right = inset;
            parachute.style.right = inset;
            jumpButton.style.right = inset;
            fireButton.style.bottom = 250 + Mathf.Max(0, diameter - 187.5f);
            if(blindFireButton!=null)blindFireButton.style.bottom=fireButton.style.bottom;
        }

        private void JumpDown(PointerDownEvent evt)
        {
            if (jumpPointer >= 0) return;
            jumpPointer = evt.pointerId;
            jumpButton.CapturePointer(jumpPointer);
            actions?.RequestTouchJump();
            evt.StopPropagation();
        }
        private void JumpUp(PointerUpEvent evt) { if (evt.pointerId == jumpPointer) ReleaseJump(); }
        private void JumpCancel(PointerCancelEvent evt) { if (evt.pointerId == jumpPointer) ReleaseJump(); }
        private void JumpLost(PointerCaptureOutEvent evt) { if (evt.pointerId == jumpPointer) ReleaseJump(); }
        private void ReleaseJump()
        {
            int pointer = jumpPointer; jumpPointer = -1;
            if (pointer >= 0 && jumpButton != null && jumpButton.HasPointerCapture(pointer)) jumpButton.ReleasePointer(pointer);
        }

        private void SelectFirst(ClickEvent evt) => actions?.SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Firearm1);
        private void SelectSecond(ClickEvent evt) => actions?.SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Firearm2);
        private void SelectMelee(ClickEvent evt) => actions?.SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Melee);
        private void Reload(ClickEvent evt) => actions?.RequestReload();
        private void Supply(ClickEvent evt)=>actions?.RequestInteraction();
        private void HealDown(PointerDownEvent evt){if(healPointer>=0)return;healPointer=evt.pointerId;healButton.CapturePointer(healPointer);actions?.SetHealIntent(true);evt.StopPropagation();}
        private void HealUp(PointerUpEvent evt){if(evt.pointerId==healPointer)ReleaseHeal();}
        private void HealCancel(PointerCancelEvent evt){if(evt.pointerId==healPointer)ReleaseHeal();}
        private void HealLost(PointerCaptureOutEvent evt){if(evt.pointerId==healPointer)ReleaseHeal();}
        private void ReleaseHeal(){int pointer=healPointer;healPointer=-1;if(pointer>=0&&healButton!=null&&healButton.HasPointerCapture(pointer))healButton.ReleasePointer(pointer);actions?.SetHealIntent(false);}
        private void UpdateFireIntent() => actions?.SetFireIntent(firingZone || firePointer >= 0);
        private void FireDown(PointerDownEvent evt)
        {
            if (firePointer >= 0) return;
            var control=evt.currentTarget as VisualElement;
            if(control==blindFireButton){if(cover==null||!cover.IsCovered)return;aimStick?.Release();}
            activeFireControl=control;firePointer = evt.pointerId; activeFireControl.CapturePointer(firePointer); UpdateFireIntent(); evt.StopPropagation();
        }
        private void FireUp(PointerUpEvent evt) { if (evt.pointerId == firePointer && evt.currentTarget == activeFireControl) ReleaseFire(); }
        private void FireCancel(PointerCancelEvent evt) { if (evt.pointerId == firePointer && evt.currentTarget == activeFireControl) ReleaseFire(); }
        private void FireCaptureLost(PointerCaptureOutEvent evt) { if (evt.pointerId == firePointer && evt.currentTarget == activeFireControl) ReleaseFire(); }
        private void ReleaseFire()
        {
            int pointer = firePointer; firePointer = -1;
            var control=activeFireControl;activeFireControl=null;
            if (pointer >= 0 && control != null && control.HasPointerCapture(pointer)) control.ReleasePointer(pointer);
            UpdateFireIntent();
        }

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
            bool inCover=cover!=null&&cover.IsCovered;
            if(fireButton!=null)fireButton.style.display=inCover?DisplayStyle.None:DisplayStyle.Flex;
            if(blindFireButton!=null)blindFireButton.style.display=inCover?DisplayStyle.Flex:DisplayStyle.None;
            if(!inCover&&activeFireControl==blindFireButton||inCover&&activeFireControl==fireButton)ReleaseFire();
            if(pressureFrame!=null){float pressure=suppression!=null?suppression.Pressure:0;pressureFrame.style.display=pressure>.005f?DisplayStyle.Flex:DisplayStyle.None;var color=new Color(1,.7f,.2f,pressure*.12f);pressureFrame.style.borderLeftColor=pressureFrame.style.borderRightColor=pressureFrame.style.borderTopColor=pressureFrame.style.borderBottomColor=color;}
            bool deployed = traversal != null && traversal.IsParachuting;
            parachute.EnableInClassList("deployed", deployed);
            string caption = deployed ? "RETRACT\nCHUTE" : "DEPLOY\nCHUTE";
            if (chuteLabel != null && chuteLabel.text != caption) chuteLabel.text = caption;
            if (loadoutLabel != null && Time.unscaledTime >= nextReadout)
            {
                nextReadout = Time.unscaledTime + .2f;
                var weapon = loadout != null ? loadout.SelectedWeapon : null;
                string equipment = loadout == null ? "Loadout: unbound" : weapon == null ? $"{loadout.SelectedSlot}: empty" :
                    $"{loadout.SelectedSlot}: {weapon.Definition.DisplayName}\n" +
                    (weapon.UsesAmmunition ? $"Ammo {weapon.MagazineAmmo} / reserve {weapon.ReserveAmmo}" : "Melee / unlimited uses");
                loadoutLabel.text=equipment+(weapon!=null&&weapon.Cycle.IsReloading ? "\nRELOADING" : "") + (health != null ? $"\nHealth {health.Current:0}/{health.Maximum:0}" : "") +
                    (armor==null?"":$"\nArmor {armor.Condition:0}/{armor.Capacity:0}")+
                    (revive==null?"":$"\nFaintanyl {revive.Injections}/{InsaneDreams.Combat.PlayerRevive.Capacity}"+(revive.ProtectionRemaining>0?$" — PROTECTED {revive.ProtectionRemaining:0.0}s":""))+
                    (healing==null?"":$"\nDuct Tape {healing.DuctTape:0.0}"+(healing.IsHealing?"\nHEALING":healing.WaitingForRelease?"\nINTERRUPTED — release heal":healing.CooldownRemaining>0?"\nHEAL INTERRUPTED":"\nHold H / HEAL"));
                loadoutLabel.style.color=healing!=null&&healing.IsHealing?new Color(.35f,1,.55f):Color.white;
                if(healButton!=null){healButton.text=healing!=null&&healing.IsHealing?"HEALING":"HOLD\nHEAL";healButton.EnableInClassList("healing",healing!=null&&healing.IsHealing);}
                if(supplyButton!=null)supplyButton.EnableInClassList("hidden",supplies==null||supplies.NearbyCrate==null);
                if(suppliesNotice!=null)suppliesNotice.text=supplies!=null?supplies.Notification:"";
            }
            if(reviveOverlay!=null)
            {
                bool offer=revive!=null&&revive.CanOfferRevive;
                reviveOverlay.style.display=offer?DisplayStyle.Flex:DisplayStyle.None;
                if(offer){reviveButton.text=$"INJECT FAINTANYL — REVIVE ({revive.Injections}/2)";if(Time.timeScale>0)reviveOverlay.BringToFront();}
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
            ReleaseFire();
            ReleaseHeal();
            ReleaseJump();
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
            blindFireButton?.UnregisterCallback<PointerDownEvent>(FireDown);
            blindFireButton?.UnregisterCallback<PointerUpEvent>(FireUp);
            blindFireButton?.UnregisterCallback<PointerCancelEvent>(FireCancel);
            blindFireButton?.UnregisterCallback<PointerCaptureOutEvent>(FireCaptureLost);
            pressureFrame?.RemoveFromHierarchy();pressureFrame=null;
            reviveOverlay?.RemoveFromHierarchy();reviveOverlay=null;
            CancelInput();
            if (subscribedActions != null) subscribedActions.InputReset -= ClearPointers;
            subscribedActions = null;
            moveStick?.Dispose(); aimStick?.Dispose();
            if (root == null) return;
            safeArea?.UnregisterCallback<GeometryChangedEvent>(LayoutControls);
            jumpButton?.UnregisterCallback<PointerDownEvent>(JumpDown);
            jumpButton?.UnregisterCallback<PointerUpEvent>(JumpUp);
            jumpButton?.UnregisterCallback<PointerCancelEvent>(JumpCancel);
            jumpButton?.UnregisterCallback<PointerCaptureOutEvent>(JumpLost);
            root.Q<Button>("slot1")?.UnregisterCallback<ClickEvent>(SelectFirst);
            root.Q<Button>("slot2")?.UnregisterCallback<ClickEvent>(SelectSecond);
            root.Q<Button>("melee")?.UnregisterCallback<ClickEvent>(SelectMelee);
            root.Q<Button>("reload")?.UnregisterCallback<ClickEvent>(Reload);
            healButton?.UnregisterCallback<PointerDownEvent>(HealDown,TrickleDown.TrickleDown);
            healButton?.UnregisterCallback<PointerUpEvent>(HealUp,TrickleDown.TrickleDown);
            healButton?.UnregisterCallback<PointerCancelEvent>(HealCancel);
            healButton?.UnregisterCallback<PointerCaptureOutEvent>(HealLost);
            supplyButton?.UnregisterCallback<ClickEvent>(Supply);
            fireButton?.UnregisterCallback<PointerDownEvent>(FireDown);
            fireButton?.UnregisterCallback<PointerUpEvent>(FireUp);
            fireButton?.UnregisterCallback<PointerCancelEvent>(FireCancel);
            fireButton?.UnregisterCallback<PointerCaptureOutEvent>(FireCaptureLost);
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
