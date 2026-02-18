using Cybarch.Web.Models;

namespace Cybarch.Web.ViewModels;

public class CharacterListItemVm
{
    public int CharacterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }

    public string ClassDisplay { get; set; } = string.Empty;
    public string SubclassDisplay { get; set; } = string.Empty;

    public List<UserVm> AssignedPlayers { get; set; } = new();
    public HashSet<string> AssignedUserIds { get; set; } = new();

    public bool CanOpenDetails { get; set; }
}

public class GameDetailsVm
{
    public Game Game { get; set; } = null!;
    public Universe Universe { get; set; } = null!;

    public string RulesetDisplayName { get; set; } = string.Empty;

    public string? NonGameMemberMessage { get; set; }

    public bool IsDm { get; set; }

    public string CurrentUserId { get; set; } = string.Empty;
    public bool IsUniverseCreator { get; set; }
    public bool IsGodMode { get; set; }
    public int DmCount { get; set; }

    public string CampaignEditText { get; set; } = string.Empty;

    public List<UserVm> Dms { get; set; } = new();
    public List<UserVm> Players { get; set; } = new();
    public List<UserVm> NotPlayers { get; set; } = new();

    public List<CharacterListItemVm> Characters { get; set; } = new();

    public CharacterCreateVm CreateCharacter { get; set; } = new();
    public bool ShowCreateCharacterModal { get; set; }

    public string? TempError { get; set; }
}
