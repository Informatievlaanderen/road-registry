namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Infrastructure;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.StreetName;
using RoadSegment.Events.V2;
using Tests.AggregateTests;
using Xunit.Abstractions;
using RoadSegmentAggregate = RoadRegistry.RoadSegment.RoadSegment;

// The list filters with SQL on the GIN-indexed street name ids and on the generated status column, and pages through
// the result by id - none of which the in-memory unit tests run.
//
// A class of its own, so a database of its own: what the list returns without a filter, and how it pages, depends on
// every road segment in the table, so it cannot share one with the tests of the other queries.
[Collection(nameof(DockerFixtureCollection))]
public class ListRoadSegmentsQueryTests : IClassFixture<DatabaseFixture>
{
    private const int StreetNameId = 100;
    private const string OrganizationCode = "ORG-A";

    private readonly DatabaseFixture _databaseFixture;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Fixture _fixture = new RoadNetworkTestDataV2().Fixture;

    public ListRoadSegmentsQueryTests(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
    {
        _databaseFixture = databaseFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task ListsAPageOfTheRoadSegmentsMatchingTheFilters()
    {
        var segment1 = RoadSegmentAdded(1, streetNameId: StreetNameId) with { Status = RoadSegmentStatusV2.Gerealiseerd };
        var segment2 = RoadSegmentAdded(2, streetNameId: StreetNameId) with { Status = RoadSegmentStatusV2.Gepland };
        var segment3 = RoadSegmentAdded(3, streetNameId: 200) with { Status = RoadSegmentStatusV2.Gerealiseerd };
        var segment4 = RoadSegmentAdded(4, streetNameId: StreetNameId) with { Status = RoadSegmentStatusV2.Gerealiseerd };
        var segment4Removed = new RoadSegmentWasRemoved { RoadSegmentId = new RoadSegmentId(4), Provenance = segment4.Provenance };
        var segment5 = RoadSegmentAdded(5, streetNameId: StreetNameId) with { Status = RoadSegmentStatusV2.Gerealiseerd };

        await CreateProjectionTestRunner()
            .Given<RoadSegmentAggregate, RoadSegmentId>(segment1.RoadSegmentId, segment1)
            .Given<RoadSegmentAggregate, RoadSegmentId>(segment2.RoadSegmentId, segment2)
            .Given<RoadSegmentAggregate, RoadSegmentId>(segment3.RoadSegmentId, segment3)
            .Given<RoadSegmentAggregate, RoadSegmentId>(segment4.RoadSegmentId, segment4, segment4Removed)
            .Given<RoadSegmentAggregate, RoadSegmentId>(segment5.RoadSegmentId, segment5)
            .Expect(async (_, session) =>
            {
                async Task<int[]> List(int? streetNameId, RoadSegmentStatusV2? status, int offset = 0, int limit = 10)
                {
                    var roadSegments = await session.ListRoadSegments(
                        streetNameId is null ? null : new StreetNameLocalId(streetNameId.Value), status, offset, limit, CancellationToken.None);
                    return roadSegments.Select(x => x.RoadSegmentId.ToInt32()).ToArray();
                }

                (await List(null, null)).Should().Equal(1, 2, 3, 5);
                (await List(StreetNameId, null)).Should().Equal(1, 2, 5);
                (await List(null, RoadSegmentStatusV2.Gerealiseerd)).Should().Equal(1, 3, 5);
                (await List(StreetNameId, RoadSegmentStatusV2.Gerealiseerd)).Should().Equal(1, 5);
                (await List(999, null)).Should().BeEmpty();
                (await List(null, RoadSegmentStatusV2.BuitenGebruik)).Should().BeEmpty();

                (await List(null, null, offset: 1, limit: 2)).Should().Equal(2, 3);
                (await List(StreetNameId, RoadSegmentStatusV2.Gerealiseerd, offset: 1, limit: 1)).Should().Equal(5);
            });
    }

    private RoadSegmentWasAdded RoadSegmentAdded(int roadSegmentId, int streetNameId)
    {
        var added = _fixture.Create<RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(roadSegmentId),
            StartNodeId = new RoadNodeId(1),
            EndNodeId = new RoadNodeId(2)
        };

        return added with
        {
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(new StreetNameLocalId(streetNameId), added.Geometry),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(OrganizationCode), added.Geometry)
        };
    }

    private MartenProjectionIntegrationTestRunner CreateProjectionTestRunner()
    {
        var logger = _testOutputHelper.ToLogger<RoadSegmentReadProjection>();

        return new MartenProjectionIntegrationTestRunner(_databaseFixture, logger)
            .ConfigureRoadNetworkChangesProjection(
                [
                    new RoadSegmentReadProjection(new NullStreetNameClient(), logger)
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
