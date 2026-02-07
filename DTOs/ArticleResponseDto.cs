namespace NewsAggregator.DTOs
{
    public class ArticleResponseDto
    {
        public string Title { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string Source { get; set; } = null!;
        public DateTime PublishedAt { get; set; }
        public string? Description { get; set; }
    }
}
