using Bookings.Common;
using Bookings.Common.Events;

using BookingService.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Persistence
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<PricingRule> PricingRules => Set<PricingRule>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Booking>(b =>
            {
                b.HasKey(e => e.Id);

                b.OwnsOne(e => e.TotalPrice, m =>
                {
                    m.Property(p => p.Amount).HasColumnName("total_price");
                    m.Property(p => p.Currency).HasColumnName("currency")
                                                .HasMaxLength(3)
                                                .HasDefaultValue("EUR");
                });
            });

            mb.Entity<PricingRule>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.StrategyKey).IsRequired();
                e.Property(x => x.Priority).HasDefaultValue(0);
                e.HasIndex(x => x.ValidFrom);
            });

            base.OnModelCreating(mb);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var domainEvents = ChangeTracker.Entries<Entity>()
                                            .SelectMany(e => e.Entity.DomainEvents)
                                            .ToList();

            var result = await base.SaveChangesAsync(ct);

            if (domainEvents.Any())
            {
                var dispatcher = this.GetService<IDomainEventDispatcher>();   // короткий путь
                await dispatcher.DispatchAsync(domainEvents, ct);
            }

            ChangeTracker.Entries<Entity>()
                         .ToList()
                         .ForEach(e => e.Entity.ClearEvents());

            return result;
        }
    }
}
