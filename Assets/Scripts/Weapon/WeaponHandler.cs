using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

namespace Assets.Scripts
{
    public class WeaponHandler : MonoBehaviour
    {
        public enum WeaponShootType
        {
            Manual,
            Automatic,
        }
        public enum WeaponReloadingType
        {
            Manual,
            Magazine,
        }
        [Header("Current Parameters")]
        public int CurrentAmmo;
        public bool Reload;

        [Header("Current Parameters")]
        public float EquipingTime;

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
        public WeaponReloadingType ReloadingType;

        [Header("Recoil Parameters")]
        public float RecoilForce = 1f;

        [Header("Main")]
        public GameObject Owner;
        public Vector3 MuzzleWorldVelocity { get; private set; }

        [Header("Audio")] 
        public AudioClip ShootSfx;
        public AudioClip MisfireSfx;
        public AudioClip ReloadingSfx;
        public AudioClip ManualPumpSfx;
        [Header("Main AudioSource")] 
        public AudioSource m_ShootAudioSource;

        [Header("Reload AudioSource")] 
        public AudioSource m_ReloadAudioSource;

        [Header("VFX")]
        public VisualEffect MuzzleFlashVFX;

        float m_LastTimeShot = Mathf.NegativeInfinity;
        float m_LastTimeReloading = Mathf.NegativeInfinity;
        Vector3 m_LastMuzzlePosition;
        public Animator animator;
        bool m_inputHeldCurrentFrame = false;
        bool m_inputWasHeld = false;
        bool isEquiping = false;
        float delayBetweenShots;

        void Awake()
        {
            animator = GetComponent<Animator>();
            CurrentAmmo = MagazineSize;
            Reload = false;
            m_LastMuzzlePosition = WeaponMuzzle.position;

            delayBetweenShots = 60f / FireRate;
            if (MuzzleFlashVFX != null)
            {
                MuzzleFlashVFX.Stop();
            }
        }

        void Update()
        {
            if(Reload && m_LastTimeReloading + AmmoReloadRate < Time.time)
            {
                Reload = false;
            }
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
            if (isEquiping) return false;
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

        public void HandleReload()
        {
            if (!isEquiping && !Reload && m_LastTimeShot + delayBetweenShots < Time.time && CurrentAmmo < MagazineSize)
            {
                switch (ReloadingType)
                {
                    case WeaponReloadingType.Manual:
                        StartCoroutine(ManualReloading());
                        break;
                    case WeaponReloadingType.Magazine:
                        MagazineReloading();
                        break;
                }
            }
        }

        public void MagazineReloading()
        {
            if(animator != null)
            {
                animator.SetTrigger("Reload");
            }
            CurrentAmmo = MagazineSize;
            Reload = true;
            m_LastTimeReloading = Time.time;
            m_ReloadAudioSource.PlayOneShot(ReloadingSfx);
        }

        public IEnumerator ManualReloading()
        {
            if (animator != null)
            {
                animator.SetBool("IsReloading", true);
                animator.SetTrigger("Reload");
            }
            m_LastTimeReloading = Time.time;
            Reload = true;
            yield return new WaitForSeconds(0.334f);

            while (CurrentAmmo < MagazineSize)
            {
                CurrentAmmo += 1;
                m_ShootAudioSource.PlayOneShot(ReloadingSfx);
                yield return new WaitForSeconds(0.5f);
            }

            if (animator != null)
            {
                animator.SetBool("IsReloading", false);
            }
            m_ShootAudioSource.PlayOneShot(ManualPumpSfx);
            yield return new WaitForSeconds(0.8f);
            Reload = false;
        }

        bool TryShoot()
        {
            if (Reload) return false;

            if (CurrentAmmo >= 1
                && m_LastTimeShot + delayBetweenShots < Time.time)
            {
                HandleShoot();
                CurrentAmmo -= 1;
                if (CurrentAmmo == 0) m_inputHeldCurrentFrame = false;

                return true;
            }

            if (CurrentAmmo == 0 && !m_inputWasHeld)
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

            if (animator != null)
            {
                animator.ResetTrigger("Fire");
                animator.SetTrigger("Fire");
            }
            if (MuzzleFlashVFX != null)
            {
                MuzzleFlashVFX.Play();
            }
            m_LastTimeShot = Time.time;

            switch (ReloadingType)
                {
                    case WeaponReloadingType.Manual:
                        StartCoroutine(PlayShootSound());
                        break;
                    case WeaponReloadingType.Magazine:
                        m_ShootAudioSource.PlayOneShot(ShootSfx);;
                        break;
                }
        }

        IEnumerator PlayShootSound()
        {
            m_ShootAudioSource.PlayOneShot(ShootSfx);
            yield return new WaitForSeconds(0.334f);
            m_ShootAudioSource.PlayOneShot(ManualPumpSfx);
        }

        public IEnumerator Equip()
        {
            if (MuzzleFlashVFX != null)
            {
                MuzzleFlashVFX.Stop();
            }

            isEquiping = true;
            animator.SetTrigger("Equip");
            yield return new WaitForSeconds(EquipingTime);
            isEquiping = false;
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