namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenRemovingRoadNode;

using Actions.RemoveRoadNode;
using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadSegment;
using Sqs.RoadNodes.V2;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

// VAL-8: removing a road node joins the two roads that meet at it, and the result may not run across a third road more
// than once. The domain has always said so, and a unit test has always covered it - on a road network put together by
// hand with that third road in it.
//
// Production never handed it one. The action loaded the road network seeded from the road node alone, which brings in
// the roads hanging off that node and nothing else, and the check skips over exactly those. So it had nothing to
// compare against and the merge went through unreported (GAWR-7824). This goes through the real thing: events into
// the store, the topology projection, the action's own scope.
//
//          road 3 (node 4 -- node 5)
//   -------------------------------------  y = 0
//        /                      \
//   road 1                      road 2     meeting at node 2, above the line
//
// Each road crosses road 3 once, which is allowed. Joined into one they cross it twice, which is not.
[Collection(nameof(DockerFixtureCollection))]
public class GivenTheMergedRoadWouldCrossAnotherRoadTwice : RoadNetworkIntegrationTest
{
    private static readonly RoadNodeId MergeNodeId = new(2);

    public GivenTheMergedRoadWouldCrossAnotherRoadTwice(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task ThenItIsReported()
    {
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await GivenTheRoadNetwork(store);

        var ticketId = await WhenRemovingTheRoadNode(sp);

        PrintTicketErrorsIfAvailable(ticketId);

        var ticketError = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Error) && x.Arguments[0].Equals(ticketId))
            .Select(x => (TicketError)x.Arguments[1])
            .SingleOrDefault();

        ticketError.Should().NotBeNull();
        ticketError!.Errors.Should().Contain(x => x.ErrorCode == "RoadSegmentsMergeMultipleIntersections");
    }

    private async Task GivenTheRoadNetwork(IDocumentStore store)
    {
        // The two roads that are to become one, meeting above the line at node 2.
        await AppendRoadNode(store, 1, 0, -5, RoadNodeTypeV2.Eindknoop);
        await AppendRoadNode(store, 2, 10, 5, RoadNodeTypeV2.Validatieknoop);
        await AppendRoadNode(store, 3, 20, -5, RoadNodeTypeV2.Eindknoop);
        await AppendRoadSegment(store, 1, 0, -5, 10, 5, new RoadNodeId(1), MergeNodeId);
        await AppendRoadSegment(store, 2, 10, 5, 20, -5, MergeNodeId, new RoadNodeId(3));

        // The road they would cross twice. It hangs off neither node of the two, so the road network only has it when
        // the action looks beyond the road node it was handed.
        await AppendRoadNode(store, 4, -10, 0, RoadNodeTypeV2.Eindknoop);
        await AppendRoadNode(store, 5, 30, 0, RoadNodeTypeV2.Eindknoop);
        await AppendRoadSegment(store, 3, -10, 0, 30, 0, new RoadNodeId(4), new RoadNodeId(5));
    }

    private async Task<TicketId> WhenRemovingTheRoadNode(IServiceProvider sp)
    {
        var ticketId = TestData.Fixture.Create<TicketId>();

        var request = new RemoveRoadNodeV2SqsRequest
        {
            RoadNodeId = MergeNodeId,
            TicketId = ticketId,
            ProvenanceData = new RoadRegistryProvenanceData()
        };

        using var scope = sp.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<RemoveRoadNodeV2SqsLambdaRequestHandler>();
        await handler.Handle(new RemoveRoadNodeV2SqsLambdaRequest(string.Empty, request), CancellationToken.None);

        return ticketId;
    }

    private Task AppendRoadNode(IDocumentStore store, int roadNodeId, double x, double y, RoadNodeTypeV2 type)
    {
        return Append(store, StreamKeyFactory.Create(typeof(RoadNode), new RoadNodeId(roadNodeId)),
            TestData.Fixture.Create<RoadNodeWasAdded>() with
            {
                RoadNodeId = new RoadNodeId(roadNodeId),
                Geometry = new Point(x, y) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
                Grensknoop = false,
                Type = type
            });
    }

    private Task AppendRoadSegment(IDocumentStore store, int roadSegmentId, int x1, int y1, int x2, int y2, RoadNodeId startNodeId, RoadNodeId endNodeId)
    {
        var geometry = BuildRoadSegmentGeometry(x1, y1, x2, y2);

        return Append(store, StreamKeyFactory.Create(typeof(RoadSegment), new RoadSegmentId(roadSegmentId)),
            TestData.Fixture.Create<RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(roadSegmentId),
                Geometry = geometry,
                Status = RoadSegmentStatusV2.Gerealiseerd,
                GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
                StartNodeId = startNodeId,
                EndNodeId = endNodeId
            });
    }

    // One session per event: the topology functions compare event timestamps, and events saved together share one.
    private static async Task Append(IDocumentStore store, string streamKey, object @event)
    {
        await using var session = store.LightweightSession();
        session.Events.AppendOrStartStream(streamKey, @event);
        await session.SaveChangesAsync();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<RemoveRoadNodeV2SqsLambdaRequestHandler>();
    }
}
