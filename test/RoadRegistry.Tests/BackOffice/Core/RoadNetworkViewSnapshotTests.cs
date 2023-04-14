namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using KellermanSoftware.CompareNetObjects;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;
using Point = RoadRegistry.BackOffice.Messages.Point;
using RoadSegmentLaneAttribute = RoadRegistry.BackOffice.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = RoadRegistry.BackOffice.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = RoadRegistry.BackOffice.RoadSegmentWidthAttribute;

public class RoadNetworkViewSnapshotTests
{
    private readonly Fixture _fixture;

    public RoadNetworkViewSnapshotTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadNodeType();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeRoadSegmentAccessRestriction();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeCrabStreetnameId();
        _fixture.CustomizeEuropeanRoadNumber();
        _fixture.CustomizeNationalRoadNumber();
        _fixture.CustomizeNumberedRoadNumber();
        _fixture.CustomizeGradeSeparatedJunctionId();
        _fixture.CustomizeGradeSeparatedJunctionType();
        _fixture.CustomizeTransactionId();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeRoadSegmentLaneDirection();
        _fixture.CustomizeRoadSegmentLaneCount();
        _fixture.CustomizeRoadSegmentLaneAttribute();
        _fixture.CustomizeRoadSegmentSurfaceType();
        _fixture.CustomizeRoadSegmentSurfaceAttribute();
        _fixture.CustomizeRoadSegmentWidth();
        _fixture.CustomizeRoadSegmentWidthAttribute();

        _fixture.Customize<RoadNodeGeometry>(customizer =>
            customizer.FromFactory(_ => new RoadNodeGeometry
            {
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                Point = _fixture.Create<Point>()
            }).OmitAutoProperties());

        _fixture.Customize<RoadNetworkSnapshotNode>(customizer =>
            customizer.FromFactory(_ => new RoadNetworkSnapshotNode
            {
                Id = _fixture.Create<RoadNodeId>(),
                Type = _fixture.Create<RoadNodeType>(),
                Geometry = _fixture.Create<RoadNodeGeometry>(),
                Segments = _fixture.CreateMany<RoadSegmentId>(10).Distinct().Select(id => id.ToInt32()).ToArray()
            }).OmitAutoProperties());

        _fixture.CustomizeRoadSegmentGeometry();

        _fixture.Customize<RoadNetworkSnapshotSegmentLaneAttribute>(customizer =>
            customizer.FromFactory(_ =>
            {
                var attribute = _fixture.Create<RoadSegmentLaneAttribute>();
                return new RoadNetworkSnapshotSegmentLaneAttribute
                {
                    FromPosition = attribute.From,
                    ToPosition = attribute.To,
                    Count = attribute.Count,
                    Direction = attribute.Direction.Translation.Identifier
                };
            }).OmitAutoProperties());

        _fixture.Customize<RoadNetworkSnapshotSegmentSurfaceAttribute>(customizer =>
            customizer.FromFactory(_ =>
            {
                var attribute = _fixture.Create<RoadSegmentSurfaceAttribute>();
                return new RoadNetworkSnapshotSegmentSurfaceAttribute
                {
                    FromPosition = attribute.From,
                    ToPosition = attribute.To,
                    Type = attribute.Type.Translation.Identifier
                };
            }).OmitAutoProperties());

        _fixture.Customize<RoadNetworkSnapshotSegmentWidthAttribute>(customizer =>
            customizer.FromFactory(_ =>
            {
                var attribute = _fixture.Create<RoadSegmentWidthAttribute>();
                return new RoadNetworkSnapshotSegmentWidthAttribute
                {
                    FromPosition = attribute.From,
                    ToPosition = attribute.To,
                    Width = attribute.Width
                };
            }).OmitAutoProperties());

