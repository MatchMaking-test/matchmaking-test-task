using Confluent.Kafka;
using MatchMaking.Service.Services;
using Shared.Contracts;

namespace MatchMaking.Service.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly MatchStorage _matchStorage;
    private readonly IConfiguration _configuration;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger, MatchStorage matchStorage, IConfiguration configuration)
    {
        _logger = logger;
        _matchStorage = matchStorage;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KafkaConsumerService started ExecuteAsync()");
        Task.Run(() => RunConsumerLoop(stoppingToken), stoppingToken);
        return Task.CompletedTask;
    }

    private void RunConsumerLoop(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "kafka:9092",
            GroupId = "worker-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("matchmaking.complete");

        _logger.LogInformation("Kafka consumer started and subscribed to matchmaking.complete");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(cancellationToken);
                    if (!string.IsNullOrEmpty(result.Message.Value))
                    {
                        var msg = System.Text.Json.JsonSerializer.Deserialize<MatchCompleteMessage>(result.Message.Value);
                        if (msg != null)
                        {
                            _logger.LogInformation("Consumed match: {user1}, {user2}, {user3}", msg.UserIds[0], msg.UserIds[1], msg.UserIds[2]);
                            _ = _matchStorage.SaveMatchAsync(msg);
                        }
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error while consuming Kafka message.");
                    Thread.Sleep(5000); // Wait before retrying to consume
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumer canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in KafkaConsumerService.");
                }
            }
        }
        finally
        {
            _logger.LogInformation("Kafka consumer is stopping due to cancellation.");
            consumer.Close();
        }
    }
}
