namespace Cybarch.Web.Services.Rulesets;

public class RulesetRegistry
{
    private readonly Dictionary<string, IRulesetDefinition> _rulesets;

    public RulesetRegistry(IEnumerable<IRulesetDefinition> rulesets)
    {
        _rulesets = rulesets.ToDictionary(r => r.Key, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IRulesetDefinition> All => _rulesets.Values.OrderBy(r => r.DisplayName).ToList();

    public IRulesetDefinition GetByKey(string key)
    {
        if (_rulesets.TryGetValue(key, out var r)) return r;
        // fallback
        return _rulesets.Values.First();
    }
}
