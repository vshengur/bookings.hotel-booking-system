namespace BookingService.Application.FunctionalTests;

using static BookingService.Application.FunctionalTests.Testing;

[TestFixture]
public abstract class BaseTestFixture
{
    [SetUp]
    public async Task TestSetUp()
    {
        await ResetState();
    }
}
