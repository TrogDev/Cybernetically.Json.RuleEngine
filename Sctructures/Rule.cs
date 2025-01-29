using Cybernetically.Json.RuleEngine.Enums;

namespace Cybernetically.Json.RuleEngine.Sctructures;

public class Rule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public RuleType Type { get; set; } = RuleType.Once;
    public required List<string> Query { get; set; }
    public string? QueryValue { get; set; }
    public List<List<string>> ValueQueries { get; set; } = [];
    public required string Value { get; set; }
    public string? AddKey { get; set; } = null;
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        return ((Rule)obj).Id == Id;
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
