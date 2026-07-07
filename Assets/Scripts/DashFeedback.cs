using UnityEngine;

public class DashFeedback : MonoBehaviour
{
    public SpriteRenderer targetRenderer;
    public TrailRenderer dashTrail;
    public Color dashColor = new Color(0.35f, 0.9f, 1f, 1f);

    private Color defaultColor = Color.white;
    private bool hasDefaultColor;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        if (dashTrail == null)
        {
            dashTrail = GetComponent<TrailRenderer>();
        }

        CaptureDefaultColor();
        SetDashing(false);
    }

    public void CaptureDefaultColor()
    {
        if (targetRenderer == null)
        {
            return;
        }

        defaultColor = targetRenderer.color;
        hasDefaultColor = true;
    }

    public void SetDashing(bool isDashing)
    {
        if (targetRenderer == null)
        {
            return;
        }

        if (!hasDefaultColor)
        {
            CaptureDefaultColor();
        }

        targetRenderer.color = isDashing ? dashColor : defaultColor;

        if (dashTrail != null)
        {
            dashTrail.emitting = isDashing;
        }
    }
}
