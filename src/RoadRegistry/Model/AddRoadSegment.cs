namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
    using Messages;
    using NetTopologySuite.Geometries;

    public class AddRoadSegment : IRequestedChange
    {
        public AddRoadSegment(
            RoadSegmentId id,
            RoadNodeId startNode,
            RoadNodeId endNode,
            MultiLineString geometry,
            MaintenanceAuthorityId maintenanceAuthority,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            RoadSegmentMorphology morphology,
            RoadSegmentStatus status,
            RoadSegmentCategory category,
            RoadSegmentAccessRestriction accessRestriction,
            CrabStreetnameId? leftSideStreetNameId,
            CrabStreetnameId? rightSideStreetNameId,
            IReadOnlyCollection<EuropeanRoadNumber> partOfEuropeanRoads,
            IReadOnlyCollection<NationalRoadNumber> partOfNationalRoads,
            IReadOnlyCollection<RoadSegmentNumberedRoadAttribute> partOfNumberedRoads,
            IReadOnlyCollection<RoadSegmentLaneAttribute> lanes,
            IReadOnlyCollection<RoadSegmentWidthAttribute> widths,
            IReadOnlyCollection<RoadSegmentSurfaceAttribute> surfaces)
        {
            Id = id;
            StartNode = startNode;
            EndNode = endNode;
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            MaintenanceAuthority = maintenanceAuthority;
            GeometryDrawMethod = geometryDrawMethod ?? throw new ArgumentNullException(nameof(geometryDrawMethod));
            Morphology = morphology ?? throw new ArgumentNullException(nameof(morphology));
            Status = status ?? throw new ArgumentNullException(nameof(status));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            AccessRestriction = accessRestriction ?? throw new ArgumentNullException(nameof(accessRestriction));
            LeftSideStreetNameId = leftSideStreetNameId;
            RightSideStreetNameId = rightSideStreetNameId;
            PartOfEuropeanRoads = partOfEuropeanRoads ?? throw new ArgumentNullException(nameof(partOfEuropeanRoads));
            PartOfNationalRoads = partOfNationalRoads ?? throw new ArgumentNullException(nameof(partOfNationalRoads));
            PartOfNumberedRoads = partOfNumberedRoads ?? throw new ArgumentNullException(nameof(partOfNumberedRoads));
            Lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
            Widths = widths ?? throw new ArgumentNullException(nameof(widths));
            Surfaces = surfaces ?? throw new ArgumentNullException(nameof(surfaces));
        }

        public RoadSegmentId Id { get; }
        public RoadNodeId StartNode { get; }
        public RoadNodeId EndNode { get; }
        public MultiLineString Geometry { get; }
        public MaintenanceAuthorityId MaintenanceAuthority { get; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
        public RoadSegmentMorphology Morphology { get; }
        public RoadSegmentStatus Status { get; }
        public RoadSegmentCategory Category { get; }
        public RoadSegmentAccessRestriction AccessRestriction { get; }
        public Nullable<CrabStreetnameId> LeftSideStreetNameId { get; }
        public Nullable<CrabStreetnameId> RightSideStreetNameId { get; }
        public IReadOnlyCollection<EuropeanRoadNumber> PartOfEuropeanRoads { get; }
        public IReadOnlyCollection<NationalRoadNumber> PartOfNationalRoads { get; }
        public IReadOnlyCollection<RoadSegmentNumberedRoadAttribute> PartOfNumberedRoads { get; }
        public IReadOnlyCollection<RoadSegmentLaneAttribute> Lanes { get; }
        public IReadOnlyCollection<RoadSegmentWidthAttribute> Widths { get; }
        public IReadOnlyCollection<RoadSegmentSurfaceAttribute> Surfaces { get; }

        public AcceptedChange Accept(WellKnownBinaryWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            return new AcceptedChange
            {
                RoadSegmentAdded = new RoadSegmentAdded
                {
                    Id = Id,
                    StartNodeId = StartNode,
                    EndNodeId = EndNode,
                    Geometry = writer.Write(Geometry),
                    MaintenanceAuthority = new MaintenanceAuthority
                    {
                        Code = MaintenanceAuthority
                    },
                    GeometryDrawMethod = GeometryDrawMethod,
                    Morphology = Morphology,
                    Status = Status,
                    Category = Category,
                    AccessRestriction = AccessRestriction,
                    LeftSide = new RoadSegmentSideAttributes
                    {
                        StreetNameId = LeftSideStreetNameId.GetValueOrDefault()
                    },
                    RightSide = new RoadSegmentSideAttributes
                    {
                        StreetNameId = RightSideStreetNameId.GetValueOrDefault()
                    },
                    PartOfEuropeanRoads = PartOfEuropeanRoads
                        .Select(item => new RoadSegmentEuropeanRoadAttributes
                        {
                            AttributeId = 1,
                            RoadNumber = item
                        })
                        .ToArray(),
                    PartOfNationalRoads = PartOfNationalRoads
                        .Select(item => new RoadSegmentNationalRoadAttributes
                        {
                            AttributeId = 1,
                            Ident2 = item
                        })
                        .ToArray(),
                    PartOfNumberedRoads = PartOfNumberedRoads
                        .Select(item => new RoadSegmentNumberedRoadAttributes
                        {
                            AttributeId = 1,
                            Direction = item.Direction,
                            Ident8 = item.Number,
                            Ordinal = item.Ordinal
                        })
                        .ToArray(),
                    Lanes = Lanes
                        .Select(item => new Messages.RoadSegmentLaneAttributes
                        {
                            AttributeId = 1,
                            AsOfGeometryVersion = 1,
                            Count = item.Count,
                            Direction = item.Direction,
                            FromPosition = item.From,
                            ToPosition = item.To
                        })
                        .ToArray(),
                    Widths = Widths
                        .Select(item => new Messages.RoadSegmentWidthAttributes
                        {
                            AttributeId = 1,
                            AsOfGeometryVersion = 1,
                            Width = item.Width,
                            FromPosition = item.From,
                            ToPosition = item.To
                        })
                        .ToArray(),
                    Surfaces = Surfaces
                        .Select(item => new Messages.RoadSegmentSurfaceAttributes
                        {
                            AttributeId = 1,
                            AsOfGeometryVersion = 1,
                            Type = item.Type,
                            FromPosition = item.From,
                            ToPosition = item.To
                        })
                        .ToArray()
                }
            };
        }
    }
}
