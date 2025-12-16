namespace RoadRegistry.Projections.Tests.Projections.ExtractRoadSegment;

using AutoFixture;
using FluentAssertions;
using JasperFx.Events;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice;
using RoadSegment.Events.V2;

public class RoadSegmentProjectionTests
{
    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        var excludeEventTypes = new[]
        {
            typeof(RoadRegistry.RoadNetwork.Events.V1.RoadNetworkChangesAccepted),
            typeof(RoadRegistry.RoadNode.Events.V1.ImportedRoadNode),
            typeof(RoadRegistry.RoadNode.Events.V1.RoadNodeAdded),
            typeof(RoadRegistry.RoadNode.Events.V1.RoadNodeModified),
            typeof(RoadRegistry.RoadNode.Events.V1.RoadNodeRemoved),
            typeof(GradeSeparatedJunction.Events.V1.ImportedGradeSeparatedJunction),
            typeof(GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionRemoved),

            typeof(RoadRegistry.RoadNetwork.Events.V2.RoadNetworkChanged),
            typeof(RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded),
            typeof(RoadRegistry.RoadNode.Events.V2.RoadNodeWasModified),
            typeof(RoadRegistry.RoadNode.Events.V2.RoadNodeWasRemoved),
            typeof(GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded),
            typeof(GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasModified),
            typeof(GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasRemoved)
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
    public Task WhenRoadSegmentAdded_ThenSucceeded()
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegment1Added.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegment1Added.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegment1Added.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegment1Added.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegment1Added.SurfaceType),
            EuropeanRoadNumbers = roadSegment1Added.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegment1Added.NationalRoadNumbers.ToList(),
            Origin = roadSegment1Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment1Added.Provenance.ToEventTimestamp()
        };
        var expectedRoadSegment2 = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegment2Added.RoadSegmentId,
            Geometry = roadSegment2Added.Geometry,
            StartNodeId = roadSegment2Added.StartNodeId,
            EndNodeId = roadSegment2Added.EndNodeId,
            GeometryDrawMethod = roadSegment2Added.GeometryDrawMethod,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegment2Added.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegment2Added.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegment2Added.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegment2Added.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegment2Added.SurfaceType),
            EuropeanRoadNumbers = roadSegment2Added.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegment2Added.NationalRoadNumbers.ToList(),
            Origin = roadSegment2Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment2Added.Provenance.ToEventTimestamp()
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegment1Added.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegment1Added.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegment1Added.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegment1Added.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegment1Added.SurfaceType),
            EuropeanRoadNumbers = roadSegment1Added.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegment1Added.NationalRoadNumbers.ToList(),
            Origin = roadSegment1Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment1Added.Provenance.ToEventTimestamp()
        };
        var expectedRoadSegment2 = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegment2Added.RoadSegmentId,
            Geometry = roadSegment2Added.Geometry,
            StartNodeId = roadSegment2Added.StartNodeId,
            EndNodeId = roadSegment2Added.EndNodeId,
            GeometryDrawMethod = roadSegment2Added.GeometryDrawMethod,
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegment2Added.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegment2Added.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegment2Added.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegment2Added.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegment2Added.SurfaceType),
            EuropeanRoadNumbers = roadSegment2Added.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegment2Added.NationalRoadNumbers.ToList(),
            Origin = roadSegment2Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment2Added.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegment1Added, roadSegment2Added)
            .Expect(expectedRoadSegment1, expectedRoadSegment2);
    }

    [Fact]
    public Task WhenRoadSegmentModified_ThenSucceeded()
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegmentModified.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegmentModified.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegmentModified.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegmentModified.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentModified.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentModified.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegmentModified.SurfaceType),
            EuropeanRoadNumbers = roadSegmentAdded.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegmentAdded.NationalRoadNumbers.ToList(),
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public async Task WhenRoadSegmentRemoved_ThenNone()
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
    public Task WhenRoadSegmentAddedToEuropeanRoad_ThenSucceeded()
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegmentAdded.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegmentAdded.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegmentAdded.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegmentAdded.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegmentAdded.SurfaceType),
            EuropeanRoadNumbers = [roadSegmentModified.Number],
            NationalRoadNumbers = roadSegmentAdded.NationalRoadNumbers.ToList(),
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public Task WhenRoadSegmentAddedToNationalRoad_ThenSucceeded()
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegmentAdded.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegmentAdded.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegmentAdded.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegmentAdded.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegmentAdded.SurfaceType),
            EuropeanRoadNumbers = roadSegmentAdded.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = [roadSegmentModified.Number],
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public Task WhenRoadSegmentRemovedFromEuropeanRoad_ThenSucceeded()
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegmentAdded.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegmentAdded.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegmentAdded.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegmentAdded.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegmentAdded.SurfaceType),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = roadSegmentAdded.NationalRoadNumbers.ToList(),
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentModified)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public Task WhenRoadSegmentRemovedFromNationalRoad_ThenSucceeded()
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(roadSegmentAdded.AccessRestriction),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(roadSegmentAdded.Category),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(roadSegmentAdded.Morphology),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(roadSegmentAdded.Status),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(roadSegmentAdded.SurfaceType),
            EuropeanRoadNumbers = roadSegmentAdded.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = [],
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentModified.Provenance.ToEventTimestamp()
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
