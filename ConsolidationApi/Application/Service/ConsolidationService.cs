using AutoMapper;
using BalanceLedgerApi.Application.Dto;
using ClosedXML.Excel;
using ConsolidationApi.Application.Dto;
using System.Collections.Concurrent;

namespace ConsolidationApi.Application.Service
{
    public class ConsolidationService(ITransactionRepository _transactionRepository, IConsolidationRepository _consolidationRepository, 
        IMapper _mapper, ILogger<ConsolidationService> _logger) : IConsolidationService
    {
        private const int PAGE_SIZE = 1000;
        public async Task ConsolidationProcess(DateTime startDate, DateTime endDate)
        {
            try
            {
                await Run(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on Consolidation Process");
                throw;
            }
        }

        public async Task<byte[]> GenerateExcelReport(int consolidationsNumber)
        {
            var data = await _consolidationRepository.GetLastConsolidations(consolidationsNumber);
            return GenerateExcelReport([.. data]);
        }

        public async Task<CommonResponseDto<IEnumerable<ConsolidationDto>>> GetAll() 
        {
            try
            {
                var consolidations = _mapper.Map<IEnumerable<ConsolidationDto>>(await _consolidationRepository.All());

                return CommonResponseDto<IEnumerable<ConsolidationDto>>.SuccessResponse(consolidations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consolidations");
                return CommonResponseDto<IEnumerable<ConsolidationDto>>.ErrorResponse(ex.Message);
                throw;
            }
                  
        }

        private async Task Run(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Consolidation Process Started with StartDate = {StartDate} and EndDate = {EndDate}", startDate, endDate);
            var totalOfRecordsToBeProcessed = await _transactionRepository.GetCountByDateInterval(startDate, endDate);

            if (totalOfRecordsToBeProcessed == 0)
            {
                _logger.LogInformation("No Records to be processed");
                return;
            }
            _logger.LogInformation("Total of Records to be processed: {TotalOfRecordsToBeProcessed}", totalOfRecordsToBeProcessed);

            var chunks = (int)Math.Ceiling(totalOfRecordsToBeProcessed / (double)PAGE_SIZE);

            var debitTotalBag = new ConcurrentBag<decimal>();
            var creditTotalBag = new ConcurrentBag<decimal>();

            await ProcessChunks(startDate, endDate, chunks, debitTotalBag, creditTotalBag);
            _logger.LogInformation("All Chunks Processed");

            var debitTotal = debitTotalBag.Sum();
            var creditTotal = creditTotalBag.Sum();

            var finalConsolidation = new Consolidation()
            {
                DateCreated = DateTime.UtcNow,
                DebitTotal = debitTotal,
                CreditTotal = creditTotal,
                Total = creditTotal + debitTotal
            };

            await _consolidationRepository.Save(finalConsolidation);

            _logger.LogInformation("Consolidation Process Finished");
        }

        private async Task ProcessChunks(DateTime startDate, DateTime endDate, int chumks, ConcurrentBag<decimal> debitTotalBag, ConcurrentBag<decimal> creditTotalBag)
        {
            var tasks = new List<Task>();
            // limitando o numero de tasks para 5
            var semaphore = new SemaphoreSlim(5);

            for (int i = 0; i < chumks; i++)
            {
                var page = i + 1;
                _logger.LogInformation("Processing Chunk {Chunk} of {TotalChunks}", i, chumks);
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var transactions = await _transactionRepository.GetPaginatedByDateInterval(startDate, endDate, page, PAGE_SIZE);

                        debitTotalBag.Add(transactions.Where(t => t.Type == TransactionType.Debit).Sum(t => t.Value));
                        creditTotalBag.Add(transactions.Where(t => t.Type == TransactionType.Credit).Sum(t => t.Value));
                    }
                    finally
                    {
                        semaphore.Release();
                    }          
                }));
            }

            await Task.WhenAll(tasks);
        }

        private static byte[] GenerateExcelReport(List<Consolidation> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Consolidated Data");

            worksheet.Cell(1, 1).Value = "Créditos";
            worksheet.Cell(1, 2).Value = "Débitos";
            worksheet.Cell(1, 3).Value = "Total Consolidado";
            worksheet.Cell(1, 4).Value = "Data de Consolidação";

            for (int i = 0; i < data.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = data[i].CreditTotal;
                worksheet.Cell(i + 2, 2).Value = data[i].DebitTotal;
                worksheet.Cell(i + 2, 3).Value = data[i].Total;
                worksheet.Cell(i + 2, 4).Value = data[i].DateCreated.ToString("dd-MM-yyyy");
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
