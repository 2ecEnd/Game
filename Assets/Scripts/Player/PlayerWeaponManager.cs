using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Assets.Scripts.Player
{
    public class PlayerWeaponManager : MonoBehaviour
    {
        public WeaponHandler StartWeaponPrefab;
        public Transform WeaponPos;
        WeaponHandler activeWeapon;
        PlayerInputHandler m_InputHandler;

        void Start()
        {
            m_InputHandler = GetComponent<PlayerInputHandler>();
            AddWeapon(StartWeaponPrefab);
        }

        void Update()
        {
            bool hasFired = activeWeapon.HandleShootInputs(
                m_InputHandler.GetFireInputDown(),
                m_InputHandler.GetFireInputHeld());
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