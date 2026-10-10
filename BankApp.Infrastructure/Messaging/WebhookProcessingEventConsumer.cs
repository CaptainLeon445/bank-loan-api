using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BankApp.Infrastructure.Messaging;

public class WebhookProcessingEventConsumer : BackgroundService
{
    private readonly IKafkaClientConnectionFactory _connectionFactory;
    private readonly ILogger<WebhookProcessingEventConsumer> _logger;

    public WebhookProcessingEventConsumer(IKafkaClientConnectionFactory connectionFactory, ILogger<WebhookProcessingEventConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Spawning execution sequence context directly off synchronous pipeline layers
        return Task.Run(async () =>
        {
            using var consumer = _connectionFactory.GetConsumer("webhook-processing-group");
            consumer.Subscribe(new[] { "thirdparty-webhooks-received", "loan-events" });

            _logger.LogInformation("Kafka event consumer engine listening stream configurations mounted safely.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult == null) continue;

                    // Asynchronous Event Processing Framework Router
                    _logger.LogInformation("Ingested message segment footprint from Topic: {Topic} | Payload: {Payload}", 
                        consumeResult.Topic, consumeResult.Message.Value);

                    // Execute downstream background workflows (e.g., scoring validation or notifying customers via SMS)
                    await ProcessEventPayloadAsync(consumeResult.Topic, consumeResult.Message.Value);

                    // Explicitly commit current read head state only after processing finishes successfully
                    consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka stream pipeline tracking consumption failure validation error.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Downstream service exception hit while running transaction events.");
                }
            }
            
            consumer.Close();
        }, stoppingToken);
    }

    private async Task ProcessEventPayloadAsync(string topic, string payload)
    {
        // Internal business mapping routing steps 
        await Task.Delay(10); 
    }
}
