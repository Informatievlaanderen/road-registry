namespace MartenPoc;

using System;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using Weasel.Postgresql.Tables;

public class RoadNetworkTopologyProjection : EventProjection
{
    private const string TableName = "roadnetworksegments";

    public RoadNetworkTopologyProjection()
    {
        var table = new Table(TableName);
        table.AddColumn<Guid>("id").AsPrimaryKey();
        table.AddColumn<int>("version");
        table.AddColumn("geometry", "geometry");
        table.AddColumn<Guid>("start_node_id");
        table.AddColumn<Guid>("end_node_id");
        table.AddColumn<string>("causation_id");
        SchemaObjects.Add(table);

        Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);
    }

    public void Project(IEvent<WegsegmentWerdToegevoegd> e, IDocumentOperations ops)
    {
        //note: bounding box van geometry opslaan voor snelheid?

        ops.QueueSqlCommand($"insert into {TableName} (id, version, geometry, start_node_id, end_node_id, causation_id) values (?, ?, ST_GeomFromText(?, 0), ?, ?, ?)",
            e.Data.Id, e.Version, e.Data.Geometry, e.Data.StartNodeId, e.Data.EndNodeId, e.CausationId
        );
    }

    public void Project(IEvent<WegsegmentWerdGewijzigd> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"update {TableName} set version = ?, geometry = ?, start_node_id = ?, end_node_id = ?, causation_id = ? where id = ?",
            e.Version, e.Data.Geometry, e.Data.StartNodeId, e.Data.EndNodeId, e.CausationId, e.Data.Id
        );
    }

    // public RoadNetworkSegment Create(IEvent<WegsegmentWerdToegevoegd> input)
    // {
    //     return new RoadNetworkSegment(input.Data.Id, input.Data.Geometry, input.Data.StartNodeId, input.Data.EndNodeId, input.CausationId);
    // }
    //
    // public RoadNetworkSegment Create(IEvent<WegsegmentWerdGewijzigd> input)
    // {
    //     return new RoadNetworkSegment(input.Data.Id, input.Data.Geometry, input.Data.StartNodeId, input.Data.EndNodeId, input.CausationId);
    // }
}

// public record RoadNetworkSegment(Guid Id, string Geometry, Guid StartNodeId, Guid EndNodeId, string CausationId)
// {
//     public static RoadNetworkSegment Create(IEvent<WegsegmentWerdToegevoegd> werdToegevoegd) =>
//         new(
//             werdToegevoegd.Id,
//             werdToegevoegd.Data.Geometry,
//             werdToegevoegd.Data.StartNodeId,
//             werdToegevoegd.Data.EndNodeId,
//             werdToegevoegd.CausationId);
//
//     public RoadNetworkSegment Apply(WegsegmentWerdGewijzigd werdGewijzigd) =>
//         this with
//         {
//             Geometry = werdGewijzigd.Geometry,
//             StartNodeId = werdGewijzigd.StartNodeId,
//             EndNodeId = werdGewijzigd.EndNodeId
//         };
// }
