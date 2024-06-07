namespace RoadRegistry.Integration.Schema.RoadSegments;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.Utilities;
using NetTopologySuite.Geometries;
using NodaTime;

public class RoadSegmentRecord
{
    public int Id { get; set; }
    public double? BoundingBoxMaximumM { get; set; }
    public double? BoundingBoxMaximumX { get; set; }
    public double? BoundingBoxMaximumY { get; set; }
    public double? BoundingBoxMinimumM { get; set; }
    public double? BoundingBoxMinimumX { get; set; }
    public double? BoundingBoxMinimumY { get; set; }
    public int EndNodeId { get; set; }
    public Geometry Geometry { get; set; }
    public int StartNodeId { get; set; }
    public bool IsRemoved { get; set; }

    public int Version { get; set; }
    public int GeometryVersion { get; set; }
    public int AccessRestrictionId { get; set; }
    public string AccessRestrictionLabel { get; set; }
    public string CategoryId { get; set; }
    public string CategoryLabel { get; set; }
    public int? LeftSideStreetNameId { get; set; }
    public string LeftSideStreetNameName { get; set; } // Enable nullable and make string?
    public string MaintainerId { get; set; }
    public string MaintainerName { get; set; }
    public int MethodId { get; set; }
    public string MethodLabel { get; set; }
    public int MorphologyId { get; set; }
    public string MorphologyLabel { get; set; }
    public int? RightSideStreetNameId { get; set; }
    public string RightSideStreetNameName { get; set; } // Enable nullable and make string?
    public int StatusId { get; set; }
    public string StatusLabel { get; set; }

    public int TransactionId { get; set; }
    public DateTime RecordingDate { get; set; }
    public DateTime BeginTime { get; set; }
    public string BeginOrganizationId { get; set; }
    public string BeginOrganizationName { get; set; }

    public string Puri { get; set; }
    public string Namespace { get; set; }
    public string VersionAsString { get; set; }
    private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

    public Instant VersionTimestamp
    {
        get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
        set
        {
            VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
            VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
        }
    }

    public RoadSegmentBoundingBox GetBoundingBox() => BoundingBoxMaximumX is not null
        ? new()
        {
            MaximumX = BoundingBoxMaximumX!.Value,
            MaximumY = BoundingBoxMaximumY!.Value,
            MaximumM = BoundingBoxMaximumM!.Value,
            MinimumX = BoundingBoxMinimumX!.Value,
            MinimumY = BoundingBoxMinimumY!.Value,
            MinimumM = BoundingBoxMinimumM!.Value
        }
        : null;

    public RoadSegmentRecord WithBoundingBox(RoadSegmentBoundingBox value)
    {
        BoundingBoxMaximumX = value.MaximumX;
        BoundingBoxMaximumY = value.MaximumY;
        BoundingBoxMaximumM = value.MaximumM;
        BoundingBoxMinimumX = value.MinimumX;
        BoundingBoxMinimumY = value.MinimumY;
        BoundingBoxMinimumM = value.MinimumM;
        return this;
    }

    public void SetAccessRestriction(RoadSegmentAccessRestriction accessRestriction)
    {
        AccessRestrictionId = accessRestriction.Translation.Identifier;
        AccessRestrictionLabel = accessRestriction.Translation.Name;
    }

    public void SetCategory(RoadSegmentCategory category)
    {
        CategoryId = category.Translation.Identifier;
        CategoryLabel = category.Translation.Name;
    }

    public void SetMethod(RoadSegmentGeometryDrawMethod method)
    {
        MethodId = method.Translation.Identifier;
        MethodLabel = method.Translation.Name;
    }

    public void SetMorphology(RoadSegmentMorphology morphology)
    {
        MorphologyId = morphology.Translation.Identifier;
        MorphologyLabel = morphology.Translation.Name;
    }

    public void SetStatus(RoadSegmentStatus status)
    {
        StatusId = status.Translation.Identifier;
        StatusLabel = status.Translation.Name;
    }
}
