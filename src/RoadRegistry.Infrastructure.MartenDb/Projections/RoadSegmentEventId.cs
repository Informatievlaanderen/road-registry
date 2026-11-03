namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JasperFx.Events;

// The road segment an event is about, read by property name rather than declared per event type. A table mapping every
// handled event to its id would be a second list to remember when an event is added, and forgetting it would cost
// throughput while saying nothing - so the name is the contract, and each projection's coverage test holds it.
//
// Not finding one is not an error: that segment is left out of the prefetch and its handlers read per event as before.
//
// Shared rather than per projection because it is pure reflection over the event's own shape: every road-segment-keyed
// read model asks the same question of the same events, and two copies of the answer is two places for a new event to
// be missed.
public static class RoadSegmentEventId
{
    private static readonly ConcurrentDictionary<Type, Func<object, int?>?> Accessors = new();

    public static int? Of(IEvent @event)
    {
        var data = @event.Data;
        return data is null ? null : Accessors.GetOrAdd(data.GetType(), BuildAccessor)?.Invoke(data);
    }

    // Public so a projection's coverage test can ask, of every event it handles, whether its id can be read - which is
    // the only thing standing between a new event and a silent return to one round trip per lookup.
    public static Func<object, int?>? BuildAccessor(Type eventType)
    {
        var property = eventType.GetProperty("RoadSegmentId", BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            return null;
        }

        // V1 carries the id as a plain int, V2 as a value object.
        if (property.PropertyType == typeof(int))
        {
            return data => (int?)property.GetValue(data);
        }

        var toInt32 = property.PropertyType.GetMethod("ToInt32", Type.EmptyTypes);
        if (toInt32 is null || toInt32.ReturnType != typeof(int))
        {
            return null;
        }

        return data =>
        {
            var value = property.GetValue(data);
            return value is null ? null : (int?)toInt32.Invoke(value, null);
        };
    }
}
