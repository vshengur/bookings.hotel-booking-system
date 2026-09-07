using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookingsService.ArchTests;

// ResideInNamespace() does an exact match; ResideInNamespaceMatching() takes a regex —
// this tripped up the first version of this file (silently matched 0 types, which
// ArchUnitNET correctly refuses to treat as a passing rule).
public class LayerDependencyTests
{
    private const string Domain = @"^BookingService\.Domain(\..*)?$";
    private const string Application = @"^BookingService\.Application(\..*)?$";
    private const string Infrastructure = @"^BookingService\.Infrastructure(\..*)?$";
    private const string Api = @"^BookingService\.Api(\..*)?$";

    private static readonly Architecture Architecture = new ArchUnitNET.Loader.ArchLoader()
        .LoadAssemblies(
            typeof(BookingService.Domain.Aggregates.Booking.Booking).Assembly,
            typeof(BookingService.Application.Abstractions.IInventoryGateway).Assembly,
            typeof(BookingService.Infrastructure.Adapters.RoomServiceInventoryGateway).Assembly,
            typeof(BookingService.Api.Controllers.BookingController).Assembly)
        .Build();

    [Fact]
    public void Domain_Does_Not_Depend_On_Outer_Layers()
    {
        Types().That().ResideInNamespaceMatching(Domain)
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(Application))
            .AndShould().NotDependOnAny(Types().That().ResideInNamespaceMatching(Infrastructure))
            .AndShould().NotDependOnAny(Types().That().ResideInNamespaceMatching(Api))
            .Because("Domain is the innermost layer — nothing outside it should leak in")
            .Check(Architecture);
    }

    [Fact]
    public void Application_Does_Not_Depend_On_Infrastructure_Or_Api()
    {
        Types().That().ResideInNamespaceMatching(Application)
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(Infrastructure))
            .AndShould().NotDependOnAny(Types().That().ResideInNamespaceMatching(Api))
            .Because("Application defines abstractions (I*Gateway); Infrastructure implements them, never the reverse")
            .Check(Architecture);
    }

    [Fact]
    public void Infrastructure_Does_Not_Depend_On_Api()
    {
        Types().That().ResideInNamespaceMatching(Infrastructure)
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(Api))
            .Because("Api is the composition root; it wires Infrastructure, not the other way round")
            .Check(Architecture);
    }
}
