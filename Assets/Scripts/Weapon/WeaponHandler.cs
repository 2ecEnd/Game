using UnityEngine;

namespace Assets.Scripts
{
    public class WeaponHandler : MonoBehaviour
    {
        public enum WeaponShootType
        {
            Manual,
            Automatic,
        }

        [Header("Shoot Parameters")]
        public float FireRate = 750;
        public float BulletSpreadAngle;
        public int BulletsPerShot = 1;
        public WeaponShootType ShootType;
        public Transform WeaponMuzzle;
        public ProjectileBase ProjectilePrefab;

        [Header("Ammo Parameters")]
        public int MagazineSize = 30;
        public float AmmoReloadRate;

        [Header("Recoil Parameters")]
        public float RecoilForce = 1f;

        public GameObject Owner { get; set; }
        public Vector3 MuzzleWorldVelocity { get; private set; }

        [Header("Audio")] 
        public AudioClip ShootSfx;
        public AudioClip MisfireSfx;
        public AudioClip ReloadingSfx;

        float m_LastTimeShot = Mathf.NegativeInfinity;
        float m_LastTimeReloading = Mathf.NegativeInfinity;
        int m_CurrentAmmo;
        Vector3 m_LastMuzzlePosition;
        AudioSource m_ShootAudioSource;
        bool m_inputHeldCurrentFrame = false;
        bool m_inputWasHeld = false;

        void Start()
        {
            m_ShootAudioSource = GetComponent<AudioSource>();
            m_CurrentAmmo = MagazineSize;
            m_LastMuzzlePosition = WeaponMuzzle.position;
        }

        void Update()
        {
            if (Time.deltaTime > 0)
            {
                MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
                m_LastMuzzlePosition = WeaponMuzzle.position;
            }
        }

        void LateUpdate()
        {
            m_inputWasHeld = m_inputHeldCurrentFrame;
        }

        public bool HandleShootInputs(bool inputDown, bool inputHeld)
        {
            m_inputHeldCurrentFrame = inputHeld;
            switch (ShootType)
            {
                case WeaponShootType.Manual:
                    if (inputDown)
                    {
                        return TryShoot();
                    }

                    return false;

                case WeaponShootType.Automatic:
                    if (inputHeld)
                    {
                        return TryShoot();
                    }

                    return false;

                default:
                    return false;
            }
        }

        public void HandleReload(bool input)
        {
            if (input && m_LastTimeReloading + AmmoReloadRate < Time.time)
            {
                m_CurrentAmmo = MagazineSize;
                m_LastTimeReloading = Time.time;
                m_ShootAudioSource.PlayOneShot(ReloadingSfx);
            }
        }

        bool TryShoot()
        {
            if (m_LastTimeReloading + AmmoReloadRate >= Time.time) return false;

            float DelayBetweenShots = 60f / FireRate;
            if (m_CurrentAmmo >= 1
                && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                HandleShoot();
                m_CurrentAmmo -= 1;
                if (m_CurrentAmmo == 0) m_inputHeldCurrentFrame = false;

                return true;
            }

            if (m_CurrentAmmo == 0 && !m_inputWasHeld)
            {
                m_ShootAudioSource.PlayOneShot(MisfireSfx);
            }
            return false;
        }

        void HandleShoot()
        {
            for (int i = 0; i < BulletsPerShot; i++)
            {
                Vector3 shotDirection = GetShotDirectionWithinSpread(WeaponMuzzle);
                ProjectileBase newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position,
                    Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(this);
            }

            m_LastTimeShot = Time.time;
            m_ShootAudioSource.PlayOneShot(ShootSfx);
        }

        public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
        {
            float spreadAngleRatio = BulletSpreadAngle / 180f;
            Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
                spreadAngleRatio);

            return spreadWorldDirection;
        }
    }
}