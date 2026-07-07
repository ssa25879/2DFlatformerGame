using UnityEngine;

public sealed class EnvironmentTreeIdleAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 1f;
    [SerializeField] private float startOffset;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (targetRenderer == null || frames == null || frames.Length == 0 || framesPerSecond <= 0f)
        {
            return;
        }

        int frameIndex = Mathf.FloorToInt((Time.time + startOffset) * framesPerSecond) % frames.Length;
        targetRenderer.sprite = frames[frameIndex];
    }
}
