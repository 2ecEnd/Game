using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class EnemyStatistic
{
    public int Kills;
    public int Score;
    public EnemyStatistic(int score)
    {
        Score = score;
    }
}
public static class GlobalInspector
{
    public static bool PlayerAlive = true;
    public static EnemyStatistic[] EnemyStatistics;

    //public static int KilledMelee = 0;
    //public static int KilledRange = 0;
    //public static int KilledCart = 0;

    //private static int MeleeScore = 10;
    //private static int RangeScore = 15;
    //private static int CartScore = 15;

    private static int DeathPenalty = 10;
    public static int DeathCount = 0;
    public static int GetScore()
    {
        int score = 0;

        for (int i = 0; i < EnemyStatistics.Length; i++)
        {
            score += EnemyStatistics[i].Score * EnemyStatistics[i].Kills;
        }

        //score = KilledMelee * MeleeScore + KilledRange * RangeScore + KilledCart * CartScore;

        score -= DeathCount * DeathPenalty;

        return score;
    }
    public static void PlayerDeath()
    {
        Time.timeScale = 0;
        PlayerAlive = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public static void PlayerRevive()
    {
        Time.timeScale = 1;
        PlayerAlive = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
