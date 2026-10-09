namespace RoadRegistry.Read.Projections;

using System;
using System.Threading.Tasks;
using BackOffice;
using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class GradeSeparatedJunctionReadProjection : MartenRoadNetworkChangesProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<GradeSeparatedJunctionReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_gradeseparatedjunctions")
            .Identity(x => x.Id)
            // The two road segments this crossing is between, as indexed columns on the document's own table. That is
            // what lets a road segment find its crossings without either document having to know about the other -
            // see ReadModelQueries.
            .Duplicate(x => x.LowerRoadSegmentId, configure: index => { index.Name = "ix_read_gradeseparatedjunctions_lowerroadsegmentid"; }, notNull: true)
            .Duplicate(x => x.UpperRoadSegmentId, configure: index => { index.Name = "ix_read_gradeseparatedjunctions_upperroadsegmentid"; }, notNull: true)
            ;
    }

    public GradeSeparatedJunctionReadProjection()
    {
        // V1
        When<IEvent<GradeSeparatedJunction.Events.V1.ImportedGradeSeparatedJunction>>((session, e, _) =>
        {
            session.Store(new GradeSeparatedJunctionReadItem
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(e.Data.Id),
                LowerRoadSegmentId = e.Data.LowerRoadSegmentId,
                UpperRoadSegmentId = e.Data.UpperRoadSegmentId,
                Type = e.Data.Type,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            });

            return Task.CompletedTask;
        });
        When<IEvent<GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionAdded>>((session, e, _) =>
        {
            session.Store(new GradeSeparatedJunctionReadItem
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(e.Data.Id),
                LowerRoadSegmentId = e.Data.LowerRoadSegmentId,
                UpperRoadSegmentId = e.Data.UpperRoadSegmentId,
                Type = e.Data.Type,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            });

            return Task.CompletedTask;
        });
        When<IEvent<GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionModified>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.Id, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.Id}");
            }

            junction.LastModified = e.Data.Provenance.ToEventTimestamp();
            junction.Type = e.Data.Type;
            junction.LowerRoadSegmentId = e.Data.LowerRoadSegmentId;
            junction.UpperRoadSegmentId = e.Data.UpperRoadSegmentId;

            session.Store(junction);
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
        });

        // V2
        When<IEvent<GradeSeparatedJunctionWasAdded>>((session, e, _) =>
        {
            session.Store(new GradeSeparatedJunctionReadItem
            {
                GradeSeparatedJunctionId = e.Data.GradeSeparatedJunctionId,
                LowerRoadSegmentId = e.Data.LowerRoadSegmentId,
                UpperRoadSegmentId = e.Data.UpperRoadSegmentId,
                Type = e.Data.Type.ToString(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            });

            return Task.CompletedTask;
        });

        // Not a newly observed crossing: it is the one the named grade junction used to record, now recorded as a
        // grade separated junction. For this projection it is an insert all the same.
        When<IEvent<GradeSeparatedJunctionWasAddedBecauseOfGradeJunctionChange>>((session, e, _) =>
        {
            session.Store(new GradeSeparatedJunctionReadItem
            {
                GradeSeparatedJunctionId = e.Data.GradeSeparatedJunctionId,
                LowerRoadSegmentId = e.Data.LowerRoadSegmentId,
                UpperRoadSegmentId = e.Data.UpperRoadSegmentId,
                Type = e.Data.Type.ToString(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            });

            return Task.CompletedTask;
        });
        When<IEvent<GradeSeparatedJunctionWasModified>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            junction.LastModified = e.Data.Provenance.ToEventTimestamp();
            junction.Type = e.Data.Type?.ToString() ?? junction.Type;
            junction.LowerRoadSegmentId = e.Data.LowerRoadSegmentId?.ToInt32() ?? junction.LowerRoadSegmentId;
            junction.UpperRoadSegmentId = e.Data.UpperRoadSegmentId?.ToInt32() ?? junction.UpperRoadSegmentId;

            session.Store(junction);
        });
        When<IEvent<GradeSeparatedJunctionWasMigrated>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            junction.LastModified = e.Data.Provenance.ToEventTimestamp();
            junction.Type = e.Data.Type.ToString();
            junction.LowerRoadSegmentId = e.Data.LowerRoadSegmentId;
            junction.UpperRoadSegmentId = e.Data.UpperRoadSegmentId;
            junction.IsV2 = true;

            session.Store(junction);
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
        });

        // The crossing did not disappear: it is a grade junction from here on, which the grade junction projection
        // inserts. For this projection the grade separated junction is gone all the same.
        When<IEvent<GradeSeparatedJunctionWasChangedToGradeJunction>>(async (session, e, ct) =>
        {
            var junction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(e.Data.GradeSeparatedJunctionId, ct);
            if (junction is null)
            {
                throw new InvalidOperationException($"No grade separated junction found for Id {e.Data.GradeSeparatedJunctionId}");
            }

            junction.IsRemoved = true;
            session.Store(junction);
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

    // Plain ints rather than RoadSegmentId: Marten writes a duplicated column straight from the .NET member and
    // Npgsql has no mapping for the value object. The stored document is unchanged either way - RoadSegmentId
    // serializes as a bare number.
    public required int LowerRoadSegmentId { get; set; }
    public required int UpperRoadSegmentId { get; set; }
    public required string Type { get; set; }

    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; set; }
    public required bool IsV2 { get; set; }
    public bool IsRemoved { get; set; }
}
