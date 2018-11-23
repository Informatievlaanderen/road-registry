namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class AddRoadSegment : IRequestedChange
    {
        public AddRoadSegment(RoadSegmentId id,
            RoadSegmentId temporaryId,
            RoadNodeId startNodeId,
            RoadNodeId? temporaryStartNodeId,
            RoadNodeId endNodeId,
            RoadNodeId? temporaryEndNodeId,
            MultiLineString geometry,
            MaintenanceAuthorityId maintenanceAuthority,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            RoadSegmentMorphology morphology,
            RoadSegmentStatus status,
            RoadSegmentCategory category,
            RoadSegmentAccessRestriction accessRestriction,
            CrabStreetnameId? leftSideStreetNameId,
            CrabStreetnameId? rightSideStreetNameId,
            IReadOnlyCollection<RoadSegmentEuropeanRoadAttribute> partOfEuropeanRoads,
            IReadOnlyCollection<RoadSegmentNationalRoadAttribute> partOfNationalRoads,
            IReadOnlyCollection<RoadSegmentNumberedRoadAttribute> partOfNumberedRoads,
            IReadOnlyCollection<RoadSegmentLaneAttribute> lanes,
            IReadOnlyCollection<RoadSegmentWidthAttribute> widths,
            IReadOnlyCollection<RoadSegmentSurfaceAttribute> surfaces)
        {
            Id = id;
            TemporaryId = temporaryId;
            StartNodeId = startNodeId;
            TemporaryStartNodeId = temporaryStartNodeId;
            EndNodeId = endNodeId;
            TemporaryEndNodeId = temporaryEndNodeId;
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
        public RoadSegmentId TemporaryId { get; }
        public RoadNodeId StartNodeId { get; }
        public RoadNodeId? TemporaryStartNodeId { get; }
        public RoadNodeId EndNodeId { get; }
        public RoadNodeId? TemporaryEndNodeId { get; }
        public MultiLineString Geometry { get; }
        public MaintenanceAuthorityId MaintenanceAuthority { get; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
        public RoadSegmentMorphology Morphology { get; }
        public RoadSegmentStatus Status { get; }
        public RoadSegmentCategory Category { get; }
        public RoadSegmentAccessRestriction AccessRestriction { get; }
        public Nullable<CrabStreetnameId> LeftSideStreetNameId { get; }
        public Nullable<CrabStreetnameId> RightSideStreetNameId { get; }
        public IReadOnlyCollection<RoadSegmentEuropeanRoadAttribute> PartOfEuropeanRoads { get; }
        public IReadOnlyCollection<RoadSegmentNationalRoadAttribute> PartOfNationalRoads { get; }
        public IReadOnlyCollection<RoadSegmentNumberedRoadAttribute> PartOfNumberedRoads { get; }
        public IReadOnlyCollection<RoadSegmentLaneAttribute> Lanes { get; }
        public IReadOnlyCollection<RoadSegmentWidthAttribute> Widths { get; }
        public IReadOnlyCollection<RoadSegmentSurfaceAttribute> Surfaces { get; }

        public Messages.AcceptedChange Accept()
        {
            return new Messages.AcceptedChange
            {
                RoadSegmentAdded = new Messages.RoadSegmentAdded
                {
                    Id = Id,
                    TemporaryId = TemporaryId,
                    StartNodeId = StartNodeId,
                    EndNodeId = EndNodeId,
                    Geometry = GeometryTranslator.Translate(Geometry),
                    MaintenanceAuthority = new Messages.MaintenanceAuthority
                    {
                        Code = MaintenanceAuthority
                    },
                    GeometryDrawMethod = GeometryDrawMethod,
                    Morphology = Morphology,
                    Status = Status,
                    Category = Category,
                    AccessRestriction = AccessRestriction,
                    LeftSide = new Messages.RoadSegmentSideAttributes
                    {
                        StreetNameId = LeftSideStreetNameId.GetValueOrDefault()
                    },
                    RightSide = new Messages.RoadSegmentSideAttributes
                    {
                        StreetNameId = RightSideStreetNameId.GetValueOrDefault()
                    },
                    PartOfEuropeanRoads = PartOfEuropeanRoads
                        .Select(item => new Messages.RoadSegmentEuropeanRoadAttributes
                        {
                            AttributeId = item.Id,
                            RoadNumber = item.Number
                        })
                        .ToArray(),
                    PartOfNationalRoads = PartOfNationalRoads
                        .Select(item => new Messages.RoadSegmentNationalRoadAttributes
                        {
                            AttributeId = item.Id,
                            Ident2 = item.Number
                        })
                        .ToArray(),
                    PartOfNumberedRoads = PartOfNumberedRoads
                        .Select(item => new Messages.RoadSegmentNumberedRoadAttributes
                        {
                            AttributeId = item.Id,
                            Direction = item.Direction,
                            Ident8 = item.Number,
                            Ordinal = item.Ordinal
                        })
                        .ToArray(),
                    Lanes = Lanes
                        .Select(item => new Messages.RoadSegmentLaneAttributes
                        {
                            AttributeId = item.Id,
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
                            AttributeId = item.Id,
                            AsOfGeometryVersion = 1,
                            Width = item.Width,
                            FromPosition = item.From,
                            ToPosition = item.To
                        })
                        .ToArray(),
                    Surfaces = Surfaces
                        .Select(item => new Messages.RoadSegmentSurfaceAttributes
                        {
                            AttributeId = item.Id,
                            AsOfGeometryVersion = 1,
                            Type = item.Type,
                            FromPosition = item.From,
                            ToPosition = item.To
                        })
                        .ToArray()
                }
            };
        }

        public Messages.RejectedChange Reject(IEnumerable<Messages.Reason> reasons)
        {
            return new Messages.RejectedChange
            {
                AddRoadSegment = new Messages.AddRoadSegment
                {
                    TemporaryId = TemporaryId,
                    StartNodeId = TemporaryStartNodeId ?? StartNodeId,
                    EndNodeId = TemporaryEndNodeId ?? EndNodeId,
                    Geometry = GeometryTranslator.Translate(Geometry),
                    MaintenanceAuthority = MaintenanceAuthority,
                    GeometryDrawMethod = GeometryDrawMethod,
                    Morphology = Morphology,
                    Status = Status,
                    Category = Category,
                    AccessRestriction = AccessRestriction,
                    LeftSideStreetNameId = LeftSideStreetNameId.GetValueOrDefault(),
                    RightSideStreetNameId = RightSideStreetNameId.GetValueOrDefault(),
                    PartOfEuropeanRoads = PartOfEuropeanRoads
                        .Select(item => new Messages.RequestedRoadSegmentEuropeanRoadAttributes
                        {
                            RoadNumber = item.Number
                        })
                        .ToArray(),
                    PartOfNationalRoads = PartOfNationalRoads
                        .Select(item => new Messages.RequestedRoadSegmentNationalRoadAttributes
                        {
                            Ident2 = item.Number
                        })
                        .ToArray(),
                    PartOfNumberedRoads = PartOfNumberedRoads
                        .Select(item => new Messages.RequestedRoadSegmentNumberedRoadAttributes
                        {
                            Direction = item.Direction,
                            Ident8 = item.Number,
                            Ordinal = item.Ordinal
                        })
                        .ToArray(),
                    Lanes = Lanes
                        .Select(item => new Messages.RequestedRoadSegmentLaneAttributes
                        {
                            Count = item.Count,
                            Direction = item.Direction,
                            FromPosition = item.From,
                            ToPosition = item.To
                        })
                        .ToArray(),
                    Widths = Widths
                        .Select(item => new Messages.RequestedRoadSegmentWidthAttributes
                        {
                            Width = item.Width,
                            FromPosition = item.From,
                            ToPosition = item.To
                        })
                        .ToArray(),
                    Surfaces = Surfaces
                        .Select(item => new Messages.RequestedRoadSegmentSurfaceAttributes
                        {
                            Type = item.Type,
                            FromPosition = item.From,
                            ToPosition = item.To
                        })
                        .ToArray()
                },
                Reasons = reasons.ToArray()
            };
        }
    }
}
