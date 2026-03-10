using Cybarch.Web.Models;
using Cybarch.Web.Services.Rulesets;
using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.ViewModels;

public class CharacterCreateVm
{
    public int GameId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Race { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Level { get; set; } = 1;

    [Required]
    public string ClassName { get; set; } = string.Empty;

    public string? SubclassName { get; set; }

    // dropdown data
    public List<CatalogItem> RaceOptions { get; set; } = new();
    public List<CatalogItem> ClassOptions { get; set; } = new();
    public List<SubclassItem> SubclassOptions { get; set; } = new();
    public int Str { get; set; }
    public int Int { get; set; }
    public int End { get; set; }
    public int Dex { get; set; }
    public int Char { get; set; }
}

public class CharacterEditVm
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Race { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Level { get; set; } = 1;

    public int Experience { get; set; }

    [Required]
    public string ClassName { get; set; } = string.Empty;

    public string? SubclassName { get; set; }

    public int Str { get; set; }
    public int Dex { get; set; }
    public int Con { get; set; }
    public int Int { get; set; }
    public int Cha { get; set; }

    public string Notes { get; set; } = string.Empty;

    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int MaxMana { get; set; }
    public int CurrentMana { get; set; }

    public List<CatalogItem> RaceOptions { get; set; } = new();
    public List<CatalogItem> ClassOptions { get; set; } = new();
    public List<SubclassItem> SubclassOptions { get; set; } = new();
}

public class CharacterDetailsVm
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public bool IsDm { get; set; }
    public bool IsAssignedPlayer { get; set; }

    public string RulesetDisplayName { get; set; } = string.Empty;
    public string RulesetKey { get; set; } = string.Empty;

    public Cybarch.Web.Models.Character Character { get; set; } = null!;

    public CharacterEditVm EditForm { get; set; } = new();
    public bool ShowEditModal { get; set; }

    // Inventory + nosnosť
    public int CarryCapacity { get; set; }
    public decimal TotalWeight { get; set; }
    public bool IsOverCapacity { get; set; }
    public bool CanManageInventory { get; set; }
    public List<InventoryItemVm> InventoryItems { get; set; } = new();
    public List<ItemPickerItemVm> AvailableItems { get; set; } = new();

    // Currency
    public List<CharacterCurrencyVm> CurrencyRows { get; set; } = new();
    public string CurrencyExchangeNote { get; set; } = string.Empty;
    public decimal CurrencyWeight { get; set; }
    public bool CanManageCurrency { get; set; }

    public string? ErrorMessage { get; set; }
}

public class InventoryItemVm
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitWeight { get; set; }
    public decimal TotalWeight => UnitWeight * Quantity;
    public string Price { get; set; } = string.Empty; // napr. "12.03 zl"

    public string? StatsLine { get; set; }
}

public class ItemPickerItemVm
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Price { get; set; } = "";
    public string? StatsLine { get; set; }
}

public class CharacterCurrencyVm
{
    public string DenominationKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;

    public int Amount { get; set; }
    public decimal CoinWeight { get; set; }
    public decimal TotalWeight => CoinWeight * Amount;
}
