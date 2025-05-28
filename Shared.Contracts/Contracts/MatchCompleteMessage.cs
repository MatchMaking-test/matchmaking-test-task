
namespace Shared.Contracts;

public record MatchCompleteMessage(string MatchId, List<string> UserIds);
