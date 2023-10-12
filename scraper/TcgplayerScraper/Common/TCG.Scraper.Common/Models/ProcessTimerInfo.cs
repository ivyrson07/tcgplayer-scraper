using System.Diagnostics;

namespace TCG.Scraper.Common.Models
{
    public class ProcessTimerInfo
    {
        public Guid ProcessId { get; set; }

        public Stopwatch Timer { get; set; }
    }
}
