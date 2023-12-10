using Microsoft.Extensions.Logging;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Runners.Http
{
    internal class HttpTestRunner
    {
        ManualResetEvent finished = new ManualResetEvent(false);
        ManualResetEvent discoveryComplete = new ManualResetEvent(false);
        private readonly ILogger _logger;
        private readonly CustomAssemblyRunner _runner;

        public HttpTestRunner(ILogger logger, Assembly testAssembly)
        {
            _logger = logger;

            var testAssemblyName = testAssembly.Location;

            _runner = CustomAssemblyRunner.WithoutAppDomain(testAssemblyName);

            _runner.OnDiscoveryComplete = OnDiscoveryComplete;
            _runner.OnExecutionComplete = OnExecutionComplete;
            _runner.OnTestFailed = OnTestFailed;
            _runner.OnTestSkipped = OnTestSkipped;
        }

        public IReadOnlyCollection<ITestCase> Discover()
        {
            _runner.Discover();

            discoveryComplete.WaitOne();

            return _runner.TestCasesToRun;
        }

        public string RunTest(string testDisplayName)
        {
            var testToRun = _runner.TestCasesToRun.Single(tc => tc.DisplayName.Equals(testDisplayName));

            var response = "Passed";

            _runner.OnTestFailed = failed =>
            {
                response = "Failure: " + failed.ExceptionMessage + "\n" + failed.ExceptionStackTrace;
            };

            _runner.Run(new[] { testToRun });

            finished.WaitOne();
            finished.Reset();

            return response;
        }

        void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            _logger.LogInformation("Running {TestCasesToRun} of {TestCasesDiscovered} tests...", info.TestCasesToRun, info.TestCasesDiscovered);
            discoveryComplete.Set();
        }

        void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            _logger.LogInformation("Finished: {TotalTests} tests in {ExecutionTime}s ({TestsFailed} failed, {TestsSkipped} skipped)",
               info.TotalTests, Math.Round(info.ExecutionTime, 3), info.TestsFailed, info.TestsSkipped);

            finished.Set();
        }

        void OnTestFailed(TestFailedInfo info)
        {
            _logger.LogError("{TestDisplayName} Failed: {ExceptionMessage}", info.TestDisplayName, $"[{info.ExceptionType}] {info.ExceptionMessage} :: {info.ExceptionStackTrace}");
        }

        void OnTestSkipped(TestSkippedInfo info)
        {
            _logger.LogWarning("[SKIP] {TestDisplayName}: {SkipReason}", info.TestDisplayName, info.SkipReason);
        }
    }
}