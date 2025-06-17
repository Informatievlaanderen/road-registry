namespace MartenPoc.Projections;

using System;
using System.Threading.Tasks;
using Domain;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Marten.Events.Projections.Flattened;
using Marten.Schema;
using NpgsqlTypes;
using Weasel.Postgresql.Tables;

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

public class RoadSegmentProjection : MultiStreamProjection<RoadNetworkChange, string>
// FlatTableProjection
// EventProjection
// MultiStreamProjection<RoadSegment, string>
{
    //TODO-pr afwerken
    private const string TableName = "road.segments";

    public RoadSegmentProjection()
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
