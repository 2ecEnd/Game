using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public float LookSensitivity = 1f;

        private InputAction m_MoveAction;
        private InputAction m_LookAction;
        private InputAction m_JumpAction;
        private InputAction m_FireAction;
        private InputAction m_AimAction;
        private InputAction m_SprintAction;
        private InputAction m_CrouchAction;
        private InputAction m_ReloadAction;
        private InputAction m_NextWeaponAction;

        bool m_FireInputWasHeld;

        void LateUpdate()
        {
            m_FireInputWasHeld = GetFireInputHeld();
        }


        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_LookAction = InputSystem.actions.FindAction("Player/Look");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            m_FireAction = InputSystem.actions.FindAction("Player/Fire");
            m_AimAction = InputSystem.actions.FindAction("Player/Aim");
            m_SprintAction = InputSystem.actions.FindAction("Player/Sprint");
            m_CrouchAction = InputSystem.actions.FindAction("Player/Crouch");
            m_ReloadAction = InputSystem.actions.FindAction("Player/Reload");
            m_NextWeaponAction = InputSystem.actions.FindAction("Player/NextWeapon");

            m_MoveAction.Enable();
            m_LookAction.Enable();
            m_JumpAction.Enable();
            m_FireAction.Enable();
            m_AimAction.Enable();
            m_SprintAction.Enable();
            m_CrouchAction.Enable();
            m_ReloadAction.Enable();
            m_NextWeaponAction.Enable();
        }

        public bool CanProcessInput()
        {
            return Cursor.lockState == CursorLockMode.Locked;
        }

        public Vector3 GetMoveInput()
        {
            if (CanProcessInput())
            {
                var input = m_MoveAction.ReadValue<Vector2>();
                Vector3 move = new Vector3(input.x, 0f, input.y);

                move = Vector3.ClampMagnitude(move, 1);

                return move;
            }

            return Vector3.zero;
        }

        public float GetLookInputsHorizontal()
        {
            if (!CanProcessInput())
                return 0.0f;

            float input = m_LookAction.ReadValue<Vector2>().x;

            input *= LookSensitivity;

            return input;
        }

        public float GetLookInputsVertical()
        {
            if (!CanProcessInput())
                return 0.0f;

            float input = m_LookAction.ReadValue<Vector2>().y;

            input *= LookSensitivity;

            return input;
        }

        public bool GetJumpInputDown()
        {
            if (CanProcessInput())
            {
                return m_JumpAction.WasPressedThisFrame();
            }

            return false;
        }

        public bool GetSprintInputHeld()
        {
            if (CanProcessInput())
            {
                return m_SprintAction.IsPressed();
            }

            return false;
        }

        public bool GetReloadButtonDown()
        {
            if (CanProcessInput())
            {
                return m_ReloadAction.WasPressedThisFrame();
            }

            return false;
        }

        public bool GetFireInputDown()
        {
            return GetFireInputHeld() && !m_FireInputWasHeld;
        }

        public bool GetFireInputHeld()
        {
            if (CanProcessInput())
            {
                return m_FireAction.IsPressed();
            }

            return false;
        }
    }
}
