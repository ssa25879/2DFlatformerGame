using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CodexDashPlatformerSceneSetup
{
    private const string ScenePath = "Assets/Scene/Main.unity";
    private const string Stage01ScenePath = StageSceneConfig.Stage01Path;
    private const string Stage02ScenePath = StageSceneConfig.Stage02Path;
    private const string Stage03ScenePath = StageSceneConfig.Stage03Path;
    private const string Stage04ScenePath = StageSceneConfig.Stage04Path;
    private const string Stage02SceneName = StageSceneConfig.Stage02Name;
    private const string Stage03SceneName = StageSceneConfig.Stage03Name;
    private const string Stage04SceneName = StageSceneConfig.Stage04Name;
    private const string PlayerControllerPath = "Assets/Animations/Player.controller";
    private const float DashRechargeTriggerRadius = 1.05f;
    private const float PlayerSpriteScale = 0.66f;
    private const float UpwardDashDamping = 0.7f;
    private const float MostlyUpwardDashThreshold = 0.85f;
    private const float DeadZoneHorizontalPadding = 2f;
    private const float DeadZoneWidthMultiplier = 1.4f;
    private const float DeadZoneY = -6.5f;
    private const float DeadZoneHeight = 1f;
    private static readonly Vector2 PlayerColliderSize = new Vector2(1f, 1.5f);

    private struct StageObjectSpec
    {
        public string Name;
        public Vector2 Position;
        public Vector2 Size;

        public StageObjectSpec(string name, Vector2 position, Vector2 size)
        {
            Name = name;
            Position = position;
            Size = size;
        }
    }

    public static void ConfigureDashPlatformer()
    {
        BackupScene();
        AddTag("Dead");
        AddTag("Trap");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        DisableRunnerComponents();
        DisableLegacyPlatforms();

        GameObject player = FindOrCreate("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-6f, -1.55f, 0f);
        ConfigurePlayer(player);

        ConfigureCamera(player.transform);
        BuildStage();
        ConfigureGameManager(player.GetComponent<PlayerController>());
        ConfigureAnimator(player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Dash platformer scene setup complete.");
    }

    public static void ApplyTestObjectVisibility()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        ApplyTestVisibility("DashRecharge Pickup");
        ApplyTestVisibility("Goal");
        ApplyTestVisibility("Trap");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Test object visibility updated.");
    }

    public static void ApplyTrapTuning()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject trap = GameObject.Find("Trap");
        if (trap != null)
        {
            ConfigureTrapShape(trap);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Trap tuning updated.");
    }

    public static void CleanupPrototypeDuplicates()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        CleanupDuplicateClearUi();
        CleanupDuplicateDeadZones();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Prototype duplicates cleaned up.");
    }

    public static void ApplyDashStatusUI()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            throw new InvalidOperationException("PlayerController is missing.");
        }

        GameManager manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (manager == null)
        {
            manager = FindOrCreate("GameManager").AddComponent<GameManager>();
        }

        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        DisableScoreText(manager);
        ConfigureDashStatusUI(canvas.transform, player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Dash status UI applied.");
    }

    public static void ApplyDashFeedback()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            throw new InvalidOperationException("Player object is missing.");
        }

        ConfigureDashFeedback(player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Dash feedback applied.");
    }

    public static void ApplyAnimatorCleanup()
    {
        BackupAnimatorController();

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);
        if (controller == null)
        {
            throw new InvalidOperationException("Player AnimatorController is missing.");
        }

        ConfigureAnimatorController(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Animator cleanup applied.");
    }

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
        CreateOrUpdatePlatform(stage.transform, "Respawn Challenge Platform", new Vector2(16f, 0.7f), new Vector2(3.2f, 0.5f));
        CreateOrUpdateDashRecharge(stage.transform, "DashRecharge Pickup Respawn Challenge", new Vector2(14.4f, 1.7f));
        CreateOrUpdateTrap(stage.transform, "Trap Respawn Challenge", new Vector2(15.8f, 1.25f));
        MoveGoalToExtendedEndpoint(stage.transform, new Vector2(18.2f, 1.7f));
        ConfigureDeadZone(stage.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Stage extension applied.");
    }

    public static void CreateOrUpdateStageProgressionScenes()
    {
        AddTag("Dead");
        AddTag("Trap");

        BackupAssetFile(Stage01ScenePath);
        ConfigureStageScene(Stage01ScenePath, "STAGE 1-1", Stage02SceneName, null);
        CreateOrUpdateStage02Scene();
        CreateOrUpdateStage03Scene();
        CreateOrUpdateStage04Scene();

        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Stage progression scenes created or updated.");
    }

    public static void CreateOrUpdateStage02Scene()
    {
        CopyStageSceneFromBaseline(Stage02ScenePath);
        ConfigureStageScene(
            Stage02ScenePath,
            "STAGE 1-2",
            Stage03SceneName,
            () => ApplyStageProgressionLayout(
                new[]
                {
                    new StageObjectSpec("Stage 1-2 Start Platform", new Vector2(-6f, -2.8f), new Vector2(3.8f, 0.5f)),
                    new StageObjectSpec("Stage 1-2 Dash Approach Platform", new Vector2(-0.8f, -1.25f), new Vector2(3f, 0.5f)),
                    new StageObjectSpec("Stage 1-2 Chain Platform", new Vector2(5.4f, 0.15f), new Vector2(2.8f, 0.5f)),
                    new StageObjectSpec("Stage 1-2 Goal Platform", new Vector2(12.6f, 0.9f), new Vector2(3.2f, 0.5f)),
                },
                new[]
                {
                    new StageObjectSpec("Stage 1-2 DashRecharge Pickup A", new Vector2(2.2f, 0.15f), Vector2.zero),
                    new StageObjectSpec("Stage 1-2 DashRecharge Pickup B", new Vector2(8.7f, 1.65f), Vector2.zero),
                },
                new[]
                {
                    new StageObjectSpec("Stage 1-2 Trap A", new Vector2(-0.8f, -0.65f), Vector2.zero),
                    new StageObjectSpec("Stage 1-2 Trap B", new Vector2(5.4f, 0.75f), Vector2.zero),
                },
                new Vector2(14.1f, 1.85f)));
    }

    public static void CreateOrUpdateStage03Scene()
    {
        CopyStageSceneFromBaseline(Stage03ScenePath);
        ConfigureStageScene(
            Stage03ScenePath,
            "STAGE 1-3",
            Stage04SceneName,
            () => ApplyStageProgressionLayout(
                new[]
                {
                    new StageObjectSpec("Stage 1-3 Start Platform", new Vector2(-6f, -2.8f), new Vector2(3.5f, 0.5f)),
                    new StageObjectSpec("Stage 1-3 Precision Platform", new Vector2(-0.2f, -1.05f), new Vector2(2.6f, 0.5f)),
                    new StageObjectSpec("Stage 1-3 Midair Chain Platform", new Vector2(6.4f, 0.35f), new Vector2(2.4f, 0.5f)),
                    new StageObjectSpec("Stage 1-3 Spike Bridge Platform", new Vector2(13.8f, 1.05f), new Vector2(2.5f, 0.5f)),
                    new StageObjectSpec("Stage 1-3 Goal Platform", new Vector2(21.8f, 1.55f), new Vector2(2.9f, 0.5f)),
                },
                new[]
                {
                    new StageObjectSpec("Stage 1-3 DashRecharge Pickup A", new Vector2(2.7f, 0.35f), Vector2.zero),
                    new StageObjectSpec("Stage 1-3 DashRecharge Pickup B", new Vector2(9.9f, 1.45f), Vector2.zero),
                    new StageObjectSpec("Stage 1-3 DashRecharge Pickup C", new Vector2(17.4f, 2.15f), Vector2.zero),
                },
                new[]
                {
                    new StageObjectSpec("Stage 1-3 Trap A", new Vector2(-0.2f, -0.45f), Vector2.zero),
                    new StageObjectSpec("Stage 1-3 Trap B", new Vector2(6.4f, 0.95f), Vector2.zero),
                    new StageObjectSpec("Stage 1-3 Trap C", new Vector2(12.6f, 1.65f), Vector2.zero),
                    new StageObjectSpec("Stage 1-3 Trap D", new Vector2(15f, 1.65f), Vector2.zero),
                },
                new Vector2(23.3f, 2.55f)));
    }

    public static void CreateOrUpdateStage04Scene()
    {
        CopyStageSceneFromBaseline(Stage04ScenePath);
        ConfigureStageScene(
            Stage04ScenePath,
            "STAGE 1-4",
            "",
            () => ApplyStageProgressionLayout(
                new[]
                {
                    new StageObjectSpec("Stage 1-4 Start Platform", new Vector2(-6f, -2.8f), new Vector2(3.3f, 0.5f)),
                    new StageObjectSpec("Stage 1-4 Narrow Entry Platform", new Vector2(0.2f, -1f), new Vector2(2.3f, 0.5f)),
                    new StageObjectSpec("Stage 1-4 Vertical Dash Platform", new Vector2(7.4f, 0.25f), new Vector2(2.1f, 0.5f)),
                    new StageObjectSpec("Stage 1-4 Needle Landing Platform", new Vector2(15.6f, 1.05f), new Vector2(2f, 0.5f)),
                    new StageObjectSpec("Stage 1-4 Recharge Ledge Platform", new Vector2(24.4f, 1.65f), new Vector2(1.9f, 0.5f)),
                    new StageObjectSpec("Stage 1-4 Final Platform", new Vector2(33f, 2.15f), new Vector2(2.6f, 0.5f)),
                },
                new[]
                {
                    new StageObjectSpec("Stage 1-4 DashRecharge Pickup A", new Vector2(3.2f, 0.45f), Vector2.zero),
                    new StageObjectSpec("Stage 1-4 DashRecharge Pickup B", new Vector2(11.2f, 1.4f), Vector2.zero),
                    new StageObjectSpec("Stage 1-4 DashRecharge Pickup C", new Vector2(20.3f, 2.25f), Vector2.zero),
                    new StageObjectSpec("Stage 1-4 DashRecharge Pickup D", new Vector2(28.8f, 2.95f), Vector2.zero),
                },
                new[]
                {
                    new StageObjectSpec("Stage 1-4 Trap A", new Vector2(0.2f, -0.4f), Vector2.zero),
                    new StageObjectSpec("Stage 1-4 Trap B", new Vector2(7.4f, 0.85f), Vector2.zero),
                    new StageObjectSpec("Stage 1-4 Trap C", new Vector2(13.8f, 1.65f), Vector2.zero),
                    new StageObjectSpec("Stage 1-4 Trap D", new Vector2(17.5f, 1.65f), Vector2.zero),
                    new StageObjectSpec("Stage 1-4 Trap E", new Vector2(24.4f, 2.25f), Vector2.zero),
                    new StageObjectSpec("Stage 1-4 Trap F", new Vector2(30.5f, 2.75f), Vector2.zero),
                },
                new Vector2(34.5f, 3.15f)));
    }

    public static void RegisterStageScenesInBuildSettings()
    {
        string[] stagePaths =
        {
            Stage01ScenePath,
            Stage02ScenePath,
            Stage03ScenePath,
            Stage04ScenePath,
        };

        var scenes = stagePaths
            .Select(path => new EditorBuildSettingsScene(path, true))
            .ToList();

        foreach (EditorBuildSettingsScene existingScene in EditorBuildSettings.scenes)
        {
            if (!stagePaths.Contains(existingScene.path))
            {
                scenes.Add(existingScene);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Stage progression scenes registered in Build Settings.");
    }

    public static void ApplyPlayerSpriteScale()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            throw new InvalidOperationException("Player object is missing.");
        }

        ApplyPlayerShape(player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Player sprite scale and collider shape applied.");
    }

    public static void ApplyPlayerDashTuning()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            throw new InvalidOperationException("Player object is missing.");
        }

        var controller = EnsureComponent<PlayerController>(player);
        ApplyDashTuning(controller);
        ConfigureDashAimIndicator(player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Player dash tuning applied.");
    }

    public static void ApplyDashRechargePickupTuning()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        TuneDashRechargePickup("DashRecharge Pickup");
        TuneDashRechargePickup("DashRecharge Pickup Extended");
        TuneDashRechargePickup("DashRecharge Pickup Respawn Challenge");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Dash recharge pickup tuning applied.");
    }

    public static void ApplyDeadZoneCoverage()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject stage = GameObject.Find("Dash Platformer Stage");
        if (stage == null)
        {
            throw new InvalidOperationException("Dash Platformer Stage is missing.");
        }

        ConfigureDeadZone(stage.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] DeadZone coverage applied.");
    }

    public static void ApplyPixelStageArtMap()
    {
        BackupScene();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        ConfigurePixelStageSpriteImportSettings();
        DisableLegacyBackgroundRenderers();
        ConfigurePixelStageLayers();
        ApplyPixelArtToStageObjects();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexDashPlatformerSceneSetup] Pixel stage art map applied.");
    }

    private static void ConfigurePixelStageSpriteImportSettings()
    {
        ConfigureSpriteImport("Assets/Sprites/Sky.png", 100f);
        ConfigureSpriteImport("Assets/Sprites/SkyUpperLayer_ForestRuins.png", 100f);
        ConfigureSpriteImport("Assets/Sprites/BackgroundLayer_ForestRuins.png", 100f);
        ConfigureSpriteImport("Assets/Sprites/AbyssLayer_ForestRuins.png", 100f);
        ConfigureSpriteImport("Assets/Sprites/EnvironmentLayer_ForestRuins.png", 100f);
        ConfigureSpriteImport("Assets/Sprites/EnvironmentLowerLayer_ForestRuins.png", 100f);
        ConfigureSpriteImport("Assets/Sprites/Platform_Long.png", 64f);
        ConfigureSpriteImport("Assets/Sprites/Platform.png", 64f);
        ConfigureSpriteImport("Assets/Sprites/Obstacle.png", 80f);
        ConfigureSpriteImport("Assets/Sprites/Trap_RootSpikes.png", 80f);
    }

    private static void ConfigureSpriteImport(string assetPath, float pixelsPerUnit)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    private static void BackupScene()
    {
        string fullScenePath = Path.GetFullPath(Path.Combine(Application.dataPath, "Scene/Main.unity"));
        string backupDir = Path.GetFullPath(Path.Combine(Application.dataPath, "Scene/Backup"));
        Directory.CreateDirectory(backupDir);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        File.Copy(fullScenePath, Path.Combine(backupDir, $"Main.unity.{timestamp}.bak"), true);
    }

    private static void BackupAnimatorController()
    {
        string fullControllerPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../", PlayerControllerPath));
        string backupDir = Path.GetFullPath(Path.Combine(Application.dataPath, "Animations/Backup"));
        Directory.CreateDirectory(backupDir);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        File.Copy(fullControllerPath, Path.Combine(backupDir, $"Player.controller.{timestamp}.bak"), true);
    }

    private static void DisableRunnerComponents()
    {
        DisableAll<ScrollingObject>();
        DisableAll<BackgroundLoop>();
        DisableAll<PlatformSpawner>();
    }

    private static void DisableAll<T>() where T : Behaviour
    {
        foreach (var component in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            component.enabled = false;
        }
    }

    private static void DisableLegacyPlatforms()
    {
        GameObject stage = GameObject.Find("Dash Platformer Stage");
        foreach (var collider in UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            GameObject target = collider.gameObject;
            if (stage != null && target.transform.IsChildOf(stage.transform))
            {
                continue;
            }

            if (target.name.Contains("Platform") || target.GetComponent<Platform>() != null)
            {
                collider.enabled = false;
                var renderer = target.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }
    }

    private static void CopyStageSceneFromBaseline(string targetScenePath)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Stage01ScenePath) == null)
        {
            throw new InvalidOperationException("Stage01 scene is missing: " + Stage01ScenePath);
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScenePath) != null)
        {
            BackupAssetFile(targetScenePath);
            AssetDatabase.DeleteAsset(targetScenePath);
        }

        if (!AssetDatabase.CopyAsset(Stage01ScenePath, targetScenePath))
        {
            throw new InvalidOperationException("Failed to copy scene asset: " + targetScenePath);
        }

        AssetDatabase.ImportAsset(targetScenePath);
    }

    private static void ConfigureStageScene(string scenePath, string stageLabel, string nextSceneName, Action configureLayout)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        configureLayout?.Invoke();
        ConfigureStageLabel(stageLabel);
        ConfigureStageNextSceneName(nextSceneName);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ApplyStageProgressionLayout(
        StageObjectSpec[] platforms,
        StageObjectSpec[] dashRecharges,
        StageObjectSpec[] traps,
        Vector2 goalPosition)
    {
        GameObject stage = FindStageProgressionRoot();
        ClearStageChildren(stage.transform);

        foreach (StageObjectSpec platform in platforms)
        {
            CreateOrUpdatePlatform(stage.transform, platform.Name, platform.Position, platform.Size);
        }

        foreach (StageObjectSpec dashRecharge in dashRecharges)
        {
            CreateOrUpdateDashRecharge(stage.transform, dashRecharge.Name, dashRecharge.Position);
        }

        foreach (StageObjectSpec trap in traps)
        {
            CreateOrUpdateTrap(stage.transform, trap.Name, trap.Position);
        }

        MoveGoalToExtendedEndpoint(stage.transform, goalPosition);
        ConfigureDeadZone(stage.transform);
    }

    private static GameObject FindStageProgressionRoot()
    {
        GameObject stage = GameObject.Find("Relic Dash Forest Ruins Stage") ?? GameObject.Find("Dash Platformer Stage");
        if (stage != null)
        {
            return stage;
        }

        return new GameObject("Relic Dash Forest Ruins Stage");
    }

    private static void ClearStageChildren(Transform stage)
    {
        for (int i = stage.childCount - 1; i >= 0; i--)
        {
            GameObject child = stage.GetChild(i).gameObject;
            if (child.GetComponent<Collider2D>() != null)
            {
                UnityEngine.Object.DestroyImmediate(child);
            }
        }
    }

    private static void ConfigureStageLabel(string stageLabel)
    {
        foreach (TextMeshProUGUI label in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (label.name == "StageChip" || label.text.StartsWith("STAGE 1-", StringComparison.Ordinal))
            {
                label.text = stageLabel;
                EditorUtility.SetDirty(label);
            }
        }
    }

    private static void ConfigureStageNextSceneName(string nextSceneName)
    {
        GameManager manager = UnityEngine.Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        if (manager == null)
        {
            manager = FindOrCreate("GameManager").AddComponent<GameManager>();
        }

        manager.nextSceneName = nextSceneName;
        EditorUtility.SetDirty(manager);
    }

    private static void BackupAssetFile(string assetPath)
    {
        string fullAssetPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../", assetPath));
        if (!File.Exists(fullAssetPath))
        {
            return;
        }

        string backupRoot = Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "../Backups",
            DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_stage_progression_editor"));
        string backupPath = Path.Combine(backupRoot, assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
        File.Copy(fullAssetPath, backupPath, true);
    }

    private static GameObject FindOrCreate(string name)
    {
        GameObject found = GameObject.Find(name);
        return found != null ? found : new GameObject(name);
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static void ConfigurePlayer(GameObject player)
    {
        ApplyPlayerShape(player);

        var rigidbody = EnsureComponent<Rigidbody2D>(player);
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        rigidbody.gravityScale = Mathf.Max(1f, rigidbody.gravityScale);
        rigidbody.freezeRotation = true;

        EnsureComponent<BoxCollider2D>(player);
        EnsureComponent<AudioSource>(player);

        var controller = EnsureComponent<PlayerController>(player);
        controller.moveSpeed = 6f;
        controller.dashSpeed = 18f;
        controller.dashDuration = 0.16f;
        ApplyDashTuning(controller);

        ConfigureDashFeedback(player);
        ConfigureDashAimIndicator(player);
    }

    private static void ApplyDashTuning(PlayerController controller)
    {
        controller.upwardDashDamping = UpwardDashDamping;
        controller.mostlyUpwardDashThreshold = MostlyUpwardDashThreshold;
    }

    private static void ApplyPlayerShape(GameObject player)
    {
        player.transform.localScale = Vector3.one * PlayerSpriteScale;
        var collider = EnsureComponent<BoxCollider2D>(player);
        collider.size = PlayerColliderSize;
    }

    private static void ConfigureDashFeedback(GameObject player)
    {
        var feedback = EnsureComponent<DashFeedback>(player);
        feedback.targetRenderer = player.GetComponent<SpriteRenderer>();
        feedback.dashColor = new Color(0.35f, 0.9f, 1f, 1f);
        feedback.CaptureDefaultColor();
    }

    private static void ConfigureDashAimIndicator(GameObject player)
    {
        var indicator = EnsureComponent<DashAimIndicator>(player);
        var line = EnsureComponent<LineRenderer>(player);
        indicator.player = player.GetComponent<PlayerController>();
        indicator.lineRenderer = line;
        indicator.indicatorLength = 1.4f;
        indicator.lineWidth = 0.08f;
        indicator.showWhenDashUnavailable = true;
        indicator.readyColor = new Color(0.9529412f, 0.827451f, 0.5411765f, 0.85f);
        indicator.unavailableColor = new Color(0.9529412f, 0.827451f, 0.5411765f, 0.25f);

        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = indicator.lineWidth;
        line.endWidth = indicator.lineWidth;
        line.sortingLayerName = "Foreground";
        line.sortingOrder = 120;
        line.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        line.startColor = indicator.readyColor;
        line.endColor = new Color(indicator.readyColor.r, indicator.readyColor.g, indicator.readyColor.b, indicator.readyColor.a * 0.35f);
        line.enabled = false;
    }

    private static void ConfigureCamera(Transform target)
    {
        Camera camera = Camera.main;
        GameObject cameraObject = camera != null ? camera.gameObject : FindOrCreate("Main Camera");
        if (camera == null)
        {
            camera = EnsureComponent<Camera>(cameraObject);
            cameraObject.tag = "MainCamera";
        }

        camera.orthographic = true;
        camera.orthographicSize = 4.5f;
        var follow = EnsureComponent<CameraFollow>(cameraObject);
        follow.target = target;
        follow.offset = new Vector3(0f, 1f, -10f);
        follow.smoothTime = 0.15f;
        cameraObject.transform.position = target.position + follow.offset;
    }

    private static void BuildStage()
    {
        GameObject parent = GameObject.Find("Dash Platformer Stage");
        if (parent != null)
        {
            UnityEngine.Object.DestroyImmediate(parent);
        }

        parent = new GameObject("Dash Platformer Stage");

        CreatePlatform(parent.transform, "Start Platform", new Vector2(-6f, -2.8f), new Vector2(4f, 0.5f));
        CreatePlatform(parent.transform, "Middle Platform", new Vector2(0f, -1.4f), new Vector2(3.5f, 0.5f));
        CreatePlatform(parent.transform, "Goal Platform", new Vector2(6f, -0.4f), new Vector2(4f, 0.5f));

        CreateDashRecharge(parent.transform, new Vector2(2.4f, 0.6f));
        CreateGoal(parent.transform, new Vector2(7.4f, 0.4f));
        CreateTrap(parent.transform, new Vector2(0f, -0.75f));
        ConfigureDeadZone(parent.transform);
    }

    private static void CreatePlatform(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject platform = new GameObject(name);
        platform.transform.SetParent(parent);
        platform.transform.position = position;

        var renderer = platform.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Platform_Long.png") ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Platform.png");
        renderer.color = Color.white;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;

        var collider = platform.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private static void ConfigurePixelStageLayers()
    {
        GameObject root = FindOrCreate("Pixel Stage Art Layers");
        root.transform.position = Vector3.zero;

        Sprite sky = LoadSprite("Assets/Sprites/SkyUpperLayer_ForestRuins.png") ?? LoadSprite("Assets/Sprites/Sky.png");
        Sprite background = LoadSprite("Assets/Sprites/BackgroundLayer_ForestRuins.png");
        Sprite abyss = LoadSprite("Assets/Sprites/AbyssLayer_ForestRuins.png") ?? background;
        Sprite environment = LoadSprite("Assets/Sprites/EnvironmentLayer_ForestRuins.png");
        Sprite environmentLower = LoadSprite("Assets/Sprites/EnvironmentLowerLayer_ForestRuins.png") ?? environment;
        float layerWidth = background != null ? background.bounds.size.x : 20.48f;
        float layerHeight = background != null ? background.bounds.size.y : 10.24f;

        for (int i = -3; i <= 3; i++)
        {
            float x = 6f + (layerWidth * i);
            CreateOrUpdateArtLayer(root.transform, $"Sky Upper Layer {i + 4}", sky, new Vector3(x, layerHeight, 0f), -220);
            CreateOrUpdateArtLayer(root.transform, $"Sky Upper Layer High {i + 4}", sky, new Vector3(x, layerHeight * 2f, 0f), -221);
            CreateOrUpdateArtLayer(root.transform, $"Background Layer {i + 4}", background, new Vector3(x, 0f, 0f), -200);
            CreateOrUpdateArtLayer(root.transform, $"Abyss Layer {i + 4}", abyss, new Vector3(x, -layerHeight, 0f), -190);
            CreateOrUpdateArtLayer(root.transform, $"Abyss Layer Lower {i + 4}", abyss, new Vector3(x, -layerHeight * 2f, 0f), -191);
            CreateOrUpdateArtLayer(root.transform, $"Environment Layer {i + 4}", environment, new Vector3(x, 0f, 0f), -120);
            CreateOrUpdateArtLayer(root.transform, $"Environment Lower Layer {i + 4}", environmentLower, new Vector3(x, -layerHeight, 0f), -119);
            CreateOrUpdateArtLayer(root.transform, $"Environment Lower Layer Deep {i + 4}", environmentLower, new Vector3(x, -layerHeight * 2f, 0f), -121);
        }
    }

    private static void CreateOrUpdateArtLayer(Transform parent, string name, Sprite sprite, Vector3 position, int sortingOrder)
    {
        GameObject layer = FindChildOrCreate(parent, name);
        layer.transform.position = position;
        layer.transform.localScale = Vector3.one;

        var renderer = EnsureComponent<SpriteRenderer>(layer);
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = sortingOrder;

        foreach (Collider2D collider in layer.GetComponents<Collider2D>())
        {
            collider.enabled = false;
        }
    }

    private static void ApplyPixelArtToStageObjects()
    {
        Sprite platformSprite = LoadSprite("Assets/Sprites/Platform_Long.png") ?? LoadSprite("Assets/Sprites/Platform.png");
        Sprite trapSprite = LoadSprite("Assets/Sprites/Obstacle.png") ?? LoadSprite("Assets/Sprites/Trap_RootSpikes.png");

        GameObject stage = GameObject.Find("Dash Platformer Stage");
        if (stage != null && platformSprite != null)
        {
            foreach (SpriteRenderer renderer in stage.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (!renderer.gameObject.name.Contains("Platform"))
                {
                    continue;
                }

                renderer.sprite = platformSprite;
                renderer.color = Color.white;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.sortingLayerName = "Foreground";
                renderer.sortingOrder = 0;
            }
        }

        if (trapSprite != null)
        {
            foreach (GameObject trap in FindSceneObjectsWithTag("Trap"))
            {
                var renderer = EnsureComponent<SpriteRenderer>(trap);
                renderer.sprite = trapSprite;
                renderer.color = Color.white;
                renderer.drawMode = SpriteDrawMode.Simple;
                renderer.sortingLayerName = "Foreground";
                renderer.sortingOrder = -5;

                var collider = EnsureComponent<BoxCollider2D>(trap);
                collider.isTrigger = true;
                collider.offset = new Vector2(0f, 0.18f);
                collider.size = new Vector2(1.35f, 1.25f);
            }
        }
    }

    private static void DisableLegacyBackgroundRenderers()
    {
        foreach (BackgroundLoop backgroundLoop in UnityEngine.Object.FindObjectsByType<BackgroundLoop>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            backgroundLoop.enabled = false;
            SpriteRenderer renderer = backgroundLoop.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            Collider2D collider = backgroundLoop.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

    private static Sprite LoadSprite(string assetPath)
    {
        Sprite direct = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (direct != null)
        {
            return direct;
        }

        return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault();
    }

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
        renderer.color = Color.white;
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 0;

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
        collider.radius = DashRechargeTriggerRadius;
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

    private static void CreateDashRecharge(Transform parent, Vector2 position)
    {
        GameObject pickup = new GameObject("DashRecharge Pickup");
        pickup.transform.SetParent(parent);
        pickup.transform.position = position;

        var renderer = pickup.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Toko_Jump.png") ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstacle.png");
        ConfigureTestRenderer(renderer);
        pickup.transform.localScale = Vector3.one * 0.35f;

        var collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = DashRechargeTriggerRadius;
        pickup.AddComponent<DashRechargePickup>();
    }

    private static void TuneDashRechargePickup(string objectName)
    {
        GameObject pickup = GameObject.Find(objectName);
        if (pickup == null)
        {
            return;
        }

        var collider = EnsureComponent<CircleCollider2D>(pickup);
        collider.isTrigger = true;
        collider.radius = DashRechargeTriggerRadius;
    }

    private static void CreateGoal(Transform parent, Vector2 position)
    {
        GameObject goal = new GameObject("Goal");
        goal.transform.SetParent(parent);
        goal.transform.position = position;

        var renderer = goal.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Platform.png");
        ConfigureTestRenderer(renderer);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(0.8f, 1.8f);

        var collider = goal.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 1.8f);
        goal.AddComponent<GoalTrigger>();
    }

    private static void CreateTrap(Transform parent, Vector2 position)
    {
        GameObject trap = new GameObject("Trap");
        trap.transform.SetParent(parent);
        trap.tag = "Trap";
        trap.transform.position = position;

        var renderer = trap.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstacle.png");
        ConfigureTestRenderer(renderer);

        var collider = trap.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        ConfigureTrapShape(trap);
    }

    private static void ConfigureTrapShape(GameObject trap)
    {
        trap.transform.position = new Vector3(trap.transform.position.x, -1.25f, trap.transform.position.z);
        trap.transform.localScale = Vector3.one * 0.5f;

        var collider = trap.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.offset = new Vector2(0f, -0.5f);
            collider.size = new Vector2(1f, 1f);
        }
    }

    private static void ApplyTestVisibility(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            return;
        }

        var renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            ConfigureTestRenderer(renderer);
        }
    }

    private static void ConfigureTestRenderer(SpriteRenderer renderer)
    {
        renderer.color = Color.black;
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 100;
    }

    private static void ConfigureDeadZone(Transform parent)
    {
        GameObject deadZone = FindChildOrCreate(parent, "DeadZone");
        deadZone.tag = "Dead";

        var collider = EnsureComponent<BoxCollider2D>(deadZone);
        collider.isTrigger = true;

        Vector2[] stagePoints = GetStageCoveragePoints(parent);
        CodexDashPlatformerValidation.CalculateDeadZoneBoundsForTest(
            stagePoints,
            DeadZoneHorizontalPadding,
            DeadZoneWidthMultiplier,
            DeadZoneY,
            DeadZoneHeight,
            out Vector2 position,
            out Vector2 size);

        deadZone.transform.position = position;
        collider.size = size;
    }

    private static Vector2[] GetStageCoveragePoints(Transform stage)
    {
        return stage.GetComponentsInChildren<Transform>(true)
            .Where(t => t != stage && t.name != "DeadZone" && !t.name.Contains("Disabled Duplicate"))
            .Select(t => (Vector2)t.position)
            .ToArray();
    }

    private static void ConfigureGameManager(PlayerController player)
    {
        GameManager manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (manager == null)
        {
            manager = FindOrCreate("GameManager").AddComponent<GameManager>();
        }

        if (manager.gameoverUI != null)
        {
            manager.gameoverUI.SetActive(false);
        }

        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        DisableScoreText(manager);
        ConfigureDashStatusUI(canvas.transform, player);
        GameObject clearUI = FindSceneObject("Clear UI") ?? new GameObject("Clear UI");
        clearUI.transform.SetParent(canvas.transform, false);
        RectTransform rect = EnsureComponent<RectTransform>(clearUI);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(420f, 120f);

        TextMeshProUGUI text = clearUI.GetComponent<TextMeshProUGUI>() ?? clearUI.AddComponent<TextMeshProUGUI>();
        text.text = "CLEAR";
        text.fontSize = 64f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        clearUI.SetActive(false);
        manager.clearUI = clearUI;
    }

    private static void DisableScoreText(GameManager manager)
    {
        manager.scoreText = null;

        GameObject scoreText = FindSceneObject("Score Text");
        if (scoreText != null)
        {
            scoreText.SetActive(false);
        }
    }

    private static void ConfigureDashStatusUI(Transform canvas, PlayerController player)
    {
        GameObject dashUI = FindSceneObject("Dash Ready UI") ?? new GameObject("Dash Ready UI");
        dashUI.transform.SetParent(canvas, false);
        dashUI.SetActive(true);

        RectTransform rect = EnsureComponent<RectTransform>(dashUI);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(24f, 16f);
        rect.sizeDelta = new Vector2(206f, 32f);

        TextMeshProUGUI rootLabel = dashUI.GetComponent<TextMeshProUGUI>();
        if (rootLabel != null)
        {
            UnityEngine.Object.DestroyImmediate(rootLabel);
        }

        Image rootImage = dashUI.GetComponent<Image>();
        if (rootImage != null)
        {
            UnityEngine.Object.DestroyImmediate(rootImage);
        }

        GameObject backgroundObject = FindChildOrCreate(dashUI.transform, "Dash Ready UI Background");
        RectTransform backgroundRect = EnsureComponent<RectTransform>(backgroundObject);
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = Vector2.zero;

        Image background = EnsureComponent<Image>(backgroundObject);
        background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/HUD_Bar_Frame.png");
        background.type = Image.Type.Simple;
        background.preserveAspect = false;
        background.raycastTarget = false;
        background.color = new Color(1f, 1f, 1f, 0.94f);

        GameObject accentObject = FindChildOrCreate(dashUI.transform, "Dash Ready UI Accent");
        RectTransform accentRect = EnsureComponent<RectTransform>(accentObject);
        accentRect.anchorMin = new Vector2(0f, 0.5f);
        accentRect.anchorMax = new Vector2(0f, 0.5f);
        accentRect.pivot = new Vector2(0.5f, 0.5f);
        accentRect.anchoredPosition = new Vector2(23f, -1f);
        accentRect.sizeDelta = new Vector2(18f, 18f);

        Image accent = EnsureComponent<Image>(accentObject);
        accent.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/HUD_RuneDiamond.png");
        accent.type = Image.Type.Simple;
        accent.preserveAspect = true;
        accent.raycastTarget = false;
        accent.color = Color.white;

        GameObject labelObject = FindChildOrCreate(dashUI.transform, "Dash Ready UI Label");
        RectTransform labelRect = EnsureComponent<RectTransform>(labelObject);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(14f, -1f);
        labelRect.sizeDelta = new Vector2(-40f, 0f);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>() ?? labelObject.AddComponent<TextMeshProUGUI>();
        label.text = "DASH READY";
        label.fontSize = 16f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.margin = new Vector4(22f, 3f, 8f, 3f);

        DashStatusUI statusUI = EnsureComponent<DashStatusUI>(dashUI);
        statusUI.player = player;
        statusUI.label = label;
        statusUI.background = background;
        statusUI.accent = accent;
        statusUI.readyColor = new Color(0.92f, 1f, 0.96f, 1f);
        statusUI.emptyColor = new Color(1f, 0.82f, 0.78f, 1f);
        statusUI.readyBackgroundColor = new Color(1f, 1f, 1f, 0.94f);
        statusUI.emptyBackgroundColor = new Color(0.95f, 0.82f, 0.78f, 0.9f);
        statusUI.readyAccentColor = Color.white;
        statusUI.emptyAccentColor = new Color(1f, 0.55f, 0.45f, 1f);
        statusUI.Refresh();
    }

    private static void CleanupDuplicateClearUi()
    {
        GameManager manager = UnityEngine.Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        GameObject keep = manager != null && manager.clearUI != null ? manager.clearUI : FindSceneObject("Clear UI");
        foreach (var clearUi in FindSceneObjects("Clear UI"))
        {
            if (clearUi == keep)
            {
                continue;
            }

            clearUi.name = "Clear UI Disabled Duplicate";
            clearUi.SetActive(false);
        }
    }

    private static void CleanupDuplicateDeadZones()
    {
        GameObject keep = FindSceneObject("DeadZone");
        foreach (var deadObject in FindSceneObjectsWithTag("Dead"))
        {
            if (deadObject == keep)
            {
                continue;
            }

            deadObject.name = deadObject.name + " Disabled Duplicate";
            deadObject.SetActive(false);
        }
    }

    private static GameObject FindSceneObject(string objectName)
    {
        return FindSceneObjects(objectName).FirstOrDefault();
    }

    private static IEnumerable<GameObject> FindSceneObjects(string objectName)
    {
        return UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(target => target.scene.isLoaded && target.name == objectName);
    }

    private static IEnumerable<GameObject> FindSceneObjectsWithTag(string tag)
    {
        return UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(target => target.scene.isLoaded && target.CompareTag(tag));
    }

    private static void ConfigureAnimator(GameObject player)
    {
        Animator animator = player.GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller == null)
        {
            return;
        }

        EnsureParameter(controller, "Grounded", AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, "IsDashing", AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "VerticalSpeed", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "Die", AnimatorControllerParameterType.Trigger);
        ConfigureAnimatorController(controller);
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureAnimatorController(AnimatorController controller)
    {
        EnsureParameter(controller, "Grounded", AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, "IsDashing", AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "VerticalSpeed", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "Die", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions.ToArray())
        {
            stateMachine.RemoveAnyStateTransition(transition);
        }

        foreach (ChildAnimatorState childState in stateMachine.states.ToArray())
        {
            stateMachine.RemoveState(childState.state);
        }

        Motion runMotion = AssetDatabase.LoadAssetAtPath<Motion>("Assets/Animations/Run.anim");
        Motion jumpMotion = AssetDatabase.LoadAssetAtPath<Motion>("Assets/Animations/Jump.anim");
        Motion dieMotion = AssetDatabase.LoadAssetAtPath<Motion>("Assets/Animations/Die.anim");

        AnimatorState idle = AddState(stateMachine, "Idle", runMotion, new Vector3(260f, 80f, 0f));
        AnimatorState run = AddState(stateMachine, "Run", runMotion, new Vector3(500f, 80f, 0f));
        AnimatorState dash = AddState(stateMachine, "Dash", jumpMotion, new Vector3(500f, 200f, 0f));
        AnimatorState fall = AddState(stateMachine, "Fall", jumpMotion, new Vector3(260f, 200f, 0f));
        AnimatorState die = AddState(stateMachine, "Die", dieMotion, new Vector3(260f, 320f, 0f));
        stateMachine.defaultState = idle;

        AddAnyStateTransition(stateMachine, die, "Die", AnimatorConditionMode.If, 0f);
        AddAnyStateTransition(stateMachine, dash, "IsDashing", AnimatorConditionMode.If, 0f);

        AddTransition(idle, run, ("Grounded", AnimatorConditionMode.If, 0f), ("Speed", AnimatorConditionMode.Greater, 0.1f));
        AddTransition(run, idle, ("Grounded", AnimatorConditionMode.If, 0f), ("Speed", AnimatorConditionMode.Less, 0.1f));
        AddTransition(idle, fall, ("Grounded", AnimatorConditionMode.IfNot, 0f));
        AddTransition(run, fall, ("Grounded", AnimatorConditionMode.IfNot, 0f));
        AddTransition(fall, idle, ("Grounded", AnimatorConditionMode.If, 0f), ("Speed", AnimatorConditionMode.Less, 0.1f));
        AddTransition(fall, run, ("Grounded", AnimatorConditionMode.If, 0f), ("Speed", AnimatorConditionMode.Greater, 0.1f));
        AddTransition(dash, idle, ("IsDashing", AnimatorConditionMode.IfNot, 0f), ("Grounded", AnimatorConditionMode.If, 0f), ("Speed", AnimatorConditionMode.Less, 0.1f));
        AddTransition(dash, run, ("IsDashing", AnimatorConditionMode.IfNot, 0f), ("Grounded", AnimatorConditionMode.If, 0f), ("Speed", AnimatorConditionMode.Greater, 0.1f));
        AddTransition(dash, fall, ("IsDashing", AnimatorConditionMode.IfNot, 0f), ("Grounded", AnimatorConditionMode.IfNot, 0f));

        EditorUtility.SetDirty(controller);
    }

    private static AnimatorState AddState(AnimatorStateMachine stateMachine, string name, Motion motion, Vector3 position)
    {
        AnimatorState state = stateMachine.AddState(name, position);
        state.motion = motion;
        return state;
    }

    private static void AddAnyStateTransition(
        AnimatorStateMachine stateMachine,
        AnimatorState destination,
        string parameter,
        AnimatorConditionMode mode,
        float threshold)
    {
        AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destination);
        ConfigureTransition(transition);
        transition.AddCondition(mode, threshold, parameter);
    }

    private static void AddTransition(
        AnimatorState source,
        AnimatorState destination,
        params (string parameter, AnimatorConditionMode mode, float threshold)[] conditions)
    {
        AnimatorStateTransition transition = source.AddTransition(destination);
        ConfigureTransition(transition);
        foreach ((string parameter, AnimatorConditionMode mode, float threshold) in conditions)
        {
            transition.AddCondition(mode, threshold, parameter);
        }
    }

    private static void ConfigureTransition(AnimatorStateTransition transition)
    {
        transition.duration = 0f;
        transition.exitTime = 0f;
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
    }

    private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        foreach (var parameter in controller.parameters)
        {
            if (parameter.name == name)
            {
                return;
            }
        }

        controller.AddParameter(name, type);
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
            {
                return;
            }
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
