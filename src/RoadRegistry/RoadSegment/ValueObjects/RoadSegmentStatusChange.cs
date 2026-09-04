namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;

// Every road segment status change the register supports, and what each one is called at every layer it travels
// through: the ticket action it is recorded under, and the event it raises.
//
// 'gerealiseerd' is the only status that knots a segment into the road network (see
// RoadSegmentStatusV2.ConnectsToRoadNodes), so a change has one of three shapes, and which one follows entirely from
// the two statuses:
//
//   Connect     - the segment is hooked into the network: it snaps onto the road nodes within reach, gets end nodes
//                 where it finds none, and the crossings it makes are worked out.
//   Disconnect  - the segment comes loose from the network: it gives up its road nodes and the crossings it took
//                 part in go with it.
//   Unconnected - the segment was outside the network and stays outside it, so nothing but the status moves.
//
// Adding a status change is adding one entry here plus the event it raises; the domain, the queue and the API all
// work from this table.
public sealed class RoadSegmentStatusChange : IEquatable<RoadSegmentStatusChange>
{
    public static readonly RoadSegmentStatusChange PlannedToRealized =
        Connect<RoadSegmentWasRealizedFromPlanned>(
            "ChangeRoadSegmentFromPlannedToRealized",
            RoadSegmentStatusV2.Gepland,
            // The one status change that predates this table, so it keeps the problem code it was published with.
            ProblemCode.RoadSegment.Realize.StatusNotValid,
            data => new RoadSegmentWasRealizedFromPlanned
            {
                RoadSegmentId = data.RoadSegmentId,
                Geometry = data.Geometry,
                StartNodeId = data.StartNodeId,
                EndNodeId = data.EndNodeId,
                AccessRestriction = data.Attributes.AccessRestriction,
                Category = data.Attributes.Category,
                Morphology = data.Attributes.Morphology,
                StreetNameId = data.Attributes.StreetNameId,
                MaintenanceAuthorityId = data.Attributes.MaintenanceAuthorityId,
                SurfaceType = data.Attributes.SurfaceType,
                CarTrafficDirection = data.Attributes.CarTrafficDirection,
                BikeTrafficDirection = data.Attributes.BikeTrafficDirection,
                PedestrianTrafficDirection = data.Attributes.PedestrianTrafficDirection,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange OutOfUseToRealized =
        Connect<RoadSegmentWasRealizedFromOutOfUse>(
            "ChangeRoadSegmentFromOutOfUseToRealized",
            RoadSegmentStatusV2.BuitenGebruik,
            data => new RoadSegmentWasRealizedFromOutOfUse
            {
                RoadSegmentId = data.RoadSegmentId,
                Geometry = data.Geometry,
                StartNodeId = data.StartNodeId,
                EndNodeId = data.EndNodeId,
                AccessRestriction = data.Attributes.AccessRestriction,
                Category = data.Attributes.Category,
                Morphology = data.Attributes.Morphology,
                StreetNameId = data.Attributes.StreetNameId,
                MaintenanceAuthorityId = data.Attributes.MaintenanceAuthorityId,
                SurfaceType = data.Attributes.SurfaceType,
                CarTrafficDirection = data.Attributes.CarTrafficDirection,
                BikeTrafficDirection = data.Attributes.BikeTrafficDirection,
                PedestrianTrafficDirection = data.Attributes.PedestrianTrafficDirection,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange HistorizedToRealized =
        Connect<RoadSegmentWasCorrectedFromHistorizedToRealized>(
            "CorrectRoadSegmentFromHistorizedToRealized",
            RoadSegmentStatusV2.Gehistoreerd,
            data => new RoadSegmentWasCorrectedFromHistorizedToRealized
            {
                RoadSegmentId = data.RoadSegmentId,
                Geometry = data.Geometry,
                StartNodeId = data.StartNodeId,
                EndNodeId = data.EndNodeId,
                AccessRestriction = data.Attributes.AccessRestriction,
                Category = data.Attributes.Category,
                Morphology = data.Attributes.Morphology,
                StreetNameId = data.Attributes.StreetNameId,
                MaintenanceAuthorityId = data.Attributes.MaintenanceAuthorityId,
                SurfaceType = data.Attributes.SurfaceType,
                CarTrafficDirection = data.Attributes.CarTrafficDirection,
                BikeTrafficDirection = data.Attributes.BikeTrafficDirection,
                PedestrianTrafficDirection = data.Attributes.PedestrianTrafficDirection,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange RealizedToPlanned =
        Disconnect<RoadSegmentWasCorrectedFromRealizedToPlanned>(
            "CorrectRoadSegmentFromRealizedToPlanned",
            RoadSegmentStatusV2.Gepland,
            // Predates this table as well, so it too keeps the problem code it was published with.
            ProblemCode.RoadSegment.CorrectFromRealizedToPlanned.StatusNotValid,
            data => new RoadSegmentWasCorrectedFromRealizedToPlanned
            {
                RoadSegmentId = data.RoadSegmentId,
                PreviousStartNodeId = data.PreviousStartNodeId,
                PreviousEndNodeId = data.PreviousEndNodeId,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange RealizedToOutOfUse =
        Disconnect<RoadSegmentWasTakenOutOfUseFromRealized>(
            "ChangeRoadSegmentFromRealizedToOutOfUse",
            RoadSegmentStatusV2.BuitenGebruik,
            data => new RoadSegmentWasTakenOutOfUseFromRealized
            {
                RoadSegmentId = data.RoadSegmentId,
                PreviousStartNodeId = data.PreviousStartNodeId,
                PreviousEndNodeId = data.PreviousEndNodeId,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange RealizedToHistorized =
        Disconnect<RoadSegmentWasHistorizedFromRealized>(
            "ChangeRoadSegmentFromRealizedToHistorized",
            RoadSegmentStatusV2.Gehistoreerd,
            data => new RoadSegmentWasHistorizedFromRealized
            {
                RoadSegmentId = data.RoadSegmentId,
                PreviousStartNodeId = data.PreviousStartNodeId,
                PreviousEndNodeId = data.PreviousEndNodeId,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange OutOfUseToHistorized =
        Unconnected<RoadSegmentWasHistorizedFromOutOfUse>(
            "ChangeRoadSegmentFromOutOfUseToHistorized",
            RoadSegmentStatusV2.BuitenGebruik,
            RoadSegmentStatusV2.Gehistoreerd,
            data => new RoadSegmentWasHistorizedFromOutOfUse
            {
                RoadSegmentId = data.RoadSegmentId,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange PlannedToNotRealized =
        Unconnected<RoadSegmentWasNotRealizedFromPlanned>(
            "ChangeRoadSegmentFromPlannedToNotRealized",
            RoadSegmentStatusV2.Gepland,
            RoadSegmentStatusV2.NietGerealiseerd,
            data => new RoadSegmentWasNotRealizedFromPlanned
            {
                RoadSegmentId = data.RoadSegmentId,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange NotRealizedToPlanned =
        Unconnected<RoadSegmentWasCorrectedFromNotRealizedToPlanned>(
            "CorrectRoadSegmentFromNotRealizedToPlanned",
            RoadSegmentStatusV2.NietGerealiseerd,
            RoadSegmentStatusV2.Gepland,
            data => new RoadSegmentWasCorrectedFromNotRealizedToPlanned
            {
                RoadSegmentId = data.RoadSegmentId,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange HistorizedToOutOfUse =
        Unconnected<RoadSegmentWasCorrectedFromHistorizedToOutOfUse>(
            "CorrectRoadSegmentFromHistorizedToOutOfUse",
            RoadSegmentStatusV2.Gehistoreerd,
            RoadSegmentStatusV2.BuitenGebruik,
            data => new RoadSegmentWasCorrectedFromHistorizedToOutOfUse
            {
                RoadSegmentId = data.RoadSegmentId,
                Provenance = data.Provenance
            });

    public static readonly RoadSegmentStatusChange[] All =
    [
        PlannedToRealized, OutOfUseToRealized, HistorizedToRealized,
        RealizedToPlanned, RealizedToOutOfUse, RealizedToHistorized,
        OutOfUseToHistorized, NotRealizedToPlanned, HistorizedToOutOfUse,
        PlannedToNotRealized
    ];

    private static readonly IReadOnlyDictionary<string, RoadSegmentStatusChange> ByName =
        All.ToDictionary(x => x.Name);

    private static readonly IReadOnlyDictionary<Type, RoadSegmentStatusChange> ByEventType =
        All.ToDictionary(x => x.EventType);

    private readonly Func<RoadSegmentConnectedChangeData, IRoadSegmentWasConnectedEvent>? _buildConnectedEvent;
    private readonly Func<RoadSegmentDisconnectedChangeData, IRoadSegmentWasDisconnectedEvent>? _buildDisconnectedEvent;
    private readonly Func<RoadSegmentUnconnectedChangeData, IRoadSegmentUnconnectedStatusChangeEvent>? _buildUnconnectedEvent;

    private RoadSegmentStatusChange(
        string name,
        Type eventType,
        RoadSegmentStatusV2 from,
        RoadSegmentStatusV2 to,
        ProblemCode statusNotValid,
        Func<RoadSegmentConnectedChangeData, IRoadSegmentWasConnectedEvent>? buildConnectedEvent,
        Func<RoadSegmentDisconnectedChangeData, IRoadSegmentWasDisconnectedEvent>? buildDisconnectedEvent,
        Func<RoadSegmentUnconnectedChangeData, IRoadSegmentUnconnectedStatusChangeEvent>? buildUnconnectedEvent)
    {
        Name = name;
        EventType = eventType;
        From = from;
        To = to;
        StatusNotValidProblemCode = statusNotValid;
        _buildConnectedEvent = buildConnectedEvent;
        _buildDisconnectedEvent = buildDisconnectedEvent;
        _buildUnconnectedEvent = buildUnconnectedEvent;
    }

    // How the change identifies itself outside the domain: on the queue, and as the ticket action.
    public string Name { get; }

    public Type EventType { get; }

    // The only status the segment may be in for this change to be allowed.
    public RoadSegmentStatusV2 From { get; }

    // The status the segment ends up in. It is not carried on the event - the event is the status - so a reader that
    // has an event and wants the status it settled on asks ForEvent.
    public RoadSegmentStatusV2 To { get; }

    public ProblemCode StatusNotValidProblemCode { get; }

    public bool Connects => !From.ConnectsToRoadNodes && To.ConnectsToRoadNodes;
    public bool Disconnects => From.ConnectsToRoadNodes && !To.ConnectsToRoadNodes;
    public bool StaysUnconnected => !From.ConnectsToRoadNodes && !To.ConnectsToRoadNodes;

    public IRoadSegmentWasConnectedEvent BuildEvent(RoadSegmentConnectedChangeData data)
        => _buildConnectedEvent!(data);

    public IRoadSegmentWasDisconnectedEvent BuildEvent(RoadSegmentDisconnectedChangeData data)
        => _buildDisconnectedEvent!(data);

    public IRoadSegmentUnconnectedStatusChangeEvent BuildEvent(RoadSegmentUnconnectedChangeData data)
        => _buildUnconnectedEvent!(data);

    // The status change an event came from, and with it the status the segment ended up in.
    public static RoadSegmentStatusChange ForEvent(IRoadSegmentStatusChangeEvent @event)
        => ByEventType[@event.GetType()];

    public static RoadSegmentStatusChange Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed))
        {
            throw new FormatException($"The value {value} is not a well known road segment status change.");
        }

        return parsed;
    }

    public static bool TryParse(string value, out RoadSegmentStatusChange parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        return ByName.TryGetValue(value.Trim(), out parsed!);
    }

    public bool Equals(RoadSegmentStatusChange? other) => other is not null && other.Name == Name;
    public override bool Equals(object? obj) => obj is RoadSegmentStatusChange other && Equals(other);
    public override int GetHashCode() => Name.GetHashCode();
    public override string ToString() => Name;

    public static implicit operator string?(RoadSegmentStatusChange? instance) => instance?.ToString();

    private static RoadSegmentStatusChange Connect<TEvent>(
        string name,
        RoadSegmentStatusV2 from,
        Func<RoadSegmentConnectedChangeData, IRoadSegmentWasConnectedEvent> buildEvent)
        => Connect<TEvent>(name, from, ProblemCode.RoadSegment.ChangeStatus.StatusNotValid, buildEvent);

    private static RoadSegmentStatusChange Connect<TEvent>(
        string name,
        RoadSegmentStatusV2 from,
        ProblemCode statusNotValid,
        Func<RoadSegmentConnectedChangeData, IRoadSegmentWasConnectedEvent> buildEvent)
        => new(name, typeof(TEvent), from, RoadSegmentStatusV2.Gerealiseerd, statusNotValid, buildEvent, null, null);

    private static RoadSegmentStatusChange Disconnect<TEvent>(
        string name,
        RoadSegmentStatusV2 to,
        Func<RoadSegmentDisconnectedChangeData, IRoadSegmentWasDisconnectedEvent> buildEvent)
        => Disconnect<TEvent>(name, to, ProblemCode.RoadSegment.ChangeStatus.StatusNotValid, buildEvent);

    private static RoadSegmentStatusChange Disconnect<TEvent>(
        string name,
        RoadSegmentStatusV2 to,
        ProblemCode statusNotValid,
        Func<RoadSegmentDisconnectedChangeData, IRoadSegmentWasDisconnectedEvent> buildEvent)
        => new(name, typeof(TEvent), RoadSegmentStatusV2.Gerealiseerd, to, statusNotValid, null, buildEvent, null);

    private static RoadSegmentStatusChange Unconnected<TEvent>(
        string name,
        RoadSegmentStatusV2 from,
        RoadSegmentStatusV2 to,
        Func<RoadSegmentUnconnectedChangeData, IRoadSegmentUnconnectedStatusChangeEvent> buildEvent)
        => new(name, typeof(TEvent), from, to, ProblemCode.RoadSegment.ChangeStatus.StatusNotValid, null, null, buildEvent);
}

// What a Connect change settles: the geometry as it ended up after snapping, the two nodes the segment hangs off, and
// the attributes already remapped onto that geometry.
public sealed record RoadSegmentConnectedChangeData
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadNodeId StartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    public required RoadSegmentAttributes Attributes { get; init; }
    public required ProvenanceData Provenance { get; init; }
}

// What a Disconnect change settles: the two nodes the segment came loose from.
public sealed record RoadSegmentDisconnectedChangeData
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadNodeId PreviousStartNodeId { get; init; }
    public required RoadNodeId PreviousEndNodeId { get; init; }
    public required ProvenanceData Provenance { get; init; }
}

// What an Unconnected change settles: nothing but the segment it names.
public sealed record RoadSegmentUnconnectedChangeData
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required ProvenanceData Provenance { get; init; }
}
