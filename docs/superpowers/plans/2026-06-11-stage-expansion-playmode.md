# Stage Expansion And Play Mode Verification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend the current dash platformer stage with one additional playable section, then verify the complete player flow in Play Mode without disturbing existing platform rendering fixes.

**Architecture:** Keep stage changes isolated in editor-only setup/validation scripts and reuse existing runtime components (`PlayerController`, `DashRechargePickup`, `GoalTrigger`, `DashFeedback`). Do not rebuild platform renderer/layer behavior globally; only add new named stage objects under the existing `Dash Platformer Stage` parent and move the existing Goal to the new endpoint.

**Tech Stack:** Unity 6000.3.10f1, C#, UnityEditor scene setup APIs, Unity Test Framework EditMode tests, existing batchmode `CodexEditModeTestRunner`.

---

## File Structure

- Modify `Assets/Editor/CodexDashPlatformerSceneSetup.cs`
  - Add an idempotent `ApplyStageExtension()` editor method.
  - Add helpers to create or update only named extension objects.
  - Do not call `BuildStage()` from this task.
- Modify `Assets/Editor/CodexDashPlatformerValidation.cs`
  - Validate the new extension objects, Goal placement, and required trigger components.
- Create `Assets/Tests/EditMode/StageExtensionValidationEditModeTests.cs`
  - Unit-test the new pure validation helpers before scene mutation.
- Modify `Assets/Scene/Main.unity`
  - Saved by `ApplyStageExtension()` only after backing up the scene.
- Write/update `D:\Codex\Log\20260611_Uni-Run_Log.md`
  - Record stage extension, validation, and test results.

## Constraints

- Do not change existing platform renderer colors, sorting layers, or layer names unless the named extension object is being created for the first time.
- Do not delete user-created objects.
- Do not call `ConfigureDashPlatformer()` or `BuildStage()` for this task because those rebuild the stage.
- Always run Unity with `-projectPath D:\work\Uni-Run` and confirm logs contain `Successfully changed project path to: D:\work\Uni-Run`.
- If another Unity instance has `D:\work\Uni-Run` open, stop and ask the user to close it before batchmode writes.

---

### Task 1: Add Stage Extension Validation Tests

**Files:**
- Create: `Assets/Tests/EditMode/StageExtensionValidationEditModeTests.cs`
- Modify: `Assets/Editor/CodexDashPlatformerValidation.cs`

- [ ] **Step 1: Write failing tests for extension positions**

Create `Assets/Tests/EditMode/StageExtensionValidationEditModeTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class StageExtensionValidationEditModeTests
{
    [Test]
    public void IsGoalAfterExtendedSectionForTest_ReturnsTrueWhenGoalIsPastFinalPlatform()
    {
        Assert.IsTrue(CodexDashPlatformerValidation.IsGoalAfterExtendedSectionForTest(
            new Vector2(13.2f, 0.4f),
            new Vector2(11f, -0.1f)));
    }

    [Test]
    public void IsGoalAfterExtendedSectionForTest_ReturnsFalseWhenGoalStaysInOriginalSection()
    {
        Assert.IsFalse(CodexDashPlatformerValidation.IsGoalAfterExtendedSectionForTest(
            new Vector2(7.4f, 0.4f),
            new Vector2(11f, -0.1f)));
    }

    [Test]
    public void IsPickupBetweenPlatformsForTest_ReturnsTrueForBridgePickup()
    {
        Assert.IsTrue(CodexDashPlatformerValidation.IsPickupBetweenPlatformsForTest(
            new Vector2(9.2f, 1.1f),
            new Vector2(6f, -0.4f),
            new Vector2(11f, -0.1f)));
    }

    [Test]
    public void IsPickupBetweenPlatformsForTest_ReturnsFalseWhenPickupIsBehindGoalPlatform()
    {
        Assert.IsFalse(CodexDashPlatformerValidation.IsPickupBetweenPlatformsForTest(
            new Vector2(4f, 1.1f),
            new Vector2(6f, -0.4f),
            new Vector2(11f, -0.1f)));
    }
}
```

- [ ] **Step 2: Run tests to verify RED**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe' -batchmode -quit -projectPath 'D:\work\Uni-Run' -executeMethod CodexEditModeTestRunner.RunEditModeTests -logFile 'D:\work\Uni-Run\Logs\Codex_StageExtension_RED.log'
```

Expected:
- First run may only compile.
- Re-run once if `Logs\EditModeResults.xml` is not updated.
- Final RED failure should be compile errors for missing `CodexDashPlatformerValidation.IsGoalAfterExtendedSectionForTest` and `IsPickupBetweenPlatformsForTest`.

- [ ] **Step 3: Add minimal pure validation helpers**

In `Assets/Editor/CodexDashPlatformerValidation.cs`, add these public static methods inside the class:

```csharp
public static bool IsGoalAfterExtendedSectionForTest(Vector2 goalPosition, Vector2 finalPlatformPosition)
{
    return goalPosition.x > finalPlatformPosition.x + 1.5f;
}

