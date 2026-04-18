using System.Collections.Frozen;
using System.Reflection;

namespace Warehouse.EventSourcing;

public static class EventTypeRegistry
{
    private static readonly FrozenDictionary<string, Type> TypeMap;

    static EventTypeRegistry()
    {
        TypeMap = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(Event)) && !t.IsAbstract)
            .ToFrozenDictionary(t => t.Name, t => t);
    }

    public static Type? Resolve(string typeName) =>
        TypeMap.GetValueOrDefault(typeName);
}
