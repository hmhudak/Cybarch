using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.Models;

public class Game
{
    public int Id { get; set; }

    public int UniverseId { get; set; }
    public Universe Universe { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string CampaignText { get; set; } = string.Empty;

    [Required]
    public string RulesetKey { get; set; } = "drd16";

    public List<GameMember> Members { get; set; } = new();
    public List<Character> Characters { get; set; } = new();
}
