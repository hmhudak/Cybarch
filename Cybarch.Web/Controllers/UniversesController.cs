using System.Security.Claims;
using Cybarch.Web.Data;
using Cybarch.Web.Models;
using Cybarch.Web.Services;
using Cybarch.Web.Services.Rulesets;
using Cybarch.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cybarch.Web.Controllers;

[Authorize]
public class UniversesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UniverseAccessService _universeAccess;
    private readonly RulesetRegistry _rulesets;

    public UniversesController(ApplicationDbContext db, UniverseAccessService universeAccess, RulesetRegistry rulesets)
    {
        _db = db;
        _universeAccess = universeAccess;
        _rulesets = rulesets;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var universeIds = await _db.UniverseMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.UniverseId)
            .ToListAsync();

        var universes = await _db.Universes
            .Where(u => universeIds.Contains(u.Id))
            .OrderBy(u => u.Name)
            .AsNoTracking()
            .ToListAsync();

        return View(new UniverseIndexVm { Universes = universes });
    }

    public IActionResult Create()
    {
        var vm = new UniverseCreateVm
        {
            RulesetOptions = _rulesets.All.Select(r => (r.Key, r.DisplayName)).ToList(),
            RulesetKey = _rulesets.All.First().Key
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UniverseCreateVm vm)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        vm.RulesetOptions = _rulesets.All.Select(r => (r.Key, r.DisplayName)).ToList();

        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), "Name je povinné.");

        if (!_rulesets.All.Any(r => string.Equals(r.Key, vm.RulesetKey, StringComparison.OrdinalIgnoreCase)))
            ModelState.AddModelError(nameof(vm.RulesetKey), "Neplatný ruleset.");

        if (!ModelState.IsValid)
            return View(vm);

        var joinPass = string.IsNullOrWhiteSpace(vm.JoinPassword)
            ? JoinPasswordGenerator.Generate()
            : vm.JoinPassword.Trim();

        var universe = new Universe
        {
            Name = vm.Name.Trim(),
            CreatorUserId = userId,
            JoinPassword = joinPass,
            CreatedAtUtc = DateTime.UtcNow,
            RulesetKey = vm.RulesetKey
        };

        _db.Universes.Add(universe);
        await _db.SaveChangesAsync();

        _db.UniverseMembers.Add(new UniverseMember
        {
            UniverseId = universe.Id,
            UserId = userId,
            Role = UniverseMemberRole.Creator,
            JoinedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = universe.Id });
    }

    public IActionResult Join() => View(new UniverseJoinVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(UniverseJoinVm vm)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!ModelState.IsValid)
            return View(vm);

        var universe = await _db.Universes.FirstOrDefaultAsync(u => u.Id == vm.UniverseId);
        if (universe is null)
        {
            ModelState.AddModelError(nameof(vm.UniverseId), "Universe neexistuje.");
            return View(vm);
        }

        if (!string.Equals(universe.JoinPassword, vm.JoinPassword?.Trim() ?? string.Empty, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(vm.JoinPassword), "Nesprávne join password.");
            return View(vm);
        }

        var already = await _universeAccess.IsMemberAsync(universe.Id, userId);
        if (!already)
        {
            _db.UniverseMembers.Add(new UniverseMember
            {
                UniverseId = universe.Id,
                UserId = userId,
                Role = UniverseMemberRole.Member,
                JoinedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = universe.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!await _universeAccess.IsMemberAsync(id, userId))
            return Forbid();

        var vm = await BuildDetailsVmAsync(id, userId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGame([Bind(Prefix = "CreateGame")] GameCreateVm form)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var role = await _universeAccess.GetRoleAsync(form.UniverseId, userId);
        if (role != UniverseMemberRole.Creator)
            return Forbid();

        if (string.IsNullOrWhiteSpace(form.Name))
            ModelState.AddModelError($"CreateGame.{nameof(form.Name)}", "Názov hry je povinný.");

        if (!ModelState.IsValid)
        {
            var vm = await BuildDetailsVmAsync(form.UniverseId, userId, showCreateGameModal: true, createGameOverride: form);
            return View("Details", vm);
        }

        var universe = await _db.Universes.FirstOrDefaultAsync(u => u.Id == form.UniverseId);
        if (universe is null) return NotFound();

        var game = new Game
        {
            UniverseId = universe.Id,
            Name = form.Name.Trim(),
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            RulesetKey = universe.RulesetKey,
            CampaignText = string.Empty
        };

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        // tvorca hry je defaultne DM
        _db.GameMembers.Add(new GameMember
        {
            GameId = game.Id,
            UserId = userId,
            Role = GameMemberRole.DM,
            JoinedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Games", new { id = game.Id });
    }

    private async Task<UniverseDetailsVm> BuildDetailsVmAsync(int universeId, string currentUserId, bool showCreateGameModal = false, GameCreateVm? createGameOverride = null)
    {
        var universe = await _db.Universes
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == universeId);

        if (universe is null)
            throw new InvalidOperationException("Universe not found.");

        var role = await _universeAccess.GetRoleAsync(universeId, currentUserId) ?? UniverseMemberRole.Member;

        var games = await _db.Games
            .Where(g => g.UniverseId == universeId)
            .OrderBy(g => g.Name)
            .AsNoTracking()
            .ToListAsync();

        var gameIds = games.Select(g => g.Id).ToList();
        var myRoles = await _db.GameMembers
            .Where(m => m.UserId == currentUserId && gameIds.Contains(m.GameId))
            .ToDictionaryAsync(m => m.GameId, m => (GameMemberRole?)m.Role);

        var gameItems = games.Select(g => new GameListItemVm(g, myRoles.ContainsKey(g.Id) ? myRoles[g.Id] : null)).ToList();

        var memberRows = await _db.UniverseMembers
            .Where(m => m.UniverseId == universeId)
            .Select(m => new { m.UserId, m.Role })
            .ToListAsync();

        var memberIds = memberRows.Select(m => m.UserId).Distinct().ToList();

        // EF Core tip: zoradiť podľa UserName v DB, nie podľa VM property.
        var users = await _db.Users
            .Where(u => memberIds.Contains(u.Id))
            .OrderBy(u => u.UserName ?? u.Id)
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();

        var usersDict = users.ToDictionary(x => x.Id, x => x.UserName ?? x.Id);

        var members = memberRows
            .Select(m => new UniverseMemberVm(
                m.UserId,
                usersDict.ContainsKey(m.UserId) ? usersDict[m.UserId] : m.UserId,
                m.Role))
            .OrderBy(m => m.DisplayName)
            .ToList();

        var createGame = createGameOverride ?? new GameCreateVm { UniverseId = universeId };
        createGame.UniverseId = universeId;

        return new UniverseDetailsVm
        {
            Universe = universe,
            MemberRole = role,
            RulesetDisplayName = _rulesets.GetByKey(universe.RulesetKey).DisplayName,
            Games = gameItems,
            Members = members,
            CreateGame = createGame,
            ShowCreateGameModal = showCreateGameModal
        };
    }
}

internal static class JoinPasswordGenerator
{
    public static string Generate()
    {
        // jednoduché generovanie
        var guid = Guid.NewGuid().ToString("N");
        return guid.Substring(0, 8);
    }
}
