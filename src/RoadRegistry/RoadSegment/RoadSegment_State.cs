namespace RoadRegistry.RoadSegment;

using System;
using System.Collections.Generic;
using BackOffice;
using Events;
using NetTopologySuite.Geometries;
using ValueObjects;

public sealed class RoadSegmentAttributes
{
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public RoadSegmentDynamicAttributeCollection<RoadSegmentAccessRestriction> AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeCollection<RoadSegmentCategory> Category { get; init; }
    public RoadSegmentDynamicAttributeCollection<RoadSegmentMorphology> Morphology { get; init; }
    public RoadSegmentDynamicAttributeCollection<RoadSegmentStatus> Status { get; init; }
    public RoadSegmentDynamicAttributeCollection<StreetNameLocalId> StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeCollection<OrganizationId> MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeCollection<RoadSegmentSurfaceType> SurfaceType { get; init; }
    public IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; init; }

    public bool Equals(RoadSegmentAttributes other)
    {
        //TODO-pr implement equality check, taking dynamic attributes into account
        throw new NotImplementedException();
    }
}

public partial class RoadSegment
{
    public RoadSegmentId Id { get; private set; }
    public MultiLineString Geometry { get; private set; }
    public RoadNodeId StartNodeId { get; private set; }
    public RoadNodeId EndNodeId { get; private set; }
    public RoadSegmentAttributes Attributes { get; private set; }

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

    public bool IsRemoved { get; private set; }

    private RoadSegment()
    {
        Register<RoadSegmentAdded>(When);
        Register<RoadSegmentModified>(When);
        Register<RoadSegmentRemoved>(When);
    }

    private void When(RoadSegmentAdded @event)
    {
        Id = @event.Id;
        Geometry = @event.Geometry.ToMultiLineString();
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new()
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            Status = @event.Status,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers,
            NationalRoadNumbers = @event.NationalRoadNumbers
        };
        //LastEventHash = @event.GetHash();
    }

    private void When(RoadSegmentModified @event)
    {
        Geometry = @event.Geometry.ToMultiLineString();
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new()
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            Status = @event.Status,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers,
            NationalRoadNumbers = @event.NationalRoadNumbers
        };
        //LastEventHash = @event.GetHash();
    }

    private void When(RoadSegmentRemoved @event)
    {
        IsRemoved = true;
        //LastEventHash = @event.GetHash();
    }
}
