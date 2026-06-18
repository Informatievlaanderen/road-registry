namespace RoadRegistry.Read.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using GradeJunction.Events.V2;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class GradeJunctionReadProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<GradeJunctionReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_gradejunctions")
            .Identity(x => x.Id);
    }

    public GradeJunctionReadProjection()
    {
        // V2
        When<IEvent<GradeJunctionWasAdded>>(async (session, e, ct) =>
        {
            var junction = new GradeJunctionReadItem
            {
                GradeJunctionId = e.Data.GradeJunctionId,
                RoadSegmentId1 = new RoadSegmentId(e.Data.RoadSegmentId1),
                RoadSegmentId2 = new RoadSegmentId(e.Data.RoadSegmentId2),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(junction);

            await UpdateRoadSegmentGradeJunctionIds(session, junction.GradeJunctionId, (null, null), (junction.RoadSegmentId1, junction.RoadSegmentId2), ct);
        });
        When<IEvent<GradeJunctionWasRemoved>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeJunctionReadItem>(e.Data.GradeJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade junction found for Id {e.Data.GradeJunctionId}");
            }

            junction.IsRemoved = true;
            session.Store(junction);

            await UpdateRoadSegmentGradeJunctionIds(session, junction.GradeJunctionId, (junction.RoadSegmentId1, junction.RoadSegmentId2), (null, null), ct);
        });
    }

    private static async Task UpdateRoadSegmentGradeJunctionIds(
        IDocumentOperations session,
        GradeJunctionId gradeJunctionId,
        (RoadSegmentId? First, RoadSegmentId? Second) originalRoadSegmentIds,
        (RoadSegmentId? First, RoadSegmentId? Second) updatedRoadSegmentIds,
        CancellationToken ct)
    {
        var roadSegmentIds = new[]
            {
                originalRoadSegmentIds.First,
                originalRoadSegmentIds.Second,
                updatedRoadSegmentIds.First,
                updatedRoadSegmentIds.Second
            }
            .Where(x => x is not null)
            .Select(x => x!.Value.ToInt32())
            .Distinct()
            .ToArray();

        var roadSegments = await session.LoadManyAsync<RoadSegmentReadItem>(ct, roadSegmentIds);

        if (originalRoadSegmentIds.First is not null)
        {
            var roadSegment = roadSegments.SingleOrDefault(x => x.RoadSegmentId == originalRoadSegmentIds.First.Value)
                              ?? throw new InvalidOperationException($"No road segment found for Id {originalRoadSegmentIds.First.Value}");
            roadSegment.GradeJunctionIds = roadSegment.GradeJunctionIds.Except([gradeJunctionId]).ToArray();
            session.Store(roadSegment);
        }
        if (originalRoadSegmentIds.Second is not null)
        {
            var roadSegment = roadSegments.SingleOrDefault(x => x.RoadSegmentId == originalRoadSegmentIds.Second.Value)
                              ?? throw new InvalidOperationException($"No road segment found for Id {originalRoadSegmentIds.Second.Value}");
            roadSegment.GradeJunctionIds = roadSegment.GradeJunctionIds.Except([gradeJunctionId]).ToArray();
            session.Store(roadSegment);
        }

        if (updatedRoadSegmentIds.First is not null)
        {
            var roadSegment = roadSegments.SingleOrDefault(x => x.RoadSegmentId == updatedRoadSegmentIds.First.Value)
                              ?? throw new InvalidOperationException($"No road segment found for Id {updatedRoadSegmentIds.First.Value}");
            roadSegment.GradeJunctionIds = roadSegment.GradeJunctionIds.Union([gradeJunctionId]).OrderBy(x => x).ToArray();
            session.Store(roadSegment);
        }
        if (updatedRoadSegmentIds.Second is not null)
        {
            var roadSegment = roadSegments.SingleOrDefault(x => x.RoadSegmentId == updatedRoadSegmentIds.Second.Value)
                              ?? throw new InvalidOperationException($"No road segment found for Id {updatedRoadSegmentIds.Second.Value}");
            roadSegment.GradeJunctionIds = roadSegment.GradeJunctionIds.Union([gradeJunctionId]).OrderBy(x => x).ToArray();
            session.Store(roadSegment);
        }
    }
}

public sealed class GradeJunctionReadItem
{
    [JsonIgnore]
    public int Id { get; private set; }

    public required GradeJunctionId GradeJunctionId
    {
        get => new(Id);
        set => Id = value;
    }

    public required RoadSegmentId RoadSegmentId1 { get; set; }
    public required RoadSegmentId RoadSegmentId2 { get; set; }

    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; set; }
    public required bool IsV2 { get; set; }
    public bool IsRemoved { get; set; }
}
