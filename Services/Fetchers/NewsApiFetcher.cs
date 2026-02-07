using Newtonsoft.Json;
using NewsAggregator.Models;
using NewsAggregator.Services.Interfaces;

namespace NewsAggregator.Services.Fetchers
{
    public class NewsApiFetcher : INewsSourceFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NewsApiFetcher(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<IEnumerable<Article>> FetchAsync(CancellationToken cancellationToken)
        {
            var apiKey = _configuration["NewsApi:ApiKey"];
            var baseUrl = _configuration["NewsApi:BaseUrl"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
                return Enumerable.Empty<Article>();

            var requestUrl = $"{baseUrl}&apiKey={apiKey}";

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var newsApiResponse = JsonConvert.DeserializeObject<NewsApiResponse>(json);

            if (newsApiResponse?.Articles == null)
                return Enumerable.Empty<Article>();

            var articles = new List<Article>();

            foreach (var item in newsApiResponse.Articles)
            {
                articles.Add(new Article
                {
                    Title = item.Title!.Trim(),
                    Url = item.Url!.Trim(),
                    Description = item.Description?.Trim(),
                    Source = item.Source?.Name ?? "Unknown Source",
                    PublishedAt = item.PublishedAt?.ToUniversalTime() ?? DateTime.UtcNow
                });
            }

            return articles;
        }
    }
}
