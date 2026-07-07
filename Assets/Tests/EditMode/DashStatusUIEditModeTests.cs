using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DashStatusUIEditModeTests
{
    private GameObject playerObject;
    private PlayerController player;
    private GameObject uiObject;
    private TextMeshProUGUI label;
    private Image background;
    private Image accent;
    private DashStatusUI dashStatusUI;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject("Player");
        playerObject.AddComponent<Rigidbody2D>();
        playerObject.AddComponent<BoxCollider2D>();
        playerObject.AddComponent<AudioSource>();
        player = playerObject.AddComponent<PlayerController>();

        uiObject = new GameObject("Dash Ready UI");
        uiObject.AddComponent<RectTransform>();

        GameObject backgroundObject = new GameObject("Dash Ready UI Background");
        backgroundObject.transform.SetParent(uiObject.transform, false);
        backgroundObject.AddComponent<RectTransform>();
        backgroundObject.AddComponent<CanvasRenderer>();
        background = backgroundObject.AddComponent<Image>();

        GameObject accentObject = new GameObject("Dash Ready UI Accent");
        accentObject.transform.SetParent(uiObject.transform, false);
        accentObject.AddComponent<RectTransform>();
        accentObject.AddComponent<CanvasRenderer>();
        accent = accentObject.AddComponent<Image>();

        GameObject labelObject = new GameObject("Dash Ready UI Label");
        labelObject.transform.SetParent(uiObject.transform, false);
        labelObject.AddComponent<RectTransform>();
        labelObject.AddComponent<CanvasRenderer>();
        label = labelObject.AddComponent<TextMeshProUGUI>();

        dashStatusUI = uiObject.AddComponent<DashStatusUI>();
        dashStatusUI.player = player;
        dashStatusUI.label = label;
        dashStatusUI.background = background;
        SetImageFieldIfPresent("accent", accent);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(uiObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void Refresh_WhenDashIsAvailable_ShowsReadyText()
    {
        dashStatusUI.Refresh();

        Assert.AreEqual("DASH READY", label.text);
    }

    [Test]
    public void Refresh_WhenDashIsConsumed_ShowsEmptyText()
    {
        player.ConsumeDashForTest();

        dashStatusUI.Refresh();

        Assert.AreEqual("DASH EMPTY", label.text);
    }

    [Test]
    public void Refresh_UpdatesBackgroundColorWithDashState()
    {
        dashStatusUI.Refresh();

        Assert.AreEqual(dashStatusUI.readyBackgroundColor, background.color);

        player.ConsumeDashForTest();
        dashStatusUI.Refresh();

        Assert.AreEqual(dashStatusUI.emptyBackgroundColor, background.color);
    }

    [Test]
    public void Refresh_UpdatesAccentColorWithDashState()
    {
        FieldInfo accentField = typeof(DashStatusUI).GetField("accent");
        FieldInfo readyAccentColorField = typeof(DashStatusUI).GetField("readyAccentColor");
        FieldInfo emptyAccentColorField = typeof(DashStatusUI).GetField("emptyAccentColor");

        Assert.IsNotNull(accentField);
        Assert.IsNotNull(readyAccentColorField);
        Assert.IsNotNull(emptyAccentColorField);

        dashStatusUI.Refresh();

        Assert.AreEqual((Color)readyAccentColorField.GetValue(dashStatusUI), accent.color);

        player.ConsumeDashForTest();
        dashStatusUI.Refresh();

        Assert.AreEqual((Color)emptyAccentColorField.GetValue(dashStatusUI), accent.color);
    }

    private void SetImageFieldIfPresent(string fieldName, Image value)
    {
        FieldInfo field = typeof(DashStatusUI).GetField(fieldName);
        if (field != null)
        {
            field.SetValue(dashStatusUI, value);
        }
    }
}
