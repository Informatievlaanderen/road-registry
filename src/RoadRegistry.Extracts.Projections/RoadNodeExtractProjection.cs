namespace RoadRegistry.Extracts.Projections;

using System;
using System.Threading.Tasks;
using BackOffice;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadNode.Events.V1;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class RoadNodeExtractProjection : MartenRoadNetworkChangesProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadNodeExtractItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("extract_roadnodes")
            .Identity(x => x.Id);
    }

    public RoadNodeExtractProjection()
    {
        // V1
        When<IEvent<ImportedRoadNode>>((session, e, _) =>
        {
            session.Store(new RoadNodeExtractItem
            {
                RoadNodeId = new RoadNodeId(e.Data.RoadNodeId),
                Geometry = e.Data.Geometry.EnsureLambert08().RoundToCm(),
                Type = e.Data.Type,
                Grensknoop = false,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNodeAdded>>((session, e, _) =>
        {
            session.Store(new RoadNodeExtractItem
            {
                RoadNodeId = new RoadNodeId(e.Data.RoadNodeId),
                Geometry = e.Data.Geometry.EnsureLambert08().RoundToCm(),
                Type = e.Data.Type,
                Grensknoop = false,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNodeModified>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Type = e.Data.Type;
            node.Geometry = e.Data.Geometry.EnsureLambert08().RoundToCm();

            session.Store(node);
        });
        When<IEvent<RoadNodeRemoved>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            session.Delete(node);
        });

        // V2
        When<IEvent<RoadNode.Events.V2.RoadNodeWasAdded>>((session, e, _) =>
        {
            session.Store(new RoadNodeExtractItem
            {
                RoadNodeId = e.Data.RoadNodeId,
                Geometry = e.Data.Geometry,
                Type = e.Data.Type,
                Grensknoop = e.Data.Grensknoop,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeTypeWasChanged>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Type = e.Data.Type;

            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasModified>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Geometry = e.Data.Geometry ?? node.Geometry;
            node.Grensknoop = e.Data.Grensknoop ?? node.Grensknoop;

            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasMigrated>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                // A road node whose removal was corrected is collected again, and this is where its row comes back:
                // the correction itself carries nothing to build one from, this event carries the geometry and the
                // grensknoop, and the RoadNodeTypeWasChanged that follows carries the type.
                node = new RoadNodeExtractItem
                {
                    RoadNodeId = e.Data.RoadNodeId,
                    Geometry = e.Data.Geometry,
                    Type = null!,
                    Grensknoop = e.Data.Grensknoop,
                    Origin = e.Data.Provenance.ToEventTimestamp(),
                    LastModified = e.Data.Provenance.ToEventTimestamp(),
                    IsV2 = true
                };

                session.Store(node);
                return;
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Geometry = e.Data.Geometry;
            node.Grensknoop = e.Data.Grensknoop;
            node.IsV2 = true;

            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasRemoved>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            session.Delete(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasRemovedBecauseOfMigration>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            session.Delete(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeRemovalWasCorrectedBecauseOfMigration>>((_, _, _) =>
        {
            // Nothing here: the row was deleted when the road node was removed, and the correction says no more than
            // that the removal is undone. The road node is collected again right after it, and the RoadNodeWasMigrated
            // above builds the row back with the real geometry rather than a placeholder invented here.
            return Task.CompletedTask;
        });
    }
}

public sealed class RoadNodeExtractItem
{
    [JsonIgnore] public int Id { get; private set; }

    public required RoadNodeId RoadNodeId
    {
        get => new(Id);
        set => Id = value;
    }

    public required RoadNodeGeometry Geometry { get; set; }
    public required string Type { get; set; }
    public required bool Grensknoop { get; set; }
    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; set; }
    public required bool IsV2 { get; set; }
}
