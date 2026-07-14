using NUnit.Framework;
using UnityEngine;

public class StageProgressEditModeTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(StageProgress.UnlockedStageCountKey);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(StageProgress.UnlockedStageCountKey);
        PlayerPrefs.Save();
    }

    [Test]
    public void GetUnlockedStageCount_DefaultsToOne()
    {
        Assert.AreEqual(1, StageProgress.GetUnlockedStageCount());
    }

    [Test]
    public void UnlockStage_RaisesUnlockedStageCount()
    {
        StageProgress.UnlockStage(3);

        Assert.AreEqual(3, StageProgress.GetUnlockedStageCount());
        Assert.IsTrue(StageProgress.IsStageUnlocked(3));
    }

    [Test]
    public void UnlockStage_DoesNotLowerUnlockedStageCount()
    {
        StageProgress.UnlockStage(4);
        StageProgress.UnlockStage(2);

        Assert.AreEqual(4, StageProgress.GetUnlockedStageCount());
    }

    [Test]
    public void IsStageUnlocked_UsesHighestUnlockedStageNumber()
    {
        StageProgress.UnlockStage(2);

        Assert.IsTrue(StageProgress.IsStageUnlocked(1));
        Assert.IsTrue(StageProgress.IsStageUnlocked(2));
        Assert.IsFalse(StageProgress.IsStageUnlocked(3));
    }
}
