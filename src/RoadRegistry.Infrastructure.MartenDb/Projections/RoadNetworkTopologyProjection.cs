namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using RoadSegment.Events;
using Weasel.Postgresql.Tables;

public class RoadNetworkTopologyProjection : EventProjection
{
    private const string TableName = "road.roadnetworksegments";

    public RoadNetworkTopologyProjection()
    {
        var table = new Table(TableName);
        table.AddColumn<Guid>("id").AsPrimaryKey();
        table.AddColumn("geometry", "geometry");
        table.AddColumn<Guid>("start_node_id");
        table.AddColumn<Guid>("end_node_id");
        table.AddColumn<string>("causation_id");
        SchemaObjects.Add(table);

        Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);
    }

    public void Project(IEvent<RoadSegmentAdded> e, IDocumentOperations ops)
    {
        //TODO-pr bounding box van geometry opslaan voor snelheid?

        ops.QueueSqlCommand($"insert into {TableName} (id,  geometry, start_node_id, end_node_id, causation_id) values (?, ?, ST_GeomFromText(?, 0), ?, ?, ?)",
            e.Data.Id, e.Data.Geometry, e.Data.StartNodeId, e.Data.EndNodeId, e.CausationId
        );
    }

    public void Project(IEvent<RoadSegmentModified> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"update {TableName} set geometry = ?, start_node_id = ?, end_node_id = ?, causation_id = ? where id = ?",
            e.Data.Geometry, e.Data.StartNodeId, e.Data.EndNodeId, e.CausationId, e.Data.Id
        );
    }

    public void Project(IEvent<RoadSegmentRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"delete from {TableName} where id = ?",
            e.Data.Id
        );
    }
}
