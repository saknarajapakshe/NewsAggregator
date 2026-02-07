using Polly;
using Polly.Extensions.Http;
using System.Net;
using NewsAggregator.Data;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Services.Fetchers;
using NewsAggregator.Services.Interfaces;
using NewsAggregator.Services;
using NewsAggregator.BackgroundJobs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("NewsClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

// Typed HttpClients (clean + testable)
builder.Services.AddHttpClient<RssNewsFetcher>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
}).AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<NewsApiFetcher>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
    c.DefaultRequestHeaders.Add("User-Agent", "NewsAggregator/1.0");
}).AddPolicyHandler(retryPolicy);

// Register fetchers as sources
builder.Services.AddScoped<INewsSourceFetcher, RssNewsFetcher>();
builder.Services.AddScoped<INewsSourceFetcher, NewsApiFetcher>();


// Aggregator + worker
builder.Services.AddScoped<NewsAggregatorService>();
builder.Services.AddHostedService<NewsAggregator.BackgroundJobs.NewsFetchWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
