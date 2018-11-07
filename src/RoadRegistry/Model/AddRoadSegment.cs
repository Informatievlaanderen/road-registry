namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using Aiv.Vbr.Shaperon;
    using NetTopologySuite.Geometries;

    public class AddRoadSegment : IRequestedChange
    {
        public AddRoadSegment(
            RoadSegmentId id,
            RoadNodeId startNode,
            RoadNodeId endNode,
            MultiLineString geometry,
            MaintainerId maintainer,
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
            RoadSegmentLaneAttributes lanes,
            RoadSegmentWidthAttributes widths,
            RoadSegmentHardeningAttributes hardenings)
        {
            Id = id;
            StartNode = startNode;
            EndNode = endNode;
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            Maintainer = maintainer;
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
            Hardenings = hardenings ?? throw new ArgumentNullException(nameof(hardenings));
        }

        public RoadSegmentId Id { get; }
        public RoadNodeId StartNode { get; }
        public RoadNodeId EndNode { get; }
        public MultiLineString Geometry { get; }
        public MaintainerId Maintainer { get; }
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
        public RoadSegmentLaneAttributes Lanes { get; }
        public RoadSegmentWidthAttributes Widths { get; }
        public RoadSegmentHardeningAttributes Hardenings { get; }
    }
}
