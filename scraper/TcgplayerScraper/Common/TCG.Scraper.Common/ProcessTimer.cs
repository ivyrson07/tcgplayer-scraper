using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TCG.Scraper.Common.Models;

namespace TCG.Scraper.Common
{
    public interface IProcessTimer
    {
        void ProcessStartTimer(Guid processId, string message = "");

        void ProcessEndTimer(Guid processId, string message = "");
    }

    public class ProcessTimer : IProcessTimer
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private List<ProcessTimerInfo> _processTimerInfo;

        public ProcessTimer()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .AddDebug();
            });

            _logger = _loggerFactory.CreateLogger<ProcessTimer>();
            _processTimerInfo = new List<ProcessTimerInfo>();
        }

        public void ProcessStartTimer(Guid processId, string message = "")
        {
            _processTimerInfo.Add(new ProcessTimerInfo
            {
                ProcessId = processId,
                Timer = Stopwatch.StartNew()
            });

            if (!string.IsNullOrWhiteSpace(message))
            {
                _logger.LogInformation(message);
                _logger.LogInformation($"PROCESS ID : {processId}");
            }
        }

        public void ProcessEndTimer(Guid processId, string message = "")
        {
            var process = _processTimerInfo.FirstOrDefault(x => x.ProcessId == processId);

            if (process != null)
            {
                process.Timer.Stop();

                var ts = process.Timer.Elapsed;
                var hours = $"{ts.Hours} hour{(ts.Hours > 1 ? "s" : "")}";
                var minutes = $"{ts.Minutes} minute{(ts.Minutes > 1 ? "s" : "")}";
                var seconds = $"{ts.Seconds} second{(ts.Seconds > 1 ? "s" : "")}";
                var milliseconds = $"{ts.Milliseconds} ms";

                if (!string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogInformation(message);
                    _logger.LogInformation($"PROCESS ID : {processId}");
                    _logger.LogInformation($"PROCESS DURATION : {hours}, {minutes}, {seconds}, {milliseconds}");
                }
            }
        }
    }
}
