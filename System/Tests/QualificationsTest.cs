using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

[Ignore]
[TestClass]
public class QualificationTests : PageTest
{
    private static readonly String InitialQualificationName = "000_QualificationName1";
    private static readonly String ChangedQualificationName = "000_QualificationName2";
    private static readonly String QualificationDesc = "Qualification desc.";


    [TestMethod]
    [Priority(1)]
    public async Task AddQualification_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/qualifications");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Neue Qualifikation" }).ClickAsync();

        await Page.GetByLabel("Bezeichnung*").FillAsync(InitialQualificationName);
        await Page.GetByLabel("Beschreibung*").FillAsync(QualificationDesc);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Expect(Page.GetByText(InitialQualificationName)).ToBeVisibleAsync(new() { Timeout = 1000 });
    }

    [TestMethod]
    [Priority(2)]
    public async Task ModifyQualification_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/qualifications");

        var row = Page.Locator("tr").Filter(new() { HasText = InitialQualificationName });
        await row.Locator(".pi-pencil").ClickAsync();

        await Page.GetByLabel("Bezeichnung*").FillAsync(ChangedQualificationName);
        await Page.GetByLabel("Beschreibung*").FillAsync(QualificationDesc);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = InitialQualificationName })).ToBeHiddenAsync(new() { Timeout = 1000 });
        await Expect(Page.GetByText(ChangedQualificationName)).ToBeVisibleAsync(new() { Timeout = 1000 });
    }

    [TestMethod]
    [Priority(3)]
    public async Task RemoveQualification_SuccessfulFlow()
    {
        await Page.GotoAsync($"http://localhost:4200/qualifications");

        var row = Page.Locator("tr").Filter(new() { HasText = ChangedQualificationName });
        await row.Locator(".pi-trash").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ChangedQualificationName }))
        .ToBeHiddenAsync(new() { Timeout = 1000 });
    }
}