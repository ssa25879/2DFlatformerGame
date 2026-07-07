using UnityEngine;

public class DashAimIndicator : MonoBehaviour
{
    public PlayerController player;
    public LineRenderer lineRenderer;
    public float indicatorLength = 1.4f;
    public float lineWidth = 0.08f;
    public bool showWhenDashUnavailable = true;
    public Color readyColor = new Color(0.2f, 1f, 1f, 0.85f);
    public Color unavailableColor = new Color(0.2f, 1f, 1f, 0.25f);

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        ConfigureLine();
    }

    private void LateUpdate()
    {
        if (player == null || lineRenderer == null || Camera.main == null)
        {
            SetVisible(false);
            return;
        }

        if (player.IsControlLocked || (!player.CanDash && !showWhenDashUnavailable))
        {
            SetVisible(false);
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rawDirection = (Vector2)(mouseWorld - player.transform.position);
        if (rawDirection.sqrMagnitude < 0.001f)
        {
            SetVisible(false);
            return;
        }

        Vector2 dashDirection = PlayerController.AdjustDashVectorForTest(
            rawDirection,
            player.upwardDashDamping,
            player.mostlyUpwardDashThreshold);

        Color color = player.CanDash ? readyColor : unavailableColor;
        lineRenderer.startColor = color;
        lineRenderer.endColor = new Color(color.r, color.g, color.b, color.a * 0.35f);
        lineRenderer.SetPosition(0, player.transform.position);
        lineRenderer.SetPosition(1, player.transform.position + (Vector3)(dashDirection * indicatorLength));
        SetVisible(true);
    }

    private void ConfigureLine()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.sortingLayerName = "Foreground";
        lineRenderer.sortingOrder = 120;
    }

    private void SetVisible(bool visible)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible;
        }
    }

    public void ConfigureLineForTest()
    {
        ConfigureLine();
    }
}
