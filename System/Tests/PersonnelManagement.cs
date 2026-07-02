using Backend.Data.Entitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace SystemTests.Tests
{
    [TestClass]
    public class PersonnelManagement : BaseSystemTest
    {
        private string _personnelFrontEndUrl = FrontendUrl + "/qualifications";

        [TestMethod]
        public async Task CreateEmployee_SuccessfulFlow()
        {
            string qualName = "UC-1 Qualification Name";
            string qualDesc = "UC-1 Qualification Description";

            await Page.GotoAsync(_personnelFrontEndUrl);

            await Page.GetByRole(AriaRole.Button, new() { Name = "New Qualification" }).ClickAsync();

            await Page.GetByLabel("Name*").FillAsync(qualName);
            await Page.GetByLabel("Description*").FillAsync(qualDesc);

            await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

            var row = Page.Locator("tr").Filter(new() { HasText = qualName });
            await Expect(row.GetByText(qualName)).ToBeVisibleAsync();
            await Expect(row.GetByText(qualDesc)).ToBeVisibleAsync();

            Qualification? qualification = await Db.Qualifications.FirstOrDefaultAsync(q => q.Name == qualName && q.Description == qualDesc);
            Assert.IsNotNull(qualification);
        }

        [TestMethod]
        public async Task EditEmployee_SuccessfulFlow()
        {
            string qualName = "UC-1a Qualification Name";
            string qualDesc = "UC-1a Qualification Description";

            string qualNameEdited = "UC-1a Qualification Name Edited";
            string qualDescEdited = "UC-1a Qualification Description Edited";

            await Db.Qualifications.AddAsync(new Qualification() { Name = qualName, Description = qualDesc });
            await Db.SaveChangesAsync();

            await Page.GotoAsync(_personnelFrontEndUrl);

            var row = Page.Locator("tr").Filter(new() { HasText = qualName });

            await row.Locator(".pi-pencil").ClickAsync();

            await Page.GetByLabel("Name*").FillAsync(qualNameEdited);
            await Page.GetByLabel("Description*").FillAsync(qualDescEdited);

            await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

            await Expect(row.GetByText(qualNameEdited)).ToBeVisibleAsync();
            await Expect(row.GetByText(qualDescEdited)).ToBeVisibleAsync();

            Qualification? qualification = await Db.Qualifications.FirstOrDefaultAsync(q => q.Name == qualNameEdited && q.Description == qualDescEdited);
            Assert.IsNotNull(qualification);
        }

        [TestMethod]
        public async Task DeleteEmployee_SuccessfulFlow()
        {
            string qualName = "UC-1b Qualification Name";
            string qualDesc = "UC-1b Qualification Description";

            await Db.Qualifications.AddAsync(new Qualification() { Name = qualName, Description = qualDesc });
            await Db.SaveChangesAsync();

            await Page.GotoAsync(_personnelFrontEndUrl);

            var row = Page.Locator("tr").Filter(new() { HasText = qualName });

            await row.Locator(".pi-trash").ClickAsync();

            await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

            await Expect(row.GetByText(qualName)).ToBeHiddenAsync();
            await Expect(row.GetByText(qualDesc)).ToBeHiddenAsync();

            Qualification? qualification = await Db.Qualifications.FirstOrDefaultAsync(q => q.Name == qualName && q.Description == qualDesc);
            Assert.IsNull(qualification);
        }
    }
}
