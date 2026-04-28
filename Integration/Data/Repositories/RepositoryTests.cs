using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Data.Repositories.Tests
{
    [TestClass()]
    public class RepositoryTests : BaseIntegrationTest
    {
        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
                => await GlobalSetup(new WindPowerFactory());

        [TestMethod]
        public async Task GivenNewEntity_WhenAddAsyncCalled_ThenEntityIsPersisted()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var repo = new Repository<Qualification>(db);
            var qual = new Qualification("Test", "Test");

            var result = await repo.AddAsync(qual);

            Assert.AreNotEqual(0, result.Id, "Base AddAsync failed to assign an ID.");
            var exists = await db.Qualifications.AnyAsync(q => q.Id == result.Id);
            Assert.IsTrue(exists, "Base AddAsync did not commit the transaction to Postgres.");
        }
    }
}