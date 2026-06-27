using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;

namespace TodoApi.Tests;

public class UserApiTests
{
    [Fact]
    public async Task CanCreateAUser()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();

        var client = application.CreateClient();
        var response = await client.PostAsJsonAsync("/users/register", new UserInfo { Email = "todouser@todoapp.com", Password = "@pwd" }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);

        var user = db.Users.Single();
        Assert.NotNull(user);

        Assert.Equal("todouser@todoapp.com", user.UserName);
    }

    [Fact]
    public async Task MissingUserOrPasswordReturnsBadRequest()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();

        var client = application.CreateClient();
        var response = await client.PostAsJsonAsync("/users/register", new UserInfo { Email = "todouser", Password = "" }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problemDetails);

        Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
        Assert.NotEmpty(problemDetails.Errors);
        // TODO: Follow up on the new errors
        // Assert.Equal(new[] { "The Password field is required." }, problemDetails.Errors["Password"]);

        response = await client.PostAsJsonAsync("/users/register", new UserInfo { Email = "", Password = "password" }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problemDetails);

        Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
        Assert.NotEmpty(problemDetails.Errors);
        // Assert.Equal(new[] { "The Username field is required." }, problemDetails.Errors["Username"]);
    }

    [Fact]
    public async Task MissingUsernameOrProviderKeyReturnsBadRequest()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();

        var client = application.CreateClient();
        var response = await client.PostAsJsonAsync("/users/token/Google", new ExternalUserInfo { Username = "todouser" }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problemDetails);

        Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
        Assert.NotEmpty(problemDetails.Errors);
        Assert.Equal(new[] { $"The {nameof(ExternalUserInfo.ProviderKey)} field is required." }, problemDetails.Errors[nameof(ExternalUserInfo.ProviderKey)]);

        response = await client.PostAsJsonAsync("/users/token/Google", new ExternalUserInfo { ProviderKey = "somekey" }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problemDetails);

        Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
        Assert.NotEmpty(problemDetails.Errors);
        Assert.Equal(new[] { $"The Username field is required." }, problemDetails.Errors["Username"]);
    }

    [Fact]
    public async Task CanGetATokenForValidUser()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync("todouser", "p@assw0rd1");

        var client = application.CreateClient();
        var response = await client.PostAsJsonAsync("/users/login", new UserInfo { Email = "todouser", Password = "p@assw0rd1" }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);

        var token = await response.Content.ReadFromJsonAsync<AuthToken>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(token?.Token);

        // Check that the token is indeed valid

        var req = new HttpRequestMessage(HttpMethod.Get, "/todos");
        req.Headers.Authorization = new("Bearer", token.Token);
        response = await client.SendAsync(req, TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task CanGetATokenForExternalUser()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();

        var client = application.CreateClient();

        var encryptedId = application.Services.GetRequiredService<IDataProtectionProvider>()
                                    .CreateProtector("Google")
                                    .Protect("1003");

        var response = await client.PostAsJsonAsync("/users/token/Google", new ExternalUserInfo { Username = "todouser", ProviderKey = encryptedId }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);

        var token = await response.Content.ReadFromJsonAsync<AuthToken>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(token?.Token);

        // Check that the token is indeed valid

        var req = new HttpRequestMessage(HttpMethod.Get, "/todos");
        req.Headers.Authorization = new("Bearer", token.Token);
        response = await client.SendAsync(req, TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);

        using var scope = application.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TodoUser>>();
        var user = await userManager.FindByLoginAsync("Google", "1003");
        Assert.NotNull(user);
        Assert.Equal("todouser", user.UserName);
    }

    [Fact]
    public async Task UnauthorizedForInvalidCredentials()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync("todouser", "p@assw0rd1");

        var client = application.CreateClient();
        var response = await client.PostAsJsonAsync("/users/login", new UserInfo { Email = "todouser", Password = "prd1" }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    class AuthToken
    {
        [JsonPropertyName("accessToken")]
        public string? Token { get; set; }
    }
}
