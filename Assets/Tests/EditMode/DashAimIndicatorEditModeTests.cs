using NUnit.Framework;
using UnityEngine;

public class DashAimIndicatorEditModeTests
{
    private GameObject indicatorObject;
    private DashAimIndicator indicator;

    [SetUp]
    public void SetUp()
    {
        indicatorObject = new GameObject("Dash Aim Indicator");
        indicator = indicatorObject.AddComponent<DashAimIndicator>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(indicatorObject);
    }

    [Test]
    public void DefaultsToShortVisibleAimLine()
    {
        Assert.AreEqual(1.4f, indicator.indicatorLength);
        Assert.AreEqual(0.08f, indicator.lineWidth);
        Assert.IsTrue(indicator.showWhenDashUnavailable);
    }

    [Test]
    public void ConfigureLineForTest_AssignsRendererShape()
    {
        LineRenderer line = indicatorObject.AddComponent<LineRenderer>();
        indicator.lineRenderer = line;

        indicator.ConfigureLineForTest();

        Assert.AreEqual(2, line.positionCount);
        Assert.AreEqual(indicator.lineWidth, line.startWidth);
        Assert.AreEqual(indicator.lineWidth, line.endWidth);
    }
}
