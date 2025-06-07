using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerWeaponManager : MonoBehaviour
    {
        [Header("General")]
        public WeaponHandler StartWeaponPrefab;
        public Transform WeaponPos;

        [Header("Recoil Parameters")]
        public float RecoilSharpness = 50f;
        public float RecoilRestitutionSharpness = 10f;
        public float MaxRecoilDistance = 0.5f;
        WeaponHandler activeWeapon;
        PlayerInputHandler m_InputHandler;

        Vector3 OriginalWeaponPos;
        Vector3 m_WeaponRecoilLocalPosition;
        Vector3 m_AccumulatedRecoil;
        void Start()
        {
            m_InputHandler = GetComponent<PlayerInputHandler>();
            AddWeapon(StartWeaponPrefab);
            OriginalWeaponPos = WeaponPos.localPosition;
        }

        void Update()
        {
            bool hasFired = activeWeapon.HandleShootInputs(
                m_InputHandler.GetFireInputDown(),
                m_InputHandler.GetFireInputHeld());
            if (hasFired)
            {
                m_AccumulatedRecoil += Vector3.back * activeWeapon.RecoilForce;
                m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
            }
        }

        void LateUpdate()
        {
            UpdateWeaponRecoil();

            WeaponPos.localPosition =
                OriginalWeaponPos + m_WeaponRecoilLocalPosition;
        }

        void UpdateWeaponRecoil()
        {

            if (m_WeaponRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f)
            {
                m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, m_AccumulatedRecoil,
                    RecoilSharpness * Time.deltaTime);
            }
            else
            {
                m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero,
                    RecoilRestitutionSharpness * Time.deltaTime);
                m_AccumulatedRecoil = m_WeaponRecoilLocalPosition;
            }
        }

        void AddWeapon(WeaponHandler weaponPrefab)
        {
            WeaponHandler weaponInstance = Instantiate(weaponPrefab, WeaponPos);

            weaponInstance.transform.localPosition = Vector3.zero;
            weaponInstance.transform.localRotation = Quaternion.identity;

            weaponInstance.Owner = gameObject;

            activeWeapon = weaponInstance;
        }
    }

}