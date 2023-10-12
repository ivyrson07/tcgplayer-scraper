namespace TCG.Scraper.Sellers.Models
{
    public class Seller
    {
        public string FeedbackUrl { get; set; }

        public bool IsCertified { get; set; }

        public bool IsDirect { get; set; }

        public bool IsGold { get; set; }

        public List<string> Listings { get; set; }

        public string Location { get; set; }

        public string Name { get; set; }

        public string Ratings { get; set; }
    }
}
