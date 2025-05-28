using MatchMaking.Service.Kafka;
using MatchMaking.Service.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();

// Custom services
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddSingleton<MatchStorage>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Matchmaking API",
        Version = "v1"
    });
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    Console.WriteLine("[Swagger] Not in Development env, but loading anyway");
}

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Matchmaking API V1");
    c.RoutePrefix = string.Empty;
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
