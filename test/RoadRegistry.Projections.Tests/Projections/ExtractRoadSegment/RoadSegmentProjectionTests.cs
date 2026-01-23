namespace RoadRegistry.Projections.Tests.Projections.ExtractRoadSegment;

using AutoFixture;
using Extracts.Projections;
using FluentAssertions;
using GradeSeparatedJunction.Events.V1;
using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using RoadNode.Events.V1;
using RoadNode.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice;
using RoadSegment.Events.V2;
using ScopedRoadNetwork.Events.V1;
using ScopedRoadNetwork.Events.V2;
using VehicleAccess = RoadSegment.ValueObjects.VehicleAccess;

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

    [Fact]
    public Task WhenRoadSegmentWasAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().Fixture;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegment1Added.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegment1Added.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegment1Added.PedestrianAccess),
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegment1Added.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegment1Added.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegment1Added.PedestrianAccess),
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
        var fixture = new RoadNetworkTestData().Fixture;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegment1Added.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegment1Added.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegment1Added.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegment1Added.PedestrianAccess),
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegment2Added.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegment2Added.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegment2Added.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegment2Added.PedestrianAccess),
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
        var fixture = new RoadNetworkTestData().Fixture;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegmentModified.AccessRestriction!.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegmentModified.Category!.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegmentModified.Morphology!.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegmentModified.Status!.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentModified.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentModified.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegmentModified.SurfaceType!.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentModified.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentModified.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentModified.PedestrianAccess),
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
        var fixture = new RoadNetworkTestData().Fixture;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegmentMigrated.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegmentMigrated.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegmentMigrated.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegmentMigrated.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentMigrated.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentMigrated.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegmentMigrated.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentMigrated.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentMigrated.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentMigrated.PedestrianAccess),
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
        var fixture = new RoadNetworkTestData().Fixture;
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
        var fixture = new RoadNetworkTestData().Fixture;
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
        var fixture = new RoadNetworkTestData().Fixture;
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
        var fixture = new RoadNetworkTestData().Fixture;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentAdded.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentAdded.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentAdded.PedestrianAccess),
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
        var fixture = new RoadNetworkTestData().Fixture;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentAdded.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentAdded.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentAdded.PedestrianAccess),
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
        var fixture = new RoadNetworkTestData().Fixture;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentAdded.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentAdded.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentAdded.PedestrianAccess),
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
        var fixture = new RoadNetworkTestData().Fixture;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString()),
            Category = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Category.Values.Single().Value.ToString()),
            Morphology = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Morphology.Values.Single().Value.ToString()),
            Status = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.Status.Values.Single().Value.ToString()),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<string>(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString()),
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentAdded.CarAccess),
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>(roadSegmentAdded.BikeAccess),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentAdded.PedestrianAccess),
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

    private RoadSegmentProjection BuildProjection()
    {
        return new RoadSegmentProjection();
    }
}
