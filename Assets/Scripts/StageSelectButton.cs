using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StageSelectButton : MonoBehaviour
{
    public string targetSceneName;
    public int requiredStageNumber = 1;
    public TextMeshProUGUI label;
    public string unlockedLabel = "STAGE 1-1";
    public string lockedLabel = "LOCKED";
    public Color unlockedTextColor = new Color(0.93f, 0.95f, 0.93f, 1f);
    public Color lockedTextColor = new Color(0.55f, 0.62f, 0.6f, 1f);

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(LoadTargetScene);
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        bool isUnlocked = StageProgress.IsStageUnlocked(requiredStageNumber);
        button.interactable = isUnlocked;

        if (label != null)
        {
            label.text = isUnlocked ? unlockedLabel : lockedLabel;
            label.color = isUnlocked ? unlockedTextColor : lockedTextColor;
        }
    }

    private void LoadTargetScene()
    {
        if (!StageProgress.IsStageUnlocked(requiredStageNumber))
        {
            Refresh();
            return;
        }

        UniRunLogger.Info("StageSelectButton", "Loading stage scene: " + targetSceneName, this);
        SceneManager.LoadScene(targetSceneName);
    }
}
