namespace Cybarch.Web.Services.Rulesets.Drd16;

using Cybarch.Web.Services.Rulesets;

public static class Drd16Currency
{
    public static readonly CurrencyDefinition Definition =
        new CurrencyDefinition(
            majorDenominationKey: "zl",
            exchangeNote: "Kurz: 10 st = 1 zlatá, 10 md = 1 st.",
            denominations: new List<CurrencyDenomination>
            {
                new("zl", "Zlaté", "zl", MinorValue: 100, Weight: 0.2m),
                new("st", "Strieborné", "st", MinorValue: 10, Weight: 0.2m),
                new("md", "Medené", "md", MinorValue: 1, Weight: 0.1m),
            }
        );

    /// <summary>
    /// Pomôcka na zapisovanie cien: zl/st/md -> Money(Minor)
    /// </summary>
    public static Money Price(int zl, int st = 0, int md = 0)
        => new Money(zl * 100 + st * 10 + md);
}
