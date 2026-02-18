using Cybarch.Web.Services.Rulesets;

namespace Cybarch.Web.Services.Rulesets.Drd16;

public static class Drd16CatalogSubclasses
{
    public static readonly IReadOnlyList<SubclassItem> All = new List<SubclassItem>
    {
        new("berserker", "Berserker", "warrior"),
        new("paladin", "Paladin", "warrior"),

        new("sniper", "Sniper", "ranger"),
        new("beastmaster", "Beastmaster", "ranger"),

        new("elementalist", "Elementalist", "mage"),
        new("necromancer", "Necromancer", "mage"),

        new("healer", "Healer", "cleric"),
        new("inquisitor", "Inquisitor", "cleric"),

        new("assassin", "Assassin", "thief"),
        new("shadow", "Shadow", "thief"),
    };
}
