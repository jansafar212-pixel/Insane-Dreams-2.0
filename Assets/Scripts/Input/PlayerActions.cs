using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InsaneDreams.Input
{
    [RequireComponent(typeof(PlayerMotor2D), typeof(PlayerTraversal2D))]
    public sealed class PlayerActions : MonoBehaviour
    {
        [SerializeField] private bool readKeyboard = true;
        [SerializeField] private Transform aimIndicator;
        [SerializeField] private bool debugAimMarker;
        [SerializeField] private JoystickJumpGesture jumpGesture = new JoystickJumpGesture();
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private PlayerPosture2D posture;
        private Vector2 touchMove;
        private Vector2 touchAim;
        private bool touchParachute;
        private bool touchFire;
        private Camera aimCamera;
        public event System.Action InputReset;
        public event System.Action ReloadRequested;
        public event System.Action InteractionRequested;
        public event System.Action HealRequested;
        public event System.Action<InsaneDreams.Inventory.EquipmentSlot> WeaponSelectionRequested;
        public Vector2 Move { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public bool FireHeld { get; private set; }
        public bool HasAimIntent { get; private set; }

        private void Awake()
        {
            motor = GetComponent<PlayerMotor2D>();
            traversal = GetComponent<PlayerTraversal2D>();
            posture = GetComponent<PlayerPosture2D>();
            aimCamera = Camera.main;
            if (aimIndicator != null) aimIndicator.gameObject.SetActive(debugAimMarker);
        }

        public void SetTouchMove(Vector2 value) => touchMove = Vector2.ClampMagnitude(value, 1);
        public void SetTouchAim(Vector2 value) => touchAim = Vector2.ClampMagnitude(value, 1);
        public void SetTouchParachute(bool held)
        {
            if (held && !touchParachute) traversal.ToggleParachute();
            touchParachute = held;
        }
        public void SetFireIntent(bool held) => touchFire = held;
        public void RequestReload() => ReloadRequested?.Invoke();
        public void RequestInteraction() => InteractionRequested?.Invoke();
        public void RequestHeal() => HealRequested?.Invoke();
        public void BindAimCamera(Camera camera) => aimCamera = camera;
        public void SelectWeapon(InsaneDreams.Inventory.EquipmentSlot slot) => WeaponSelectionRequested?.Invoke(slot);
        public void Jump() { if (posture == null || posture.PrepareJump()) traversal.Jump(); }

        private void Update()
        {
            Vector2 keyboardMove = Vector2.zero;
            bool keyboardParachute = false;
            bool desktopFire = false;
            Keyboard key = readKeyboard ? Keyboard.current : null;
            if (key != null && Application.isFocused)
            {
                keyboardMove.x = (key.dKey.isPressed || key.rightArrowKey.isPressed ? 1 : 0)
                    - (key.aKey.isPressed || key.leftArrowKey.isPressed ? 1 : 0);
                keyboardMove.y = (key.wKey.isPressed || key.upArrowKey.isPressed ? 1 : 0)
                    - (key.sKey.isPressed || key.downArrowKey.isPressed ? 1 : 0);
                keyboardParachute = key.leftShiftKey.wasPressedThisFrame;
                if (key.spaceKey.wasPressedThisFrame) Jump();
                if (key.rKey.wasPressedThisFrame) RequestReload();
                if (key.eKey.wasPressedThisFrame) RequestInteraction();
                if (key.hKey.wasPressedThisFrame) RequestHeal();
                desktopFire = key.fKey.isPressed;
                if (key.digit1Key.wasPressedThisFrame) SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Firearm1);
                if (key.digit2Key.wasPressedThisFrame) SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Firearm2);
                if (key.digit3Key.wasPressedThisFrame) SelectWeapon(InsaneDreams.Inventory.EquipmentSlot.Melee);
            }
            Move = touchMove.sqrMagnitude > 0.01f ? touchMove : keyboardMove;
            if (keyboardParachute) traversal.ToggleParachute();
            posture?.SetInput(Move);
            bool usableLadder = jumpGesture.NeedsDecision(touchMove) && traversal.CanUseLadder;
            // Consume airborne jump gestures without queuing a jump that could execute upon landing.
            if (jumpGesture.Evaluate(touchMove, usableLadder, traversal.IsClimbing, motor.IsGrounded) && motor.IsGrounded) Jump();
            motor.SetMoveInput(Move.x * traversal.HorizontalSpeedMultiplier * (posture != null ? posture.SpeedMultiplier : 1));
            Vector2 traversalMove = Move;
            // A jump consumes its edge, not ongoing airborne climb intent. Traversal still owns
            // reachable-candidate checks, smooth alignment and post-dismount lockout/release.
            bool airborneClimbIntent = !motor.IsGrounded && traversal.HasUpwardClimbIntent(traversalMove);
            if (touchMove.sqrMagnitude > 0.01f && jumpGesture.ClimbSuppressed && !airborneClimbIntent) traversalMove.y = 0;
            if (posture != null && !traversal.IsClimbing && motor.IsGrounded && Move.y < 0 && !traversal.CanEnterFromTop(Move)) traversalMove.y = 0;
            traversal.SetInput(traversalMove, false);
            var mouse = readKeyboard && Application.isFocused ? Mouse.current : null;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                if (aimCamera != null && touchAim.sqrMagnitude <= .04f)
                {
                    Vector2 point = aimCamera.ScreenToWorldPoint(mouse.position.ReadValue());
                    Vector2 direction = point - (posture != null ? posture.WorldCenter : (Vector2)transform.position);
                    if (direction.sqrMagnitude > .01f) AimDirection = direction.normalized;
                }
                desktopFire |= mouse.leftButton.isPressed;
            }
            FireHeld = touchFire || desktopFire;
            HasAimIntent = touchAim.sqrMagnitude > .04f || mouse != null && mouse.rightButton.isPressed;
            if (touchAim.sqrMagnitude > 0.04f) AimDirection = touchAim.normalized;
            if (aimIndicator != null)
            {
                aimIndicator.gameObject.SetActive(debugAimMarker);
                aimIndicator.localPosition = (Vector3)(posture != null ? posture.LocalCenter : Vector2.zero) + new Vector3(AimDirection.x * 0.85f, AimDirection.y * 0.85f, 0);
                aimIndicator.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg);
            }
        }

        public void ResetActions() => ResetInput(true);
        public void SuspendInput() => ResetInput(false);
        private void ResetInput(bool resetDeployment)
        {
            touchMove = touchAim = Move = Vector2.zero;
            touchParachute = false;
            touchFire = FireHeld = false;
            HasAimIntent = false;
            jumpGesture.Reset();
            if (motor != null) motor.SetMoveInput(0);
            if (traversal != null) traversal.SetInput(Vector2.zero, false);
            if (resetDeployment && traversal != null) traversal.ClearParachute();
            posture?.SetInput(Vector2.zero);
            InputReset?.Invoke();
        }

        private void OnApplicationFocus(bool focused) { if (!focused) ResetInput(false); }
        private void OnApplicationPause(bool paused) { if (paused) ResetInput(false); }
        private void OnDisable() => ResetActions();
    }
}
