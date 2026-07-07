using NUnit.Framework;
using UnityEngine;

public class DashRechargePickupEditModeTests
{
    private GameObject pickupObject;
    private DashRechargePickup pickup;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    [SetUp]
    public void SetUp()
    {
        pickupObject = new GameObject("DashRecharge Pickup");
        spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
        circleCollider = pickupObject.AddComponent<CircleCollider2D>();
        circleCollider.enabled = true;
        pickup = pickupObject.AddComponent<DashRechargePickup>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(pickupObject);
    }

    [Test]
    public void RespawnAfterSeconds_DefaultsToFiveSeconds()
    {
        Assert.AreEqual(5f, pickup.respawnAfterSeconds);
    }

    [Test]
    public void RespawnWarningSeconds_DefaultsToOneSecond()
    {
        Assert.AreEqual(1f, pickup.respawnWarningSeconds);
    }

    [Test]
    public void RespawnWarningColor_DefaultsToBrightCyan()
    {
        Assert.AreEqual(new Color(0.2f, 1f, 1f, 1f), pickup.respawnWarningColor);
    }

    [Test]
    public void CollectForTest_WithRespawn_DisablesVisiblePickupButKeepsGameObjectActive()
    {
        pickup.respawnAfterSeconds = 5f;

        pickup.CollectForTest();

        Assert.IsTrue(pickupObject.activeSelf);
        Assert.IsFalse(spriteRenderer.enabled);
        Assert.IsFalse(circleCollider.enabled);
        Assert.IsTrue(pickup.IsCollectedForTest);
    }

    [Test]
    public void ShowRespawnWarningForTest_ShowsRendererButKeepsColliderDisabled()
    {
        pickup.respawnAfterSeconds = 5f;
        pickup.CollectForTest();

        pickup.ShowRespawnWarningForTest();

        Assert.IsTrue(spriteRenderer.enabled);
        Assert.IsFalse(circleCollider.enabled);
        Assert.IsTrue(pickup.IsCollectedForTest);
    }

    [Test]
    public void ShowRespawnWarningForTest_AppliesWarningColor()
    {
        spriteRenderer.color = Color.black;
        pickup.CollectForTest();

        pickup.ShowRespawnWarningForTest();

        Assert.AreEqual(pickup.respawnWarningColor, spriteRenderer.color);
    }

    [Test]
    public void RespawnForTest_ReenablesVisiblePickup()
    {
        pickup.respawnAfterSeconds = 5f;
        pickup.CollectForTest();

        pickup.RespawnForTest();

        Assert.IsTrue(spriteRenderer.enabled);
        Assert.IsTrue(circleCollider.enabled);
        Assert.IsFalse(pickup.IsCollectedForTest);
    }

    [Test]
    public void RespawnForTest_RestoresRendererColorAfterWarning()
    {
        spriteRenderer.color = Color.black;
        pickup.CollectForTest();
        pickup.ShowRespawnWarningForTest();

        pickup.RespawnForTest();

        Assert.AreEqual(Color.black, spriteRenderer.color);
    }

    [Test]
    public void CollectForTest_WithoutRespawn_DeactivatesGameObject()
    {
        pickup.respawnAfterSeconds = 0f;

        pickup.CollectForTest();

        Assert.IsFalse(pickupObject.activeSelf);
    }
}
