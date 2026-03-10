using Cybarch.Web.Services.Rulesets;

namespace Cybarch.Web.Services.Rulesets.Drd16;

public static class Drd16CatalogRaces
{
    public static readonly IReadOnlyList<CatalogItem> All = new List<CatalogItem>
    { 
        new("clovek",   "Člověk"),
        new("elf",      "Elf"),
        new("trpaslik", "Trpaslík"),
        new("kuduk",    "Kuduk"),
        new("kroll",    "Kroll"),
        new("hobit",    "Hobit"),
        new("barbar",   "Barbar"),
    };
}
