using Index.Shared.RabbitMQ;
using Index.Shared.RabbitMQ.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Products.Elasticsearch;
using Index.Shared.DTOs;

namespace Index.Search.Consumers;

public class AdMessageConsumer : BackgroundService
{
    private string consumerTag;
    private AsyncEventingBasicConsumer consumer;
    private readonly IConnection _connection;
    private readonly RabbitMQConfig _config;
    private readonly ElasticsearchClient _elasticClient;
    public AdMessageConsumer(RabbitMQConfig config, RabbitMQPersistedConnection persistedConnection, ElasticsearchClient elasticClient)
    {
        _connection = persistedConnection.GetConnection();
        _config = config;
        _elasticClient = elasticClient;
    }
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageType = typeof(AdMessage).Name;
        var queueName = _config.Queues?.FirstOrDefault(q => q.MessageType == messageType)?.Name;
        if (queueName == null)
        {
            throw new ApplicationException($"Queue for message type {messageType} not found in config.");
        }

        var channel = _connection.CreateModel();
        channel.BasicQos(0, 1, false);

        consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += HandleMessage;

        try
        {
            consumerTag = channel.BasicConsume(queueName, false, consumer);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"BasicConsume for consumer {nameof(AdMessageConsumer)} failed.", ex);
        }

        await Task.CompletedTask;
    }

    private async Task HandleMessage(object ch, BasicDeliverEventArgs ea)
    {
        var channel = ((AsyncEventingBasicConsumer)ch).Model;

        try
        {
            var messageAsJson = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<AdMessage>(messageAsJson);

            ElasticsearchResponse response;

            if (message.Type == AdMessageType.Insert)
            {
                response = await _elasticClient.IndexAsync(message.Ad);
            }
            else
            {
                response = await _elasticClient.DeleteAsync<AdDto>(message.Ad.Id);
            }

            if (response.IsSuccess())
            {
                channel.BasicAck(ea.DeliveryTag, false);
            }
            else
            {
                channel.BasicReject(ea.DeliveryTag, false);
            }
        }
        catch (Exception ex)
        {
            var a = ex.Message;
            channel.BasicReject(ea.DeliveryTag, false);
        }
    }
}
