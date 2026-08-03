using Microsoft.EntityFrameworkCore;
using PricingService.Domain.Entities;

namespace PricingService.Infrastructure.Data;

/// <summary>
/// Database context for Pricing Service
/// </summary>
public class PricingDbContext : DbContext
{
    public PricingDbContext(DbContextOptions<PricingDbContext> options) : base(options)
    {
    }

    public DbSet<RoomPrice> RoomPrices => Set<RoomPrice>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RoomPrice configuration
        modelBuilder.Entity<RoomPrice>(entity =>
        {
            entity.ToTable("room_prices");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoomId).HasColumnName("room_id").IsRequired();
            entity.Property(e => e.RoomType).HasColumnName("room_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.BasePrice).HasColumnName("base_price").HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from").IsRequired();
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasIndex(e => e.RoomId).HasDatabaseName("idx_room_prices_room_id");
            entity.HasIndex(e => e.RoomType).HasDatabaseName("idx_room_prices_room_type");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_room_prices_is_active");
        });

        // PricingRule configuration
        modelBuilder.Entity<PricingRule>(entity =>
        {
            entity.ToTable("pricing_rules");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RuleName).HasColumnName("rule_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.RuleType).HasColumnName("rule_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Multiplier).HasColumnName("multiplier").HasPrecision(5, 2).IsRequired();
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent").HasPrecision(5, 2);
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
            entity.Property(e => e.MinNights).HasColumnName("min_nights");
            entity.Property(e => e.Priority).HasColumnName("priority").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(e => e.Conditions).HasColumnName("conditions").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasIndex(e => e.RuleType).HasDatabaseName("idx_pricing_rules_rule_type");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_pricing_rules_is_active");
            entity.HasIndex(e => e.Priority).HasDatabaseName("idx_pricing_rules_priority");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow;

        // Seed room prices for different room types
        modelBuilder.Entity<RoomPrice>().HasData(
            new RoomPrice
            {
                Id = 1,
                RoomId = 1,
                RoomType = "Standard",
                BasePrice = 100.00m,
                Currency = "EUR",
                ValidFrom = new DateTime(2026, 1, 1),
                ValidTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RoomPrice
            {
                Id = 2,
                RoomId = 2,
                RoomType = "Deluxe",
                BasePrice = 150.00m,
                Currency = "EUR",
                ValidFrom = new DateTime(2026, 1, 1),
                ValidTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RoomPrice
            {
                Id = 3,
                RoomId = 3,
                RoomType = "Suite",
                BasePrice = 250.00m,
                Currency = "EUR",
                ValidFrom = new DateTime(2026, 1, 1),
                ValidTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RoomPrice
            {
                Id = 4,
                RoomId = 4,
                RoomType = "Presidential",
                BasePrice = 500.00m,
                Currency = "EUR",
                ValidFrom = new DateTime(2026, 1, 1),
                ValidTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        // Seed pricing rules
        modelBuilder.Entity<PricingRule>().HasData(
            new PricingRule
            {
                Id = 1,
                RuleName = "Summer Peak Season",
                RuleType = PricingRuleTypes.Seasonal,
                Multiplier = 1.5m,
                DiscountPercent = null,
                ValidFrom = new DateTime(2026, 6, 1),
                ValidTo = new DateTime(2026, 8, 31),
                MinNights = null,
                Priority = 3,
                IsActive = true,
                Conditions = null,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PricingRule
            {
                Id = 2,
                RuleName = "Weekend Premium",
                RuleType = PricingRuleTypes.Weekend,
                Multiplier = 1.2m,
                DiscountPercent = null,
                ValidFrom = null,
                ValidTo = null,
                MinNights = null,
                Priority = 2,
                IsActive = true,
                Conditions = null,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PricingRule
            {
                Id = 3,
                RuleName = "Long Stay Discount",
                RuleType = PricingRuleTypes.LongStay,
                Multiplier = 0.85m,
                DiscountPercent = 15.0m,
                ValidFrom = null,
                ValidTo = null,
                MinNights = 7,
                Priority = 4,
                IsActive = true,
                Conditions = null,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PricingRule
            {
                Id = 4,
                RuleName = "Early Bird Special",
                RuleType = PricingRuleTypes.Promotional,
                Multiplier = 0.80m,
                DiscountPercent = 20.0m,
                ValidFrom = new DateTime(2026, 1, 1),
                ValidTo = new DateTime(2026, 12, 31),
                MinNights = null,
                Priority = 5,
                IsActive = true,
                Conditions = "{\"advanceBookingDays\": 30}",
                CreatedAt = now,
                UpdatedAt = now
            }
        );
    }
}
