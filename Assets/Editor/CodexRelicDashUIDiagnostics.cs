using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CodexRelicDashUIDiagnostics
{
    private const string ScenePath = "Assets/Scene/RelicDash_ForestRuins_Stage01.unity";
    private const string ReportPath = "Logs/RelicDash_UI_Diagnostics.txt";
    private const string ScreenshotFolder = "Logs/Screenshots";

    public static void Diagnose()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var builder = new StringBuilder();
        builder.AppendLine("Relic Dash UI Diagnostics");
        builder.AppendLine("Scene: " + scene.path);

        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            builder.AppendLine();
            builder.AppendLine($"Canvas {PathOf(canvas.transform)} renderMode={canvas.renderMode}");
            foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
            {
                RectTransform rect = image.rectTransform;
                builder.AppendLine(
                    $"IMAGE {PathOf(image.transform)} active={image.gameObject.activeInHierarchy} " +
                    $"sprite={(image.sprite != null ? image.sprite.name : "<none>")} type={image.type} preserve={image.preserveAspect} " +
                    $"anchorMin={rect.anchorMin} anchorMax={rect.anchorMax} pivot={rect.pivot} pos={rect.anchoredPosition} size={rect.sizeDelta} " +
                    $"offsetMin={rect.offsetMin} offsetMax={rect.offsetMax} fill={image.fillAmount}");
            }

            foreach (TextMeshProUGUI text in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                RectTransform rect = text.rectTransform;
                builder.AppendLine(
                    $"TEXT {PathOf(text.transform)} active={text.gameObject.activeInHierarchy} text='{text.text}' " +
                    $"fontSize={text.fontSize} color={text.color} align={text.alignment} wrap={text.textWrappingMode} overflow={text.overflowMode} " +
                    $"anchorMin={rect.anchorMin} anchorMax={rect.anchorMax} pivot={rect.pivot} pos={rect.anchoredPosition} size={rect.sizeDelta}");
            }
        }

        System.IO.Directory.CreateDirectory("Logs");
        System.IO.File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        Debug.Log("[CodexRelicDashUIDiagnostics] Wrote " + ReportPath);
    }

    public static void CaptureHudStates()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            throw new System.InvalidOperationException("HUDCanvas is missing.");
        }

        GameObject gameOver = FindSceneObject("GameOverUI");
        GameObject clear = FindSceneObject("ClearUI");
        bool gameOverActive = gameOver != null && gameOver.activeSelf;
        bool clearActive = clear != null && clear.activeSelf;

        System.IO.Directory.CreateDirectory(ScreenshotFolder);
        try
        {
            if (gameOver != null)
            {
                gameOver.SetActive(false);
            }

            if (clear != null)
            {
                clear.SetActive(false);
            }

            SaveCanvasPng(canvas, System.IO.Path.Combine(ScreenshotFolder, "RelicDash_HUD_Normal.png"));

            if (gameOver != null)
            {
                gameOver.SetActive(true);
                SaveCanvasPng(canvas, System.IO.Path.Combine(ScreenshotFolder, "RelicDash_HUD_GameOver.png"));
                gameOver.SetActive(false);
            }

            if (clear != null)
            {
                clear.SetActive(true);
                SaveCanvasPng(canvas, System.IO.Path.Combine(ScreenshotFolder, "RelicDash_HUD_Clear.png"));
                clear.SetActive(false);
            }
        }
        finally
        {
            if (gameOver != null)
            {
                gameOver.SetActive(gameOverActive);
            }

            if (clear != null)
            {
                clear.SetActive(clearActive);
            }
        }

        Debug.Log("[CodexRelicDashUIDiagnostics] Captured HUD state screenshots.");
    }

    public static void ApplyHudImageFixes()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        SetPreserveAspect("RuneIcon", true);
        SetPreserveAspect("DashRuneSeed", true);

        SetRect("ScorePlaque", new Vector2(28f, -18f), new Vector2(390f, 92f));
        SetRect("RuneIcon", new Vector2(-148f, 0f), new Vector2(52f, 52f));
        SetRect("ScoreLabel", new Vector2(-44f, 18f), new Vector2(150f, 30f));
        SetRect("ScoreValue", new Vector2(98f, -12f), new Vector2(198f, 42f));
        SetText("ScoreValue", "000000", 34f, TextAlignmentOptions.Right);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexRelicDashUIDiagnostics] Applied HUD image/aspect fixes.");
    }

    private static string PathOf(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject target in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (target.name == objectName)
            {
                return target;
            }
        }

        return null;
    }

    private static void SetPreserveAspect(string objectName, bool preserveAspect)
    {
        Image image = FindSceneObject(objectName).GetComponent<Image>();
        image.preserveAspect = preserveAspect;
    }

    private static void SetRect(string objectName, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = FindSceneObject(objectName).GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void SetText(string objectName, string textValue, float fontSize, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI text = FindSceneObject(objectName).GetComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.alignment = alignment;
    }

    private static void SaveCanvasPng(Canvas canvas, string path)
    {
        RenderMode originalRenderMode = canvas.renderMode;
        Camera originalCamera = canvas.worldCamera;
        float originalPlaneDistance = canvas.planeDistance;

        GameObject cameraObject = new GameObject("Codex Relic Dash UI Screenshot Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        Texture2D texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);

        try
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            camera.targetTexture = renderTexture;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;

            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }
        finally
        {
            canvas.renderMode = originalRenderMode;
            canvas.worldCamera = originalCamera;
            canvas.planeDistance = originalPlaneDistance;
            camera.targetTexture = null;
            renderTexture.Release();
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(cameraObject);
        }
    }
}
