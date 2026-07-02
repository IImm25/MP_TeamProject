using Backend.Data.Entitites;
using Microsoft.Playwright;
using System.Security.Cryptography.X509Certificates;

namespace SystemTests.Tests
{
    [TestClass]
    public class Scheduler : BaseSystemTest
    {
        private string _schedulerFrontEndUrl = FrontendUrl + "/scheduler";

        [TestMethod]
        public async Task CreateSchedule_SuccessfulFlow()
        {
            // Convers UC-4 Normal flow using GMPL TC-01 as example

            Backend.Data.Entitites.Location w_1 = new Backend.Data.Entitites.Location("UC-4 w_1", 20.0f, 20.0f, true) { Id = 1 }; // harbor needs id = 1
            Backend.Data.Entitites.Location w_2 = new Backend.Data.Entitites.Location("UC-4 w_2", 20.1f, 20.1f, false);

            Qualification q_1 = new Qualification("UC-4 q_1", "Qualification q_1");

            Tool to_1 = new Tool("UC-4 to_1", 5);

            PlanBoat boat = new PlanBoat();

            Person p_1 = new Person("UC-4 p_1", "p_1");
            PersonQualification p_1_q_1 = new PersonQualification()
            {
                Person = p_1,
                PersonId = p_1.Id,
                Qualification = q_1,
                QualificationId = q_1.Id,
            };
            p_1.Qualifications.Add(p_1_q_1);

            TaskQualification ta_1_q_1 = new TaskQualification()
            {
                RequiredAmount = 1,
                Qualification = q_1,
                QualificationId = q_1.Id
            };

            TaskTool ta_1_to_1 = new TaskTool()
            {
                RequiredAmount = 1,
                Tool = to_1,
                ToolId = to_1.Id
            };

            TaskItem ta_1 = new TaskItem("UC-4 ta_1", 2.0f, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 10));

            ta_1.Location = w_2;
            ta_1.LocationId = w_2.Id;
            ta_1.RequiredQualifications.Add(ta_1_q_1);
            ta_1.RequiredTools.Add(ta_1_to_1);

            await Db.Locations.AddAsync(w_1);
            await Db.Locations.AddAsync(w_2);

            await Db.Qualifications.AddAsync(q_1);
            await Db.Tools.AddAsync(to_1);

            await Db.Persons.AddAsync(p_1);
            await Db.PersonQualifications.AddAsync(p_1_q_1);

            await Db.TaskQualifications.AddAsync(ta_1_q_1);
            await Db.TaskTools.AddAsync(ta_1_to_1);

            await Db.Tasks.AddAsync(ta_1);

            await Db.SaveChangesAsync();

            // ---

            await Page.GotoAsync(_schedulerFrontEndUrl);

            await Page.Locator("#maxWorkHours").GetByRole(AriaRole.Spinbutton).FillAsync("8");
            await Page.Locator("#boatNumber").GetByRole(AriaRole.Spinbutton).FillAsync("1");
            await Page.GetByRole(AriaRole.Combobox, new() { Name = "MM/DD/YYYY" }).FillAsync(ta_1.ExecutionIntervalStart.ToString("MM/dd/yyyy"));
            await Page.Locator("#boatSpeed").GetByRole(AriaRole.Spinbutton).FillAsync("36");

            await Page.GetByRole(AriaRole.Button, new() { Name = "Generate Plan" }).ClickAsync();

            // ---

            await Expect(Page.GetByText("Boat")).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Tasks" }).ClickAsync();
            await Expect(Page.GetByText(ta_1.Name)).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Employees" }).ClickAsync();
            await Expect(Page.GetByText(p_1.Firstname)).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Tools" }).ClickAsync();
            await Expect(Page.GetByText(to_1.Name)).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task CreateSchedule_UnsuccessfulFlow()
        {
            // same TC as CreateSchedule_SuccessfulFlow, adjusted working time to force error

            Backend.Data.Entitites.Location w_1 = new Backend.Data.Entitites.Location("UC-4a w_1", 20.0f, 20.0f, true) { Id = 1 };
            Backend.Data.Entitites.Location w_2 = new Backend.Data.Entitites.Location("UC-4a w_2", 20.1f, 20.1f, false);

            Qualification q_1 = new Qualification("UC-4a q_1", "Qualification q_1");

            Tool to_1 = new Tool("UC-4a to_1", 5);

            Person p_1 = new Person("UC-4a p_1", "p_1");
            PersonQualification p_1_q_1 = new PersonQualification()
            {
                Person = p_1,
                PersonId = p_1.Id,
                Qualification = q_1,
                QualificationId = q_1.Id,
            };
            p_1.Qualifications.Add(p_1_q_1);

            TaskQualification ta_1_q_1 = new TaskQualification()
            {
                RequiredAmount = 1,
                Qualification = q_1,
                QualificationId = q_1.Id
            };

            TaskTool ta_1_to_1 = new TaskTool()
            {
                RequiredAmount = 1,
                Tool = to_1,
                ToolId = to_1.Id
            };

            TaskItem ta_1 = new TaskItem("UC-4a ta_1", 8.0f, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 10));

            ta_1.Location = w_2;
            ta_1.LocationId = w_2.Id;
            ta_1.RequiredQualifications.Add(ta_1_q_1);
            ta_1.RequiredTools.Add(ta_1_to_1);

            await Db.Locations.AddAsync(w_1);
            await Db.Locations.AddAsync(w_2);

            await Db.Qualifications.AddAsync(q_1);
            await Db.Tools.AddAsync(to_1);

            await Db.Persons.AddAsync(p_1);
            await Db.PersonQualifications.AddAsync(p_1_q_1);

            await Db.TaskQualifications.AddAsync(ta_1_q_1);
            await Db.TaskTools.AddAsync(ta_1_to_1);

            await Db.Tasks.AddAsync(ta_1);

            await Db.SaveChangesAsync();

            // ---

            await Page.GotoAsync(_schedulerFrontEndUrl);

            await Page.Locator("#maxWorkHours").GetByRole(AriaRole.Spinbutton).FillAsync("4");
            await Page.Locator("#boatNumber").GetByRole(AriaRole.Spinbutton).FillAsync("1");
            await Page.GetByRole(AriaRole.Combobox, new() { Name = "MM/DD/YYYY" }).FillAsync(ta_1.ExecutionIntervalStart.ToString("MM/dd/yyyy"));
            await Page.Locator("#boatSpeed").GetByRole(AriaRole.Spinbutton).FillAsync("36");

            await Page.GetByRole(AriaRole.Button, new() { Name = "Generate Plan" }).ClickAsync();

            // ---

            await Expect(Page.GetByText("Error generating plan")).ToBeVisibleAsync();
        }
    }
}
