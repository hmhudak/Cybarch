using System.ComponentModel.DataAnnotations;
using Cybarch.Web.Models;

namespace Cybarch.Web.ViewModels;

public class UniverseIndexVm
{
    public List<Universe> Universes { get; set; } = new();
}

public class UniverseCreateVm
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string RulesetKey { get; set; } = "drd16";

    public string? JoinPassword { get; set; }

    public List<(string Key, string Name)> RulesetOptions { get; set; } = new();
}

public class UniverseJoinVm
{
    [Required]
    public int UniverseId { get; set; }

    [Required]
    public string JoinPassword { get; set; } = string.Empty;
}

public class GameCreateVm
{
    public int UniverseId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}

public class UniverseDetailsVm
{
    public Universe Universe { get; set; } = null!;
    public UniverseMemberRole MemberRole { get; set; }
    public string RulesetDisplayName { get; set; } = string.Empty;

    public List<GameListItemVm> Games { get; set; } = new();
    public List<UniverseMemberVm> Members { get; set; } = new();

    public GameCreateVm CreateGame { get; set; } = new();
    public bool ShowCreateGameModal { get; set; }
}
