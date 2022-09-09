using DiffusionWorkerOMSFIlesIntegration.Application.Configuration;
using DiffusionWorkerOMSFIlesIntegration.Application.Services.Interfaces;

namespace DiffusionWorkerOMSFIlesIntegration
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IProcessHandler<string> _handler;

        public Worker(ILogger<Worker> logger, IProcessHandler<string> handler)
        {
            _logger = logger;
            _handler = handler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _handler.StartProcessAsync("");
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}