using Cybarch.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cybarch.Web.Data;

/// <summary>
/// DbContext aplikácie.
///
/// - Dedíme z IdentityDbContext, takže tabuľky pre Identity (Users, Roles, ...) sú súčasťou databázy.
/// - K našim doménovým entitám pridávame DbSety.
/// - V OnModelCreating definujeme kompozitné kľúče (UniverseMember, GameMember, CharacterAssignment).
/// </summary>
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Universe> Universes => Set<Universe>();
    public DbSet<UniverseMember> UniverseMembers => Set<UniverseMember>();

    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameMember> GameMembers => Set<GameMember>();

    public DbSet<Character> Characters => Set<Character>();
    public DbSet<CharacterAssignment> CharacterAssignments => Set<CharacterAssignment>();
    public DbSet<CharacterItem> CharacterItems => Set<CharacterItem>();
    public DbSet<CharacterCurrency> CharacterCurrencies => Set<CharacterCurrency>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // UniverseMember: User <-> Universe (kompozitný kľúč)
        builder.Entity<UniverseMember>()
            .HasKey(x => new { x.UniverseId, x.UserId });

        builder.Entity<UniverseMember>()
            .HasOne(x => x.Universe)
            .WithMany(u => u.Members)
            .HasForeignKey(x => x.UniverseId)
            .OnDelete(DeleteBehavior.Cascade);

        // GameMember: User <-> Game (kompozitný kľúč)
        builder.Entity<GameMember>()
            .HasKey(x => new { x.GameId, x.UserId });

        builder.Entity<GameMember>()
            .HasOne(x => x.Game)
            .WithMany(g => g.Members)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // CharacterAssignment: Character ↔ User (kompozitný kľúč)
        builder.Entity<CharacterAssignment>()
            .HasKey(x => new { x.CharacterId, x.UserId });

        builder.Entity<CharacterAssignment>()
            .HasOne(x => x.Character)
            .WithMany(c => c.Assignments)
            .HasForeignKey(x => x.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Universe
        builder.Entity<Universe>()
            .HasIndex(u => u.Name);

        // Game
        builder.Entity<Game>()
            .HasIndex(g => new { g.UniverseId, g.Name });

        // Character
        builder.Entity<Character>()
            .HasIndex(c => new { c.GameId, c.Name });

        // CharacterItem: Character <-> RulesetItem (kompozitný kľúč)
        builder.Entity<CharacterItem>()
            .HasKey(x => new { x.CharacterId, x.ItemId });

        builder.Entity<CharacterItem>()
            .HasOne(x => x.Character)
            .WithMany(c => c.Items)
            .HasForeignKey(x => x.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CharacterItem>()
            .HasIndex(x => new { x.CharacterId, x.AddedAtUtc });

        // CharacterCurrency: Character <->> DenominationKey (kompozitný kľúč)
        builder.Entity<CharacterCurrency>()
            .HasKey(x => new { x.CharacterId, x.DenominationKey });

        builder.Entity<CharacterCurrency>()
            .HasOne(x => x.Character)
            .WithMany(c => c.Currency)
            .HasForeignKey(x => x.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);


    }
}
