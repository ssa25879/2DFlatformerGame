using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DashStatusUI : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI label;
    public Image background;
    public Image accent;
    public string readyText = "DASH READY";
    public string emptyText = "DASH EMPTY";
    public Color readyColor = new Color(0.14f, 0.20f, 0.19f, 1f);
    public Color emptyColor = new Color(0.36f, 0.40f, 0.39f, 1f);
    public Color readyBackgroundColor = new Color(0.93f, 0.95f, 0.93f, 0.96f);
    public Color emptyBackgroundColor = new Color(0.87f, 0.89f, 0.88f, 0.94f);
    public Color readyAccentColor = new Color(0.2f, 1f, 1f, 1f);
    public Color emptyAccentColor = new Color(0.56f, 0.61f, 0.6f, 1f);

    private void Awake()
    {
        if (label == null)
        {
            label = GetComponent<TextMeshProUGUI>();
        }

        if (background == null)
        {
            background = FindChildImage("Background");
        }

        if (accent == null)
        {
            accent = FindChildImage("Accent");
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (label == null || player == null)
        {
            return;
        }

        bool canDash = player.CanDash;
        label.text = canDash ? readyText : emptyText;
        label.color = canDash ? readyColor : emptyColor;

        if (background != null)
        {
            background.color = canDash ? readyBackgroundColor : emptyBackgroundColor;
        }

        if (accent != null)
        {
            accent.color = canDash ? readyAccentColor : emptyAccentColor;
        }
    }

    private Image FindChildImage(string namePart)
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
        {
            if (image.name.Contains(namePart))
            {
                return image;
            }
        }

        return null;
    }
}
