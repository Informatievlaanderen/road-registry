namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using RoadSegment;
using RoadSegment.Events;
using Weasel.Postgresql.Tables;

public class RoadNetworkTopologyProjection : EventProjection
{
    public const string TableName = "road.roadnetworksegments";

    public RoadNetworkTopologyProjection()
    {
        var table = new Table(TableName);
        table.AddColumn<int>("id").AsPrimaryKey();
        table.AddColumn("geometry", "geometry");
        table.AddColumn<int>("start_node_id");
        table.AddColumn<int>("end_node_id");
        table.AddColumn<string>("causation_id");
        SchemaObjects.Add(table);

        Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);
    }

    public void Project(IEvent<RoadSegmentAdded> e, IDocumentOperations ops)
    {
        //TODO-pr bounding box van geometry opslaan voor snelheid?

        ops.QueueSqlCommand($"insert into {TableName} (id, geometry, start_node_id, end_node_id, causation_id) values (?, ST_GeomFromText(?, ?), ?, ?, ?)",
            e.Data.Id.ToInt32(), e.Data.Geometry.AsMultiLineString().AsText(), e.Data.Geometry.SRID, e.Data.StartNodeId.ToInt32(), e.Data.EndNodeId.ToInt32(), e.CausationId!
        );
    }

    public void Project(IEvent<RoadSegmentModified> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"update {TableName} set geometry = ST_GeomFromText(?, ?), start_node_id = ?, end_node_id = ?, causation_id = ? where id = ?",
            e.Data.Geometry.AsMultiLineString().AsText(), e.Data.Geometry.SRID, e.Data.StartNodeId.ToInt32(), e.Data.EndNodeId.ToInt32(), e.CausationId!, e.Data.Id.ToInt32()
        );
    }

    public void Project(IEvent<RoadSegmentRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"delete from {TableName} where id = ?",
            e.Data.Id.ToInt32()
        );
    }
}
