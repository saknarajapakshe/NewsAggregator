using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data;
using NewsAggregator.DTOs;

namespace NewsAggregator.Controllers
{
    [ApiController]
    [Route("news")]
    public class NewsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NewsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int limit = 20,
            [FromQuery] DateTime? cursor = null,
            CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 100);

            var query = _db.Articles.AsNoTracking();

            // Cursor pagination
            if (cursor.HasValue)
            {
                query = query.Where(a => a.PublishedAt < cursor.Value);
            }

            var items = await query
                .OrderByDescending(a => a.PublishedAt)
                .ThenByDescending(a => a.Id) //when multiple articles have the same PublishedAt, order by Id to ensure consistent pagination
                .Take(limit)
                .Select(a => new ArticleResponseDto
                {
                    Title = a.Title,
                    Url = a.Url,
                    Source = a.Source,
                    PublishedAt = a.PublishedAt,
                    Description = a.Description
                })
                .ToListAsync(ct);

            var nextCursor = items.Count > 0 ? items[^1].PublishedAt : (DateTime?)null;

            return Ok(new
            {
                items,
                nextCursor
            });
        }
    }
}
