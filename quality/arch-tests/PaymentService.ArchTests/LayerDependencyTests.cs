using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PaymentService.ArchTests;

// No rule for PaymentService.Domain here — it currently has zero source files (see
// services/payment-service/CLAUDE.md). A rule against an empty namespace would fail
// ArchUnitNET's "positive evaluation" check, the same trap the bookings-service version
// of this file hit with a namespace pattern that matched nothing.
public class LayerDependencyTests
{
    private const string Application = @"^PaymentService\.Application(\..*)?$";
    private const string Infrastructure = @"^PaymentService\.Infrastructure(\..*)?$";
    private const string Api = @"^PaymentService\.API(\..*)?$";

    private static readonly Architecture Architecture = new ArchUnitNET.Loader.ArchLoader()
        .LoadAssemblies(
            typeof(PaymentService.Application.Messages.PaymentStatusChanged).Assembly,
            typeof(PaymentService.Infrastructure.Occupancy.OccupancyAdapter).Assembly,
            typeof(PaymentService.API.Controllers.PaymentController).Assembly)
        .Build();

    [Fact]
    public void Application_Does_Not_Depend_On_Infrastructure_Or_Api()
    {
        Types().That().ResideInNamespaceMatching(Application)
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(Infrastructure))
            .AndShould().NotDependOnAny(Types().That().ResideInNamespaceMatching(Api))
            .Because("Application must not depend on Infrastructure or the composition root")
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
