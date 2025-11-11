using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Noodle.Api.Data;
using Noodle.Api.Repositories;
using Noodle.Api.Services;
using Noodle.Api.Helpers;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Thêm cấu hình DatabaseSettings từ appsettings.json
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

// 2️⃣ Đăng ký MongoClient để dùng qua Dependency Injection
builder.Services.AddSingleton<IMongoClient>(s =>
{
    var settings = s.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// 3️⃣ Thêm Controller (sẽ dùng nếu bạn thêm file Controllers/*.cs)
builder.Services.AddControllers();

// ===== Register Repositories & Services =====
builder.Services.AddScoped<IStablecoinsRepository, StablecoinsRepository>();
builder.Services.AddScoped<IStablecoinsService, StablecoinsService>();
builder.Services.AddScoped<IStocksRepository, StocksRepository>();
builder.Services.AddScoped<IStocksService, StocksService>();

// 4️⃣ Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

BestYieldHelper.StartAutoRefresh();

// 5️⃣ Swagger cho môi trường dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 6️⃣ Middleware cơ bản
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ✅ Chạy ứng dụng
app.Run();