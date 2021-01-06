namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;
    using NetTopologySuite.Geometries;

    public class ModifyRoadSegment : ITranslatedChange
    {
        public ModifyRoadSegment(
            RecordNumber recordNumber,
            RoadSegmentId id,
            RoadNodeId startNodeId,
            RoadNodeId endNodeId,
            OrganizationId maintenanceAuthority,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            RoadSegmentMorphology morphology,
            RoadSegmentStatus status,
            RoadSegmentCategory category,
            RoadSegmentAccessRestriction accessRestriction,
            CrabStreetnameId? leftSideStreetNameId,
            CrabStreetnameId? rightSideStreetNameId)
        {
            RecordNumber = recordNumber;
            Id = id;
            StartNodeId = startNodeId;
            EndNodeId = endNodeId;
            Geometry = null;
            MaintenanceAuthority = maintenanceAuthority;
            GeometryDrawMethod = geometryDrawMethod ?? throw new ArgumentNullException(nameof(geometryDrawMethod));
            Morphology = morphology ?? throw new ArgumentNullException(nameof(morphology));
            Status = status ?? throw new ArgumentNullException(nameof(status));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            AccessRestriction = accessRestriction ?? throw new ArgumentNullException(nameof(accessRestriction));
            LeftSideStreetNameId = leftSideStreetNameId;
            RightSideStreetNameId = rightSideStreetNameId;
            Lanes = new RoadSegmentLaneAttribute[0];
            Widths = new RoadSegmentWidthAttribute[0];
            Surfaces = new RoadSegmentSurfaceAttribute[0];
        }

        private ModifyRoadSegment(
            RecordNumber recordNumber,
            RoadSegmentId id,
            RoadNodeId startNodeId,
            RoadNodeId endNodeId,
            MultiLineString geometry,
            OrganizationId maintenanceAuthority,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            RoadSegmentMorphology morphology,
            RoadSegmentStatus status,
            RoadSegmentCategory category,
            RoadSegmentAccessRestriction accessRestriction,
            CrabStreetnameId? leftSideStreetNameId,
            CrabStreetnameId? rightSideStreetNameId,
            IReadOnlyList<RoadSegmentLaneAttribute> lanes,
            IReadOnlyList<RoadSegmentWidthAttribute> widths,
            IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces)
        {
            RecordNumber = recordNumber;
            Id = id;
            StartNodeId = startNodeId;
            EndNodeId = endNodeId;
            Geometry = geometry;
            MaintenanceAuthority = maintenanceAuthority;
            GeometryDrawMethod = geometryDrawMethod;
            Morphology = morphology;
            Status = status;
            Category = category;
            AccessRestriction = accessRestriction;
            LeftSideStreetNameId = leftSideStreetNameId;
            RightSideStreetNameId = rightSideStreetNameId;
            Lanes = lanes;
            Widths = widths;
            Surfaces = surfaces;
        }

        public RecordNumber RecordNumber { get; }
        public RoadSegmentId Id { get; }
        public RoadNodeId StartNodeId { get; }
        public RoadNodeId EndNodeId { get; }
        public MultiLineString Geometry { get; }
        public OrganizationId MaintenanceAuthority { get; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
        public RoadSegmentMorphology Morphology { get; }
        public RoadSegmentStatus Status { get; }
        public RoadSegmentCategory Category { get; }
        public RoadSegmentAccessRestriction AccessRestriction { get; }
        public CrabStreetnameId? LeftSideStreetNameId { get; }
        public CrabStreetnameId? RightSideStreetNameId { get; }
        public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
        public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }
        public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }

        public ModifyRoadSegment WithGeometry(MultiLineString geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            return new ModifyRoadSegment(
                RecordNumber, Id, StartNodeId, EndNodeId, geometry,
                MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
                LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces);
        }

        public ModifyRoadSegment WithLane(RoadSegmentLaneAttribute lane)
        {
            var lanes = new List<RoadSegmentLaneAttribute>(Lanes) {lane};
            lanes.Sort((left, right) => left.From.CompareTo(right.From));
            return new ModifyRoadSegment(
                RecordNumber, Id, StartNodeId, EndNodeId, Geometry,
                MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
                LeftSideStreetNameId, RightSideStreetNameId,
                lanes, Widths, Surfaces);
        }

        public ModifyRoadSegment WithWidth(RoadSegmentWidthAttribute width)
        {
            var widths = new List<RoadSegmentWidthAttribute>(Widths) { width };
            widths.Sort((left, right) => left.From.CompareTo(right.From));
            return new ModifyRoadSegment(
                RecordNumber, Id, StartNodeId, EndNodeId, Geometry,
                MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
                LeftSideStreetNameId, RightSideStreetNameId,
                Lanes, widths, Surfaces);
        }

        public ModifyRoadSegment WithSurface(RoadSegmentSurfaceAttribute surface)
        {
            var surfaces = new List<RoadSegmentSurfaceAttribute>(Surfaces) { surface };
            surfaces.Sort((left, right) => left.From.CompareTo(right.From));
            return new ModifyRoadSegment(
                RecordNumber, Id, StartNodeId, EndNodeId, Geometry,
                MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
                LeftSideStreetNameId, RightSideStreetNameId,
                Lanes, Widths, surfaces);
        }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.ModifyRoadSegment = new Messages.ModifyRoadSegment
            {
                Id = Id,
                StartNodeId = StartNodeId,
                EndNodeId = EndNodeId,
                Geometry = GeometryTranslator.Translate(Geometry),
                MaintenanceAuthority = MaintenanceAuthority,
                GeometryDrawMethod = GeometryDrawMethod,
                Morphology = Morphology,
                Status = Status,
                Category = Category,
                AccessRestriction = AccessRestriction,
                LeftSideStreetNameId = LeftSideStreetNameId.GetValueOrDefault(),
                RightSideStreetNameId = RightSideStreetNameId.GetValueOrDefault(),
                Lanes = Lanes
                    .Select(item => new Messages.RequestedRoadSegmentLaneAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Count = item.Count,
                        Direction = item.Direction,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Widths = Widths
                    .Select(item => new Messages.RequestedRoadSegmentWidthAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Width = item.Width,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Surfaces = Surfaces
                    .Select(item => new Messages.RequestedRoadSegmentSurfaceAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Type = item.Type,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray()
            };
        }
    }
}
