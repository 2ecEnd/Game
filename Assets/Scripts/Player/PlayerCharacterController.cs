using Assets.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player
{
    public class PlayerCharacterController : MonoBehaviour
    {
        [Header("References")]
        public Camera PlayerCamera;

        public AudioSource AudioSource;

        [Header("General")]
        public float GravityDownForce = 20f;
        public float MaxHealth = 20f;
        public float Health = 20f;

        [Header("Movement")]
        public float MaxSpeedOnGround = 10f;

        public float MovementSharpnessOnGround = 15;

        [Range(0, 1)]
        public float MaxSpeedCrouchedRatio = 0.5f;

        public float MaxSpeedInAir = 10f;

        public float AccelerationSpeedInAir = 25f;

        public float SprintSpeedModifier = 2f;

        public float KillHeight = -50f;

        [Header("Rotation")]
        public float RotationSpeed = 200f;

        [Range(0.1f, 1f)]
        public float AimingRotationMultiplier = 0.4f;

        [Header("Jump")]
        public float JumpForce = 9f;

        [Header("Stance")]
        public float CameraHeightRatio = 0.9f;

        public float CapsuleHeightStanding = 1.8f;

        public float CapsuleHeightCrouching = 0.9f;

        public float CrouchingSharpness = 10f;

        [Header("SFX")]
        public AudioClip FootstepSfx;
        public AudioClip JumpSfx;
        public AudioClip LandSfx;
        public float FootstepSfxFrequency = 1f;
        public float SprintingSfxFrequency = 1f;


        public Vector3 CharacterVelocity { get; set; }
        public bool IsGrounded { get; private set; }

        PlayerGUI gui;
        CharacterController m_Controller;
        private ArenaManager m_ArenaManager;
        float m_CameraVerticalAngle = 0f;
        float m_FootstepDistanceCounter;
        bool m_isDead = false;

        void Start()
        {
            m_Controller = GetComponent<CharacterController>();
            gui = PlayerCamera.GetComponent<PlayerGUI>();
            m_ArenaManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GetComponent<ArenaManager>();
        }

        void FixedUpdate()
        {
            if (IsFall())
                TakeDamage(MaxHealth);
        }

        void Update()
        {
            bool wasGrounded = IsGrounded;
            GroundCheck();
            if (IsGrounded && !wasGrounded)
            {
                //AudioSource.PlayOneShot(LandSfx);
            }
            HandleCharacterMovement();

            if (Input.GetKey(KeyCode.T))
                Revive();
        }

        void GroundCheck()
        {
            IsGrounded = m_Controller.isGrounded;
        }

        void HandleCharacterMovement()
        {
            if (m_isDead) return;
            {
                transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X"), 0f), Space.Self);
            }

            {
                m_CameraVerticalAngle -= Input.GetAxis("Mouse Y");

                m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

                PlayerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
            }

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            {
                float speedModifier = isSprinting ? SprintSpeedModifier : 1f;

                Vector3 worldspaceMoveInput = transform.TransformVector(Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), 1));

                if (IsGrounded)
                {
                    Vector3 targetVelocity = worldspaceMoveInput * MaxSpeedOnGround * speedModifier;

                    CharacterVelocity = Vector3.Lerp(CharacterVelocity, targetVelocity,
                            MovementSharpnessOnGround * Time.deltaTime);

                    if (Input.GetKey(KeyCode.Space))
                    {
                        {
                            CharacterVelocity = new Vector3(CharacterVelocity.x, 0f, CharacterVelocity.z);

                            CharacterVelocity += Vector3.up * JumpForce;

                            IsGrounded = false;

                            AudioSource.PlayOneShot(JumpSfx);
                        }
                    }

                    float chosenFootstepSfxFrequency =
                        (isSprinting ? SprintingSfxFrequency : FootstepSfxFrequency);
                    if (m_FootstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
                    {
                        m_FootstepDistanceCounter = 0f;
                        AudioSource.PlayOneShot(FootstepSfx);
                    }

                    m_FootstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
                }
                else
                {
                    CharacterVelocity += worldspaceMoveInput * AccelerationSpeedInAir * Time.deltaTime;

                    float verticalVelocity = CharacterVelocity.y;
                    Vector3 horizontalVelocity = Vector3.ProjectOnPlane(CharacterVelocity, Vector3.up);
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir * speedModifier);
                    CharacterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                    CharacterVelocity += Vector3.down * GravityDownForce * Time.deltaTime;
                }
            }

            m_Controller.Move(CharacterVelocity * Time.deltaTime);
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                m_isDead = true;
                gui.Death = true;
            }
        }

        void Revive()
        {
            Health = MaxHealth;
            m_isDead = false;
            gui.Death = false;
            transform.position = new Vector3(32, m_ArenaManager.heightMap[7, 7] + 1, 32);
            CharacterVelocity = new Vector3();
        }

        bool IsFall()
        {
            return transform.position.y < m_ArenaManager.getKillHeight();
        }
    }
}