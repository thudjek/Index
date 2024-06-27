using Elastic.Clients.Elasticsearch;
using Index.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Index.Search.Controllers;

[ApiController]
[Route("search/ad")]
public class AdController : ControllerBase
{
    private readonly ElasticsearchClient _elasticClient;
    public AdController(ElasticsearchClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAds([FromQuery] int pageNumber)
    {
        var response = await _elasticClient.SearchAsync<AdDto>(s => s
            .From((pageNumber * 10) - 10)
            .Size(10)
            .Sort(so => so
                .Field(a => a.Title, fs => fs.Order(SortOrder.Asc))));

        if (response.IsValidResponse)
        {
            return Ok(response.Documents);
        }

        return BadRequest();
    }
}
