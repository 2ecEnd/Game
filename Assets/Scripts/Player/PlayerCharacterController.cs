using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player
{
    public class PlayerCharacterController : MonoBehaviour
    {
        public Camera PlayerCamera;
        public float SprintSpeedModifier = 2f;
        public float MaxSpeedOnGround = 10f;
        public float MovementSharpnessOnGround = 15;

        public Vector3 CharacterVelocity { get; set; }

        PlayerInputHandler m_InputHandler;
        CharacterController m_Controller;
        float m_CameraVerticalAngle = 0f;

        void Start()
        {
            m_Controller = GetComponent<CharacterController>();
            m_InputHandler = GetComponent<PlayerInputHandler>();
        }

        void Update()
        {
            HandleCharacterMovement();
        }

        void HandleCharacterMovement()
        {
            // horizontal character rotation
            {
                transform.Rotate(
                    new Vector3(0f, (m_InputHandler.GetLookInputsHorizontal()),
                        0f), Space.Self);
            }

            // vertical camera rotation
            {
                m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical();

                m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

                PlayerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
            }

            // character movement handling
            bool isSprinting = m_InputHandler.GetSprintInputHeld();
            {
                float speedModifier = isSprinting ? SprintSpeedModifier : 1f;

                Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());

                Vector3 targetVelocity = worldspaceMoveInput * MaxSpeedOnGround * speedModifier;

                CharacterVelocity = Vector3.Lerp(CharacterVelocity, targetVelocity,
                        MovementSharpnessOnGround * Time.deltaTime);
            }

            m_Controller.Move(CharacterVelocity * Time.deltaTime);
        }
    }
}