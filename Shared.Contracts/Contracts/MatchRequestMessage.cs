using System.Text.Json.Serialization;

namespace Shared.Contracts;

public record MatchRequestMessage
{
	[JsonConstructor]
	public MatchRequestMessage(string userId)
	{
		UserId = userId;
	}

	public string UserId { get; init; }
}
