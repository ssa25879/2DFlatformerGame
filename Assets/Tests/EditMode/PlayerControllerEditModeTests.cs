using NUnit.Framework;
using UnityEngine;

public class PlayerControllerEditModeTests
{
    private GameObject playerObject;
    private PlayerController controller;

    [SetUp]
    public void SetUp()
    {
        GameManager.instance = null;
        playerObject = new GameObject("Player");
        playerObject.AddComponent<Rigidbody2D>();
        playerObject.AddComponent<BoxCollider2D>();
        playerObject.AddComponent<AudioSource>();
        controller = playerObject.AddComponent<PlayerController>();
    }

    [TearDown]
    public void TearDown()
    {
        GameManager.instance = null;
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void RechargeDash_MakesDashAvailable()
    {
        controller.ConsumeDashForTest();

        controller.RechargeDash();

        Assert.IsTrue(controller.CanDash);
    }

    [Test]
    public void KillForTest_LocksControlAndMarksDead()
    {
        controller.KillForTest();

        Assert.IsTrue(controller.IsDead);
        Assert.IsTrue(controller.IsControlLocked);
    }

    [Test]
    public void ShouldStartDashForTest_UsesMouseButtonsButIgnoresSpace()
    {
        Assert.IsTrue(PlayerController.ShouldStartDashForTest(true, false, false));
        Assert.IsTrue(PlayerController.ShouldStartDashForTest(false, true, false));
        Assert.IsFalse(PlayerController.ShouldStartDashForTest(false, false, true));
    }

    [Test]
    public void AdjustDashVectorForTest_SoftensMostlyUpwardDash()
    {
        Vector2 direction = PlayerController.AdjustDashVectorForTest(Vector2.up, 0.7f, 0.85f);

        Assert.AreEqual(0f, direction.x, 0.0001f);
        Assert.AreEqual(0.7f, direction.y, 0.0001f);
    }

    [Test]
    public void AdjustDashVectorForTest_KeepsLowForwardDiagonalDash()
    {
        Vector2 input = new Vector2(1f, 0.5f).normalized;

        Vector2 direction = PlayerController.AdjustDashVectorForTest(input, 0.7f, 0.85f);

        Assert.AreEqual(input.x, direction.x, 0.0001f);
        Assert.AreEqual(input.y, direction.y, 0.0001f);
    }

    [Test]
    public void AdjustDashVectorForTest_DoesNotLetSteepDiagonalExceedPureUpHeight()
    {
        Vector2 pureUp = PlayerController.AdjustDashVectorForTest(Vector2.up, 0.7f, 0.85f);
        Vector2 steepDiagonal = PlayerController.AdjustDashVectorForTest(new Vector2(0.55f, 0.84f), 0.7f, 0.85f);

        Assert.LessOrEqual(steepDiagonal.y, pureUp.y + 0.0001f);
    }

    [Test]
    public void AdjustDashVectorForTest_DoesNotSoftenDownwardDash()
    {
        Vector2 input = new Vector2(0.25f, -1f).normalized;

        Vector2 direction = PlayerController.AdjustDashVectorForTest(input, 0.7f, 0.85f);

        Assert.AreEqual(input.x, direction.x, 0.0001f);
        Assert.AreEqual(input.y, direction.y, 0.0001f);
    }

    [Test]
    public void ShouldRechargeDashFromGroundContactForTest_OnlyRechargesWhenLanding()
    {
        Assert.IsTrue(PlayerController.ShouldRechargeDashFromGroundContactForTest(false, true));
        Assert.IsFalse(PlayerController.ShouldRechargeDashFromGroundContactForTest(true, true));
        Assert.IsFalse(PlayerController.ShouldRechargeDashFromGroundContactForTest(false, false));
    }

    [Test]
    public void ShouldRechargeDashAfterDashForTest_RechargesWhenStillGrounded()
    {
        Assert.IsTrue(PlayerController.ShouldRechargeDashAfterDashForTest(true));
        Assert.IsFalse(PlayerController.ShouldRechargeDashAfterDashForTest(false));
    }

    [Test]
    public void ShouldFaceLeftForTest_FollowsHorizontalDirection()
    {
        Assert.IsFalse(PlayerController.ShouldFaceLeftForTest(1f));
        Assert.IsTrue(PlayerController.ShouldFaceLeftForTest(-1f));
        Assert.IsFalse(PlayerController.ShouldFaceLeftForTest(0f));
    }

    [Test]
    public void CombineHorizontalInputForTest_PrefersKeyboardWhenPressed()
    {
        float combined = PlayerController.CombineHorizontalInputForTest(-1f, 1f);

        Assert.AreEqual(-1f, combined, 0.0001f);
    }

    [Test]
    public void CombineHorizontalInputForTest_FallsBackToMobileWhenKeyboardIdle()
    {
        float combined = PlayerController.CombineHorizontalInputForTest(0f, 1f);

        Assert.AreEqual(1f, combined, 0.0001f);
    }

    [Test]
    public void SetMobileHorizontalInput_UpdatesMobileHorizontalInputForTest()
    {
        controller.SetMobileHorizontalInput(-1f);

        Assert.AreEqual(-1f, controller.MobileHorizontalInputForTest, 0.0001f);
    }
}
