using Microsoft.Playwright.MSTest;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

public abstract class BaseSystemTest : PageTest
{
    protected static WindPowerFactory Factory => TestingInfrastructure.Factory;
    private static Respawner _respawner = null!;
    protected IServiceScope Scope = null!;
    protected AppDbContext Db = null!;

    [TestInitialize]
    public async Task SystemSetup()
    {
        if (_respawner == null)
        {
            using var conn = new NpgsqlConnection(Factory.GetConnectionString());
            await conn.OpenAsync();
            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            });
        }

        using (var conn = new NpgsqlConnection(Factory.GetConnectionString()))
        {
            await conn.OpenAsync();
            await _respawner.ResetAsync(conn);
        }

        Scope = Factory.Services.CreateScope();
        Db = Scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await Page.GotoAsync(Factory.ServerAddress);
    }

    [TestCleanup]
    public void SystemCleanup()
    {
        Scope?.Dispose();
    }
}