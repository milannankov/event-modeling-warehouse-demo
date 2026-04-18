using Warehouse.EventSourcing;

namespace Warehouse.Tests.EventSourcing;

/// <summary>
/// A Given/Then test fixture for projections (State View slices), inspired by the Event Modeling
/// book's approach. Projections have no "When" — given a series of events, the read model
/// should reflect the expected state.
///
/// This fixture tests projection logic in-memory by tracking state as a dictionary,
/// applying events through a user-supplied projection function.
/// </summary>
public class ProjectionFixture<TReadModel>
{
    private readonly List<Event> _givenEvents = [];
    private readonly Func<Dictionary<string, TReadModel>, Event, Dictionary<string, TReadModel>> _projectFunc;
    private Dictionary<string, TReadModel> _state = new();

    public ProjectionFixture(Func<Dictionary<string, TReadModel>, Event, Dictionary<string, TReadModel>> projectFunc)
    {
        _projectFunc = projectFunc;
    }

    public ProjectionFixture<TReadModel> Given(params Event[] events)
    {
        _givenEvents.AddRange(events);
        return this;
    }

    public ProjectionFixture<TReadModel> ThenExpect(string key, TReadModel expected)
    {
        Project();
        Assert.True(_state.ContainsKey(key), $"Expected key '{key}' not found in projected state.");
        Assert.Equal(expected, _state[key]);
        return this;
    }

    public ProjectionFixture<TReadModel> ThenExpectCount(int expectedCount)
    {
        Project();
        Assert.Equal(expectedCount, _state.Count);
        return this;
    }

    public ProjectionFixture<TReadModel> ThenExpectEmpty()
    {
        Project();
        Assert.Empty(_state);
        return this;
    }

    public ProjectionFixture<TReadModel> ThenExpect(Action<Dictionary<string, TReadModel>> assertion)
    {
        Project();
        assertion(_state);
        return this;
    }

    private void Project()
    {
        _state = new Dictionary<string, TReadModel>();
        foreach (var evt in _givenEvents)
        {
            _state = _projectFunc(_state, evt);
        }
    }
}
