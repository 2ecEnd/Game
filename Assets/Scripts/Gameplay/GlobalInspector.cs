using UnityEngine;

public static class GlobalInspector
{
    private static int MeleeScore = 10;
    private static int RangeScore = 15;
    private static int CartScore = 15;

    public static int KilledMelee = 0;
    public static int KilledRange = 0;
    public static int KilledCart = 0;

    private static int DeathPenalty = 10;
    public static int DeathCount = 0;

    public static int GetScore()
    {
        int score = 0;

        score = KilledMelee * MeleeScore + KilledRange * RangeScore + KilledCart * CartScore;
        score -= DeathCount * DeathPenalty;

        return score;
    }
}
