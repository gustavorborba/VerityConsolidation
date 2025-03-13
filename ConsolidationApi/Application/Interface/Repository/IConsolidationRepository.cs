using ConsolidationApi.Domain.Model;

namespace ConsolidationApi.Application.Interface.Repository
{
    public interface IConsolidationRepository
    {
        Task<Consolidation> Save(Consolidation consolidation);
        Task<IEnumerable<Consolidation>> All();
        Task<IEnumerable<Consolidation>> GetLastConsolidations(int consolidationsNumber);
    }
}
