namespace MartenPoc.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Marten.Events.Projections.Flattened;
using Marten.Schema;
using NpgsqlTypes;
using Weasel.Core;
using Weasel.Postgresql.Tables;

public class FlattenedRoadNetworkChangeProjection : IProjection
{
    private readonly IRoadNetworkChangeProjection[] _projections = [new RoadSegmentProjection(), new RoadNodeProjection()];

    public static void Configure(StoreOptions options)
    {
        options.Projections.Add(new FlattenedRoadNetworkChangeProjection(), ProjectionLifecycle.Async,
            //projectionName: "FlattenedRoadNetworkChangeProjection",
            asyncConfiguration: options =>
            {
                options.BatchSize = 10000;
            });

        options.Schema.For<RoadSegment>()
            .DocumentAlias("projection_roadsegments")
            .Identity(x => x.Id);

        options.Schema.For<RoadNode>()
            .DocumentAlias("projection_roadnodes")
            .Identity(x => x.Id);
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        var roadNetworkChanges = events
            .GroupBy(x => x.CausationId!)
            .Select(x => x.ToArray())
            .ToArray();

        foreach (var roadNetworkChange in roadNetworkChanges)
        {
            await using var session = operations.DocumentStore.LightweightSession();

            foreach(var projection in _projections)
            {
                await projection.Project(roadNetworkChange, operations, session);
            }

            await session.SaveChangesAsync(cancellation);
        }
    }
}

public interface IRoadNetworkChangeProjection
{
    Task Project(ICollection<IEvent> events, IDocumentOperations ops, IDocumentSession session);
}

public sealed class RoadSegment
{
    public Guid Id { get; set; }
    public long Version { get; set; }
}
public class RoadSegmentProjection: IRoadNetworkChangeProjection
{
    public async Task Project(ICollection<IEvent> events, IDocumentOperations ops, IDocumentSession session)
    {
        foreach (var evt in events.OfType<IEvent<WegsegmentWerdToegevoegd>>())
        {
            await Project(evt, ops, session);
        }

        foreach (var evt in events.OfType<IEvent<WegsegmentWerdGewijzigd>>())
        {
            await Project(evt, ops, session);
        }
    }

    private async Task Project(IEvent<WegsegmentWerdToegevoegd> e, IDocumentOperations ops, IDocumentSession session)
    {
        ops.Store(new RoadSegment
        {
            Id = e.Data.Id,
            Version = e.Version
        });
    }

    private async Task Project(IEvent<WegsegmentWerdGewijzigd> e, IDocumentOperations ops, IDocumentSession session)
    {
        var roadSegment = await session.LoadAsync<RoadSegment>(e.Data.Id);

        roadSegment.Version = e.Version;

        ops.Store(roadSegment);
    }
}

public sealed class RoadNode
{
    public Guid Id { get; set; }
    public long Version { get; set; }
}
public class RoadNodeProjection : IRoadNetworkChangeProjection
{
    public async Task Project(ICollection<IEvent> events, IDocumentOperations ops, IDocumentSession session)
    {
        foreach (var evt in events.OfType<IEvent<WegknoopWerdToegevoegd>>())
        {
            await Project(evt, ops, session);
        }

        foreach (var evt in events.OfType<IEvent<WegknoopWerdGewijzigd>>())
        {
            await Project(evt, ops, session);
        }
    }

    private async Task Project(IEvent<WegknoopWerdToegevoegd> e, IDocumentOperations ops, IDocumentSession session)
    {
        ops.Store(new RoadNode
        {
            Id = e.Data.Id,
            Version = e.Version
        });
    }

    private async Task Project(IEvent<WegknoopWerdGewijzigd> e, IDocumentOperations ops, IDocumentSession session)
    {
        var roadNode = await session.LoadAsync<RoadNode>(e.Data.Id);

        roadNode.Version = e.Version;

        ops.Store(roadNode);
    }
}

public sealed class RoadNetworkChange
{
    public string Id { get; set; }
    public int Version { get; set; }
    [DuplicateField(DbType = NpgsqlDbType.Geometry, IndexMethod = IndexMethod.gist, PgType = "geometry")]
    public string Geometry { get; set; }
    public Guid StartNodeId { get; set; }
    public Guid EndNodeId { get; set; }
    public string CausationId { get; set; }
}

public class RoadNetworkProjection : MultiStreamProjection<object, string>
// FlatTableProjection
// EventProjection
// MultiStreamProjection<RoadSegment, string>
{
    //TODO-pr afwerken
    private const string TableName = "road.segments";

    public RoadNetworkProjection()
    {
        //Identity<IEvent>(e => e.CausationId ?? throw new ArgumentException("CausationId must be present"));
        CustomGrouping((session, events, grouping) =>
        {
            foreach (var evt in events)
            {
                grouping.AddEvent(evt.CausationId!, evt);
            }

            return Task.CompletedTask;
        });

        // var table = new Table(TableName);
        // table.AddColumn<Guid>(nameof(RoadSegment.Id)).AsPrimaryKey();
        // table.AddColumn<int>(nameof(RoadSegment.Version));
        // table.AddColumn(nameof(RoadSegment.Geometry), "geometry");
        // table.AddColumn<Guid>(nameof(RoadSegment.StartNodeId));
        // table.AddColumn<Guid>(nameof(RoadSegment.EndNodeId));
        // table.AddColumn<string>(nameof(RoadSegment.CausationId));
        // SchemaObjects.Add(table);

        // Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);

        ProjectEvent<IEvent<WegsegmentWerdToegevoegd>>((segment, evt) =>
        {

        });

        ProjectEvent<IEvent<WegknoopWerdToegevoegd>>((segment, evt) =>
        {

        });
    }

    // public RoadNetworkChange Create(IEvent<WegknoopWerdToegevoegd> evt)
    // {
    //     throw new NotImplementedException();
    // }
    // public void Apply(RoadNetworkChange aggregate, IEvent<WegknoopWerdToegevoegd> evt)
    // {
    //     throw new NotImplementedException();
    // }


    // public void Project(IEvent<WegsegmentWerdToegevoegd> e, IDocumentOperations ops)
    // {
    //     // ops.QueueSqlCommand($"insert into {TableName} ({nameof(RoadSegment.Id)}, {nameof(RoadSegment.Version)}, {nameof(RoadSegment.Geometry)}, {nameof(RoadSegment.StartNodeId)}, {nameof(RoadSegment.EndNodeId)}, {nameof(RoadSegment.CausationId)}) values (?, ?, ST_GeomFromText(?, 0), ?, ?, ?)",
    //     //     e.Data.Id, e.Version, e.Data.Geometry, e.Data.StartNodeId, e.Data.EndNodeId, e.CausationId
    //     // );
    // }
}
