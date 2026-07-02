using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright.MSTest;
using Npgsql;
using Respawn;

public abstract class BaseSystemTest : PageTest
{
    protected AppDbContext Db = null!;
    private static Respawner _respawner = null!;
    private static readonly string DbConnectionString =
        Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
        ?? "Host=localhost;Database=windpower_test;Username=admin;Password=admin";

    public static readonly string FrontendUrl =
        Environment.GetEnvironmentVariable("FRONTEND_URL")
        ?? "http://localhost:4200";

    [TestInitialize]
    public async Task BaseSetup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(DbConnectionString)
            .Options;
        Db = new AppDbContext(options);

        using var conn = new NpgsqlConnection(DbConnectionString);
        await conn.OpenAsync();

        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            });
        }

        await _respawner.ResetAsync(conn);
    }

    [TestCleanup]
    public async Task BaseCleanup()
    {
        if (Db != null)
        {
            await Db.DisposeAsync();
        }
    }
}