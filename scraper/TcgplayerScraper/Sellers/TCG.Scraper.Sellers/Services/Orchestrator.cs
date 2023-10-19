using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TCG.Scraper.Common;
using TCG.Scraper.Sellers.Models;
using TCG.Scraper.Sellers.Helpers;

namespace TCG.Scraper.Sellers.Services
{
    public interface IOrchestrator
    {
        Task StartAsync(CancellationToken cancellationToken);
    }

    public class Orchestrator : BaseScraper, IOrchestrator
    {
        private readonly ISellersService _sellerService;
        private readonly IProcessTimer _processTimer;

        private readonly AppConfig _appConfig;

        private readonly ILogger<Orchestrator> _logger;

        public Orchestrator(
            ISellersService sellerService,
            IProcessTimer processTimer,
            IOptions<AppConfig> appConfig,
            ILogger<Orchestrator> logger)
        {
            _sellerService = sellerService;
            _processTimer = processTimer;
            _appConfig = appConfig.Value;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var scrapingProcessId = Guid.NewGuid();
                var dbSavingProcessId = Guid.NewGuid();

                _processTimer.ProcessStartTimer(scrapingProcessId, "Started new scraping process");

                var sellers = new List<Seller>();
                var sellersCount = 0;
                var totalPages = await GetTotalPages();

                _logger.LogInformation($"Total Pages : {totalPages}");

                var segments = TaskSegment.GetSegments(totalPages, 8);

                foreach (var segment in segments)
                {
                    var tasks = new Task[(segment.Maximum - segment.Minimum) + 1];

                    for (var page = segment.Minimum; page <= segment.Maximum; page++)
                    {
                        var taskExecutionSuccess = false;
                        var task = Task.CompletedTask;

                        try
                        {
                            task = await Task.Factory.StartNew(async () =>
                            {
                                var requestUrl = GetRequestUrl(page);
                                var doc = await LoadWebPageAsync(requestUrl);

                                var sellersDiv = doc.DocumentNode
                                    .SelectNodes("//div[contains(@class, 'scWrapper')]");

                                foreach (var div in sellersDiv)
                                {
                                    var seller = new Seller
                                    {
                                        FeedbackUrl = $"{_appConfig.BaseUrl}",
                                        Id = "",
                                        IsCertified = false,
                                        IsDirect = false,
                                        IsGold = false,
                                        Listings = new List<string>(),
                                        Location = "",
                                        Name = "",
                                        Ratings = ""
                                    };

                                    var feedbackAnchor = div
                                        .SelectSingleNode(".//div[contains(@class, 'scTitle largetext')]//a[1]");

                                    if (feedbackAnchor != null)
                                    {
                                        seller.Name = feedbackAnchor
                                            .InnerText
                                            .Replace("&amp;", "&");

                                        seller.FeedbackUrl += feedbackAnchor
                                            .Attributes["href"]
                                            .Value;

                                        seller.Id = Trimmer.GetRemainingString(seller.FeedbackUrl, "sellerfeedback/");
                                    }

                                    var sellerInfo = div
                                        .SelectSingleNode(".//div[contains(@class, 'scPrice')]")
                                        .InnerHtml
                                        .Replace("/", "")
                                        .Replace(" ", "")
                                        .Replace("\r", "")
                                        .Replace("\n", "")
                                        .Split("<br>");

                                    seller.Location = sellerInfo[0].Replace("Location:", "");
                                    seller.Listings = sellerInfo[1]
                                        .Replace("Listings:", "")
                                        .Split(",")
                                        .Where(x => x != "")
                                        .ToList();

                                    var sellerBadge = div.SelectSingleNode(".//div[contains(@class, 'iconLineContainer')]");

                                    if (sellerBadge != null)
                                    {
                                        seller.IsCertified = sellerBadge.SelectSingleNode(".//span[contains(@class, 'iconCertified')]") != null;
                                        seller.IsDirect = sellerBadge.SelectSingleNode(".//span[contains(@class, 'iconDirect')]") != null;
                                        seller.IsGold = sellerBadge.SelectSingleNode(".//span[contains(@class, 'iconGold')]") != null;
                                        seller.Ratings = sellerBadge.SelectSingleNode(".//span[contains(text(), 'Rating')]").InnerText;
                                    }

                                    sellers.Add(seller);

                                    _logger.LogInformation($"Scraped seller : {seller.Name} - {seller.Location} - {seller.Ratings}");

                                    _processTimer.ProcessStartTimer(dbSavingProcessId, $"Saving {seller.Name} to DynamoDB.");

                                    await _sellerService.CreateSeller(seller);

                                    _processTimer.ProcessEndTimer(dbSavingProcessId, $"End of process, execution of _sellerService.CreateSeller(sellers) done.");
                                }
                            });

                            taskExecutionSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);

                            await DelayScraping();

                            taskExecutionSuccess = await Retry(task);
                        }

                        if (taskExecutionSuccess)
                        {
                            tasks[page - segment.Minimum] = task;
                        }
                        else
                        {
                            tasks[page - segment.Minimum] = Task.CompletedTask;
                        }
                    }

                    await Task.WhenAll(tasks);

                    await DelayScraping();
                }

                _processTimer.ProcessEndTimer(scrapingProcessId, $"End of process, scraped {sellersCount} sellers.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
        }

        private async Task<int> GetTotalPages()
        {
            try
            {
                var doc = await LoadWebPageAsync(GetRequestUrl());

                var lastPageButton = doc.DocumentNode
                    .SelectSingleNode("//a[contains(@class, 'pageLast') and contains(text(), 'LAST')]");

                if (lastPageButton != null)
                {
                    var href = lastPageButton
                        .Attributes["href"]
                        .Value;

                    var totalPages = Convert.ToInt32(Trimmer.GetRemainingString(href, "page="));

                    return totalPages;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }

            return 0;
        }

        private string GetRequestUrl(int? page = null, bool isCertified = false, bool isDirect = false, bool isGoldStar = false)
        {
            var categoryId = 2;

            if (page != null)
            {
                return _appConfig.GetSellersSpecificPageUrl(isCertified, isDirect, isGoldStar, categoryId, page.Value);
            }
            else
            {
                return _appConfig.GetSellersFirstPageUrl(isCertified, isDirect, isGoldStar, categoryId);
            }
        }

        private async Task DelayScraping()
        {
            var scraperDelayInMinutes = _appConfig.ScraperDelayTime / 60000;

            _logger.LogInformation($"SCRAPING PROCESS PAUSED FOR {scraperDelayInMinutes} MINUTE{(scraperDelayInMinutes > 1 ? "S" : "")}. PLEASE WAIT.");

            await Task.Delay(_appConfig.ScraperDelayTime);
        }
    }
}
