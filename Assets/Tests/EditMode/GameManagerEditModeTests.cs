using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerEditModeTests
{
    private GameObject managerObject;
    private GameManager manager;
    private GameObject gameoverUI;
    private GameObject clearUI;
    private GameObject scoreTextObject;
    private TextMeshProUGUI scoreText;

    [SetUp]
    public void SetUp()
    {
        GameManager.instance = null;
        managerObject = new GameObject("GameManager");
        manager = managerObject.AddComponent<GameManager>();

        gameoverUI = new GameObject("GameOverUI");
        clearUI = new GameObject("ClearUI");
        gameoverUI.SetActive(false);
        clearUI.SetActive(false);

        scoreTextObject = new GameObject("ScoreText");
        scoreTextObject.AddComponent<RectTransform>();
        scoreTextObject.AddComponent<CanvasRenderer>();
        scoreText = scoreTextObject.AddComponent<TextMeshProUGUI>();

        manager.gameoverUI = gameoverUI;
        manager.clearUI = clearUI;
        manager.scoreText = scoreText;
        manager.InitializeForTest();
    }

    [TearDown]
    public void TearDown()
    {
        if (managerObject != null)
        {
            Object.DestroyImmediate(managerObject);
        }

        if (gameoverUI != null)
        {
            Object.DestroyImmediate(gameoverUI);
        }

        if (clearUI != null)
        {
            Object.DestroyImmediate(clearUI);
        }

        if (scoreTextObject != null)
        {
            Object.DestroyImmediate(scoreTextObject);
        }

        GameManager.instance = null;
    }

    [Test]
    public void InitializeForTest_AssignsSingletonInstance()
    {
        Assert.AreSame(manager, GameManager.instance);
    }

    [Test]
    public void InitializeForTest_WithExistingSingleton_KeepsOriginalInstance()
    {
        var originalManager = manager;
        var duplicateManagerObject = new GameObject("GameManager_Duplicate");
        var duplicateManager = duplicateManagerObject.AddComponent<GameManager>();

        LogAssert.Expect(LogType.Warning, "[GameManager] 씬에 2개 이상의 게임 매니저가 존재합니다.");
        duplicateManager.InitializeForTest();

        Assert.AreSame(originalManager, GameManager.instance);
        Assert.IsTrue(duplicateManagerObject == null);
    }

    [Test]
    public void IsRestartInputPressed_UsesMouseButtonsAndSpace()
    {
        Assert.IsTrue(GameManager.IsRestartInputPressed(true, false, false));
        Assert.IsTrue(GameManager.IsRestartInputPressed(false, true, false));
        Assert.IsTrue(GameManager.IsRestartInputPressed(false, false, true));
        Assert.IsFalse(GameManager.IsRestartInputPressed(false, false, false));
    }

    [Test]
    public void TryGetNextSceneForTest_ReturnsFalseWhenNextSceneNameIsEmpty()
    {
        Assert.IsFalse(GameManager.TryGetNextSceneForTest("", out string nextSceneName));
        Assert.AreEqual("", nextSceneName);
    }

    [Test]
    public void TryGetNextSceneForTest_ReturnsFalseWhenNextSceneNameIsWhitespace()
    {
        Assert.IsFalse(GameManager.TryGetNextSceneForTest("   ", out string nextSceneName));
        Assert.AreEqual("", nextSceneName);
    }

    [Test]
    public void TryGetNextSceneForTest_ReturnsNextSceneNameWhenConfigured()
    {
        Assert.IsTrue(GameManager.TryGetNextSceneForTest("RelicDash_ForestRuins_Stage02", out string nextSceneName));
        Assert.AreEqual("RelicDash_ForestRuins_Stage02", nextSceneName);
    }

    [Test]
    public void TryGetNextSceneForTest_TrimsConfiguredNextSceneName()
    {
        Assert.IsTrue(GameManager.TryGetNextSceneForTest(" RelicDash_ForestRuins_Stage02 ", out string nextSceneName));
        Assert.AreEqual("RelicDash_ForestRuins_Stage02", nextSceneName);
    }

    [Test]
    public void TryGetStageNumberForTest_ParsesTrailingDigits()
    {
        Assert.IsTrue(GameManager.TryGetStageNumberForTest("FR_Stage02", out int stageNumber));
        Assert.AreEqual(2, stageNumber);
    }

    [Test]
    public void TryGetStageNumberForTest_ReturnsFalseWhenSceneNameHasNoTrailingDigits()
    {
        Assert.IsFalse(GameManager.TryGetStageNumberForTest("Intro", out int stageNumber));
        Assert.AreEqual(0, stageNumber);
    }

    [Test]
    public void TryGetStageToUnlockForTest_UnlocksNextStageFromCurrentStageScene()
    {
        Assert.IsTrue(GameManager.TryGetStageToUnlockForTest("FR_Stage03", out int stageToUnlock));
        Assert.AreEqual(4, stageToUnlock);
    }

    [Test]
    public void OnPlayerDead_ActivatesGameOverStateOnly()
    {
        manager.OnPlayerDead();

        Assert.IsTrue(manager.isGameover);
        Assert.IsFalse(manager.isCleared);
        Assert.IsTrue(gameoverUI.activeSelf);
        Assert.IsFalse(clearUI.activeSelf);
    }

    [Test]
    public void OnStageCleared_ActivatesClearStateOnly()
    {
        manager.OnStageCleared();

        Assert.IsTrue(manager.isCleared);
        Assert.IsFalse(manager.isGameover);
        Assert.IsTrue(clearUI.activeSelf);
        Assert.IsFalse(gameoverUI.activeSelf);
    }

    [Test]
    public void OnPlayerDead_ThenOnStageCleared_RemainsGameoverOnly()
    {
        manager.OnPlayerDead();
        manager.OnStageCleared();

        Assert.IsTrue(manager.isGameover);
        Assert.IsFalse(manager.isCleared);
        Assert.IsTrue(gameoverUI.activeSelf);
        Assert.IsFalse(clearUI.activeSelf);
    }

    [Test]
    public void OnStageCleared_ThenOnPlayerDead_RemainsClearedOnly()
    {
        manager.OnStageCleared();
        manager.OnPlayerDead();

        Assert.IsTrue(manager.isCleared);
        Assert.IsFalse(manager.isGameover);
        Assert.IsTrue(clearUI.activeSelf);
        Assert.IsFalse(gameoverUI.activeSelf);
    }

    [Test]
    public void AddScore_AfterGameoverOrClear_DoesNotChangeDisplayedScore()
    {
        manager.AddScore(10);
        Assert.AreEqual("000010", scoreText.text);

        manager.OnPlayerDead();
        manager.AddScore(5);
        Assert.AreEqual("000010", scoreText.text);

        Object.DestroyImmediate(managerObject);
        GameManager.instance = null;

        managerObject = new GameObject("GameManager_ClearCase");
        manager = managerObject.AddComponent<GameManager>();
        manager.gameoverUI = gameoverUI;
        manager.clearUI = clearUI;
        manager.scoreText = scoreText;
        manager.InitializeForTest();

        manager.AddScore(3);
        Assert.AreEqual("000003", scoreText.text);

        manager.OnStageCleared();
        manager.AddScore(7);
        Assert.AreEqual("000003", scoreText.text);
    }
}
