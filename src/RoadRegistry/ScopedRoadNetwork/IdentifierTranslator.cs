namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

public interface IIdentifierTranslator
{
    Problems RegisterMapping(RoadNodeId temporaryId, RoadNodeId permanentId);
    Problems RegisterMapping(RoadSegmentIdReference idReference, RoadSegmentId permanentId);
    Problems RegisterMapping(GradeSeparatedJunctionId temporaryId, GradeSeparatedJunctionId permanentId);
    RoadNodeId TranslateToTemporaryId(RoadNodeId id);
    RoadSegmentIdReference TranslateToTemporaryId(RoadSegmentId id);
    GradeSeparatedJunctionId TranslateToTemporaryId(GradeSeparatedJunctionId id);
    RoadSegmentId TranslateToPermanentId(RoadSegmentId id);
}

public class IdentifierTranslator : IIdentifierTranslator
{
    private readonly Dictionary<RoadNodeId, RoadNodeId> _mapTemporaryToPermanentNodeIdentifiers = [];
    private readonly Dictionary<RoadNodeId, RoadNodeId> _mapPermanentToTemporaryNodeIdentifiers = [];
    private readonly Dictionary<RoadSegmentIdReference, RoadSegmentId> _mapSegmentIdReferenceToPermanentId = [];
    private readonly Dictionary<RoadSegmentId, RoadSegmentId> _mapSegmentTemporaryIdToPermanentId = [];
    private readonly Dictionary<RoadSegmentId, RoadSegmentIdReference> _mapToTemporarySegmentIdentifiers = [];
    private readonly Dictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> _mapTemporaryToPermanentJunctionIdentifiers = [];
    private readonly Dictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> _mapPermanentToTemporaryJunctionIdentifiers = [];

    public Problems RegisterMapping(RoadNodeId temporaryId, RoadNodeId permanentId)
    {
        if (!_mapTemporaryToPermanentNodeIdentifiers.TryAdd(temporaryId, permanentId))
        {
            return Problems.Single(
                new Error(ProblemCode.RoadNode.TemporaryIdNotUnique.ToString(),
                    new ProblemParameter("TemporaryId", temporaryId.ToString())
                ));
        }

        _mapPermanentToTemporaryNodeIdentifiers[permanentId] = temporaryId;

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

    public Problems RegisterMapping(GradeSeparatedJunctionId temporaryId, GradeSeparatedJunctionId permanentId)
    {
        if (!_mapTemporaryToPermanentJunctionIdentifiers.TryAdd(temporaryId, permanentId))
        {
            return Problems.Single(
                new Error(ProblemCode.GradeSeparatedJunction.TemporaryIdNotUnique.ToString(),
                    new ProblemParameter("TemporaryId", temporaryId.ToString())
                ));
        }

        _mapPermanentToTemporaryJunctionIdentifiers[permanentId] = temporaryId;

        return Problems.None;
    }

    public RoadNodeId TranslateToTemporaryId(RoadNodeId id)
    {
        return _mapPermanentToTemporaryNodeIdentifiers.GetValueOrDefault(id, id);
    }

    public RoadSegmentIdReference TranslateToTemporaryId(RoadSegmentId id)
    {
        return _mapToTemporarySegmentIdentifiers.GetValueOrDefault(id, new RoadSegmentIdReference(id));
    }

    public GradeSeparatedJunctionId TranslateToTemporaryId(GradeSeparatedJunctionId id)
    {
        return _mapPermanentToTemporaryJunctionIdentifiers.GetValueOrDefault(id, id);
    }

    public RoadSegmentId TranslateToPermanentId(RoadSegmentId id)
    {
        return _mapSegmentTemporaryIdToPermanentId.GetValueOrDefault(id, id);
    }
}
