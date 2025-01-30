using Newtonsoft.Json.Linq;

namespace Cybernetically.Json.RuleEngine.Interfaces;

public interface ITypeProvider
{
    string GetTypeName(JTokenType type);
}
