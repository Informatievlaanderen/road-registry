namespace RoadRegistry.Infrastructure.MartenDb.Projections;

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
            table.AddColumn<string>("causation_id");

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
            table.AddColumn<string>("causation_id");

            table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_lower_road_segment_id").AgainstColumns("lower_road_segment_id"));
            table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_upper_road_segment_id").AgainstColumns("upper_road_segment_id"));

            SchemaObjects.Add(table);
            Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);
        }
    }

    //TODO-pr unit test toevoegen die checkt of elke event hier een Project-methode heeft?

    public void Project(IEvent<RoadSegmentAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"insert into {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id, causation_id) values (?, ST_GeomFromText(?, ?), ?, ?, ?)",
            e.Data.RoadSegmentId.ToInt32(), e.Data.Geometry.AsMultiLineString().AsText(), e.Data.Geometry.SRID, e.Data.StartNodeId.ToInt32(), e.Data.EndNodeId.ToInt32(), e.CausationId!
        );
    }

    public void Project(IEvent<RoadSegmentModified> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"update {RoadSegmentsTableName} set geometry = ST_GeomFromText(?, ?), start_node_id = ?, end_node_id = ?, causation_id = ? where id = ?",
            e.Data.Geometry.AsMultiLineString().AsText(), e.Data.Geometry.SRID, e.Data.StartNodeId.ToInt32(), e.Data.EndNodeId.ToInt32(), e.CausationId!, e.Data.RoadSegmentId.ToInt32()
        );
    }

    public void Project(IEvent<RoadSegmentRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"delete from {RoadSegmentsTableName} where id = ?",
            e.Data.RoadSegmentId.ToInt32()
        );
    }

    //TODO-pr gradeseparated junctions: zonder geometry, enkel administratieve data
    public void Project(IEvent<GradeSeparatedJunctionAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"insert into {GradeSeparatedJunctionsTableName} (id, lower_road_segment_id, upper_road_segment_id, causation_id) values (?, ST_GeomFromText(?, ?), ?, ?, ?)",
            e.Data.GradeSeparatedJunctionId.ToInt32(), e.Data.LowerRoadSegmentId.ToInt32(), e.Data.UpperRoadSegmentId.ToInt32(), e.CausationId!
        );
    }
}
