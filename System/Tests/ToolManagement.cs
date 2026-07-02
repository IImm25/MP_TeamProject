using Backend.Data.Entitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace SystemTests.Tests
{
    [TestClass]
    [Ignore]
    public class ToolManagement : BaseSystemTest
    {
        private string _toolFrontEndUrl = FrontendUrl + "/tools";

        [TestMethod]
        public async Task CreateTool_SuccessfulFlow()
        {
            string toolName = "UC-2 Tool Name";
            int toolStock = 3;

            await Page.GotoAsync(_toolFrontEndUrl);

            await Page.GetByRole(AriaRole.Button, new() { Name = "New Tool" }).ClickAsync();

            await Page.GetByLabel("Name*").FillAsync(toolName);
            await Page.Locator("#availableStock").GetByRole(AriaRole.Spinbutton).FillAsync(toolStock.ToString());

            await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

            var row = Page.Locator("tr").Filter(new() { HasText = toolName });
            await Expect(row.GetByText(toolName)).ToBeVisibleAsync();
            await Expect(row.GetByText(toolStock.ToString())).ToBeVisibleAsync();

            Tool? tool = await Db.Tools.FirstOrDefaultAsync(t => t.Name == toolName && t.AvailableStock == toolStock);
            Assert.IsNotNull(tool);
        }

        [TestMethod]
        public async Task EditTool_SuccessfulFlow()
        {
            string toolName = "UC-2a Tool Name";
            int toolStock = 3;

            string toolNameEdited = "UC-2a Tool Name Edited";
            int toolStockEdited = 4;

            await Db.Tools.AddAsync(new Tool() { Name = toolName, AvailableStock = toolStock });
            await Db.SaveChangesAsync();

            await Page.GotoAsync(_toolFrontEndUrl);

            var row = Page.Locator("tr").Filter(new() { HasText = toolName });

            await row.Locator(".pi-pencil").ClickAsync();

            await Page.GetByLabel("Name*").FillAsync(toolNameEdited);
            await Page.Locator("#availableStock").GetByRole(AriaRole.Spinbutton).FillAsync(toolStockEdited.ToString());

            await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

            await Expect(row.GetByText(toolNameEdited)).ToBeVisibleAsync();
            await Expect(row.GetByText(toolStockEdited.ToString())).ToBeVisibleAsync();

            Tool? tool = await Db.Tools.FirstOrDefaultAsync(t => t.Name == toolNameEdited && t.AvailableStock == toolStockEdited);
            Assert.IsNotNull(tool);
        }

        [TestMethod]
        public async Task DeleteTool_SuccessfulFlow()
        {
            string toolName = "UC-2b Tool Name";
            int toolStock = 3;

            await Db.Tools.AddAsync(new Tool() { Name = toolName, AvailableStock = toolStock });
            await Db.SaveChangesAsync();

            await Page.GotoAsync(_toolFrontEndUrl);

            var row = Page.Locator("tr").Filter(new() { HasText = toolName });

            await row.Locator(".pi-trash").ClickAsync();

            await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

            await Expect(row.GetByText(toolName)).ToBeHiddenAsync();
            await Expect(row.GetByText(toolStock.ToString())).ToBeHiddenAsync();

            Tool? tool = await Db.Tools.FirstOrDefaultAsync(t => t.Name == toolName && t.AvailableStock == toolStock);
            Assert.IsNull(tool);
        }
    }
}
