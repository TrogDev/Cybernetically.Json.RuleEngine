using Cybernetically.Json.RuleEngine.Interfaces;
using Newtonsoft.Json.Linq;

namespace Cybernetically.Json.RuleEngine.Services;

public class DefaultTypeProvider : ITypeProvider
{
    public string GetTypeName(JTokenType type)
    {
        return type switch
        {
            JTokenType.Integer => "number",
            JTokenType.Float => "number",
            JTokenType.Boolean => "boolean",
            _ => "string",
        };
    }
}
