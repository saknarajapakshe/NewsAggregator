using NewsAggregator.Models;

namespace NewsAggregator.Services.Interfaces
{
    public interface INewsFetcher
    {
        Task<IEnumerable<Article>> FetchAsync(CancellationToken cancellationToken);
    }
}