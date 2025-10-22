namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using BackOffice;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Events;
using NetTopologySuite.Geometries;
using ValueObjects;
using RoadSegmentLaneAttribute = ValueObjects.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = ValueObjects.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = ValueObjects.RoadSegmentWidthAttribute;

public partial class RoadSegment
{
    public RoadSegmentId Id { get; private set; }
    public MultiLineString Geometry { get; private set; }
    public RoadNodeId StartNodeId { get; private set; }
    public RoadNodeId EndNodeId { get; private set; }
    public AttributeHash AttributeHash { get; private set; }
    public IReadOnlyCollection<RoadSegmentLaneAttribute> Lanes { get; private set; }
    public IReadOnlyCollection<RoadSegmentSurfaceAttribute> Surfaces { get; private set; }
    public IReadOnlyCollection<RoadSegmentWidthAttribute> Widths { get; private set; }
    public IReadOnlyCollection<RoadSegmentEuropeanRoadAttribute> EuropeanRoads { get; private set; }
    public IReadOnlyCollection<RoadSegmentNationalRoadAttribute> NationalRoads { get; private set; }
    public IReadOnlyCollection<RoadSegmentNumberedRoadAttribute> NumberedRoads { get; private set; }
    public IEnumerable<RoadNodeId> Nodes
    {
        get
        {
            if (StartNodeId > 0)
            {
                yield return StartNodeId;
            }

            if (EndNodeId > 0)
            {
                yield return EndNodeId;
            }
        }
    }
    //public string LastEventHash { get; private set; }

    private RoadSegment()
    {
        Register<RoadSegmentAdded>(When);
        Register<RoadSegmentModified>(When);
    }

    private void When(RoadSegmentAdded @event)
    {
        Id = new RoadSegmentId(@event.Id);
        Geometry = @event.Geometry.ToMultiLineString();
        StartNodeId = new RoadNodeId(@event.StartNodeId);
        EndNodeId = new RoadNodeId(@event.EndNodeId);
        AttributeHash = new AttributeHash(
            RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
            RoadSegmentCategory.Parse(@event.Category),
            RoadSegmentMorphology.Parse(@event.Morphology),
            RoadSegmentStatus.Parse(@event.Status),
            StreetNameLocalId.FromValue(@event.LeftSide.StreetNameId),
            StreetNameLocalId.FromValue(@event.RightSide.StreetNameId),
            new OrganizationId(@event.MaintenanceAuthorityId),
            RoadSegmentGeometryDrawMethod.Parse(@event.GeometryDrawMethod)
        );
        Lanes = @event.Lanes.Select(x => new RoadSegmentLaneAttribute(
            new AttributeId(x.AttributeId),
            new RoadSegmentPosition(x.FromPosition),
            new RoadSegmentPosition(x.ToPosition),
            new RoadSegmentLaneCount(x.Count),
            RoadSegmentLaneDirection.Parse(x.Direction))).ToArray();
        Surfaces = @event.Surfaces.Select(x => new RoadSegmentSurfaceAttribute(
            new AttributeId(x.AttributeId),
            new RoadSegmentPosition(x.FromPosition),
            new RoadSegmentPosition(x.ToPosition),
            RoadSegmentSurfaceType.Parse(x.Type))).ToArray();
        Widths = @event.Widths.Select(x => new RoadSegmentWidthAttribute(
            new AttributeId(x.AttributeId),
            new RoadSegmentPosition(x.FromPosition),
            new RoadSegmentPosition(x.ToPosition),
            new RoadSegmentWidth(x.Width))).ToArray();
        EuropeanRoads = [];
        NationalRoads = [];
        NumberedRoads = [];
        //LastEventHash = @event.GetHash();
    }

    private void When(RoadSegmentModified @event)
    {
        Id = new RoadSegmentId(@event.Id);
        Geometry = @event.Geometry.ToMultiLineString();
        StartNodeId = new RoadNodeId(@event.StartNodeId);
        EndNodeId = new RoadNodeId(@event.EndNodeId);
        AttributeHash = new AttributeHash(
            RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
            RoadSegmentCategory.Parse(@event.Category),
            RoadSegmentMorphology.Parse(@event.Morphology),
            RoadSegmentStatus.Parse(@event.Status),
            StreetNameLocalId.FromValue(@event.LeftSide.StreetNameId),
            StreetNameLocalId.FromValue(@event.RightSide.StreetNameId),
            new OrganizationId(@event.MaintenanceAuthorityId),
            RoadSegmentGeometryDrawMethod.Parse(@event.GeometryDrawMethod)
        );
        Lanes = @event.Lanes.Select(x => new RoadSegmentLaneAttribute(
            new AttributeId(x.AttributeId),
            new RoadSegmentPosition(x.FromPosition),
            new RoadSegmentPosition(x.ToPosition),
            new RoadSegmentLaneCount(x.Count),
            RoadSegmentLaneDirection.Parse(x.Direction))).ToArray();
        Surfaces = @event.Surfaces.Select(x => new RoadSegmentSurfaceAttribute(
            new AttributeId(x.AttributeId),
            new RoadSegmentPosition(x.FromPosition),
            new RoadSegmentPosition(x.ToPosition),
            RoadSegmentSurfaceType.Parse(x.Type))).ToArray();
        Widths = @event.Widths.Select(x => new RoadSegmentWidthAttribute(
            new AttributeId(x.AttributeId),
            new RoadSegmentPosition(x.FromPosition),
            new RoadSegmentPosition(x.ToPosition),
            new RoadSegmentWidth(x.Width))).ToArray();
        //LastEventHash = @event.GetHash();
    }
}
