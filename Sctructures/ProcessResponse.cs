using Newtonsoft.Json.Linq;

namespace Cybernetically.Json.RuleEngine.Sctructures;

public record ProcessResponse
{
    public required List<Rule> NegativeRules { get; init; }
    public required JToken Result { get; init; }
}