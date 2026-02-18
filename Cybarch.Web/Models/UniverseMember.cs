using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.Models;

public class UniverseMember
{
    public int UniverseId { get; set; }
    public Universe Universe { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public UniverseMemberRole Role { get; set; } = UniverseMemberRole.Member;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}
