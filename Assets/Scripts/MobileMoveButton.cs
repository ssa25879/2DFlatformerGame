using UnityEngine;
using UnityEngine.EventSystems;

public class MobileMoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerController player;
    public float direction = 1f;

    private void Awake()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player != null)
        {
            player.SetMobileHorizontalInput(direction);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (player != null)
        {
            player.SetMobileHorizontalInput(0f);
        }
    }
}
