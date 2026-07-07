using UnityEditor;

public static class CodexPixelStageArtApplyRunner
{
    public static void Execute()
    {
        AssetDatabase.Refresh();
        CodexDashPlatformerSceneSetup.ApplyPixelStageArtMap();
    }
}
