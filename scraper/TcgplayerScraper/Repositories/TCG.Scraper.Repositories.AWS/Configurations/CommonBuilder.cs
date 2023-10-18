using Microsoft.Extensions.Configuration;

namespace TCG.Scraper.Repositories.AWS.Configurations
{
    internal class CommonBuilder
    {
        private static IConfigurationRoot configurationProvider = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        internal static AWSConfig GetAWSConfiguration()
        {
            var awsConfig = new AWSConfig();

            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("AWSConfig")
                .Bind(awsConfig);

            // temporary solution
            return new AWSConfig
            {
                AccessKey = "",
                SecretKey = ""
            };

            //return awsConfig;
        }
    }
}
