using Cybarch.Web.Services.Rulesets;

namespace Cybarch.Web.Services.Rulesets.Drd16;

public static class Drd16CatalogSubclasses
{
    public static readonly IReadOnlyList<SubclassItem> All = new List<SubclassItem>
    {
        new("bojovnik", "Bojovník", "valecnik"),
        new("sermir", "Šermíř", "valecnik"),

        new("druid", "Druid", "hranicar"),
        new("chodec", "Chodec", "hranicar"),

        new("theurg", "Theurg", "alchymista"),
        new("pyrofor", "Pyrofor", "mage"),

        new("mag", "Mág", "kouzelnik"),
        new("carodej", "Čaroděj", "kouzelnik"),

        new("lupic", "Lupič", "zlodej"),
        new("sicco", "Sicco", "zlodej"),
    };
}
