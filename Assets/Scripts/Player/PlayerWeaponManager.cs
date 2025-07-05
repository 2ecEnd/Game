using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerWeaponManager : MonoBehaviour
    {
        [Header("General")]
        public Camera PlayerCamera;
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
        List<WeaponHandler> AddedWeapons;
        int curentGun;
        Vector3 cameraInitialPosition;
        Quaternion startRotation;
        void Start()
        {
            AddedWeapons = new List<WeaponHandler>();
            //m_InputHandler = GetComponent<PlayerInputHandler>();
            curentGun = 0;
            for (int i = 0; i < Weapons.Length; i++)
                AddWeapon(Weapons[i]);
            OriginalWeaponPos = WeaponPos.localPosition;
            SelectWeapon(curentGun);
            cameraInitialPosition = PlayerCamera.transform.localPosition;
        }

        void Update()
        {
            RaycastHit hit;
            if ((Physics.Raycast(
            origin: PlayerCamera.transform.position,
            direction: PlayerCamera.transform.forward,
            hitInfo: out hit,
            maxDistance: 100f)))
            {
                if(!hit.collider.gameObject.CompareTag("Player") && (transform.position - hit.point).magnitude > 2)
                    WeaponPos.LookAt(hit.point);
            }
            else
                WeaponPos.transform.localRotation = Quaternion.Euler(0f, -0.3f, 0f);

            if (!GlobalInspector.PlayerAlive)
            {
                ActiveWeapon.gameObject.SetActive(false);
                return;
            }
            else if (!ActiveWeapon.gameObject.activeSelf)
            {
                ActiveWeapon.gameObject.SetActive(true);
            }
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
                if (curentGun == Weapons.Length)
                {
                    curentGun = 0;
                }
                //AddWeapon(Weapons[curentGun]);
                if (ActiveWeapon.Reload) ActiveWeapon.StopReload();
                SelectWeapon(curentGun);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                curentGun = 0;
                if (ActiveWeapon.Reload) ActiveWeapon.StopReload();
                SelectWeapon(curentGun);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                curentGun = 1;
                if (ActiveWeapon.Reload) ActiveWeapon.StopReload();
                SelectWeapon(curentGun);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                curentGun = 2;
                if (ActiveWeapon.Reload) ActiveWeapon.StopReload();
                SelectWeapon(curentGun);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                curentGun = 3;
                if (ActiveWeapon.Reload) ActiveWeapon.StopReload();
                SelectWeapon(curentGun);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                curentGun = 4;
                if (ActiveWeapon.Reload) ActiveWeapon.StopReload();
                SelectWeapon(curentGun);
            }
        }

        void LateUpdate()
        {
            if (!GlobalInspector.PlayerAlive)
            {
                return;
            }
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
            // if (ActiveWeapon != null)
            // {
            //     Destroy(ActiveWeapon.gameObject);
            // }

            WeaponHandler weaponInstance = Instantiate(weaponPrefab, WeaponPos);

            weaponInstance.transform.localPosition = Vector3.zero;
            weaponInstance.transform.localRotation = Quaternion.identity;

            weaponInstance.Owner = gameObject;

            //ActiveWeapon = weaponInstance;
            AddedWeapons.Add(weaponInstance);
            //weaponInstance.gameObject.SetActive(false);
            SetActive(weaponInstance.gameObject, false);
        }

        void SelectWeapon(int index)
        {
            if (ActiveWeapon != null)
            {
                SetActive(ActiveWeapon.gameObject, false);
                //ActiveWeapon.gameObject.SetActive(false);
            }

            AddedWeapons[index].StartCoroutine(AddedWeapons[index].Equip());
            SetActive(AddedWeapons[index].gameObject, true);
            //AddedWeapons[index].gameObject.SetActive(true);
            ActiveWeapon = AddedWeapons[index];
        }

        void SetActive(GameObject weapon, bool flag)
        {
            weapon.GetComponent<WeaponHandler>().enabled = flag;
            foreach (Transform child in weapon.transform)
            {
                child.gameObject.SetActive(flag);
            }

            if (weapon.GetComponent<WeaponHandler>().MuzzleFlashVFX != null)
            {
                weapon.GetComponent<WeaponHandler>().MuzzleFlashVFX.Stop();
            }
        }
    }

}