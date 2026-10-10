using System.Collections.Concurrent;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace BankApp.Infrastructure.Messaging;

public interface IKafkaClientConnectionFactory
{
    IProducer<string, string> GetProducer();
    IConsumer<string, string> GetConsumer(string groupId);
}

public class KafkaClientConnectionFactory : IKafkaClientConnectionFactory
{
    private readonly IConfiguration _config;
    public KafkaClientConnectionFactory(IConfiguration config) => _config = config;

    public IProducer<string, string> GetProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 5
        };
        return new ProducerBuilder<string, string>(config).Build();
    }

    public IConsumer<string, string> GetConsumer(string groupId)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit= false
        };
        return new ConsumerBuilder<string, string>(config).Build();
    }
}