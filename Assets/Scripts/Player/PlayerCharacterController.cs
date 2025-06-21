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
        public float MaxHealth = 100f;
        public float Health = 100f;

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
        public float DashForce = 10;
        public float DashReload = 5;
        private float DashReloadTime;

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
        public Vector3 ExtraVelocity { get; set; }
        public bool IsGrounded { get; private set; }

        bool canSecondJump;
        PlayerGUI gui;
        CharacterController controller;
        private PlayerWeaponManager playerWeaponManager;
        private ArenaManager arenaManager;
        float cameraVerticalAngle = 0f;
        //float footstepDistanceCounter;
        bool isDead = false;
        bool needRevive = false;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            gui = PlayerCamera.GetComponent<PlayerGUI>();
            arenaManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GetComponent<ArenaManager>();
            playerWeaponManager = gameObject.GetComponent<PlayerWeaponManager>();
            DashCount = DashMaxCount;
        }

        void FixedUpdate()
        {
            DestroyOnFall();


            if (needRevive)
            {
                Revive();
                needRevive = false;
            }
        }

        void Update()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = controller.isGrounded;
            if (IsGrounded && !wasGrounded)
            {
                //AudioSource.PlayOneShot(LandSfx);
            }
            HandleCharacterMovement();

            if (Input.GetKeyDown(KeyCode.T))
                PRevive();
        }

        void HandleCharacterMovement()
        {
            if (isDead) return; //мёртвые не двигаются)

            transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X"), 0f), Space.Self); //поворот игрока по оси Y

            cameraVerticalAngle -= Input.GetAxis("Mouse Y");
            cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);
            PlayerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);//поворот камеры по оси X

            Vector3 worldspaceMoveInput = transform.TransformVector(Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), 1)); //направление ускорения движения

            ExtraVelocity = Vector3.Lerp(ExtraVelocity, Vector3.zero, MovementSharpnessOnGround * Time.deltaTime); //Дополнительное направление движения без ограничений скорости
            if (Input.GetKeyDown(KeyCode.LeftShift) && DashCount > 0) //рывок
            {
                ExtraVelocity += worldspaceMoveInput * DashForce;
                if(DashCount == DashMaxCount)
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
            if (controller.isGrounded)
            {
                canSecondJump = true;
                CharacterVelocity = Vector3.Lerp(CharacterVelocity, worldspaceMoveInput * MaxSpeedOnGround, MovementSharpnessOnGround * Time.deltaTime); //направление движения с ограничением скорости
                if (Input.GetKeyDown(KeyCode.Space)) //прыжок
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
                //if (footstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
                //{
                //    footstepDistanceCounter = 0f;
                //    AudioSource.PlayOneShot(FootstepSfx);
                //}
                //footstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
            }
            else
            {
                CharacterVelocity = Vector3.Lerp(CharacterVelocity, worldspaceMoveInput * MaxSpeedInAir, AccelerationSpeedInAir * Time.deltaTime); //направление движения с ограничением скорости
                if (Input.GetKeyDown(KeyCode.Space) && canSecondJump) //прыжок
                {
                    verticalVelocity = JumpForce;
                    canSecondJump = false;
                    AudioSource.PlayOneShot(JumpSfx);
                }
            }
            CharacterVelocity = new Vector3(CharacterVelocity.x, verticalVelocity, CharacterVelocity.z);
            controller.Move((CharacterVelocity + ExtraVelocity) * Time.deltaTime); //движение игрока
        }

        public void ReceiveDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0 && !isDead)
            {
                Die();
            }
            else if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }

        public void Die(bool needScored = true)
        {
            GlobalInspector.PlayerDeath();
            isDead = true;

            if (needScored)
                GlobalInspector.DeathCount++;
        }

        void DestroyOnFall()
        {
            if (transform.position.y < arenaManager.GetKillHeight() && !isDead)
                Die();
        }
        public void PRevive()
        {
            GlobalInspector.PlayerRevive();
            needRevive = true;
        }

        void Revive()
        {
            if (!isDead)
                Die();
            GlobalInspector.PlayerRevive();
            Health = MaxHealth;
            DashCount = DashMaxCount;
            playerWeaponManager.ActiveWeapon.CurrentAmmo = playerWeaponManager.ActiveWeapon.MagazineSize;
            isDead = false;
            gui.Death = false;

            int arenaCenter = arenaManager.GetArenaSize() / 2;
            float y = arenaManager.heightMap[arenaCenter - 1, arenaCenter - 1];
            y = Mathf.Max(y, arenaManager.heightMap[arenaCenter, arenaCenter - 1]);
            y = Mathf.Max(y, arenaManager.heightMap[arenaCenter - 1, arenaCenter]);
            y = Mathf.Max(y, arenaManager.heightMap[arenaCenter, arenaCenter]);
            if (y == 0)
            {
                y = Mathf.Max(y, arenaManager.heightMap[arenaCenter - 2, arenaCenter - 1]);
                y = Mathf.Max(y, arenaManager.heightMap[arenaCenter - 2, arenaCenter]);
                y = Mathf.Max(y, arenaManager.heightMap[arenaCenter - 1, arenaCenter - 2]);
                y = Mathf.Max(y, arenaManager.heightMap[arenaCenter, arenaCenter - 2]);
                y = Mathf.Max(y, arenaManager.heightMap[arenaCenter + 1, arenaCenter - 1]);
                y = Mathf.Max(y, arenaManager.heightMap[arenaCenter + 1, arenaCenter]);
                y = Mathf.Max(y, arenaManager.heightMap[arenaCenter - 1, arenaCenter + 1]);
                y = Mathf.Max(y, arenaManager.heightMap[arenaCenter, arenaCenter + 1]);
            }
            y += 0.1f;
            transform.position = new Vector3(
                arenaManager.GetArenaSize() * arenaManager.GetChunkScale() / 2 - arenaManager.GetChunkScale() / 2,
                y,
                arenaManager.GetArenaSize() * arenaManager.GetChunkScale() / 2 - arenaManager.GetChunkScale() / 2);
            CharacterVelocity = new Vector3();
        }
    }
}