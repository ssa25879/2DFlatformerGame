using NUnit.Framework;
using UnityEngine;

public class StageExtensionValidationEditModeTests
{
    [Test]
    public void IsGoalAfterExtendedSectionForTest_ReturnsTrueWhenGoalIsPastFinalPlatform()
    {
        Assert.IsTrue(CodexDashPlatformerValidation.IsGoalAfterExtendedSectionForTest(
            new Vector2(13.2f, 0.4f),
            new Vector2(11f, -0.1f)));
    }

    [Test]
    public void IsGoalAfterExtendedSectionForTest_ReturnsFalseWhenGoalStaysInOriginalSection()
    {
        Assert.IsFalse(CodexDashPlatformerValidation.IsGoalAfterExtendedSectionForTest(
            new Vector2(7.4f, 0.4f),
            new Vector2(11f, -0.1f)));
    }

    [Test]
    public void IsPickupBetweenPlatformsForTest_ReturnsTrueForBridgePickup()
    {
        Assert.IsTrue(CodexDashPlatformerValidation.IsPickupBetweenPlatformsForTest(
            new Vector2(9.2f, 1.1f),
            new Vector2(6f, -0.4f),
            new Vector2(11f, -0.1f)));
    }

    [Test]
    public void IsPickupBetweenPlatformsForTest_ReturnsFalseWhenPickupIsBehindGoalPlatform()
    {
        Assert.IsFalse(CodexDashPlatformerValidation.IsPickupBetweenPlatformsForTest(
            new Vector2(4f, 1.1f),
            new Vector2(6f, -0.4f),
            new Vector2(11f, -0.1f)));
    }

    [Test]
    public void DeadZoneCoversHorizontalRangeForTest_ReturnsTrueWhenGoalIsInsideDeadZoneWidth()
    {
        Assert.IsTrue(CodexDashPlatformerValidation.DeadZoneCoversHorizontalRangeForTest(
            new Vector2(1f, -6.5f),
            new Vector2(40f, 1f),
            new Vector2(18.2f, 1.7f)));
    }

    [Test]
    public void DeadZoneCoversHorizontalRangeForTest_ReturnsFalseWhenGoalIsPastDeadZoneWidth()
    {
        Assert.IsFalse(CodexDashPlatformerValidation.DeadZoneCoversHorizontalRangeForTest(
            new Vector2(1f, -6.5f),
            new Vector2(18f, 1f),
            new Vector2(18.2f, 1.7f)));
    }

    [Test]
    public void CalculateDeadZoneBoundsForTest_ExpandsToCoverLongerStage()
    {
        Vector2[] stagePoints =
        {
            new Vector2(-8f, -2.8f),
            new Vector2(7.4f, 0.4f),
            new Vector2(18.2f, 1.7f),
            new Vector2(30f, 2f)
        };

        CodexDashPlatformerValidation.CalculateDeadZoneBoundsForTest(stagePoints, 2f, -6.5f, 1f, out Vector2 position, out Vector2 size);

        Assert.AreEqual(11f, position.x, 0.0001f);
        Assert.AreEqual(-6.5f, position.y, 0.0001f);
        Assert.AreEqual(42f, size.x, 0.0001f);
        Assert.AreEqual(1f, size.y, 0.0001f);
    }

    [Test]
    public void CalculateDeadZoneBoundsForTest_UsesMinimumWidthForShortStage()
    {
        Vector2[] stagePoints =
        {
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f)
        };

        CodexDashPlatformerValidation.CalculateDeadZoneBoundsForTest(stagePoints, 2f, -6.5f, 1f, out Vector2 position, out Vector2 size);

        Assert.AreEqual(0f, position.x, 0.0001f);
        Assert.AreEqual(12f, size.x, 0.0001f);
    }

    [Test]
    public void CalculateDeadZoneBoundsForTest_AppliesWidthMultiplierAroundStageCenter()
    {
        Vector2[] stagePoints =
        {
            new Vector2(-6f, -2.8f),
            new Vector2(18.2f, 1.7f)
        };

        CodexDashPlatformerValidation.CalculateDeadZoneBoundsForTest(stagePoints, 2f, 1.4f, -6.5f, 1f, out Vector2 position, out Vector2 size);

        Assert.AreEqual(6.1f, position.x, 0.0001f);
        Assert.AreEqual(39.48f, size.x, 0.0001f);
    }
}
