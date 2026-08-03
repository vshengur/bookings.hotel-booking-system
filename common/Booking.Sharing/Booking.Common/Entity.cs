namespace Bookings.Common;

public abstract class Entity
{
    private readonly List<IDomainEvent> _events = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();

    protected void AddEvent(IDomainEvent @event) => _events.Add(@event);
    public void ClearEvents() => _events.Clear();
}

public interface IDomainEvent { }
public interface IAggregateRoot { }