namespace RoadRegistry.Projections.Tests.Projections.ExtractRoadSegment;

using AutoFixture;
using Extensions;
using Extracts.Projections;
using FluentAssertions;
using GradeSeparatedJunction.Events.V1;
using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using NetTopologySuite.Geometries;
using RoadNode.Events.V1;
using RoadNode.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadSegment.Events.V1;
using RoadSegment.Events.V1.ValueObjects;
using RoadSegment.Events.V2;
using RoadSegment.ValueObjects;
using ScopedRoadNetwork.Events.V1;
using ScopedRoadNetwork.Events.V2;

public class RoadSegmentProjectionTests
{
    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        var excludeEventTypes = new[]
        {
            typeof(RoadNetworkChangesAccepted),
            typeof(ImportedRoadNode),
            typeof(RoadNodeAdded),
            typeof(RoadNodeModified),
            typeof(RoadNodeRemoved),
            typeof(ImportedGradeSeparatedJunction),
            typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified),
            typeof(GradeSeparatedJunctionRemoved),

            typeof(RoadNetworkWasChanged),
            typeof(RoadNodeWasAdded),
            typeof(RoadNodeTypeWasChanged),
            typeof(RoadNodeWasModified),
            typeof(RoadNodeWasMigrated),
            typeof(RoadNodeWasRemoved),
            typeof(GradeSeparatedJunctionWasAdded),
            typeof(GradeSeparatedJunctionWasModified),
            typeof(GradeSeparatedJunctionWasRemoved)
        };
        var allEventTypes = typeof(IMartenEvent).Assembly
            .GetTypes()
            .Where(x => typeof(IMartenEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .ToArray();
        allEventTypes.Should().NotBeEmpty();

        var usedEventTypes = BuildProjection().Handlers
            .Select(x => typeof(IEvent).IsAssignableFrom(x.Message) ? x.Message.GetGenericArguments().Single() : x.Message)
            .ToArray();
        usedEventTypes.Should().NotBeEmpty();
        usedEventTypes.Distinct().Count().Should().Be(usedEventTypes.Length);

        var missingEventTypes = allEventTypes.Except(usedEventTypes).Except(excludeEventTypes).ToArray();
        if (missingEventTypes.Any())
        {
            Assert.Fail($"Missing handlers for event types:{Environment.NewLine}{string.Join(Environment.NewLine, missingEventTypes.Select(x => x.FullName).OrderBy(x => x))}");
        }
    }

    [Theory]
    [InlineData(100.02, false)]
    [InlineData(100.01, true)]
    [InlineData(99.99, true)]
    [InlineData(99.98, false)]
    public Task V1_WhenSurfaceTypeToPositionIs1CmDifferentThanGeometryLength_ThenGeometryLengthIsUsed(double surfaceTypeToPosition, bool expectGeometryLength)
    {
        var v1Fixture = new RoadNetworkTestData().ObjectProvider;
        v1Fixture.CustomizeUniqueInteger();

        var roadSegment1Added = v1Fixture.Create<RoadSegmentAdded>();
        roadSegment1Added.LeftSide = new RoadSegmentSideAttributes { StreetNameId = StreetNameLocalId.NotApplicable };
        roadSegment1Added.RightSide = new RoadSegmentSideAttributes { StreetNameId = StreetNameLocalId.NotApplicable };
        roadSegment1Added.MaintenanceAuthority.Code = OrganizationId.Unknown.ToString();

        roadSegment1Added.Geometry = BuildRoadSegmentGeometry(0, 0, 100, 0);
        roadSegment1Added.Surfaces =
        [
            new RoadSegmentSurfaceAttributes
            {
                FromPosition = 0,
                ToPosition = surfaceTypeToPosition,
                Type = v1Fixture.Create<RoadSegmentSurfaceType>().ToString(),
                AsOfGeometryVersion = 0,
                AttributeId = 1
            }
        ];

        var expectedRoadSegment1 = new RoadSegmentExtractItem
        {
            RoadSegmentId = new RoadSegmentId(roadSegment1Added.RoadSegmentId),
            Geometry = new RoadSegmentGeometry(3812, "MULTILINESTRING ((500021.1638673437 499983.87319930363, 500121.1630778698 499983.8851437904))"),
            StartNodeId = new RoadNodeId(roadSegment1Added.StartNodeId),
            EndNodeId = new RoadNodeId(roadSegment1Added.EndNodeId),
            GeometryDrawMethod = roadSegment1Added.GeometryDrawMethod,
            Status = roadSegment1Added.Status,
            AccessRestriction = ForEntireLength(roadSegment1Added.AccessRestriction, roadSegment1Added.Geometry),
            Category = ForEntireLength(roadSegment1Added.Category, roadSegment1Added.Geometry),
            Morphology = ForEntireLength(roadSegment1Added.Morphology, roadSegment1Added.Geometry),
            StreetNameId = ForEntireLength(StreetNameLocalId.NotApplicable, roadSegment1Added.Geometry),
            MaintenanceAuthorityId = ForEntireLength(new OrganizationId(roadSegment1Added.MaintenanceAuthority.Code), roadSegment1Added.Geometry),
            SurfaceType = expectGeometryLength
                ? ForEntireLength(roadSegment1Added.Surfaces.Single().Type, roadSegment1Added.Geometry)
                : new ExtractRoadSegmentDynamicAttribute<string>([(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(surfaceTypeToPosition), RoadSegmentAttributeSide.Both, roadSegment1Added.Surfaces.Single().Type)]),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = [],
            Origin = roadSegment1Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment1Added.Provenance.ToEventTimestamp(),
            IsV2 = false
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegment1Added)
            .Expect(expectedRoadSegment1);
    }

    [Fact]
    public Task WhenRoadSegmentWasAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.CustomizeUniqueInteger();

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>();
        var roadSegment2Added = fixture.Create<RoadSegmentWasAdded>();

        var expectedRoadSegment1 = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegment1Added.RoadSegmentId,
            Geometry = roadSegment1Added.Geometry,
            StartNodeId = roadSegment1Added.StartNodeId,
            EndNodeId = roadSegment1Added.EndNodeId,
            GeometryDrawMethod = roadSegment1Added.GeometryDrawMethod,
            Status = roadSegment1Added.Status,
            AccessRestriction = ForEntireLength(roadSegment1Added.AccessRestriction.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Category = ForEntireLength(roadSegment1Added.Category.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Morphology = ForEntireLength(roadSegment1Added.Morphology.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegment1Added.SurfaceType.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.PedestrianAccess),
            EuropeanRoadNumbers = roadSegment1Added.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegment1Added.NationalRoadNumbers.ToList(),
            Origin = roadSegment1Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment1Added.Provenance.ToEventTimestamp(),
            IsV2 = true
        };
        var expectedRoadSegment2 = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegment2Added.RoadSegmentId,
            Geometry = roadSegment2Added.Geometry,
            StartNodeId = roadSegment2Added.StartNodeId,
            EndNodeId = roadSegment2Added.EndNodeId,
            GeometryDrawMethod = roadSegment2Added.GeometryDrawMethod,
            Status = roadSegment2Added.Status,
            AccessRestriction = ForEntireLength(roadSegment2Added.AccessRestriction.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Category = ForEntireLength(roadSegment2Added.Category.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Morphology = ForEntireLength(roadSegment2Added.Morphology.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegment2Added.SurfaceType.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.PedestrianAccess),
            EuropeanRoadNumbers = roadSegment2Added.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegment2Added.NationalRoadNumbers.ToList(),
            Origin = roadSegment2Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment2Added.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegment1Added, roadSegment2Added)
            .Expect(expectedRoadSegment1, expectedRoadSegment2);
    }

    [Fact]
    public Task WhenRoadSegmentWasMerged_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.CustomizeUniqueInteger();

        var roadSegment1Added = fixture.Create<RoadSegmentWasMerged>();
        var roadSegment2Added = fixture.Create<RoadSegmentWasMerged>();

        var expectedRoadSegment1 = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegment1Added.RoadSegmentId,
            Geometry = roadSegment1Added.Geometry,
            StartNodeId = roadSegment1Added.StartNodeId,
            EndNodeId = roadSegment1Added.EndNodeId,
            GeometryDrawMethod = roadSegment1Added.GeometryDrawMethod,
            Status = roadSegment1Added.Status,
            AccessRestriction = ForEntireLength(roadSegment1Added.AccessRestriction.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Category = ForEntireLength(roadSegment1Added.Category.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Morphology = ForEntireLength(roadSegment1Added.Morphology.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegment1Added.SurfaceType.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment1Added.PedestrianAccess),
            EuropeanRoadNumbers = roadSegment1Added.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegment1Added.NationalRoadNumbers.ToList(),
            Origin = roadSegment1Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment1Added.Provenance.ToEventTimestamp(),
            IsV2 = true
        };
        var expectedRoadSegment2 = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegment2Added.RoadSegmentId,
            Geometry = roadSegment2Added.Geometry,
            StartNodeId = roadSegment2Added.StartNodeId,
            EndNodeId = roadSegment2Added.EndNodeId,
            GeometryDrawMethod = roadSegment2Added.GeometryDrawMethod,
            Status = roadSegment2Added.Status,
            AccessRestriction = ForEntireLength(roadSegment2Added.AccessRestriction.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Category = ForEntireLength(roadSegment2Added.Category.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Morphology = ForEntireLength(roadSegment2Added.Morphology.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegment2Added.SurfaceType.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegment2Added.PedestrianAccess),
            EuropeanRoadNumbers = roadSegment2Added.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegment2Added.NationalRoadNumbers.ToList(),
            Origin = roadSegment2Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment2Added.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegment1Added, roadSegment2Added)
            .Expect(expectedRoadSegment1, expectedRoadSegment2);
    }

    [Fact]
    public Task WhenRoadSegmentWasModified_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = fixture.Create<RoadSegmentWasAdded>();
        var roadSegmentModified = fixture.Create<RoadSegmentWasModified>();

        var expectedRoadSegment = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegmentAdded.RoadSegmentId,
            Geometry = roadSegmentModified.Geometry,
            StartNodeId = roadSegmentModified.StartNodeId!.Value,
            EndNodeId = roadSegmentModified.EndNodeId!.Value,
            GeometryDrawMethod = roadSegmentModified.GeometryDrawMethod,
            Status = roadSegmentModified.Status,
            AccessRestriction = ForEntireLength(roadSegmentModified.AccessRestriction!.Values.Single().Value.ToString(), roadSegmentModified.Geometry!),
            Category = ForEntireLength(roadSegmentModified.Category!.Values.Single().Value.ToString(), roadSegmentModified.Geometry!),
            Morphology = ForEntireLength(roadSegmentModified.Morphology!.Values.Single().Value.ToString(), roadSegmentModified.Geometry!),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentModified.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentModified.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegmentModified.SurfaceType!.Values.Single().Value.ToString(), roadSegmentModified.Geometry!),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentModified.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentModified.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentModified.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentModified.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentModified.PedestrianAccess),
            EuropeanRoadNumbers = roadSegmentAdded.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegmentAdded.NationalRoadNumbers.ToList(),
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public Task WhenRoadSegmentWasMigrated_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = fixture.Create<RoadSegmentWasAdded>();
        var roadSegmentMigrated = fixture.Create<RoadSegmentWasMigrated>();

        var expectedRoadSegment = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegmentAdded.RoadSegmentId,
            Geometry = roadSegmentMigrated.Geometry,
            StartNodeId = roadSegmentMigrated.StartNodeId,
            EndNodeId = roadSegmentMigrated.EndNodeId,
            GeometryDrawMethod = roadSegmentMigrated.GeometryDrawMethod,
            Status = roadSegmentMigrated.Status,
            AccessRestriction = ForEntireLength(roadSegmentMigrated.AccessRestriction.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            Category = ForEntireLength(roadSegmentMigrated.Category.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            Morphology = ForEntireLength(roadSegmentMigrated.Morphology.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentMigrated.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentMigrated.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegmentMigrated.SurfaceType.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentMigrated.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentMigrated.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentMigrated.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentMigrated.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentMigrated.PedestrianAccess),
            EuropeanRoadNumbers = roadSegmentMigrated.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegmentMigrated.NationalRoadNumbers.ToList(),
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentMigrated.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentMigrated)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public async Task WhenRoadSegmentWasRemoved_ThenNone()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>();
        var roadSegment1Removed = fixture.Create<RoadSegmentWasRemoved>();

        await BuildProjection()
            .Scenario()
            .Given(roadSegment1Added, roadSegment1Removed)
            .ExpectNone();
    }

    [Fact]
    public async Task WhenRoadSegmentWasRetiredBecauseOfMerger_ThenNone()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>();
        var roadSegment1Removed = fixture.Create<RoadSegmentWasRetiredBecauseOfMerger>();

        await BuildProjection()
            .Scenario()
            .Given(roadSegment1Added, roadSegment1Removed)
            .ExpectNone();
    }

    [Fact]
    public async Task WhenRoadSegmentWasRetiredBecauseOfMigration_ThenNone()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>();
        var roadSegment1Removed = fixture.Create<RoadSegmentWasRetiredBecauseOfMigration>();

        await BuildProjection()
            .Scenario()
            .Given(roadSegment1Added, roadSegment1Removed)
            .ExpectNone();
    }

    [Fact]
    public Task WhenRoadSegmentWasAddedToEuropeanRoad_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = fixture.Create<RoadSegmentWasAdded>() with
        {
            EuropeanRoadNumbers = []
        };
        var roadSegmentModified = fixture.Create<RoadSegmentWasAddedToEuropeanRoad>();

        var expectedRoadSegment = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegmentAdded.RoadSegmentId,
            Geometry = roadSegmentAdded.Geometry,
            StartNodeId = roadSegmentAdded.StartNodeId,
            EndNodeId = roadSegmentAdded.EndNodeId,
            GeometryDrawMethod = roadSegmentAdded.GeometryDrawMethod,
            Status = roadSegmentAdded.Status,
            AccessRestriction = ForEntireLength(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Category = ForEntireLength(roadSegmentAdded.Category.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Morphology = ForEntireLength(roadSegmentAdded.Morphology.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.PedestrianAccess),
            EuropeanRoadNumbers = [roadSegmentModified.Number],
            NationalRoadNumbers = roadSegmentAdded.NationalRoadNumbers.ToList(),
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public Task WhenRoadSegmentWasAddedToNationalRoad_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = fixture.Create<RoadSegmentWasAdded>() with
        {
            NationalRoadNumbers = []
        };
        var roadSegmentModified = fixture.Create<RoadSegmentWasAddedToNationalRoad>();

        var expectedRoadSegment = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegmentAdded.RoadSegmentId,
            Geometry = roadSegmentAdded.Geometry,
            StartNodeId = roadSegmentAdded.StartNodeId,
            EndNodeId = roadSegmentAdded.EndNodeId,
            GeometryDrawMethod = roadSegmentAdded.GeometryDrawMethod,
            Status = roadSegmentAdded.Status,
            AccessRestriction = ForEntireLength(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Category = ForEntireLength(roadSegmentAdded.Category.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Morphology = ForEntireLength(roadSegmentAdded.Morphology.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.PedestrianAccess),
            EuropeanRoadNumbers = roadSegmentAdded.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = [roadSegmentModified.Number],
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public Task WhenRoadSegmentWasRemovedFromEuropeanRoad_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze<EuropeanRoadNumber>();

        var roadSegmentAdded = fixture.Create<RoadSegmentWasAdded>() with
        {
            EuropeanRoadNumbers = [fixture.Create<EuropeanRoadNumber>()]
        };
        var roadSegmentModified = fixture.Create<RoadSegmentWasRemovedFromEuropeanRoad>();

        var expectedRoadSegment = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegmentAdded.RoadSegmentId,
            Geometry = roadSegmentAdded.Geometry,
            StartNodeId = roadSegmentAdded.StartNodeId,
            EndNodeId = roadSegmentAdded.EndNodeId,
            GeometryDrawMethod = roadSegmentAdded.GeometryDrawMethod,
            Status = roadSegmentAdded.Status,
            AccessRestriction = ForEntireLength(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Category = ForEntireLength(roadSegmentAdded.Category.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Morphology = ForEntireLength(roadSegmentAdded.Morphology.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.PedestrianAccess),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = roadSegmentAdded.NationalRoadNumbers.ToList(),
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public Task WhenRoadSegmentWasRemovedFromNationalRoad_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze<NationalRoadNumber>();

        var roadSegmentAdded = fixture.Create<RoadSegmentWasAdded>() with
        {
            NationalRoadNumbers = [fixture.Create<NationalRoadNumber>()]
        };
        var roadSegmentModified = fixture.Create<RoadSegmentWasRemovedFromNationalRoad>();

        var expectedRoadSegment = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegmentAdded.RoadSegmentId,
            Geometry = roadSegmentAdded.Geometry,
            StartNodeId = roadSegmentAdded.StartNodeId,
            EndNodeId = roadSegmentAdded.EndNodeId,
            GeometryDrawMethod = roadSegmentAdded.GeometryDrawMethod,
            Status = roadSegmentAdded.Status,
            AccessRestriction = ForEntireLength(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Category = ForEntireLength(roadSegmentAdded.Category.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Morphology = ForEntireLength(roadSegmentAdded.Morphology.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.CarAccessForward),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.CarAccessBackward),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.BikeAccessForward),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.BikeAccessBackward),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(roadSegmentAdded.PedestrianAccess),
            EuropeanRoadNumbers = roadSegmentAdded.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = [],
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    private static ExtractRoadSegmentDynamicAttribute<T> ForEntireLength<T>(T value, RoadSegmentGeometry geometry)
    {
        return new ExtractRoadSegmentDynamicAttribute<T>([(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm()), RoadSegmentAttributeSide.Both, value)]);
    }

    private RoadSegmentProjection BuildProjection()
    {
        return new RoadSegmentProjection();
    }

    protected static RoadSegmentGeometry BuildRoadSegmentGeometry(int x1, int y1, int x2, int y2)
    {
        return BuildRoadSegmentGeometry(new Point(x1, y1), new Point(x2, y2));
    }

    protected static RoadSegmentGeometry BuildRoadSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithMeasureOrdinates()
            .ToRoadSegmentGeometry();
    }
}
