using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

[Ignore]
[TestClass]
public class SchedulerTest : PageTest
{
    private static readonly int Duration1 = 8;
    private static readonly int BoatAmount1 = 3;
    private static readonly String[] ToolNames1 = ["000_ToolPersisted1", "000_ToolPersisted2"];
    private static readonly String[] TaskNames1 = ["000_TaskPersisted1", "000_TaskPersisted2"];
    private static readonly String[] EmployeeNames1 = ["Jane Persisted", "John Persisted"];

    [TestMethod]
    [Priority(1)]
    public async Task CreateSchedule_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/scheduler");

        await Page.GetByLabel("Maximale Arbeitszeit (h)").FillAsync(Duration1.ToString());
        await Page.GetByLabel("Anzahl der Boote").FillAsync(BoatAmount1.ToString());

        await Page.GetByRole(AriaRole.Button, new() { Name = "Weiter" }).ClickAsync();

        foreach (var toolName in ToolNames1)
        {
            var toolRow = Page.Locator("tr").Filter(new() { HasText = toolName });
            await toolRow.GetByRole(AriaRole.Checkbox).CheckAsync();
        }

        await Page.GetByRole(AriaRole.Button, new() { Name = "Weiter" }).ClickAsync();

        foreach (var taskName in TaskNames1)
        {
            var taskRow = Page.Locator("tr").Filter(new() { HasText = taskName });
            await taskRow.GetByRole(AriaRole.Checkbox).CheckAsync();
        }

        await Page.GetByRole(AriaRole.Button, new() { Name = "Weiter" }).ClickAsync();

        foreach (var employeeName in EmployeeNames1)
        {
            var employeeRow = Page.Locator("tr").Filter(new() { HasText = employeeName });
            await employeeRow.GetByRole(AriaRole.Checkbox).CheckAsync();
        }

        await Page.GetByRole(AriaRole.Button, new() { Name = "Plan generieren" }).ClickAsync();

        Assert.Fail();
    }
}