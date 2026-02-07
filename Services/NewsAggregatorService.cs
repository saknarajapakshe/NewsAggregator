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

        public NewsAggregatorService(AppDbContext db, IEnumerable<INewsSourceFetcher> fetchers)
        {
            _db = db;
            _fetchers = fetchers;
        }

        public async Task FetchAndStoreAsync(CancellationToken ct)
        {
            // 1) Fetch + dedupe by URL (avoid duplicates across sources)
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
                return;

            // 2) Find which URLs already exist in DB (single query)
            var urls = incomingByUrl.Keys.ToList();

            var existingUrls = await _db.Articles
                .Where(x => urls.Contains(x.Url))
                .Select(x => x.Url)
                .ToListAsync(ct);

            var existingSet = existingUrls.ToHashSet();

            // 3) Insert only new articles
            var newArticles = incomingByUrl.Values
                .Where(a => !existingSet.Contains(a.Url))
                .ToList();

            if (newArticles.Count == 0)
                return;

            await _db.Articles.AddRangeAsync(newArticles, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}
