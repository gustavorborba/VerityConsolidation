using ConsolidationApi.Domain.Model;

namespace ConsolidationApi.Application.Interface.Repository
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetPaginatedByDateInterval(DateTime startDate, DateTime endDate, int page, int size);
        Task<long> GetCountByDateInterval(DateTime startDate, DateTime endDate);
    }
}
