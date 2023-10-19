using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using TCG.Scraper.Common;

namespace TCG.Scraper
{
    public class BaseScraper
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public BaseScraper()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .AddDebug();
            });

            _logger = _loggerFactory.CreateLogger<BaseScraper>();
        }

        public async Task<HtmlDocument> LoadWebPageAsync(string requestUrl)
        {
            var timer = new ProcessTimer();
            var processId = Guid.NewGuid();

            timer.ProcessStartTimer(processId, $"Requesting {requestUrl}");

            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(requestUrl);

                timer.ProcessEndTimer(processId, "Web page loaded successfully");

                return doc;
            }
            catch (Exception ex)
            {
                timer.ProcessEndTimer(processId, $"Error occured");

                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);

                return null;
            }

        }

        public async Task<bool> Retry(Task task)
        {
            var retryCount = 1;

            while (retryCount <= 5)
            {
                try
                {
                    _logger.LogWarning($"Re-execute task, retry: {retryCount}");

                    await task;

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);

                    retryCount++;
                }
            }

            return false;
        }
    }
}
