using Newtonsoft.Json.Linq;

namespace Cybernetically.Json.RuleEngine.Sctructures;

public record ProcessResponse
{
    public required TrainedModel Model { get; init; }
    public required JToken Result { get; init; }
}
