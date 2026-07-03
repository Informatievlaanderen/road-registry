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
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.RoadSegment;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadSegment.Events.V1;
using RoadSegment.Events.V1.ValueObjects;
using RoadSegment.Events.V2;
using RoadSegment.ValueObjects;
using ScopedRoadNetwork.Events.V1;
using ScopedRoadNetwork.Events.V2;

public class RoadSegmentExtractProjectionTests
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
            typeof(RoadNodeWasRemovedBecauseOfMigration),
            typeof(GradeSeparatedJunctionWasAdded),
            typeof(GradeSeparatedJunctionWasModified),
            typeof(GradeSeparatedJunctionWasRemoved),
            typeof(GradeSeparatedJunctionWasRemovedBecauseOfMigration),
            typeof(GradeJunctionWasAdded),
            typeof(GradeJunctionWasRemoved),

            typeof(StreetNameWasCreated),
            typeof(StreetNameWasModified),
            typeof(StreetNameWasRemoved),
            typeof(StreetNameWasRenamed),
            typeof(OrganizationWasImported),
            typeof(OrganizationWasCreated),
            typeof(OrganizationWasModified),
            typeof(OrganizationWasRemoved),
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
    [InlineData(100.03, false)]
    [InlineData(100.02, true)]
    [InlineData(100.01, true)]
    [InlineData(99.99, true)]
    [InlineData(99.98, true)]
    [InlineData(99.97, false)]
    [InlineData(120.00, true)]
    public Task V1_WhenLastSurfaceTypeToPosition_ThenGeometryLengthIsUsed(double surfaceTypeToPosition, bool expectGeometryLength)
    {
        var v1Fixture = new RoadNetworkTestData().ObjectProvider;
        v1Fixture.CustomizeUniqueInteger();

        var roadSegment1Added = v1Fixture.Create<RoadSegmentAdded>();
        roadSegment1Added.LeftSide = new RoadSegmentSideAttributes { StreetNameId = StreetNameLocalId.NotApplicable };
        roadSegment1Added.RightSide = new RoadSegmentSideAttributes { StreetNameId = StreetNameLocalId.NotApplicable };
        roadSegment1Added.MaintenanceAuthority.Code = OrganizationId.Unknown.ToString();

        roadSegment1Added.Geometry = BuildRoadSegmentGeometryLambert72(0, 0, 100, 0);
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
            Geometry = roadSegment1Added.Geometry.EnsureLambert08().RoundToCm(),
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
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(),
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
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegment1Added.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegment1Added.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegment1Added.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegment1Added.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegment1Added.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegment1Added.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegment1Added.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegment1Added.PedestrianTrafficDirection),
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
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegment2Added.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegment2Added.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegment2Added.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegment2Added.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegment2Added.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegment2Added.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegment2Added.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegment2Added.PedestrianTrafficDirection),
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
    public Task WhenOutlinedRoadSegmentWasAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.CustomizeUniqueInteger();

        var roadSegment1Added = fixture.Create<OutlinedRoadSegmentWasAdded>();
        var roadSegment2Added = fixture.Create<OutlinedRoadSegmentWasAdded>();

        var expectedRoadSegment1 = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegment1Added.RoadSegmentId,
            Geometry = roadSegment1Added.Geometry,
            StartNodeId = null,
            EndNodeId = null,
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Status = roadSegment1Added.Status,
            AccessRestriction = ForEntireLength(roadSegment1Added.AccessRestriction.Values.Single().Value!.ToString(), roadSegment1Added.Geometry),
            Category = ForEntireLength(roadSegment1Added.Category.Values.Single().Value!.ToString(), roadSegment1Added.Geometry),
            Morphology = ForEntireLength(roadSegment1Added.Morphology.Values.Single().Value!.ToString(), roadSegment1Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment1Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment1Added.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegment1Added.SurfaceType.Values.Single().Value!.ToString(), roadSegment1Added.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegment1Added.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegment1Added.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegment1Added.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegment1Added.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegment1Added.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegment1Added.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegment1Added.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegment1Added.PedestrianTrafficDirection),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = [],
            Origin = roadSegment1Added.Provenance.ToEventTimestamp(),
            LastModified = roadSegment1Added.Provenance.ToEventTimestamp(),
            IsV2 = true
        };
        var expectedRoadSegment2 = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegment2Added.RoadSegmentId,
            Geometry = roadSegment2Added.Geometry,
            StartNodeId = null,
            EndNodeId = null,
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Status = roadSegment2Added.Status,
            AccessRestriction = ForEntireLength(roadSegment2Added.AccessRestriction.Values.Single().Value!.ToString(), roadSegment2Added.Geometry),
            Category = ForEntireLength(roadSegment2Added.Category.Values.Single().Value!.ToString(), roadSegment2Added.Geometry),
            Morphology = ForEntireLength(roadSegment2Added.Morphology.Values.Single().Value!.ToString(), roadSegment2Added.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegment2Added.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegment2Added.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegment2Added.SurfaceType.Values.Single().Value!.ToString(), roadSegment2Added.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegment2Added.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegment2Added.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegment2Added.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegment2Added.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegment2Added.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegment2Added.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegment2Added.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegment2Added.PedestrianTrafficDirection),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = [],
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

        var roadSegmentAdded = fixture.Create<RoadSegmentWasAdded>();
        var roadSegmentMerged = fixture.Create<RoadSegmentWasMerged>() with
        {
            RoadSegmentId = roadSegmentAdded.RoadSegmentId
        };

        var expectedRoadSegment = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegmentMerged.RoadSegmentId,
            Geometry = roadSegmentMerged.Geometry,
            StartNodeId = roadSegmentMerged.StartNodeId,
            EndNodeId = roadSegmentMerged.EndNodeId,
            GeometryDrawMethod = roadSegmentMerged.GeometryDrawMethod,
            Status = roadSegmentMerged.Status,
            AccessRestriction = ForEntireLength(roadSegmentMerged.AccessRestriction.Values.Single().Value!.ToString(), roadSegmentMerged.Geometry),
            Category = ForEntireLength(roadSegmentMerged.Category.Values.Single().Value!.ToString(), roadSegmentMerged.Geometry),
            Morphology = ForEntireLength(roadSegmentMerged.Morphology.Values.Single().Value!.ToString(), roadSegmentMerged.Geometry),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentMerged.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentMerged.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegmentMerged.SurfaceType.Values.Single().Value!.ToString(), roadSegmentMerged.Geometry),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentMerged.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentMerged.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentMerged.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentMerged.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegmentMerged.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentMerged.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentMerged.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegmentMerged.PedestrianTrafficDirection),
            EuropeanRoadNumbers = roadSegmentMerged.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = roadSegmentMerged.NationalRoadNumbers.ToList(),
            Origin = roadSegmentAdded.Provenance.ToEventTimestamp(),
            LastModified = roadSegmentMerged.Provenance.ToEventTimestamp(),
            IsV2 = true
        };

        return BuildProjection()
            .Scenario()
            .Given(roadSegmentAdded, roadSegmentMerged)
            .Expect(expectedRoadSegment);
    }

    [Fact]
    public Task WhenRoadSegmentGeometryWasModified_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = fixture.Create<RoadSegmentWasAdded>();
        var roadSegmentModified = fixture.Create<RoadSegmentGeometryWasModified>();

        var expectedRoadSegment = new RoadSegmentExtractItem
        {
            RoadSegmentId = roadSegmentAdded.RoadSegmentId,
            Geometry = roadSegmentModified.Geometry,
            StartNodeId = roadSegmentModified.StartNodeId,
            EndNodeId = roadSegmentModified.EndNodeId,
            GeometryDrawMethod = roadSegmentAdded.GeometryDrawMethod,
            Status = roadSegmentAdded.Status,
            AccessRestriction = ForEntireLength(roadSegmentAdded.AccessRestriction!.Values.Single().Value.ToString(), roadSegmentModified.Geometry!),
            Category = ForEntireLength(roadSegmentAdded.Category!.Values.Single().Value.ToString(), roadSegmentModified.Geometry!),
            Morphology = ForEntireLength(roadSegmentAdded.Morphology!.Values.Single().Value.ToString(), roadSegmentModified.Geometry!),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(roadSegmentAdded.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(roadSegmentAdded.MaintenanceAuthorityId),
            SurfaceType = ForEntireLength(roadSegmentAdded.SurfaceType!.Values.Single().Value.ToString(), roadSegmentAdded.Geometry!),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegmentAdded.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegmentAdded.PedestrianTrafficDirection),
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
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentModified.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentModified.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentModified.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentModified.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegmentModified.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentModified.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentModified.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegmentModified.PedestrianTrafficDirection),
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
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentMigrated.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentMigrated.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentMigrated.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentMigrated.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegmentMigrated.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentMigrated.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentMigrated.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegmentMigrated.PedestrianTrafficDirection),
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
    public async Task WhenRoadSegmentWasRemovedBecauseOfMigration_ThenNone()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>();
        var roadSegment1Removed = fixture.Create<RoadSegmentWasRemovedBecauseOfMigration>();

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
        var roadSegment1Removed = fixture.Create<RoadSegmentWasRetired>();

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
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegmentAdded.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegmentAdded.PedestrianTrafficDirection),
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
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegmentAdded.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegmentAdded.PedestrianTrafficDirection),
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
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegmentAdded.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegmentAdded.PedestrianTrafficDirection),
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
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(roadSegmentAdded.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(roadSegmentAdded.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(roadSegmentAdded.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(roadSegmentAdded.PedestrianTrafficDirection),
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

    private RoadSegmentExtractProjection BuildProjection()
    {
        return new RoadSegmentExtractProjection();
    }

    protected static RoadSegmentGeometry BuildRoadSegmentGeometryLambert72(int x1, int y1, int x2, int y2)
    {
        return BuildRoadSegmentGeometryLambert72(new Point(x1, y1), new Point(x2, y2));
    }

    protected static RoadSegmentGeometry BuildRoadSegmentGeometryLambert72(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithSrid(WellknownSrids.Lambert72)
            .ToRoadSegmentGeometry();
    }
}
