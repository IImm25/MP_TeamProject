using Backend.Data.Entitites;
using Microsoft.Playwright;

namespace SystemTests.Tests
{
    [TestClass]
    public class HavarieReport : BaseSystemTest
    {
        private string _scheduleViewFrontEndUrl = FrontendUrl + "/schedule-view";

        [TestMethod]
        public async Task CreateHavarie_SuccessfulFlow()
        {
            string havarieName = "UC-5 h_1";
            DateOnly havarieDate = new DateOnly(2026, 7, 2);

            Backend.Data.Entitites.Location w_1 = new Backend.Data.Entitites.Location("UC-5 w_1", 20.0f, 20.0f, true) { Id = 1 };
            Backend.Data.Entitites.Location w_2 = new Backend.Data.Entitites.Location("UC-5 w_2", 20.1f, 20.1f, false);

            Qualification q_1 = new Qualification("UC-5 q_1", "Qualification q_1");

            Tool to_1 = new Tool("UC-5 to_1", 5);

            Person p_1 = new Person("UC-5 p_1", "p_1");
            PersonQualification p_1_q_1 = new PersonQualification()
            {
                Person = p_1,
                PersonId = p_1.Id,
                Qualification = q_1,
                QualificationId = q_1.Id,
            };
            p_1.Qualifications.Add(p_1_q_1);

            await Db.Locations.AddAsync(w_1);
            await Db.Locations.AddAsync(w_2);

            await Db.Qualifications.AddAsync(q_1);
            await Db.Tools.AddAsync(to_1);

            await Db.Persons.AddAsync(p_1);
            await Db.PersonQualifications.AddAsync(p_1_q_1);

            await Db.SaveChangesAsync();

            // ---

            await Page.GotoAsync(_scheduleViewFrontEndUrl);

            await Page.GetByRole(AriaRole.Button, new() { Name = "Accident" }).ClickAsync();

            // copied from TaskManagement

            var dialog = Page.GetByRole(AriaRole.Dialog);

            await dialog.GetByLabel("Name*").FillAsync(havarieName);
            await dialog.Locator("#durationHours").GetByRole(AriaRole.Spinbutton).FillAsync("1");
            await dialog.Locator("#durationMinutes").GetByRole(AriaRole.Spinbutton).FillAsync("0");

            var startDateInput = dialog.Locator("#startDate").GetByRole(AriaRole.Combobox);
            await startDateInput.FocusAsync();
            await startDateInput.PressSequentiallyAsync(havarieDate.ToString("MM/dd/yyyy"), new() { Delay = 15 });

            var endDateInput = dialog.Locator("#endDate").GetByRole(AriaRole.Combobox);
            await endDateInput.FocusAsync();
            await endDateInput.PressSequentiallyAsync(havarieDate.ToString("MM/dd/yyyy"), new() { Delay = 15 });

            await dialog.GetByRole(AriaRole.Combobox, new() { Name = "Select a wind turbine!" }).ClickAsync();
            await Page.GetByRole(AriaRole.Option, new() { Name = w_2.Name }).ClickAsync();

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

            var qualDialog = Page.GetByRole(AriaRole.Dialog);
            await qualDialog.GetByRole(AriaRole.Combobox, new() { Name = "Qualification" }).ClickAsync();
            await Page.GetByRole(AriaRole.Option, new() { Name = q_1.Name }).ClickAsync();
            await qualDialog.Locator("p-inputnumber[formcontrolname='selectedQualificationAmount'] input").FillAsync("1");
            await qualDialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

            var toolDialog = Page.GetByRole(AriaRole.Dialog);
            await toolDialog.GetByRole(AriaRole.Combobox, new() { Name = "Tool" }).ClickAsync(); // issue in headless
            await Page.GetByRole(AriaRole.Option, new() { Name = to_1.Name }).ClickAsync();
            await toolDialog.Locator("p-inputnumber[formcontrolname='selectedToolAmount'] input").FillAsync("1");
            await toolDialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).Last.ClickAsync();
            // .Last

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

            // ---

            await Expect(Page.GetByText("Boat")).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Tasks" }).ClickAsync();
            await Expect(Page.GetByText(havarieName)).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Employees" }).ClickAsync();
            await Expect(Page.GetByText(p_1.Firstname)).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Tools" }).ClickAsync();
            await Expect(Page.GetByText(to_1.Name)).ToBeVisibleAsync();
        }
    }
}
