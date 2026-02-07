using Newtonsoft.Json;
using NewsAggregator.Models;
using NewsAggregator.Services.Interfaces;

namespace NewsAggregator.Services.Fetchers
{
    public class NewsApiFetcher : INewsSourceFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<NewsApiFetcher> _logger;

        public NewsApiFetcher(HttpClient httpClient, IConfiguration config, ILogger<NewsApiFetcher> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<IEnumerable<Article>> FetchAsync(CancellationToken ct)
        {
            var apiKey = _config["NewsApi:ApiKey"];
            var baseUrl = _config["NewsApi:BaseUrl"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(baseUrl))
                return Enumerable.Empty<Article>();

            var url = $"{baseUrl}?country=us&apiKey={apiKey}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "NewsAggregator/1.0");

                var response = await _httpClient.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(ct);
                var data = JsonConvert.DeserializeObject<NewsApiResponse>(json);

                if (data?.Articles == null) return Enumerable.Empty<Article>();

                return data.Articles
                    .Where(a => !string.IsNullOrWhiteSpace(a.Title) && !string.IsNullOrWhiteSpace(a.Url))
                    .Select(a => new Article
                    {
                        Title = a.Title!.Trim(),
                        Url = a.Url!.Trim(),
                        Description = a.Description?.Trim(),
                        Source = a.Source?.Name?.Trim() ?? "Unknown Source",
                        PublishedAt = a.PublishedAt?.ToUniversalTime() ?? DateTime.UtcNow
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NewsAPI fetch failed");
                return Enumerable.Empty<Article>();
            }
        }
    }
}
