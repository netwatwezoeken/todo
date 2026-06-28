using System.Net.Http.Json;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using TodoApi;

namespace AspireIntegrationTest;

public class ApprovalTest
{
    [Fact]
    public async Task GetTodos()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.TodoApp_AppHost>(TestContext.Current.CancellationToken);
        var app = await builder.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
        await app.ResourceNotifications.WaitForResourceHealthyAsync(
            "todo-web-server",
            cts.Token);
        
        var connectionString = await app.GetConnectionStringAsync("Todos", cancellationToken: cts.Token);
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var db = new TodoDbContext(options);
        await db.Database.EnsureDeletedAsync(cts.Token);
        await db.Database.EnsureCreatedAsync(cts.Token);
        
        var apiClient = app.CreateHttpClient("todoapi");
        var createResponse = await apiClient.PostAsJsonAsync("/users/register", 
            new UserInfo { Email = "todo@example.com", Password = "T0psecret!" }, 
            cancellationToken: TestContext.Current.CancellationToken);
        
        var tokenResponse = await apiClient.PostAsJsonAsync("/users/login", 
            new UserInfo { Email = "todo@example.com", Password = "T0psecret!" }, 
            cancellationToken: TestContext.Current.CancellationToken);

        var token = await tokenResponse.Content.ReadFromJsonAsync<AuthToken>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(token?.Token);
        
        await CreateTodoItem(token, apiClient, "I want to do this thing tomorrow");
        await CreateTodoItem(token, apiClient, "Take out trash");
        await CreateTodoItem(token, apiClient, "Listen to this new album");
        await CreateTodoItem(token, apiClient, "Do sports");
        
        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        var todosResponse = await apiClient.SendAsync(request, 
            cancellationToken: TestContext.Current.CancellationToken);
        var response = await todosResponse.Content.ReadAsStringAsync(cts.Token);
        
        // Assert
        await VerifyJson(response);
    }

    private static async Task CreateTodoItem(AuthToken token, HttpClient apiClient, string title)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
        {
            Content = JsonContent.Create(new TodoItem { Title = title })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

        var todoResponse = await apiClient.SendAsync(request, TestContext.Current.CancellationToken);
        todoResponse.EnsureSuccessStatusCode();
    }
}
