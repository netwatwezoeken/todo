using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using TodoApi;

namespace AspireIntegrationTest.TestSetup;

[Binding]
public class AspireTestRunBinding
{
    public static AspireTestFixture? Fixture { get; private set; }

    [AfterTestRun]
    public static async Task After()
    {
        if (Fixture == null) return;
        await Fixture.DisposeAsync();
    }

    [BeforeScenario]
    public async Task BeforeScenario(ScenarioContext scenarioContext)
    {
        if (!scenarioContext.ScenarioInfo.Tags.Contains("integration")) return;

        if (Fixture == null)
        {
            InstallPlaywright();
            Fixture = new AspireTestFixture();
            await Fixture.InitializeAsync();
            
            var connectionString = await Fixture.App!.GetConnectionStringAsync("Todos");
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            var db = new TodoDbContext(options);
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }
        await Fixture.CreateNewContext();

        await Fixture.Context!.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }


    [AfterScenario]
    public async Task AfterScenario(ScenarioContext scenarioContext, FeatureContext featureContext)
    {
        if (!scenarioContext.ScenarioInfo.Tags.Contains("integration")) return;

        if (Fixture == null) return;

        var feature = featureContext.FeatureInfo.Title;
        var scenario = scenarioContext.ScenarioInfo.Title;
        var filename = $"{feature} - {scenario}";
        var path = $"../../../traces/{filename}.zip";
        await Fixture.Context!.Tracing.StopAsync(new()
        {
            Path = path
        });
    }

    private static void InstallPlaywright()
    {
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        if (exitCode != 0)
            throw new Exception("Failed to install playwright");
    }
}