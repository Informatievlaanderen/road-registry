namespace RoadRegistry.Read.Projections;

using System;
using System.Collections.Generic;
using System.Threading;
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
        When<IEvent<ImportedRoadNode>>((session, e, ct) =>
            AddOrUpdateRoadNode(session, new RoadNodeReadItem
            {
                RoadNodeId = new RoadNodeId(e.Data.RoadNodeId),
                Geometry = ProjectGeometry(e.Data.Geometry, isV2: false),
                Type = e.Data.Type,
                Grensknoop = false,
                RoadSegmentIds = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            }, ct));
        When<IEvent<RoadNodeAdded>>((session, e, ct) =>
            AddOrUpdateRoadNode(session, new RoadNodeReadItem
            {
                RoadNodeId = new RoadNodeId(e.Data.RoadNodeId),
                Geometry = ProjectGeometry(e.Data.Geometry, isV2: false),
                Type = e.Data.Type,
                Grensknoop = false,
                RoadSegmentIds = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            }, ct));
        When<IEvent<RoadNodeModified>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Type = e.Data.Type;
            node.Geometry = ProjectGeometry(e.Data.Geometry, isV2: false);

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
        When<IEvent<RoadNode.Events.V2.RoadNodeWasAdded>>((session, e, ct) =>
            AddOrUpdateRoadNode(session, new RoadNodeReadItem
            {
                RoadNodeId = e.Data.RoadNodeId,
                Geometry = ProjectGeometry(e.Data.Geometry, isV2: true),
                Type = null,
                Grensknoop = e.Data.Grensknoop,
                RoadSegmentIds = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            }, ct));
        When<IEvent<RoadNode.Events.V2.RoadNodeWasMigrated>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeReadItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No road node found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Geometry = ProjectGeometry(e.Data.Geometry, isV2: true);
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
            node.Geometry = e.Data.Geometry is not null ? ProjectGeometry(e.Data.Geometry, isV2: true) : node.Geometry;
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

    private static async Task AddOrUpdateRoadNode(IDocumentOperations session, RoadNodeReadItem roadNode, CancellationToken ct)
    {
        // Apply idempotently. If the node already exists - because the add event is reprocessed, or another
        // projection already loaded it into this shared session during the batch - mutate the existing tracked
        // instance instead of Storing a new one. Storing a different instance for an already-tracked id makes Marten
        // throw "Document ... with same Id already added to the session", and it would also drop the RoadSegmentIds
        // owned by the road segment projection.
        var existing = await session.LoadAsync<RoadNodeReadItem>(roadNode.RoadNodeId, ct);
        if (existing is not null)
        {
            existing.CopyDataFrom(roadNode);
            session.Store(existing);
        }
        else
        {
            session.Store(roadNode);
        }
    }

    private static RoadNodeGeometryProjections ProjectGeometry(RoadNodeGeometry geometry, bool isV2)
    {
        return new RoadNodeGeometryProjections
        {
            Lambert72 = isV2 ? geometry.EnsureLambert72().RoundToCm() : geometry.EnsureLambert72(),
            Lambert08 = geometry.EnsureLambert08().RoundToCm(),
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
    public required string? Type { get; set; }
    public required bool Grensknoop { get; set; }

    public IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; set; } = [];

    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; set; }
    public required bool IsV2 { get; set; }
    public bool IsRemoved { get; set; }

    // Overwrites the event-owned fields from another read item, deliberately leaving the identity (Id), the original
    // Origin and the RoadSegmentIds (owned by the road segment projection) untouched. Used to apply an "add" event
    // idempotently onto an already-existing document without losing those fields.
    public void CopyDataFrom(RoadNodeReadItem source)
    {
        Geometry = source.Geometry;
        Type = source.Type;
        Grensknoop = source.Grensknoop;
        LastModified = source.LastModified;
        IsV2 = source.IsV2;
        IsRemoved = source.IsRemoved;
    }
}
