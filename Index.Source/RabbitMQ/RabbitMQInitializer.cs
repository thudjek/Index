using Index.Shared.RabbitMQ;
using RabbitMQ.Client;

namespace Index.Source.RabbitMQ;

public class RabbitMQInitializer
{
    private readonly RabbitMQConfig _config;
    private readonly RabbitMQPersistedConnection _persistedConnection;

    public RabbitMQInitializer(RabbitMQConfig config, RabbitMQPersistedConnection persistedConnection)
    {
        _config = config;
        _persistedConnection = persistedConnection;
    }

    public void InitializeExchangesAndQueues()
    {
        var connection = _persistedConnection.GetConnection();
        var channel = connection.CreateModel();

        channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Direct);

        foreach (var queue in _config.Queues)
        {
            //tu se mogu deklarirati i dead letter queueovi za svaki queue

            var queueArgs = new Dictionary<string, object>
            {
                { "x-queue-type", "quorum" },
                { "overflow", "reject-publish" }
            };

            channel.QueueDeclare(queue.Name, true, false, false, queueArgs);
            channel.QueueBind(queue.Name, _config.ExchangeName, queue.Name, null);
        }

        channel.Close();
    }
}
