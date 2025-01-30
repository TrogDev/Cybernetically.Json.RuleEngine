using System.Text.Json;
using Cybernetically.Json.RuleEngine.Enums;
using Cybernetically.Json.RuleEngine.Interfaces;
using Cybernetically.Json.RuleEngine.Sctructures;
using Cybernetically.Json.RuleEngine.Services;
using Newtonsoft.Json.Linq;

namespace Cybernetically.Json.RuleEngine;

public class JsonRuleEngine
{
    private const string enginePrefix = "$";
    private readonly List<Rule> rules = [];
    public ITypeProvider TypeProvider { get; set; } = new DefaultTypeProvider();

    public void AddRule(Rule rule)
    {
        rules.Add(rule);
    }

    public void RemoveRule(Guid id)
    {
        rules.RemoveAt(rules.FindIndex(e => e.Id == id));
    }

    public ProcessResponse Process(JToken json)
    {
        json = WrapToRoot(json);

        List<Rule> negative = [];

        for (int i = 0; i < rules.Count; )
        {
            int changes = 0;
            Rule rule = rules[i];

            if (rule.Type == RuleType.All)
            {
                List<JToken> validTokens = IterateJson(json)
                    .Where(t => IsQueryValid(t, rule.Query))
                    .ToList();

                foreach (JToken token in validTokens)
                {
                    HandleRule(token, rule, negative);
                    changes++;
                }
            }
            else
            {
                foreach (JToken token in IterateJson(json))
                {
                    if (IsQueryValid(token, rule.Query))
                    {
                        HandleRule(token, rule, negative);
                        changes++;
                        break;
                    }
                }
            }

            if (rule.Type != RuleType.Recursive || changes == 0)
            {
                i++;
            }
        }

        negative.Reverse();
        return new ProcessResponse()
        {
            NegativeRules = negative,
            Result = json[$"{enginePrefix}root"]!
        };
    }

    private static IEnumerable<JToken> IterateJson(JToken json)
    {
        if (json is JObject jsonObject)
        {
            if (!jsonObject.ContainsKey($"{enginePrefix}root"))
            {
                yield return json;
            }

            foreach (JProperty property in jsonObject.Properties())
            {
                foreach (JToken childToken in IterateJson(property.Value))
                {
                    yield return childToken;
                }
            }
        }
        else if (json is JArray jsonArray)
        {
            yield return json;

            foreach (JToken token in jsonArray.Children())
            {
                foreach (JToken childToken in IterateJson(token))
                {
                    yield return childToken;
                }
            }
        }
        else
        {
            yield return json;
        }
    }

    private bool IsQueryValid(JToken token, List<Sensor> query)
    {
        return query.All(s =>
            (IsQueryValueValid(token, s) && IsQueryPathValid(token, s.Search))
                ? !s.IsNegative
                : s.IsNegative
        );
    }

    private bool IsQueryValueValid(JToken token, Sensor sensor)
    {
        if (sensor.Value == null)
        {
            return true;
        }

        string variable = sensor.Search.Last();

        if (variable == $"{enginePrefix}keysLength")
        {
            if (token is not JObject jsonObject)
            {
                return false;
            }
            return jsonObject.Count.ToString() == sensor.Value;
        }
        else if (variable == $"{enginePrefix}length")
        {
            if (token is not JArray arrayObject)
            {
                return false;
            }
            return arrayObject.Count.ToString() == sensor.Value;
        }
        else if (variable == $"{enginePrefix}hasKey")
        {
            if (token is not JObject jsonObject)
            {
                return false;
            }
            return jsonObject.Properties().Any(e => e.Name == sensor.Value);
        }
        else
        {
            if (token is JObject && token is JArray)
            {
                return false;
            }
            return GetRawValue(token) == sensor.Value;
        }
    }

