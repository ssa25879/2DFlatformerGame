using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CodexDashPlatformerValidation
{
    private const string ScenePath = "Assets/Scene/Main.unity";
    private const float MinimumDashRechargeTriggerRadius = 1f;
    private const float ExpectedPlayerSpriteScale = 0.66f;
    private const float ExpectedUpwardDashDamping = 0.7f;
    private const float ExpectedMostlyUpwardDashThreshold = 0.85f;
    private const float DeadZoneHorizontalPadding = 2f;
    private const float DeadZoneWidthMultiplier = 1.4f;
    private const float DeadZoneY = -6.5f;
    private const float DeadZoneHeight = 1f;
    private static readonly Vector2 ExpectedPlayerColliderSize = new Vector2(1f, 1.5f);

    public static void ValidatePrototype()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var errors = new List<string>();
        var warnings = new List<string>();

        ValidatePlayer(errors, warnings);
        ValidateGameManager(errors, warnings);
        ValidateDashStatusUI(errors);
        ValidateCamera(errors);
        ValidateStageObjects(errors, warnings);
        ValidateRunnerComponents(warnings);

        foreach (string warning in warnings)
        {
            Debug.LogWarning("[CodexDashPlatformerValidation] " + warning);
        }

        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError("[CodexDashPlatformerValidation] " + error);
            }

            throw new System.InvalidOperationException("Dash platformer validation failed with " + errors.Count + " error(s).");
        }

        Debug.Log("[CodexDashPlatformerValidation] Validation passed with " + warnings.Count + " warning(s).");
    }

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

    public static bool DeadZoneCoversHorizontalRangeForTest(Vector2 deadZonePosition, Vector2 deadZoneSize, Vector2 targetPosition)
    {
        float halfWidth = deadZoneSize.x * 0.5f;
        return targetPosition.x >= deadZonePosition.x - halfWidth
            && targetPosition.x <= deadZonePosition.x + halfWidth;
    }

    public static void CalculateDeadZoneBoundsForTest(
        IReadOnlyList<Vector2> stagePoints,
        float horizontalPadding,
        float y,
        float height,
        out Vector2 position,
        out Vector2 size)
    {
        CalculateDeadZoneBoundsForTest(stagePoints, horizontalPadding, 1f, y, height, out position, out size);
    }

    public static void CalculateDeadZoneBoundsForTest(
        IReadOnlyList<Vector2> stagePoints,
        float horizontalPadding,
        float widthMultiplier,
        float y,
        float height,
        out Vector2 position,
        out Vector2 size)
    {
        if (stagePoints == null || stagePoints.Count == 0)
        {
            position = new Vector2(0f, y);
            size = new Vector2(12f, height);
            return;
        }

        float minX = stagePoints[0].x;
        float maxX = stagePoints[0].x;
        for (int i = 1; i < stagePoints.Count; i++)
        {
            minX = Mathf.Min(minX, stagePoints[i].x);
            maxX = Mathf.Max(maxX, stagePoints[i].x);
        }

        float paddedMinX = minX - horizontalPadding;
        float paddedMaxX = maxX + horizontalPadding;
        float width = Mathf.Max(12f, paddedMaxX - paddedMinX);
        width *= Mathf.Max(1f, widthMultiplier);
        position = new Vector2((paddedMinX + paddedMaxX) * 0.5f, y);
        size = new Vector2(width, height);
    }

    private static void ValidatePlayer(List<string> errors, List<string> warnings)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            errors.Add("Player object is missing.");
            return;
        }

        RequireComponent<PlayerController>(player, errors);
        RequireComponent<DashFeedback>(player, errors);
        RequireComponent<DashAimIndicator>(player, errors);
        RequireComponent<LineRenderer>(player, errors);
        RequireComponent<TrailRenderer>(player, errors);
        RequireComponent<Rigidbody2D>(player, errors);
        RequireComponent<Collider2D>(player, errors);

        if (!player.CompareTag("Player"))
        {
            errors.Add("Player tag must be Player.");
        }

        Vector3 playerScale = player.transform.localScale;
        if (!Mathf.Approximately(playerScale.x, ExpectedPlayerSpriteScale)
            || !Mathf.Approximately(playerScale.y, ExpectedPlayerSpriteScale)
            || !Mathf.Approximately(playerScale.z, ExpectedPlayerSpriteScale))
        {
            errors.Add("Player localScale must be 0.66 on all axes.");
        }

        var playerCollider = player.GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            errors.Add("Player must have a BoxCollider2D.");
        }
        else if (!Mathf.Approximately(playerCollider.size.x, ExpectedPlayerColliderSize.x)
            || !Mathf.Approximately(playerCollider.size.y, ExpectedPlayerColliderSize.y))
        {
            errors.Add("Player BoxCollider2D size must be 1 x 1.5.");
        }

        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            if (controller.moveSpeed <= 0f)
            {
                errors.Add("PlayerController.moveSpeed must be positive.");
            }

            if (controller.dashSpeed <= 0f || controller.dashDuration <= 0f)
            {
                errors.Add("PlayerController dash settings must be positive.");
            }

            if (!Mathf.Approximately(controller.upwardDashDamping, ExpectedUpwardDashDamping))
            {
                errors.Add("PlayerController.upwardDashDamping must be 0.7.");
            }

            if (!Mathf.Approximately(controller.mostlyUpwardDashThreshold, ExpectedMostlyUpwardDashThreshold))
            {
                errors.Add("PlayerController.mostlyUpwardDashThreshold must be 0.85.");
            }
        }

        var feedback = player.GetComponent<DashFeedback>();
        if (feedback != null && feedback.targetRenderer == null)
        {
            errors.Add("DashFeedback.targetRenderer must be assigned.");
        }

        if (feedback != null && feedback.dashTrail == null)
        {
            errors.Add("DashFeedback.dashTrail must be assigned.");
        }

        var indicator = player.GetComponent<DashAimIndicator>();
        if (indicator != null)
        {
            if (indicator.player == null)
            {
                errors.Add("DashAimIndicator.player must be assigned.");
            }

            if (indicator.lineRenderer == null)
            {
                errors.Add("DashAimIndicator.lineRenderer must be assigned.");
            }
        }

        var animator = player.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController is AnimatorController animatorController)
        {
            string[] requiredParams = { "Grounded", "IsDashing", "Speed", "VerticalSpeed", "Die" };
            foreach (string param in requiredParams)
            {
                if (!animatorController.parameters.Any(p => p.name == param))
                {
                    warnings.Add("Animator parameter missing: " + param);
                }
            }

            ValidateAnimatorStates(animatorController, errors);
        }
        else
        {
            warnings.Add("Player Animator or AnimatorController is missing.");
        }
    }

    private static void ValidateGameManager(List<string> errors, List<string> warnings)
    {
        GameManager manager = Object.FindFirstObjectByType<GameManager>();
        if (manager == null)
        {
            errors.Add("GameManager is missing.");
            return;
        }

        if (manager.gameoverUI == null)
        {
            errors.Add("GameManager.gameoverUI is not assigned.");
        }

        if (manager.clearUI == null)
        {
            errors.Add("GameManager.clearUI is not assigned.");
        }

        int clearUiCount = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Count(t => t.name == "Clear UI" && t.gameObject.activeInHierarchy);
        if (clearUiCount > 1)
        {
            warnings.Add("Multiple Clear UI objects found: " + clearUiCount);
        }

        GameObject scoreText = FindSceneObject("Score Text");
        if (scoreText != null && scoreText.activeInHierarchy)
        {
            errors.Add("Score Text must be inactive for dash UI prototype.");
        }
    }

    private static void ValidateDashStatusUI(List<string> errors)
    {
        GameObject dashUI = FindSceneObject("Dash Ready UI");
        if (dashUI == null)
        {
            errors.Add("Dash Ready UI is missing.");
            return;
        }

        RequireComponent<DashStatusUI>(dashUI, errors);

        var statusUI = dashUI.GetComponent<DashStatusUI>();
        if (statusUI != null)
        {
            if (statusUI.player == null)
            {
                errors.Add("Dash Ready UI must reference PlayerController.");
            }

            if (statusUI.label == null)
            {
                errors.Add("Dash Ready UI must reference its TextMeshPro label.");
            }
        }
    }

    private static void ValidateCamera(List<string> errors)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            errors.Add("Main Camera is missing.");
            return;
        }

        var follow = camera.GetComponent<CameraFollow>();
        if (follow == null)
        {
            errors.Add("Main Camera is missing CameraFollow.");
        }
        else if (follow.target == null || follow.target.name != "Player")
        {
            errors.Add("CameraFollow target must be Player.");
        }
    }

    private static void ValidateStageObjects(List<string> errors, List<string> warnings)
    {
        ValidateTrigger<DashRechargePickup>("DashRecharge Pickup", errors);
        ValidateTrigger<GoalTrigger>("Goal", errors);

        GameObject trap = GameObject.Find("Trap");
        if (trap == null)
        {
            warnings.Add("Trap object is missing.");
        }
        else
        {
            if (!trap.CompareTag("Trap"))
            {
                errors.Add("Trap tag must be Trap.");
            }

            var trapCollider = trap.GetComponent<Collider2D>();
            if (trapCollider == null || !trapCollider.isTrigger)
            {
                errors.Add("Trap must have a trigger Collider2D.");
            }
        }

        var deadZones = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(t => t.CompareTag("Dead") && t.gameObject.activeInHierarchy)
            .ToArray();
        if (deadZones.Length == 0)
        {
            errors.Add("No Dead tagged object found.");
        }
        else if (deadZones.Length > 1)
        {
            warnings.Add("Multiple Dead tagged objects found: " + deadZones.Length);
        }

        ValidateStageExtension(errors);
    }

    private static void ValidateStageExtension(List<string> errors)
    {
        GameObject extendedPlatform = GameObject.Find("Extended Dash Platform");
        if (extendedPlatform == null)
        {
            errors.Add("Extended Dash Platform is missing.");
            return;
        }

        RequireComponent<BoxCollider2D>(extendedPlatform, errors);
        ValidatePlatformRenderer(extendedPlatform, errors);

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

            CircleCollider2D pickupCircle = extendedPickup.GetComponent<CircleCollider2D>();
            if (pickupCircle == null || pickupCircle.radius < MinimumDashRechargeTriggerRadius)
            {
                errors.Add("DashRecharge Pickup Extended trigger radius is too small for dash pickup timing.");
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

        GameObject respawnChallengePlatform = GameObject.Find("Respawn Challenge Platform");
        if (respawnChallengePlatform == null)
        {
            errors.Add("Respawn Challenge Platform is missing.");
            return;
        }

        RequireComponent<BoxCollider2D>(respawnChallengePlatform, errors);
        ValidatePlatformRenderer(respawnChallengePlatform, errors);

        GameObject respawnChallengePickup = GameObject.Find("DashRecharge Pickup Respawn Challenge");
        if (respawnChallengePickup == null)
        {
            errors.Add("DashRecharge Pickup Respawn Challenge is missing.");
        }
        else
        {
            RequireComponent<DashRechargePickup>(respawnChallengePickup, errors);
            Collider2D pickupCollider = respawnChallengePickup.GetComponent<Collider2D>();
            if (pickupCollider == null || !pickupCollider.isTrigger)
            {
                errors.Add("DashRecharge Pickup Respawn Challenge must have a trigger Collider2D.");
            }

            CircleCollider2D pickupCircle = respawnChallengePickup.GetComponent<CircleCollider2D>();
            if (pickupCircle == null || pickupCircle.radius < MinimumDashRechargeTriggerRadius)
            {
                errors.Add("DashRecharge Pickup Respawn Challenge trigger radius is too small for dash pickup timing.");
            }
        }

        GameObject respawnChallengeTrap = GameObject.Find("Trap Respawn Challenge");
        if (respawnChallengeTrap == null)
        {
            errors.Add("Trap Respawn Challenge is missing.");
        }
        else
        {
            if (!respawnChallengeTrap.CompareTag("Trap"))
            {
                errors.Add("Trap Respawn Challenge tag must be Trap.");
            }

            Collider2D trapCollider = respawnChallengeTrap.GetComponent<Collider2D>();
            if (trapCollider == null || !trapCollider.isTrigger)
            {
                errors.Add("Trap Respawn Challenge must have a trigger Collider2D.");
            }
        }

        if (!IsGoalAfterExtendedSectionForTest(goal.transform.position, respawnChallengePlatform.transform.position))
        {
            errors.Add("Goal must be placed after Respawn Challenge Platform.");
        }

        if (respawnChallengePickup != null && !IsPickupBetweenPlatformsForTest(respawnChallengePickup.transform.position, extendedPlatform.transform.position, respawnChallengePlatform.transform.position))
        {
            errors.Add("DashRecharge Pickup Respawn Challenge must be between Extended Dash Platform and Respawn Challenge Platform.");
        }

        ValidateDeadZoneCoverage(errors);
    }

    private static void ValidateDeadZoneCoverage(List<string> errors)
    {
        GameObject deadZone = GameObject.Find("DeadZone");
        if (deadZone == null)
        {
            errors.Add("DeadZone is missing.");
            return;
        }

        var collider = deadZone.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            errors.Add("DeadZone must have a BoxCollider2D.");
            return;
        }

        Vector2[] stagePoints = GetStageCoveragePoints();
        CalculateDeadZoneBoundsForTest(stagePoints, DeadZoneHorizontalPadding, DeadZoneWidthMultiplier, DeadZoneY, DeadZoneHeight, out Vector2 expectedPosition, out Vector2 expectedSize);
        if (!Mathf.Approximately(deadZone.transform.position.x, expectedPosition.x)
            || !Mathf.Approximately(deadZone.transform.position.y, expectedPosition.y)
            || !Mathf.Approximately(collider.size.x, expectedSize.x)
            || !Mathf.Approximately(collider.size.y, expectedSize.y))
        {
            errors.Add("DeadZone must match current stage horizontal bounds.");
            return;
        }

        foreach (Vector2 stagePoint in stagePoints)
        {
            if (!DeadZoneCoversHorizontalRangeForTest(deadZone.transform.position, collider.size, stagePoint))
            {
                errors.Add("DeadZone must cover every stage horizontal point.");
                return;
            }
        }
    }

    private static Vector2[] GetStageCoveragePoints()
    {
        GameObject stage = GameObject.Find("Dash Platformer Stage");
        if (stage == null)
        {
            return new Vector2[0];
        }

        return stage.GetComponentsInChildren<Transform>(true)
            .Where(t => t != stage.transform && t.name != "DeadZone" && !t.name.Contains("Disabled Duplicate"))
            .Select(t => (Vector2)t.position)
            .ToArray();
    }

    private static void ValidateRunnerComponents(List<string> warnings)
    {
        CountEnabled<ScrollingObject>(warnings);
        CountEnabled<BackgroundLoop>(warnings);
        CountEnabled<PlatformSpawner>(warnings);
    }

    private static void ValidateTrigger<T>(string objectName, List<string> errors) where T : Component
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            errors.Add(objectName + " is missing.");
            return;
        }

        RequireComponent<T>(target, errors);
        var collider = target.GetComponent<Collider2D>();
        if (collider == null || !collider.isTrigger)
        {
            errors.Add(objectName + " must have a trigger Collider2D.");
        }

        var renderer = target.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            errors.Add(objectName + " must have a SpriteRenderer for test visibility.");
        }
        else if (renderer.sortingLayerName != "Foreground")
        {
            errors.Add(objectName + " SpriteRenderer must use Foreground sorting layer.");
        }
    }

    private static void RequireComponent<T>(GameObject target, List<string> errors) where T : Component
    {
        if (target.GetComponent<T>() == null)
        {
            errors.Add(target.name + " is missing " + typeof(T).Name + ".");
        }
    }

    private static void ValidatePlatformRenderer(GameObject platform, List<string> errors)
    {
        var renderer = platform.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            errors.Add(platform.name + " must have a SpriteRenderer.");
            return;
        }

        if (renderer.sortingLayerName != "Foreground")
        {
            errors.Add(platform.name + " SpriteRenderer must use Foreground sorting layer.");
        }
    }

    private static GameObject FindSceneObject(string objectName)
    {
        return Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(target => target.scene.isLoaded && target.name == objectName);
    }

    private static void CountEnabled<T>(List<string> warnings) where T : Behaviour
    {
        int enabledCount = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Count(component => component.enabled);
        if (enabledCount > 0)
        {
            warnings.Add(typeof(T).Name + " enabled count: " + enabledCount);
        }
    }

    private static void ValidateAnimatorStates(AnimatorController controller, List<string> errors)
    {
        string[] requiredStates = { "Idle", "Run", "Dash", "Fall", "Die" };
        string[] stateNames = controller.layers[0].stateMachine.states
            .Select(child => child.state.name)
            .ToArray();

        foreach (string state in requiredStates)
        {
            if (!stateNames.Contains(state))
            {
                errors.Add("Animator state missing: " + state);
            }
        }

        AnimatorState defaultState = controller.layers[0].stateMachine.defaultState;
        if (defaultState == null || defaultState.name != "Idle")
        {
            errors.Add("Animator default state must be Idle.");
        }
    }
}
