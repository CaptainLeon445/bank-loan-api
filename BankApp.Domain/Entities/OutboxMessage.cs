using System;
namespace BankApp.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Topic { get; private set; }
    public string Payload { get; private set; }
    public bool Processed { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    private OutboxMessage()
    {
        Topic = string.Empty;
        Payload = string.Empty;
    } //for EF core

    public OutboxMessage(string topic, string payload)
    {
        Id = Guid.NewGuid();
        Topic = string.IsNullOrWhiteSpace(topic) ? throw new ArgumentException("Invalid topic.") : topic;
        Payload = payload ?? string.Empty;
        Processed = false;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkProcessed()
    {
        Processed = true;
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
