using Newtonsoft.Json.Linq;
using Cybernetically.Json.RuleEngine.Sctructures;
using Cybernetically.Json.RuleEngine.Enums;

namespace Cybernetically.Json.RuleEngine;

public class JsonRuleEngine
{
    private const string enginePrefix = "$";
    private readonly List<Rule> rules = [];

    public void AddRule(Rule rule)
    {
        rules.Add(rule);
    }

    public void RemoveRule(Guid id)
    {
        rules.RemoveAt(rules.FindIndex(e => e.Id == id));
    }

    public List<Rule> Process(JObject json)
    {
        List<Rule> negative = [];

        for (int i = 0; i < rules.Count;)
        {
            int changes = 0;
            Rule rule = rules[i];

            foreach (JToken token in IterateJson(json))
            {
                if (IsQueryValid(token, rule))
                {
                    HandleRule(token, rule, negative);
                    changes++;
                    break;
                }
            }

            if (rule.Type != RuleType.Recursive || changes == 0)
            {
                i++;
            }
        }

        negative.Reverse();
        return negative;
    }

    private static IEnumerable<JToken> IterateJson(JToken json)
    {
        yield return json;

        if (json is JObject jsonObject)
        {
            foreach (JProperty property in jsonObject.Properties())
            {
                foreach (JToken childToken in IterateJson(property.Value))
                {
                    yield return childToken;
                }
            }
        }
        if (json is JArray jsonArray)
        {
            foreach (JToken token in jsonArray.Children())
            {
                foreach (JToken childToken in IterateJson(token))
                {
                    yield return childToken;
                }
            }
        }
    }

    private bool IsQueryValid(JToken token, Rule rule)
    {
        return IsQueryValueValid(token, rule) && IsQueryPathValid(token, rule.Query);
    }

    private bool IsQueryValueValid(JToken token, Rule rule)
    {
        if (rule.QueryValue == null)
        {
            return true;
        }

        string variable = rule.Query.Last();

        if (variable == $"{enginePrefix}keysLength")
        {
            if (token is not JObject jsonObject)
            {
                return false;
            }
            return jsonObject.Count.ToString() == rule.QueryValue;
        }
        else if (variable == $"{enginePrefix}length")
        {
            if (token is not JArray arrayObject)
            {
                return false;
            }
            return arrayObject.Count.ToString() == rule.QueryValue;
        }
        else
        {
            if (token is JObject && token is JArray)
            {
                return false;
            }
            return token.ToString() == rule.QueryValue;
        }
    }

    private bool IsQueryPathValid(JToken token, List<string> query)
    {
        int skip = 0;
        string last = query.Last();

        if (last == $"{enginePrefix}keysLength" || last == $"{enginePrefix}length")
        {
            skip++;
        }

        JToken currToken = token;

        for (int i = query.Count - 1 - skip; i >= 0; i--)
        {
            string name = query[i];

            if (currToken.Parent == null)
            {
                return name == $"{enginePrefix}root";
            }
            else if (currToken.Parent is JProperty propertyParent)
            {
                if (name != $"{enginePrefix}*" && propertyParent.Name != name)
                {
                    return false;
                }
                currToken = currToken.Parent;
            }
            else if (currToken.Parent is JArray arrayParent)
            {
                if ($"[{arrayParent.IndexOf(currToken)}]" != name)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            currToken = currToken.Parent!;
        }

        return true;
    }

    private void HandleRule(JToken token, Rule rule, List<Rule> negative)
    {
        string value = rule.Value;

        if (value == $"{enginePrefix}remove")
        {
            HandleRemove(token, negative);
            return;
        }

        for (int i = 0; i < rule.ValueQueries.Count; i++)
        {
            List<string> query = rule.ValueQueries[i];
            JToken queryRoot = token.DeepClone();

            foreach (JToken queryToken in IterateJson(queryRoot))
            {
                if (IsQueryPathValid(queryToken, query))
                {
                    value = value.Replace($"{enginePrefix}{i}", GetRawValue(queryToken));
                    break;
                }
            }
        }

        List<string> negativeQuery = GetCurrentQuery(token);

        if (rule.AddKey == null)
        {
            if (token.Parent is JProperty property)
            {
                negative.Add(new Rule()
                {
                    Query = negativeQuery,
                    Value = GetRawValue(property.Value)
                });
                property.Value = JToken.Parse(value);
            }
            if (token.Parent is JArray array)
            {
                negative.Add(new Rule()
                {
                    Query = negativeQuery,
                    Value = GetRawValue(token)
                });
                array[array.IndexOf(token)] = JToken.Parse(value);
            }
        }
        else
        {
            if (token is JObject property)
            {
                negativeQuery.Add(rule.AddKey);
                negative.Add(new Rule()
                {
                    Query = negativeQuery,
                    Value = "$remove"
                });
                property.Add(new JProperty(rule.AddKey, JToken.Parse(value)));
            }
            if (token is JArray array)
            {
                negativeQuery.Add(Math.Min(int.Parse(rule.AddKey), array.Count).ToString());
                negative.Add(new Rule()
                {
                    Query = negativeQuery,
                    Value = "$remove"
                });
                array.Insert(Math.Min(int.Parse(rule.AddKey), array.Count), JToken.Parse(value));
            }
        }
    }

    private void HandleRemove(JToken token, List<Rule> negative)
    {
        List<string> query = GetCurrentQuery(token);

        if (token.Parent is JProperty property)
        {
            negative.Add(new Rule()
            {
                Query = query.GetRange(0, query.Count - 1),
                ValueQueries = [],
                Value = GetRawValue(property.Value),
                AddKey = query.Last()
            });
            property.Remove();
        }
        if (token.Parent is JArray array)
        {
            string val = token.ToString();
            negative.Add(new Rule()
            {
                Query = query.GetRange(0, query.Count - 1),
                ValueQueries = [],
                Value = GetRawValue(token),
                AddKey = query.Last().Replace("[", "").Replace("]", "")
            });
            array.Remove(token);
        }
    }

    private List<string> GetCurrentQuery(JToken token)
    {
        List<string> query = [];

        JToken currToken = token;

        while (currToken.Parent != null)
        {
            if (currToken.Parent is JProperty property)
            {
                query.Insert(0, property.Name);
                currToken = currToken.Parent;
            }
            if (currToken.Parent is JArray array)
            {
                query.Insert(0, $"[{array.IndexOf(currToken)}]");
            }

            currToken = currToken.Parent!;
        }

        query.Insert(0, $"{enginePrefix}root");

        return query;
    }

    private string GetRawValue(JToken token)
    {
        string value = token.ToString();

        if (token.Type == JTokenType.String)
        {
            value = $"\"{value}\"";
        }

        return value;
    }
}
