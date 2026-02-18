using Cybarch.Web.Services.Rulesets;

namespace Cybarch.Web.Services.Rulesets.Drd16;

public static class Drd16CatalogRaces
{
    public static readonly IReadOnlyList<CatalogItem> All = new List<CatalogItem>
    {
        new("human", "Human"),
        new("elf", "Elf"),
        new("dwarf", "Dwarf"),
        new("halfling", "Halfling"),
        new("orc", "Orc"),
    };
}
