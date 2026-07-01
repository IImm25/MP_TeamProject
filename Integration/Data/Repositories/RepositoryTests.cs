using Backend.Data.Entitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Data.Repositories.Tests
{
    [TestClass]
    public class RepositoryTests : BaseIntegrationTest
    {
        private IRepository<Qualification> _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            _repo = Scope.ServiceProvider.GetRequiredService<IRepository<Qualification>>();
        }

        [TestMethod]
        public async Task GivenNewEntity_WhenAddAsyncCalled_ThenEntityIsPersisted()
        {
            var qual = new Qualification("qualName", "qualDesc");

            var id = await _repo.AddAsync(qual);

            Assert.AreNotEqual(0, id);
            Assert.IsTrue(await Db.Qualifications.AnyAsync(q => q.Id == id));
        }

        [TestMethod]
        public async Task GivenExistingId_WhenGetByIdAsyncCalled_ThenReturnsCorrectEntity()
        {
            var qual = new Qualification("qualName", "qualDesc");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(qual.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(qual.Name, result.Name);
        }

        [TestMethod]
        public async Task GivenExistingEntity_WhenUpdateAsyncCalled_ThenChangesArePersisted()
        {
            var qual = new Qualification("qualName", "qualDesc");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();

            qual.Name = "qualName2";
            await _repo.UpdateAsync(qual);

            var dbQual = await Db.Qualifications.AsNoTracking().FirstOrDefaultAsync(q => q.Id == qual.Id);
            Assert.AreEqual("qualName2", dbQual?.Name);
        }

        [TestMethod]
        public async Task GivenExistingId_WhenDeleteAsyncCalled_ThenReturnsTrueAndRemovesEntity()
        {
            var qual = new Qualification("qualName", "qualDesc");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();

            var result = await _repo.DeleteAsync(qual.Id);

            Assert.IsTrue(result, "DeleteAsync should return true when item exists.");
            Assert.IsFalse(await Db.Qualifications.AnyAsync(q => q.Id == qual.Id), "Entity should be removed from database.");
        }

        [TestMethod]
        public async Task GivenNonExistentId_WhenDeleteAsyncCalled_ThenReturnsFalse()
        {
            var result = await _repo.DeleteAsync(9999);

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

            var results = await _repo.GetAllAsync();

            Assert.AreEqual(2, results.Count);
        }
    }
}