        _fixture.Customize<RoadNetworkSnapshotSegmentAttributeHash>(customizer =>
            customizer.FromFactory(_ =>
            {
                var geometryDrawMethod = _fixture.Create<RoadSegmentGeometryDrawMethod>();

                return new RoadNetworkSnapshotSegmentAttributeHash
                {
                    AccessRestriction = _fixture.Create<RoadSegmentAccessRestriction>(),
                    Category = _fixture.Create<RoadSegmentCategory>(),
                    Morphology = _fixture.Create<RoadSegmentMorphology>(),
                    Status = _fixture.Create<RoadSegmentStatus>(),
                    OrganizationId = _fixture.Create<OrganizationId>(),
                    LeftSideStreetNameId = _fixture.Create<CrabStreetnameId>().ToInt32(),
                    RightSideStreetNameId = _fixture.Create<CrabStreetnameId>().ToInt32(),
                    GeometryDrawMethod = geometryDrawMethod.ToString(),
                    Lanes = _fixture.CreateMany<RoadNetworkSnapshotSegmentLaneAttribute>(geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined ? 1 : 10).ToArray(),
                    Surfaces = _fixture.CreateMany<RoadNetworkSnapshotSegmentSurfaceAttribute>(geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined ? 1 : 10).ToArray(),
                    Widths = _fixture.CreateMany<RoadNetworkSnapshotSegmentWidthAttribute>(geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined ? 1 : 10).ToArray()
                };
            }).OmitAutoProperties());

        _fixture.Customize<RoadNetworkSnapshotSegment>(customizer =>
            customizer.FromFactory(_ => new RoadNetworkSnapshotSegment
            {
                Id = _fixture.Create<RoadNodeId>(),
                Geometry = _fixture.Create<RoadSegmentGeometry>(),
                StartNodeId = _fixture.Create<RoadNodeId>(),
                EndNodeId = _fixture.Create<RoadNodeId>(),
                AttributeHash = _fixture.Create<RoadNetworkSnapshotSegmentAttributeHash>(),
                PartOfEuropeanRoads = _fixture.CreateMany<EuropeanRoadNumber>(10).Distinct().Select(number => number.ToString()).ToArray(),
                PartOfNationalRoads = _fixture.CreateMany<NationalRoadNumber>(10).Distinct().Select(number => number.ToString()).ToArray(),
                PartOfNumberedRoads = _fixture.CreateMany<NumberedRoadNumber>(10).Distinct().Select(number => number.ToString()).ToArray()
            }).OmitAutoProperties());

        _fixture.Customize<RoadNetworkSnapshotGradeSeparatedJunction>(customizer =>
            customizer.FromFactory(_ => new RoadNetworkSnapshotGradeSeparatedJunction
            {
                Id = _fixture.Create<GradeSeparatedJunctionId>(),
                Type = _fixture.Create<GradeSeparatedJunctionType>(),
                LowerSegmentId = _fixture.Create<RoadSegmentId>(),
                UpperSegmentId = _fixture.Create<RoadSegmentId>()
            }).OmitAutoProperties());

