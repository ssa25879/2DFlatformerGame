# Dash Platformer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert the existing Uni-Run Unity runner example into a short playable stage-clear 2D platformer with horizontal movement, mouse-button mouse-aimed dash, landing/item dash recharge, hazards, and a goal.

**Architecture:** Keep the existing Unity project and replace the runner-centered player flow with player-centered stage control. `PlayerController` owns movement, dash, recharge, death, and animation parameters; `GameManager` owns game state and restart flow; small focused trigger scripts handle pickups and goal completion; `CameraFollow` keeps the camera attached to the player.

**Tech Stack:** Unity 2D, C#, Rigidbody2D, Collider2D triggers/collisions, Animator, TextMeshPro UI, Unity Test Framework.

**Current Status - 2026-06-10:** The short playable prototype is implemented and manually verified in Unity Play Mode. Horizontal movement, left/right mouse dash, Space dash removal, air double-dash prevention, landing recharge, grounded downward-dash recharge, pickup recharge/deactivation, goal clear, Trap game-over, DeadZone game-over, and restart input have all been confirmed by Play Mode checks. Unity MCP compile checks report no C# compile errors. Automated scene validation passes with 0 warnings. Unity batchmode EditMode test execution is still pending because the project is currently open in another Unity Editor instance.

---

## File Structure

- Modify: `D:/work/Uni-Run/Assets/Scripts/PlayerController.cs`
  - Responsibility: horizontal movement, mouse-aimed dash, dash recharge, death handling, animation parameters.
- Modify: `D:/work/Uni-Run/Assets/Scripts/GameManager.cs`
  - Responsibility: game-over state, clear state, restart input, UI activation.
- Create: `D:/work/Uni-Run/Assets/Scripts/CameraFollow.cs`
  - Responsibility: smoothly follow the player while preserving camera Z.
- Create: `D:/work/Uni-Run/Assets/Scripts/DashRechargePickup.cs`
  - Responsibility: restore player dash once, then deactivate.
- Create: `D:/work/Uni-Run/Assets/Scripts/GoalTrigger.cs`
  - Responsibility: notify `GameManager` when the player reaches the goal.
- Create: `D:/work/Uni-Run/Assets/Tests/EditMode/PlayerControllerEditModeTests.cs`
  - Responsibility: verify dash recharge API and death/lock state helpers without needing Play Mode scene setup.
- Create: `D:/work/Uni-Run/Assets/Tests/EditMode/GameManagerEditModeTests.cs`
  - Responsibility: verify clear/game-over state transitions and UI activation.

Because `D:/work/Uni-Run` is not currently a git repository, each implementation task must create timestamped backups before modifying existing files. Commit steps are replaced with explicit backup/checkpoint verification.

---

### Task 1: Add PlayerController EditMode Tests

**Files:**
- Create: `D:/work/Uni-Run/Assets/Tests/EditMode/PlayerControllerEditModeTests.cs`
- No production code change in this task.

- [ ] **Step 1: Create the test folder**

Run:

```powershell
New-Item -ItemType Directory -Force -Path 'D:\work\Uni-Run\Assets\Tests\EditMode'
```

Expected: directory exists.

- [ ] **Step 2: Create the failing tests**