public static bool IsPickupBetweenPlatformsForTest(Vector2 pickupPosition, Vector2 startPlatformPosition, Vector2 finalPlatformPosition)
{
    return pickupPosition.x > startPlatformPosition.x + 1f
        && pickupPosition.x < finalPlatformPosition.x
        && pickupPosition.y > startPlatformPosition.y;
}
```

- [ ] **Step 4: Run tests to verify GREEN**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe' -batchmode -quit -projectPath 'D:\work\Uni-Run' -executeMethod CodexEditModeTestRunner.RunEditModeTests -logFile 'D:\work\Uni-Run\Logs\Codex_StageExtension_Task1.log'
```

Expected:
- `Logs\EditModeResults.xml` includes `testcasecount="26"`.
- All tests pass with `failed="0"`.

---

### Task 2: Add Idempotent Stage Extension Setup

**Files:**
- Modify: `Assets/Editor/CodexDashPlatformerSceneSetup.cs`
- Modify by Unity save: `Assets/Scene/Main.unity`

- [ ] **Step 1: Back up the editor setup script**

Run:

```powershell
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
New-Item -ItemType Directory -Force -Path 'Assets\Editor\Backup' | Out-Null
Copy-Item -LiteralPath 'Assets\Editor\CodexDashPlatformerSceneSetup.cs' -Destination "Assets\Editor\Backup\CodexDashPlatformerSceneSetup.cs.$timestamp.bak"
```

Expected:
- A new `.bak` file exists in `Assets\Editor\Backup`.

- [ ] **Step 2: Add `ApplyStageExtension()` method**

In `Assets/Editor/CodexDashPlatformerSceneSetup.cs`, add this public method after `ApplyAnimatorCleanup()`:

```csharp
public static void ApplyStageExtension()
{
    BackupScene();
    var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

    GameObject stage = GameObject.Find("Dash Platformer Stage");
    if (stage == null)
    {
        throw new InvalidOperationException("Dash Platformer Stage is missing.");
    }

    CreateOrUpdatePlatform(stage.transform, "Extended Dash Platform", new Vector2(11f, -0.1f), new Vector2(3.6f, 0.5f));
    CreateOrUpdateDashRecharge(stage.transform, "DashRecharge Pickup Extended", new Vector2(9.2f, 1.1f));
    CreateOrUpdateTrap(stage.transform, "Trap Extended", new Vector2(10.4f, 0.45f));
    MoveGoalToExtendedEndpoint(stage.transform, new Vector2(13.2f, 0.9f));

    EditorSceneManager.MarkSceneDirty(scene);
    EditorSceneManager.SaveScene(scene);
    AssetDatabase.SaveAssets();
    Debug.Log("[CodexDashPlatformerSceneSetup] Stage extension applied.");
}
```

- [ ] **Step 3: Add idempotent stage helper methods**

In `Assets/Editor/CodexDashPlatformerSceneSetup.cs`, add these private helpers near the existing stage creation helpers:

```csharp
private static GameObject FindChildOrCreate(Transform parent, string name)
{
    Transform child = parent.Find(name);
    if (child != null)
    {
        return child.gameObject;
    }

    GameObject created = new GameObject(name);
    created.transform.SetParent(parent);
    return created;
}

private static void CreateOrUpdatePlatform(Transform parent, string name, Vector2 position, Vector2 size)
{
    GameObject platform = FindChildOrCreate(parent, name);
    platform.transform.position = position;

    var renderer = EnsureComponent<SpriteRenderer>(platform);
    if (renderer.sprite == null)
    {
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Platform_Long.png")
            ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Platform.png");
    }

    renderer.drawMode = SpriteDrawMode.Sliced;
    renderer.size = size;

    var collider = EnsureComponent<BoxCollider2D>(platform);
    collider.isTrigger = false;
    collider.size = size;
}

private static void CreateOrUpdateDashRecharge(Transform parent, string name, Vector2 position)
{
    GameObject pickup = FindChildOrCreate(parent, name);
    pickup.transform.position = position;
    pickup.transform.localScale = Vector3.one * 0.35f;

    var renderer = EnsureComponent<SpriteRenderer>(pickup);
    if (renderer.sprite == null)
    {
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Toko_Jump.png")
            ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstacle.png");
    }

    ConfigureTestRenderer(renderer);

    var collider = EnsureComponent<CircleCollider2D>(pickup);
    collider.isTrigger = true;
    collider.radius = 0.6f;
    EnsureComponent<DashRechargePickup>(pickup);
}

private static void CreateOrUpdateTrap(Transform parent, string name, Vector2 position)
{
    GameObject trap = FindChildOrCreate(parent, name);
    trap.tag = "Trap";
    trap.transform.position = position;
    trap.transform.localScale = Vector3.one * 0.45f;

    var renderer = EnsureComponent<SpriteRenderer>(trap);
    if (renderer.sprite == null)
    {
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstacle.png");
    }

    ConfigureTestRenderer(renderer);

    var collider = EnsureComponent<BoxCollider2D>(trap);
    collider.isTrigger = true;
    collider.offset = Vector2.zero;
    collider.size = new Vector2(1f, 1f);
}

private static void MoveGoalToExtendedEndpoint(Transform stage, Vector2 position)
{
    GameObject goal = GameObject.Find("Goal");
    if (goal == null)
    {
        goal = new GameObject("Goal");
    }

    goal.transform.SetParent(stage, true);
    goal.transform.position = position;

    var renderer = EnsureComponent<SpriteRenderer>(goal);
    if (renderer.sprite == null)
    {
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Platform.png");
    }

    ConfigureTestRenderer(renderer);
    renderer.drawMode = SpriteDrawMode.Sliced;
    renderer.size = new Vector2(0.8f, 1.8f);

    var collider = EnsureComponent<BoxCollider2D>(goal);
    collider.isTrigger = true;
    collider.size = new Vector2(0.8f, 1.8f);
    EnsureComponent<GoalTrigger>(goal);
}
```

- [ ] **Step 4: Run compile check**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe' -batchmode -quit -projectPath 'D:\work\Uni-Run' -logFile 'D:\work\Uni-Run\Logs\Codex_StageExtension_Compile.log'
```

Expected:
- Log contains `Successfully changed project path to: D:\work\Uni-Run`.
- Log contains `Tundra build success`.
- No `error CS`.

- [ ] **Step 5: Apply stage extension to the scene**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe' -batchmode -quit -projectPath 'D:\work\Uni-Run' -executeMethod CodexDashPlatformerSceneSetup.ApplyStageExtension -logFile 'D:\work\Uni-Run\Logs\Codex_StageExtension_Apply.log'
```

Expected:
- If first run only compiles, run the same command once more with log file `Codex_StageExtension_Apply_2.log`.
- `Assets\Scene\Main.unity` contains:
  - `m_Name: Extended Dash Platform`
  - `m_Name: DashRecharge Pickup Extended`
  - `m_Name: Trap Extended`
  - `m_Name: Goal`
- A scene backup exists in `Assets\Scene\Backup\Main.unity.<timestamp>.bak`.

---

### Task 3: Extend Scene Validation

**Files:**
- Modify: `Assets/Editor/CodexDashPlatformerValidation.cs`

- [ ] **Step 1: Back up validation script**

Run:

```powershell
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
New-Item -ItemType Directory -Force -Path 'Assets\Editor\Backup' | Out-Null
Copy-Item -LiteralPath 'Assets\Editor\CodexDashPlatformerValidation.cs' -Destination "Assets\Editor\Backup\CodexDashPlatformerValidation.cs.$timestamp.bak"
```

Expected:
- A new `.bak` file exists in `Assets\Editor\Backup`.

- [ ] **Step 2: Add extension validation call**

In `ValidateStageObjects(List<string> errors, List<string> warnings)`, after existing trigger/deadzone validation, add:

```csharp
ValidateStageExtension(errors);
```

- [ ] **Step 3: Add `ValidateStageExtension` method**

In `Assets/Editor/CodexDashPlatformerValidation.cs`, add:

