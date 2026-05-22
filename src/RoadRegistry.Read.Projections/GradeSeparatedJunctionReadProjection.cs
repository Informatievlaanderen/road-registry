namespace RoadRegistry.Read.Projections;

using System;
using System.Threading.Tasks;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
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
        // V2
        When<IEvent<GradeSeparatedJunctionWasAdded>>((session, e, _) =>
        {
            session.Store(new GradeSeparatedJunctionReadItem
            {
                GradeSeparatedJunctionId = e.Data.GradeSeparatedJunctionId,
                LowerRoadSegmentId = new RoadSegmentId(e.Data.LowerRoadSegmentId),
                UpperRoadSegmentId = new RoadSegmentId(e.Data.UpperRoadSegmentId),
                Type = e.Data.Type,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            });
            return Task.CompletedTask;
        });
        When<IEvent<GradeSeparatedJunctionWasModified>>(async (session, e, _) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId);
            if (junction is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            junction.LastModified = e.Data.Provenance.ToEventTimestamp();
            junction.Type = e.Data.Type ?? junction.Type;
            junction.LowerRoadSegmentId = e.Data.LowerRoadSegmentId ?? junction.LowerRoadSegmentId;
            junction.UpperRoadSegmentId = e.Data.UpperRoadSegmentId ?? junction.UpperRoadSegmentId;

            session.Store(junction);
        });
        When<IEvent<GradeSeparatedJunctionWasRemoved>>(async (session, e, _) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId);
            if (junction is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            session.Delete(junction);
        });
        When<IEvent<GradeSeparatedJunctionWasRemovedBecauseOfMigration>>(async (session, e, _) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId);
            if (junction is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            session.Delete(junction);
        });
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
}
