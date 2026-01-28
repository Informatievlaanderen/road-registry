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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.AccessRestriction.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.Category.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.Morphology.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.Status.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.SurfaceType.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegment1Added.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegment1Added.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.AccessRestriction.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.Category.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.Morphology.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.Status.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.SurfaceType.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegment2Added.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegment2Added.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.AccessRestriction.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.Category.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.Morphology.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.Status.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment1Added.SurfaceType.Values.Single().Value.ToString(), roadSegment1Added.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegment1Added.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegment1Added.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.AccessRestriction.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.Category.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.Morphology.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.Status.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegment2Added.SurfaceType.Values.Single().Value.ToString(), roadSegment2Added.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegment2Added.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegment2Added.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentModified.AccessRestriction!.Values.Single().Value.ToString(), roadSegmentModified.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentModified.Category!.Values.Single().Value.ToString(), roadSegmentModified.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentModified.Morphology!.Values.Single().Value.ToString(), roadSegmentModified.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentModified.Status!.Values.Single().Value.ToString(), roadSegmentModified.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentModified.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentModified.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentModified.SurfaceType!.Values.Single().Value.ToString(), roadSegmentModified.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentModified.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentModified.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentMigrated.AccessRestriction.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentMigrated.Category.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentMigrated.Morphology.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentMigrated.Status.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentMigrated.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentMigrated.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentMigrated.SurfaceType.Values.Single().Value.ToString(), roadSegmentMigrated.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentMigrated.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentMigrated.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Category.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Morphology.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Status.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentAdded.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentAdded.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Category.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Morphology.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Status.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentAdded.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentAdded.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Category.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Morphology.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Status.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentAdded.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentAdded.BikeAccess),
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
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.AccessRestriction.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Category.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Morphology.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            Status = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.Status.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(roadSegmentAdded.SurfaceType.Values.Single().Value.ToString(), roadSegmentAdded.Geometry),
            CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentAdded.CarAccess),
            BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(roadSegmentAdded.BikeAccess),
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

    private RoadSegmentProjection BuildProjection()
    {
        return new RoadSegmentProjection();
    }
}
