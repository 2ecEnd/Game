using System.Linq.Expressions;
using Assets.Scripts;
using Assets.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerGUI : MonoBehaviour
{
    public PlayerUI playerUI;
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
    public GameObject player;
    private PlayerCharacterController characterController;
    private PlayerWeaponManager playerWeaponManager;
    private Rect[] rects = new Rect[23];
    private int screenWidth;
    private int screenHeight;
    public int flag;
    private int oldFlag;

    private void Start()
    {
        GlobalInspector.PlayerGUI = this;
        player = transform.parent.gameObject;
        characterController = transform.parent.GetComponent<PlayerCharacterController>();
        playerWeaponManager = transform.parent.GetComponent<PlayerWeaponManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        flag = 1;

        screenWidth = Screen.width;
        screenHeight = Screen.height;
        rects[0] = new Rect(screenWidth * 0.5f - screenHeight * 0.02f, screenHeight * 0.5f - screenHeight * 0.02f, screenHeight * 0.04f, screenHeight * 0.04f); //�����������
        rects[1] = new Rect(0, -screenHeight, screenWidth, screenHeight); //����� ������
        rects[2] = new Rect(0, screenHeight - screenWidth * 0.125f, screenWidth * 0.125f, screenWidth * 0.125f); //�� � ���������� ��������
        rects[3] = new Rect(screenWidth * -0.125f, screenHeight - screenWidth * 0.125f, screenWidth * 0.125f, screenWidth * 0.125f); //������� ��
        rects[4] = new Rect(screenWidth * 0.005f, screenHeight - screenWidth * 0.0425f, screenWidth * 0.05f, screenWidth * 0.0375f); //���������� ��������
        rects[5] = new Rect(screenWidth * 0.5f - screenWidth * 0.24f, screenHeight * 0.8f - screenHeight * 0.04f, screenWidth * 0.16f, screenHeight * 0.08f); //������ �����������
        rects[6] = new Rect(screenWidth * 0.5f - screenWidth * 0.08f, screenHeight * 0.8f - screenHeight * 0.04f, screenWidth * 0.16f, screenHeight * 0.08f); //������ ���������� ����������
        rects[7] = new Rect(screenWidth * 0.925f, screenHeight * 0.925f, screenWidth * 0.075f, screenHeight * 0.075f); //���������
        rects[8] = new Rect(screenWidth * 0.25f, screenHeight * 0.25f, screenWidth * 0.5f, screenHeight * 0.5f); //���� ���������� ����������
        rects[9] = new Rect(0, 0, screenWidth, screenHeight); //������
        rects[10] = new Rect(screenWidth * 0.425f, screenHeight * 0.1f, screenWidth * 0.15f, screenHeight * 0.1f); //����� ����� �����
        rects[11] = new Rect(screenWidth * 0.25f, screenHeight * 0.1f, screenWidth * 0.5f, screenHeight * 0.8f); //������� ����
        rects[12] = new Rect(screenWidth * 0.3f, screenHeight * 0.15f, screenWidth * 0.4f, screenHeight * 0.15f); //����������
        rects[13] = new Rect(screenWidth * 0.3f, screenHeight * 0.7f, screenWidth * 0.4f, screenHeight * 0.15f); //����� � ������� ����
        rects[14] = new Rect(screenWidth * 0.1f, screenHeight * 0.1f, screenWidth * 0.8f, screenHeight * 0.8f); //���� ������������
        rects[15] = new Rect(screenWidth * 0.1f, screenHeight * 0.1f, screenWidth * 0.4f, screenHeight * 0.2f); //������ �����������
        rects[16] = new Rect(screenWidth * 0.5f, screenHeight * 0.1f, screenWidth * 0.4f, screenHeight * 0.2f); //������ ������ �����
        rects[17] = new Rect(screenWidth * 0.1f, screenHeight * 0.3f, screenWidth * 0.4f, screenHeight * 0.2f); //������ ���������� �����
        rects[18] = new Rect(screenWidth * 0.5f, screenHeight * 0.3f, screenWidth * 0.4f, screenHeight * 0.2f); //������ �������� �����
        rects[19] = new Rect(screenWidth * 0.1f, screenHeight * 0.5f, screenWidth * 0.4f, screenHeight * 0.2f); //������ ������ �����
        rects[20] = new Rect(screenWidth * 0.5f, screenHeight * 0.5f, screenWidth * 0.4f, screenHeight * 0.2f); //������ ��������� �����
        rects[21] = new Rect(screenWidth * 0.1f, screenHeight * 0.7f, screenWidth * 0.4f, screenHeight * 0.2f); //������ ����� ��
        rects[22] = new Rect(screenWidth * 0.5f, screenHeight * 0.7f, screenWidth * 0.4f, screenHeight * 0.2f); //������ ����� ��������
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (flag != -1)
            {
                oldFlag = flag;
                if (oldFlag == 0)
                {
                    oldFlag = 1;
                }
                flag = -1;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
            }
            else
            {
                flag = oldFlag;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                GlobalInspector.PlayerAlive = true;
            }
        }
    }

    private void OnGUI()
    {
        GUI.skin = MainGUISkin;
        GUI.skin.label.fontSize = screenHeight / 20;
        GUI.skin.box.fontSize = screenHeight / 20;
        GUI.skin.button.fontSize = screenHeight / 40;
        GUI.depth = 0;
        switch (flag)
        {
            case -1:
                GUI.Box(rects[14], "");
                if (GUI.Button(rects[15], "Возродиться")) // Revive
                {
                    GlobalInspector.PlayerRevive();
                    flag = -1;
                }
                if (GUI.Button(rects[16], "Пустая волна")) // Empty wave
                {
                    GlobalInspector.GameController.WaveNumber = -1;
                    GlobalInspector.GameController.NewWave();
                }
                if (GUI.Button(rects[17], "Предыдущая волна")) // Previous wave
                {
                    GlobalInspector.GameController.WaveNumber--;
                    GlobalInspector.GameController.NewWave();
                }
                if (GUI.Button(rects[18], "Следующая волна")) // Next wave
                {
                    GlobalInspector.GameController.WaveNumber++;
                    GlobalInspector.GameController.NewWave();
                }
                if (GUI.Button(rects[19], "Заготовленная арена")) // Preset
                {
                    GlobalInspector.ArenaManager.ChooseFromPresets();
                }
                if (GUI.Button(rects[20], "Сгенерировать арену")) // Generate arena
                {
                    GlobalInspector.ArenaManager.GenerateCircleArena();
                }
                if (GUI.Button(rects[21], "iddqd on/off")) // Invulnerability
                {
                    if (GlobalInspector.PlayerCharacterController.MaxHealth == 100)
                    {
                        GlobalInspector.PlayerCharacterController.MaxHealth = 1000000;
                        GlobalInspector.PlayerCharacterController.Health = 1000000;
                    }
                    else
                    {
                        GlobalInspector.PlayerCharacterController.MaxHealth = 100;
                        GlobalInspector.PlayerCharacterController.Health = 100;
                    }
                }
                if (GUI.Button(rects[22], "\"Анигиляторная пушка\" on/off")) // Anihilation
                {
                    GlobalInspector.CheatAmmo = !GlobalInspector.CheatAmmo;
                }
                break;
            case 0:
                GUI.Box(rects[11], "");
                if (GUI.Button(rects[12], "Статистика")) // Statistic
                {
                    flag = oldFlag;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1;
                    GlobalInspector.PlayerAlive = true;
                }
                if (GUI.Button(rects[13], "Выход в главное меню")) // Main menu
                {
                    Time.timeScale = 1;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    GlobalInspector.PlayerAlive = true;
                    flag = 0;
                    SceneManager.LoadScene(0);
                }
                break;
            case 1:
                if (!playerWeaponManager.ActiveWeapon.Reload)
                    GUI.Box(rects[4], playerWeaponManager.ActiveWeapon.CurrentAmmo.ToString());
                else
                    GUI.Box(rects[4], "R");

                switch (characterController.DashCount)
                {
                    case 0:
                        GUI.DrawTexture(rects[0], CrossNoDash1);
                        GUI.DrawTexture(rects[0], CrossNoDash2);
                        break;
                    case 1:
                        GUI.DrawTexture(rects[0], CrossDash1);
                        GUI.DrawTexture(rects[0], CrossNoDash2);
                        break;
                    case 2:
                        GUI.DrawTexture(rects[0], CrossDash1);
                        GUI.DrawTexture(rects[0], CrossDash2);
                        break;
                }
                if (GlobalInspector.Rest) // Wave counter
                    GUI.Box(rects[10], "Волна " + (GlobalInspector.WaveNumber + 1).ToString());

                GUI.Box(rects[7], GlobalInspector.GetScore().ToString());
                GUI.DrawTexture(rects[2], HP_Menu);
                GUI.depth = 1;
                float arrow_rotation = characterController.Health * HPArrowStep;
                GUIUtility.RotateAroundPivot(HPArrowStart + arrow_rotation, new Vector2(0, screenHeight));
                GUI.DrawTexture(rects[3], Arrow);
                break;
            case 2: // Statistic screen
                GUI.Box(rects[8], GlobalInspector.Statistics());
                if (GUI.Button(rects[6], "Назад"))
                {
                    flag = oldFlag;
                }
                break;
            case 3: // Death screeen
                GUI.DrawTexture(rects[1], DeathScreen);
                if (rects[1].y < 0)
                {
                    rects[1].y += 500 * Time.unscaledDeltaTime;
                }
                else
                {
                    rects[1].y = 0;
                    playerUI.ShowDeathScreen();
                    // if (GUI.Button(rects[5], "Начать заново")) // Try Again
                    // {
                    //     rects[1] = new Rect(0, -screenHeight, screenWidth, screenHeight); //���������� ������ ������

                    //     GlobalInspector.Restart();
                    // }
                    // if (GUI.Button(rects[6], "Статистика"))  // Statistic
                    // {
                    //     flag = 2;
                    //     oldFlag = 3;
                    // }
                }
                break;
            case 4: // Victory screen
                GUI.Box(rects[9], "Вы выиграли!"); // You win!
                if (GUI.Button(rects[5], "Начать заново"))  // Try Again
                {
                    GlobalInspector.Restart();
                }
                if (GUI.Button(rects[6], "Статистика"))   // Statistic
                {
                    flag = 2;
                    oldFlag = 4;
                }
                break;
            default:
                break;
        }
    }

    public void RestartButton()
    {
        rects[1] = new Rect(0, -screenHeight, screenWidth, screenHeight);
        GlobalInspector.Restart();
    }

    public void StatisticsButton()
    {
        flag = 2;
        oldFlag = 3;
    }
}