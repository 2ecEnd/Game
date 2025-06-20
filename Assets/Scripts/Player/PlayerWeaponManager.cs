using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerWeaponManager : MonoBehaviour
    {
        [Header("General")]
        public WeaponHandler[] Weapons;
        public Transform WeaponPos;

        [Header("Recoil Parameters")]
        public float RecoilSharpness = 50f;
        public float RecoilRestitutionSharpness = 10f;
        public float MaxRecoilDistance = 0.5f;
        public WeaponHandler ActiveWeapon;
        //PlayerInputHandler m_InputHandler;

        Vector3 OriginalWeaponPos;
        Vector3 m_WeaponRecoilLocalPosition;
        Vector3 m_AccumulatedRecoil;
        int curentGun;
        void Start()
        {
            //m_InputHandler = GetComponent<PlayerInputHandler>();
            curentGun = 0;
            AddWeapon(Weapons[curentGun]);
            OriginalWeaponPos = WeaponPos.localPosition;
        }

        void Update()
        {
            bool hasFired = ActiveWeapon.HandleShootInputs(Input.GetKeyDown(KeyCode.Mouse0), Input.GetKey(KeyCode.Mouse0));
            if (hasFired)
            {
                m_AccumulatedRecoil += Vector3.back * ActiveWeapon.RecoilForce;
                m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ActiveWeapon.HandleReload();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                curentGun++;
                if(curentGun == Weapons.Length)
                {
                    curentGun = 0;
                }
                AddWeapon(Weapons[curentGun]);
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
            if(ActiveWeapon != null)
            {
                Destroy(ActiveWeapon.gameObject);
            }

            WeaponHandler weaponInstance = Instantiate(weaponPrefab, WeaponPos);

            weaponInstance.transform.localPosition = Vector3.zero;
            weaponInstance.transform.localRotation = Quaternion.identity;

            weaponInstance.Owner = gameObject;

            ActiveWeapon = weaponInstance;
        }
    }

}