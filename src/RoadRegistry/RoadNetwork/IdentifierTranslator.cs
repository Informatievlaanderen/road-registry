namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using BackOffice;
using RoadSegment.ValueObjects;

public interface IIdentifierTranslator
{
    RoadNodeId TranslateToTemporaryId(RoadNodeId id);
    RoadSegmentId TranslateToTemporaryId(RoadSegmentId id);
    bool TryTranslateToPermanent(RoadNodeId id, out RoadNodeId permanent);
    bool TryTranslateToPermanent(RoadSegmentId id, out RoadSegmentId permanent);
}

public class IdentifierTranslator : IIdentifierTranslator
{
    private readonly Dictionary<RoadNodeId, RoadNodeId> _mapToPermanentNodeIdentifiers = [];
    private readonly Dictionary<RoadNodeId, RoadNodeId> _mapToOriginalNodeIdentifiers = [];
    private readonly Dictionary<RoadSegmentId, RoadSegmentId> _mapToPermanentSegmentIdentifiers = [];
    private readonly Dictionary<RoadSegmentId, RoadSegmentId> _mapToOriginalSegmentIdentifiers = [];

    public void RegisterMapping(RoadNodeId temporaryId, RoadNodeId permanentId)
    {
        _mapToPermanentNodeIdentifiers[temporaryId] = permanentId;
        _mapToOriginalNodeIdentifiers[permanentId] = temporaryId;
    }

    public void RegisterMapping(RoadSegmentId temporaryId, RoadSegmentId permanentId)
    {
        _mapToPermanentSegmentIdentifiers[temporaryId] = permanentId;
        _mapToOriginalSegmentIdentifiers[permanentId] = temporaryId;
    }

    public RoadNodeId TranslateToTemporaryId(RoadNodeId id)
    {
        return _mapToOriginalNodeIdentifiers.GetValueOrDefault(id, id);
    }

    public RoadSegmentId TranslateToTemporaryId(RoadSegmentId id)
    {
        return _mapToOriginalSegmentIdentifiers.GetValueOrDefault(id, id);
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
