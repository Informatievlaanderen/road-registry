namespace RoadRegistry.ScopedRoadNetwork.Events.V2;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public record RoadNetworkWasChanged : IMartenEvent, ICreatedEvent
{
    public required RoadNetworkId RoadNetworkId { get; init; }
    public RoadNetworkChangeGeometry? ScopeGeometry { get; init; }
    public DownloadId? DownloadId { get; init; }
    public required RoadNetworkChangedSummary Summary { get; init; }

    public required ProvenanceData Provenance { get; init; }
}

public sealed class RoadNetworkChangedSummary
{
    public RoadNetworkChangedEntitySummary RoadNodes { get; init; }
    public RoadNetworkChangedEntitySummary RoadSegments { get; init; }
    public RoadNetworkChangedEntitySummary GradeSeparatedJunctions { get; init; }

    public RoadNetworkChangedSummary()
    {
    }

    public RoadNetworkChangedSummary(RoadNetworkChangesSummary summary)
    {
        RoadNodes = new()
        {
            Added = summary.RoadNodes.Added.Select(x => x.ToInt32()).ToList(),
            Modified = summary.RoadNodes.Modified.Select(x => x.ToInt32()).ToList(),
            Removed = summary.RoadNodes.Removed.Select(x => x.ToInt32()).ToList()
        };
        RoadSegments = new()
        {
            Added = summary.RoadSegments.Added.Select(x => x.ToInt32()).ToList(),
            Modified = summary.RoadSegments.Modified.Select(x => x.ToInt32()).ToList(),
            Removed = summary.RoadSegments.Removed.Select(x => x.ToInt32()).ToList()
        };
        GradeSeparatedJunctions = new()
        {
            Added = summary.GradeSeparatedJunctions.Added.Select(x => x.ToInt32()).ToList(),
            Modified = summary.GradeSeparatedJunctions.Modified.Select(x => x.ToInt32()).ToList(),
            Removed = summary.GradeSeparatedJunctions.Removed.Select(x => x.ToInt32()).ToList()
        };
    }

    public RoadNetworkChangesSummary ToRoadNetworkChangesSummary()
    {
        return new RoadNetworkChangesSummary
        {
            RoadNodes = RoadNodes.ToRoadNetworkEntityChangesSummary(x => new RoadNodeId(x)),
            RoadSegments = RoadSegments.ToRoadNetworkEntityChangesSummary(x => new RoadSegmentId(x)),
            GradeSeparatedJunctions = GradeSeparatedJunctions.ToRoadNetworkEntityChangesSummary(x => new GradeSeparatedJunctionId(x)),
        };
    }
}

public sealed record RoadNetworkChangedEntitySummary
{
    public required ICollection<int> Added { get; init; }
    public required ICollection<int> Modified { get; init; }
    public required ICollection<int> Removed { get; init; }

    public RoadNetworkEntityChangesSummary<TIdentifier> ToRoadNetworkEntityChangesSummary<TIdentifier>(Func<int, TIdentifier> converter)
    {
        return new RoadNetworkEntityChangesSummary<TIdentifier>
        {
            Added = new UniqueList<TIdentifier>(Added.Select(converter)),
            Modified = new UniqueList<TIdentifier>(Modified.Select(converter)),
            Removed = new UniqueList<TIdentifier>(Removed.Select(converter))
        };
    }
}