Create `D:/work/Uni-Run/Assets/Tests/EditMode/PlayerControllerEditModeTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class PlayerControllerEditModeTests
{
    private GameObject playerObject;
    private PlayerController controller;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject("Player");
        playerObject.AddComponent<Rigidbody2D>();
        playerObject.AddComponent<BoxCollider2D>();
        playerObject.AddComponent<AudioSource>();
        controller = playerObject.AddComponent<PlayerController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void RechargeDash_MakesDashAvailable()
    {
        controller.ConsumeDashForTest();

        controller.RechargeDash();

        Assert.IsTrue(controller.CanDash);
    }

    [Test]
    public void KillForTest_LocksControlAndMarksDead()
    {
        controller.KillForTest();

        Assert.IsTrue(controller.IsDead);
        Assert.IsTrue(controller.IsControlLocked);
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

Run from Unity Test Runner or Unity CLI:

```powershell
Unity.exe -batchmode -projectPath 'D:\work\Uni-Run' -runTests -testPlatform EditMode -testResults 'D:\work\Uni-Run\Logs\EditModeResults.xml' -quit
```

Expected: compile fails because `CanDash`, `IsDead`, `IsControlLocked`, `ConsumeDashForTest`, `RechargeDash`, and `KillForTest` do not exist yet.

- [ ] **Step 4: Checkpoint**

Run:

```powershell
Test-Path 'D:\work\Uni-Run\Assets\Tests\EditMode\PlayerControllerEditModeTests.cs'
```

Expected: `True`.

---

### Task 2: Replace PlayerController With Dash Movement

**Files:**
- Modify: `D:/work/Uni-Run/Assets/Scripts/PlayerController.cs`
- Test: `D:/work/Uni-Run/Assets/Tests/EditMode/PlayerControllerEditModeTests.cs`

- [ ] **Step 1: Back up the existing script**

Run:

```powershell
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupDir = 'D:\work\Uni-Run\Assets\Scripts\Backup'
New-Item -ItemType Directory -Force -Path $backupDir
Copy-Item 'D:\work\Uni-Run\Assets\Scripts\PlayerController.cs' "$backupDir\PlayerController.cs.$timestamp.bak"
```

Expected: a timestamped backup exists in `Assets/Scripts/Backup`.

- [ ] **Step 2: Implement dash-focused PlayerController**

Replace `D:/work/Uni-Run/Assets/Scripts/PlayerController.cs` with:

```csharp
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float dashSpeed = 18f;
    public float dashDuration = 0.16f;

    [Header("Audio")]
    public AudioClip dashClip;
    public AudioClip deathClip;

    private Rigidbody2D playerRigidbody;
    private Animator animator;
    private AudioSource playerAudio;

    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;
    private bool isDead;
    private bool isClearLocked;
    private float horizontalInput;

    public bool CanDash => canDash;
    public bool IsDead => isDead;
    public bool IsControlLocked => isDead || isClearLocked || isDashing;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isDead || isClearLocked)
        {
            UpdateAnimator();
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && canDash)
        {
            StartCoroutine(Dash());
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (IsControlLocked)
        {
            return;
        }

        playerRigidbody.linearVelocity = new Vector2(horizontalInput * moveSpeed, playerRigidbody.linearVelocity.y);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        Vector2 dashDirection = GetDashDirection();
        playerRigidbody.linearVelocity = dashDirection * dashSpeed;

        if (playerAudio != null && dashClip != null)
        {
            playerAudio.PlayOneShot(dashClip);
        }

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }

    private Vector2 GetDashDirection()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return transform.localScale.x < 0f ? Vector2.left : Vector2.right;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorld - transform.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return transform.localScale.x < 0f ? Vector2.left : Vector2.right;
        }

        return direction.normalized;
    }

    public void RechargeDash()
    {
        canDash = true;
    }

    public void LockForClear()
    {
        isClearLocked = true;
        playerRigidbody.linearVelocity = Vector2.zero;
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
        playerRigidbody.linearVelocity = Vector2.zero;

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
        if (HasGroundContact(collision))
        {
            isGrounded = true;
            RechargeDash();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (HasGroundContact(collision))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
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
}
```

- [ ] **Step 3: Run PlayerController tests**

Run:

```powershell
Unity.exe -batchmode -projectPath 'D:\work\Uni-Run' -runTests -testPlatform EditMode -testResults 'D:\work\Uni-Run\Logs\EditModeResults.xml' -quit
```

Expected: `PlayerControllerEditModeTests` pass. If Unity also runs unrelated failing tests, record them separately and confirm these two tests pass.

- [ ] **Step 4: Compile check**

Open Unity or run a batch compile. Expected: no C# compile errors for `PlayerController.cs`.

- [ ] **Step 5: Checkpoint**

Run:

```powershell
Get-ChildItem 'D:\work\Uni-Run\Assets\Scripts\Backup' -Filter 'PlayerController.cs.*.bak' | Select-Object -Last 1 FullName
```

Expected: backup path is printed.

---

### Task 3: Add GameManager State Tests

**Files:**
- Create: `D:/work/Uni-Run/Assets/Tests/EditMode/GameManagerEditModeTests.cs`
- No production code change in this task.

- [ ] **Step 1: Create failing GameManager tests**

Create `D:/work/Uni-Run/Assets/Tests/EditMode/GameManagerEditModeTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class GameManagerEditModeTests
{
    private GameObject managerObject;
    private GameManager manager;
    private GameObject gameoverUI;
    private GameObject clearUI;

    [SetUp]
    public void SetUp()
    {
        GameManager.instance = null;
        managerObject = new GameObject("GameManager");
        manager = managerObject.AddComponent<GameManager>();

        gameoverUI = new GameObject("GameOverUI");
        clearUI = new GameObject("ClearUI");
        gameoverUI.SetActive(false);
        clearUI.SetActive(false);

        manager.gameoverUI = gameoverUI;
        manager.clearUI = clearUI;
        manager.InitializeForTest();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(managerObject);
        Object.DestroyImmediate(gameoverUI);
        Object.DestroyImmediate(clearUI);
        GameManager.instance = null;
    }

    [Test]
    public void OnPlayerDead_ActivatesGameOverStateOnly()
    {
        manager.OnPlayerDead();

        Assert.IsTrue(manager.isGameover);
        Assert.IsFalse(manager.isCleared);
        Assert.IsTrue(gameoverUI.activeSelf);
        Assert.IsFalse(clearUI.activeSelf);
    }

    [Test]
    public void OnStageCleared_ActivatesClearStateOnly()
    {
        manager.OnStageCleared();

        Assert.IsTrue(manager.isCleared);
        Assert.IsFalse(manager.isGameover);
        Assert.IsTrue(clearUI.activeSelf);
        Assert.IsFalse(gameoverUI.activeSelf);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```powershell
Unity.exe -batchmode -projectPath 'D:\work\Uni-Run' -runTests -testPlatform EditMode -testResults 'D:\work\Uni-Run\Logs\EditModeResults.xml' -quit
```

Expected: compile fails because `clearUI`, `isCleared`, `InitializeForTest`, and `OnStageCleared` do not exist yet.

- [ ] **Step 3: Checkpoint**

Run:

```powershell
Test-Path 'D:\work\Uni-Run\Assets\Tests\EditMode\GameManagerEditModeTests.cs'
```

Expected: `True`.

---

### Task 4: Add Clear State To GameManager

**Files:**
- Modify: `D:/work/Uni-Run/Assets/Scripts/GameManager.cs`
- Test: `D:/work/Uni-Run/Assets/Tests/EditMode/GameManagerEditModeTests.cs`

- [ ] **Step 1: Back up the existing script**

Run:

```powershell
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupDir = 'D:\work\Uni-Run\Assets\Scripts\Backup'
New-Item -ItemType Directory -Force -Path $backupDir
Copy-Item 'D:\work\Uni-Run\Assets\Scripts\GameManager.cs' "$backupDir\GameManager.cs.$timestamp.bak"
```

Expected: a timestamped backup exists in `Assets/Scripts/Backup`.

- [ ] **Step 2: Implement clear state and restart flow**

Replace `D:/work/Uni-Run/Assets/Scripts/GameManager.cs` with:

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isGameover = false;
    public bool isCleared = false;
    public TextMeshProUGUI scoreText;
    public GameObject gameoverUI;
    public GameObject clearUI;

    private int score = 0;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Update()
    {
        if ((isGameover || isCleared) && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
    }

    public void AddScore(int newScore)
    {
        if (!isGameover && !isCleared)
        {
            score += newScore;
            if (scoreText != null)
            {
                scoreText.text = "Score : " + score;
            }
        }
    }

    public void OnPlayerDead()
    {
        if (isCleared)
        {
            return;
        }

        isGameover = true;
        if (gameoverUI != null)
        {
            gameoverUI.SetActive(true);
        }
        if (clearUI != null)
        {
            clearUI.SetActive(false);
        }
    }

    public void OnStageCleared()
    {
        if (isGameover)
        {
            return;
        }

        isCleared = true;
        if (clearUI != null)
        {
            clearUI.SetActive(true);
        }
        if (gameoverUI != null)
        {
            gameoverUI.SetActive(false);
        }
    }

    public void InitializeForTest()
    {
        InitializeSingleton();
    }
}
```

- [ ] **Step 3: Run GameManager tests**

Run:

```powershell
Unity.exe -batchmode -projectPath 'D:\work\Uni-Run' -runTests -testPlatform EditMode -testResults 'D:\work\Uni-Run\Logs\EditModeResults.xml' -quit
```

Expected: `GameManagerEditModeTests` pass.

- [ ] **Step 4: Compile check**

Expected: no C# compile errors for `GameManager.cs`.

- [ ] **Step 5: Checkpoint**

Run:

```powershell
Get-ChildItem 'D:\work\Uni-Run\Assets\Scripts\Backup' -Filter 'GameManager.cs.*.bak' | Select-Object -Last 1 FullName
```

Expected: backup path is printed.

---

### Task 5: Add CameraFollow

**Files:**
- Create: `D:/work/Uni-Run/Assets/Scripts/CameraFollow.cs`

- [ ] **Step 1: Create CameraFollow script**

Create `D:/work/Uni-Run/Assets/Scripts/CameraFollow.cs`:

```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1f, -10f);
    public float smoothTime = 0.15f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = offset.z;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
```

- [ ] **Step 2: Compile check**

Run Unity compile or batch test command. Expected: no C# compile errors.

- [ ] **Step 3: Scene wiring note**

In `Main.unity`, attach `CameraFollow` to `Main Camera` and assign `Player` to `target`. If using Coplay Unity MCP, perform this as an editor automation step.

- [ ] **Step 4: Checkpoint**

Run:

```powershell
Test-Path 'D:\work\Uni-Run\Assets\Scripts\CameraFollow.cs'
```

Expected: `True`.

---

### Task 6: Add DashRechargePickup

**Files:**
- Create: `D:/work/Uni-Run/Assets/Scripts/DashRechargePickup.cs`

- [ ] **Step 1: Create DashRechargePickup script**

Create `D:/work/Uni-Run/Assets/Scripts/DashRechargePickup.cs`:

```csharp
using UnityEngine;

public class DashRechargePickup : MonoBehaviour
{
    public bool deactivateOnCollect = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        player.RechargeDash();

        if (deactivateOnCollect)
        {
            gameObject.SetActive(false);
        }
    }
}
```

- [ ] **Step 2: Compile check**

Run Unity compile or batch test command. Expected: no C# compile errors.

- [ ] **Step 3: Scene wiring note**

Create a trigger object in `Main.unity` with `Collider2D.isTrigger = true` and `DashRechargePickup`. Place it in the air between platforms.

- [ ] **Step 4: Checkpoint**

Run:

```powershell
Test-Path 'D:\work\Uni-Run\Assets\Scripts\DashRechargePickup.cs'
```

Expected: `True`.

---

### Task 7: Add GoalTrigger

**Files:**
- Create: `D:/work/Uni-Run/Assets/Scripts/GoalTrigger.cs`

- [ ] **Step 1: Create GoalTrigger script**

Create `D:/work/Uni-Run/Assets/Scripts/GoalTrigger.cs`:

```csharp
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
```

- [ ] **Step 2: Compile check**

Run Unity compile or batch test command. Expected: no C# compile errors.

- [ ] **Step 3: Scene wiring note**

Create a trigger object in `Main.unity` with `Collider2D.isTrigger = true` and `GoalTrigger`. Place it at the end of the short stage.

- [ ] **Step 4: Checkpoint**

Run:

```powershell
Test-Path 'D:\work\Uni-Run\Assets\Scripts\GoalTrigger.cs'
```

Expected: `True`.

---

### Task 8: Scene Conversion Checklist

**Files:**
- Modify in Unity Editor: `D:/work/Uni-Run/Assets/Scene/Main.unity`

- [ ] **Step 1: Back up the scene**

Run:

```powershell
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupDir = 'D:\work\Uni-Run\Assets\Scene\Backup'
New-Item -ItemType Directory -Force -Path $backupDir
Copy-Item 'D:\work\Uni-Run\Assets\Scene\Main.unity' "$backupDir\Main.unity.$timestamp.bak"
```

Expected: a timestamped scene backup exists.

- [ ] **Step 2: Disable runner movement objects**

In `Main.unity`, disable components that create runner motion for the first prototype:

```text
Disable ScrollingObject on scrolling background/platform objects.
Disable BackgroundLoop on looping backgrounds.
Do not use PlatformSpawner for this prototype.
```

Expected: platforms and background no longer move backward automatically in Play Mode.

- [ ] **Step 3: Configure Player**

Set the `Player` object:

```text
Rigidbody2D: Dynamic
Rigidbody2D Gravity Scale: keep current project value unless movement feels unusable
Collider2D: enabled
PlayerController moveSpeed: 6
PlayerController dashSpeed: 18
PlayerController dashDuration: 0.16
PlayerController dashClip: assign existing jump/dash placeholder audio if available
PlayerController deathClip: keep existing death clip
```

Expected: Player can move through world space.

- [ ] **Step 4: Configure camera**

Set `Main Camera`:

```text
Add CameraFollow
CameraFollow target: Player transform
CameraFollow offset: (0, 1, -10)
CameraFollow smoothTime: 0.15
```

Expected: camera follows Player.

- [ ] **Step 5: Build a short stage**

Create static level pieces:

```text
Start platform at Player spawn.
Gap requiring one dash.
Middle platform with one airborne DashRecharge pickup.
Second gap requiring pickup-restored dash.
Goal trigger at the end.
DeadZone trigger below the full stage.
One Trap trigger on or near a platform.
```

Expected: stage can be completed in under one minute.

- [ ] **Step 6: Configure UI references**

Set `GameManager`:

```text
gameoverUI: existing game over UI
clearUI: create or assign clear UI object, inactive by default
scoreText: keep existing reference if present
```

Expected: game-over and clear UI can be activated separately.

- [ ] **Step 7: Animator parameter setup**

In `Player.controller`, add these parameters if missing:

```text
Grounded bool
IsDashing bool
Speed float
VerticalSpeed float
Die trigger
```

Expected: missing parameters do not cause runtime Animator warnings.

- [ ] **Step 8: Play Mode verification**

Manual Play Mode checks:

```text
A/D and left/right arrows move horizontally.
Left mouse dashes toward mouse.
Right mouse dashes toward mouse.
Space does not dash or jump upward by itself.
Dash cannot be repeated in air without landing or pickup.
Landing restores dash.
Pickup restores dash in air and deactivates.
Trap and DeadZone show game-over UI.
Goal shows clear UI.
Mouse left click or Space restarts after game-over or clear.
```

Expected: every item is observed in Play Mode.

---

### Task 9: Final Verification

**Files:**
- Read: `D:/work/Uni-Run/Logs/EditModeResults.xml`
- Read: Unity Console or Editor log.

- [ ] **Step 1: Run EditMode tests**

Run:

```powershell
Unity.exe -batchmode -projectPath 'D:\work\Uni-Run' -runTests -testPlatform EditMode -testResults 'D:\work\Uni-Run\Logs\EditModeResults.xml' -quit
```

Expected: EditMode tests pass.

- [ ] **Step 2: Confirm compile status**

Check Unity Console. Expected: no C# compile errors.

- [ ] **Step 3: Confirm scene behavior**

Run the scene in Play Mode and execute the Task 8 checklist. Expected: all gameplay verification criteria pass.

- [ ] **Step 4: Record work log**

Append to `D:/Codex/Log/20260610_Uni-Run_Log.md`:

```markdown
## 대시 플랫포머 구현 검증
- EditMode 테스트 실행 결과 기록.
- Unity 컴파일 에러 여부 기록.
- Play Mode 수동 검증 항목 기록.
- 생성/수정 파일 백업 위치 기록.
```

Expected: work log contains the verification summary.

---

## Self-Review

Spec coverage:

- Stage-clear conversion: Task 8.
- Horizontal movement: Task 2.
- No jump: Task 2 and Task 8 verification.
- Mouse-aimed dash: Task 2.
- Left mouse or right mouse dash: Task 2.
- Landing recharge: Task 2.
- Air recharge pickup: Task 6.
- Trap/death zone game over: Task 2 and Task 8.
- Goal clear: Task 4 and Task 7.
- Camera follow: Task 5.
- Temporary animation state parameters: Task 2 and Task 8.
- Compile/test verification: Task 9.

Placeholder scan:

- No placeholder markers or unspecified implementation steps remain.
- Gamepad aiming is explicitly out of scope.
- Camera clamping is explicitly out of scope.

Type consistency:

- `PlayerController.RechargeDash`, `PlayerController.LockForClear`, `PlayerController.CanDash`, `PlayerController.IsDead`, `PlayerController.IsControlLocked`, `PlayerController.ConsumeDashForTest`, and `PlayerController.KillForTest` are defined before use by tests and trigger scripts.
- `GameManager.isCleared`, `GameManager.clearUI`, `GameManager.InitializeForTest`, and `GameManager.OnStageCleared` are defined before use by tests and `GoalTrigger`.

