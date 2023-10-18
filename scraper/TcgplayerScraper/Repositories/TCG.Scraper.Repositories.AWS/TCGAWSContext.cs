using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Newtonsoft.Json;
using TCG.Scraper.Repositories.AWS.Configurations;
using TCG.Scraper.Repositories.Entities;

namespace TCG.Scraper.Repositories.AWS
{
    public class TCGAWSContext : DynamoDBContext
    {
        private static readonly AmazonDynamoDBClient _client = new AmazonDynamoDBClient(
                new BasicAWSCredentials(
                    CommonBuilder.GetAWSConfiguration().AccessKey,
                    CommonBuilder.GetAWSConfiguration().SecretKey),
                Amazon.RegionEndpoint.APSoutheast1);

        public TCGAWSContext() : base(_client)
        {

        }

        public async Task SaveAsync<T>(T entity)
        {
            await AddCreatedTimestamps(entity);

            await base.SaveAsync(entity);
        }

        public async Task SaveMultipleAsync<T>(List<T> entityList)
        {
            List<Task> tasks = new List<Task>();

            foreach (T entity in entityList)
            {
                tasks.Add(await Task.Factory.StartNew(async () =>
                {
                    await AddCreatedTimestamps(entity);
                    await base.SaveAsync(entity);
                }));
            }

            await Task.WhenAll(tasks);
        }

        private async Task AddCreatedTimestamps<T>(T entity)
        {
            await Task.Factory.StartNew(() =>
            {
                BaseEntity baseEntity = entity as BaseEntity;

                baseEntity.Created = DateTime.Now;
                baseEntity.CreatedBy = Guid.Empty;
            });
        }
        
        private async Task AddModifiedTimestamps<T>(T entity)
        {
            await Task.Factory.StartNew(() =>
            {
                BaseEntity baseEntity = entity as BaseEntity;

                baseEntity.Modified = DateTime.Now;
                baseEntity.ModifiedBy = Guid.Empty;
            });
        }
        
        private async Task AddMultipleModifiedTimestamps<T>(IEnumerable<T> entityList)
        {
            foreach (T entity in entityList)
            {
                await AddModifiedTimestamps(entity);
            }
        }
    }
}
