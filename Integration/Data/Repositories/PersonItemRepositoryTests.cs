using Microsoft.Extensions.DependencyInjection;

namespace Backend.Data.Repositories.Tests
{
    [TestClass()]
    public class PersonRepositoryTests : BaseIntegrationTest
    {
        private IPersonRepository GetRepo()
            => Scope.ServiceProvider.GetRequiredService<IPersonRepository>();

        [TestMethod()]
        public async Task GivenPersonWithQualifications_WhenGetFullByIdAsyncCalled_ThenQualificationsAreLoaded()
        {
            var repo = GetRepo();

            var qual = new Qualification("qualName", "qualDesc");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();

            var person = new Person("Max", "M");
            person.Qualifications.Add(new PersonQualification { QualificationId = qual.Id });

            await repo.AddAsync(person);

            var result = await repo.GetFullByIdAsync(person.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Max", result.Firstname);
            Assert.AreEqual(1, result.Qualifications.Count, "PersonQualification link was not found.");

            var nestedQual = result.Qualifications.First().Qualification;
            Assert.IsNotNull(nestedQual, "The actual Qualification entity was not loaded.");
            Assert.AreEqual("qualName", nestedQual.Name);
        }

        [TestMethod()]
        public async Task GivenPersonWithoutQualifications_WhenGetFullByIdAsyncCalled_ThenReturnsPersonWithEmptyList()
        {
            var repo = GetRepo();
            var person = new Person("Max", "M");
            await repo.AddAsync(person);

            var result = await repo.GetFullByIdAsync(person.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Qualifications.Count, "Expected empty list for person with no qualifications.");
        }

        [TestMethod()]
        public async Task GivenInvalidPersonId_WhenGetFullByIdAsyncCalled_ThenReturnsNull()
        {
            var result = await GetRepo().GetFullByIdAsync(12345);

            Assert.IsNull(result);
        }

        [TestMethod()]
        public async Task WhenGetAllFullAsyncCalled_ThenReturnsAllPeopleWithTheirSkillsets()
        {
            var repo = GetRepo();
            var qual = new Qualification("qualName1", "qualDesc1");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();

            var p1 = new Person("Max1", "M");
            p1.Qualifications.Add(new PersonQualification { QualificationId = qual.Id });

            var p2 = new Person("Max2", "M");
            p2.Qualifications.Add(new PersonQualification { QualificationId = qual.Id });

            await repo.AddAsync(p1);
            await repo.AddAsync(p2);

            var results = await repo.GetAllFullAsync();

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(p => p.Qualifications.Any()), "All people should have their qualification lists populated.");
            Assert.IsTrue(results.All(p => p.Qualifications.First().Qualification != null), "Nested Qualifications were not loaded in the list view.");
        }

        [ClassCleanup]
        public static async Task ClassTeardown()
        {
            if (Factory != null)
                await Factory.DisposeAsync();
        }
    }
}