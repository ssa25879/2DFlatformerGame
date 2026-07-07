using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CodexDashRechargeArtRunner
{
    private const string SpritePath = "Assets/Sprites/DashRecharge_RuneSeed.png";
    private const string ScenePath = "Assets/Scene/Main.unity";

    public static void Execute()
    {
        ConfigureSprite();
        ApplyToScenePickups();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CodexDashRechargeArtRunner] Dash recharge pickup art applied.");
    }

    private static void ConfigureSprite()
    {
        var importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[CodexDashRechargeArtRunner] Missing texture importer: {SpritePath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SaveAndReimport();
    }

    private static void ApplyToScenePickups()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
        {
            Debug.LogWarning($"[CodexDashRechargeArtRunner] Sprite not loaded: {SpritePath}");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath);
        foreach (var pickup in Object.FindObjectsByType<DashRechargePickup>(FindObjectsSortMode.None))
        {
            var spriteRenderer = pickup.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = 80;
                EditorUtility.SetDirty(spriteRenderer);
            }

            var circleCollider = pickup.GetComponent<CircleCollider2D>();
            if (circleCollider != null)
            {
                circleCollider.radius = 0.78f;
                circleCollider.offset = Vector2.zero;
                EditorUtility.SetDirty(circleCollider);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