        _fixture.Customize<RoadNetworkSnapshotSegmentReusableAttributeIdentifiers>(customizer =>
            customizer.FromFactory(_ => new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
            {
                SegmentId = _fixture.Create<RoadSegmentId>(),
                ReusableAttributeIdentifiers = _fixture.CreateMany<AttributeId>(10).Distinct().Select(id => id.ToInt32()).ToArray()
            }).OmitAutoProperties());
    }

    [Fact]
    public void Taking_a_snapshot_of_a_mutable_view_restored_from_a_known_snapshot_results_in_a_snapshot_that_equals_the_known_snapshot()
    {
        var snapshotBefore = new RoadNetworkSnapshot
        {
            Nodes = _fixture.CreateMany<RoadNetworkSnapshotNode>(10).ToArray(),
            Segments = _fixture.CreateMany<RoadNetworkSnapshotSegment>(10).ToArray(),
            GradeSeparatedJunctions = _fixture.CreateMany<RoadNetworkSnapshotGradeSeparatedJunction>(10).ToArray(),
            MaximumNodeId = _fixture.Create<RoadNodeId>(),
            MaximumSegmentId = _fixture.Create<RoadSegmentId>(),
            MaximumGradeSeparatedJunctionId = _fixture.Create<GradeSeparatedJunctionId>(),
            MaximumTransactionId = _fixture.Create<TransactionId>(),
            MaximumLaneAttributeId = _fixture.Create<AttributeId>(),
            MaximumSurfaceAttributeId = _fixture.Create<AttributeId>(),
            MaximumWidthAttributeId = _fixture.Create<AttributeId>(),
            MaximumEuropeanRoadAttributeId = _fixture.Create<AttributeId>(),
            MaximumNationalRoadAttributeId = _fixture.Create<AttributeId>(),
            MaximumNumberedRoadAttributeId = _fixture.Create<AttributeId>(),
            SegmentReusableLaneAttributeIdentifiers = _fixture.CreateMany<RoadNetworkSnapshotSegmentReusableAttributeIdentifiers>(10).ToArray(),
            SegmentReusableSurfaceAttributeIdentifiers = _fixture.CreateMany<RoadNetworkSnapshotSegmentReusableAttributeIdentifiers>(10).ToArray(),
            SegmentReusableWidthAttributeIdentifiers = _fixture.CreateMany<RoadNetworkSnapshotSegmentReusableAttributeIdentifiers>(10).ToArray()
        };
        var sut = ImmutableRoadNetworkView.Empty.ToBuilder();
        var restored = sut.RestoreFromSnapshot(snapshotBefore);

        var snapshotAfter = restored.TakeSnapshot();

        var comparer = new CompareLogic { Config = { MaxDifferences = int.MaxValue, IgnoreCollectionOrder = true, ComparePrivateFields = false, ComparePrivateProperties = false } };
        var result = comparer.Compare(snapshotBefore, snapshotAfter);
        Assert.True(result.AreEqual, result.DifferencesString);
    }

    [Fact]
    public void Taking_a_snapshot_of_an_immutable_view_restored_from_a_known_snapshot_results_in_a_snapshot_that_equals_the_known_snapshot()
    {
        var snapshotBefore = new RoadNetworkSnapshot
        {
            Nodes = _fixture.CreateMany<RoadNetworkSnapshotNode>(10).ToArray(),
            Segments = _fixture.CreateMany<RoadNetworkSnapshotSegment>(10).ToArray(),
            GradeSeparatedJunctions = _fixture.CreateMany<RoadNetworkSnapshotGradeSeparatedJunction>(10).ToArray(),
            MaximumNodeId = _fixture.Create<RoadNodeId>(),
            MaximumSegmentId = _fixture.Create<RoadSegmentId>(),
            MaximumGradeSeparatedJunctionId = _fixture.Create<GradeSeparatedJunctionId>(),
            MaximumTransactionId = _fixture.Create<TransactionId>(),
            MaximumLaneAttributeId = _fixture.Create<AttributeId>(),
            MaximumSurfaceAttributeId = _fixture.Create<AttributeId>(),
            MaximumWidthAttributeId = _fixture.Create<AttributeId>(),
            MaximumEuropeanRoadAttributeId = _fixture.Create<AttributeId>(),
            MaximumNationalRoadAttributeId = _fixture.Create<AttributeId>(),
            MaximumNumberedRoadAttributeId = _fixture.Create<AttributeId>(),
            SegmentReusableLaneAttributeIdentifiers = _fixture.CreateMany<RoadNetworkSnapshotSegmentReusableAttributeIdentifiers>(10).ToArray(),
            SegmentReusableSurfaceAttributeIdentifiers = _fixture.CreateMany<RoadNetworkSnapshotSegmentReusableAttributeIdentifiers>(10).ToArray(),
            SegmentReusableWidthAttributeIdentifiers = _fixture.CreateMany<RoadNetworkSnapshotSegmentReusableAttributeIdentifiers>(10).ToArray()
        };
        var sut = ImmutableRoadNetworkView.Empty;
        var restored = sut.RestoreFromSnapshot(snapshotBefore);

        var snapshotAfter = restored.TakeSnapshot();

        var comparer = new CompareLogic { Config = { MaxDifferences = int.MaxValue, IgnoreCollectionOrder = true, ComparePrivateFields = false, ComparePrivateProperties = false } };
        var result = comparer.Compare(snapshotBefore, snapshotAfter);
        Assert.True(result.AreEqual, result.DifferencesString);
    }
}
