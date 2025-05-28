using Microsoft.AspNetCore.Mvc;
using MatchMaking.Service.Kafka;
using MatchMaking.Service.Services;
using Shared.Contracts;

[ApiController]
[Route("matchmaking")]
public class MatchmakingController : ControllerBase
{
     private readonly KafkaProducerService _kafka;
     private readonly MatchStorage _matchStorage;

    public MatchmakingController(KafkaProducerService kafka, MatchStorage matchStorage)
    {
        _kafka = kafka;
        _matchStorage = matchStorage;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        await _kafka.SendAsync(new MatchRequestMessage(userId));
        return NoContent();
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] string userId)
    {
        var match = await _matchStorage.GetMatchByUserIdAsync(userId);
        if (match is null) return NotFound();
        return Ok(match);
    }
}
