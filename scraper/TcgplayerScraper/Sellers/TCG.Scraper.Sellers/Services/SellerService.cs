using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCG.Scraper.Sellers.Models;
using TCG.Scraper.Repositories.AWS;
using TCG.Scraper.Repositories.AWS.Entities;

namespace TCG.Scraper.Sellers.Services
{
    public interface ISellersService
    {
        Task CreateSeller(Seller seller);
    }

    public class SellerService : ISellersService
    {
        private readonly TCGAWSContext _tcgAwsContext;

        private readonly IMapper _mapper;

        private readonly ILogger<Orchestrator> _logger;

        public SellerService(
            TCGAWSContext tcgAwsContext,
            IMapper mapper,
            ILogger<Orchestrator> logger)
        {
            _tcgAwsContext = tcgAwsContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task CreateSeller(Seller seller)
        {
            var entity = _mapper.Map<SellerEntity>(seller);

            await _tcgAwsContext.SaveAsync(entity);
        }
    }
}
