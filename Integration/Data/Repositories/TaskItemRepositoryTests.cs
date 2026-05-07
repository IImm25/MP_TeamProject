using Backend.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Web.Repositories.Tests
{
    [TestClass()]
    public class TaskItemRepositoryTests : BaseIntegrationTest
    {
        private ITaskItemRepository GetRepo()
                    => Scope.ServiceProvider.GetRequiredService<ITaskItemRepository>();

        [TestMethod()]
        public async Task GivenTaskWithRelationsInDb_WhenGetFullByIdAsyncCalled_ThenAllNavigationPropertiesAreLoaded()
        {
            string expectedQualName = "qualName";
            string expectedToolName = "toolName";
            var repo = GetRepo();

            var qual = new Qualification(expectedQualName, "");
            var tool = new Tool(expectedToolName, 1);
            Db.Qualifications.Add(qual);
            Db.Tools.Add(tool);
            await Db.SaveChangesAsync();

            var task = new TaskItem("taskName", 6);
            task.RequiredQualifications.Add(new TaskQualification { QualificationId = qual.Id, RequiredAmount = 1 });
            task.RequiredTools.Add(new TaskTool { ToolId = tool.Id, RequiredAmount = 1 });

            await repo.AddAsync(task);

            var result = await repo.GetFullByIdAsync(task.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RequiredQualifications.Count);
            Assert.IsNotNull(result.RequiredQualifications.First().Qualification);
            Assert.AreEqual(expectedQualName, result.RequiredQualifications.First().Qualification.Name);
            Assert.IsNotNull(result.RequiredTools.First().Tool);
        }

        [TestMethod()]
        public async Task GivenNonExistentId_WhenGetFullByIdAsyncCalled_ThenReturnsNull()
        {
            var repo = GetRepo();

            var result = await repo.GetFullByIdAsync(1234);

            Assert.IsNull(result, "Should return null for a non-existent Task ID.");
        }

        [TestMethod()]
        public async Task GivenTaskWithoutRelations_WhenGetFullByIdAsyncCalled_ThenReturnsTaskWithEmptyCollections()
        {
            var repo = GetRepo();
            var task = new TaskItem("noQualNoToolTask", 2);
            await repo.AddAsync(task);

            var result = await repo.GetFullByIdAsync(task.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.RequiredQualifications.Count, "Collections should be empty but initialized.");
            Assert.AreEqual(0, result.RequiredTools.Count, "Collections should be empty but initialized.");
        }

        [TestMethod()]
        public async Task GivenMultipleTasksWithRelations_WhenGetAllFullAsyncCalled_ThenReturnsAllWithProperties()
        {
            var repo = GetRepo();
            var qual = new Qualification("Safety", "Desc");
            Db.Qualifications.Add(qual);
            await Db.SaveChangesAsync();

            var task1 = new TaskItem("Task 1", 1);
            task1.RequiredQualifications.Add(new TaskQualification { QualificationId = qual.Id });

            var task2 = new TaskItem("Task 2", 2);
            task2.RequiredQualifications.Add(new TaskQualification { QualificationId = qual.Id });

            await repo.AddAsync(task1);
            await repo.AddAsync(task2);

            var result = await repo.GetAllFullAsync();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => t.RequiredQualifications.Count > 0), "All tasks should have qualifications loaded.");
            Assert.IsTrue(result.All(t => t.RequiredQualifications.First().Qualification != null), "Nested properties should be loaded for all list items.");
        }
    }
}