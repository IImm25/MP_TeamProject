[TestClass]
public class TestingInfrastructure
{
    public static WindPowerFactory Factory = null!;

    [AssemblyInitialize]
    public static async Task AssemblyInit(TestContext context)
    {
        Factory = new WindPowerFactory();
        await Factory.CreateClient().GetAsync("/");
    }
}