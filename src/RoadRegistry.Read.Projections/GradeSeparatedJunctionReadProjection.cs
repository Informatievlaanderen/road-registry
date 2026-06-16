namespace RoadRegistry.Read.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class GradeSeparatedJunctionReadProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<GradeSeparatedJunctionReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_gradeseparatedjunctions")
            .Identity(x => x.Id);
    }

    public GradeSeparatedJunctionReadProjection()
    {
        // V1
        When<IEvent<GradeSeparatedJunction.Events.V1.ImportedGradeSeparatedJunction>>(async (session, e, ct) =>
        {
            var junction = new GradeSeparatedJunctionReadItem
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(e.Data.Id),
                LowerRoadSegmentId = new RoadSegmentId(e.Data.LowerRoadSegmentId),
                UpperRoadSegmentId = new RoadSegmentId(e.Data.UpperRoadSegmentId),
                Type = e.Data.Type,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            };
            session.Store(junction);

            await UpdateRoadSegmentGradeSeparatedJunctionIds(session, junction.GradeSeparatedJunctionId, (null, null), (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId), ct);
        });
        When<IEvent<GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionAdded>>(async (session, e, ct) =>
        {
            var junction = new GradeSeparatedJunctionReadItem
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(e.Data.Id),
                LowerRoadSegmentId = new RoadSegmentId(e.Data.LowerRoadSegmentId),
                UpperRoadSegmentId = new RoadSegmentId(e.Data.UpperRoadSegmentId),
                Type = e.Data.Type,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            };
            session.Store(junction);

            await UpdateRoadSegmentGradeSeparatedJunctionIds(session, junction.GradeSeparatedJunctionId, (null, null), (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId), ct);
        });
        When<IEvent<GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionModified>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.Id, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.Id}");
            }

            var originalRoadSegmentIds = (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId);

            junction.LastModified = e.Data.Provenance.ToEventTimestamp();
            junction.Type = e.Data.Type;
            junction.LowerRoadSegmentId = new RoadSegmentId(e.Data.LowerRoadSegmentId);
            junction.UpperRoadSegmentId = new RoadSegmentId(e.Data.UpperRoadSegmentId);

            session.Store(junction);

            await UpdateRoadSegmentGradeSeparatedJunctionIds(session, junction.GradeSeparatedJunctionId, originalRoadSegmentIds, (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId), ct);
        });
        When<IEvent<GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionRemoved>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.Id, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.Id}");
            }

            junction.IsRemoved = true;
            session.Store(junction);

            await UpdateRoadSegmentGradeSeparatedJunctionIds(session, junction.GradeSeparatedJunctionId, (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId), (null, null), ct);
        });

        // V2
        When<IEvent<GradeSeparatedJunctionWasAdded>>(async (session, e, ct) =>
        {
            var junction = new GradeSeparatedJunctionReadItem
            {
                GradeSeparatedJunctionId = e.Data.GradeSeparatedJunctionId,
                LowerRoadSegmentId = new RoadSegmentId(e.Data.LowerRoadSegmentId),
                UpperRoadSegmentId = new RoadSegmentId(e.Data.UpperRoadSegmentId),
                Type = e.Data.Type.ToString(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(junction);

            await UpdateRoadSegmentGradeSeparatedJunctionIds(session, junction.GradeSeparatedJunctionId, (null, null), (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId), ct);
        });
        When<IEvent<GradeSeparatedJunctionWasModified>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            var originalRoadSegmentIds = (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId);

            junction.LastModified = e.Data.Provenance.ToEventTimestamp();
            junction.Type = e.Data.Type?.ToString() ?? junction.Type;
            junction.LowerRoadSegmentId = e.Data.LowerRoadSegmentId ?? junction.LowerRoadSegmentId;
            junction.UpperRoadSegmentId = e.Data.UpperRoadSegmentId ?? junction.UpperRoadSegmentId;

            session.Store(junction);

            await UpdateRoadSegmentGradeSeparatedJunctionIds(session, junction.GradeSeparatedJunctionId, originalRoadSegmentIds, (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId), ct);
        });
        When<IEvent<GradeSeparatedJunctionWasRemoved>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            junction.IsRemoved = true;
            session.Store(junction);

            await UpdateRoadSegmentGradeSeparatedJunctionIds(session, junction.GradeSeparatedJunctionId, (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId), (null, null), ct);
        });
        When<IEvent<GradeSeparatedJunctionWasRemovedBecauseOfMigration>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            junction.IsRemoved = true;
            session.Store(junction);

            await UpdateRoadSegmentGradeSeparatedJunctionIds(session, junction.GradeSeparatedJunctionId, (junction.LowerRoadSegmentId, junction.UpperRoadSegmentId), (null, null), ct);
        });
    }

    private static async Task UpdateRoadSegmentGradeSeparatedJunctionIds(
        IDocumentOperations session,
        GradeSeparatedJunctionId gradeSeparatedJunctionId,
        (RoadSegmentId? Lower, RoadSegmentId? Upper) originalRoadSegmentIds,
        (RoadSegmentId? Lower, RoadSegmentId? Upper) updatedRoadSegmentIds,
        CancellationToken ct)
    {
        var roadSegmentIds = new[]
            {
                originalRoadSegmentIds.Lower,
                originalRoadSegmentIds.Upper,
                updatedRoadSegmentIds.Lower,
                updatedRoadSegmentIds.Upper
            }
            .Where(x => x is not null)
            .Select(x => x!.Value.ToInt32())
            .Distinct()
            .ToArray();

        var roadSegments = await session.LoadManyAsync<RoadSegmentReadItem>(ct, roadSegmentIds);

        if (originalRoadSegmentIds.Lower is not null)
        {
            var roadSegment = roadSegments.SingleOrDefault(x => x.RoadSegmentId == originalRoadSegmentIds.Lower.Value)
                              ?? throw new InvalidOperationException($"No road segment found for Id {originalRoadSegmentIds.Lower.Value}");
            roadSegment.GradeSeparatedJunctionIds = roadSegment.GradeSeparatedJunctionIds.Except([gradeSeparatedJunctionId]).ToArray();
            session.Store(roadSegment);
        }
        if (originalRoadSegmentIds.Upper is not null)
        {
            var roadSegment = roadSegments.SingleOrDefault(x => x.RoadSegmentId == originalRoadSegmentIds.Upper.Value)
                              ?? throw new InvalidOperationException($"No road segment found for Id {originalRoadSegmentIds.Upper.Value}");
            roadSegment.GradeSeparatedJunctionIds = roadSegment.GradeSeparatedJunctionIds.Except([gradeSeparatedJunctionId]).ToArray();
            session.Store(roadSegment);
        }

        if (updatedRoadSegmentIds.Lower is not null)
        {
            var roadSegment = roadSegments.SingleOrDefault(x => x.RoadSegmentId == updatedRoadSegmentIds.Lower.Value)
                              ?? throw new InvalidOperationException($"No road segment found for Id {updatedRoadSegmentIds.Lower.Value}");
            roadSegment.GradeSeparatedJunctionIds = roadSegment.GradeSeparatedJunctionIds.Union([gradeSeparatedJunctionId]).OrderBy(x => x).ToArray();
            session.Store(roadSegment);
        }
        if (updatedRoadSegmentIds.Upper is not null)
        {
            var roadSegment = roadSegments.SingleOrDefault(x => x.RoadSegmentId == updatedRoadSegmentIds.Upper.Value)
                              ?? throw new InvalidOperationException($"No road segment found for Id {updatedRoadSegmentIds.Upper.Value}");
            roadSegment.GradeSeparatedJunctionIds = roadSegment.GradeSeparatedJunctionIds.Union([gradeSeparatedJunctionId]).OrderBy(x => x).ToArray();
            session.Store(roadSegment);
        }
    }
}

public sealed class GradeSeparatedJunctionReadItem
{
    [JsonIgnore]
    public int Id { get; private set; }

    public required GradeSeparatedJunctionId GradeSeparatedJunctionId
    {
        get => new(Id);
        set => Id = value;
    }

    public required RoadSegmentId LowerRoadSegmentId { get; set; }
    public required RoadSegmentId UpperRoadSegmentId { get; set; }
    public required string Type { get; set; }

    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; set; }
    public required bool IsV2 { get; set; }
    public bool IsRemoved { get; set; }
}
