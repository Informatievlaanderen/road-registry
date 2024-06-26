namespace RoadRegistry.Integration.Schema.RoadSegments.Version;

using System;
using System.Collections.Generic;
using System.Linq;
using BackOffice;
using NetTopologySuite.Geometries;
using RoadSegments;

public class RoadSegmentVersion
{
    public required long Position { get; init; }
    public required int Id { get; init; }

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
    public string MaintainerId { get; set; }
    public int MethodId { get; set; }
    public string MethodLabel { get; set; }
    public int MorphologyId { get; set; }
    public string MorphologyLabel { get; set; }
    public int? RightSideStreetNameId { get; set; }
    public int StatusId { get; set; }
    public string StatusLabel { get; set; }
    public string OrganizationId { get; set; }
    public string OrganizationName { get; set; }

    public DateTimeOffset VersionTimestamp { get; set; }
    public DateTimeOffset CreatedOnTimestamp { get; set; }

    public ICollection<RoadSegmentLaneAttributeVersion> Lanes { get; set; } = [];
    public ICollection<RoadSegmentSurfaceAttributeVersion> Surfaces { get; set; } = [];
    public ICollection<RoadSegmentWidthAttributeVersion> Widths { get; set; } = [];
    public ICollection<RoadSegmentEuropeanRoadAttributeVersion> EuropeanRoads { get; set; } = [];
    public ICollection<RoadSegmentNationalRoadAttributeVersion> NationalRoads { get; set; } = [];
    public ICollection<RoadSegmentNumberedRoadAttributeVersion> NumberedRoads { get; set; } = [];

    public RoadSegmentVersion WithBoundingBox(RoadSegmentBoundingBox value)
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

    public RoadSegmentVersion Clone(long newPosition)
    {
        var newItem = new RoadSegmentVersion
        {
            Position = newPosition,
            Id = Id,

            BoundingBoxMaximumM = BoundingBoxMaximumM,
            BoundingBoxMaximumX = BoundingBoxMaximumX,
            BoundingBoxMaximumY = BoundingBoxMaximumY,
            BoundingBoxMinimumM = BoundingBoxMinimumM,
            BoundingBoxMinimumX = BoundingBoxMinimumX,
            BoundingBoxMinimumY = BoundingBoxMinimumY,
            EndNodeId = EndNodeId,
            Geometry = Geometry,
            StartNodeId = StartNodeId,
            IsRemoved = IsRemoved,
            Version = Version,
            GeometryVersion = GeometryVersion,
            AccessRestrictionId = AccessRestrictionId,
            AccessRestrictionLabel = AccessRestrictionLabel,
            CategoryId = CategoryId,
            CategoryLabel = CategoryLabel,
            LeftSideStreetNameId = LeftSideStreetNameId,
            MaintainerId = MaintainerId,
            MethodId = MethodId,
            MethodLabel = MethodLabel,
            MorphologyId = MorphologyId,
            MorphologyLabel = MorphologyLabel,
            RightSideStreetNameId = RightSideStreetNameId,
            StatusId = StatusId,
            StatusLabel = StatusLabel,
            OrganizationId = OrganizationId,
            OrganizationName = OrganizationName,

            VersionTimestamp = VersionTimestamp,
            CreatedOnTimestamp = CreatedOnTimestamp,

            Lanes = Lanes
                .Where(x => !x.IsRemoved)
                .Select(x => x.Clone(newPosition))
                .ToList(),
            Surfaces = Surfaces
                .Where(x => !x.IsRemoved)
                .Select(x => x.Clone(newPosition))
                .ToList(),
            Widths = Widths
                .Where(x => !x.IsRemoved)
                .Select(x => x.Clone(newPosition))
                .ToList(),

            //TODO-rik
            //EuropeanRoads = EuropeanRoads
            //    .Where(x => !x.IsRemoved)
            //    .Select(x => x.Clone(newPosition))
            //    .ToList(),
            //NationalRoads = NationalRoads
            //    .Where(x => !x.IsRemoved)
            //    .Select(x => x.Clone(newPosition))
            //    .ToList(),
            //NumberedRoads = NumberedRoads
            //    .Where(x => !x.IsRemoved)
            //    .Select(x => x.Clone(newPosition))
            //    .ToList()
        };

        return newItem;
    }
}
