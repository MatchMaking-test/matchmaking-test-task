using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Shared.Contracts;

namespace MatchMaking.Service.Services;

public class MatchStorage
{
    private readonly IDistributedCache _redis;
    private readonly ILogger<MatchStorage> _logger;

    public MatchStorage(IDistributedCache redis, ILogger<MatchStorage> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task SaveMatchAsync(MatchCompleteMessage match)
    {
        var json = JsonSerializer.Serialize(match);
        foreach (var userId in match.UserIds)
        {
            _logger.LogInformation("[REDIS SET] user:{userId} => {json}", userId, json);
            await _redis.SetStringAsync($"user:{userId}", json);
        }
            var usersJson = JsonSerializer.Serialize(match.UserIds);
            await _redis.SetStringAsync($"match:{match.MatchId}", usersJson);
            _logger.LogInformation("[REDIS SET] match:{matchId} => {usersJson}", match.MatchId, usersJson);
    }

    public async Task<MatchCompleteMessage?> GetMatchByUserIdAsync(string userId)
    {
        var json = await _redis.GetStringAsync($"user:{userId}");
        _logger.LogInformation("[REDIS GET] user:{userId} => {json}", userId, json);
        return json is null ? null : JsonSerializer.Deserialize<MatchCompleteMessage>(json);
    }
}
