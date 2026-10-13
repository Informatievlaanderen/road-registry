namespace RoadRegistry.Read.Projections;

using System;
using System.Threading.Tasks;
using BackOffice;
using GradeJunction.Events.V2;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class GradeJunctionReadProjection : MartenRoadNetworkChangesProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<GradeJunctionReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_gradejunctions")
            .Identity(x => x.Id)
            // The two road segments this crossing is between, as indexed columns on the document's own table. That is
            // what lets a road segment find its crossings without either document having to know about the other -
            // see ReadModelQueries.
            .Duplicate(x => x.RoadSegmentId1, configure: index => { index.Name = "ix_read_gradejunctions_roadsegmentid1"; }, notNull: true)
            .Duplicate(x => x.RoadSegmentId2, configure: index => { index.Name = "ix_read_gradejunctions_roadsegmentid2"; }, notNull: true)
            ;
    }

    public GradeJunctionReadProjection()
    {
        // V2
        When<IEvent<GradeJunctionWasAdded>>((session, e, _) =>
        {
            session.Store(new GradeJunctionReadItem
            {
                GradeJunctionId = e.Data.GradeJunctionId,
                RoadSegmentId1 = e.Data.RoadSegmentId1,
                RoadSegmentId2 = e.Data.RoadSegmentId2,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            });

            return Task.CompletedTask;
        });

        // Not a newly observed crossing: it is the one the named grade separated junction used to record, now recorded
        // as a grade junction. For this projection it is an insert all the same.
        When<IEvent<GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange>>((session, e, _) =>
        {
            session.Store(new GradeJunctionReadItem
            {
                GradeJunctionId = e.Data.GradeJunctionId,
                RoadSegmentId1 = e.Data.RoadSegmentId1,
                RoadSegmentId2 = e.Data.RoadSegmentId2,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            });

            return Task.CompletedTask;
        });
        When<IEvent<GradeJunctionWasModified>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeJunctionReadItem>(e.Data.GradeJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade junction found for Id {e.Data.GradeJunctionId}");
            }

            junction.LastModified = e.Data.Provenance.ToEventTimestamp();
            junction.RoadSegmentId1 = e.Data.RoadSegmentId1?.ToInt32() ?? junction.RoadSegmentId1;
            junction.RoadSegmentId2 = e.Data.RoadSegmentId2?.ToInt32() ?? junction.RoadSegmentId2;
            session.Store(junction);
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
        });

        // The crossing did not disappear: it is a grade separated junction from here on, which the grade separated
        // projection inserts. For this projection the grade junction is gone all the same.
        When<IEvent<GradeJunctionWasChangedToGradeSeparatedJunction>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeJunctionReadItem>(e.Data.GradeJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade junction found for Id {e.Data.GradeJunctionId}");
            }

            junction.IsRemoved = true;
            session.Store(junction);
        });
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

    // Plain ints rather than RoadSegmentId: Marten writes a duplicated column straight from the .NET member and
    // Npgsql has no mapping for the value object. The stored document is unchanged either way - RoadSegmentId
    // serializes as a bare number.
    public required int RoadSegmentId1 { get; set; }
    public required int RoadSegmentId2 { get; set; }

    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; set; }
    public required bool IsV2 { get; set; }
    public bool IsRemoved { get; set; }
}
