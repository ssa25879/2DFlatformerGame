using NUnit.Framework;
using UnityEngine;

public class DashFeedbackEditModeTests
{
    private GameObject playerObject;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private DashFeedback dashFeedback;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject("Player");
        spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
        trailRenderer = playerObject.AddComponent<TrailRenderer>();
        trailRenderer.emitting = false;

        dashFeedback = playerObject.AddComponent<DashFeedback>();
        dashFeedback.targetRenderer = spriteRenderer;
        dashFeedback.dashTrail = trailRenderer;
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

    [Test]
    public void SetDashing_WhenTrue_EnablesTrailEmission()
    {
        dashFeedback.SetDashing(true);

        Assert.IsTrue(trailRenderer.emitting);
    }

    [Test]
    public void SetDashing_WhenFalse_DisablesTrailEmission()
    {
        dashFeedback.SetDashing(true);

        dashFeedback.SetDashing(false);

        Assert.IsFalse(trailRenderer.emitting);
    }
}
