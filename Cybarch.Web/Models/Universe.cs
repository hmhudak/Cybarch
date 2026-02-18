using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.Models;

public class Universe
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string CreatorUserId { get; set; } = string.Empty;

    /// <summary>
    /// Jednoduché join heslo (MVP). V produkcii by to malo byť hashované.
    /// </summary>
    [Required, MaxLength(200)]
    public string JoinPassword { get; set; } = string.Empty;

    [Required]
    public string RulesetKey { get; set; } = "drd16";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<UniverseMember> Members { get; set; } = new();
    public List<Game> Games { get; set; } = new();
}
