using Index.Shared.RabbitMQ.Messages;
using Index.Source.RabbitMQ;
using Index.Source.Models;
using Microsoft.AspNetCore.Mvc;
using Index.Shared.DTOs;
using Index.Source.Repositories;

namespace Index.Source.Controllers;

[ApiController]
[Route("source/ad")]
public class AdController : ControllerBase
{
    private readonly AdRepository _adRepository;
    private readonly RabbitMQPublisher _publisher;
    public AdController(AdRepository adRepository, RabbitMQPublisher publisher)
    {
        _adRepository = adRepository;
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> PostAd([FromBody] Ad postedAd)
    {
        await _adRepository.Create(postedAd);
        _publisher.PublishMessage(new AdMessage()
        {
            Type = AdMessageType.Insert,
            Ad = postedAd.ToDto()
        });

        return Ok(postedAd);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteAd(string id)
    {
        await _adRepository.Delete(id);
        _publisher.PublishMessage(new AdMessage()
        {
            Type = AdMessageType.Delete,
            Ad = new AdDto() { Id = id }
        });

        return NoContent();
    }
}
