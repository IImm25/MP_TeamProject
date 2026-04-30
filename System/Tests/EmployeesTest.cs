using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

[TestClass]
public class EmployeesTest : PageTest
{
    private static readonly String InitialFirstName = "John";
    private static readonly String InitialLastName = "Smith";
    private static readonly String InitialName = InitialFirstName + " " + InitialLastName;
    private static readonly String ChangedLastName = "Marley";
    private static readonly String ChangedName = InitialFirstName + " " + ChangedLastName;
    private static readonly String QualificationName1 = "Electrical Systems Basics";
    private static readonly String QualificationName2 = "High Voltage Safety";

    [TestMethod]
    [Priority(1)]
    public async Task AddEmployee_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/employees");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Neuer Mitarbeiter" }).ClickAsync();

        await Page.GetByLabel("Vorname*").FillAsync(InitialFirstName);
        await Page.GetByLabel("Nachname*").FillAsync(InitialLastName);

        var multiSelect = Page.Locator("p-multiselect[formcontrolname='qualifications']");
        await multiSelect.ClickAsync();
        var overlay = Page.Locator(".p-multiselect-overlay");
        await overlay.GetByText(QualificationName1).ClickAsync();
        await Page.Keyboard.PressAsync("Escape");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        var row = Page.Locator("tr").Filter(new() { HasText = InitialName });
        await Expect(row.GetByText(QualificationName1)).ToBeVisibleAsync();
    }

    [TestMethod]
    [Priority(2)]
    public async Task ModifyEmployee_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/employees");

        var row = Page.Locator("tr").Filter(new() { HasText = InitialName });
        await row.Locator(".pi-pencil").ClickAsync();

        await Page.GetByLabel("Vorname*").FillAsync(InitialFirstName);
        await Page.GetByLabel("Nachname*").FillAsync(ChangedLastName);

        var multiSelect = Page.Locator("p-multiselect[formcontrolname='qualifications']");
        await multiSelect.ClickAsync();
        var overlay = Page.Locator(".p-multiselect-overlay");
        await overlay.GetByText(QualificationName1).ClickAsync();
        await overlay.GetByText(QualificationName2).ClickAsync();
        await Page.Keyboard.PressAsync("Escape");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        row = Page.Locator("tr").Filter(new() { HasText = ChangedName });
        await Expect(row.GetByText(QualificationName2)).ToBeVisibleAsync();
    }

    [TestMethod]
    [Priority(3)]
    public async Task RemoveEmployee_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/employees");

        var row = Page.Locator("tr").Filter(new() { HasText = ChangedName });
        await row.Locator(".pi-trash").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ChangedLastName }))
        .ToBeHiddenAsync(new() { Timeout = 1000 });
    }
}