```csharp
private static void ValidateStageExtension(List<string> errors)
{
    GameObject extendedPlatform = GameObject.Find("Extended Dash Platform");
    if (extendedPlatform == null)
    {
        errors.Add("Extended Dash Platform is missing.");
        return;
    }

    RequireComponent<BoxCollider2D>(extendedPlatform, errors);

    GameObject extendedPickup = GameObject.Find("DashRecharge Pickup Extended");
    if (extendedPickup == null)
    {
        errors.Add("DashRecharge Pickup Extended is missing.");
    }
    else
    {
        RequireComponent<DashRechargePickup>(extendedPickup, errors);
        Collider2D pickupCollider = extendedPickup.GetComponent<Collider2D>();
        if (pickupCollider == null || !pickupCollider.isTrigger)
        {
            errors.Add("DashRecharge Pickup Extended must have a trigger Collider2D.");
        }
    }

    GameObject extendedTrap = GameObject.Find("Trap Extended");
    if (extendedTrap == null)
    {
        errors.Add("Trap Extended is missing.");
    }
    else
    {
        if (!extendedTrap.CompareTag("Trap"))
        {
            errors.Add("Trap Extended tag must be Trap.");
        }

        Collider2D trapCollider = extendedTrap.GetComponent<Collider2D>();
        if (trapCollider == null || !trapCollider.isTrigger)
        {
            errors.Add("Trap Extended must have a trigger Collider2D.");
        }
    }

    GameObject goal = GameObject.Find("Goal");
    if (goal == null)
    {
        errors.Add("Goal is missing.");
        return;
    }

    if (!IsGoalAfterExtendedSectionForTest(goal.transform.position, extendedPlatform.transform.position))
    {
        errors.Add("Goal must be placed after Extended Dash Platform.");
    }

    if (extendedPickup != null && !IsPickupBetweenPlatformsForTest(extendedPickup.transform.position, new Vector2(6f, -0.4f), extendedPlatform.transform.position))
    {
        errors.Add("DashRecharge Pickup Extended must be between the old goal platform and the extended platform.");
    }
}
```

- [ ] **Step 4: Run scene validation**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe' -batchmode -quit -projectPath 'D:\work\Uni-Run' -executeMethod CodexDashPlatformerValidation.ValidatePrototype -logFile 'D:\work\Uni-Run\Logs\Codex_StageExtension_Validation.log'
```

Expected:
- If first run only compiles, run the same command once more with log file `Codex_StageExtension_Validation_2.log`.
- Preferred success log contains `Validation passed`.
- If Unity exits after compile without `Validation passed`, verify scene YAML and EditMode tests; record the limitation in the work log.

---

### Task 4: Play Mode Verification

**Files:**
- Create or update: `Logs\PlayModeManualChecklist_StageExtension.md`
- Update: `D:\Codex\Log\20260611_Uni-Run_Log.md`

- [ ] **Step 1: Create manual Play Mode checklist**

Create `Logs\PlayModeManualChecklist_StageExtension.md` with:

```markdown
# Stage Extension Play Mode Checklist

Project: D:\work\Uni-Run
Scene: Assets/Scene/Main.unity

- [ ] Player starts on Start Platform.
- [ ] A/D or Left/Right moves player horizontally.
- [ ] Left mouse dash moves toward mouse direction.
- [ ] Right mouse dash moves toward mouse direction.
- [ ] Space does not start dash during gameplay.
- [ ] Dash Ready UI changes to empty after air dash.
- [ ] Landing recharges dash.
- [ ] DashRecharge Pickup Extended recharges dash and disappears.
- [ ] Trap Extended causes gameover.
- [ ] DeadZone still causes gameover.
- [ ] Goal at extended endpoint causes clear.
- [ ] After gameover, left click/right click/Space restarts.
- [ ] After clear, left click/right click/Space restarts.
```

- [ ] **Step 2: Run compile and EditMode tests before manual Play Mode**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe' -batchmode -quit -projectPath 'D:\work\Uni-Run' -executeMethod CodexEditModeTestRunner.RunEditModeTests -logFile 'D:\work\Uni-Run\Logs\Codex_StageExtension_EditMode.log'
```

Expected:
- `Logs\EditModeResults.xml` includes at least `testcasecount="26"`.
- `failed="0"`.

- [ ] **Step 3: Open Play Mode manually in Unity**

Use the Unity editor for `D:\work\Uni-Run`, not the other `Uni-Run` project. Confirm the title/path or project root before pressing Play.

Expected:
- All checklist items can be tested from the start of the scene without scene editing.

- [ ] **Step 4: Record Play Mode result**

Append to `D:\Codex\Log\20260611_Uni-Run_Log.md`:

```markdown

### Stage Extension Play Mode 확인
- 체크리스트 파일: `D:\work\Uni-Run\Logs\PlayModeManualChecklist_StageExtension.md`
- Play Mode 결과: [PASS 또는 확인 필요 항목 기록]
- EditMode 테스트 결과: `Logs/EditModeResults.xml`, testcasecount=[숫자], passed=[숫자], failed=0.
```

---

## Self-Review

- Spec coverage: The plan covers one new playable section, extended pickup/trap/goal validation, and Play Mode verification.
- Placeholder scan: No `TBD`, `TODO`, or unspecified helper names remain.
- Type consistency: The plan uses existing classes `DashRechargePickup`, `GoalTrigger`, `CodexEditModeTestRunner`, `CodexDashPlatformerSceneSetup`, and `CodexDashPlatformerValidation`.
- Scope control: The plan does not call `BuildStage()` and does not rebuild existing platform renderers or layers.
