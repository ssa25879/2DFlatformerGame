using UnityEditor;
using UnityEngine;

public static class CodexEnvironmentTreeImportRunner
{
    private const int PixelsPerUnit = 32;

    public static void Execute()
    {
        AssetDatabase.Refresh();

        ConfigureSheet("Assets/Sprites/EnvironmentTrees/EnvironmentTree_Round_Idle.png", 160, 192, "EnvironmentTree_Round_Idle");
        ConfigureSheet("Assets/Sprites/EnvironmentTrees/EnvironmentTree_Tall_Idle.png", 128, 224, "EnvironmentTree_Tall_Idle");
        ConfigureSheet("Assets/Sprites/EnvironmentTrees/EnvironmentTree_Vine_Idle.png", 160, 224, "EnvironmentTree_Vine_Idle");
        ConfigureSingle("Assets/Sprites/EnvironmentTreePlacementPreview.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CodexEnvironmentTreeImportRunner] Environment tree sprites imported.");
    }

    private static void ConfigureSheet(string path, int frameWidth, int frameHeight, string spriteName)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[CodexEnvironmentTreeImportRunner] Missing texture importer: {path}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;

        var sprites = new SpriteMetaData[4];
        for (var i = 0; i < sprites.Length; i++)
        {
            sprites[i] = new SpriteMetaData
            {
                name = $"{spriteName}_{i}",
                rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight),
                alignment = (int)SpriteAlignment.BottomCenter,
                pivot = new Vector2(0.5f, 0f)
            };
        }

#pragma warning disable CS0618
        importer.spritesheet = sprites;
#pragma warning restore CS0618
        importer.SaveAndReimport();
    }

    private static void ConfigureSingle(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }
}
