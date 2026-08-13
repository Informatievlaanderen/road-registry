-- A road segment that is not realized has no road nodes at all, but the topology projection stored 0 for
-- "no node". Two node-less segments both carrying 0 join each other on that shared sentinel in the
-- connected-segments query, so "no node" becomes NULL (the columns have always been nullable).
--
-- networktopology_update_roadsegment used to read a node id of 0 as "leave unchanged", which meant it could
-- not clear a node id. The node id parameters become real values: an id (> 0) or NULL for "no node", both
-- saved as-is on insert and update; only -1 means "leave unchanged" on update. The
-- corrected-from-realized-to-planned handler moves onto this function instead of its own UPDATE statement.

UPDATE projections.networktopology_roadsegments
SET start_node_id = NULLIF(start_node_id, 0),
    end_node_id = NULLIF(end_node_id, 0)
WHERE start_node_id = 0
   OR end_node_id = 0;

CREATE OR REPLACE FUNCTION projections.networktopology_update_roadsegment(p_id integer, p_timestamp timestamptz, p_wkt character varying, p_srid integer, p_start_node_id integer, p_end_node_id integer, p_is_v2 boolean) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE projections.networktopology_roadsegments
    SET geometry = (CASE WHEN p_wkt <> '' THEN ST_GeomFromText(p_wkt, p_srid) ELSE geometry END),
        start_node_id = (CASE WHEN p_start_node_id IS NOT DISTINCT FROM -1 THEN start_node_id ELSE p_start_node_id END),
        end_node_id = (CASE WHEN p_end_node_id IS NOT DISTINCT FROM -1 THEN end_node_id ELSE p_end_node_id END),
        is_v2 = p_is_v2,
        timestamp = p_timestamp
    WHERE id = p_id
      AND timestamp <= p_timestamp;

    GET DIAGNOSTICS updated = ROW_COUNT;

    IF updated = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on road segment %', p_id
            USING ERRCODE = '40001';
    END IF;

    RETURN updated;

END;
$$ LANGUAGE plpgsql;
