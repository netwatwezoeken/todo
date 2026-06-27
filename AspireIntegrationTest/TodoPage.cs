using AspireIntegrationTest.TestSetup;
using Microsoft.Playwright;
using Reqnroll;

namespace AspireIntegrationTest;

[Binding]
public class TodoPage(ITestOutputHelper output): BaseStepDefinition(output)
{
    [Then("user is looged in")]
    public async Task LoggedIn()
    {
        await Assertions.Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();
    }
}