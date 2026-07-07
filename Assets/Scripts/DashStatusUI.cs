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
    public Color readyColor = new Color(0.92f, 1f, 0.96f, 1f);
    public Color emptyColor = new Color(1f, 0.82f, 0.78f, 1f);
    public Color readyBackgroundColor = new Color(1f, 1f, 1f, 0.94f);
    public Color emptyBackgroundColor = new Color(0.95f, 0.82f, 0.78f, 0.9f);
    public Color readyAccentColor = Color.white;
    public Color emptyAccentColor = new Color(1f, 0.55f, 0.45f, 1f);

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
