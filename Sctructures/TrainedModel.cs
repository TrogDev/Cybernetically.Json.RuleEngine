using System.Data;

namespace Cybernetically.Json.RuleEngine.Sctructures;

public record TrainedModel
{
    public List<Rule> PositiveRules { get; init; } = [];
    public List<Rule> NegativeRules { get; init; } = [];
}
