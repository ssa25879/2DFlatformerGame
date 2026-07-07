using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private bool isTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered)
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        isTriggered = true;
        player.LockForClear();

        if (GameManager.instance != null)
        {
            GameManager.instance.OnStageCleared();
        }
    }
}
