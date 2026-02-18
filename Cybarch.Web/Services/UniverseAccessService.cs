using Cybarch.Web.Data;
using Cybarch.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Cybarch.Web.Services;

/// <summary>
/// Universe-level autorizačné helpery.
///
/// Poznámka:
/// - Toto nie je "plná" policy-based autorizácia.
/// - Teraz máme explicitné kontroly v controlleri.
/// - Neskôr sa to dá prehodiť na AuthorizationHandler + policies.
/// </summary>
public class UniverseAccessService
{
    private readonly ApplicationDbContext _db;

    public UniverseAccessService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsMemberAsync(int universeId, string userId)
        => _db.UniverseMembers.AnyAsync(m => m.UniverseId == universeId && m.UserId == userId);

    public async Task<UniverseMemberRole?> GetRoleAsync(int universeId, string userId)
    {
        var role = await _db.UniverseMembers
            .Where(m => m.UniverseId == universeId && m.UserId == userId)
            .Select(m => (UniverseMemberRole?)m.Role)
            .FirstOrDefaultAsync();

        return role;
    }
}
