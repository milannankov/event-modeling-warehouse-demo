using Warehouse.EventSourcing;

namespace Warehouse.Tests.EventSourcing;

/// <summary>
/// A Given/When/Then test fixture for aggregates, inspired by the Event Modeling book's
/// AggregateTestFixture pattern. Enables testing aggregate behavior as a black box:
/// given prior events, when a command action is performed, then expect resulting events or exceptions.
/// </summary>
public class AggregateFixture<TAggregate> where TAggregate : Aggregate, new()
{
    private readonly List<Event> _givenEvents = [];
    private TAggregate? _aggregate;
    private Exception? _caughtException;
    private bool _actionExecuted;

    public AggregateFixture<TAggregate> Given(params Event[] events)
    {
        _givenEvents.AddRange(events);
        return this;
    }

    public AggregateFixture<TAggregate> When(Action<TAggregate> action)
    {
        _aggregate = new TAggregate();
        _aggregate.Load(_givenEvents);
        _aggregate.ClearUncommittedEvents();

        try
        {
            action(_aggregate);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }

        _actionExecuted = true;
        return this;
    }

    public AggregateFixture<TAggregate> ThenExpectEvents(params Event[] expectedEvents)
    {
        EnsureActionExecuted();
        Assert.Null(_caughtException);
        var uncommitted = _aggregate!.UncommittedEvents;

        Assert.Equal(expectedEvents.Length, uncommitted.Count);

        for (var i = 0; i < expectedEvents.Length; i++)
        {
            var expected = expectedEvents[i];
            var actual = uncommitted[i];

            Assert.Equal(expected.GetType(), actual.GetType());
            AssertEventDataEqual(expected, actual);
        }

        return this;
    }

    public AggregateFixture<TAggregate> ThenExpectNoEvents()
    {
        EnsureActionExecuted();
        Assert.Null(_caughtException);
        Assert.Empty(_aggregate!.UncommittedEvents);
        return this;
    }

    public AggregateFixture<TAggregate> ThenExpectException<TException>(string? messageContains = null)
        where TException : Exception
    {
        EnsureActionExecuted();
        Assert.NotNull(_caughtException);
        Assert.IsType<TException>(_caughtException);

        if (messageContains is not null)
        {
            Assert.Contains(messageContains, _caughtException.Message);
        }

        return this;
    }

    private void EnsureActionExecuted()
    {
        if (!_actionExecuted)
            throw new InvalidOperationException("Must call When() before asserting results.");
    }

    private static void AssertEventDataEqual(Event expected, Event actual)
    {
        // Compare all init properties of the event record, excluding infrastructure fields
        // (Id, StreamSequenceNumber, CreatedAt, Metadata) which are set by the store, not the aggregate.
        var properties = expected.GetType().GetProperties()
            .Where(p => p.Name is not ("Id" or "StreamSequenceNumber" or "CreatedAt" or "Metadata" or "Type" or "Version"));

        foreach (var prop in properties)
        {
            var expectedValue = prop.GetValue(expected);
            var actualValue = prop.GetValue(actual);
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
