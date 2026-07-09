using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class RelicDashUILayoutEditModeTests
{
    private const string ScenePath = StageSceneConfig.Stage01Path;

    [SetUp]
    public void SetUp()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [Test]
    public void HudIcons_PreserveAspectRatio()
    {
        Assert.IsTrue(FindImage("HUDCanvas/HUD/ScorePlaque/RuneIcon").preserveAspect);
        Assert.IsTrue(FindImage("HUDCanvas/HUD/DashGauge/DashRuneSeed").preserveAspect);
    }

    [Test]
    public void ScorePlaque_LabelDoesNotOverlapRuneIcon()
    {
        RectTransform rune = FindImage("HUDCanvas/HUD/ScorePlaque/RuneIcon").rectTransform;
        RectTransform label = FindComponent<RectTransform>("HUDCanvas/HUD/ScorePlaque/ScoreLabel");

        Assert.IsFalse(LocalRectsOverlap(rune, label));
    }

    [Test]
    public void ScorePlaque_ValueHasVisibleArea()
    {
        TextMeshProUGUI value = FindComponent<TextMeshProUGUI>("HUDCanvas/HUD/ScorePlaque/ScoreValue");
        RectTransform rect = value.rectTransform;

        Assert.GreaterOrEqual(rect.sizeDelta.x, 120f);
        Assert.GreaterOrEqual(value.color.a, 0.95f);
        Assert.AreEqual("000000", value.text);
    }

    private static Image FindImage(string objectName)
    {
        return FindComponent<Image>(objectName);
    }

    private static T FindComponent<T>(string objectPath) where T : Component
    {
        GameObject target = GameObject.Find(objectPath);
        Assert.IsNotNull(target, objectPath);
        T component = target.GetComponent<T>();
        Assert.IsNotNull(component, objectPath);
        return component;
    }

    private static bool LocalRectsOverlap(RectTransform a, RectTransform b)
    {
        Rect aRect = RectInParent(a);
        Rect bRect = RectInParent(b);
        return aRect.Overlaps(bRect);
    }

    private static Rect RectInParent(RectTransform rect)
    {
        Vector2 size = rect.rect.size;
        Vector2 min = rect.anchoredPosition - Vector2.Scale(size, rect.pivot);
        return new Rect(min, size);
    }
}
