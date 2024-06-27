using System.Text.Json;
using System.Text;
using Index.Shared.RabbitMQ;

namespace Index.Source.RabbitMQ;

public class RabbitMQPublisher
{
    private readonly RabbitMQPersistedConnection _persistedConnection;
    private readonly RabbitMQConfig _config;
    public RabbitMQPublisher(RabbitMQPersistedConnection persistedConnection, RabbitMQConfig config)
    {
        _persistedConnection = persistedConnection;
        _config = config;
    }

    public void PublishMessage<TMessage>(TMessage message)
    {
        var messageType = typeof(TMessage).Name;
        var queueName = _config.Queues?.FirstOrDefault(q => q.MessageType == messageType)?.Name;

        if (queueName == null)
        {
            throw new ApplicationException($"Failed to publish message. Queue for message type {messageType} not found in config");
        }

        var connection = _persistedConnection.GetConnection();
        var channel = connection.CreateModel();

        var messageAsJson = JsonSerializer.Serialize(message);
        var messageBytes = Encoding.UTF8.GetBytes(messageAsJson);

        try
        {
            channel.BasicPublish(_config.ExchangeName, queueName, false, null, messageBytes);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to publish message.", ex);
        }

        channel.Close();
    }
}
