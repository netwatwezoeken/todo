using Aspire.Hosting.Testing;
using Microsoft.Playwright;

namespace AspireIntegrationTest;

public class PlaywrightTest
{
    [Fact]
    public async Task CreateUser()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.TodoApp_AppHost>(TestContext.Current.CancellationToken);
        
        var app = await builder.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
        await app.ResourceNotifications.WaitForResourceHealthyAsync(
            "todo-web-server",
            cts.Token);
        
        var playwright = await Playwright.CreateAsync();
        var options = new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 2000
        };
        var browser = await playwright.Chromium.LaunchAsync(options);
        var context = await browser.NewContextAsync();
        await context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
        
        var page = await context.NewPageAsync();
        var uri = app.GetEndpoint("todo-web-server");
        
        await page.GotoAsync(uri.ToString());
        
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email" })
            .FillAsync("playwright@example.com");
        
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" })
            .FillAsync("P@ssw0rd");
        
        await page.GetByRole(AriaRole.Button, new() { Name = "Create User" })
            .ClickAsync();
        
        await context.Tracing.StopAsync(new()
        {
            Path = "../../../traces/PlaywrightTest.zip"
        });
    }
}
