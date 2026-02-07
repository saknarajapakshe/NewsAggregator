using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data;
using NewsAggregator.Models;
using NewsAggregator.Services.Interfaces;

namespace NewsAggregator.Services
{
    public class NewsAggregatorService
    {
        private readonly AppDbContext _db;
        private readonly IEnumerable<INewsSourceFetcher> _fetchers;
        private readonly ILogger<NewsAggregatorService> _logger;

        public NewsAggregatorService(AppDbContext db, IEnumerable<INewsSourceFetcher> fetchers, ILogger<NewsAggregatorService> logger)
        {
            _db = db;
            _fetchers = fetchers;
            _logger = logger;
        }

        public async Task FetchAndStoreAsync(CancellationToken ct)
            {_logger.LogInformation("Starting news fetch cycle with {Count} fetchers", _fetchers.Count());

            var incomingByUrl = new Dictionary<string, Article>();

            foreach (var fetcher in _fetchers)
            {
                var articles = await fetcher.FetchAsync(ct);

                foreach (var a in articles)
                {
                    if (string.IsNullOrWhiteSpace(a.Url))
                        continue;

                    // keep the first article for each URL
                    incomingByUrl.TryAdd(a.Url, a);
                }
            }

            if (incomingByUrl.Count == 0)
            {
                _logger.LogWarning("No articles fetched from any source");
                return;
            }

            _logger.LogInformation("Fetched {Count} total articles (after deduplication)", incomingByUrl.Count);

            // Find which URLs already exist in DB 
            var urls = incomingByUrl.Keys.ToList();

            var existingUrls = await _db.Articles
                .Where(x => urls.Contains(x.Url))
                .Select(x => x.Url)
                .ToListAsync(ct);

            var existingSet = existingUrls.ToHashSet();

            // Insert only new articles
            var newArticles = incomingByUrl.Values
                .Where(a => !existingSet.Contains(a.Url))
                .ToList();

            if (newArticles.Count == 0)
            {
                _logger.LogInformation("No new articles to store (all already exist in database)");
                return;
            }

            await _db.Articles.AddRangeAsync(newArticles, ct);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Stored {Count} new articles in database", newArticles.Count);
        }
    }
}
