// using NewsAggregator.Data;
// using NewsAggregator.Models;
// using NewsAggregator.Services.Fetchers;
// using NewsAggregator.Services.Interfaces;

// namespace NewsAggregator.Services
// {
//     public class NewsAggregatorService : INewsFetcher
//     {
//         private readonly AppDbContext _dbContext;
//         private readonly RssNewsFetcher _rssFetcher;
//         private readonly NewsApiFetcher _apiFetcher;

//         public NewsAggregatorService(AppDbContext dbContext, RssNewsFetcher rssFetcher, NewsApiFetcher apiFetcher)
//         {
//             _dbContext = dbContext;
//             _rssFetcher = rssFetcher;
//             _apiFetcher = apiFetcher;
//         }

//         public async Task FetchAndStoreArticlesAsync()
//         {
//             var rssArticles = await _rssFetcher.FetchArticlesAsync();
//             var apiArticles = await _apiFetcher.FetchArticlesAsync();

//             var allArticles = rssArticles.Concat(apiArticles);

//             foreach (var article in allArticles)
//             {
//                 if (!_dbContext.Articles.Any(a => a.Url == article.Url))
//                 {
//                     _dbContext.Articles.Add(article);
//                 }
//             }

//             await _dbContext.SaveChangesAsync();
//         }
//     }
// }
