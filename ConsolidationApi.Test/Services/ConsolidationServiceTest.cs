using ConsolidationApi.Application.Interface.Repository;
using ConsolidationApi.Application.Service;
using ConsolidationApi.Domain.Enum;
using ConsolidationApi.Domain.Model;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ConsolidationApi.Test.Services
{
    public class ConsolidationServiceTest
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IConsolidationRepository _consolidationRepository;
        private readonly ILogger<ConsolidationService> _logger;
        private readonly ConsolidationService _consolidationService;

        public ConsolidationServiceTest()
        {
            _transactionRepository = Substitute.For<ITransactionRepository>();
            _consolidationRepository = Substitute.For<IConsolidationRepository>();
            _logger = Substitute.For<ILogger<ConsolidationService>>();
            _consolidationService = new ConsolidationService(_transactionRepository, _consolidationRepository, _logger);
        }

        [Fact]
        public async Task ConsolidationProcess_ShouldLogInformation_WhenNoRecordsToProcess()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;
            _transactionRepository.GetCountByDateInterval(startDate, endDate).Returns(0);

            // Act
            await _consolidationService.ConsolidationProcess(startDate, endDate);

            // Assert
            _logger.Received().LogInformation("No Records to be processed");
        }

        [Fact]
        public async Task ConsolidationProcess_ShouldProcessChunks_WhenRecordsExist()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 31);
            var totalRecords = 2500;
            var transactions = new List<Transaction>
            {
                new() { Type = TransactionType.Debit, Value = 100 },
                new() { Type = TransactionType.Credit, Value = 200 }
            };

            _transactionRepository.GetCountByDateInterval(startDate, endDate).Returns(totalRecords);
            _transactionRepository.GetPaginatedByDateInterval(startDate, endDate, Arg.Any<int>(), Arg.Any<int>()).Returns(transactions);

            // Act
            await _consolidationService.ConsolidationProcess(startDate, endDate);

            // Assert
            await _transactionRepository.Received(1).GetCountByDateInterval(startDate, endDate);
            await _transactionRepository.ReceivedWithAnyArgs(3).GetPaginatedByDateInterval(startDate, endDate, default, default);
            await _consolidationRepository.Received(1).Save(Arg.Any<Consolidation>());
        }

        [Fact]
        public async Task ConsolidationProcess_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;
            _transactionRepository.GetCountByDateInterval(startDate, endDate).Returns(Task.FromException<long>(new Exception("Test Exception")));

            // Act
            await Assert.ThrowsAsync<Exception>(() => _consolidationService.ConsolidationProcess(startDate, endDate));
            // Assert
            _logger.Received().LogError(Arg.Any<Exception>(), "Error on Consolidation Process");
        }

        [Fact]
        public async Task GenerateExcelReport_ShouldReturnByteArray()
        {
            // Arrange
            var consolidationsNumber = 5;
            var consolidations = new List<Consolidation>
            {
                new() { CreditTotal = 100, DebitTotal = 50, Total = 150, DateCreated = DateTime.UtcNow }
            };
            _consolidationRepository.GetLastConsolidations(consolidationsNumber).Returns(consolidations);

            // Act
            var result = await _consolidationService.GenerateExcelReport(consolidationsNumber);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllConsolidations()
        {
            // Arrange
            var consolidations = new List<Consolidation>
            {
                new() { CreditTotal = 100, DebitTotal = 50, Total = 150, DateCreated = DateTime.UtcNow }
            };
            _consolidationRepository.All().Returns(consolidations);

            // Act
            var result = await _consolidationService.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(consolidations, result);
        }
    }
}