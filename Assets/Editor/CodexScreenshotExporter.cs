using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CodexScreenshotExporter
{
    public static void CaptureDashReadyUI()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            throw new InvalidOperationException("Canvas is missing.");
        }

        Directory.CreateDirectory("Logs/Screenshots");
        string outputPath = Path.GetFullPath(Path.Combine(
            "Logs/Screenshots",
            $"DashReadyUI_{DateTime.Now:yyyyMMdd_HHmmss}.png"));

        RenderCanvasToPng(canvas, outputPath, 640, 360);
        Debug.Log($"[CodexScreenshotExporter] Saved UI screenshot: {outputPath}");
    }

    public static void CaptureDashReadyUIOverGame()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        Camera mainCamera = Camera.main;
        if (canvas == null)
        {
            throw new InvalidOperationException("Canvas is missing.");
        }

        if (mainCamera == null)
        {
            throw new InvalidOperationException("Main Camera is missing.");
        }

        Directory.CreateDirectory("Logs/Screenshots");
        string outputPath = Path.GetFullPath(Path.Combine(
            "Logs/Screenshots",
            $"DashReadyUI_Game_{DateTime.Now:yyyyMMdd_HHmmss}.png"));

        RenderGameAndCanvasToPng(mainCamera, canvas, outputPath, 960, 540);
        Debug.Log($"[CodexScreenshotExporter] Saved game UI screenshot: {outputPath}");
    }

    private static void RenderCanvasToPng(Canvas canvas, string outputPath, int width, int height)
    {
        RenderMode originalRenderMode = canvas.renderMode;
        Camera originalCamera = canvas.worldCamera;
        float originalPlaneDistance = canvas.planeDistance;

        GameObject cameraObject = new GameObject("Codex UI Screenshot Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

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
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;

            File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }
        finally
        {
            canvas.renderMode = originalRenderMode;
            canvas.worldCamera = originalCamera;
            canvas.planeDistance = originalPlaneDistance;
            camera.targetTexture = null;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }

    private static void RenderGameAndCanvasToPng(Camera mainCamera, Canvas canvas, string outputPath, int width, int height)
    {
        RenderTexture originalMainTarget = mainCamera.targetTexture;
        RenderMode originalRenderMode = canvas.renderMode;
        Camera originalCanvasCamera = canvas.worldCamera;
        float originalPlaneDistance = canvas.planeDistance;

        GameObject uiCameraObject = new GameObject("Codex Game UI Screenshot Camera");
        Camera uiCamera = uiCameraObject.AddComponent<Camera>();
        RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        try
        {
            mainCamera.targetTexture = renderTexture;
            mainCamera.Render();

            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.backgroundColor = Color.clear;
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 5f;
            uiCamera.nearClipPlane = 0.01f;
            uiCamera.farClipPlane = 100f;
            uiCamera.targetTexture = renderTexture;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();
            uiCamera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;

            File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }
        finally
        {
            mainCamera.targetTexture = originalMainTarget;
            canvas.renderMode = originalRenderMode;
            canvas.worldCamera = originalCanvasCamera;
            canvas.planeDistance = originalPlaneDistance;
            uiCamera.targetTexture = null;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(uiCameraObject);
        }
    }
}
