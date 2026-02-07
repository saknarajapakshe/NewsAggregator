# News Aggregator

A .NET 8 Web API that aggregates news from multiple sources (BBC RSS feed and NewsAPI) and stores them in a SQL Server database.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB or full SQL Server instance)
- NewsAPI API Key (get one free from [newsapi.org](https://newsapi.org))

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd NewsAggregator
```

### 2. Configure the Application

Open `appsettings.json` and update the following settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=NewsAggregator;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "NewsApi": {
    "BaseUrl": "https://newsapi.org/v2/top-headlines?country=us",
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

**Important:** Replace `YOUR_API_KEY_HERE` with your actual NewsAPI key.

### 3. Database Setup

Run the following command to apply database migrations:

```bash
dotnet ef database update
```

If you don't have the Entity Framework tools installed, run:

```bash
dotnet tool install --global dotnet-ef
```

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Build the Project

```bash
dotnet build
```

## Running the Application

### Start the Application

```bash
dotnet run
```

The application will start and display:
```
Now listening on: http://localhost:5168
Application started. Press Ctrl+C to shut down.
```

### Access the API

Once the application is running, you can access:

- **Swagger UI**: http://localhost:5168/swagger
- **API Endpoint**: http://localhost:5168/api/news

### Using Swagger UI

1. Open your browser and navigate to **http://localhost:5168/swagger**
2. You'll see the interactive API documentation
3. Click on **GET /api/news** to expand the endpoint
4. Click the **"Try it out"** button
5. Click **"Execute"** to fetch all articles
6. View the response in the **"Response body"** section below

The Swagger UI allows you to test the API without any additional tools.

### Background Worker

The news fetcher runs automatically every **5 minutes** in the background. You'll see log messages like:

```
Starting news fetch cycle with 2 fetchers
Fetching news from BBC RSS: https://feeds.bbci.co.uk/news/rss.xml
Successfully fetched 35 articles from BBC RSS
Fetching news from NewsAPI: https://newsapi.org/v2/top-headlines?country=us
Successfully fetched 20 articles from NewsAPI
Stored 55 new articles in database
```

## API Endpoints

### Get All Articles

```http
GET /api/news
```

Returns all news articles from the database.

**Example Response:**
```json
[
  {
    "id": 1,
    "title": "Breaking News Title",
    "description": "Article description...",
    "url": "https://example.com/article",
    "source": "BBC News",
    "publishedAt": "2026-02-07T10:30:00Z",
    "createdAt": "2026-02-07T10:35:00Z"
  }
]
```

**Testing with Swagger:**
Visit http://localhost:5168/swagger for interactive API documentation and testing.

## Project Structure

```
NewsAggregator/
├── BackgroundJobs/
│   └── NewsFetchWorker.cs       # Background service that fetches news every 5 minutes
├── Controllers/
│   └── NewsController.cs        # API endpoints
├── Data/
│   └── AppDbContext.cs          # Entity Framework DbContext
├── DTOs/
│   ├── ArticleResponseDto.cs    # Response data transfer object
│   └── Api/
│       └── NewsApiDtos.cs       # NewsAPI response models
├── Migrations/                  # Database migrations
├── Models/
│   └── Article.cs               # Article entity
├── Services/
│   ├── NewsAggregatorService.cs # Main aggregation service
│   ├── Fetchers/
│   │   ├── NewsApiFetcher.cs    # NewsAPI fetcher
│   │   └── RssNewsFetcher.cs    # BBC RSS fetcher
│   └── Interfaces/
│       └── INewsSourceFetcher.cs
└── appsettings.json             # Configuration
```

## News Sources

1. **BBC RSS Feed** - Fetches from `https://feeds.bbci.co.uk/news/rss.xml`
2. **NewsAPI** - Fetches US top headlines (requires API key)

## Features

- ✅ Fetches news from multiple sources
- ✅ Automatic deduplication by URL
- ✅ Background worker runs every 5 minutes
- ✅ Stores articles in SQL Server database
- ✅ RESTful API with Swagger documentation
- ✅ Retry policy with exponential backoff
- ✅ Comprehensive error handling and logging

## Troubleshooting

### NewsAPI Returns 400 Error

If you see errors like `userAgentMissing`, ensure you've restarted the application after configuring the API key.

### Database Connection Issues

If you encounter database connection errors:
1. Ensure SQL Server LocalDB is installed
2. Check the connection string in `appsettings.json`
3. Run `dotnet ef database update` to create the database

### No Articles Being Fetched

- Wait 5 minutes for the first fetch cycle to run
- Check the terminal logs for any error messages
- Verify your NewsAPI key is valid and not rate-limited

## Configuration

### Change Fetch Interval

Edit `BackgroundJobs/NewsFetchWorker.cs` and modify the timer interval:

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5)); // Change this value
```

## License

MIT
