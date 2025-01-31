namespace Cybernetically.Json.RuleEngine.Sctructures;

public class Sensor
{
    public required List<string> Path { get; set; }
    public string? Value { get; set; }
    public bool IsNegative { get; set; } = false;
}
