using Microsoft.Playwright;

namespace SystemTests.Tests
{
    [TestClass]
    public class WeatherReport : BaseSystemTest
    {
        private string _scheduleViewFrontEndUrl = FrontendUrl + "/schedule-view";
    }
}
