using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public static class CodexEditModeTestRunner
{
    private const string ResultPath = "Logs/EditModeResults.xml";
    private static TestRunnerApi activeApi;
    private static CodexEditModeTestCallbacks activeCallback;

    public static void RunEditModeTests()
    {
        Directory.CreateDirectory("Logs");

        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        var settings = new ExecutionSettings(new Filter
        {
            testMode = UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,
            assemblyNames = new[] { "UniRun.EditModeTests" }
        })
        {
            runSynchronously = true
        };

        var callback = ScriptableObject.CreateInstance<CodexEditModeTestCallbacks>();
        callback.ResultPath = ResultPath;
        api.RegisterCallbacks(callback);
        api.Execute(settings);
        api.UnregisterCallbacks(callback);

        if (!callback.HasResult)
        {
            throw new System.InvalidOperationException("EditMode tests did not produce a result.");
        }

        Debug.Log($"[CodexEditModeTestRunner] Passed={callback.PassCount}, Failed={callback.FailCount}, Skipped={callback.SkipCount}, Inconclusive={callback.InconclusiveCount}");
        if (callback.FailCount > 0)
        {
            throw new System.InvalidOperationException("EditMode tests failed. See " + ResultPath);
        }
    }

    public static void StartEditModeTestsAsync()
    {
        Directory.CreateDirectory("Logs");

        activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        activeCallback = ScriptableObject.CreateInstance<CodexEditModeTestCallbacks>();
        activeCallback.ResultPath = ResultPath;
        activeCallback.OnRunFinished = () =>
        {
            activeApi.UnregisterCallbacks(activeCallback);
            activeApi = null;
            activeCallback = null;
        };

        var settings = new ExecutionSettings(new Filter
        {
            testMode = UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,
            assemblyNames = new[] { "UniRun.EditModeTests" }
        });

        activeApi.RegisterCallbacks(activeCallback);
        activeApi.Execute(settings);
        Debug.Log("[CodexEditModeTestRunner] EditMode tests started asynchronously.");
    }

    private sealed class CodexEditModeTestCallbacks : ScriptableObject, ICallbacks
    {
        public string ResultPath;
        public System.Action OnRunFinished;
        public bool HasResult;
        public int PassCount;
        public int FailCount;
        public int SkipCount;
        public int InconclusiveCount;

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            HasResult = true;
            PassCount = result.PassCount;
            FailCount = result.FailCount;
            SkipCount = result.SkipCount;
            InconclusiveCount = result.InconclusiveCount;
            TestRunnerApi.SaveResultToFile(result, ResultPath);
            Debug.Log($"[CodexEditModeTestRunner] Passed={PassCount}, Failed={FailCount}, Skipped={SkipCount}, Inconclusive={InconclusiveCount}");
            OnRunFinished?.Invoke();
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }
    }
}
