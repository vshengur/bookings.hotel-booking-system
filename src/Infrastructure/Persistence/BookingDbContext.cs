using Bookings.Common;
using Bookings.Common.Events;

using BookingService.Domain.Aggregates.Booking;

using MassTransit;

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
        public DbSet<BookingLineItem> BookingLineItems => Set<BookingLineItem>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Booking>(b =>
            {
                b.HasKey(e => e.Id);

                b.Ignore(b => b.TotalPrice);

                b.HasMany(x => x.Items)
                    .WithOne()
                    .HasForeignKey(li => li.BookingId).OnDelete(DeleteBehavior.Cascade);
            });
            mb.Entity<BookingLineItem>(li =>
            {
                li.HasKey(e => e.Id);

                li.OwnsOne(e => e.PricePerNight, m =>
                {
                    m.Property(p => p.Amount)
                        .HasColumnName("ppn_amount")
                        .HasColumnType("numeric(18,2)");
                    m.Property(p => p.Currency)
                        .HasColumnName("ppn_currency")
                        .HasMaxLength(3)
                        .HasDefaultValue("EUR");
                });

                li.HasIndex(x => new { x.RoomId, x.BookingId });
            });

            // ── MassTransit EF Outbox/Inbox ──
            mb.AddOutboxMessageEntity();
            mb.AddOutboxStateEntity();
            mb.AddInboxStateEntity();   // если используете Inbox (идемпотентность потребителей)

            base.OnModelCreating(mb);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var domainEvents = ChangeTracker.Entries<Entity>()
                                            .SelectMany(e => e.Entity.DomainEvents)
                                            .ToList();

            var result = await base.SaveChangesAsync(ct);

            if (domainEvents.Count != 0)
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
