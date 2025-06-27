using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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
    void Start()
    {
        //color = Color.black;
        //color.a = 0;
        //black = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        //print(HP_Menu.format);
        //black.alphaIsTransparency = true;
        //black.SetPixel(0, 0, color);
        col = Black.GetPixel(0, 0);
        col.a = 0;
        Black.SetPixel(0, 0, col);
        Black.Apply(false);
        enemyNumber = 0;
        for(int i = 1; i < Enemies.childCount; i++)
        {
            Enemies.GetChild(i).gameObject.SetActive(false);
        }
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        float gap = Screen.height * 0.01f;
        float buttonHeight = (Screen.height - gap * 6) * 0.2f;
        rects[0] = new Rect(0, 0, Screen.width, Screen.height); //затемнение экрана
        rects[1] = new Rect(gap, gap, buttonHeight, buttonHeight); //кнопка старта игры
        rects[2] = new Rect(gap, 2 * gap + buttonHeight, buttonHeight, buttonHeight); //кнопка бестиария
        rects[3] = new Rect(gap, 3 * gap + 2 * buttonHeight, buttonHeight, buttonHeight); //кнопка настроек
        rects[4] = new Rect(gap, 4 * gap + 3 * buttonHeight, buttonHeight, buttonHeight); //кнопка разработчиков
        rects[5] = new Rect(gap, 5 * gap + 4 * buttonHeight, buttonHeight, buttonHeight); //кнопка выхода
        rects[6] = new Rect(2 * gap + buttonHeight, gap, screenWidth - 3 * gap - buttonHeight, screenHeight - 2 * gap); //окно
        rects[7] = new Rect(3 * gap + buttonHeight, gap + 0.075f * screenHeight, 0.2f * screenWidth, 0.1f * screenHeight); //настройка звука
        rects[8] = new Rect(screenWidth - 2 * gap - buttonHeight, gap + 0.0875f * screenHeight, buttonHeight, 0.1f * screenHeight); //ползунок звука
        rects[9] = new Rect(0.95f * screenWidth - 2 * gap - buttonHeight, gap + 0.075f * screenHeight, buttonHeight, 0.1f * screenHeight); //уровень звука
    }
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, RotationSpeed * Time.deltaTime);
        if(!changing && transform.rotation.eulerAngles.y > 90 && transform.rotation.eulerAngles.y < 100)
        {
            changing = true;
        }
        if(changing)
        {
            ChangeEnemy();
        }
    }
    private void OnGUI()
    {
        if (play)
        {
            GUI.skin.box.fontSize = 200;
            GUI.Box(rects[0], "загрузОчка");
            SceneManager.LoadScene(1);
        }
        GUI.skin.box.fontSize = (int)(screenHeight * 0.04f);
        GUI.skin.button.fontSize = (int)(screenHeight * 0.025f);
        GUI.skin.label.fontSize = (int)(screenHeight * 0.025f);
        GUI.DrawTexture(rects[0], Black);
        if (GUI.Button(rects[1], "Играть"))
        {
            menuFlag = 1;
        }
        if (GUI.Button(rects[2], "Бестиарий"))
        {
            menuFlag = 2;
        }
        if (GUI.Button(rects[3], "Настройки"))
        {
            menuFlag = 3;
        }
        if (GUI.Button(rects[4], "Разработчики"))
        {
            menuFlag = 4;
        }
        if (GUI.Button(rects[5], "Выход ("))
        {
            menuFlag = 5;
        }
        switch (menuFlag)
        {
            case 1:
                GUI.skin.box.fontSize = 200;
                GUI.Box(rects[0], "загрузОчка");
                play = true;
                break;
            case 2:
                GUI.Box(rects[6], "Бестиарий:\nНу всякие разные враги\n которые по всякому разному наносят урон\n и их можно по всякому разному убить");
                break;
            case 3:
                GUI.Box(rects[6], "Настройки");
                GUI.Label(rects[7], "Громкость аудио");
                AudioListener.volume = GUI.HorizontalSlider(rects[8], AudioListener.volume, 0, 1);
                GUI.Label(rects[9], Mathf.Round(AudioListener.volume * 100).ToString());
                break;
            case 4:
                GUI.skin.box.fontSize = (int)(screenHeight * 0.08f);
                GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                GUI.Box(rects[6], "Разработчики:\nАлександр 2ec_End Ли\nКирилл Several_SIZE Нерадовский\nНикита Remos Бушуев\nОсобая благодарность:\nАнтон Sosuha Кособуцкий\n ");
                GUI.skin.box.fontSize = (int)(screenHeight * 0.04f);
                GUI.skin.box.alignment = TextAnchor.UpperCenter;
                break;
            case 5:
                GUI.skin.button.fontSize = (int)(screenHeight * 0.08f);
                if (GUI.Button(rects[6], "Вы точно хотите выйти?\n(((((((((((((((((((((((("))
                {
                    Application.Quit();
                }
                GUI.skin.button.fontSize = (int)(screenHeight * 0.04f);
                break;
        }
    }
    private void ChangeEnemy()
    {
        col.a = Mathf.Abs(Mathf.Sin((transform.rotation.eulerAngles.y - 90) / 20));
        if (col.a > 0.98f && !changed)
        {
            Enemies.GetChild(enemyNumber).gameObject.SetActive(false);
            enemyNumber = (enemyNumber + 1) % Enemies.childCount;
            Enemies.GetChild(enemyNumber).gameObject.SetActive(true);
            changed = true;
        }
        if (col.a < 0.02f && changed)
        {
            col.a = 0;
            changing = false;
            changed = false;
        }
        Black.SetPixel(0, 0, col);
        Black.Apply(false);
    }
}