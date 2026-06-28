using Microsoft.Playwright;
using Reqnroll;

namespace AspireIntegrationTest.TestSetup;

[Binding]
public abstract class BaseStepDefinition
{
    protected IPage Page;
    protected readonly ITestOutputHelper Output;

    protected BaseStepDefinition(ITestOutputHelper output)
    {
        Output = output;
        Fixture = AspireTestRunBinding.Fixture!;
        Page = Fixture.Page!;
        Assertions.SetDefaultExpectTimeout(100_000);
    }

    protected AspireTestFixture Fixture { get; set; }
}
