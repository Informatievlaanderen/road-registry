namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using Marten.Events.Projections;
using Weasel.Postgresql;
using Weasel.Postgresql.Functions;
using Weasel.Postgresql.Tables;

public partial class RoadNetworkTopologyProjection : EventProjection
{
    public const string RoadSegmentsTableName = "projections.networktopology_roadsegments";
    public const string GradeSeparatedJunctionsTableName = "projections.networktopology_gradeseparatedjunctions";

    public RoadNetworkTopologyProjection()
    {
        {
            var table = new Table(RoadSegmentsTableName);
            table.AddColumn<int>("id").AsPrimaryKey();
            table.AddColumn("geometry", "geometry");
            table.AddColumn<int>("start_node_id");
            table.AddColumn<int>("end_node_id");
            table.AddColumn<DateTimeOffset>("timestamp");

            var geometryIndex = new IndexDefinition("idx_segments_geometry").AgainstColumns("geometry");
            geometryIndex.CustomMethod = "gist";
            table.Indexes.Add(geometryIndex);

            table.Indexes.Add(new IndexDefinition("idx_roadsegments_start_node_id").AgainstColumns("start_node_id"));
            table.Indexes.Add(new IndexDefinition("idx_roadsegments_end_node_id").AgainstColumns("end_node_id"));

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
        RAISE EXCEPTION 'Concurrency conflict on segment %', 42
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;"));

            SchemaObjects.Add(new Function(new PostgresqlObjectName("projections", "networktopology_update_roadsegment"), @$"
CREATE OR REPLACE FUNCTION projections.networktopology_update_roadsegment(p_id integer, p_timestamp timestamptz, p_wkt character varying, p_srid integer, p_start_node_id integer, p_end_node_id integer) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE {RoadSegmentsTableName}
    SET geometry = (CASE WHEN p_wkt IS NOT NULL THEN ST_GeomFromText(p_wkt, p_srid) ELSE geometry END),
        start_node_id = coalesce(p_start_node_id, start_node_id),
        end_node_id = coalesce(p_end_node_id, end_node_id),
        timestamp = p_timestamp
    WHERE id = p_id
      AND timestamp < p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on segment %', 42
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;"));
        }

        {
            var table = new Table(GradeSeparatedJunctionsTableName);
            table.AddColumn<int>("id").AsPrimaryKey();
            table.AddColumn<int>("lower_road_segment_id");
            table.AddColumn<int>("upper_road_segment_id");
            table.AddColumn<DateTimeOffset>("timestamp");

            table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_lower_road_segment_id").AgainstColumns("lower_road_segment_id"));
            table.Indexes.Add(new IndexDefinition("idx_gradeseparatedjunctions_upper_road_segment_id").AgainstColumns("upper_road_segment_id"));

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
CREATE OR REPLACE FUNCTION projections.networktopology_update_roadsegment(p_id integer, p_timestamp timestamptz, p_lower_road_segment_id integer, p_upper_road_segment_id integer) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE {GradeSeparatedJunctionsTableName}
    SET lower_road_segment_id = coalesce(p_lower_road_segment_id, lower_road_segment_id),
        upper_road_segment_id = coalesce(p_upper_road_segment_id, upper_road_segment_id),
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
}
