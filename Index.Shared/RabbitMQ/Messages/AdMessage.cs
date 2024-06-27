using Index.Shared.DTOs;

namespace Index.Shared.RabbitMQ.Messages;

public class AdMessage
{
    public AdMessageType Type { get; set; }
    public AdDto Ad { get; set; }
}

public enum AdMessageType
{
    Insert,
    Delete
}