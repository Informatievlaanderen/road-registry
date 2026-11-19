namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using GradeJunction.Events.V2;
using GradeSeparatedJunction.Events.V2;
using Infrastructure;
using RoadNode.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.StreetName;
using RoadSegment.Events.V2;
using Tests.AggregateTests;
using Xunit.Abstractions;
using GradeJunctionAggregate = RoadRegistry.GradeJunction.GradeJunction;
using GradeSeparatedJunctionAggregate = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNodeAggregate = RoadRegistry.RoadNode.RoadNode;
using RoadSegmentAggregate = RoadRegistry.RoadSegment.RoadSegment;

// The read documents carry only their own foreign keys, duplicated into columns of their own table, and everything
// that needs the other direction queries those columns through ReadModelQueries. Those queries are the one part of
// this that a unit test cannot cover: in memory they are evaluated by Linq to Objects, and whether Marten translates
// them onto the duplicated columns - a containment match on an integer[] / varchar[] column among them - only shows
// against Postgres.
[Collection(nameof(DockerFixtureCollection))]
public class ReadModelQueriesTests : IClassFixture<DatabaseFixture>
{
    private const int StreetNameId = 100;
    private const string OrganizationCode = "ORG-A";

    private readonly DatabaseFixture _databaseFixture;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Fixture _fixture = new RoadNetworkTestDataV2().Fixture;

    public ReadModelQueriesTests(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
    {
        _databaseFixture = databaseFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task FindsWhatReferencesADocumentAndLeavesOutWhatDoesNot()
    {
        // Node 1 -- segment 1 -- node 2 -- segment 2 -- node 3, with segment 3 also on node 1 but removed.
        var node1 = RoadNodeAdded(1);
        var node2 = RoadNodeAdded(2);
        var node3 = RoadNodeAdded(3);

        var segment1 = RoadSegmentAdded(1, startNodeId: 1, endNodeId: 2, streetNameId: StreetNameId, organizationCode: OrganizationCode);
        var segment2 = RoadSegmentAdded(2, startNodeId: 2, endNodeId: 3, streetNameId: StreetNameId, organizationCode: "ORG-B");
        var segment3 = RoadSegmentAdded(3, startNodeId: 1, endNodeId: 3, streetNameId: StreetNameId, organizationCode: OrganizationCode);
        var segment3Removed = new RoadSegmentWasRemoved { RoadSegmentId = new RoadSegmentId(3), Provenance = segment3.Provenance };

        // Crossings on segment 1, plus one that is nowhere near it and one that was removed.
        var gradeJunction11 = GradeJunctionAdded(11, roadSegmentId1: 1, roadSegmentId2: 2);
        var gradeJunction12 = GradeJunctionAdded(12, roadSegmentId1: 2, roadSegmentId2: 3);
        var gradeJunction13 = GradeJunctionAdded(13, roadSegmentId1: 1, roadSegmentId2: 3);
        var gradeJunction13Removed = new GradeJunctionWasRemoved { GradeJunctionId = new GradeJunctionId(13), Provenance = gradeJunction13.Provenance };

        var gradeSeparatedJunction21 = GradeSeparatedJunctionAdded(21, lowerRoadSegmentId: 2, upperRoadSegmentId: 1);
        var gradeSeparatedJunction22 = GradeSeparatedJunctionAdded(22, lowerRoadSegmentId: 2, upperRoadSegmentId: 3);

        await CreateProjectionTestRunner()
            .Given<RoadNodeAggregate, RoadNodeId>(node1.RoadNodeId, node1)
            .Given<RoadNodeAggregate, RoadNodeId>(node2.RoadNodeId, node2)
            .Given<RoadNodeAggregate, RoadNodeId>(node3.RoadNodeId, node3)
            .Given<RoadSegmentAggregate, RoadSegmentId>(segment1.RoadSegmentId, segment1)
            .Given<RoadSegmentAggregate, RoadSegmentId>(segment2.RoadSegmentId, segment2)
            .Given<RoadSegmentAggregate, RoadSegmentId>(segment3.RoadSegmentId, segment3, segment3Removed)
            .Given<GradeJunctionAggregate, GradeJunctionId>(gradeJunction11.GradeJunctionId, gradeJunction11)
            .Given<GradeJunctionAggregate, GradeJunctionId>(gradeJunction12.GradeJunctionId, gradeJunction12)
            .Given<GradeJunctionAggregate, GradeJunctionId>(gradeJunction13.GradeJunctionId, gradeJunction13, gradeJunction13Removed)
            .Given<GradeSeparatedJunctionAggregate, GradeSeparatedJunctionId>(gradeSeparatedJunction21.GradeSeparatedJunctionId, gradeSeparatedJunction21)
            .Given<GradeSeparatedJunctionAggregate, GradeSeparatedJunctionId>(gradeSeparatedJunction22.GradeSeparatedJunctionId, gradeSeparatedJunction22)
            .Expect(async (_, session) =>
            {
                // Start node and end node both count, a removed segment does not.
                var node1Segments = await session.FindRoadSegmentsForRoadNode(new RoadNodeId(1), CancellationToken.None);
                node1Segments.Select(x => x.RoadSegmentId).Should().Equal(new RoadSegmentId(1));

                var node2Segments = await session.FindRoadSegmentsForRoadNode(new RoadNodeId(2), CancellationToken.None);
                node2Segments.Select(x => x.RoadSegmentId).Should().Equal(new RoadSegmentId(1), new RoadSegmentId(2));

                // Either side of the crossing counts, a removed crossing does not.
                var segment1GradeJunctions = await session.FindGradeJunctionsForRoadSegment(new RoadSegmentId(1), CancellationToken.None);
                segment1GradeJunctions.Select(x => x.GradeJunctionId).Should().Equal(new GradeJunctionId(11));

                var segment2GradeJunctions = await session.FindGradeJunctionsForRoadSegment(new RoadSegmentId(2), CancellationToken.None);
                segment2GradeJunctions.Select(x => x.GradeJunctionId).Should().Equal(new GradeJunctionId(11), new GradeJunctionId(12));

                var segment1GradeSeparatedJunctions = await session.FindGradeSeparatedJunctionsForRoadSegment(new RoadSegmentId(1), CancellationToken.None);
                segment1GradeSeparatedJunctions.Select(x => x.GradeSeparatedJunctionId).Should().Equal(new GradeSeparatedJunctionId(21));

                // The containment match on the duplicated array column, which is the one query Marten does not
                // translate for us.
                var streetNameSegments = await session.FindRoadSegmentsForStreetName(new StreetNameLocalId(StreetNameId), CancellationToken.None);
                streetNameSegments.Select(x => x.RoadSegmentId).Should().Equal(new RoadSegmentId(1), new RoadSegmentId(2));

                // Nothing references this one.
                (await session.FindRoadSegmentsForStreetName(new StreetNameLocalId(999), CancellationToken.None)).Should().BeEmpty();
            });
    }

    private RoadNodeWasAdded RoadNodeAdded(int roadNodeId)
    {
        return _fixture.Create<RoadNodeWasAdded>() with { RoadNodeId = new RoadNodeId(roadNodeId) };
    }

    private RoadSegmentWasAdded RoadSegmentAdded(int roadSegmentId, int startNodeId, int endNodeId, int streetNameId, string organizationCode)
    {
        var added = _fixture.Create<RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(roadSegmentId),
            StartNodeId = new RoadNodeId(startNodeId),
            EndNodeId = new RoadNodeId(endNodeId)
        };

        return added with
        {
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(new StreetNameLocalId(streetNameId), added.Geometry),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(organizationCode), added.Geometry)
        };
    }

