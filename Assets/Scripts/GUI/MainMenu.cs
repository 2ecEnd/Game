using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject Buttons;
    public Animator BlackPanelAnim;
    public GameObject BestiatyPanel;
    public GameObject SettingsPanel;
    public GameObject DevelopersPanel;

    public Transform Enemies;
    public float RotationSpeed = 10;
    public Texture2D Black;
    int enemyNumber;
    bool changing;
    bool changed;
    bool play;
    //Texture2D black;
    //Color color;
    Color col;
    int menuFlag = 0;
    private int screenWidth;
    private int screenHeight;
    private Rect[] rects = new Rect[10];
    private GameObject[] Panels;
    Animator[] buttonAnimators;
    void Start()
    {
        Panels = new GameObject[3] { BestiatyPanel, SettingsPanel, DevelopersPanel };
        buttonAnimators = Buttons.GetComponentsInChildren<Animator>();
        //color = Color.black;
        //color.a = 0;
        //black = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        //print(HP_Menu.format);
        //black.alphaIsTransparency = true;
        //black.SetPixel(0, 0, color);
        // col = Black.GetPixel(0, 0);
        // col.a = 0;
        // Black.SetPixel(0, 0, col);
        // Black.Apply(false);
        enemyNumber = 0;
        for (int i = 1; i < Enemies.childCount; i++)
        {
            Enemies.GetChild(i).gameObject.SetActive(false);
        }
        // screenWidth = Screen.width;
        // screenHeight = Screen.height;
        // float gap = Screen.height * 0.01f;
        // float buttonHeight = (Screen.height - gap * 6) * 0.2f;
        // rects[0] = new Rect(0, 0, Screen.width, Screen.height); //���������� ������
        // rects[1] = new Rect(gap, gap, buttonHeight, buttonHeight); //������ ������ ����
        // rects[2] = new Rect(gap, 2 * gap + buttonHeight, buttonHeight, buttonHeight); //������ ���������
        // rects[3] = new Rect(gap, 3 * gap + 2 * buttonHeight, buttonHeight, buttonHeight); //������ ��������
        // rects[4] = new Rect(gap, 4 * gap + 3 * buttonHeight, buttonHeight, buttonHeight); //������ �������������
        // rects[5] = new Rect(gap, 5 * gap + 4 * buttonHeight, buttonHeight, buttonHeight); //������ ������
        // rects[6] = new Rect(2 * gap + buttonHeight, gap, screenWidth - 3 * gap - buttonHeight, screenHeight - 2 * gap); //����
        // rects[7] = new Rect(3 * gap + buttonHeight, gap + 0.075f * screenHeight, 0.2f * screenWidth, 0.1f * screenHeight); //��������� �����
        // rects[8] = new Rect(screenWidth - 2 * gap - buttonHeight, gap + 0.0875f * screenHeight, buttonHeight, 0.1f * screenHeight); //�������� �����
        // rects[9] = new Rect(0.95f * screenWidth - 2 * gap - buttonHeight, gap + 0.075f * screenHeight, buttonHeight, 0.1f * screenHeight); //������� �����

        StartCoroutine(ChangeEnemy());
    }
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, RotationSpeed * Time.deltaTime);
        // if (!changing && transform.rotation.eulerAngles.y > 90 && transform.rotation.eulerAngles.y < 100)
        // {
        //     changing = true;
        // }
        // if (changing)
        // {
        //     ChangeEnemy();
        // }
    }
    // private void OnGUI()
    // {
    //     if (play)
    //     {
    //         GUI.skin.box.fontSize = 200;
    //         GUI.Box(rects[0], "����������");
    //         SceneManager.LoadScene(1);
    //     }
    //     GUI.skin.box.fontSize = (int)(screenHeight * 0.04f);
    //     GUI.skin.button.fontSize = (int)(screenHeight * 0.025f);
    //     GUI.skin.label.fontSize = (int)(screenHeight * 0.025f);
    //     GUI.DrawTexture(rects[0], Black);
    //     if (GUI.Button(rects[1], "������"))
    //     {
    //         menuFlag = 1;
    //     }
    //     if (GUI.Button(rects[2], "���������"))
    //     {
    //         menuFlag = 2;
    //     }
    //     if (GUI.Button(rects[3], "���������"))
    //     {
    //         menuFlag = 3;
    //     }
    //     if (GUI.Button(rects[4], "������������"))
    //     {
    //         menuFlag = 4;
    //     }
    //     if (GUI.Button(rects[5], "����� ("))
    //     {
    //         menuFlag = 5;
    //     }
    //     switch (menuFlag)
    //     {
    //         case 1:
    //             GUI.skin.box.fontSize = 200;
    //             GUI.Box(rects[0], "����������");
    //             play = true;
    //             break;
    //         case 2:
    //             GUI.Box(rects[6], "���������:\n�� ������ ������ �����\n ������� �� ������� ������� ������� ����\n � �� ����� �� ������� ������� �����");
    //             break;
    //         case 3:
    //             GUI.Box(rects[6], "���������");
    //             GUI.Label(rects[7], "��������� �����");
    //             AudioListener.volume = GUI.HorizontalSlider(rects[8], AudioListener.volume, 0, 1);
    //             GUI.Label(rects[9], Mathf.Round(AudioListener.volume * 100).ToString());
    //             break;
    //         case 4:
    //             GUI.skin.box.fontSize = (int)(screenHeight * 0.08f);
    //             GUI.skin.box.alignment = TextAnchor.MiddleCenter;
    //             GUI.Box(rects[6], "������������:\n��������� 2ec_End ��\n������ Several_SIZE �����������\n������ Remos ������\n������ �������������:\n����� Sosuha ����������\n ");
    //             GUI.skin.box.fontSize = (int)(screenHeight * 0.04f);
    //             GUI.skin.box.alignment = TextAnchor.UpperCenter;
    //             break;
    //         case 5:
    //             GUI.skin.button.fontSize = (int)(screenHeight * 0.08f);
    //             if (GUI.Button(rects[6], "�� ����� ������ �����?\n(((((((((((((((((((((((("))
    //             {
    //                 Application.Quit();
    //             }
    //             GUI.skin.button.fontSize = (int)(screenHeight * 0.04f);
    //             break;
    //     }
    // }
    // private void ChangeEnemy()
    // {
    //     col.a = Mathf.Abs(Mathf.Sin((transform.rotation.eulerAngles.y - 90) / 20));
    //     if (col.a > 0.98f && !changed)
    //     {
    //         Enemies.GetChild(enemyNumber).gameObject.SetActive(false);
    //         enemyNumber = (enemyNumber + 1) % Enemies.childCount;
    //         Enemies.GetChild(enemyNumber).gameObject.SetActive(true);
    //         changed = true;
    //     }
    //     if (col.a < 0.02f && changed)
    //     {
    //         col.a = 0;
    //         changing = false;
    //         changed = false;
    //     }
    //     Black.SetPixel(0, 0, col);
    //     Black.Apply(false);
    // }

    IEnumerator ChangeEnemy()
    {
        while(true)
        {
            yield return new WaitForSeconds(13.5f);
            BlackPanelAnim.SetTrigger("Play");
            yield return new WaitForSeconds(0.5f);
            Enemies.GetChild(enemyNumber).gameObject.SetActive(false);
            enemyNumber = (enemyNumber + 1) % Enemies.childCount;
            Enemies.GetChild(enemyNumber).gameObject.SetActive(true);
        }
    }

    private void CloseOtherTabs()
    {
        for (int i = 0; i < Panels.Length; i++)
            Panels[i].SetActive(false);
        foreach (Animator anim in buttonAnimators)
        {
            anim.SetBool("IsPressed", false);
        }
    }
    public void PlayButton()
    {
        SceneManager.LoadScene(1);
    }

    public void BestiaryButton()
    {
        CloseOtherTabs();
        BestiatyPanel.SetActive(true);
        Buttons.transform.Find("BestiaryButton").gameObject.GetComponent<Animator>().SetBool("IsPressed", true);
    }

    public void SettingsButton()
    {
        CloseOtherTabs();
        SettingsPanel.SetActive(true);
        Buttons.transform.Find("SettingsButton").gameObject.GetComponent<Animator>().SetBool("IsPressed", true);
    }

    public void DevelopersButton()
    {
        CloseOtherTabs();
        DevelopersPanel.SetActive(true);
        Buttons.transform.Find("DevelopersButton").gameObject.GetComponent<Animator>().SetBool("IsPressed", true);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void VolumeSlider(float value)
    {
        AudioListener.volume = value;
    }
}