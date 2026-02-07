using System.Xml.Linq;
using NewsAggregator.Models;
using NewsAggregator.Services.Interfaces;

namespace NewsAggregator.Services.Fetchers
{
    public class RssNewsFetcher : INewsSourceFetcher
    {
        private const string RssUrl = "https://feeds.bbci.co.uk/news/rss.xml";

        private readonly HttpClient _httpClient;
        private readonly ILogger<RssNewsFetcher> _logger;

        public RssNewsFetcher(HttpClient httpClient, ILogger<RssNewsFetcher> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Article>> FetchAsync(CancellationToken cancellationToken)
        {
            try
            {
                var xml = await _httpClient.GetStringAsync(RssUrl, cancellationToken);
                var document = XDocument.Parse(xml);

                var articles = document
                    .Descendants("item")
                    .Select(item => new Article
                    {
                        Title = (string)item.Element("title")!,
                        Url = (string)item.Element("link")!,
                        Description = (string)item.Element("description")!,
                        Source = "BBC News",
                        PublishedAt = DateTime.Parse((string)item.Element("pubDate")!)
                    })
                    .ToList();

                _logger.LogInformation("Successfully fetched {Count} articles from BBC RSS", articles.Count);
                return articles;
            }
             catch (Exception ex)
            {
                _logger.LogError(ex, "BBC RSS fetch failed");
                return Enumerable.Empty<Article>();
            }
        }
    }
}
