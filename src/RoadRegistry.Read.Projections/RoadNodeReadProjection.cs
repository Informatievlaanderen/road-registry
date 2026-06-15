namespace RoadRegistry.Read.Projections;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackOffice;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadNode.Events.V1;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class RoadNodeReadProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadNodeReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_roadnodes")
            .Identity(x => x.Id);
    }

    public RoadNodeReadProjection()
    {
        // V1
        When<IEvent<ImportedRoadNode>>((session, e, _) =>
        {
            session.Store(new RoadNodeReadItem
            {
                RoadNodeId = new RoadNodeId(e.Data.RoadNodeId),
                Geometry = ProjectGeometry(e.Data.Geometry),
                Type = e.Data.Type,
                Grensknoop = false,
                RoadSegmentIds = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNodeAdded>>((session, e, _) =>
        {
            session.Store(new RoadNodeReadItem
            {
                RoadNodeId = new RoadNodeId(e.Data.RoadNodeId),
                Geometry = ProjectGeometry(e.Data.Geometry),
                Type = e.Data.Type,
                Grensknoop = false,
                RoadSegmentIds = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNodeModified>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Type = e.Data.Type;
            node.Geometry = ProjectGeometry(e.Data.Geometry);

            session.Store(node);
        });
        When<IEvent<RoadNodeRemoved>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.IsRemoved = true;
            session.Store(node);
        });

        // V2
        When<IEvent<RoadNode.Events.V2.RoadNodeWasAdded>>((session, e, _) =>
        {
            session.Store(new RoadNodeReadItem
            {
                RoadNodeId = e.Data.RoadNodeId,
                Geometry = ProjectGeometry(e.Data.Geometry),
                Type = null,
                Grensknoop = e.Data.Grensknoop,
                RoadSegmentIds = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasMigrated>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Geometry = ProjectGeometry(e.Data.Geometry);
            node.Grensknoop = e.Data.Grensknoop;
            node.IsV2 = true;

            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeTypeWasChanged>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Type = e.Data.Type.ToString();

            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasModified>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Geometry = e.Data.Geometry is not null ? ProjectGeometry(e.Data.Geometry) : node.Geometry;
            node.Grensknoop = e.Data.Grensknoop ?? node.Grensknoop;

            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasRemoved>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.IsRemoved = true;
            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasRemovedBecauseOfMigration>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.IsRemoved = true;
            session.Store(node);
        });
    }

    private static RoadNodeGeometryProjections ProjectGeometry(RoadNodeGeometry geometry)
    {
        return new RoadNodeGeometryProjections
        {
            Lambert72 = geometry.EnsureLambert72(),
            Lambert08 = geometry.EnsureLambert08(),
        };
    }
}

public sealed class RoadNodeGeometryProjections
{
    public required RoadNodeGeometry Lambert72 { get; init; }
    public required RoadNodeGeometry Lambert08 { get; init; }
}

public sealed class RoadNodeReadItem
{
    [JsonIgnore]
    public int Id { get; private set; }

    public required RoadNodeId RoadNodeId
    {
        get => new(Id);
        set => Id = value;
    }

    public required RoadNodeGeometryProjections Geometry { get; set; }
    public required string Type { get; set; }
    public required bool Grensknoop { get; set; }
    public required IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; set; }

    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; set; }

    public required bool IsV2 { get; set; }
    public bool IsRemoved { get; set; }
}
