using Assets.Scripts;
using Assets.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerGUI : MonoBehaviour
{
    public GUISkin MainGUISkin;
    public Texture2D HP_Menu;
    public Texture2D Arrow;
    public bool Death;
    public float HPArrowStart;
    public float HPArrowStep;
    private GameObject player;
    private PlayerCharacterController characterController;
    private PlayerWeaponManager playerWeaponManager;
    //private WeaponHandler weaponHandler;
    private Rect[] rects = new Rect[8];
    private int screenWidth;
    private int screenHeight;

    private void Start()
    {
        player = transform.parent.gameObject;
        characterController = transform.parent.GetComponent<PlayerCharacterController>();
        playerWeaponManager = transform.parent.GetComponent<PlayerWeaponManager>();
        //weaponHandler = playerWeaponManager.ActiveWeapon;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        screenWidth = Screen.width;
        screenHeight = Screen.height;
        rects[0] = new Rect(screenWidth * 0.5f - 1, screenHeight * 0.5f - 1, 2, 2); //перекрестие
        rects[1] = new Rect(0, 0, screenWidth, screenHeight); //экран смерти
        rects[2] = new Rect(0, screenHeight - screenWidth * 0.125f, screenWidth * 0.125f, screenWidth * 0.125f); //хп и количество патронов
        rects[3] = new Rect(screenWidth * -0.125f, screenHeight - screenWidth * 0.125f, screenWidth * 0.125f, screenWidth * 0.125f); //стрелка хп
        rects[4] = new Rect(screenWidth * 0.005f, screenHeight - screenWidth * 0.0425f, screenWidth * 0.05f, screenWidth * 0.0375f); //количество патронов
        rects[5] = new Rect(screenWidth * 0.005f, screenHeight - screenWidth * 0.0675f, screenWidth * 0.02f, screenWidth * 0.02f); //рывок 1
        rects[6] = new Rect(screenWidth * 0.030f, screenHeight - screenWidth * 0.0675f, screenWidth * 0.02f, screenWidth * 0.02f); //рывок 2
        rects[7] = new Rect(screenWidth * 0.925f, screenHeight * 0.925f, screenWidth * 0.075f, screenHeight * 0.075f); //результат
    }

    private void OnGUI()
    {
        GUI.skin = MainGUISkin;
        GUI.skin.label.fontSize = screenHeight / 20;
        GUI.skin.box.fontSize = screenHeight / 20;
        GUI.depth = 0;
        GUI.Box(rects[0], "");
        if (Death)
        {
            GUI.Box(rects[1], "смэртб");
            return;
        }
        if (!playerWeaponManager.ActiveWeapon.Reload)
        {
            GUI.Box(rects[4], playerWeaponManager.ActiveWeapon.CurrentAmmo.ToString());
        }
        else
        {
            GUI.Box(rects[4], "R");
        }
        if (characterController.DashCount > 0)
        {
            GUI.Box(rects[5], "");
        }
        if (characterController.DashCount > 1)
        {
            GUI.Box(rects[6], "");
        }
        GUI.Box(rects[7], GlobalInspector.GetScore().ToString());
        GUI.DrawTexture(rects[2], HP_Menu);
        GUI.depth = 1;
        float arrow_rotation = characterController.Health * HPArrowStep;
        //Matrix4x4 GUIIs = GUI.matrix;
        //GUI.matrix = Matrix4x4.TRS(new Vector3(GUIIs.lossyScale.x, GUIIs.lossyScale.y, 0), GUIIs.rotation, GUIIs.lossyScale);
        GUIUtility.RotateAroundPivot(HPArrowStart + arrow_rotation, new Vector2(0, screenHeight));
        GUI.DrawTexture(rects[3], Arrow);
    }
}