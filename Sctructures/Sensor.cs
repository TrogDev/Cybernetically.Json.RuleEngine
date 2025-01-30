namespace Cybernetically.Json.RuleEngine.Sctructures;

public class Sensor
{
    public required List<string> Search { get; set; }
    public string? Value { get; set; }
    public bool IsNegative { get; set; } = false;
}
