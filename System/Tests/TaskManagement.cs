using Backend.Data.Entitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace SystemTests.Tests
{
    [TestClass]
    public class TaskManagement : BaseSystemTest
    {
        private string _taskFrontEndUrl = FrontendUrl + "/tasks";

        [TestMethod]
        public async Task CreateTask_SuccessfulFlow()
        {
            string taskName = "UC-3 Task Name";
            float taskDurationHours = 4;
            float taskDurationMinutes = 30;
            DateTime taskStartDate = new DateTime(2000, 1, 1);
            DateTime taskEndDate = new DateTime(2000, 1, 1);

            string turbineName = "UC-3 Turbine";

            string qualName = "UC-3 Qualification Name";
            string qualDescription = "UC-3 Qualification Description";
            int qualAmount = 1;

            string toolName = "UC-3 Tool Name";
            int toolStock = 3;

            await Db.Turbines.AddAsync(new Turbine(turbineName, 20, 20));
            await Db.Qualifications.AddAsync(new Qualification(qualName, qualDescription));
            await Db.Tools.AddAsync(new Tool(toolName, toolStock));
            await Db.SaveChangesAsync();

            await Page.GotoAsync(_taskFrontEndUrl);

            await Page.GetByRole(AriaRole.Button, new() { Name = "New Task" }).ClickAsync();

            var dialog = Page.GetByRole(AriaRole.Dialog);

            await dialog.GetByLabel("Name*").FillAsync(taskName);
            await dialog.Locator("#durationHours").GetByRole(AriaRole.Spinbutton).FillAsync(taskDurationHours.ToString());
            await dialog.Locator("#durationMinutes").GetByRole(AriaRole.Spinbutton).FillAsync(taskDurationMinutes.ToString());

            var startDateInput = dialog.Locator("#startDate").GetByRole(AriaRole.Combobox);
            await startDateInput.FocusAsync();
            await startDateInput.PressSequentiallyAsync(taskStartDate.ToString("MM/dd/yyyy"), new() { Delay = 15 });

            var endDateInput = dialog.Locator("#endDate").GetByRole(AriaRole.Combobox);
            await endDateInput.FocusAsync();
            await endDateInput.PressSequentiallyAsync(taskEndDate.ToString("MM/dd/yyyy"), new() { Delay = 15 });

            await dialog.GetByRole(AriaRole.Combobox, new() { Name = "Select a wind turbine!" }).ClickAsync();
            await dialog.GetByRole(AriaRole.Option, new() { Name = turbineName }).ClickAsync();

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

            var qualDialog = Page.GetByRole(AriaRole.Dialog);
            await qualDialog.GetByRole(AriaRole.Combobox, new() { Name = "Qualification" }).ClickAsync();
            await Page.GetByRole(AriaRole.Option, new() { Name = qualName }).ClickAsync();
            await qualDialog.Locator("p-inputnumber[formcontrolname='selectedQualificationAmount'] input").FillAsync(qualAmount.ToString());
            await qualDialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();
            
            var toolDialog = Page.GetByRole(AriaRole.Dialog);
            await toolDialog.GetByRole(AriaRole.Combobox, new() { Name = "Tool" }).ClickAsync(); // issue in headless
            await Page.GetByRole(AriaRole.Option, new() { Name = toolName }).ClickAsync();
            await toolDialog.Locator("p-inputnumber[formcontrolname='selectedToolAmount'] input").FillAsync(toolStock.ToString());
            await toolDialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).Last.ClickAsync();
            // .Last

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

            var row = Page.Locator("tr").Filter(new() { HasText = taskName });
            await Expect(row.GetByText(taskName)).ToBeVisibleAsync();

            TaskItem? task = await Db.Tasks.FirstOrDefaultAsync(t => t.Name == taskName);
            Assert.IsNotNull(task);
            // more checks
        }
    }
}