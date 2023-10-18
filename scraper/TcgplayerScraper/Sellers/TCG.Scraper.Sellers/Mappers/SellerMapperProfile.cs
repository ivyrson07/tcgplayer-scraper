using AutoMapper;
using TCG.Scraper.Repositories.AWS.Entities;
using TCG.Scraper.Sellers.Models;

namespace TCG.Scraper.Sellers.Mappers
{
    public class SellerMapperProfile : Profile
    {
        public SellerMapperProfile()
        {
            CreateMap<SellerEntity, Seller>()
                .ReverseMap();
        }
    }
}
