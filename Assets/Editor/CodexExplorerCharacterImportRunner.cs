using UnityEditor;
using UnityEngine;

public static class CodexExplorerCharacterImportRunner
{
    private const int PixelsPerUnit = 32;

    public static void Execute()
    {
        AssetDatabase.Refresh();

        ConfigureSheet("Assets/Sprites/Explorer/Explorer_Idle.png", 64, 64, 4, "Explorer_Idle");
        ConfigureSheet("Assets/Sprites/Explorer/Explorer_Run.png", 64, 64, 8, "Explorer_Run");
        ConfigureSheet("Assets/Sprites/Explorer/Explorer_Dash.png", 64, 64, 4, "Explorer_Dash");
        ConfigureSheet("Assets/Sprites/Explorer/Explorer_Die.png", 64, 64, 8, "Explorer_Die");
        ConfigureSingle("Assets/Sprites/Explorer/Explorer_CharacterPreview.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CodexExplorerCharacterImportRunner] Explorer character sprites imported.");
    }

    private static void ConfigureSheet(string path, int frameWidth, int frameHeight, int frameCount, string spriteName)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[CodexExplorerCharacterImportRunner] Missing texture importer: {path}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;

        var sprites = new SpriteMetaData[frameCount];
        for (var i = 0; i < frameCount; i++)
        {
            sprites[i] = new SpriteMetaData
            {
                name = $"{spriteName}_{i}",
                rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
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
