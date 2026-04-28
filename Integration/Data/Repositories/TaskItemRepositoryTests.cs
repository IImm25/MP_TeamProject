using Backend.Data;
using Backend.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Web.Repositories.Tests
{
    [TestClass()]
    public class TaskItemRepositoryTests : BaseIntegrationTest
    {
        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
                => await GlobalSetup(new WindPowerFactory());

        [TestMethod()]
        public async Task GivenTaskWithRelationsInDb_WhenGetFullByIdAsyncCalled_ThenAllNavigationPropertiesAreLoaded()
        {
            string expectedQualName = "qualName";
            string expectedToolName = "toolName";
            string expectedTaskName = "taskName";

            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var repo = scope.ServiceProvider.GetRequiredService<ITaskItemRepository>();

            var qual = new Qualification(expectedQualName, "");
            var tool = new Tool(expectedToolName, 1);
            db.Qualifications.Add(qual);
            db.Tools.Add(tool);

            await db.SaveChangesAsync();

            var task = new TaskItem(expectedTaskName, 6);
            task.RequiredQualifications.Add(new TaskQualification { QualificationId = qual.Id, RequiredAmount = 1 });
            task.RequiredTools.Add(new TaskTool { ToolId = tool.Id, RequiredAmount = 1 });

            await repo.AddAsync(task);

            var result = await repo.GetFullByIdAsync(task.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RequiredQualifications.Count);
            Assert.IsNotNull(result.RequiredQualifications.First().Qualification, "The Qualification was not loaded.");
            Assert.AreEqual(expectedQualName, result.RequiredQualifications.First().Qualification.Name);

            Assert.AreEqual(1, result.RequiredTools.Count);
            Assert.IsNotNull(result.RequiredTools.First().Tool, "The Tool was not loaded. Ensure the second .Include() chain is working.");
            Assert.AreEqual(expectedToolName, result.RequiredTools.First().Tool.Name);
        }

        [ClassCleanup]
        public static async Task ClassTeardown()
        {
            await Factory.DisposeAsync();
        }
    }
}