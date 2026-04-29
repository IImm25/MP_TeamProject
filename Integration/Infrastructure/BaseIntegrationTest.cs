using Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

public abstract class BaseIntegrationTest
{
    protected static WindPowerFactory Factory => TestingInfrastructure.Factory;
    protected static Respawner Respawner = null!;
    protected IServiceScope Scope = null!;
    protected AppDbContext Db = null!;

    [TestInitialize]
    public async Task BaseSetup()
    {
        if (Respawner == null)
        {
            using var conn = new NpgsqlConnection(Factory.GetConnectionString());
            await conn.OpenAsync();
            Respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            });
        }

        using (var conn = new NpgsqlConnection(Factory.GetConnectionString()))
        {
            await conn.OpenAsync();
            await Respawner.ResetAsync(conn);
        }

        Scope = Factory.Services.CreateScope();
        Db = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [TestCleanup]
    public void BaseCleanup()
    {
        Scope?.Dispose();
    }
}