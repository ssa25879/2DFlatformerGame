using System.Collections.Generic;
using NUnit.Framework;

public class UniRunLoggerEditModeTests
{
    private RecordingLogSink sink;

    [SetUp]
    public void SetUp()
    {
        sink = new RecordingLogSink();
        UniRunLogger.SetSinkForTest(sink);
        UniRunLogger.MinimumLevel = UniRunLogLevel.Debug;
    }

    [TearDown]
    public void TearDown()
    {
        UniRunLogger.ResetForTest();
    }

    [Test]
    public void Info_WritesCategoryAndMessageToSink()
    {
        UniRunLogger.Info("Dash", "Started");

        Assert.AreEqual(1, sink.Entries.Count);
        Assert.AreEqual(UniRunLogLevel.Info, sink.Entries[0].Level);
        Assert.AreEqual("[Dash] Started", sink.Entries[0].Message);
    }

    [Test]
    public void Log_IgnoresMessagesBelowMinimumLevel()
    {
        UniRunLogger.MinimumLevel = UniRunLogLevel.Warning;

        UniRunLogger.Info("Dash", "Started");
        UniRunLogger.Warning("Dash", "Unavailable");

        Assert.AreEqual(1, sink.Entries.Count);
        Assert.AreEqual(UniRunLogLevel.Warning, sink.Entries[0].Level);
    }

    [Test]
    public void IsEnabled_ReportsWhetherLevelMeetsMinimumLevel()
    {
        UniRunLogger.MinimumLevel = UniRunLogLevel.Warning;

        Assert.IsFalse(UniRunLogger.IsEnabled(UniRunLogLevel.Info));
        Assert.IsTrue(UniRunLogger.IsEnabled(UniRunLogLevel.Error));
    }

    private sealed class RecordingLogSink : IUniRunLogSink
    {
        public readonly List<LogEntry> Entries = new List<LogEntry>();

        public void Log(UniRunLogLevel level, string message, UnityEngine.Object context)
        {
            Entries.Add(new LogEntry(level, message, context));
        }
    }

    private readonly struct LogEntry
    {
        public LogEntry(UniRunLogLevel level, string message, UnityEngine.Object context)
        {
            Level = level;
            Message = message;
            Context = context;
        }

        public UniRunLogLevel Level { get; }
        public string Message { get; }
        public UnityEngine.Object Context { get; }
    }
}
