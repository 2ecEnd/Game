using Assets.Scripts.Gameplay;
using Assets.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine;

[System.Serializable]
public class EnemyStatistic
{
    public string Name;
    public int Kills;
    public int Score;
    public EnemyStatistic(string name, int score)
    {
        Name = name;
        Score = score;
    }
}
public static class GlobalInspector
{
    public static bool CheatAmmo = false;
    public static bool Win = false;
    public static int WaveNumber = 0;
    public static bool Rest = false;
    public static float MouseSensitivity = 1f;

    public static bool PlayerAlive = true;
    public static EnemyStatistic[] EnemyStatistics;

    private static int DeathPenalty = 10;
    public static int DeathCount = 0;

    public static PlayerGUI PlayerGUI;
    public static GameController GameController;
    public static PlayerCharacterController PlayerCharacterController;
    public static ArenaManager ArenaManager;
    public static int GetScore()
    {
        int score = 0;

        for (int i = 0; i < EnemyStatistics.Length; i++)
            score += EnemyStatistics[i].Score * EnemyStatistics[i].Kills;

        score -= DeathCount * DeathPenalty;

        return score;
    }
    public static string Statistics()
    {
        string st = "Общий счёт " + GetScore().ToString() + "\n";
        for (int i = 0; i < EnemyStatistics.Length; i++)
        {
            st += EnemyStatistics[i].Name + " " + EnemyStatistics[i].Kills.ToString() + "\n";
        }
        st += "Смертей " + DeathCount.ToString();
        return st;
    }
    public static void PlayerDeath(bool needScored = true)
    {
        //Time.timeScale = 0;
        PlayerCharacterController.Die();
        PlayerAlive = false;
        PlayerGUI.flag = 3;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (needScored)
        {
            DeathCount++;
        }
    }
    public static void PlayerRevive()
    {
        //Time.timeScale = 1;
        PlayerCharacterController.PRevive();
        PlayerAlive = true;
        PlayerGUI.flag = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public static void PlayerWin()
    {
        //Time.timeScale = 0;
        Win = true;
        PlayerGUI.flag = 4;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public static void Restart()
    {
        Win = false;
        GameController.WaveNumber = 0;
        GameController.NewWave();
        PlayerRevive();
        GameController.Restart();
        DeathCount = 0;
        for (int i = 0; i < EnemyStatistics.Length; i++)
            EnemyStatistics[i].Kills = 0;
        SceneManager.LoadScene(1);
    }
}
