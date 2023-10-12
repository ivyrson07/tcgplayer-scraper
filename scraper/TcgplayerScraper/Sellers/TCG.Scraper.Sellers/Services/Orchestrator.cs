﻿using Microsoft.Extensions.Logging;
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
        private readonly IProcessTimer _processTimer;

        private readonly AppConfig _appConfig;

        private readonly ILogger<Orchestrator> _logger;

        public Orchestrator(
            IProcessTimer processTimer,
            IOptions<AppConfig> appConfig,
            ILogger<Orchestrator> logger)
        {
            _processTimer = processTimer;
            _appConfig = appConfig.Value;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var processId = Guid.NewGuid();

                _processTimer.ProcessStartTimer(processId, "Started new scraping process");

                var sellers = new List<Seller>();
                var totalPages = await GetTotalPages();

                _logger.LogInformation($"Total Pages : {totalPages}");

                var segments = TaskSegment.GetSegments(totalPages, 8);

                foreach (var segment in segments)
                {
                    var tasks = new Task[(segment.Maximum - segment.Minimum) + 1];

                    for (var page = segment.Minimum; page <= segment.Maximum; page++)
                    {
                        tasks[page - segment.Minimum] = await Task.Factory.StartNew(async () =>
                        {
                            var doc = await LoadWebPageAsync(GetRequestUrl(page));

                            var sellersDiv = doc.DocumentNode
                                .SelectNodes("//div[contains(@class, 'scWrapper')]");

                            foreach (var div in sellersDiv)
                            {
                                var seller = new Seller
                                {
                                    FeedbackUrl = $"{_appConfig.BaseUrl}",
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
                            }
                        });
                    }

                    await Task.WhenAll(tasks);

                    _logger.LogInformation("SCRAPING PROCESS PAUSED FOR 1 MINUTE. PLEASE WAIT.");
                    await Task.Delay(60000);
                }

                _processTimer.ProcessEndTimer(processId, $"End of process, scraped {sellers.Count} sellers.");
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

                    var keyword = "page=";
                    var startIndex = href.IndexOf(keyword) + keyword.Length;
                    var totalPages = Convert.ToInt32(href.Substring(startIndex));

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
    }
}