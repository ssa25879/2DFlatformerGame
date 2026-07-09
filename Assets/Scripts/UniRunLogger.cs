using UnityEngine;

public enum UniRunLogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

public interface IUniRunLogSink
{
    void Log(UniRunLogLevel level, string message, Object context);
}

public static class UniRunLogger
{
    private static readonly IUniRunLogSink DefaultSink = new UnityDebugLogSink();
    private static IUniRunLogSink sink = DefaultSink;

    public static UniRunLogLevel MinimumLevel { get; set; } = UniRunLogLevel.Info;

    public static bool IsEnabled(UniRunLogLevel level)
    {
        return level >= MinimumLevel;
    }

    public static void Debug(string category, string message, Object context = null)
    {
        Log(UniRunLogLevel.Debug, category, message, context);
    }

    public static void Info(string category, string message, Object context = null)
    {
        Log(UniRunLogLevel.Info, category, message, context);
    }

    public static void Warning(string category, string message, Object context = null)
    {
        Log(UniRunLogLevel.Warning, category, message, context);
    }

    public static void Error(string category, string message, Object context = null)
    {
        Log(UniRunLogLevel.Error, category, message, context);
    }

    public static void Log(UniRunLogLevel level, string category, string message, Object context = null)
    {
        if (!IsEnabled(level))
        {
            return;
        }

        sink.Log(level, FormatMessage(category, message), context);
    }

    public static void SetSinkForTest(IUniRunLogSink testSink)
    {
        sink = testSink ?? DefaultSink;
    }

    public static void ResetForTest()
    {
        sink = DefaultSink;
        MinimumLevel = UniRunLogLevel.Info;
    }

    private static string FormatMessage(string category, string message)
    {
        string safeCategory = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();
        return "[" + safeCategory + "] " + message;
    }

    private sealed class UnityDebugLogSink : IUniRunLogSink
    {
        public void Log(UniRunLogLevel level, string message, Object context)
        {
            switch (level)
            {
                case UniRunLogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message, context);
                    break;
                case UniRunLogLevel.Error:
                    UnityEngine.Debug.LogError(message, context);
                    break;
                default:
                    UnityEngine.Debug.Log(message, context);
                    break;
            }
        }
    }
}
