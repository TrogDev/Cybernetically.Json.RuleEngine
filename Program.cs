using Cybernetically.Json.RuleEngine.Enums;
using Cybernetically.Json.RuleEngine.Sctructures;
using Newtonsoft.Json.Linq;

namespace Cybernetically.Json.RuleEngine;

public class Program
{
    public static void Main(string[] args)
    {
        JsonRuleEngine engine = new JsonRuleEngine();

        engine.AddRule(new Rule()
        {
            Query = ["marketplace_id"],
            Value = "$remove",
            Type = RuleType.Recursive
        });
        engine.AddRule(new Rule()
        {
            Query = ["$keysLength"],
            QueryValue = "1",
            ValueQueries = [["$root", "$*"]],
            Value = "$0",
            Type = RuleType.Recursive
        });

        JObject json = JObject.Parse(@"
            {
                'size': 'xxl',
                'colors': [
                    {
                        'value': 'red',
                        'marketplace_id': 'sad'
                    },
                    {
                        'value': 'blue'
                    }
                ]
            }
        ".Replace("'", "\""));

        List<Rule> negativeRules = engine.Process(json);

        Console.WriteLine(json.ToString());

        JsonRuleEngine negativeEngine = new JsonRuleEngine();

        foreach (Rule negativeRule in negativeRules)
        {
            negativeEngine.AddRule(negativeRule);
        }

        negativeEngine.Process(json);

        Console.WriteLine(json.ToString());
    }
}
