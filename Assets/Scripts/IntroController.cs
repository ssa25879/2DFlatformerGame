using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    public string stageSelectSceneName = "StageSelect";

    private void Update()
    {
        if (!IsContinueInputPressedForTest(
            Input.GetMouseButtonDown(0),
            Input.GetMouseButtonDown(1),
            Input.GetKeyDown(KeyCode.Space)))
        {
            return;
        }

        UniRunLogger.Info("IntroController", "Loading stage select scene: " + stageSelectSceneName, this);
        SceneManager.LoadScene(stageSelectSceneName);
    }

    public static bool IsContinueInputPressedForTest(bool leftMouseDown, bool rightMouseDown, bool spaceDown)
    {
        return leftMouseDown || rightMouseDown || spaceDown;
    }
}
