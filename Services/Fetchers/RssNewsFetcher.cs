using System.Xml.Linq;
using NewsAggregator.Models;
using NewsAggregator.Services.Interfaces;

namespace NewsAggregator.Services.Fetchers
{
    public class RssNewsFetcher : INewsFetcher
    {
        private const string RssUrl = "https://feeds.bbci.co.uk/news/rss.xml";

        private readonly HttpClient _httpClient;

        public RssNewsFetcher(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Article>> FetchAsync(CancellationToken cancellationToken)
        {
            var xml = await _httpClient.GetStringAsync(RssUrl, cancellationToken);

            var document = XDocument.Parse(xml);

            return document
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
        }
    }
}
