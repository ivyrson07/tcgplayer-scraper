using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using TCG.Scraper.Repositories.Entities;

namespace TCG.Scraper.Repositories.AWS.Entities
{
    [DynamoDBTable("Seller")]
    public class SellerEntity : BaseEntity
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

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
