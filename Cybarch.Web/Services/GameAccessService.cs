using Cybarch.Web.Data;
using Cybarch.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Cybarch.Web.Services;

public class GameAccessService
{
    private readonly ApplicationDbContext _db;

    public GameAccessService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsMemberAsync(int gameId, string userId)
        => _db.GameMembers.AnyAsync(m => m.GameId == gameId && m.UserId == userId);

    public async Task<GameMemberRole?> GetRoleAsync(int gameId, string userId)
    {
        var role = await _db.GameMembers
            .Where(m => m.GameId == gameId && m.UserId == userId)
            .Select(m => (GameMemberRole?)m.Role)
            .FirstOrDefaultAsync();

        return role;
    }

    public Task<bool> IsDmAsync(int gameId, string userId)
        => _db.GameMembers.AnyAsync(m => m.GameId == gameId && m.UserId == userId && m.Role == GameMemberRole.DM);
}
