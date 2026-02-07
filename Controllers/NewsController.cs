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
        public async Task<IActionResult> Get([FromQuery] int limit = 20, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 100);

            var items = await _db.Articles
                .OrderByDescending(a => a.PublishedAt)
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

            return Ok(items);
        }
    }
}
