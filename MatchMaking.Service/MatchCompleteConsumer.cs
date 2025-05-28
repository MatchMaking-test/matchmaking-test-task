using Confluent.Kafka;
using Shared.Contracts;
using StackExchange.Redis;
using System.Text.Json;

public class MatchCompleteConsumer : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ConsumerConfig _config = new()
    {
        BootstrapServers = "kafka:9092",
        GroupId = "matchmaking.service",
        AutoOffsetReset = AutoOffsetReset.Earliest
    };

    public MatchCompleteConsumer(IConnectionMultiplexer redis) => _redis = redis;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
        consumer.Subscribe("matchmaking.complete");

        var db = _redis.GetDatabase();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(stoppingToken);
                var msg = JsonSerializer.Deserialize<MatchCompleteMessage>(cr.Message.Value);

                if (msg is null) continue;

                await db.StringSetAsync($"match:{msg.MatchId}", JsonSerializer.Serialize(msg.UserIds));
                foreach (var userId in msg.UserIds)
                    await db.StringSetAsync($"user:{userId}:match", msg.MatchId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error consuming match: {ex.Message}");
            }
        }
    }
}