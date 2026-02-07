using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsAggregator.Services;

namespace NewsAggregator.BackgroundJobs
{
    public class NewsFetchWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NewsFetchWorker> _logger;

        public NewsFetchWorker(IServiceScopeFactory scopeFactory, ILogger<NewsFetchWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var aggregator = scope.ServiceProvider.GetRequiredService<NewsAggregatorService>();

                    await aggregator.FetchAndStoreAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching and storing news.");
                }
            }
        }
    }
}
