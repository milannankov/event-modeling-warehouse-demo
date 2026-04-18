namespace Warehouse.EventSourcing;

public interface IProjector
{
    string Name => GetType().Name;
    Task ProjectAsync(Event evt, CancellationToken ct = default);
    bool CanHandle(Event evt);
}
