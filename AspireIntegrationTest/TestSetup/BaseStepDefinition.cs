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

    public AspireTestFixture Fixture { get; set; }
    
    /// <summary>
    /// Wait for the predicate to return a certain count or fail the assert
    /// </summary>
    /// <param name="expectedCount"></param>
    /// <param name="predicate"></param>
    /// <param name="timeout">Maximum time to wait before failing (default is 1000ms)</param>
    protected async Task WaitForAmount(int expectedCount, Func<Task<int>> predicate,  int timeout = 10000)
    {
        var timer = timeout;
        while (expectedCount != await predicate())
        {
            Thread.Sleep(100);
            timer -= 100;
            if (timer < 0)
            {
                Output.WriteLine($"Expected {expectedCount} but never got it. Last actual result was {await predicate()}");
                Assert.Fail();
            }
        }
    }
    
    /// <summary>
    /// Wait for the predicate to return at least certain count or fail the assert
    /// </summary>
    /// <param name="expectedCount"></param>
    /// <param name="predicate"></param>
    /// <param name="timeout">Maximum time to wait before failing (default is 1000ms)</param>
    protected async Task WaitForAtLeastAmount(int expectedCount, Func<Task<int>> predicate,  int timeout = 10000)
    {
        var timer = timeout;
        while (expectedCount > await predicate())
        {
            Thread.Sleep(100);
            timer -= 100;
            if (timer < 0)
            {
                Output.WriteLine($"Expected at least {expectedCount} but never got it. Last actual result was {await predicate()}");
                Assert.Fail();
            }
        }
    }
}
