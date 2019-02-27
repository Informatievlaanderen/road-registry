namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using NetTopologySuite.Geometries;

    public class AddRoadSegment : ITranslatedChange
    {
        public AddRoadSegment(
            RoadSegmentId temporaryId,
            RoadNodeId startNodeId,
            RoadNodeId endNodeId,
            MaintenanceAuthorityId maintenanceAuthority,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            RoadSegmentMorphology morphology,
            RoadSegmentStatus status,
            RoadSegmentCategory category,
            RoadSegmentAccessRestriction accessRestriction,
            CrabStreetnameId? leftSideStreetNameId,
            CrabStreetnameId? rightSideStreetNameId)
        {
            TemporaryId = temporaryId;
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

        private AddRoadSegment(
            RoadSegmentId temporaryId,
            RoadNodeId startNodeId,
            RoadNodeId endNodeId,
            MultiLineString geometry,
            MaintenanceAuthorityId maintenanceAuthority,
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
            TemporaryId = temporaryId;
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

        public RoadSegmentId TemporaryId { get; }
        public RoadNodeId StartNodeId { get; }
        public RoadNodeId EndNodeId { get; }
        public MultiLineString Geometry { get; }
        public MaintenanceAuthorityId MaintenanceAuthority { get; }
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

        public AddRoadSegment WithGeometry(MultiLineString geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            return new AddRoadSegment(
                TemporaryId, StartNodeId, EndNodeId, geometry,
                MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
                LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces);
        }

        public AddRoadSegment WithLanes(RoadSegmentLaneAttribute[] lanes)
        {
            if (lanes == null) throw new ArgumentNullException(nameof(lanes));
            return new AddRoadSegment(
                TemporaryId, StartNodeId, EndNodeId, Geometry,
                MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
                LeftSideStreetNameId, RightSideStreetNameId, lanes, Widths, Surfaces);
        }
        
        public AddRoadSegment WithWidths(RoadSegmentWidthAttribute[] widths)
        {
            if (widths == null) throw new ArgumentNullException(nameof(widths));
            return new AddRoadSegment(
                TemporaryId, StartNodeId, EndNodeId, Geometry,
                MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
                LeftSideStreetNameId, RightSideStreetNameId, Lanes, widths, Surfaces);
        }
        
        public AddRoadSegment WithSurfaces(RoadSegmentSurfaceAttribute[] surfaces)
        {
            if (surfaces == null) throw new ArgumentNullException(nameof(surfaces));
            return new AddRoadSegment(
                TemporaryId, StartNodeId, EndNodeId, Geometry,
                MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
                LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, surfaces);
        }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegment = new Messages.AddRoadSegment
            {
                TemporaryId = TemporaryId,
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