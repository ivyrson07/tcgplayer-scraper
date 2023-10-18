using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCG.Scraper.Repositories.AWS.Configurations
{
    internal class AWSConfig
    {
        internal string AccessKey { get; set; }

        internal string SecretKey { get; set; }
    }
}
