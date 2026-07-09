using NUnit.Framework;
using UnityEngine;

public class DashFeedbackEditModeTests
{
    private GameObject playerObject;
    private SpriteRenderer spriteRenderer;
    private DashFeedback dashFeedback;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject("Player");
        spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;

        dashFeedback = playerObject.AddComponent<DashFeedback>();
        dashFeedback.targetRenderer = spriteRenderer;
        dashFeedback.dashColor = Color.cyan;
        dashFeedback.CaptureDefaultColor();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void SetDashing_WhenTrue_AppliesDashColor()
    {
        dashFeedback.SetDashing(true);

        Assert.AreEqual(Color.cyan, spriteRenderer.color);
    }

    [Test]
    public void SetDashing_WhenFalse_RestoresDefaultColor()
    {
        dashFeedback.SetDashing(true);

        dashFeedback.SetDashing(false);

        Assert.AreEqual(Color.white, spriteRenderer.color);
    }
}
