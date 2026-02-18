using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.Models;

public class Character
{
    public int Id { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Race { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
    public int Experience { get; set; }

    [Required, MaxLength(100)]
    public string ClassName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SubclassName { get; set; }

    // Attributes
    public int Str { get; set; }
    public int Dex { get; set; }
    public int Con { get; set; }
    public int Int { get; set; }
    public int Wis { get; set; }
    public int Cha { get; set; }

    public string Notes { get; set; } = string.Empty;

    // Vitals
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int MaxMana { get; set; }
    public int CurrentMana { get; set; }

    public List<CharacterAssignment> Assignments { get; set; } = new();
    public List<CharacterItem> Items { get; set; } = new();
}
