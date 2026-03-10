using Cybarch.Web.Services.Rulesets;

namespace Cybarch.Web.Services.Rulesets.Drd16;

public static class Drd16CatalogClasses
{
    public static readonly IReadOnlyList<CatalogItem> All = new List<CatalogItem>
    {
        new("valecnik",   "Válečník"),
        new("hranicar",   "Hraničář"),
        new("alchymista", "Alchymista"),
        new("kouzelnik",  "Kouzelník"),
        new("zlodej",     "Zloděj"),
    };
}
