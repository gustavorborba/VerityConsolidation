using ConsolidationApi.Application.Dto;

namespace ConsolidationApi.Application.Interface.Service
{
    public interface IConsolidationService
    {
        Task ConsolidationProcess(DateTime startDate, DateTime endDate);
        Task<CommonResponseDto<IEnumerable<ConsolidationDto>>> GetAll();
        Task<byte[]> GenerateExcelReport(int consolidationsNumber);
    }
}
