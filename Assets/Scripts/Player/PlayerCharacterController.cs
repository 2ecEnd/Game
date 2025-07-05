using System.Collections.Generic;
using Assets.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Assets.Scripts.Player
{
    public class PlayerCharacterController : MonoBehaviour, IDamagable
    {
        [Header("References")]
        public Camera PlayerCamera;
        public PlayerUI PlayerUI;
        public AudioSource AudioSource;

        [Header("General")]
        public float GravityDownForce = 20f;
        public float MaxHealth = 100f;
        public float Health = 100f;
        public float tiltAmount = 5f;
        public float tiltSpeed = 2f;

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

        [Header("Dash")]
        public int DashMaxCount = 2;
        public int DashCount;
        public float DashForce = 150;
        public float DashReload = 5;
        private float DashReloadTime;

        [Header("Stance")]
        public float CameraHeightRatio = 0.9f;
        public float CapsuleHeightStanding = 1.8f;
        public float CapsuleHeightCrouching = 0.9f;
        public float CrouchingSharpness = 10f;

        [Header("SFX")]
        public AudioClip HealSound;
        public AudioClip FootstepSfx;
        public AudioClip JumpSfx;
        public AudioClip LandSfx;
        public List<AudioClip> ReceiveDamageSfx;
        public AudioClip ReceiveBigDamageSfx;
        public float FootstepSfxFrequency = 1f;
        //public float SprintingSfxFrequency = 1f;


        public Vector3 CharacterVelocity { get; set; }
        public Vector3 ExtraVelocity { get; set; }
        public bool IsGrounded { get; private set; }

        bool canSecondJump;
        PlayerGUI gui;
        CharacterController controller;
        private PlayerWeaponManager playerWeaponManager;
        private ArenaManager arenaManager;
        float cameraVerticalAngle = 0f;
        float footstepDistanceCounter;
        bool isDead = false;
        bool needRevive = false;
        private Vector3 cameraStartPos;

        void Start()
        {
            GlobalInspector.PlayerCharacterController = this;
            controller = GetComponent<CharacterController>();
            gui = PlayerCamera.GetComponent<PlayerGUI>();
            arenaManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GetComponent<ArenaManager>();
            playerWeaponManager = gameObject.GetComponent<PlayerWeaponManager>();
            DashCount = DashMaxCount;
            cameraStartPos = PlayerCamera.transform.localPosition;
        }

        void FixedUpdate()
        {
            if (isDead)
            {
                if (PlayerCamera.transform.localPosition.magnitude < 5)
                {
                    PlayerCamera.transform.localPosition -= Vector3.forward * Time.fixedDeltaTime;
                }
                PlayerCamera.transform.RotateAround(transform.position, transform.up, 20 * Time.fixedDeltaTime);
            }
            else
            {
                PlayerCamera.transform.localPosition = cameraStartPos;
            }
            if (needRevive)
            {
                Revive();
                needRevive = false;
            }
            if (!GlobalInspector.PlayerAlive)
            {
                return;
            }
            DestroyOnFall();
        }

        void Update()
        {
            if (!GlobalInspector.PlayerAlive)
            {
                return;
            }
            bool wasGrounded = IsGrounded;
            IsGrounded = controller.isGrounded;
            if (IsGrounded && !wasGrounded)
            {
                //AudioSource.PlayOneShot(LandSfx);
            }
            HandleCharacterMovement();
        }

        void HandleCharacterMovement()
        {
            if (isDead) return;

            transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * GlobalInspector.MouseSensitivity, 0f), Space.Self);

            cameraVerticalAngle -= Input.GetAxis("Mouse Y")*GlobalInspector.MouseSensitivity;
            cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);
            PlayerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, PlayerCamera.transform.localEulerAngles.z);

            Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            Vector3 worldspaceMoveInput = transform.TransformVector(Vector3.ClampMagnitude(inputVector, 1));

            ExtraVelocity = Vector3.Lerp(ExtraVelocity, Vector3.zero, MovementSharpnessOnGround * Time.deltaTime);
            if (Input.GetKeyDown(KeyCode.LeftShift) && DashCount > 0) //�����
            {
                ExtraVelocity += worldspaceMoveInput * DashForce;
                if (DashCount == DashMaxCount)
                {
                    DashReloadTime = Time.time + DashReload;
                }
                DashCount--;
            }
            if (Time.time > DashReloadTime && DashCount < DashMaxCount)
            {
                DashCount++;
                DashReloadTime = Time.time + DashReload;
            }

            float verticalVelocity = CharacterVelocity.y - GravityDownForce * Time.deltaTime;
            
            PlayerCamera.transform.localRotation  = Quaternion.Lerp
            (
                PlayerCamera.transform.localRotation ,
                Quaternion.Euler(PlayerCamera.transform.localEulerAngles.x, 0, -tiltAmount * inputVector.x),
                tiltSpeed * Time.deltaTime
            );
            
            if (controller.isGrounded)
            {
                canSecondJump = true;
                CharacterVelocity = Vector3.Lerp(CharacterVelocity, worldspaceMoveInput * MaxSpeedOnGround, MovementSharpnessOnGround * Time.deltaTime); //����������� �������� � ������������ ��������
                if (Input.GetKeyDown(KeyCode.Space)) //������
                {
                    verticalVelocity = JumpForce;
                    //IsGrounded = false;
                    AudioSource.PlayOneShot(JumpSfx);
                }
                else
                {
                    verticalVelocity = -GravityDownForce * 0.1f;
                }
                //float chosenFootstepSfxFrequency = (isSprinting ? SprintingSfxFrequency : FootstepSfxFrequency);
                if (footstepDistanceCounter >= 1f / FootstepSfxFrequency)
                {
                    footstepDistanceCounter = 0f;
                    AudioSource.PlayOneShot(FootstepSfx);
                }
                footstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
            }
            else
            {
                CharacterVelocity = Vector3.Lerp(CharacterVelocity, worldspaceMoveInput * MaxSpeedInAir, AccelerationSpeedInAir * Time.deltaTime); //����������� �������� � ������������ ��������
                if (Input.GetKeyDown(KeyCode.Space) && canSecondJump) //������
                {
                    verticalVelocity = JumpForce;
                    canSecondJump = false;
                    AudioSource.PlayOneShot(JumpSfx);
                }
            }
            CharacterVelocity = new Vector3(CharacterVelocity.x, verticalVelocity, CharacterVelocity.z);
            controller.Move((CharacterVelocity + ExtraVelocity) * Time.deltaTime); //�������� ������
        }

        public void ReceiveDamage(float damage)
        {
            Health -= damage;
            if (damage > 0)
                PlayerUI.TakeDamage();

            if (Health <= 0 && !isDead)
            {
                GlobalInspector.PlayerDeath();
            }
            else if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
            if (damage > 0 && damage < 50)
                AudioSource.PlayOneShot(ReceiveDamageSfx[Random.Range(0, ReceiveDamageSfx.Count)]);
            else if (damage >= 50)
                AudioSource.PlayOneShot(ReceiveBigDamageSfx);
        }

        public void Die(bool needScored = true)
        {
            isDead = true;
        }

        void DestroyOnFall()
        {
            if (transform.position.y < arenaManager.GetKillHeight() && !isDead)
                GlobalInspector.PlayerDeath();
        }
        public void PRevive()
        {
            needRevive = true;
        }

        void Revive()
        {
            Health = MaxHealth;
            DashCount = DashMaxCount;
            playerWeaponManager.ActiveWeapon.CurrentAmmo = playerWeaponManager.ActiveWeapon.MagazineSize;
            isDead = false;

            int arenaCenter = (int)(arenaManager.GetArenaSize() * arenaManager.GetChunkScale() / 2);

            RaycastHit ray;
            Physics.Raycast(
                origin: new Vector3(arenaCenter , 100, arenaCenter),
                direction: new Vector3(0, -1, 0),
                hitInfo: out ray,
                maxDistance: 150f
            );

            transform.position = ray.point;
            CharacterVelocity = new Vector3();
        }

        public void PlayHealSound()
        {
            AudioSource.PlayOneShot(HealSound);
        }
    }
}