using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

[TestClass]
public class TasksTest : PageTest
{
    private static readonly String InitialTaskName = "000_TaskName";
    private static readonly String InitialQualificationName = "Electrical Systems Basics";
    private static readonly String InitialToolName = "000_ToolPersisted";
    private static readonly int InitialDurationHours = 2;
    private static readonly int InitialDurationMinutes = 2;
    private static readonly String InitialDuration = InitialDurationHours.ToString() + " h "+ InitialDurationMinutes.ToString() + " min";

    [TestMethod]
    [Priority(1)]
    public async Task AddTask_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/tasks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Neue Aufgabe" }).ClickAsync();

        await Page.GetByLabel("Bezeichnung*").FillAsync(InitialTaskName);
        var hourIncrement = Page.Locator("#durationHours .p-inputnumber-increment-button");
        await hourIncrement.ClickAsync();
        await hourIncrement.ClickAsync();
        var minuteIncrement = Page.Locator("#durationMinutes .p-inputnumber-increment-button");
        await minuteIncrement.ClickAsync();
        await minuteIncrement.ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Weiter" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Hinzufügen" }).ClickAsync();

        var qualificationSelectTrigger = Page.Locator("p-select[formcontrolname='selectedQualificationForm']");
        await qualificationSelectTrigger.ClickAsync();
        var qualificationOverlay = Page.Locator(".p-select-overlay");
        await qualificationOverlay.GetByText(InitialQualificationName).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Weiter" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Hinzufügen" }).ClickAsync();

        var toolSelectTrigger = Page.Locator("p-select[formcontrolname='selectedToolForm']");
        await toolSelectTrigger.ClickAsync();
        var toolOverlay = Page.Locator(".p-select-overlay");
        await toolOverlay.GetByText(InitialToolName).ClickAsync();

        var dialog = Page.Locator(".p-dialog-footer");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Page.GetByLabel("Neue Aufgabe").GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        var row = Page.Locator("tr").Filter(new() { HasText = InitialTaskName });
        await Expect(row.GetByText(InitialDuration)).ToBeVisibleAsync();
    }

    [TestMethod]
    [Priority(2)]
    public async Task ModifyTask_SuccessfulFlow()
    {
        Assert.Fail();
    }

    [TestMethod]
    [Priority(3)]
    public async Task RemoveTask_SuccessfulFlow()
    {
        Assert.Fail();
    }
}