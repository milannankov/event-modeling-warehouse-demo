using Warehouse.Slices.LowStockAlert;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.LowStockAlert;

public class RaiseLowStockAlertCommandHandlerTests
{
    [Fact]
    public async Task Handle_AppendsLowStockAlertRaisedEventToOwnStream()
    {
        var eventStore = new InMemoryEventStore();
        var handler = new RaiseLowStockAlertCommandHandler(eventStore);

        await handler.HandleAsync(new RaiseLowStockAlertCommand
        {
            Ean = "999",
            Name = "Water",
            Quantity = 5,
        });

        var events = await eventStore.GetEventsByStreamIdAsync("low-stock-alert-999");
        var evt = Assert.IsType<LowStockAlertRaisedEvent>(Assert.Single(events));
        Assert.Equal("low-stock-alert-999", evt.StreamId);
        Assert.Equal(1, evt.StreamSequenceNumber);
        Assert.Equal("999", evt.Ean);
        Assert.Equal("Water", evt.Name);
        Assert.Equal(5, evt.Quantity);
    }

    [Fact]
    public async Task Handle_DoesNotTouchProductStream()
    {
        var eventStore = new InMemoryEventStore();
        var handler = new RaiseLowStockAlertCommandHandler(eventStore);

        await handler.HandleAsync(new RaiseLowStockAlertCommand
        {
            Ean = "999",
            Name = "Water",
            Quantity = 5,
        });

        var productEvents = await eventStore.GetEventsByStreamIdAsync("product-999");
        Assert.Empty(productEvents);
    }

    [Fact]
    public async Task Handle_SecondAlert_AppendsAtNextSequence()
    {
        var eventStore = new InMemoryEventStore();
        var handler = new RaiseLowStockAlertCommandHandler(eventStore);

        await handler.HandleAsync(new RaiseLowStockAlertCommand { Ean = "999", Name = "Water", Quantity = 8 });
        await handler.HandleAsync(new RaiseLowStockAlertCommand { Ean = "999", Name = "Water", Quantity = 3 });

        var events = await eventStore.GetEventsByStreamIdAsync("low-stock-alert-999");
        Assert.Equal(2, events.Count);
        Assert.Equal(1, events[0].StreamSequenceNumber);
        Assert.Equal(2, events[1].StreamSequenceNumber);
        Assert.Equal(3, ((LowStockAlertRaisedEvent)events[1]).Quantity);
    }

    [Fact]
    public async Task Handle_DifferentEans_UseIndependentStreams()
    {
        var eventStore = new InMemoryEventStore();
        var handler = new RaiseLowStockAlertCommandHandler(eventStore);

        await handler.HandleAsync(new RaiseLowStockAlertCommand { Ean = "111", Name = "Widget A", Quantity = 2 });
        await handler.HandleAsync(new RaiseLowStockAlertCommand { Ean = "222", Name = "Widget B", Quantity = 4 });

        var streamA = await eventStore.GetEventsByStreamIdAsync("low-stock-alert-111");
        var streamB = await eventStore.GetEventsByStreamIdAsync("low-stock-alert-222");
        Assert.Single(streamA);
        Assert.Single(streamB);
        Assert.Equal(1, streamA[0].StreamSequenceNumber);
        Assert.Equal(1, streamB[0].StreamSequenceNumber);
    }
}
