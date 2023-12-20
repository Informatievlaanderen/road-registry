namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using Framework;

public interface IRoadRegistryEventFilter
{
    IRoadRegistryEventFilter Exclude(IEventSourcedEntity entity, object @event);
    bool IsAllowed(IEventSourcedEntity entity, object @event);
}

public class RoadRegistryEventFilter : IRoadRegistryEventFilter
{
    private readonly List<Func<IEventSourcedEntity, object, bool>> _excludes = new();

    public IRoadRegistryEventFilter Exclude(IEventSourcedEntity entity, object @event)
    {
        _excludes.Add((excludeEntity, excludeEvent) => Equals(excludeEntity, entity) && Equals(excludeEvent, @event));
        return this;
    }

    public bool IsAllowed(IEventSourcedEntity entity, object @event)
    {
        foreach (var exclude in _excludes)
        {
            if (exclude(entity, @event))
            {
                return false;
            }
        }

        return true;
    }
}
