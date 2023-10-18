namespace TCG.Scraper.Repositories.Entities
{
    public class BaseEntity
    {
        public DateTime Created { get; set; }

        public Guid CreatedBy { get; set; } = Guid.Empty;

        public DateTime Modified { get; set; }

        public Guid ModifiedBy { get; set; } = Guid.Empty;
    }
}