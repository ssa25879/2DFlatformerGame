using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float dashSpeed = 18f;
    public float dashDuration = 0.16f;
    [Range(0f, 1f)]
    public float upwardDashDamping = 0.7f;
    [Range(0f, 1f)]
    public float mostlyUpwardDashThreshold = 0.85f;

    [Header("Audio")]
    public AudioClip dashClip;
    public AudioClip deathClip;

    private Rigidbody2D playerRigidbody;
    private Animator animator;
    private AudioSource playerAudio;
    private DashFeedback dashFeedback;
    private SpriteRenderer spriteRenderer;
    private readonly HashSet<int> groundContactColliders = new HashSet<int>();

    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;
    private bool isDead;
    private bool isClearLocked;
    private float horizontalInput;
    private float mobileHorizontalInput;
    private int groundContactCount;

    public bool CanDash => canDash;
    public bool IsDead => isDead;
    public bool IsControlLocked => isDead || isClearLocked || isDashing;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        dashFeedback = GetComponent<DashFeedback>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isDead || isClearLocked)
        {
            UpdateAnimator();
            return;
        }

        float keyboardInput = Input.GetAxisRaw("Horizontal");
        horizontalInput = CombineHorizontalInputForTest(keyboardInput, mobileHorizontalInput);

        if (IsDashInputPressed() && canDash && !IsPointerOverUI())
        {
            StartCoroutine(Dash());
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (IsControlLocked || playerRigidbody == null)
        {
            return;
        }

        playerRigidbody.linearVelocity = new Vector2(horizontalInput * moveSpeed, playerRigidbody.linearVelocity.y);
        ApplyFacingFromDirection(horizontalInput);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        SetDashFeedback(true);

        Vector2 dashDirection = GetDashDirection();
        ApplyFacingFromDirection(dashDirection.x);
        playerRigidbody.linearVelocity = dashDirection * dashSpeed;
        UniRunLogger.Info("Player", "Dash started. Direction: " + dashDirection + ", Speed: " + dashSpeed, this);

        if (playerAudio != null && dashClip != null)
        {
            playerAudio.PlayOneShot(dashClip);
        }

        UpdateAnimator();
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        SetDashFeedback(false);
        if (ShouldRechargeDashAfterDashForTest(isGrounded))
        {
            RechargeDash();
        }

        UpdateAnimator();
    }

    private Vector2 GetDashDirection()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return transform.localScale.x < 0f ? Vector2.left : Vector2.right;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (Vector2)(mouseWorld - transform.position);

        if (direction.sqrMagnitude < 0.001f)
        {
            return transform.localScale.x < 0f ? Vector2.left : Vector2.right;
        }

        return AdjustDashVector(direction, upwardDashDamping, mostlyUpwardDashThreshold);
    }

    private static bool IsDashInputPressed()
    {
        return ShouldStartDashForTest(
            Input.GetMouseButtonDown(0),
            Input.GetMouseButtonDown(1),
            Input.GetKeyDown(KeyCode.Space));
    }

    public static bool ShouldStartDashForTest(bool leftMouseDown, bool rightMouseDown, bool spaceDown)
    {
        return leftMouseDown || rightMouseDown;
    }

    private static Vector2 AdjustDashVector(Vector2 direction, float upwardDamping, float mostlyUpwardThreshold)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return Vector2.right;
        }

        Vector2 normalized = direction.normalized;
        if (normalized.y <= 0f)
        {
            return normalized;
        }

        normalized.y = Mathf.Min(normalized.y, Mathf.Clamp01(upwardDamping));
        return normalized;
    }

    public static Vector2 AdjustDashVectorForTest(Vector2 direction, float upwardDamping, float mostlyUpwardThreshold)
    {
        return AdjustDashVector(direction, upwardDamping, mostlyUpwardThreshold);
    }

    private void ApplyFacingFromDirection(float directionX)
    {
        if (spriteRenderer == null || Mathf.Abs(directionX) < 0.01f)
        {
            return;
        }

        spriteRenderer.flipX = ShouldFaceLeftForTest(directionX);
    }

    public static bool ShouldFaceLeftForTest(float directionX)
    {
        return directionX < -0.01f;
    }

    public void SetMobileHorizontalInput(float value)
    {
        mobileHorizontalInput = value;
    }

    public float MobileHorizontalInputForTest => mobileHorizontalInput;

    public static float CombineHorizontalInputForTest(float keyboardInput, float mobileInput)
    {
        return Mathf.Abs(keyboardInput) > 0.01f ? keyboardInput : mobileInput;
    }

    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    public void RechargeDash()
    {
        bool wasAvailable = canDash;
        canDash = true;
        if (!wasAvailable)
        {
            UniRunLogger.Info("Player", "Dash recharged.", this);
        }
    }

    public void LockForClear()
    {
        isClearLocked = true;
        UniRunLogger.Info("Player", "Player controls locked after stage clear.", this);

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        UpdateAnimator();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isDashing = false;
        SetDashFeedback(false);
        UniRunLogger.Warning("Player", "Player died.", this);

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (playerAudio != null && deathClip != null)
        {
            playerAudio.clip = deathClip;
            playerAudio.Play();
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlayerDead();
        }

        UpdateAnimator();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Dead") || other.CompareTag("Trap")) && !isDead)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RegisterGroundContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        RegisterGroundContact(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        UnregisterGroundContact(collision);
    }

    private bool HasGroundContact(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.7f)
            {
                return true;
            }
        }

        return false;
    }

    private void RegisterGroundContact(Collision2D collision)
    {
        bool hasGroundContact = HasGroundContact(collision);
        if (!hasGroundContact)
        {
            return;
        }

        bool wasGrounded = groundContactCount > 0;
        int colliderId = collision.collider.GetInstanceID();
        if (groundContactColliders.Add(colliderId))
        {
            groundContactCount++;
        }

        isGrounded = groundContactCount > 0;
        if (ShouldRechargeDashFromGroundContactForTest(wasGrounded, isGrounded))
        {
            RechargeDash();
        }

        UpdateAnimator();
    }

    public static bool ShouldRechargeDashFromGroundContactForTest(bool wasGrounded, bool hasGroundContact)
    {
        return !wasGrounded && hasGroundContact;
    }

    public static bool ShouldRechargeDashAfterDashForTest(bool isCurrentlyGrounded)
    {
        return isCurrentlyGrounded;
    }

    private void UnregisterGroundContact(Collision2D collision)
    {
        int colliderId = collision.collider.GetInstanceID();
        bool hadGroundContact = HasGroundContact(collision) || groundContactColliders.Contains(colliderId);

        if (!hadGroundContact)
        {
            return;
        }

        if (groundContactColliders.Remove(colliderId))
        {
            groundContactCount = Mathf.Max(0, groundContactCount - 1);
        }

        if (groundContactCount <= 0)
        {
            groundContactCount = 0;
            isGrounded = false;
        }

        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator == null || playerRigidbody == null)
        {
            return;
        }

        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("IsDashing", isDashing);
        animator.SetFloat("Speed", Mathf.Abs(playerRigidbody.linearVelocity.x));
        animator.SetFloat("VerticalSpeed", playerRigidbody.linearVelocity.y);
    }

    public void ConsumeDashForTest()
    {
        canDash = false;
    }

    public void KillForTest()
    {
        Die();
    }

    private void SetDashFeedback(bool dashing)
    {
        if (dashFeedback != null)
        {
            dashFeedback.SetDashing(dashing);
        }
    }
}
