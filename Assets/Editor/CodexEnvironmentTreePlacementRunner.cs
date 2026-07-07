using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CodexEnvironmentTreePlacementRunner
{
    private const string ScenePath = "Assets/Scene/Main.unity";

    public static void Execute()
    {
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(ScenePath);
        GameObject root = FindOrCreate("Pixel Stage Art Layers");
        GameObject treeRoot = FindChildOrCreate(root.transform, "Environment Tree Objects");
        treeRoot.transform.localPosition = Vector3.zero;

        ClearChildren(treeRoot.transform);

        CreateTree(treeRoot.transform, "EnvTree_Tall_Left", "Assets/Sprites/EnvironmentTrees/EnvironmentTree_Tall_Idle.png", new Vector3(-8.9f, -3.35f, 0f), 0.9f, -108, 0.62f, 0f);
        CreateTree(treeRoot.transform, "EnvTree_Round_LeftPlatform", "Assets/Sprites/EnvironmentTrees/EnvironmentTree_Round_Idle.png", new Vector3(-4.7f, -3.28f, 0f), 0.72f, -104, 0.66f, 0.19f);
        CreateTree(treeRoot.transform, "EnvTree_Vine_UpperGap", "Assets/Sprites/EnvironmentTrees/EnvironmentTree_Vine_Idle.png", new Vector3(-1.9f, -1.94f, 0f), 0.7f, -106, 0.7f, 0.37f);
        CreateTree(treeRoot.transform, "EnvTree_Round_MidPlatform", "Assets/Sprites/EnvironmentTrees/EnvironmentTree_Round_Idle.png", new Vector3(3.45f, -1.88f, 0f), 0.62f, -102, 0.64f, 0.53f);
        CreateTree(treeRoot.transform, "EnvTree_Vine_RightLower", "Assets/Sprites/EnvironmentTrees/EnvironmentTree_Vine_Idle.png", new Vector3(6.9f, -3.82f, 0f), 0.72f, -105, 0.68f, 0.71f);
        CreateTree(treeRoot.transform, "EnvTree_Tall_Right", "Assets/Sprites/EnvironmentTrees/EnvironmentTree_Tall_Idle.png", new Vector3(10.1f, -3.35f, 0f), 0.82f, -108, 0.62f, 0.89f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[CodexEnvironmentTreePlacementRunner] Environment trees placed.");
    }

    private static void CreateTree(Transform parent, string name, string spritePath, Vector3 position, float scale, int sortingOrder, float alpha, float idleOffset)
    {
        Sprite[] frames = LoadSpriteFrames(spritePath);
        Sprite sprite = frames.Length > 0 ? frames[0] : null;
        if (sprite == null)
        {
            Debug.LogWarning($"[CodexEnvironmentTreePlacementRunner] Missing sprite: {spritePath}");
            return;
        }

        GameObject tree = new GameObject(name);
        tree.transform.SetParent(parent, false);
        tree.transform.localPosition = position;
        tree.transform.localScale = Vector3.one * scale;

        var renderer = tree.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0.72f, 0.9f, 0.86f, alpha);
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = sortingOrder;

        var idle = tree.AddComponent<EnvironmentTreeIdleAnimator>();
        SerializedObject serializedIdle = new SerializedObject(idle);
        serializedIdle.FindProperty("targetRenderer").objectReferenceValue = renderer;
        SerializedProperty framesProperty = serializedIdle.FindProperty("frames");
        framesProperty.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++)
        {
            framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        }

        serializedIdle.FindProperty("framesPerSecond").floatValue = 1f;
        serializedIdle.FindProperty("startOffset").floatValue = idleOffset;
        serializedIdle.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Sprite[] LoadSpriteFrames(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        var sprites = new System.Collections.Generic.List<Sprite>();
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite)
            {
                sprites.Add(sprite);
            }
        }

        if (sprites.Count == 0)
        {
            Sprite singleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (singleSprite != null)
            {
                sprites.Add(singleSprite);
            }
        }

        sprites.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return sprites.ToArray();
    }

    private static GameObject FindOrCreate(string name)
    {
        GameObject found = GameObject.Find(name);
        return found != null ? found : new GameObject(name);
    }

    private static GameObject FindChildOrCreate(Transform parent, string name)
    {
        Transform found = parent.Find(name);
        if (found != null)
        {
            return found.gameObject;
        }

        GameObject created = new GameObject(name);
        created.transform.SetParent(parent, false);
        return created;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
}
