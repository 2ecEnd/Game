using Assets.Scripts;
using Assets.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerGUI : MonoBehaviour
{
    public GUISkin MainGUISkin;
    public Texture2D CrossDash1;
    public Texture2D CrossDash2;
    public Texture2D CrossNoDash1;
    public Texture2D CrossNoDash2;
    public Texture2D DeathScreen;
    public Texture2D HP_Menu;
    public Texture2D Arrow;
    public bool Death;
    public float HPArrowStart;
    public float HPArrowStep;
    private GameObject player;
    private PlayerCharacterController characterController;
    private PlayerWeaponManager playerWeaponManager;
    //private WeaponHandler weaponHandler;
    private Rect[] rects = new Rect[7];
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
        rects[0] = new Rect(screenWidth * 0.5f - screenHeight * 0.02f, screenHeight * 0.5f - screenHeight * 0.02f, screenHeight * 0.04f, screenHeight * 0.04f); //перекрестие
        rects[1] = new Rect(0, 0, screenWidth, screenHeight); //экран смерти
        rects[2] = new Rect(0, screenHeight - screenWidth * 0.125f, screenWidth * 0.125f, screenWidth * 0.125f); //хп и количество патронов
        rects[3] = new Rect(screenWidth * -0.125f, screenHeight - screenWidth * 0.125f, screenWidth * 0.125f, screenWidth * 0.125f); //стрелка хп
        rects[4] = new Rect(screenWidth * 0.005f, screenHeight - screenWidth * 0.0425f, screenWidth * 0.05f, screenWidth * 0.0375f); //количество патронов
        rects[5] = new Rect(screenWidth * 0.5f - screenWidth * 0.08f, screenHeight * 0.8f - screenHeight * 0.04f, screenWidth * 0.16f, screenHeight * 0.08f); //кнопка возрождения
        rects[6] = new Rect(screenWidth * 0.925f, screenHeight * 0.925f, screenWidth * 0.075f, screenHeight * 0.075f); //результат
    }

    private void OnGUI()
    {
        GUI.skin = MainGUISkin;
        GUI.skin.label.fontSize = screenHeight / 20;
        GUI.skin.box.fontSize = screenHeight / 20;
        GUI.skin.button.fontSize = screenHeight / 40;
        GUI.depth = 0;
        if (!GlobalInspector.PlayerAlive)
        {
            GUI.DrawTexture(rects[1], DeathScreen);
            if (GUI.Button(rects[5], "Вставай заибал"))
            {
                GlobalInspector.PlayerRevive();
                player.GetComponent<PlayerCharacterController>().PRevive();
            }
            return;
        }
        //GUI.DrawTexture(rects[0], Cross);
        //GUI.Box(rects[0], "");
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
            GUI.DrawTexture(rects[0], CrossDash1);
        }
        else
        {
            GUI.DrawTexture(rects[0], CrossNoDash1);
        }
        if (characterController.DashCount > 1)
        {
            GUI.DrawTexture(rects[0], CrossDash2);
        }
        else
        {
            GUI.DrawTexture(rects[0], CrossNoDash2);
        }
        GUI.Box(rects[6], GlobalInspector.GetScore().ToString());
        GUI.DrawTexture(rects[2], HP_Menu);
        GUI.depth = 1;
        float arrow_rotation = characterController.Health * HPArrowStep;
        //Matrix4x4 GUIIs = GUI.matrix;
        //GUI.matrix = Matrix4x4.TRS(new Vector3(GUIIs.lossyScale.x, GUIIs.lossyScale.y, 0), GUIIs.rotation, GUIIs.lossyScale);
        GUIUtility.RotateAroundPivot(HPArrowStart + arrow_rotation, new Vector2(0, screenHeight));
        GUI.DrawTexture(rects[3], Arrow);
    }
}