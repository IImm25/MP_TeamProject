using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Data.Repositories.Tests
{
    [TestClass()]
    public class RepositoryTests : BaseIntegrationTest
    {
        private IRepository<Qualification> GetRepo()
            => Scope.ServiceProvider.GetRequiredService<IRepository<Qualification>>();

        [TestMethod]
        public async Task GivenNewEntity_WhenAddAsyncCalled_ThenEntityIsPersisted()
        {
            var repo = GetRepo();
            var qual = new Qualification("qualName", "qualDesc");

            var result = await repo.AddAsync(qual);

            Assert.AreNotEqual(0, result.Id);
            var exists = await Db.Qualifications.AnyAsync(q => q.Id == result.Id);
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task GivenExistingId_WhenGetByIdAsyncCalled_ThenReturnsCorrectEntity()
        {
            var qual = new Qualification("qualName", "qualDesc");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();
            var repo = GetRepo();

            var result = await repo.GetByIdAsync(qual.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(qual.Name, result.Name);
        }

        [TestMethod]
        public async Task GivenExistingEntity_WhenUpdateAsyncCalled_ThenChangesArePersisted()
        {
            var qual = new Qualification("qualName", "qualDesc");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();
            var repo = GetRepo();

            qual.Name = "qualName2";
            await repo.UpdateAsync(qual);

            var dbQual = await Db.Qualifications.AsNoTracking().FirstOrDefaultAsync(q => q.Id == qual.Id);
            Assert.AreEqual("qualName2", dbQual?.Name);
        }

        [TestMethod]
        public async Task GivenExistingId_WhenDeleteAsyncCalled_ThenReturnsTrueAndRemovesEntity()
        {
            var qual = new Qualification("qualName", "qualDesc");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();
            var repo = GetRepo();

            var result = await repo.DeleteAsync(qual.Id);

            Assert.IsTrue(result, "DeleteAsync should return true when item exists.");
            var exists = await Db.Qualifications.AnyAsync(q => q.Id == qual.Id);
            Assert.IsFalse(exists, "Entity should be removed from database.");
        }

        [TestMethod]
        public async Task GivenNonExistentId_WhenDeleteAsyncCalled_ThenReturnsFalse()
        {
            var repo = GetRepo();

            var result = await repo.DeleteAsync(9999);

            Assert.IsFalse(result, "DeleteAsync should return false for invalid IDs.");
        }

        [TestMethod]
        public async Task WhenGetAllAsyncCalled_ThenReturnsAllEntities()
        {
            Db.Qualifications.AddRange(
                new Qualification("qualName1", "qualDesc1"),
                new Qualification("qualName2", "qualDesc2")
            );
            await Db.SaveChangesAsync();
            var repo = GetRepo();

            var results = await repo.GetAllAsync();

            Assert.AreEqual(2, results.Count);
        }
    }
}
