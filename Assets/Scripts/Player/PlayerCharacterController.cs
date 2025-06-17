using Assets.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player
{
    public class PlayerCharacterController : MonoBehaviour, IDamagable
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
        CharacterController controller;
        private ArenaManager arenaManager;
        float cameraVerticalAngle = 0f;
        float footstepDistanceCounter;
        bool isDead = false;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            gui = PlayerCamera.GetComponent<PlayerGUI>();
            arenaManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GetComponent<ArenaManager>();
        }

        void FixedUpdate()
        {
            DestroyOnFall();
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
            IsGrounded = controller.isGrounded;
        }

        void HandleCharacterMovement()
        {
            if (isDead) return;
            {
                transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X"), 0f), Space.Self);
            }

            {
                cameraVerticalAngle -= Input.GetAxis("Mouse Y");

                cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

                PlayerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
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
                    if (footstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
                    {
                        footstepDistanceCounter = 0f;
                        AudioSource.PlayOneShot(FootstepSfx);
                    }

                    footstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
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

            controller.Move(CharacterVelocity * Time.deltaTime);
        }

        public void ReceiveDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
                Die();
        }

        public void Die()
        {
            isDead = true;
            gui.Death = true;
        }

        void DestroyOnFall()
        {
            if (transform.position.y < arenaManager.getKillHeight())
                Die();
        }

        void Revive()
        {
            Health = MaxHealth;
            isDead = false;
            gui.Death = false;
            transform.position = new Vector3(32, arenaManager.heightMap[7, 7] + 1, 32);
            CharacterVelocity = new Vector3();
        }
    }
}