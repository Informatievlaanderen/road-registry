-- The topology keeps every road segment, removed ones included, because an action that is handed a road network has
-- to be able to say for itself that a road segment is removed rather than be given one where it silently never
-- existed (see RoadNetworkTopologyProjection-EventsV2). That leaves it with no way to answer a caller that does
-- want a subset, and an extract is exactly that caller: it is the road network as it is on the ground and as it can
-- be handed back, so it takes the gepland and gerealiseerd road segments and nothing that was removed.
--
-- Until now the extract projection's own deletes did that filtering, implicitly: the identifiers came out of the
-- topology, the documents were loaded by id, and an identifier without a document fell away. That is what made a
-- road segment retired by a merger explode on the removal that followed it - the document had been deleted on a
-- status change. The topology now records the status and whether the road segment was removed, and
-- GetUnderlyingIdsForExtract filters on both.
--
-- Marten runs with AutoCreate.None, so the columns and the storage functions that write them are added here. The
-- columns are backfilled from the road segment aggregate, which is the state after every event of the stream, so
-- the topology does not have to be rebuilt. The status stays NULL for a V1 road segment, which has no V2 status and
-- is taken into an extract whatever state it is in; a V1 removal still deletes the row.

ALTER TABLE projections.networktopology_roadsegments
    ADD COLUMN IF NOT EXISTS status character varying,
    ADD COLUMN IF NOT EXISTS is_removed boolean DEFAULT FALSE;

-- Marten serializes the status as its bare name ("Gepland", "Gerealiseerd", ...) and camelCases the property names.
-- The document is keyed by the stream key, not by the road segment id, so the id comes out of the document itself.
UPDATE projections.networktopology_roadsegments rs
   SET status = doc.data ->> 'status',
       is_removed = coalesce((doc.data ->> 'isRemoved')::boolean, FALSE)
  FROM eventstore.mt_doc_roadsegment doc
 WHERE (doc.data ->> 'roadSegmentId')::int = rs.id
   AND rs.is_v2;

-- A V2 road segment without an aggregate to read from would silently drop out of every extract from here on, which
-- is worse than a deploy that stops and says so.
DO $$
DECLARE
    without_status int;
BEGIN
    SELECT count(*) INTO without_status
    FROM projections.networktopology_roadsegments
    WHERE is_v2 AND status IS NULL;

    IF without_status > 0 THEN
        RAISE EXCEPTION '% V2 road segments in the topology have no aggregate document to read a status from', without_status;
    END IF;
END $$;

-- The two functions gain a status parameter, and the update also gains the removal mark, so the old signatures go
-- first. An empty status means "leave unchanged", the same as the empty wkt already does for the geometry: every V1
-- event passes it, and so does every V2 event that does not move the status. A NULL removal mark leaves it as it is;
-- only the events that un-remove a road segment on the aggregate pass FALSE.

DROP FUNCTION IF EXISTS projections.networktopology_insert_roadsegment(p_id integer, p_timestamp timestamptz, p_wkt character varying, p_srid integer, p_start_node_id integer, p_end_node_id integer, p_is_v2 boolean) cascade;

CREATE OR REPLACE FUNCTION projections.networktopology_insert_roadsegment(p_id integer, p_timestamp timestamptz, p_wkt character varying, p_srid integer, p_start_node_id integer, p_end_node_id integer, p_is_v2 boolean, p_status character varying) RETURNS int AS
$$
DECLARE
    existing_timestamp timestamptz;
BEGIN

    -- Check if road segment already exists
    SELECT timestamp INTO existing_timestamp
    FROM projections.networktopology_roadsegments
    WHERE id = p_id;

    IF FOUND THEN
        RAISE EXCEPTION 'Road segment % already exists in topology projection (existing timestamp: %, new timestamp: %)',
            p_id, existing_timestamp, p_timestamp
            USING ERRCODE = '23505';
    END IF;

    INSERT INTO projections.networktopology_roadsegments (id, geometry, start_node_id, end_node_id, timestamp, is_v2, status)
    VALUES (p_id, ST_GeomFromText(p_wkt, p_srid), p_start_node_id, p_end_node_id, p_timestamp, p_is_v2, NULLIF(p_status, ''));

    RETURN 1;

END;
$$ LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS projections.networktopology_update_roadsegment(p_id integer, p_timestamp timestamptz, p_wkt character varying, p_srid integer, p_start_node_id integer, p_end_node_id integer, p_is_v2 boolean) cascade;

CREATE OR REPLACE FUNCTION projections.networktopology_update_roadsegment(p_id integer, p_timestamp timestamptz, p_wkt character varying, p_srid integer, p_start_node_id integer, p_end_node_id integer, p_is_v2 boolean, p_status character varying, p_is_removed boolean) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE projections.networktopology_roadsegments
    SET geometry = (CASE WHEN p_wkt <> '' THEN ST_GeomFromText(p_wkt, p_srid) ELSE geometry END),
        start_node_id = (CASE WHEN p_start_node_id IS NOT DISTINCT FROM -1 THEN start_node_id ELSE p_start_node_id END),
        end_node_id = (CASE WHEN p_end_node_id IS NOT DISTINCT FROM -1 THEN end_node_id ELSE p_end_node_id END),
        is_v2 = p_is_v2,
        status = (CASE WHEN p_status <> '' THEN p_status ELSE status END),
        is_removed = coalesce(p_is_removed, is_removed),
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

-- A V2 removal marks the road segment instead of deleting it.

CREATE OR REPLACE FUNCTION projections.networktopology_remove_roadsegment(p_id integer, p_timestamp timestamptz) RETURNS int AS
$$
DECLARE
    updated int;
BEGIN

    UPDATE projections.networktopology_roadsegments
    SET is_removed = TRUE,
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
