using System.Collections;
using UnityEngine;

public class DashRechargePickup : MonoBehaviour
{
    public bool deactivateOnCollect = true;
    public float respawnAfterSeconds = 5f;
    public float respawnWarningSeconds = 1f;
    public Color respawnWarningColor = new Color(0.2f, 1f, 1f, 1f);

    private bool isCollected;
    private Coroutine respawnCoroutine;
    private SpriteRenderer[] renderers;
    private Color[] defaultRendererColors;

    public bool IsCollectedForTest => isCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected)
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        player.RechargeDash();
        UniRunLogger.Info("DashRechargePickup", "Pickup collected by " + player.name + ".", this);
        Collect(true);
    }

    private void Collect(bool startRespawnTimer)
    {
        if (!deactivateOnCollect)
        {
            return;
        }

        if (respawnAfterSeconds <= 0f)
        {
            UniRunLogger.Info("DashRechargePickup", "Pickup collected and deactivated without respawn.", this);
            gameObject.SetActive(false);
            return;
        }

        isCollected = true;
        SetPickupEnabled(false);
        UniRunLogger.Info("DashRechargePickup", "Pickup hidden. Respawn in " + respawnAfterSeconds + " seconds.", this);

        if (startRespawnTimer && gameObject.activeInHierarchy)
        {
            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
            }

            respawnCoroutine = StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        float hiddenSeconds = Mathf.Max(0f, respawnAfterSeconds - respawnWarningSeconds);
        if (hiddenSeconds > 0f)
        {
            yield return new WaitForSeconds(hiddenSeconds);
        }

        if (respawnWarningSeconds > 0f)
        {
            ShowRespawnWarning();
            yield return new WaitForSeconds(respawnWarningSeconds);
        }

        Respawn();
    }

    private void Respawn()
    {
        isCollected = false;
        respawnCoroutine = null;
        SetPickupEnabled(true);
        UniRunLogger.Info("DashRechargePickup", "Pickup respawned.", this);
    }

    private void SetPickupEnabled(bool enabled)
    {
        CacheRenderers();

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            renderer.enabled = enabled;
            if (enabled)
            {
                renderer.color = defaultRendererColors[i];
            }
        }

        foreach (Collider2D pickupCollider in GetComponentsInChildren<Collider2D>(true))
        {
            pickupCollider.enabled = enabled;
        }
    }

    private void ShowRespawnWarning()
    {
        CacheRenderers();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.enabled = true;
            renderer.color = respawnWarningColor;
        }

        foreach (Collider2D pickupCollider in GetComponentsInChildren<Collider2D>(true))
        {
            pickupCollider.enabled = false;
        }

        UniRunLogger.Info("DashRechargePickup", "Pickup respawn warning shown.", this);
    }

    private void CacheRenderers()
    {
        if (renderers != null)
        {
            return;
        }

        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        defaultRendererColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            defaultRendererColors[i] = renderers[i].color;
        }
    }

    public void CollectForTest()
    {
        Collect(false);
    }

    public void ShowRespawnWarningForTest()
    {
        ShowRespawnWarning();
    }

    public void RespawnForTest()
    {
        Respawn();
    }
}
