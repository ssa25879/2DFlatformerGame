using UnityEngine;

public static class StageProgress
{
    public const string UnlockedStageCountKey = "UniRun.UnlockedStageCount";

    public static int GetUnlockedStageCount()
    {
        return Mathf.Max(1, PlayerPrefs.GetInt(UnlockedStageCountKey, 1));
    }

    public static void UnlockStage(int stageNumber)
    {
        int currentUnlockedStageCount = GetUnlockedStageCount();
        if (stageNumber <= currentUnlockedStageCount)
        {
            return;
        }

        PlayerPrefs.SetInt(UnlockedStageCountKey, stageNumber);
        PlayerPrefs.Save();
    }

    public static bool IsStageUnlocked(int stageNumber)
    {
        return stageNumber <= GetUnlockedStageCount();
    }
}
