using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CodexExplorerPlayerSwapRunner
{
    private const string ScenePath = "Assets/Scene/Main.unity";
    private const string ControllerPath = "Assets/Animations/Player.controller";

    [MenuItem("Tools/Codex/Apply Explorer Player Sprites")]
    public static void Apply()
    {
        var idleSprites = LoadSprites("Assets/Sprites/Explorer/Explorer_Idle.png");
        var runSprites = LoadSprites("Assets/Sprites/Explorer/Explorer_Run.png");
        var dashSprites = LoadSprites("Assets/Sprites/Explorer/Explorer_Dash.png");
        var dieSprites = LoadSprites("Assets/Sprites/Explorer/Explorer_Die.png");

        var idleClip = CreateOrUpdateClip("Assets/Animations/Idle.anim", "Idle", idleSprites, 5f, true);
        var runClip = CreateOrUpdateClip("Assets/Animations/Run.anim", "Run", runSprites, 14f, true);
        var jumpClip = CreateOrUpdateClip("Assets/Animations/Jump.anim", "Jump", dashSprites, 10f, true);
        var dashClip = CreateOrUpdateClip("Assets/Animations/Dash.anim", "Dash", dashSprites, 12f, true);
        var dieClip = CreateOrUpdateClip("Assets/Animations/Die.anim", "Die", dieSprites, 5f, false);

        UpdateAnimatorController(idleClip, runClip, jumpClip, dashClip, dieClip);
        UpdateScenePlayerSprite(idleSprites[0]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CodexExplorerPlayerSwapRunner] Explorer player sprites applied.");
    }

    private static Sprite[] LoadSprites(string path)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => ExtractIndex(sprite.name))
            .ThenBy(sprite => sprite.name, StringComparer.Ordinal)
            .ToArray();

        if (sprites.Length == 0)
        {
            throw new InvalidOperationException($"No sprites found at {path}");
        }

        return sprites;
    }

    private static int ExtractIndex(string name)
    {
        var lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore >= 0 && int.TryParse(name.Substring(lastUnderscore + 1), out var index))
        {
            return index;
        }

        return int.MaxValue;
    }

    private static AnimationClip CreateOrUpdateClip(string path, string clipName, Sprite[] sprites, float frameRate, bool loop)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip { name = clipName };
            AssetDatabase.CreateAsset(clip, path);
        }

        clip.name = clipName;
        clip.frameRate = frameRate;

        var binding = new EditorCurveBinding
        {
            path = string.Empty,
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (var i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / frameRate,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void UpdateAnimatorController(AnimationClip idleClip, AnimationClip runClip, AnimationClip jumpClip, AnimationClip dashClip, AnimationClip dieClip)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            throw new InvalidOperationException($"AnimatorController not found at {ControllerPath}");
        }

        var stateMachine = controller.layers[0].stateMachine;
        foreach (var childState in stateMachine.states)
        {
            var state = childState.state;
            switch (state.name)
            {
                case "Idle":
                    state.motion = idleClip;
                    break;
                case "Run":
                    state.motion = runClip;
                    break;
                case "Dash":
                    state.motion = dashClip;
                    break;
                case "Fall":
                    state.motion = jumpClip;
                    break;
                case "Die":
                    state.motion = dieClip;
                    break;
            }
        }

        EditorUtility.SetDirty(controller);
    }

    private static void UpdateScenePlayerSprite(Sprite idleSprite)
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var player = GameObject.Find("Player");
        if (player == null)
        {
            throw new InvalidOperationException("Player GameObject was not found in Main scene.");
        }

        var spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            throw new InvalidOperationException("Player SpriteRenderer was not found.");
        }

        player.transform.localScale = new Vector3(0.58f, 0.58f, 0.58f);
        spriteRenderer.sprite = idleSprite;

        var bodyCollider = player.GetComponent<BoxCollider2D>();
        if (bodyCollider != null)
        {
            bodyCollider.offset = new Vector2(0f, -0.04f);
            bodyCollider.size = new Vector2(0.76f, 1.34f);
            EditorUtility.SetDirty(bodyCollider);
        }

        EditorUtility.SetDirty(player.transform);
        EditorUtility.SetDirty(spriteRenderer);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
