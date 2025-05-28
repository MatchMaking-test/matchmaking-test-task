using Confluent.Kafka;
using System.Text.Json;
using Shared.Contracts;

namespace MatchMaking.Service.Kafka;

public class KafkaProducerService
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService()
    {
        var config = new ProducerConfig { BootstrapServers = "kafka:9092" };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public Task SendAsync(MatchRequestMessage msg)
    {
        var json = JsonSerializer.Serialize(msg);
        return _producer.ProduceAsync("matchmaking.request", new Message<Null, string> { Value = json });
    }
}
