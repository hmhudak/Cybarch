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
public class GamesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UniverseAccessService _universeAccess;
    private readonly GameAccessService _gameAccess;
    private readonly RulesetRegistry _rulesets;

    public GamesController(ApplicationDbContext db, UniverseAccessService universeAccess, GameAccessService gameAccess, RulesetRegistry rulesets)
    {
        _db = db;
        _universeAccess = universeAccess;
        _gameAccess = gameAccess;
        _rulesets = rulesets;
    }

    // Ponechané iba kvôli starým linkom – UI už nemá samostatný Games index.
    public IActionResult Index(int universeId)
        => RedirectToAction("Details", "Universes", new { id = universeId });

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var game = await _db.Games
            .Include(g => g.Universe)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null) return NotFound();

        if (!await _universeAccess.IsMemberAsync(game.UniverseId, userId))
            return Forbid();

        var vm = await BuildDetailsVmAsync(id, userId);
        return View(vm);
    }

    // ---------------------------------------------------------------------
    // CampaignText
    // ---------------------------------------------------------------------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCampaign(int id, string campaignText)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!await _gameAccess.IsDmAsync(id, userId))
            return Forbid();

        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game is null) return NotFound();

        game.CampaignText = campaignText ?? string.Empty;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    // ---------------------------------------------------------------------
    // Characters
    // ---------------------------------------------------------------------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCharacter([Bind(Prefix = "CreateCharacter")] CharacterCreateVm form)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!await _gameAccess.IsDmAsync(form.GameId, userId))
            return Forbid();

        var game = await _db.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == form.GameId);

        if (game is null) return NotFound();

        var ruleset = _rulesets.GetByKey(game.RulesetKey);

        // Server-side validácie (nebrať len klienta)
        if (string.IsNullOrWhiteSpace(form.Name))
            ModelState.AddModelError($"CreateCharacter.{nameof(form.Name)}", "Name je povinné.");

        if (!ruleset.IsValidRace(form.Race))
            ModelState.AddModelError($"CreateCharacter.{nameof(form.Race)}", "Race musí byť vybraná z katalógu.");

        if (!ruleset.IsValidClass(form.ClassName))
            ModelState.AddModelError($"CreateCharacter.{nameof(form.ClassName)}", "Class musí byť vybraná z katalógu.");

        if (form.Level < 1)
            ModelState.AddModelError($"CreateCharacter.{nameof(form.Level)}", "Level musí byť aspoň 1.");

        if (form.Level < 6 && !string.IsNullOrWhiteSpace(form.SubclassName))
            ModelState.AddModelError($"CreateCharacter.{nameof(form.SubclassName)}", "Subclass je povolený až od levelu 6.");

        if (form.Level >= 6 && !ruleset.IsValidSubclass(form.ClassName, form.SubclassName))
            ModelState.AddModelError($"CreateCharacter.{nameof(form.SubclassName)}", "Subclass nepatrí pod zvolenú Class.");

        if (!ModelState.IsValid)
        {
            var vm = await BuildDetailsVmAsync(form.GameId, userId, showCreateModal: true, createFormOverride: form);
            return View("Details", vm);
        }

        // subclass pravidlo: ak level < 6 => null
        if (form.Level < 6) form.SubclassName = null;

        var vitals = ruleset.GetDefaultVitals(form.ClassName, form.Level);

        var character = new Character
        {
            GameId = form.GameId,
            Name = form.Name.Trim(),
            Race = form.Race,
            Level = form.Level,
            Experience = 0,
            ClassName = form.ClassName,
            SubclassName = string.IsNullOrWhiteSpace(form.SubclassName) ? null : form.SubclassName,

            // MVP default attributes
            Str = form.Str,
            Dex = form.Dex,
            Con = form.End,
            Int = form.Int,
            Cha = form.Char,

            Notes = string.Empty,

            MaxHp = vitals.MaxHp,
            CurrentHp = vitals.MaxHp,
            MaxMana = vitals.MaxMana,
            CurrentMana = vitals.MaxMana,

            Currency = ruleset.Currency.Denominations
                .Select(d => new CharacterCurrency { DenominationKey = d.Key, Amount = 0 })
                .ToList(),

        };

        _db.Characters.Add(character);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = form.GameId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAssignments(int gameId, int characterId, List<string> selectedPlayerIds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!await _gameAccess.IsDmAsync(gameId, userId))
            return Forbid();

        var character = await _db.Characters
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == characterId && c.GameId == gameId);

        if (character is null) return NotFound();

        selectedPlayerIds ??= new();

        // Zoberieme len tých, ktorí sú skutočne Players v tejto hre.
        var validPlayers = await _db.GameMembers
            .Where(m => m.GameId == gameId && m.Role == GameMemberRole.Player)
            .Select(m => m.UserId)
            .ToListAsync();

        var validSelected = selectedPlayerIds.Intersect(validPlayers).ToHashSet();

        // Remove absent
        var toRemove = character.Assignments.Where(a => !validSelected.Contains(a.UserId)).ToList();
        if (toRemove.Count > 0)
            _db.CharacterAssignments.RemoveRange(toRemove);

        // Add missing
        var existing = character.Assignments.Select(a => a.UserId).ToHashSet();
        var toAdd = validSelected.Where(id => !existing.Contains(id)).ToList();

        foreach (var pid in toAdd)
        {
            _db.CharacterAssignments.Add(new CharacterAssignment
            {
                CharacterId = character.Id,
                UserId = pid,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = gameId });
    }

    // ---------------------------------------------------------------------
    // Members management (DM/Player/Not players)
    // ---------------------------------------------------------------------

    private string GodModeCookieKey(int gameId) => $"cybarch_godmode_{gameId}";

    private bool IsGodModeActive(int gameId, string userId, Universe universe)
    {
        if (universe.CreatorUserId != userId) return false;
        return Request.Cookies.TryGetValue(GodModeCookieKey(gameId), out var v) && v == "1";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleGodMode(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var game = await _db.Games
            .Include(g => g.Universe)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null) return NotFound();

        if (!await _universeAccess.IsMemberAsync(game.UniverseId, userId)) return Forbid();
        if (game.Universe.CreatorUserId != userId) return Forbid();

        var key = GodModeCookieKey(id);
        var isOn = Request.Cookies.TryGetValue(key, out var v) && v == "1";

        if (isOn)
        {
            Response.Cookies.Delete(key);
        }
        else
        {
            Response.Cookies.Append(key, "1", new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            // God mode zároveň zabezpečí, že Creator je DM (ak nebol).
            var me = await _db.GameMembers.FirstOrDefaultAsync(m => m.GameId == id && m.UserId == userId);
            if (me is null)
            {
                _db.GameMembers.Add(new GameMember
                {
                    GameId = id,
                    UserId = userId,
                    Role = GameMemberRole.DM,
                    JoinedAtUtc = DateTime.UtcNow
                });
            }
            else if (me.Role == GameMemberRole.Player)
            {
                me.Role = GameMemberRole.DM;

                // Ak sa Player zmení na DM, nesmie mu zostať assign na postavu.
                await RemoveAssignmentsForUserInGameAsync(id, userId);
            }

            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakePlayer(int id, string userIdToAdd)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game is null) return NotFound();

        if (!await _gameAccess.IsDmAsync(id, userId)) return Forbid();
        if (!await _universeAccess.IsMemberAsync(game.UniverseId, userIdToAdd)) return BadRequest();

        var gm = await _db.GameMembers.FirstOrDefaultAsync(m => m.GameId == id && m.UserId == userIdToAdd);
        if (gm is null)
        {
            _db.GameMembers.Add(new GameMember
            {
                GameId = id,
                UserId = userIdToAdd,
                Role = GameMemberRole.Player,
                JoinedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeDm(int id, string userIdToPromote)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!await _gameAccess.IsDmAsync(id, userId)) return Forbid();

        var member = await _db.GameMembers.FirstOrDefaultAsync(m => m.GameId == id && m.UserId == userIdToPromote);
        if (member is null) return RedirectToAction(nameof(Details), new { id });

        if (member.Role != GameMemberRole.Player) return RedirectToAction(nameof(Details), new { id });

        member.Role = GameMemberRole.DM;

        // Pri promócii Player -> DM zrušíme jeho assigny (DM nemá byť assigned na postavy).
        await RemoveAssignmentsForUserInGameAsync(id, userIdToPromote);

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromGame(int id, string userIdToRemove)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!await _gameAccess.IsDmAsync(id, userId)) return Forbid();

        var member = await _db.GameMembers.FirstOrDefaultAsync(m => m.GameId == id && m.UserId == userIdToRemove);
        if (member is null) return RedirectToAction(nameof(Details), new { id });

        if (member.Role != GameMemberRole.Player)
        {
            TempData["Error"] = "DM nie je možné priamo odstrániť z hry. Najprv ho degraduj na Playera.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await RemoveAssignmentsForUserInGameAsync(id, userIdToRemove);

        _db.GameMembers.Remove(member);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveGame(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game is null) return NotFound();

        var member = await _db.GameMembers.FirstOrDefaultAsync(m => m.GameId == id && m.UserId == userId);
        if (member is null) return RedirectToAction(nameof(Details), new { id });

        if (member.Role == GameMemberRole.DM)
        {
            TempData["Error"] = "Ako DM nemôžeš odísť z hry. Najprv sa degraduj na Playera (ak nie si posledný DM).";
            return RedirectToAction(nameof(Details), new { id });
        }

        await RemoveAssignmentsForUserInGameAsync(id, userId);

        _db.GameMembers.Remove(member);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Universes", new { id = game.UniverseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DegradeToPlayer(int id, string userIdToDegrade)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var game = await _db.Games
            .Include(g => g.Universe)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null) return NotFound();

        if (!await _gameAccess.IsDmAsync(id, userId)) return Forbid();

        var isGod = IsGodModeActive(id, userId, game.Universe);
        if (userIdToDegrade != userId && !isGod) return Forbid();

        var dmCount = await _db.GameMembers.CountAsync(m => m.GameId == id && m.Role == GameMemberRole.DM);
        if (dmCount <= 1)
        {
            TempData["Error"] = "Nemôžeš degradovať posledného DM.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var target = await _db.GameMembers.FirstOrDefaultAsync(m => m.GameId == id && m.UserId == userIdToDegrade);
        if (target is null) return RedirectToAction(nameof(Details), new { id });

        if (target.Role != GameMemberRole.DM) return RedirectToAction(nameof(Details), new { id });

        target.Role = GameMemberRole.Player;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task RemoveAssignmentsForUserInGameAsync(int gameId, string userId)
    {
        // Join CharacterAssignments -> Characters (filter gameId)
        var assignments = await _db.CharacterAssignments
            .Join(_db.Characters.Where(c => c.GameId == gameId),
                a => a.CharacterId,
                c => c.Id,
                (a, c) => a)
            .Where(a => a.UserId == userId)
            .ToListAsync();

        if (assignments.Count > 0)
            _db.CharacterAssignments.RemoveRange(assignments);

    }

    // ---------------------------------------------------------------------
    // ViewModel builder
    // ---------------------------------------------------------------------

    private async Task<GameDetailsVm> BuildDetailsVmAsync(int gameId, string currentUserId, bool showCreateModal = false, CharacterCreateVm? createFormOverride = null)
    {
        var game = await _db.Games
            .Include(g => g.Universe)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game is null) throw new InvalidOperationException("Game not found.");

        var universe = game.Universe;
        var ruleset = _rulesets.GetByKey(game.RulesetKey);

        var myRole = await _gameAccess.GetRoleAsync(gameId, currentUserId);
        var isDm = myRole == GameMemberRole.DM;

        var isUniverseCreator = universe.CreatorUserId == currentUserId;
        var isGodMode = IsGodModeActive(gameId, currentUserId, universe);

        // Game members
        var gmRows = await _db.GameMembers
            .Where(m => m.GameId == gameId)
            .Select(m => new { m.UserId, m.Role })
            .ToListAsync();

        var gmIds = gmRows.Select(x => x.UserId).Distinct().ToList();

        // Universe members
        var umRows = await _db.UniverseMembers
            .Where(m => m.UniverseId == universe.Id)
            .Select(m => m.UserId)
            .ToListAsync();

        // Prehľadné načítanie user display name
        var allIds = gmIds.Union(umRows).Distinct().ToList();
        var users = await _db.Users
            .Where(u => allIds.Contains(u.Id))
            .OrderBy(u => u.UserName ?? u.Id)
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();

        var usersDict = users.ToDictionary(x => x.Id, x => x.UserName ?? x.Id);

        List<UserVm> MapUsers(IEnumerable<string> ids)
            => ids.Select(id => new UserVm(id, usersDict.ContainsKey(id) ? usersDict[id] : id)).ToList();

        var dmIds = gmRows.Where(x => x.Role == GameMemberRole.DM).Select(x => x.UserId).ToList();
        var playerIds = gmRows.Where(x => x.Role == GameMemberRole.Player).Select(x => x.UserId).ToList();

        var dms = MapUsers(dmIds).OrderBy(x => x.DisplayName).ToList();
        var players = MapUsers(playerIds).OrderBy(x => x.DisplayName).ToList();

        var notPlayerIds = umRows.Except(gmIds).ToList();
        var notPlayers = MapUsers(notPlayerIds).OrderBy(x => x.DisplayName).ToList();

        var dmCount = dmIds.Count;

        // Characters + assignments
        var characters = await _db.Characters
            .Where(c => c.GameId == gameId)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        var charIds = characters.Select(c => c.Id).ToList();
        var assignments = await _db.CharacterAssignments
            .Where(a => charIds.Contains(a.CharacterId))
            .AsNoTracking()
            .ToListAsync();

        var assignedByChar = assignments
            .GroupBy(a => a.CharacterId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.UserId).ToHashSet());

        var charVms = new List<CharacterListItemVm>();
        foreach (var c in characters)
        {
            assignedByChar.TryGetValue(c.Id, out var assignedIds);
            assignedIds ??= new HashSet<string>();

            var assignedPlayers = MapUsers(assignedIds).OrderBy(x => x.DisplayName).ToList();

            // kto môže otvoriť detail postavy:
            // - DM
            // - assigned player
            var canOpen = isDm || assignedIds.Contains(currentUserId);

            var className = ruleset.Classes.FirstOrDefault(x => x.Key == c.ClassName)?.Name ?? c.ClassName;
            var subclassName = string.IsNullOrWhiteSpace(c.SubclassName)
                ? "–"
                : (ruleset.Subclasses.FirstOrDefault(x => x.Key == c.SubclassName)?.Name ?? c.SubclassName);

            charVms.Add(new CharacterListItemVm
            {
                CharacterId = c.Id,
                Name = c.Name,
                Level = c.Level,
                ClassDisplay = className,
                SubclassDisplay = subclassName,
                AssignedPlayers = assignedPlayers,
                AssignedUserIds = assignedIds,
                CanOpenDetails = canOpen
            });
        }

        // CreateCharacter form (dropdowns)
        var create = createFormOverride ?? new CharacterCreateVm { GameId = gameId, Level = 1 };
        create.GameId = gameId;
        create.RaceOptions = ruleset.Races.ToList();
        create.ClassOptions = ruleset.Classes.ToList();
        create.SubclassOptions = ruleset.Subclasses.ToList();

        // Ne-GameMember message:
        // - ak user nie je v GameMembers, zobrazíme hlášku
        // - Universe creator stále uvidí tlačidlo God mode (v view)
        string? nonMemberMessage = null;
        if (myRole is null)
            nonMemberMessage = "Nie si hráč tejto hry.";

        return new GameDetailsVm
        {
            Game = game,
            Universe = universe,
            RulesetDisplayName = ruleset.DisplayName,
            NonGameMemberMessage = nonMemberMessage,
            IsDm = isDm,
            CurrentUserId = currentUserId,
            IsUniverseCreator = isUniverseCreator,
            IsGodMode = isGodMode,
            DmCount = dmCount,
            CampaignEditText = game.CampaignText,
            Dms = dms,
            Players = players,
            NotPlayers = notPlayers,
            Characters = charVms,
            CreateCharacter = create,
            ShowCreateCharacterModal = showCreateModal,
            TempError = TempData["Error"] as string
        };
    }
}
