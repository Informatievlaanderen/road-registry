namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

public interface IIdentifierTranslator
{
    Problems RegisterMapping(RoadNodeId temporaryId, RoadNodeId permanentId);
    Problems RegisterMapping(RoadSegmentIdReference idReference, RoadSegmentId permanentId);
    RoadNodeId TranslateToTemporaryId(RoadNodeId id);
    RoadSegmentIdReference TranslateToTemporaryId(RoadSegmentId id);
    RoadSegmentId TranslateToPermanentId(RoadSegmentId id);
}

public class IdentifierTranslator : IIdentifierTranslator
{
    private readonly Dictionary<RoadNodeId, RoadNodeId> _mapToPermanentNodeIdentifiers = [];
    private readonly Dictionary<RoadNodeId, RoadNodeId> _mapToTemporaryNodeIdentifiers = [];
    private readonly Dictionary<RoadSegmentIdReference, RoadSegmentId> _mapSegmentIdReferenceToPermanentId = [];
    private readonly Dictionary<RoadSegmentId, RoadSegmentId> _mapSegmentTemporaryIdToPermanentId = [];
    private readonly Dictionary<RoadSegmentId, RoadSegmentIdReference> _mapToTemporarySegmentIdentifiers = [];

    public Problems RegisterMapping(RoadNodeId temporaryId, RoadNodeId permanentId)
    {
        if (!_mapToPermanentNodeIdentifiers.TryAdd(temporaryId, permanentId))
        {
            return Problems.Single(
                new Error(ProblemCode.RoadNode.TemporaryIdNotUnique.ToString(),
                    new ProblemParameter("TemporaryId", temporaryId.ToString())
                ));
        }

        _mapToTemporaryNodeIdentifiers[permanentId] = temporaryId;

        return Problems.None;
    }

    public Problems RegisterMapping(RoadSegmentIdReference idReference, RoadSegmentId permanentId)
    {
        if (!_mapSegmentIdReferenceToPermanentId.TryAdd(idReference, permanentId))
        {
            return Problems.Single(new Error(ProblemCode.RoadSegment.TemporaryIdNotUnique.ToString()));
        }

        _mapSegmentTemporaryIdToPermanentId[idReference.RoadSegmentId] = permanentId;
        _mapToTemporarySegmentIdentifiers[permanentId] = idReference;

        return Problems.None;
    }

    public RoadNodeId TranslateToTemporaryId(RoadNodeId id)
    {
        return _mapToTemporaryNodeIdentifiers.GetValueOrDefault(id, id);
    }

    public RoadSegmentIdReference TranslateToTemporaryId(RoadSegmentId id)
    {
        return _mapToTemporarySegmentIdentifiers.GetValueOrDefault(id, new RoadSegmentIdReference(id));
    }

    public RoadSegmentId TranslateToPermanentId(RoadSegmentId id)
    {
        return _mapSegmentTemporaryIdToPermanentId.GetValueOrDefault(id, id);
    }
}
