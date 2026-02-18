namespace Cybarch.Web.Services.Rulesets;

public record CatalogItem(string Key, string Name);
public record SubclassItem(string Key, string Name, string ClassKey);

public record DefaultVitals(int MaxHp, int MaxMana);

public enum ItemType
{
    Weapon,
    Armor,
    Ingredient,
    Provision,
    Other
}

public record WeaponStats(int Strength, int Attack, int Defense);
public record ArmorStats(int Defense);

public record RulesetItem(
    string Id,
    string Name,
    decimal Weight,
    int Price,
    ItemType Type,
    string Description,
    WeaponStats? Weapon = null,
    ArmorStats? Armor = null
);
