using AutoMapper;
using ConsolidationApi.Application.Dto;

namespace ConsolidationApi.Application.Mapping
{
    public class ConsolidationProfile : Profile
    {
        public ConsolidationProfile()
        {
            CreateMap<Consolidation, ConsolidationDto>();
        }
    }
}
