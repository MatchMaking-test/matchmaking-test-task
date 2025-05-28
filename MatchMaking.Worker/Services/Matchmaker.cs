using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Text.Json;
using Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MatchMaking.Worker.Services;

public class Matchmaker : BackgroundService
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly int _usersPerMatch = 3;
    private readonly ILogger<Matchmaker> _logger;

    public Matchmaker(ILogger<Matchmaker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "worker-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "kafka:9092"
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        consumer.Subscribe("matchmaking.request");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consume = consumer.Consume(stoppingToken);
                var raw = consume.Message.Value;
                _logger.LogInformation("Received raw message: '{raw}'", raw);

                if (string.IsNullOrWhiteSpace(raw))
                {
                    _logger.LogWarning("Empty or whitespace message received, skipping...");
                    continue;
                }

                MatchRequestMessage? msg = null;
                try
                {
                    msg = JsonSerializer.Deserialize<MatchRequestMessage>(raw);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error for message: '{raw}'", raw);
                    continue;
                }

                if (msg is null)
                {
                    _logger.LogWarning("Deserialized message is null, skipping...");
                    continue;
                }

                _queue.Enqueue(msg.UserId);

                if (_queue.Count >= _usersPerMatch)
                {
                    var users = new List<string>();
                    while (users.Count < _usersPerMatch && _queue.TryDequeue(out var u))
                        users.Add(u);

                    var matchId = Guid.NewGuid().ToString();
                    var payload = new MatchCompleteMessage(matchId, users);
                    var json = JsonSerializer.Serialize(payload);

                    await producer.ProduceAsync("matchmaking.complete", new Message<Null, string> { Value = json });
                    _logger.LogInformation("Match created: {MatchId} with users: {Users}", matchId, string.Join(", ", users));
                }
            }
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
            await Task.Delay(5000, stoppingToken); // Retry after delay, TODO: delete 
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for message: '{raw}'", ex.Message);

        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Matchmaker service is stopping...");
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Kafka consumer closed.");
        }
    }
}
