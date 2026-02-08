# News Aggregator

A .NET 8 Web API that aggregates news from multiple sources (BBC RSS feed and NewsAPI) and stores them in a SQL Server database with automatic background fetching and cursor-based pagination.

## Features

- ✅ **Multi-source aggregation** - Fetches from BBC RSS feed and NewsAPI
- ✅ **Background worker** - Automatically fetches news every 5 minutes
- ✅ **Smart deduplication** - Prevents duplicate articles by URL
- ✅ **Cursor-based pagination** - Efficient pagination for large datasets
- ✅ **Resilience patterns** - Exponential backoff retry with Polly
- ✅ **RESTful API** - Clean API with Swagger/OpenAPI documentation
- ✅ **Entity Framework Core** - SQL Server database with migrations
- ✅ **Comprehensive logging** - Detailed logging for monitoring and debugging

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB or full SQL Server instance)
- NewsAPI API Key (get one free from [newsapi.org](https://newsapi.org))

## Quick Start

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd NewsAggregator
```

### 2. Configure Application

Edit [appsettings.json](appsettings.json) and add your NewsAPI key:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=NewsAggregator;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "NewsApi": {
    "BaseUrl": "https://newsapi.org/v2/top-headlines",
    "ApiKey": "YOUR_NEWSAPI_KEY_HERE"
  }
}
```

**Important:** Get your free API key from [newsapi.org](https://newsapi.org)

### 3. Install EF Core Tools (if needed)

```bash
dotnet tool install --global dotnet-ef
```

### 4. Setup Database

```bash
dotnet restore
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:

- **HTTP:** http://localhost:5168
- **HTTPS:** https://localhost:7282
- **Swagger UI:** http://localhost:5168/swagger

## API Documentation

### Get News Articles

**Endpoint:** `GET /news`

Retrieves paginated news articles ordered by publish date (newest first).

**Query Parameters:**

- `limit` (optional) - Number of articles to return (1-100, default: 20)
- `cursor` (optional) - DateTime cursor for pagination (ISO 8601 format)

**Example Requests:**

```bash
# Get first 20 articles
GET http://localhost:5168/news

# Get 50 articles
GET http://localhost:5168/news?limit=50

# Get next page using cursor
GET http://localhost:5168/news?cursor=2026-02-07T10:30:00Z
```

**Response:**

```json
{
  "items": [
    {
      "title": "Breaking News Title",
      "url": "https://example.com/article",
      "source": "BBC News",
      "publishedAt": "2026-02-07T10:30:00Z",
      "description": "Article description or summary..."
    }
  ],
  "nextCursor": "2026-02-07T09:15:00Z"
}
```

**Notes:**

- Use `nextCursor` value from the response for the next page
- Articles are automatically deduplicated by URL
- Both RSS and API sources are combined in results

### Interactive API Testing

**Swagger UI:** http://localhost:5168/swagger

The Swagger interface provides:

- Interactive API testing
- Request/response examples
- Schema documentation
- Try-it-out functionality

## Background Worker

The news fetcher runs **automatically** on startup and then **every 5 minutes**. You'll see log messages like:

```
[Information] Starting news fetch cycle with 2 fetchers
[Information] Successfully fetched 35 articles from BBC RSS
[Information] Fetched 55 total articles (after deduplication)
[Information] Stored 12 new articles in database
```

**When no new articles:**

```
[Information] Fetched 40 total articles (after deduplication)
[Information] No new articles to store (all already exist in database)
```

**Error handling:**

```
[Error] BBC RSS fetch failed
System.Net.Http.HttpRequestException: Connection timeout
[Warning] No articles fetched from any source
```

## Architecture

### Technology Stack

- **Framework:** .NET 8 (ASP.NET Core Web API)
- **Database:** SQL Server with Entity Framework Core 8.0
- **HTTP Resilience:** Polly (exponential backoff, retry policies)
- **Serialization:** Newtonsoft.Json
- **API Documentation:** Swashbuckle (Swagger/OpenAPI)
- **Background Jobs:** IHostedService with PeriodicTimer

### Project Structure

```
NewsAggregator/
├── BackgroundJobs/
│   └── NewsFetchWorker.cs          # Background service (runs every 5 minutes)
├── Controllers/
│   └── NewsController.cs           # REST API endpoints
├── Data/
│   └── AppDbContext.cs             # Entity Framework DbContext
├── DTOs/
│   ├── ArticleResponseDto.cs       # API response model
│   └── Api/
│       └── NewsApiDtos.cs          # NewsAPI client DTOs
├── Migrations/
│   └── [EF Core migrations]        # Database schema versioning
├── Models/
│   └── Article.cs                  # Article entity model
├── Services/
│   ├── NewsAggregatorService.cs    # Orchestrates multiple fetchers
│   ├── Fetchers/
│   │   ├── NewsApiFetcher.cs       # NewsAPI.org integration
│   │   └── RssNewsFetcher.cs       # BBC RSS feed integration
│   └── Interfaces/
│       └── INewsSourceFetcher.cs   # Fetcher abstraction
├── Properties/
│   └── launchSettings.json         # Launch profiles & ports
├── appsettings.json                # Configuration
└── Program.cs                      # Application startup & DI
```

### Design Patterns

- **Strategy Pattern:** `INewsSourceFetcher` interface allows multiple news sources
- **Dependency Injection:** All services registered in DI container
- **Repository Pattern:** Entity Framework DbContext as data access layer
- **Background Service Pattern:** `IHostedService` for scheduled tasks
- **Retry Pattern:** Polly policies for transient fault handling

### How It Works

1. **Startup:** Application starts and registers all services
2. **Background Worker:** `NewsFetchWorker` runs immediately, then every 5 minutes
3. **Fetch Cycle:**
   - Aggregator service queries all registered `INewsSourceFetcher` implementations
   - Each fetcher retrieves articles from its source (BBC RSS, NewsAPI)
   - Articles are deduplicated by URL
   - New articles (not in database) are inserted
4. **API Access:** Users query `/news` endpoint to retrieve paginated articles

### Resilience Features

- **HTTP Retry Policy:** 3 retries with exponential backoff (2^attempt seconds)
- **Transient Error Handling:** Handles network failures, timeouts, 429 (Too Many Requests)
- **Timeout Configuration:** 10-second timeout per HTTP request
- **Error Isolation:** Failed fetchers don't stop other sources from working

## News Sources

### 1. BBC RSS Feed

- **URL:** `https://feeds.bbci.co.uk/news/rss.xml`
- **Type:** RSS 2.0
- **Configuration:** No API key required
- **Coverage:** International news from BBC

### 2. NewsAPI

- **URL:** `https://newsapi.org/v2/top-headlines`
- **Type:** REST API
- **Configuration:** Requires free API key
- **Coverage:** US top headlines (configurable)
- **Rate Limit:** 100 requests/day (free tier)

## Configuration Options

### Connection Strings

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=NewsAggregator;Trusted_Connection=True;TrustServerCertificate=True"
}
```

**Alternative configurations:**

- **SQL Server:** `Server=localhost;Database=NewsAggregator;User Id=sa;Password=YourPassword;TrustServerCertificate=True`
- **Azure SQL:** `Server=tcp:yourserver.database.windows.net,1433;Database=NewsAggregator;User ID=yourusername;Password=yourpassword;Encrypt=True;`

### NewsAPI Settings

```json
"NewsApi": {
  "BaseUrl": "https://newsapi.org/v2/top-headlines",
  "ApiKey": "your_api_key_here"
}
```

**To change country:**
Update the query parameter in [NewsApiFetcher.cs](Services/Fetchers/NewsApiFetcher.cs):

```csharp
var url = $"{baseUrl}?country=gb&apiKey={apiKey}";  // Change 'us' to desired country code
```

Available countries: us, gb, ca, au, de, fr, in, etc.

### Customize Fetch Interval

Edit [NewsFetchWorker.cs](BackgroundJobs/NewsFetchWorker.cs):

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));  // Change from 5 to 10 minutes
```

### Add Custom News Sources

1. Create a new fetcher class implementing `INewsSourceFetcher`:

```csharp
public class CustomNewsFetcher : INewsSourceFetcher
{
    public async Task<IEnumerable<Article>> FetchAsync(CancellationToken ct)
    {
        // Your fetch logic here
    }
}
```

2. Register it in [Program.cs](Program.cs):

```csharp
builder.Services.AddScoped<INewsSourceFetcher, CustomNewsFetcher>();
```

## Troubleshooting

### Database Connection Issues

**Problem:** `Cannot open database "NewsAggregator" requested by the login`

**Solutions:**

1. Ensure SQL Server LocalDB is installed (comes with Visual Studio or SQL Server Express)
2. Run database migrations:
   ```bash
   dotnet ef database update
   ```
3. Verify connection string in [appsettings.json](appsettings.json)

### NewsAPI Errors

**Problem:** HTTP 401 - Unauthorized

**Solutions:**

- Verify your API key is correct in [appsettings.json](appsettings.json)
- Ensure no extra spaces or quotes around the API key
- Get a new key from [newsapi.org](https://newsapi.org)

**Problem:** HTTP 429 - Too Many Requests

**Solutions:**

- Free tier allows 100 requests/day
- NewsAPI is called every 5 minutes (288 times/day)
- Consider upgrading your NewsAPI plan or increasing fetch interval

### No Articles in Database

**Problem:** API returns empty results

**Solutions:**

1. Check application logs for errors
2. Wait 5 minutes for first fetch cycle (runs on startup, then every 5 minutes)
3. Verify both news sources are accessible:
   - BBC RSS: https://feeds.bbci.co.uk/news/rss.xml
   - NewsAPI: Use your API key in a browser test

### Missing Entity Framework Tools

**Problem:** `dotnet ef` command not found

**Solution:**

```bash
dotnet tool install --global dotnet-ef
```

### Port Already in Use

**Problem:** `Address already in use: localhost:5168`

**Solutions:**

- Stop other instances of the application
- Change port in [Properties/launchSettings.json](Properties/launchSettings.json)
- Or specify port at runtime:
  ```bash
  dotnet run --urls "http://localhost:5000"
  ```

## Development

### Database Migrations

**Create new migration:**

```bash
dotnet ef migrations add MigrationName
```

**Apply migrations:**

```bash
dotnet ef database update
```

**Rollback migration:**

```bash
dotnet ef database update PreviousMigrationName
```

**Remove last migration:**

```bash
dotnet ef migrations remove
```

### Testing the API

**Using curl:**

```bash
# Get latest 20 articles
curl http://localhost:5168/news

# Get 50 articles
curl "http://localhost:5168/news?limit=50"

# Pagination
curl "http://localhost:5168/news?cursor=2026-02-07T10:30:00Z"
```

**Using PowerShell:**

```powershell
Invoke-RestMethod -Uri "http://localhost:5168/news?limit=10" -Method Get | ConvertTo-Json
```

### Dependencies

| Package                                 | Version | Purpose                            |
| --------------------------------------- | ------- | ---------------------------------- |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0   | SQL Server database provider       |
| Microsoft.EntityFrameworkCore.Tools     | 8.0.0   | Migration and design-time tools    |
| Microsoft.Extensions.Http.Polly         | 10.0.2  | HTTP resilience and retry policies |
| Newtonsoft.Json                         | 13.0.4  | JSON serialization                 |
| Swashbuckle.AspNetCore                  | 6.6.2   | OpenAPI/Swagger documentation      |

### Environment Variables

Override configuration using environment variables:

```bash
# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=NewsAggregator;..."
$env:NewsApi__ApiKey="your_key_here"
dotnet run

# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=localhost;Database=NewsAggregator;..."
export NewsApi__ApiKey="your_key_here"
dotnet run
```

## Deployment Considerations

### Production Checklist

- [ ] Use production-grade SQL Server (not LocalDB)
- [ ] Store API keys in Azure Key Vault or similar
- [ ] Enable HTTPS and configure certificates
- [ ] Configure proper logging (Application Insights, Serilog)
- [ ] Set up health check endpoints
- [ ] Configure connection pooling and timeouts
- [ ] Implement rate limiting
- [ ] Add authentication/authorization if needed
- [ ] Set up monitoring and alerts

### Docker Support (Optional)

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "NewsAggregator.dll"]
```

## Future Enhancements

- [ ] Add search and filtering capabilities
- [ ] Implement caching (Redis)
- [ ] Add more news sources
- [ ] Support for categories/topics
- [ ] User preferences and subscriptions
- [ ] Email/push notifications
- [ ] Article sentiment analysis
- [ ] Full-text search
- [ ] GraphQL API support

## License

MIT License - feel free to use this project for learning or production.

## Support

For issues or questions:

1. Check the [Troubleshooting](#troubleshooting) section
2. Review application logs
3. Verify configuration in [appsettings.json](appsettings.json)

---

**Built with .NET 8 | Last Updated: February 2026**
