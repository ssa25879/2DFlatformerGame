using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isGameover = false;
    public bool isCleared = false;
    public TextMeshProUGUI scoreText;
    public GameObject gameoverUI;
    public GameObject clearUI;
    public float restartInputDelay = 0.25f;
    public string nextSceneName = "";

    private int score = 0;
    private float stateChangedTime;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Update()
    {
        if (CanLoadNextScene(out string sceneName))
        {
            UniRunLogger.Info("GameManager", "Loading next scene: " + sceneName, this);
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (!CanRestart())
        {
            return;
        }

        UniRunLogger.Info("GameManager", "Restarting scene: " + SceneManager.GetActiveScene().name, this);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool CanRestart()
    {
        if (!isGameover && !isCleared)
        {
            return false;
        }

        if (Time.time - stateChangedTime < restartInputDelay)
        {
            return false;
        }

        return IsRestartInputPressed(
            Input.GetMouseButtonDown(0),
            Input.GetMouseButtonDown(1),
            Input.GetKeyDown(KeyCode.Space));
    }

    private bool CanLoadNextScene(out string sceneName)
    {
        sceneName = "";
        if (!isCleared || isGameover)
        {
            return false;
        }

        if (Time.time - stateChangedTime < restartInputDelay)
        {
            return false;
        }

        return TryGetNextSceneForTest(nextSceneName, out sceneName);
    }

    public static bool IsRestartInputPressed(bool leftMouseDown, bool rightMouseDown, bool spaceDown)
    {
        return leftMouseDown || rightMouseDown || spaceDown;
    }

    public static bool TryGetNextSceneForTest(string configuredNextSceneName, out string sceneName)
    {
        sceneName = string.IsNullOrWhiteSpace(configuredNextSceneName) ? "" : configuredNextSceneName.Trim();
        return sceneName.Length > 0;
    }

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            UniRunLogger.Warning("GameManager", "씬에 2개 이상의 게임 매니저가 존재합니다.", this);
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }

    public void AddScore(int newScore)
    {
        if (isGameover || isCleared)
        {
            return;
        }

        score += newScore;
        UniRunLogger.Debug("GameManager", "Score added: " + newScore + ", Total: " + score, this);
        if (scoreText != null)
        {
            scoreText.text = score.ToString("000000");
        }
    }

    public void OnPlayerDead()
    {
        if (isCleared)
        {
            return;
        }

        isGameover = true;
        stateChangedTime = Time.time;
        UniRunLogger.Info("GameManager", "Game over state entered.", this);
        if (gameoverUI != null)
        {
            gameoverUI.SetActive(true);
        }

        if (clearUI != null)
        {
            clearUI.SetActive(false);
        }
    }

    public void OnStageCleared()
    {
        if (isGameover)
        {
            return;
        }

        isCleared = true;
        stateChangedTime = Time.time;
        UniRunLogger.Info("GameManager", "Stage clear state entered.", this);
        if (clearUI != null)
        {
            clearUI.SetActive(true);
        }

        if (gameoverUI != null)
        {
            gameoverUI.SetActive(false);
        }
    }

    public void InitializeForTest()
    {
        InitializeSingleton();
    }
}
