using Cybarch.Web.Services.Rulesets;

namespace Cybarch.Web.Services.Rulesets.Drd16;

public static class Drd16CatalogClasses
{
    public static readonly IReadOnlyList<CatalogItem> All = new List<CatalogItem>
    {
        new("warrior", "Warrior"),
        new("ranger", "Ranger"),
        new("mage", "Mage"),
        new("cleric", "Cleric"),
        new("thief", "Thief"),
    };
}
