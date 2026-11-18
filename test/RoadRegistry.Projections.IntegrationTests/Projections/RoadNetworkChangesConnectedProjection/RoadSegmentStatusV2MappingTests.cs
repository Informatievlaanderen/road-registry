namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Infrastructure;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.StreetName;
using RoadRegistry.ValueObjects;
using RoadSegment.Events.V2;
using Tests.AggregateTests;
using Xunit.Abstractions;
using RoadSegmentAggregate = RoadRegistry.RoadSegment.RoadSegment;

// The status column 20261113120000_add_status_column_to_read_roadsegments generates on the road segment read documents
// repeats RoadSegmentStatusV2Mapping in SQL: the list of road segments filters on the one, the detail shows the other.
// Every V1 status with every V1 geometry draw method, and every V2 status, has to come out the same in both.
[Collection(nameof(DockerFixtureCollection))]
public class RoadSegmentStatusV2MappingTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Fixture _fixture = new RoadNetworkTestDataV2().Fixture;

    public RoadSegmentStatusV2MappingTests(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
    {
        _databaseFixture = databaseFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task TheStatusColumnIsTheStatusTheDetailShows()
    {
        var v1 = RoadSegmentStatus.All
            .SelectMany(status => RoadSegmentGeometryDrawMethod.All.Select(method => (IsV2: false, Status: status.ToString(), Method: method.ToString())));
        var v2 = RoadSegmentStatusV2.All
            .Select(status => (IsV2: true, Status: status.ToString(), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten.ToString()));
        var cases = v1.Concat(v2)
            .Select((x, index) => (RoadSegmentId: index + 1, x.IsV2, x.Status, x.Method))
            .ToList();

        var runner = CreateProjectionTestRunner();
        foreach (var @case in cases)
        {
            var added = _fixture.Create<RoadSegmentWasAdded>() with { RoadSegmentId = new RoadSegmentId(@case.RoadSegmentId) };
            runner = runner.Given<RoadSegmentAggregate, RoadSegmentId>(added.RoadSegmentId, added);
        }

        await runner.Expect(async (_, session) =>
        {
            // The projection writes V2 documents; turn each into the case it stands for, the way the projection writes
            // a V1 road segment: its V1 status and draw method as their bare names, and IsV2 false.
            foreach (var @case in cases)
            {
                var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(@case.RoadSegmentId, CancellationToken.None);
                roadSegment!.IsV2 = @case.IsV2;
                roadSegment.Status = @case.Status;
                roadSegment.GeometryDrawMethod = @case.Method;
                session.Store(roadSegment);
            }

            await session.SaveChangesAsync();

            var columns = await StatusColumns(session);
            var roadSegments = await session.LoadManyAsync<RoadSegmentReadItem>(CancellationToken.None, cases.Select(x => x.RoadSegmentId));

            foreach (var roadSegment in roadSegments)
            {
                var @case = cases.Single(x => x.RoadSegmentId == roadSegment.RoadSegmentId.ToInt32());
                columns[@case.RoadSegmentId].Should().Be(roadSegment.GetStatusV2().ToString(),
                    $"IsV2 {@case.IsV2}, status {@case.Status} and geometry draw method {@case.Method} should map the same in SQL as in C#");
            }

            columns.Should().HaveCount(cases.Count);
        });
    }

    private static async Task<IReadOnlyDictionary<int, string?>> StatusColumns(IDocumentSession session)
    {
        var json = (await session.AdvancedSql.QueryAsync<string>(
            $"select coalesce(json_agg(json_build_object('id', id, 'status', status)), '[]')::text from {WellKnownSchemas.MartenProjections}.mt_doc_read_roadsegments",
            CancellationToken.None)).Single();

        return JsonConvert.DeserializeObject<List<StatusColumn>>(json)!
            .ToDictionary(x => x.Id, x => x.Status);
    }

    private sealed record StatusColumn(int Id, string? Status);

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
                    RoadSegmentReadProjection.Configure(options);
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
