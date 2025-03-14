using ConsolidationApi.Application.Interface.Service;
using ConsolidationApi.Application.Job;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Quartz;

namespace ConsolidationApi.Test.Job
{
    public class ConsolidationJobTest
    {
        private readonly IConsolidationService _service;
        private readonly ILogger<ConsolidationJob> _logger;
        private readonly ConsolidationJob _job;

        public ConsolidationJobTest()
        {
            _service = Substitute.For<IConsolidationService>();
            _logger = Substitute.For<ILogger<ConsolidationJob>>();
            _job = new ConsolidationJob(_service, _logger);
        }

        [Fact]
        public async Task Execute_ShouldLogStartAndFinish_WhenNoException()
        {
            // Arrange
            var context = Substitute.For<IJobExecutionContext>();

            // Act
            await _job.Execute(context);

            // Assert
            _logger.Received().LogInformation("Consolidation Job Started");
            await _service.Received().ConsolidationProcess(Arg.Any<DateTime>(), Arg.Any<DateTime>());
            _logger.Received().LogInformation("Consolidation Job Finished");
        }

        [Fact]
        public async Task Execute_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var context = Substitute.For<IJobExecutionContext>();
            var exception = new Exception("Test exception");
            _service.ConsolidationProcess(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Throws(exception);

            // Act
            await _job.Execute(context);

            // Assert
            _logger.Received().LogInformation("Consolidation Job Started");
            _logger.Received().LogError(exception, "Error on Consolidation Job");
        }
    }
}
