using ConsolidationApi.Application.Interface.Repository;
using ConsolidationApi.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace ConsolidationApi.Data
{
    public class ConsolidationRepository(AppDbContext _context) : IConsolidationRepository
    {
        public async Task<Consolidation> Save(Consolidation consolidation)
        {
            await _context.Consolidations.AddAsync(consolidation);
            await _context.SaveChangesAsync();
            return consolidation;
        }

        public async Task<IEnumerable<Consolidation>> GetLastConsolidations(int consolidationsNumber) 
            => await _context.Consolidations.AsNoTracking()
            .OrderByDescending(c => c.DateCreated)
            .Take(consolidationsNumber).ToListAsync();

        public async Task<IEnumerable<Consolidation>> All() => await _context.Consolidations.AsNoTracking().ToListAsync();
    }
}
