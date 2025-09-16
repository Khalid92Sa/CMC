using CMC.Presentation.Application.DTOs.Competitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Competitions
{
    public class CompetitionUpdateQueue : BackgroundService, ICompetitionUpdateQueue
    {
        private readonly ConcurrentQueue<CompetitionStateDto> _queue = new ConcurrentQueue<CompetitionStateDto>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CompetitionUpdateQueue> _logger;

        public CompetitionUpdateQueue(
            IServiceProvider serviceProvider,
            ILogger<CompetitionUpdateQueue> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void QueueUpdate(CompetitionStateDto competitionStateData)
        {
            _queue.Enqueue(competitionStateData);
            _signal.Release();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _signal.WaitAsync(stoppingToken);

                    if (_queue.TryDequeue(out var competitionData))
                    {
                        // Create a new scope for each update to avoid tracking issues
                        using var scope = _serviceProvider.CreateScope();
                        var competitionsService = scope.ServiceProvider.GetRequiredService<ICompetitionsService>();

                        try
                        {
                            await competitionsService.UpdateCompetitonState(competitionData);
                            _logger.LogInformation($"Successfully updated competition state for ID: {competitionData.Id}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error updating competition state for ID: {competitionData.Id}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in competition update queue");
                    await Task.Delay(1000, stoppingToken); // Brief delay before continuing
                }
            }
        }
    }
}
