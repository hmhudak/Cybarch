using Cybarch.Web.Services.Rulesets.Drd16;

namespace Cybarch.Web.Services.Rulesets;

public class Drd16Ruleset : IRulesetDefinition
{
    public string Key => "drd16";
    public string DisplayName => "Dračí Doupě 1.6";

    public IReadOnlyList<CatalogItem> Races { get; } = Drd16CatalogRaces.All;
    public IReadOnlyList<CatalogItem> Classes { get; } = Drd16CatalogClasses.All;
    public IReadOnlyList<SubclassItem> Subclasses { get; } = Drd16CatalogSubclasses.All;
    public IReadOnlyList<RulesetItem> Items { get; } = Drd16CatalogItems.All;

    public bool IsValidRace(string? raceKey)
        => !string.IsNullOrWhiteSpace(raceKey) && Races.Any(r => r.Key == raceKey);

    public bool IsValidClass(string? classKey)
        => !string.IsNullOrWhiteSpace(classKey) && Classes.Any(c => c.Key == classKey);

    public bool IsValidSubclass(string? classKey, string? subclassKey)
    {
        if (string.IsNullOrWhiteSpace(subclassKey)) return true;
        if (string.IsNullOrWhiteSpace(classKey)) return false;
        return Subclasses.Any(s => s.Key == subclassKey && s.ClassKey == classKey);
    }

    public bool IsValidItem(string? itemId)
        => !string.IsNullOrWhiteSpace(itemId) && Items.Any(i => string.Equals(i.Id, itemId, StringComparison.OrdinalIgnoreCase));

    public RulesetItem? FindItem(string itemId)
        => Items.FirstOrDefault(i => string.Equals(i.Id, itemId, StringComparison.OrdinalIgnoreCase));

    public DefaultVitals GetDefaultVitals(string classKey, int level)
    {
        level = Math.Max(level, 1);

        var baseHp = classKey switch
        {
            "warrior" => 20,
            "ranger" => 16,
            "cleric" => 16,
            "thief" => 14,
            "mage" => 12,
            _ => 14
        };

        var baseMana = classKey switch
        {
            "mage" => 30,
            "cleric" => 20,
            _ => 10
        };

        return new DefaultVitals(baseHp + (level - 1) * 3, baseMana + (level - 1) * 5);
    }
}
