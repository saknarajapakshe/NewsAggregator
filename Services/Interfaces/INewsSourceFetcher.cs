using NewsAggregator.Models;

namespace NewsAggregator.Services.Interfaces
{
    public interface INewsSourceFetcher
    {
        Task<IEnumerable<Article>> FetchAsync(CancellationToken cancellationToken);
    }
}