using Microsoft.EntityFrameworkCore.Storage;
using PricingService.Domain.Interfaces;
using PricingService.Infrastructure.Data;

namespace PricingService.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PricingDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(PricingDbContext context, IRoomPriceRepository roomPrices, IPricingRuleRepository pricingRules)
    {
        _context = context;
        RoomPrices = roomPrices;
        PricingRules = pricingRules;
    }

    public IRoomPriceRepository RoomPrices { get; }
    public IPricingRuleRepository PricingRules { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
