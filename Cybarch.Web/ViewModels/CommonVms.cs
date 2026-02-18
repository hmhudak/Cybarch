using Cybarch.Web.Models;

namespace Cybarch.Web.ViewModels;

public record UserVm(string UserId, string DisplayName);

public record UniverseMemberVm(string UserId, string DisplayName, UniverseMemberRole Role);

public record GameListItemVm(Game Game, GameMemberRole? MyRole);
