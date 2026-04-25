using Backend.Data;
using Backend.Data.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Web.Repositories.Tests
{
    [TestClass()]
    public class TaskItemRepositoryTests
    {
        private WebApplicationFactory<Program> _factory = null!;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>();
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

            var repo = new TaskItemRepository(db);
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
            Assert.IsTrue(dbItem, "Data was not commited to database.");
        }
    }
}