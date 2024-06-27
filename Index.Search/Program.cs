using Elastic.Clients.Elasticsearch;
using Index.Search.Consumers;
using Index.Search.Elasticsearch;
using Index.Shared.RabbitMQ;
using Index.Shared.RabbitMQ.Messages;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo()
        {
            Version = "v1",
            Title = "Index.Source",
            Description = "Index.Source REST API docs"
        });
    });

    //RabbitMQ
    builder.Services.AddOptions<RabbitMQConfig>().Bind(builder.Configuration.GetSection(RabbitMQConfig.SectionName));
    builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMQConfig>>().Value);
    builder.Services.AddSingleton<RabbitMQPersistedConnection>();
    builder.Services.AddHostedService<AdMessageConsumer>();
    builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory()
    {
        Uri = new Uri(builder.Configuration["RabbitMQ:Uri"]),
        ClientProvidedName = "Index",
        DispatchConsumersAsync = true
    });

    //Elasticsearch
    builder.Services.AddScoped<ElasticsearchInitializer>();
    var esSettings = new ElasticsearchClientSettings(new Uri(builder.Configuration["ElasticSearch:Uri"]))
        .DefaultIndex(builder.Configuration["ElasticSearch:DefaultIndex"]);

    builder.Services.AddSingleton(new ElasticsearchClient(esSettings));

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerUI();
        app.UseSwagger();

        using var scope = app.Services.CreateScope();

        var elasticSearchInitializer = scope.ServiceProvider.GetRequiredService<ElasticsearchInitializer>();
        await elasticSearchInitializer.InitializeIndex();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to start");
}
finally
{
    Log.Information("Shutting down");
    Log.CloseAndFlush();
}