    private GradeJunctionWasAdded GradeJunctionAdded(int gradeJunctionId, int roadSegmentId1, int roadSegmentId2)
    {
        return _fixture.Create<GradeJunctionWasAdded>() with
        {
            GradeJunctionId = new GradeJunctionId(gradeJunctionId),
            RoadSegmentId1 = new RoadSegmentId(roadSegmentId1),
            RoadSegmentId2 = new RoadSegmentId(roadSegmentId2)
        };
    }

    private GradeSeparatedJunctionWasAdded GradeSeparatedJunctionAdded(int gradeSeparatedJunctionId, int lowerRoadSegmentId, int upperRoadSegmentId)
    {
        return _fixture.Create<GradeSeparatedJunctionWasAdded>() with
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(gradeSeparatedJunctionId),
            LowerRoadSegmentId = new RoadSegmentId(lowerRoadSegmentId),
            UpperRoadSegmentId = new RoadSegmentId(upperRoadSegmentId)
        };
    }

    private MartenProjectionIntegrationTestRunner CreateProjectionTestRunner()
    {
        var logger = _testOutputHelper.ToLogger<RoadSegmentReadProjection>();

        return new MartenProjectionIntegrationTestRunner(_databaseFixture, logger)
            .ConfigureRoadNetworkChangesProjection(
                [
                    new RoadNodeReadProjection(),
                    new RoadSegmentReadProjection(new NullStreetNameClient(), logger),
                    new GradeJunctionReadProjection(),
                    new GradeSeparatedJunctionReadProjection()
                ],
                options =>
                {
                    RoadNodeReadProjection.Configure(options);
                    RoadSegmentReadProjection.Configure(options);
                    GradeJunctionReadProjection.Configure(options);
                    GradeSeparatedJunctionReadProjection.Configure(options);
                    OrganizationReadProjection.Configure(options);
                    StreetNameReadProjection.Configure(options);
                },
                logger);
    }

    private sealed class NullStreetNameClient : IStreetNameClient
    {
        public Task<StreetNameItem?> GetAsync(int id, CancellationToken cancellationToken)
            => Task.FromResult<StreetNameItem?>(null);
    }
}
