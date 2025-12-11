namespace RoadRegistry.Extracts.Projections;

using System;
using System.Threading.Tasks;
using GradeSeparatedJunction.Events.V2;
using Infrastructure.MartenDb.Projections;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;

public class GradeSeparatedJunctionProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<GradeSeparatedJunctionExtractItem>()
            .DatabaseSchemaName("projections")
            .DocumentAlias("extract_gradeseparatedjunctions")
            .Identity(x => x.Id);
    }

    public GradeSeparatedJunctionProjection()
    {
        // V1
        When<IEvent<GradeSeparatedJunction.Events.V1.ImportedGradeSeparatedJunction>>((session, e, _) =>
        {
            session.Store(new GradeSeparatedJunctionExtractItem
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(e.Data.Id),
                LowerRoadSegmentId = new RoadSegmentId(e.Data.LowerRoadSegmentId),
                UpperRoadSegmentId = new RoadSegmentId(e.Data.UpperRoadSegmentId),
                Type = GradeSeparatedJunctionType.Parse(e.Data.Type),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            });
            return Task.CompletedTask;
        });
        When<IEvent<GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionAdded>>((session, e, _) =>
        {
            session.Store(new GradeSeparatedJunctionExtractItem
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(e.Data.Id),
                LowerRoadSegmentId = new RoadSegmentId(e.Data.LowerRoadSegmentId),
                UpperRoadSegmentId = new RoadSegmentId(e.Data.UpperRoadSegmentId),
                Type = GradeSeparatedJunctionType.Parse(e.Data.Type),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            });
            return Task.CompletedTask;
        });
        When<IEvent<GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionRemoved>>(async (session, e, _) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionExtractItem>(e.Data.Id);
            if (junction is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.Id}");
            }

            session.Delete(junction);
        });

        // V2
        When<IEvent<GradeSeparatedJunctionWasAdded>>((session, e, _) =>
        {
            session.Store(new GradeSeparatedJunctionExtractItem
            {
                GradeSeparatedJunctionId = e.Data.GradeSeparatedJunctionId,
                LowerRoadSegmentId = new RoadSegmentId(e.Data.LowerRoadSegmentId),
                UpperRoadSegmentId = new RoadSegmentId(e.Data.UpperRoadSegmentId),
                Type = GradeSeparatedJunctionType.Parse(e.Data.Type),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            });
            return Task.CompletedTask;
        });
        When<IEvent<GradeSeparatedJunctionWasModified>>(async (session, e, _) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionExtractItem>(e.Data.GradeSeparatedJunctionId);
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
            var junction = await session.LoadAsync<GradeSeparatedJunctionExtractItem>(e.Data.GradeSeparatedJunctionId);
            if (junction is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            session.Delete(junction);
        });
    }
}

public sealed class GradeSeparatedJunctionExtractItem
{
    [JsonIgnore]
    public int Id { get; private set; }

    public required GradeSeparatedJunctionId GradeSeparatedJunctionId
    {
        get => new(Id);
        set => Id = value;
    }
    public required EventTimestamp Origin { get; init; }

    public required RoadSegmentId LowerRoadSegmentId { get; set; }
    public required RoadSegmentId UpperRoadSegmentId { get; set; }
    public required GradeSeparatedJunctionType Type { get; set; }
    public required EventTimestamp LastModified { get; set; }

}
