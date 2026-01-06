namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using Marten.Events.Projections;
using Weasel.Postgresql;
using Weasel.Postgresql.Functions;
using Weasel.Postgresql.Tables;

public partial class RoadNetworkTopologyProjection : EventProjection
{
    public const string RoadNodesTableName = "projections.networktopology_roadnodes";
    public const string RoadSegmentsTableName = "projections.networktopology_roadsegments";
    public const string GradeSeparatedJunctionsTableName = "projections.networktopology_gradeseparatedjunctions";

    public RoadNetworkTopologyProjection()
    {
        ConfigureRoadNode();
        ConfigureRoadSegment();
        ConfigureGradeSeparatedJunction();
    }

    private void ConfigureRoadNode()
    {
        var table = new Table(RoadNodesTableName);
        table.AddColumn<int>("id").AsPrimaryKey();
        table.AddColumn("geometry", "geometry");
        table.AddColumn<DateTimeOffset>("timestamp");
        table.AddColumn<bool>("is_v2").DefaultValueByExpression(false.ToString());

        var geometryIndex = new IndexDefinition("idx_roadnodes_geometry").AgainstColumns("geometry");
        geometryIndex.CustomMethod = "gist";
        table.Indexes.Add(geometryIndex);

        table.Indexes.Add(new IndexDefinition("idx_roadnodes_is_v2").AgainstColumns("is_v2"));

        SchemaObjects.Add(table);
        Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);

        SchemaObjects.Add(new Function(new PostgresqlObjectName("projections", "networktopology_delete_roadnode"), @$"
CREATE OR REPLACE FUNCTION projections.networktopology_delete_roadnode(p_id integer, p_timestamp timestamptz) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    DELETE FROM {RoadNodesTableName}
    WHERE id = p_id
      AND timestamp < p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on road node %', 42
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;"));

        SchemaObjects.Add(new Function(new PostgresqlObjectName("projections", "networktopology_update_roadnode"), @$"
CREATE OR REPLACE FUNCTION projections.networktopology_update_roadnode(p_id integer, p_timestamp timestamptz, p_wkt character varying, p_srid integer, p_is_v2 boolean) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE {RoadNodesTableName}
    SET geometry = (CASE WHEN p_wkt <> '' THEN ST_GeomFromText(p_wkt, p_srid) ELSE geometry END),
        is_v2 = p_is_v2,
        timestamp = p_timestamp
    WHERE id = p_id
      AND timestamp < p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on road node %', 42
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;"));
    }

    private void ConfigureRoadSegment()
    {
        var table = new Table(RoadSegmentsTableName);
        table.AddColumn<int>("id").AsPrimaryKey();
        table.AddColumn("geometry", "geometry");
        table.AddColumn<int>("start_node_id");
        table.AddColumn<int>("end_node_id");
        table.AddColumn<DateTimeOffset>("timestamp");
        table.AddColumn<bool>("is_v2").DefaultValueByExpression(false.ToString());

        var geometryIndex = new IndexDefinition("idx_roadsegments_geometry").AgainstColumns("geometry");
        geometryIndex.CustomMethod = "gist";
        table.Indexes.Add(geometryIndex);

        table.Indexes.Add(new IndexDefinition("idx_roadsegments_start_node_id").AgainstColumns("start_node_id"));
        table.Indexes.Add(new IndexDefinition("idx_roadsegments_end_node_id").AgainstColumns("end_node_id"));
        table.Indexes.Add(new IndexDefinition("idx_roadsegments_is_v2").AgainstColumns("is_v2"));

        SchemaObjects.Add(table);
        Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);

        SchemaObjects.Add(new Function(new PostgresqlObjectName("projections", "networktopology_delete_roadsegment"), @$"
CREATE OR REPLACE FUNCTION projections.networktopology_delete_roadsegment(p_id integer, p_timestamp timestamptz) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    DELETE FROM {RoadSegmentsTableName}
    WHERE id = p_id
      AND timestamp < p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on road segment %', 42
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;"));

        SchemaObjects.Add(new Function(new PostgresqlObjectName("projections", "networktopology_update_roadsegment"), @$"
CREATE OR REPLACE FUNCTION projections.networktopology_update_roadsegment(p_id integer, p_timestamp timestamptz, p_wkt character varying, p_srid integer, p_start_node_id integer, p_end_node_id integer, p_is_v2 boolean) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE {RoadSegmentsTableName}
    SET geometry = (CASE WHEN p_wkt <> '' THEN ST_GeomFromText(p_wkt, p_srid) ELSE geometry END),
        start_node_id = (CASE WHEN p_start_node_id > 0 THEN p_start_node_id ELSE start_node_id END),
        end_node_id = (CASE WHEN p_end_node_id > 0 THEN p_end_node_id ELSE end_node_id END),
        is_v2 = p_is_v2,
        timestamp = p_timestamp
    WHERE id = p_id
      AND timestamp < p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on road segment %', 42
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;"));
    }

    private void ConfigureGradeSeparatedJunction()
    {
        var table = new Table(GradeSeparatedJunctionsTableName);
        table.AddColumn<int>("id").AsPrimaryKey();
        table.AddColumn<int>("lower_road_segment_id");
        table.AddColumn<int>("upper_road_segment_id");
        table.AddColumn<DateTimeOffset>("timestamp");
        table.AddColumn<bool>("is_v2").DefaultValueByExpression(false.ToString());

        table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_lower_road_segment_id").AgainstColumns("lower_road_segment_id"));
        table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_upper_road_segment_id").AgainstColumns("upper_road_segment_id"));
        table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_is_v2").AgainstColumns("is_v2"));

        SchemaObjects.Add(table);
        Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);

        SchemaObjects.Add(new Function(new PostgresqlObjectName("projections", "networktopology_delete_gradeseparatedjunction"), @$"
CREATE OR REPLACE FUNCTION projections.networktopology_delete_gradeseparatedjunction(p_id integer, p_timestamp timestamptz) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    DELETE FROM {GradeSeparatedJunctionsTableName}
    WHERE id = p_id
      AND timestamp < p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on grade separated junction %', 42
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;"));

        SchemaObjects.Add(new Function(new PostgresqlObjectName("projections", "networktopology_update_gradeseparatedjunction"), @$"
CREATE OR REPLACE FUNCTION projections.networktopology_update_gradeseparatedjunction(p_id integer, p_timestamp timestamptz, p_lower_road_segment_id integer, p_upper_road_segment_id integer) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE {GradeSeparatedJunctionsTableName}
    SET lower_road_segment_id = (CASE WHEN p_lower_road_segment_id > 0 THEN p_lower_road_segment_id ELSE lower_road_segment_id END),
        upper_road_segment_id = (CASE WHEN p_upper_road_segment_id > 0 THEN p_upper_road_segment_id ELSE upper_road_segment_id END),
        timestamp = p_timestamp
    WHERE id = p_id
      AND timestamp < p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on grade separated junction %', 42
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;"));
    }
}
