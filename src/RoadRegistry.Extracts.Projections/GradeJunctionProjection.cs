namespace RoadRegistry.Extracts.Projections;

using System;
using System.Threading.Tasks;
using BackOffice;
using GradeJunction.Events.V2;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class GradeJunctionProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<GradeJunctionExtractItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("extract_gradejunctions")
            .Identity(x => x.Id);
    }

    public GradeJunctionProjection()
    {
        // V2
        When<IEvent<GradeJunctionWasAdded>>((session, e, _) =>
        {
            session.Store(new GradeJunctionExtractItem
            {
                GradeJunctionId = e.Data.GradeJunctionId,
                RoadSegmentId1 = new RoadSegmentId(e.Data.RoadSegmentId1),
                RoadSegmentId2 = new RoadSegmentId(e.Data.RoadSegmentId2),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            });
            return Task.CompletedTask;
        });
        When<IEvent<GradeJunctionWasRemoved>>(async (session, e, _) =>
        {
            var junction = await session.LoadAsync<GradeJunctionExtractItem>(e.Data.GradeJunctionId);
            if (junction is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.GradeJunctionId}");
            }

            session.Delete(junction);
        });
    }
}

public sealed class GradeJunctionExtractItem
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
}
