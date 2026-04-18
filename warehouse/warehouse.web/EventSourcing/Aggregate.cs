namespace Warehouse.EventSourcing;

public abstract class Aggregate
{
    private readonly List<Event> _uncommittedEvents = [];

    public string StreamId { get; protected set; } = default!;
    public long StreamSequenceNumber { get; private set; } = -1;

    public IReadOnlyList<Event> UncommittedEvents => _uncommittedEvents;

    public void Load(IEnumerable<Event> history)
    {
        foreach (var evt in history)
        {
            Apply(evt);
            StreamSequenceNumber = evt.StreamSequenceNumber;
        }
    }

    protected void RaiseEvent(Event evt)
    {
        Apply(evt);
        _uncommittedEvents.Add(evt);
    }

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected abstract void Apply(Event evt);
}
