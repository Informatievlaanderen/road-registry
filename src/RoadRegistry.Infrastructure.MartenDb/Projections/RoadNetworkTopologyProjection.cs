namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Text;
using GradeSeparatedJunction.Events;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using RoadSegment;
using RoadSegment.Events;
using Weasel.Postgresql.Tables;

public class RoadNetworkTopologyProjection : EventProjection
{
    public const string RoadSegmentsTableName = "roadnetworktopology.roadsegments";
    public const string GradeSeparatedJunctionsTableName = "roadnetworktopology.gradeseparatedjunctions";

    public RoadNetworkTopologyProjection()
    {
        {
            var table = new Table(RoadSegmentsTableName);
            table.AddColumn<int>("id").AsPrimaryKey();
            table.AddColumn("geometry", "geometry");
            table.AddColumn<int>("start_node_id");
            table.AddColumn<int>("end_node_id");

            var geometryIndex = new IndexDefinition("idx_segments_geometry").AgainstColumns("geometry");
            geometryIndex.CustomMethod = "gist";
            table.Indexes.Add(geometryIndex);

            SchemaObjects.Add(table);
            Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);
        }

        {
            var table = new Table(GradeSeparatedJunctionsTableName);
            table.AddColumn<int>("id").AsPrimaryKey();
            table.AddColumn<int>("lower_road_segment_id");
            table.AddColumn<int>("upper_road_segment_id");

            table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_lower_road_segment_id").AgainstColumns("lower_road_segment_id"));
            table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_upper_road_segment_id").AgainstColumns("upper_road_segment_id"));

            SchemaObjects.Add(table);
            Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);
        }
    }

    //TODO-pr unit test toevoegen die checkt of elke event hier een Project-methode heeft

    public void Project(IEvent<RoadSegmentAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"insert into {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id) values (?, ST_GeomFromText(?, ?), ?, ?)",
            e.Data.RoadSegmentId.ToInt32(), e.Data.Geometry.ToMultiLineString().AsText(), e.Data.Geometry.SRID, e.Data.StartNodeId.ToInt32(), e.Data.EndNodeId.ToInt32()
        );
    }

    public void Project(IEvent<RoadSegmentModified> e, IDocumentOperations ops)
    {
        var parameters = new List<object>();
        var tableUpdates = new List<string>();

        if (e.Data.Geometry is not null)
        {
            tableUpdates.Add("geometry = ST_GeomFromText(?, ?)");
            parameters.Add(e.Data.Geometry.ToMultiLineString().AsText());
            parameters.Add(e.Data.Geometry.SRID);
        }

        if (e.Data.StartNodeId is not null)
        {
            tableUpdates.Add("start_node_id = ?");
            parameters.Add(e.Data.StartNodeId);
        }

        if (e.Data.EndNodeId is not null)
        {
            tableUpdates.Add("end_node_id = ?");
            parameters.Add(e.Data.EndNodeId);
        }

        if (!tableUpdates.Any())
        {
            return;
        }

        ops.QueueSqlCommand($"update {RoadSegmentsTableName} set {string.Join(", ", tableUpdates)} where id = {e.Data.RoadSegmentId}",
            parameters.ToArray()
        );
    }

    public void Project(IEvent<RoadSegmentRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"delete from {RoadSegmentsTableName} where id = ?",
            e.Data.RoadSegmentId.ToInt32()
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"insert into {GradeSeparatedJunctionsTableName} (id, lower_road_segment_id, upper_road_segment_id) values (?, ST_GeomFromText(?, ?), ?, ?)",
            e.Data.GradeSeparatedJunctionId.ToInt32(), e.Data.LowerRoadSegmentId.ToInt32(), e.Data.UpperRoadSegmentId.ToInt32()
        );
    }
}