    private bool IsQueryPathValid(JToken token, List<string> query)
    {
        int skip = 0;
        string last = query.Last();

        if (last == $"{enginePrefix}keysLength")
        {
            if (token is not JObject)
            {
                return false;
            }
            skip++;
        }
        else if (last == $"{enginePrefix}length")
        {
            if (token is not JArray)
            {
                return false;
            }
            skip++;
        }
        else if (last == $"{enginePrefix}hasKey")
        {
            if (token is not JObject)
            {
                return false;
            }
            skip++;
        }

        JToken currToken = token;

        for (int i = query.Count - 1 - skip; i >= 0; i--)
        {
            string name = query[i];

            if (currToken.Parent is JProperty propertyParent)
            {
                if (name != $"{enginePrefix}*" && propertyParent.Name != name)
                {
                    return false;
                }
                currToken = currToken.Parent;
            }
            else if (currToken.Parent is JArray arrayParent)
            {
                if (name != $"{enginePrefix}*" && $"[{arrayParent.IndexOf(currToken)}]" != name)
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
        if (rule.Value == $"{enginePrefix}remove")
        {
            HandleRemove(token, negative);
        }
        else if (rule.Value == $"{enginePrefix}toArray")
        {
            HandleToArray(token, negative);
        }
        else if (rule.Value == $"{enginePrefix}flatArray")
        {
            HandleFlatArray(token, negative);
        }
        else
        {
            HandleRewrite(token, rule, negative);
        }
    }

    private void HandleRemove(JToken token, List<Rule> negative)
    {
        List<string> negativeSearch = GetCurrentSearch(token);

        if (token.Parent is JProperty property)
        {
            negative.Add(
                new Rule()
                {
                    Query =
                    [
                        new Sensor()
                        {
                            Search = negativeSearch.GetRange(0, negativeSearch.Count - 1)
                        }
                    ],
                    Value = GetRawValue(property.Value),
                    AddKey = negativeSearch.Last()
                }
            );
            property.Remove();
        }
        else if (token.Parent is JArray array)
        {
            negative.Add(
                new Rule()
                {
                    Query =
                    [
                        new Sensor()
                        {
                            Search = negativeSearch.GetRange(0, negativeSearch.Count - 1)
                        }
                    ],
                    ValueQueries = [],
                    Value = GetRawValue(token),
                    AddKey = negativeSearch.Last().Replace("[", "").Replace("]", "")
                }
            );
            array.Remove(token);
        }
    }

    private void HandleToArray(JToken token, List<Rule> negative)
    {
        if (token is not JObject obj)
        {
            return;
        }

        JArray array = new JArray();

        foreach (JProperty property in obj.Properties())
        {
            array.Add(property.Value);
        }

        if (token.Parent is JProperty parentProperty)
        {
            parentProperty.Value = array;
        }
        else if (token.Parent is JArray parentArray)
        {
            parentArray[parentArray.IndexOf(token)] = array;
        }
    }

    private void HandleFlatArray(JToken token, List<Rule> negative)
    {
        if (token is not JArray array)
        {
            return;
        }
        if (token.Parent is not JArray parentArray)
        {
            return;
        }

        int i = parentArray.IndexOf(token);
        token.Remove();

        foreach (JToken item in array.Children().Reverse())
        {
            parentArray.Insert(i, item);
        }
    }

    private void HandleRewrite(JToken token, Rule rule, List<Rule> negative)
    {
        string value = rule.Value;

        for (int i = 0; i < rule.ValueQueries.Count; i++)
        {
            List<Sensor> query = rule.ValueQueries[i];
            JToken queryRoot = WrapToRoot(token);

            foreach (JToken queryToken in IterateJson(queryRoot))
            {
                if (IsQueryValid(queryToken, query))
                {
                    value = value.Replace($"{enginePrefix}{i}", GetRawValue(queryToken));
                    break;
                }
            }
        }

        value = value.Replace($"{enginePrefix}key", GetCurrentKey(token));
        value = value.Replace($"{enginePrefix}uuid", Guid.NewGuid().ToString());
        value = value.Replace($"{enginePrefix}type", TypeProvider.GetTypeName(token.Type));
        string path = string.Join("/", GetCurrentSearch(token).Skip(1).SkipLast(1));
        if (path.Length > 0)
        {
            path += "/";
        }
        value = value.Replace($"{enginePrefix}path", path);

        List<string> negativeSearch = GetCurrentSearch(token);

        if (rule.AddKey == null)
        {
            if (token.Parent is JProperty property)
            {
                negative.Add(
                    new Rule()
                    {
                        Query = [new Sensor() { Search = negativeSearch }],
                        Value = GetRawValue(property.Value)
                    }
                );
                property.Value = JToken.Parse(value);
            }
            else if (token.Parent is JArray array)
            {
                negative.Add(
                    new Rule()
                    {
                        Query = [new Sensor() { Search = negativeSearch }],
                        Value = GetRawValue(token)
                    }
                );
                array[array.IndexOf(token)] = JToken.Parse(value);
            }
        }
        else
        {
            if (token is JObject obj)
            {
                negativeSearch.Add(rule.AddKey);
                negative.Add(
                    new Rule()
                    {
                        Query = [new Sensor() { Search = negativeSearch }],
                        Value = $"{enginePrefix}remove"
                    }
                );
                obj.Add(new JProperty(rule.AddKey, JToken.Parse(value)));
            }
            else if (token is JArray array)
            {
                negativeSearch.Add(Math.Min(int.Parse(rule.AddKey), array.Count).ToString());
                negative.Add(
                    new Rule()
                    {
                        Query = [new Sensor() { Search = negativeSearch }],
                        Value = $"{enginePrefix}remove"
                    }
                );
                array.Insert(Math.Min(int.Parse(rule.AddKey), array.Count), JToken.Parse(value));
            }
        }
    }

    private List<string> GetCurrentSearch(JToken token)
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

        return query;
    }

    private string GetRawValue(JToken token)
    {
        if (token.Type == JTokenType.Null)
        {
            return "null";
        }

        string value = token.ToString();

        if (token.Type == JTokenType.String || token.Type == JTokenType.Date)
        {
            value = JsonSerializer.Serialize(value);
        }
        else if (token.Type == JTokenType.Boolean)
        {
            value = value.ToLower();
        }

        return value;
    }

    private string GetCurrentKey(JToken token)
    {
        if (token.Parent is JProperty property)
        {
            return property.Name;
        }
        else if (token.Parent is JArray array)
        {
            return array.IndexOf(token).ToString();
        }

        return "";
    }

    private JToken WrapToRoot(JToken token)
    {
        if (token is JObject obj)
        {
            if (obj.ContainsKey($"{enginePrefix}root"))
            {
                return obj.DeepClone();
            }
        }

        JProperty root = new JProperty($"{enginePrefix}root", token.DeepClone());
        return new JObject(root);
    }
}
