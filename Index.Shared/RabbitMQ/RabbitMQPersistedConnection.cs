using RabbitMQ.Client;

namespace Index.Shared.RabbitMQ;

public sealed class RabbitMQPersistedConnection : IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection _connection;
    public RabbitMQPersistedConnection(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IConnection GetConnection()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _connection = _connectionFactory.CreateConnection();
        }

        return _connection;
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}