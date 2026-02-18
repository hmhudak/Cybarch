namespace Cybarch.Web.Services.Rulesets;

/// <summary>
/// "Kontajner" pre pravidlá hry.
///
/// Prečo je to dobré:
/// - Controller nerieši DrD/DnD špecifiká.
/// - Neskôr vieme pridať nový ruleset (napr. DnD 5E) bez prepisovania UI logiky.
/// </summary>
public interface IRulesetDefinition
{
    string Key { get; }
    string DisplayName { get; }

    IReadOnlyList<CatalogItem> Races { get; }
    IReadOnlyList<CatalogItem> Classes { get; }
    IReadOnlyList<SubclassItem> Subclasses { get; }

    bool IsValidRace(string? raceKey);
    bool IsValidClass(string? classKey);
    bool IsValidSubclass(string? classKey, string? subclassKey);

    DefaultVitals GetDefaultVitals(string classKey, int level);

    IReadOnlyList<RulesetItem> Items { get; }

    bool IsValidItem(string? itemId);
    RulesetItem? FindItem(string itemId);
}
