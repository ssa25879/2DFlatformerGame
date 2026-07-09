using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CodexRelicDashGameSceneBuilder
{
    private const string ScenePath = StageSceneConfig.Stage01Path;
    private const string UiRoot = "Assets/Sprites/UI";
    private const string GameplayRoot = "Assets/Sprites/Gameplay";
    private const string PlayerControllerPath = "Assets/Animations/Player.controller";

    private static readonly Color TextCream = Hex("eef2ec");
    private static readonly Color TextSub = Hex("8fb7ac");
    private static readonly Color Gold = Hex("e8b24a");
    private static readonly Color GoldLight = Hex("f3d38a");
    private static readonly Color Danger = Hex("c15c4a");
    private static readonly Color Dim = new Color(0.031f, 0.063f, 0.059f, 0.82f);

    [MenuItem("Tools/Codex/Build Relic Dash Forest Ruins Stage 01")]
    public static void Build()
    {
        ConfigureImportedSprites();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = StageSceneConfig.Stage01Name;

        GameObject player = BuildGameplayScene();
        BuildHudAndScreens(player);
        EnsureEventSystem();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CodexRelicDashGameSceneBuilder] Built " + ScenePath);
    }

    private static void ConfigureImportedSprites()
    {
        AssetDatabase.Refresh();
        ConfigureSpriteFolder(UiRoot, 32f);
        ConfigureSpriteFolder(GameplayRoot, 32f);
        AssetDatabase.Refresh();
    }

    private static void ConfigureSpriteFolder(string folder, float ppu)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spriteBorder = BorderFor(path);
            importer.SaveAndReimport();
        }
    }

    private static Vector4 BorderFor(string path)
    {
        string file = System.IO.Path.GetFileNameWithoutExtension(path);
        switch (file)
        {
            case "HUD_Bar_Frame":
                return new Vector4(12, 8, 12, 14);
            case "HUD_ScorePlaque":
                return new Vector4(60, 10, 12, 10);
            case "HUD_StagePanel":
                return new Vector4(12, 10, 12, 10);
            case "HUD_ProgressBar_Track":
                return new Vector4(6, 6, 6, 6);
            case "HUD_DashGauge_Track":
            case "HUD_DashGauge_Empty":
                return new Vector4(8, 6, 8, 6);
            case "HUD_Divider":
                return new Vector4(0, 8, 0, 8);
            case "Panel_Result_Frame":
                return new Vector4(14, 14, 14, 20);
            case "Button_Gold":
            case "Button_Gold_Normal":
            case "Button_Gold_Pressed":
            case "Button_Gold_Disabled":
            case "Button_Stone":
            case "Button_Stone_Normal":
            case "Button_Stone_Pressed":
                return new Vector4(12, 14, 12, 12);
            case "StatRow_Frame":
                return new Vector4(10, 10, 10, 8);
            case "Title_Band_Gold":
            case "Title_Band_Red":
                return new Vector4(24, 4, 24, 4);
            case "Rule_Horizontal":
                return new Vector4(8, 0, 8, 0);
            case "Panel_Menu_Frame":
                return new Vector4(14, 14, 14, 20);
            case "Chip_Best":
                return new Vector4(10, 8, 10, 8);
            case "LoadingBar_Track":
                return new Vector4(8, 6, 8, 6);
            default:
                return Vector4.zero;
        }
    }

    private static GameObject BuildGameplayScene()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.4f;
        camera.backgroundColor = Hex("1c2a28");
        cameraObject.transform.position = new Vector3(-5.4f, 1.0f, -10f);

        var stage = new GameObject("Relic Dash Forest Ruins Stage");
        CreateSprite(stage.transform, "Sky Loop A", SpriteAt(GameplayRoot + "/Sky.png"), new Vector3(-19.2f, 1.8f, 8f), 19.2f, 11f, 0);
        CreateSprite(stage.transform, "Sky Loop B", SpriteAt(GameplayRoot + "/Sky.png"), new Vector3(0f, 1.8f, 8f), 19.2f, 11f, 0);
        CreateSprite(stage.transform, "Sky Loop C", SpriteAt(GameplayRoot + "/Sky.png"), new Vector3(19.2f, 1.8f, 8f), 19.2f, 11f, 0);
        CreateSprite(stage.transform, "Distant Ruins A", SpriteAt(GameplayRoot + "/BackgroundLayer_ForestRuins.png"), new Vector3(-9.6f, 0.6f, 6f), 19.2f, 7.2f, 1);
        CreateSprite(stage.transform, "Distant Ruins B", SpriteAt(GameplayRoot + "/BackgroundLayer_ForestRuins.png"), new Vector3(9.6f, 0.6f, 6f), 19.2f, 7.2f, 1);
        CreateSprite(stage.transform, "Environment Ruins A", SpriteAt(GameplayRoot + "/EnvironmentLayer_ForestRuins.png"), new Vector3(-9.6f, -0.1f, 4f), 19.2f, 7.2f, 2);
        CreateSprite(stage.transform, "Environment Ruins B", SpriteAt(GameplayRoot + "/EnvironmentLayer_ForestRuins.png"), new Vector3(9.6f, -0.1f, 4f), 19.2f, 7.2f, 2);

        CreatePlatform(stage.transform, "Tutorial Start Platform", new Vector2(-6.4f, -2.15f), new Vector2(5.8f, 0.56f));
        CreatePlatform(stage.transform, "Tutorial Jump Platform", new Vector2(-1.1f, -1.55f), new Vector2(3.6f, 0.56f));
        CreatePlatform(stage.transform, "Tutorial Dash Platform", new Vector2(3.4f, -0.65f), new Vector2(3.4f, 0.56f));
        CreatePlatform(stage.transform, "Tutorial Safe Landing", new Vector2(7.9f, -1.15f), new Vector2(4.4f, 0.56f));
        CreatePlatform(stage.transform, "Tutorial Goal Platform", new Vector2(12.3f, -0.25f), new Vector2(3.6f, 0.56f));
        CreateTrap(stage.transform, "First Root Spike Trap", new Vector2(7.9f, -0.68f));
        CreateDashRecharge(stage.transform, new Vector2(1.8f, 0.35f));
        CreateGoal(stage.transform, new Vector2(13.6f, 0.8f));
        CreateDeadZone(stage.transform);
        CreateTutorialHint(stage.transform, "Move Hint", "MOVE", new Vector2(-6.7f, -0.75f));
        CreateTutorialHint(stage.transform, "Jump Hint", "JUMP", new Vector2(-1.1f, -0.2f));
        CreateTutorialHint(stage.transform, "Dash Hint", "DASH TO RUNE", new Vector2(2.0f, 1.15f));
        CreateTutorialHint(stage.transform, "Trap Hint", "AVOID SPIKES", new Vector2(7.9f, 0.1f));
        CreateTutorialHint(stage.transform, "Goal Hint", "REACH THE RELIC", new Vector2(12.3f, 1.25f));

        GameObject player = CreatePlayer();
        cameraObject.AddComponent<CameraFollow>().target = player.transform;
        return player;
    }

    private static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-7.5f, -1.28f, 0f);
        player.transform.localScale = Vector3.one * 0.58f;

        var renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = FirstSprite("Assets/Sprites/Explorer/Explorer_Idle.png");
        renderer.sortingOrder = 20;

        var rigidbody = player.AddComponent<Rigidbody2D>();
        rigidbody.freezeRotation = true;
        rigidbody.gravityScale = 3.6f;

        var collider = player.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(0f, -0.04f);
        collider.size = new Vector2(0.76f, 1.34f);

        var animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerControllerPath);

        player.AddComponent<AudioSource>();
        player.AddComponent<PlayerController>();
        player.AddComponent<DashFeedback>();
        player.AddComponent<DashAimIndicator>();
        player.AddComponent<LineRenderer>();
        player.AddComponent<TrailRenderer>();
        return player;
    }

    private static void CreatePlatform(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject platform = CreateSprite(parent, name, SpriteAt(GameplayRoot + "/Platform_Long.png"), new Vector3(position.x, position.y, 0f), size.x, size.y, 10);
        platform.AddComponent<BoxCollider2D>().size = new Vector2(size.x, size.y * 0.7f);
        platform.AddComponent<Platform>();
    }

    private static void CreateTrap(Transform parent, string name, Vector2 position)
    {
        GameObject trap = CreateSprite(parent, name, SpriteAt("Assets/Sprites/Trap_RootSpikes.png"), new Vector3(position.x, position.y, -0.05f), 1.7f, 1.0f, 12);
        trap.tag = "Trap";
        var collider = trap.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.offset = new Vector2(0f, 0.23f);
        collider.size = new Vector2(1.25f, 0.42f);
    }

    private static void CreateDashRecharge(Transform parent, Vector2 position)
    {
        GameObject pickup = CreateSprite(parent, "Dash Recharge Rune Seed", SpriteAt(GameplayRoot + "/DashRecharge_RuneSeed.png"), new Vector3(position.x, position.y, 0f), 1.1f, 1.1f, 18);
        var collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.55f;
        pickup.AddComponent<DashRechargePickup>();
    }

    private static void CreateGoal(Transform parent, Vector2 position)
    {
        var goal = new GameObject("Goal");
        goal.transform.SetParent(parent);
        goal.transform.position = new Vector3(position.x, position.y, 0f);
        var collider = goal.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 3.2f);
        goal.AddComponent<GoalTrigger>();
    }

    private static void CreateDeadZone(Transform parent)
    {
        var deadZone = new GameObject("Abyss Dead Zone");
        deadZone.tag = "Dead";
        deadZone.transform.SetParent(parent);
        deadZone.transform.position = new Vector3(2.8f, -6.1f, 0f);
        var collider = deadZone.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(34f, 1f);
    }

    private static void CreateTutorialHint(Transform parent, string name, string text, Vector2 position)
    {
        var hint = new GameObject(name);
        hint.transform.SetParent(parent);
        hint.transform.position = new Vector3(position.x, position.y, -0.1f);

        var label = hint.AddComponent<TextMeshPro>();
        label.text = text;
        label.fontSize = 2.8f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = GoldLight;
        label.enableWordWrapping = false;
        label.sortingOrder = 30;

        var rect = hint.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(4.5f, 1f);
        }

        hint.transform.localScale = Vector3.one * 0.18f;
    }

    private static GameObject CreateSprite(Transform parent, string name, Sprite sprite, Vector3 position, float width, float height, int sortingOrder)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;

        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;

        if (sprite != null)
        {
            Vector2 spriteSize = sprite.bounds.size;
            gameObject.transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);
        }

        return gameObject;
    }

    private static void BuildHudAndScreens(GameObject player)
    {
        Canvas canvas = CreateCanvas();
        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(canvas.transform, false);
        AddStretchRect(hud, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        BuildHud(hud.transform, player);
        GameObject gameOver = BuildResultScreen(canvas.transform, "GameOverUI", "GAME OVER", Danger, "RETRY", false);
        GameObject clear = BuildResultScreen(canvas.transform, "ClearUI", "STAGE CLEAR", GoldLight, "NEXT STAGE", true);
        gameOver.SetActive(false);
        clear.SetActive(false);

        var manager = new GameObject("GameManager").AddComponent<GameManager>();
        manager.scoreText = GameObject.Find("ScoreValue").GetComponent<TextMeshProUGUI>();
        manager.gameoverUI = gameOver;
        manager.clearUI = clear;
    }

    private static Canvas CreateCanvas()
    {
        var canvasObject = new GameObject("HUDCanvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void BuildHud(Transform parent, GameObject player)
    {
        Image bar = CreateImage(parent, "BarFrame", SpriteAt(UiRoot + "/HUD/HUD_Bar_Frame.png"), Image.Type.Sliced, TextCream);
        StretchTop(bar.rectTransform, 0f, 0f, 120f);

        Image score = CreateImage(parent, "ScorePlaque", SpriteAt(UiRoot + "/HUD/HUD_ScorePlaque.png"), Image.Type.Sliced, Color.white);
        SetTopLeft(score.rectTransform, new Vector2(28f, -18f), new Vector2(390f, 92f));
        Image scoreIcon = CreateImage(score.transform, "RuneIcon", SpriteAt(UiRoot + "/HUD/HUD_RuneDiamond.png"), Image.Type.Simple, Color.white);
        scoreIcon.preserveAspect = true;
        scoreIcon.rectTransform.sizeDelta = new Vector2(52f, 52f);
        scoreIcon.rectTransform.anchoredPosition = new Vector2(-148f, 0f);
        AddText(score.transform, "ScoreLabel", "SCORE", 22, TextSub, TextAlignmentOptions.Left, new Vector2(-44f, 18f), new Vector2(150f, 30f));
        AddText(score.transform, "ScoreValue", "000000", 34, GoldLight, TextAlignmentOptions.Right, new Vector2(98f, -12f), new Vector2(198f, 42f));

        Image stage = CreateImage(parent, "StagePanel", SpriteAt(UiRoot + "/HUD/HUD_StagePanel.png"), Image.Type.Sliced, Color.white);
        SetTopCenter(stage.rectTransform, new Vector2(0f, -18f), new Vector2(620f, 92f));
        AddText(stage.transform, "StageName", "FOREST RUINS", 28, TextCream, TextAlignmentOptions.Center, new Vector2(0f, 17f), new Vector2(360f, 34f));
        AddText(stage.transform, "StageChip", "STAGE 1-1", 18, Gold, TextAlignmentOptions.Center, new Vector2(215f, 17f), new Vector2(150f, 30f));
        Image progressTrack = CreateImage(stage.transform, "ProgressTrack", SpriteAt(UiRoot + "/HUD/HUD_ProgressBar_Track.png"), Image.Type.Sliced, Color.white);
        progressTrack.rectTransform.sizeDelta = new Vector2(560f, 20f);
        progressTrack.rectTransform.anchoredPosition = new Vector2(0f, -24f);
        Image progressFill = CreateImage(progressTrack.transform, "ProgressFill", SpriteAt(UiRoot + "/HUD/HUD_ProgressBar_Fill.png"), Image.Type.Filled, Color.white);
        FillInside(progressFill.rectTransform);
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        progressFill.fillAmount = 0.35f;

        Image dash = CreateImage(parent, "DashGauge", null, Image.Type.Simple, Color.clear);
        SetTopRight(dash.rectTransform, new Vector2(-28f, -18f), new Vector2(360f, 92f));
        AddText(dash.transform, "DashLabel", "DASH", 22, TextSub, TextAlignmentOptions.Left, new Vector2(-106f, 17f), new Vector2(130f, 30f));
        Image dashTrack = CreateImage(dash.transform, "GaugeTrack", SpriteAt(UiRoot + "/HUD/HUD_DashGauge_Track.png"), Image.Type.Sliced, Color.white);
        dashTrack.rectTransform.sizeDelta = new Vector2(280f, 30f);
        dashTrack.rectTransform.anchoredPosition = new Vector2(-22f, -18f);
        Image dashFill = CreateImage(dashTrack.transform, "GaugeFill", SpriteAt(UiRoot + "/HUD/HUD_DashGauge_Fill.png"), Image.Type.Filled, Color.white);
        FillInside(dashFill.rectTransform);
        dashFill.fillMethod = Image.FillMethod.Horizontal;
        dashFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        dashFill.fillAmount = 1f;
        Image rune = CreateImage(dash.transform, "DashRuneSeed", SpriteAt("Assets/Sprites/DashRecharge_RuneSeed.png"), Image.Type.Simple, Color.white);
        rune.preserveAspect = true;
        rune.rectTransform.sizeDelta = new Vector2(56f, 56f);
        rune.rectTransform.anchoredPosition = new Vector2(148f, -2f);

        var dashStatus = dash.gameObject.AddComponent<DashStatusUI>();
        dashStatus.player = player.GetComponent<PlayerController>();
        dashStatus.label = GameObject.Find("DashLabel").GetComponent<TextMeshProUGUI>();
        dashStatus.background = dashTrack;
        dashStatus.readyText = "DASH READY";
        dashStatus.emptyText = "DASH EMPTY";
        dashStatus.readyColor = GoldLight;
        dashStatus.emptyColor = Danger;
        dashStatus.readyBackgroundColor = new Color(1f, 1f, 1f, 1f);
        dashStatus.emptyBackgroundColor = new Color(0.76f, 0.36f, 0.29f, 1f);
    }

    private static GameObject BuildResultScreen(Transform parent, string name, string title, Color titleColor, string buttonText, bool clear)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        AddStretchRect(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Image dim = CreateImage(root.transform, "DimOverlay", SpriteAt(UiRoot + "/Result/Overlay_Vignette.png"), Image.Type.Simple, Dim);
        AddStretchRect(dim.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Image panel = CreateImage(root.transform, "Panel", SpriteAt(UiRoot + "/Result/Panel_Result_Frame.png"), Image.Type.Sliced, Color.white);
        SetCenter(panel.rectTransform, Vector2.zero, new Vector2(700f, 480f));

        Image band = CreateImage(panel.transform, "TitleBand", SpriteAt(UiRoot + (clear ? "/Result/Title_Band_Gold.png" : "/Result/Title_Band_Red.png")), Image.Type.Sliced, Color.white);
        band.rectTransform.sizeDelta = new Vector2(620f, 76f);
        band.rectTransform.anchoredPosition = new Vector2(0f, 152f);
        AddText(band.transform, "TitleText", title, 44, titleColor, TextAlignmentOptions.Center, Vector2.zero, new Vector2(560f, 62f));

        AddStatRow(panel.transform, "ScoreRow", "SCORE", "000000", new Vector2(0f, 54f));
        AddStatRow(panel.transform, "BestRow", clear ? "TIME" : "BEST", clear ? "01:24" : "000000", new Vector2(0f, -20f));

        Image button = CreateImage(panel.transform, "PrimaryButton", SpriteAt(UiRoot + "/Result/Button_Gold.png"), Image.Type.Sliced, Color.white);
        button.rectTransform.sizeDelta = new Vector2(600f, 72f);
        button.rectTransform.anchoredPosition = new Vector2(0f, -118f);
        button.gameObject.AddComponent<Button>();
        AddText(button.transform, "ButtonText", buttonText, 30, Hex("233230"), TextAlignmentOptions.Center, Vector2.zero, new Vector2(560f, 54f));
        AddText(panel.transform, "HintText", "SPACE / CLICK TO RESTART", 18, TextSub, TextAlignmentOptions.Center, new Vector2(0f, -190f), new Vector2(560f, 30f));

        return root;
    }

    private static void AddStatRow(Transform parent, string name, string label, string value, Vector2 position)
    {
        Image row = CreateImage(parent, name, SpriteAt(UiRoot + "/Result/StatRow_Frame.png"), Image.Type.Sliced, Color.white);
        row.rectTransform.sizeDelta = new Vector2(600f, 66f);
        row.rectTransform.anchoredPosition = position;
        AddText(row.transform, "Label", label, 22, TextSub, TextAlignmentOptions.Left, new Vector2(-185f, 0f), new Vector2(160f, 40f));
        AddText(row.transform, "Value", value, 30, GoldLight, TextAlignmentOptions.Right, new Vector2(145f, 0f), new Vector2(260f, 44f));
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Image.Type type, Color color)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        var rect = gameObject.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100f, 100f);
        var image = gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.color = color;
        return image;
    }

    private static TextMeshProUGUI AddText(Transform parent, string name, string text, int size, Color color, TextAlignmentOptions alignment, Vector2 position, Vector2 rectSize)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        var rect = gameObject.AddComponent<RectTransform>();
        rect.sizeDelta = rectSize;
        rect.anchoredPosition = position;
        var label = gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
        return label;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static void StretchTop(RectTransform rect, float left, float right, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(left, -height);
        rect.offsetMax = new Vector2(-right, 0f);
    }

    private static void SetTopLeft(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetTopCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetTopRight(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void FillInside(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void AddStretchRect(GameObject gameObject, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rect = gameObject.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = gameObject.AddComponent<RectTransform>();
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static Sprite SpriteAt(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite FirstSprite(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        return null;
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}
