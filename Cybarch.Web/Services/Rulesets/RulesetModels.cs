using System.Globalization;

namespace Cybarch.Web.Services.Rulesets;

public record CatalogItem(string Key, string Name);
public record SubclassItem(string Key, string Name, string ClassKey);

public record DefaultVitals(int MaxHp, int MaxMana);

// ---------- Money + Currency ----------

/// <summary>
/// Cena v "minor units" (1 minor = 0.01 major).
/// Pre DrD 1.6: minor = medený (md), major = zlatý (zl).
/// </summary>
public readonly record struct Money(int Minor)
{
    public decimal AsMajor => Minor / 100m;
    public bool IsWholeMajor => Minor % 100 == 0;
}

public record CurrencyDenomination(
    string Key,          // "zl", "st", "md" (rulesetovo)
    string Name,         // "Zlaté"
    string ShortName,    // "zl"
    int MinorValue,      // hodnota jednej mince v minor units (zl=100, st=10, md=1)
    decimal Weight       // váha jednej mince
);

public class CurrencyDefinition
{
    public string MajorDenominationKey { get; }
    public string ExchangeNote { get; }
    public IReadOnlyList<CurrencyDenomination> Denominations { get; }

    public CurrencyDefinition(string majorDenominationKey, string exchangeNote, IReadOnlyList<CurrencyDenomination> denominations)
    {
        MajorDenominationKey = majorDenominationKey;
        ExchangeNote = exchangeNote;
        Denominations = denominations;
    }

    public CurrencyDenomination? FindDenomination(string key)
        => Denominations.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Format ceny vždy v major (zl) na 2 desatinné miesta, ale bez .00 (12 -> "12", 12.03 -> "12.03").
    /// Používa bodku (invariant culture), aby to sedelo s príkladmi.
    /// </summary>
    public string FormatMajor(Money money)
    {
        if (money.IsWholeMajor)
            return (money.Minor / 100).ToString(CultureInfo.InvariantCulture);

        return money.AsMajor.ToString("0.00", CultureInfo.InvariantCulture);
    }
}

// ---------- Items ----------

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
    Money Price,
    ItemType Type,
    string Description,
    WeaponStats? Weapon = null,
    ArmorStats? Armor = null
);
