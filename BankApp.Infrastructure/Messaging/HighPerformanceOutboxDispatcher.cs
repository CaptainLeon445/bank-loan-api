using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

using BankApp.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BankApp.Infrastructure.Messaging;

public class HighPerformanceOutboxDispatcher : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IKafkaClientConnectionFactory _connectionFactory;
    private readonly ILogger<HighPerformanceOutboxDispatcher> _logger;

    public HighPerformanceOutboxDispatcher(
        IServiceProvider services,
        IKafkaClientConnectionFactory connectionFactory,
        ILogger<HighPerformanceOutboxDispatcher> logger
    )
    {
        _services = services;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var producer = _connectionFactory.GetProducer();
        _logger.LogInformation("Kafka Outbox dispatcher infrastructure daemon engine successfully mounted.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BankDbContext>();

                // atomic batch isolation scanning
                var pendingMessages = await dbContext.OutboxMessages
                .Where(m=>!m.Processed)
                .OrderBy(m=>m.CreatedAtUtc)
                .Take(50)
                .ToListAsync(stoppingToken);

                if (pendingMessages.Any())
                {
                    foreach (var msg in pendingMessages)
                    {
                        var kafkaMessage = new Message<string, string>
                        {
                            Key=msg.Id.ToString(),
                            Value=msg.Payload
                        };

                        await producer.ProduceAsync(msg.Topic, kafkaMessage, stoppingToken);
                        msg.MarkProcessed();
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Dispatched batch envelope transaction footprint blocks count: {Count}", pendingMessages.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while running backgrounnd transaction outbox sync loops.");
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}