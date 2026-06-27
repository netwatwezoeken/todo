using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Playwright;

namespace AspireIntegrationTest.TestSetup;

public class AspireTestFixture
{
    public IBrowserContext? Context;
    public IPage Page { get; private set; } = null!;
    public Uri WebUri { get; private set; } = null!;
    
    public DistributedApplication? App;

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.CloseAsync();
            await _browser!.CloseAsync();
            await Context.DisposeAsync();
        }

        if (_playwright != null) 
            _playwright.Dispose();
        if (App != null) 
            await App!.DisposeAsync();
    }
    
    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.TodoApp_AppHost>(
                args: ["DcpPublisher:RandomizePorts=false",
                    "ASPNETCORE_URLS=http://localhost:18888",
                    "ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true",
                    "UseMockedGoPay=true"],
                configureBuilder: (appOptions, hostSettings) =>
                {
                    appOptions.DisableDashboard = false;
                    appOptions.AllowUnsecuredTransport = true;
                }
            );
        
        App = await builder.BuildAsync();
        await App.StartAsync();
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
        await App.ResourceNotifications.WaitForResourceHealthyAsync(
            "todo-web-server",
            cts.Token);

        _playwright = await Playwright.CreateAsync();
        var options = new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 500
        };

        if (System.Diagnostics.Debugger.IsAttached)
        {
            options.Headless = false;
            options.SlowMo = 1000;
            options.Timeout = 120000;            
        }

        _browser = await _playwright.Chromium.LaunchAsync(options);

        WebUri = App.GetEndpoint("todo-web-server");
    }

    public async Task CreateNewContext()
    {
        if (Context != null)
        {
            await Context.DisposeAsync();
        }
        
        Context = await _browser!.NewContextAsync(new BrowserNewContextOptions()
        {   
            IgnoreHTTPSErrors = true,
        });
    
        Context.SetDefaultTimeout(60000);
        
        Page = await Context.NewPageAsync();
    }
    
    private static IBrowser? _browser;
    private static IPlaywright? _playwright;
}