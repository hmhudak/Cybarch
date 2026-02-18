using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.Models;

public class GameMember
{
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public GameMemberRole Role { get; set; } = GameMemberRole.Player;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}
