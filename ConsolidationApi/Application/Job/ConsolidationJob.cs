using ConsolidationApi.Application.Interface.Service;
using Quartz;

namespace ConsolidationApi.Application.Job
{
    public class ConsolidationJob(IConsolidationService _service, ILogger<ConsolidationJob> _logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {          
            try
            {
                _logger.LogInformation("Consolidation Job Started");              
                await _service.ConsolidationProcess(DateTime.Now.AddDays(-1), DateTime.Now);
                _logger.LogInformation("Consolidation Job Finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on Consolidation Job");
            }          
        }
    }
}
