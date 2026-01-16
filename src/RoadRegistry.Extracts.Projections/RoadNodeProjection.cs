namespace RoadRegistry.Extracts.Projections;

using System;
using System.Threading.Tasks;
using BackOffice;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadNode.Events.V1;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class RoadNodeProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadNodeExtractItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("extract_roadnodes")
            .Identity(x => x.Id);
    }

    public RoadNodeProjection()
    {
        // V1
        When<IEvent<ImportedRoadNode>>((session, e, _) =>
        {
            session.Store(new RoadNodeExtractItem
            {
                RoadNodeId = new RoadNodeId(e.Data.RoadNodeId),
                Geometry = e.Data.Geometry,
                Type = RoadNodeType.Parse(e.Data.Type),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNodeAdded>>((session, e, _) =>
        {
            session.Store(new RoadNodeExtractItem
            {
                RoadNodeId = new RoadNodeId(e.Data.RoadNodeId),
                Geometry = e.Data.Geometry,
                Type = RoadNodeType.Parse(e.Data.Type),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNodeModified>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Type = RoadNodeType.Parse(e.Data.Type);
            node.Geometry = e.Data.Geometry;

            session.Store(node);
        });
        When<IEvent<RoadNodeRemoved>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadNodeId}");
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
                Type = RoadNodeType.Parse(e.Data.Type),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            });
            return Task.CompletedTask;
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasModified>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Type = e.Data.Type ?? node.Type;
            node.Geometry = e.Data.Geometry ?? node.Geometry;

            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasMigrated>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadNodeId}");
            }

            node.LastModified = e.Data.Provenance.ToEventTimestamp();
            node.Type = e.Data.Type;
            node.Geometry = e.Data.Geometry;

            session.Store(node);
        });
        When<IEvent<RoadNode.Events.V2.RoadNodeWasRemoved>>(async (session, e, _) =>
        {
            var node = await session.LoadAsync<RoadNodeExtractItem>(e.Data.RoadNodeId);
            if (node is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadNodeId}");
            }

            session.Delete(node);
        });
    }
}

public sealed class RoadNodeExtractItem
{
    [JsonIgnore]
    public int Id { get; private set; }

    public required RoadNodeId RoadNodeId
    {
        get => new(Id);
        set => Id = value;
    }
    public required EventTimestamp Origin { get; init; }

    public required RoadNodeGeometry Geometry { get; set; }
    public required RoadNodeType Type { get; set; }
    public required EventTimestamp LastModified { get; set; }
}
