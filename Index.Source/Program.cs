using Index.Shared.RabbitMQ;
using Index.Source.RabbitMQ;
using Index.Source.Middleware;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Serilog;
using MongoDB.Driver;
using Index.Source.Repositories;

//registracija servicea i konfiguracija pipelinea se može organizirati u druge fileove ili extension metode tipa "ConfigureServices" ako bi htjeli da bude više "clean" 

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

    //DB
    builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration["MongoDB:ConnectionString"]));
    builder.Services.AddSingleton(sp => 
    {
        var mongoClient = sp.GetRequiredService<IMongoClient>();
        return mongoClient.GetDatabase(builder.Configuration["MongoDB:IndexDatabaseName"]);
    });

    builder.Services.AddSingleton<AdRepository>();

    //RabbitMQ
    builder.Services.AddOptions<RabbitMQConfig>().Bind(builder.Configuration.GetSection(RabbitMQConfig.SectionName));
    builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMQConfig>>().Value);
    builder.Services.AddScoped<RabbitMQInitializer>();
    builder.Services.AddSingleton<RabbitMQPersistedConnection>();
    builder.Services.AddScoped<RabbitMQPublisher>();
    builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory()
    {
        Uri = new Uri(builder.Configuration["RabbitMQ:Uri"]),
        ClientProvidedName = "Index",
        DispatchConsumersAsync = true
    });


    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerUI();
        app.UseSwagger();

        using var scope = app.Services.CreateScope();

        var messageBrokerStateManager = scope.ServiceProvider.GetRequiredService<RabbitMQInitializer>();
        messageBrokerStateManager.InitializeExchangesAndQueues();
    }

    app.UseMiddleware<BasicAuthMiddleware>();

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