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

    private int score = 0;
    private float stateChangedTime;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Update()
    {
        if (!CanRestart())
        {
            return;
        }

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

    public static bool IsRestartInputPressed(bool leftMouseDown, bool rightMouseDown, bool spaceDown)
    {
        return leftMouseDown || rightMouseDown || spaceDown;
    }

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("씬에 2개 이상의 게임 매니저가 존재합니다.");
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
