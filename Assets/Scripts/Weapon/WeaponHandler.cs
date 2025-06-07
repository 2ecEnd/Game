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

        float m_LastTimeShot = Mathf.NegativeInfinity;
        int m_CurrentAmmo;
        Vector3 m_LastMuzzlePosition;
        AudioSource m_ShootAudioSource;
        bool m_WantsToShoot = false;

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

        public bool HandleShootInputs(bool inputDown, bool inputHeld)
        {
            m_WantsToShoot = inputDown || inputHeld;
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

        bool TryShoot()
        {
            float DelayBetweenShots = 60f / FireRate;
            if (m_CurrentAmmo >= 1
                && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                HandleShoot();
                m_CurrentAmmo -= 1;

                return true;
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