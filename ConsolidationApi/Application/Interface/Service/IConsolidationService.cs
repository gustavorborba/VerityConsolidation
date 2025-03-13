namespace ConsolidationApi.Application.Interface.Service
{
    public interface IConsolidationService
    {
        Task ConsolidationProcess(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Consolidation>> GetAll();
        Task<byte[]> GenerateExcelReport(int consolidationsNumber);
    }
}
