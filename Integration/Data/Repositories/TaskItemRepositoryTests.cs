using Backend.Data;
using Backend.Data.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

namespace Backend.Web.Repositories.Tests
{
    [TestClass()]
    public class TaskItemRepositoryTests
    {
        private static WindPowerFactory _factory = null!;
        private static Respawner _respawner = null!;

        [ClassInitialize]
        public static async Task ClassSetup(TestContext context)
        {
            _factory = new WindPowerFactory();
            // creates client, automatically handles migrations through EF Core
            _factory.CreateClient();

            // Respawn
            using var conn = new NpgsqlConnection(_factory.GetConnectionString());
            await conn.OpenAsync();
            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            });
        }

        [TestInitialize]
        public async Task TestSetup()
        {
            using var conn = new NpgsqlConnection(_factory.GetConnectionString());
            await conn.OpenAsync();
            await _respawner.ResetAsync(conn);
        }

        [TestMethod()]
        public async Task GivenValidTaskDtoWithQualifications_WhenCreateAsyncCalled_ThenTaskQualificationRelationShipSaved()
        {
            string expectedQualName = "qualName";
            string expectedTaskName = "taskName";

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var qual = new Qualification(expectedQualName, "1");
            db.Qualifications.Add(qual);
            await db.SaveChangesAsync();

            var repo = scope.ServiceProvider.GetRequiredService<ITaskItemRepository>(); ;
            var dto = new TaskItemCreateDto
            {
                Name = expectedTaskName,
                DurationHours = 4,
                RequiredQualificationIds = new List<int> { qual.Id },
                RequiredTools = new List<TaskToolDto>()
            };

            var result = await repo.CreateAsync(dto);

            Assert.IsNotNull(result, "Repository returned null.");
            Assert.AreEqual(expectedTaskName, result.Name, "Task name mismatch.");
            Assert.AreEqual(1, result.RequiredQualifications.Count, "Qualification relationship missing.");
            Assert.AreEqual(expectedQualName, result.RequiredQualifications.First().Qualification.Name, "Nested qualification name mismatch.");

            var dbItem = await db.Tasks.AnyAsync(t => t.Id == result.Id);
            Assert.IsTrue(dbItem, "Data was not commited to the database.");
        }

        [ClassCleanup]
        public static async Task ClassTeardown()
        {
            await _factory.DisposeAsync();
        }
    }
}