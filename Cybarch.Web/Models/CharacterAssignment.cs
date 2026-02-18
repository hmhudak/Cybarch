using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.Models;

public class CharacterAssignment
{
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
