using AspireIntegrationTest.TestSetup;
using Microsoft.Playwright;
using Reqnroll;

namespace AspireIntegrationTest;

[Binding]
public class LoginPage(ITestOutputHelper output): BaseStepDefinition(output)
{
    [Given("user navigates to the main page")]
    public async Task GoToLoginPage()
    {
        await Page.GotoAsync(Fixture.WebUri.ToString());
    }

    [When("they fill in {string} as email")]
    public async Task FillInAsEmail(string email)
    {
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync(email);
    }

    [When("they fill in password {string}")]
    public async Task FillInPassword(string password)
    {
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(password);
    }
    
    [When("they click Create")]
    public async Task ClickCreate()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create User" }).ClickAsync();
    }
}