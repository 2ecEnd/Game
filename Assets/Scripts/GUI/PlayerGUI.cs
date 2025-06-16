using Assets.Scripts;
using Assets.Scripts.Player;
using UnityEngine;
public class PlayerGUI : MonoBehaviour
{
    public GUISkin MainGUISkin;
    public bool Death;
    private GameObject player;
    private PlayerCharacterController characterController;
    private WeaponHandler weaponHandler;
    private Rect[] rects = new Rect[3];
    private int screenWidth;
    private int screenHeight;
    private void Start()
    {
        player = transform.parent.gameObject;
        characterController = transform.parent.GetComponent<PlayerCharacterController>();
        weaponHandler = transform.parent.GetComponent<PlayerWeaponManager>().ActiveWeapon;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        screenWidth = Screen.width;
        screenHeight = Screen.height;
        rects[0] = new Rect(screenWidth * 0.5f - 1, screenHeight * 0.5f - 1, 2, 2); //перекрестие
        rects[1] = new Rect(0, 0, screenWidth, screenHeight); //экран смерти
        rects[2] = new Rect(screenWidth * 0.9f, screenHeight * 0.9f, screenWidth * 0.1f, screenHeight * 0.1f); //количество патронов и хп
    }
    private void OnGUI()
    {
        GUI.skin.box.fontSize = 40;
        GUI.depth = 1;
        if (Death)
        {
            GUI.Box(rects[1], "смэртб");
            return;
        }
        GUI.Box(rects[0], "");
        if (!weaponHandler.Reload)
        {
            GUI.Box(rects[2], weaponHandler.CurrentAmmo.ToString() + "\n" + characterController.Health.ToString());
        }
        else
        {
            GUI.Box(rects[2], "R\n" + characterController.Health.ToString());
        }
    }
}