
using Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

[TestClass]
public abstract class BaseIntegrationTest
{
    protected static WindPowerFactory Factory = null!;
    protected static Respawner Respawner = null!;
    protected IServiceScope Scope = null!;
    protected AppDbContext Db = null!;

    public static async Task GlobalSetup(WindPowerFactory factory)
    {
        Factory = factory;
        var client = factory.CreateClient();
        var connectionString = Factory.GetConnectionString();

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        
        Respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    [TestInitialize]
    public async Task BaseSetup()
    {
        using var conn = new NpgsqlConnection(Factory.GetConnectionString());
        await conn.OpenAsync();
        await Respawner.ResetAsync(conn);

        Scope = Factory.Services.CreateScope();
        Db = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [TestCleanup]
    public void BaseCleanup()
    {
        Scope.Dispose();
    }
}
