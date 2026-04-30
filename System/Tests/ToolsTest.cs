using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

[Ignore]
[TestClass]
public class ToolsTest : PageTest
{
    private static readonly String InitialToolName = "000_ToolName1";
    private static readonly String ChangedToolName = "000_ToolName2";
    private static readonly int InitialToolAmount = 2;
    private static readonly int ChangedToolAmount = 3;


    [TestMethod]
    [Priority(1)]
    public async Task AddTool_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/tools");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Neues Werkzeug" }).ClickAsync();

        await Page.GetByLabel("Bezeichnung*").FillAsync(InitialToolName);
        var incrementBtn = Page.Locator("#availableStock .p-inputnumber-increment-button");
        await incrementBtn.ClickAsync();
        await incrementBtn.ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        var row = Page.Locator("tr").Filter(new() { HasText = InitialToolName });
        await Expect(row.GetByText(InitialToolAmount.ToString())).ToBeVisibleAsync();
    }

    [TestMethod]
    [Priority(2)]
    public async Task ModifyTool_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/tools");

        var row = Page.Locator("tr").Filter(new() { HasText = InitialToolName });
        await row.Locator(".pi-pencil").ClickAsync();

        await Page.GetByLabel("Bezeichnung*").FillAsync(ChangedToolName);
        var incrementBtn = Page.Locator("#availableStock .p-inputnumber-increment-button");
        await incrementBtn.ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        row = Page.Locator("tr").Filter(new() { HasText = ChangedToolName });
        await Expect(row.GetByText(ChangedToolAmount.ToString())).ToBeVisibleAsync();
    }

    [TestMethod]
    [Priority(3)]
    public async Task RemoveTool_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/tools");

        var row = Page.Locator("tr").Filter(new() { HasText = ChangedToolName });
        await row.Locator(".pi-trash").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ChangedToolName }))
        .ToBeHiddenAsync(new() { Timeout = 1000 });
    }
}