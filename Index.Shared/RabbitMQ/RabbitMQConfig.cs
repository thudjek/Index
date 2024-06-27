namespace Index.Shared.RabbitMQ;

public class RabbitMQConfig
{
    public const string SectionName = "RabbitMQ";

    public string ExchangeName { get; set; }
    public List<QueueConfig> Queues { get; set; }
}

public class QueueConfig
{
    public string Name { get; set; }
    public string MessageType { get; set; }
}