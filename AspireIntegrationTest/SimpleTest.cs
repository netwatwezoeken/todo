using System.Net.Http.Json;
using Aspire.Hosting.Testing;

namespace AspireIntegrationTest;

public class SimpleTest
{
    [Fact]
    public async Task CreateTodo()
    {
        //Simple
        //var builder = await DistributedApplicationTestingBuilder
        //    .CreateAsync<Projects.TodoApp_AppHost>(TestContext.Current.CancellationToken);
        
        //For Dashboard access
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.TodoApp_AppHost>(args: ["DcpPublisher:RandomizePorts=false",
                    "ASPNETCORE_URLS=http://localhost:18888",
                    "ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true"], configureBuilder: (appOptions, hostSettings) =>
                {
                    appOptions.DisableDashboard = false;
                    appOptions.AllowUnsecuredTransport = true;
                }
, cancellationToken: TestContext.Current.CancellationToken);
        
        var app = await builder.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
        await app.ResourceNotifications.WaitForResourceHealthyAsync(
            "todo-web-server",
            cts.Token);
        
        var apiClient = app.CreateHttpClient("todoapi");
        var createResponse = await apiClient.PostAsJsonAsync("/users/register", 
            new UserInfo { Email = "todo@example.com", Password = "T0psecret!" }, 
            cancellationToken: TestContext.Current.CancellationToken);
        
        var tokenResponse = await apiClient.PostAsJsonAsync("/users/login", 
            new UserInfo { Email = "todo@example.com", Password = "T0psecret!" }, 
            cancellationToken: TestContext.Current.CancellationToken);

        var token = await tokenResponse.Content.ReadFromJsonAsync<AuthToken>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(token?.Token);
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
        {
            Content = JsonContent.Create(new TodoItem { Title = "I want to do this thing tomorrow" })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

        var todoResponse = await apiClient.SendAsync(request, TestContext.Current.CancellationToken);
        todoResponse.EnsureSuccessStatusCode();
        
        await Task.Delay(millisecondsDelay: 100, cancellationToken: TestContext.Current.CancellationToken);
    }
}
