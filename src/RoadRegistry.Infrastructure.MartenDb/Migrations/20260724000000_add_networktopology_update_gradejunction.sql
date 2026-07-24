-- A grade junction can be repointed to a different road segment when a segment is split (the junction is
-- reassigned to the correct new part). This is projected as a GradeJunctionWasModified event, which the network
-- topology projection applies via this function. A passed road segment id of 0 means "leave unchanged".
CREATE OR REPLACE FUNCTION projections.networktopology_update_gradejunction(p_id integer, p_timestamp timestamptz, p_road_segment_id_1 integer, p_road_segment_id_2 integer) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE projections.networktopology_gradejunctions
    SET road_segment_id_1 = (CASE WHEN p_road_segment_id_1 > 0 THEN p_road_segment_id_1 ELSE road_segment_id_1 END),
        road_segment_id_2 = (CASE WHEN p_road_segment_id_2 > 0 THEN p_road_segment_id_2 ELSE road_segment_id_2 END),
        timestamp = p_timestamp
    WHERE id = p_id
      AND timestamp <= p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on grade junction %', p_id
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;
