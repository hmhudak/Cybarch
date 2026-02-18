using Cybarch.Web.Data;
using Cybarch.Web.Models;
using Cybarch.Web.Services;
using Cybarch.Web.Services.Rulesets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Pages (Identity UI)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Db
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=cybarch.dev.sqlite";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        // MVP nastavenie: jednoduché heslá
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Default: všetko require login (okrem [AllowAnonymous] na Identity stránkach)
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// App služby
builder.Services.AddScoped<UniverseAccessService>();
builder.Services.AddScoped<GameAccessService>();

// Rulesets
builder.Services.AddSingleton<IRulesetDefinition, Drd16Ruleset>();
builder.Services.AddSingleton<RulesetRegistry>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Dev seed
await SeedDevDataAsync(app);

app.Run();

static async Task SeedDevDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Pre MVP používame EnsureCreated (bez migrations). Prejdeme na migrations.
    await db.Database.EnsureCreatedAsync();

    // Users
    async Task<IdentityUser> EnsureUserAsync(string userName, string password)
    {
        var u = await userManager.FindByNameAsync(userName);
        if (u != null) return u;

        var email = $"{userName}@example.local";

        u = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(u, password);
        if (!result.Succeeded)
            throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        return u;
    }

    var creator = await EnsureUserAsync("creator", "Passw0rd");
    var jano = await EnsureUserAsync("jano", "Passw0rd");
    var mato = await EnsureUserAsync("mato", "Passw0rd");

    // Universe
    if (!db.Universes.Any())
    {
        var universe = new Universe
        {
            Name = "Demo Universe",
            CreatorUserId = creator.Id,
            JoinPassword = "demo",
            RulesetKey = "drd16",
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Universes.Add(universe);
        await db.SaveChangesAsync();

        db.UniverseMembers.AddRange(
            new UniverseMember { UniverseId = universe.Id, UserId = creator.Id, Role = UniverseMemberRole.Creator, JoinedAtUtc = DateTime.UtcNow },
            new UniverseMember { UniverseId = universe.Id, UserId = jano.Id, Role = UniverseMemberRole.Member, JoinedAtUtc = DateTime.UtcNow },
            new UniverseMember { UniverseId = universe.Id, UserId = mato.Id, Role = UniverseMemberRole.Member, JoinedAtUtc = DateTime.UtcNow }
        );

        var game = new Game
        {
            UniverseId = universe.Id,
            Name = "Demo Game",
            CreatedByUserId = creator.Id,
            CreatedAtUtc = DateTime.UtcNow,
            RulesetKey = universe.RulesetKey,
            CampaignText = "Welcome to Cybarch demo game!\n\nDM: creator\nPlayer: jano"
        };

        db.Games.Add(game);
        await db.SaveChangesAsync();

        db.GameMembers.AddRange(
            new GameMember { GameId = game.Id, UserId = creator.Id, Role = GameMemberRole.DM, JoinedAtUtc = DateTime.UtcNow },
            new GameMember { GameId = game.Id, UserId = jano.Id, Role = GameMemberRole.Player, JoinedAtUtc = DateTime.UtcNow }
        );        

        var ch = new Character
        {
            GameId = game.Id,
            Name = "Boris",
            Race = "human",
            Level = 5,
            ClassName = "warrior",
            SubclassName = null,
            MaxHp = 30,
            CurrentHp = 30,
            MaxMana = 10,
            CurrentMana = 10,
            Str = 12, Dex = 10, Con = 12, Int = 9, Wis = 10, Cha = 10,
            Notes = "Demo character"
        };

        db.Characters.Add(ch);
        await db.SaveChangesAsync();

        db.CharacterAssignments.Add(new CharacterAssignment { CharacterId = ch.Id, UserId = jano.Id, CreatedAtUtc = DateTime.UtcNow });

        db.CharacterItems.AddRange(
            new CharacterItem { CharacterId = ch.Id, ItemId = "weapon_short_sword", Quantity = 1, AddedAtUtc = DateTime.UtcNow },
            new CharacterItem { CharacterId = ch.Id, ItemId = "rope", Quantity = 1, AddedAtUtc = DateTime.UtcNow },
            new CharacterItem { CharacterId = ch.Id, ItemId = "prov_rations", Quantity = 3, AddedAtUtc = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();
    }
}
