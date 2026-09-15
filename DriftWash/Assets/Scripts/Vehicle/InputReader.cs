using UnityEngine;
using UnityEngine.InputSystem;

namespace DriftWash
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "DriftWash/Input Reader")]
    public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
    {
        public Vector2 Move => inputActions.Player.Move.ReadValue<Vector2>();
        public bool IsBraking => inputActions.Player.Brake.ReadValue<float>() > 0;
        public bool IsSprinting => inputActions.Player.Sprint.ReadValue<float>() > 0;

        private InputSystem_Actions inputActions;

        void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new InputSystem_Actions();
                inputActions.Player.SetCallbacks(this);
            }
        }

        public void Enable() { inputActions.Enable(); }

        // Active Actions
        public void OnMove(InputAction.CallbackContext context) { }
        public void OnLook(InputAction.CallbackContext context) { }
        public void OnFire(InputAction.CallbackContext context) { }
        public void OnBrake(InputAction.CallbackContext context) { }

        // Required Interface Actions (Left Blank to Fix Errors
        public void OnAttack(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context) { }
        public void OnSprint(InputAction.CallbackContext context) { }
    }
}
