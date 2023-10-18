using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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
            AddCreatedTimestamps(entity);

            await base.SaveAsync(entity);
        }

        public async Task SaveMultipleAsync<T>(List<T> entityList)
        {
            await AddMultipleCreatedTimestamps(entityList);
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

        private async Task AddMultipleCreatedTimestamps<T>(List<T> entityList)
        {
            foreach (T entity in entityList)
            {
                await AddCreatedTimestamps(entity);

                await base.SaveAsync(entity);
            }
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

                await base.SaveAsync(entity);
            }
        }
    }
}
