using Backend.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

public class WindPowerFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("windpower_test")
        .WithUsername("admin")
        .WithPassword("admin")
        .Build();

    public string GetConnectionString() => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _dbContainer.StartAsync().GetAwaiter().GetResult();

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}