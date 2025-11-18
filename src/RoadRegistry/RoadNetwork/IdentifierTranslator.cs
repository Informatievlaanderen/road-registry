namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using BackOffice;
using RoadSegment.ValueObjects;

public interface IIdentifierTranslator
{
    void RegisterMapping(RoadNodeId temporaryId, RoadNodeId permanentId);
    void RegisterMapping(RoadSegmentId temporaryId, RoadSegmentId permanentId);
    RoadNodeId TranslateToTemporaryId(RoadNodeId id);
    RoadSegmentId TranslateToTemporaryId(RoadSegmentId id);
    RoadNodeId TranslateToPermanentId(RoadNodeId id);
    RoadSegmentId TranslateToPermanentId(RoadSegmentId id);
    bool TryTranslateToPermanent(RoadNodeId id, out RoadNodeId permanent);
    bool TryTranslateToPermanent(RoadSegmentId id, out RoadSegmentId permanent);
}

public class IdentifierTranslator : IIdentifierTranslator
{
    private readonly Dictionary<RoadNodeId, RoadNodeId> _mapToPermanentNodeIdentifiers = [];
    private readonly Dictionary<RoadNodeId, RoadNodeId> _mapToTemporaryNodeIdentifiers = [];
    private readonly Dictionary<RoadSegmentId, RoadSegmentId> _mapToPermanentSegmentIdentifiers = [];
    private readonly Dictionary<RoadSegmentId, RoadSegmentId> _mapToTemporarySegmentIdentifiers = [];

    public void RegisterMapping(RoadNodeId temporaryId, RoadNodeId permanentId)
    {
        _mapToPermanentNodeIdentifiers[temporaryId] = permanentId;
        _mapToTemporaryNodeIdentifiers[permanentId] = temporaryId;
    }

    public void RegisterMapping(RoadSegmentId temporaryId, RoadSegmentId permanentId)
    {
        _mapToPermanentSegmentIdentifiers[temporaryId] = permanentId;
        _mapToTemporarySegmentIdentifiers[permanentId] = temporaryId;
    }

    public RoadNodeId TranslateToTemporaryId(RoadNodeId id)
    {
        return _mapToTemporaryNodeIdentifiers.GetValueOrDefault(id, id);
    }
    public RoadSegmentId TranslateToTemporaryId(RoadSegmentId id)
    {
        return _mapToTemporarySegmentIdentifiers.GetValueOrDefault(id, id);
    }

    public RoadNodeId TranslateToPermanentId(RoadNodeId id)
    {
        return _mapToPermanentNodeIdentifiers.GetValueOrDefault(id, id);
    }
    public RoadSegmentId TranslateToPermanentId(RoadSegmentId id)
    {
        return _mapToPermanentSegmentIdentifiers.GetValueOrDefault(id, id);
    }

    public bool TryTranslateToPermanent(RoadNodeId id, out RoadNodeId permanent)
    {
        return _mapToPermanentNodeIdentifiers.TryGetValue(id, out permanent);
    }
    public bool TryTranslateToPermanent(RoadSegmentId id, out RoadSegmentId permanent)
    {
        return _mapToPermanentSegmentIdentifiers.TryGetValue(id, out permanent);
    }
}
