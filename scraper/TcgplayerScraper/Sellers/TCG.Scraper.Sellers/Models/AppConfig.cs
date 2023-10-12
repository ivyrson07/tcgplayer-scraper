namespace TCG.Scraper.Sellers.Models
{
    public class AppConfig
    {
        public string Author { get; set; }

        public string Description { get; set; }

        public string BaseUrl { get; set; }

        public string SellersFirstPageUrl { get; set; }

        public string SellersSpecificPageUrl { get; set; }

        public string SellersUrl { get; set; }

        public string GetSellersFirstPageUrl(bool isCertified, bool isDirect, bool isGoldStar, int categoryId)
        {
            var url = string.Format(SellersFirstPageUrl, isDirect, isGoldStar, isCertified, categoryId);

            return url;
        }

        public string GetSellersSpecificPageUrl(bool isCertified, bool isDirect, bool isGoldStar, int categoryId, int page)
        {
            var url = string.Format(SellersSpecificPageUrl, isDirect, isGoldStar, isCertified, categoryId, page);

            return url;
        }
    }
}
