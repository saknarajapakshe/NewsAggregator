using Newtonsoft.Json;

namespace NewsAggregator.Services.Fetchers
{
    public class NewsApiResponse
    {
        [JsonProperty("articles")]
        public List<NewsApiArticle>? Articles { get; set; }
    }

    public class NewsApiArticle
    {
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("publishedAt")]
        public DateTime? PublishedAt { get; set; }

        [JsonProperty("source")]
        public NewsApiSource? Source { get; set; }
    }

    public class NewsApiSource
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}
