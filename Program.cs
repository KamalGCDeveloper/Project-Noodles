using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Noodle.Api.Data;
using Noodle.Api.Repositories;
using Noodle.Api.Services;
using Noodle.Api.Helpers;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Load configurations đầy đủ (json + env variables)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// 2️⃣ Bind DatabaseSettings (Mongo)
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

builder.Services.AddSingleton<IMongoClient>(s =>
{
    var settings = s.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// 3️⃣ CORS đa môi trường
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? new string[] { };

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers();

// Repositories + Services
builder.Services.AddScoped<IStablecoinsRepository, StablecoinsRepository>();
builder.Services.AddScoped<IStablecoinsService, StablecoinsService>();
builder.Services.AddScoped<IStocksRepository, StocksRepository>();
builder.Services.AddScoped<IStocksService, StocksService>();
builder.Services.AddScoped<ICommoditiesService, CommoditiesService>();
builder.Services.AddScoped<ICommoditiesRepository, CommoditiesRepository>();
builder.Services.AddScoped<IPriceHistoryService, PriceHistoryService>();
builder.Services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
builder.Services.AddScoped<IComparisonService, ComparisonService>();
builder.Services.AddScoped<IComparisonRepository, ComparisonRepository>();
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Background job
BestYieldHelper.StartAutoRefresh();

// Enable CORS
app.UseCors("AllowNextJs");

// Swagger only for Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Basic middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();