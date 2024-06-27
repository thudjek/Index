using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;

namespace Index.Search.Elasticsearch;

public class ElasticsearchInitializer
{
    private readonly ElasticsearchClient _client;
    private readonly IConfiguration _configuration;
    public ElasticsearchInitializer(ElasticsearchClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task InitializeIndex()
    {
        var indexName = _configuration["ElasticSearch:DefaultIndex"];

        var existsResponse = await _client.Indices.ExistsAsync(indexName);

        if (!existsResponse.Exists)
        {
            var createIndexResponse = await _client.Indices.CreateAsync(indexName, i => 
            {
                i.Mappings(m =>
                {
                    m.Properties(new Properties()
                    {
                        { "id", new KeywordProperty() },
                        { "title", new KeywordProperty() },
                        { "categories", new KeywordProperty() }
                    });
                });
            });

            if (!createIndexResponse.IsValidResponse)
            {
                throw new ApplicationException($"Failed to create index {indexName}: {createIndexResponse.ElasticsearchServerError.Error.Reason}");
            }
        }
    }
}
