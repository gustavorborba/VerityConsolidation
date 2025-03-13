using ConsolidationApi.Application.Interface.Repository;
using ConsolidationApi.Domain.Model;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace ConsolidationApi.Data
{
    public class TransactionRepository(IMongoDatabase database) : ITransactionRepository
    {
        private readonly IMongoCollection<Transaction> _context = database.GetCollection<Transaction>("Transactions");

        public async Task<long> GetCountByDateInterval(DateTime startDate, DateTime endDate) => await _context.CountDocumentsAsync(t => t.DateCreated.Date >= startDate.Date && t.DateCreated.Date < endDate.Date);

        public async Task<IEnumerable<Transaction>> GetPaginatedByDateInterval(DateTime startDate, DateTime endDate, int page, int size)
        {
            return await _context.Find(t => t.DateCreated.Date >= startDate.Date && t.DateCreated.Date <= endDate.Date)
                .Skip((page - 1) * size)
                .Limit(size)
                .ToListAsync();
        }
    }
}
