using Cybarch.Web.Services.Rulesets;

namespace Cybarch.Web.Services.Rulesets.Drd16;

public static class Drd16CatalogItems
{
    public static readonly IReadOnlyList<RulesetItem> All = new List<RulesetItem>
    {
        // Weapons
        new(
            Id: "weapon_short_sword",
            Name: "Short sword",
            Weight: 1.5m,
            Price: Drd16Currency.Price(25),
            Type: ItemType.Weapon,
            Description: "Jednoručný meč. Univerzálna zbraň na blízko.",
            Weapon: new WeaponStats(Strength: 2, Attack: 3, Defense: 1)
        ),
        new(
            Id: "weapon_dagger",
            Name: "Dagger",
            Weight: 0.5m,
            Price: Drd16Currency.Price(8),
            Type: ItemType.Weapon,
            Description: "Krátka čepeľ, ľahko skrytá. Vhodná na rýchle útoky.",
            Weapon: new WeaponStats(Strength: 1, Attack: 2, Defense: 0)
        ),
        new(
            Id: "weapon_long_sword",
            Name: "Long sword",
            Weight: 2.5m,
            Price: Drd16Currency.Price(40),
            Type: ItemType.Weapon,
            Description: "Dlhý jednoručný meč s dobrým dosahom.",
            Weapon: new WeaponStats(Strength: 3, Attack: 4, Defense: 1)
        ),

        // Armor
        new(
            Id: "armor_leather",
            Name: "Leather armor",
            Weight: 6.0m,
            Price: Drd16Currency.Price(30),
            Type: ItemType.Armor,
            Description: "Ľahké brnenie, komfortné na dlhé cestovanie.",
            Armor: new ArmorStats(Defense: 2)
        ),
        new(
            Id: "armor_chainmail",
            Name: "Chainmail",
            Weight: 12.0m,
            Price: Drd16Currency.Price(75),
            Type: ItemType.Armor,
            Description: "Zbroj z kvalitných krúžkov.",
            Armor: new ArmorStats(Defense: 4)
        ),

        // Ingredients
        new(
            Id: "ingr_healing_herb",
            Name: "Healing herb",
            Weight: 0.1m,
            Price: Drd16Currency.Price(5),
            Type: ItemType.Ingredient,
            Description: "Základná alchymistická prísada na liečivé masti a odvary."
        ),

        // Provisions
        new(
            Id: "prov_rations",
            Name: "Rations (1 day)",
            Weight: 1.0m,
            Price: Drd16Currency.Price(2),
            Type: ItemType.Provision,
            Description: "Proviant na jeden deň (sušené mäso, chlieb, voda)."
        ),
        new(
            Id: "rope",
            Name: "Rope",
            Weight: 3.0m,
            Price: Drd16Currency.Price(12, 0, 3), //12 zl, 0 st, 3 md
            Type: ItemType.Other,
            Description: "Pevné lano (cca 15m)."
        ),
    };
}
