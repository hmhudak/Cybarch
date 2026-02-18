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
public class CharactersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly GameAccessService _gameAccess;
    private readonly UniverseAccessService _universeAccess;
    private readonly RulesetRegistry _rulesets;

    public CharactersController(ApplicationDbContext db, GameAccessService gameAccess, UniverseAccessService universeAccess, RulesetRegistry rulesets)
    {
        _db = db;
        _gameAccess = gameAccess;
        _universeAccess = universeAccess;
        _rulesets = rulesets;
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var character = await _db.Characters
            .Include(c => c.Game)
            .ThenInclude(g => g.Universe)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (character is null) return NotFound();

        if (!await _universeAccess.IsMemberAsync(character.Game.UniverseId, userId))
            return Forbid();

        var isDm = await _gameAccess.IsDmAsync(character.GameId, userId);

        var assigned = await _db.CharacterAssignments
            .AnyAsync(a => a.CharacterId == id && a.UserId == userId);

        if (!isDm && !assigned)
            return Forbid();

        var ruleset = _rulesets.GetByKey(character.Game.RulesetKey);

        // Inventory (DB) + katalóg (ruleset)
        var itemRows = await _db.CharacterItems
            .Where(x => x.CharacterId == id)
            .AsNoTracking()
            .ToListAsync();

        var itemCatalog = ruleset.Items
            .OrderBy(i => i.Type)
            .ThenBy(i => i.Name)
            .ToList();

        string TypeLabel(ItemType t) => t switch
        {
            ItemType.Weapon => "Zbraň",
            ItemType.Armor => "Brnenie",
            ItemType.Ingredient => "Alchymistická prísada",
            ItemType.Provision => "Proviant",
            ItemType.Other => "Ostatné",
            _ => t.ToString()
        };

        string StatsLineFor(RulesetItem it)
        {
            if (it.Type == ItemType.Weapon && it.Weapon is not null)
                return $"Zbraň: Sila {it.Weapon.Strength}, ÚČ {it.Weapon.Attack}, OČ {it.Weapon.Defense}";
            if (it.Type == ItemType.Armor && it.Armor is not null)
                return $"Brnenie: Obrana {it.Armor.Defense}";
            return string.Empty;
        }

        var catalogDict = itemCatalog.ToDictionary(i => i.Id, i => i, StringComparer.OrdinalIgnoreCase);

        var inventoryItems = new List<InventoryItemVm>();
        foreach (var row in itemRows.OrderBy(r => r.ItemId))
        {
            if (!catalogDict.TryGetValue(row.ItemId, out var def))
            {
                inventoryItems.Add(new InventoryItemVm
                {
                    ItemId = row.ItemId,
                    Name = row.ItemId,
                    Type = "(unknown)",
                    Description = "Predmet už nie je v katalógu rulesetu.",
                    Quantity = row.Quantity,
                    UnitWeight = 0m,
                    Price = 0
                });
                continue;
            }

            inventoryItems.Add(new InventoryItemVm
            {
                ItemId = def.Id,
                Name = def.Name,
                Type = TypeLabel(def.Type),
                Description = def.Description,
                Quantity = row.Quantity,
                UnitWeight = def.Weight,
                Price = def.Price,
                StatsLine = (def.Type == ItemType.Weapon || def.Type == ItemType.Armor) ? StatsLineFor(def) : null
            });
        }

        var availableItems = itemCatalog.Select(def => new ItemPickerItemVm
        {
            ItemId = def.Id,
            Name = def.Name,
            Type = TypeLabel(def.Type),
            Description = def.Description,
            Weight = def.Weight,
            Price = def.Price,
            StatsLine = (def.Type == ItemType.Weapon || def.Type == ItemType.Armor) ? StatsLineFor(def) : null
        }).ToList();

        var capacity = character.Str * 10;
        var totalWeight = inventoryItems.Sum(x => x.TotalWeight);

        var edit = new CharacterEditVm
        {
            Id = character.Id,
            Name = character.Name,
            Race = character.Race,
            Level = character.Level,
            Experience = character.Experience,
            ClassName = character.ClassName,
            SubclassName = character.SubclassName,

            Str = character.Str,
            Dex = character.Dex,
            Con = character.Con,
            Int = character.Int,
            Wis = character.Wis,
            Cha = character.Cha,

            Notes = character.Notes,

            MaxHp = character.MaxHp,
            CurrentHp = character.CurrentHp,
            MaxMana = character.MaxMana,
            CurrentMana = character.CurrentMana,

            RaceOptions = ruleset.Races.ToList(),
            ClassOptions = ruleset.Classes.ToList(),
            SubclassOptions = ruleset.Subclasses.ToList()
        };

        var vm = new CharacterDetailsVm
        {
            GameId = character.GameId,
            GameName = character.Game.Name,
            IsDm = isDm,
            IsAssignedPlayer = assigned,
            RulesetDisplayName = ruleset.DisplayName,
            RulesetKey = character.Game.RulesetKey,
            Character = character,

            CarryCapacity = capacity,
            TotalWeight = totalWeight,
            IsOverCapacity = totalWeight > capacity,
            CanManageInventory = isDm || assigned,
            InventoryItems = inventoryItems,
            AvailableItems = availableItems,

            EditForm = edit,
            ShowEditModal = false,
            ErrorMessage = TempData["Error"] as string
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateVitals(int id, int currentHp, int currentMana)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var character = await _db.Characters
            .Include(c => c.Game)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (character is null) return NotFound();

        var isDm = await _gameAccess.IsDmAsync(character.GameId, userId);
        var assigned = await _db.CharacterAssignments.AnyAsync(a => a.CharacterId == id && a.UserId == userId);

        if (!isDm && !assigned) return Forbid();

        character.CurrentHp = Math.Clamp(currentHp, 0, character.MaxHp);
        character.CurrentMana = Math.Clamp(currentMana, 0, character.MaxMana);

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "EditForm")] CharacterEditVm form)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var character = await _db.Characters
            .Include(c => c.Game)
            .FirstOrDefaultAsync(c => c.Id == form.Id);

        if (character is null) return NotFound();

        if (!await _gameAccess.IsDmAsync(character.GameId, userId))
            return Forbid();

        var ruleset = _rulesets.GetByKey(character.Game.RulesetKey);

        if (string.IsNullOrWhiteSpace(form.Name))
            ModelState.AddModelError($"EditForm.{nameof(form.Name)}", "Name je povinné.");

        if (!ruleset.IsValidRace(form.Race))
            ModelState.AddModelError($"EditForm.{nameof(form.Race)}", "Race musí byť z katalógu.");

        if (!ruleset.IsValidClass(form.ClassName))
            ModelState.AddModelError($"EditForm.{nameof(form.ClassName)}", "Class musí byť z katalógu.");

        if (form.Level < 1)
            ModelState.AddModelError($"EditForm.{nameof(form.Level)}", "Level musí byť aspoň 1.");

        if (form.Level < 6)
        {
            form.SubclassName = null;
        }
        else
        {
            if (!ruleset.IsValidSubclass(form.ClassName, form.SubclassName))
                ModelState.AddModelError($"EditForm.{nameof(form.SubclassName)}", "Subclass nepatrí pod zvolenú Class.");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Chyby vo formulári.";
            return RedirectToAction(nameof(Details), new { id = form.Id });
        }

        character.Name = form.Name.Trim();
        character.Race = form.Race;
        character.Level = form.Level;
        character.Experience = form.Experience;
        character.ClassName = form.ClassName;
        character.SubclassName = string.IsNullOrWhiteSpace(form.SubclassName) ? null : form.SubclassName;

        character.Str = form.Str;
        character.Dex = form.Dex;
        character.Con = form.Con;
        character.Int = form.Int;
        character.Wis = form.Wis;
        character.Cha = form.Cha;

        character.Notes = form.Notes ?? string.Empty;

        character.MaxHp = Math.Max(0, form.MaxHp);
        character.CurrentHp = Math.Clamp(form.CurrentHp, 0, character.MaxHp);
        character.MaxMana = Math.Max(0, form.MaxMana);
        character.CurrentMana = Math.Clamp(form.CurrentMana, 0, character.MaxMana);

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = form.Id });
    }

    // Inventory items

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int id, string itemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var character = await _db.Characters
            .Include(c => c.Game)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (character is null) return NotFound();

        var isDm = await _gameAccess.IsDmAsync(character.GameId, userId);
        var assigned = await _db.CharacterAssignments.AnyAsync(a => a.CharacterId == id && a.UserId == userId);
        if (!isDm && !assigned) return Forbid();

        var ruleset = _rulesets.GetByKey(character.Game.RulesetKey);
        if (!ruleset.IsValidItem(itemId))
        {
            TempData["Error"] = "Neplatný predmet (nie je v katalógu rulesetu).";
            return RedirectToAction(nameof(Details), new { id });
        }

        var row = await _db.CharacterItems.FirstOrDefaultAsync(x => x.CharacterId == id && x.ItemId == itemId);
        if (row is null)
        {
            _db.CharacterItems.Add(new CharacterItem
            {
                CharacterId = id,
                ItemId = itemId,
                Quantity = 1,
                AddedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            row.Quantity += 1;
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int id, string itemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var character = await _db.Characters
            .Include(c => c.Game)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (character is null) return NotFound();

        var isDm = await _gameAccess.IsDmAsync(character.GameId, userId);
        var assigned = await _db.CharacterAssignments.AnyAsync(a => a.CharacterId == id && a.UserId == userId);
        if (!isDm && !assigned) return Forbid();

        var row = await _db.CharacterItems.FirstOrDefaultAsync(x => x.CharacterId == id && x.ItemId == itemId);
        if (row is null) return RedirectToAction(nameof(Details), new { id });

        row.Quantity -= 1;
        if (row.Quantity <= 0)
            _db.CharacterItems.Remove(row);

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }
}
