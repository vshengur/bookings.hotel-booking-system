using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PricingService.Infrastructure.Data;

/// <summary>
/// Design-time factory for PricingDbContext (used by EF Core migrations)
/// </summary>
public class PricingDbContextFactory : IDesignTimeDbContextFactory<PricingDbContext>
{
    public PricingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PricingDbContext>();

        // Use a connection string for design-time migrations
        // This can be overridden by environment variables or configuration
        var connectionString = "Host=localhost;Port=5432;Database=pricing_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new PricingDbContext(optionsBuilder.Options);
    }
}
