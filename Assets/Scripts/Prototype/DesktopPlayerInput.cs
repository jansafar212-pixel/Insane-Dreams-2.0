using UnityEngine;
using UnityEngine.InputSystem;

namespace InsaneDreams.Prototype
{
    [RequireComponent(typeof(PlayerMotor2D))]
    public sealed class DesktopPlayerInput : MonoBehaviour
    {
        private PlayerMotor2D motor;
        private void Awake() => motor = GetComponent<PlayerMotor2D>();

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) { motor.SetMoveInput(0); return; }
            bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            motor.SetMoveInput((right ? 1f : 0f) - (left ? 1f : 0f));
            if (keyboard.spaceKey.wasPressedThisFrame) motor.RequestJump();
        }

        private void OnDisable() { if (motor != null) motor.SetMoveInput(0); }
        private void OnApplicationFocus(bool focused) { if (!focused) motor.SetMoveInput(0); }
    }
}
