-- A road segment retired by a merger or a split (road_segment_was_retired_because_of_merger / _split) is historized, not
-- removed: the aggregate keeps it with status gehistoreerd and without road nodes. The topology projection deleted it
-- instead, so a historized segment that came out of a merger or a split went missing from every topology lookup, while
-- a segment historized from realized stays in with its geometry and NULL road nodes. The projection now treats both the
-- same; this puts back the rows it deleted.
--
-- The row is rebuilt from the aggregate document, which is the state after every event of the stream, and stamped with
-- the timestamp of the stream's last event so the next event on the segment updates it as usual. Segments the aggregate
-- has removed since, and segments that already have a row, are left alone, so running it again changes nothing.

INSERT INTO projections.networktopology_roadsegments (id, geometry, start_node_id, end_node_id, timestamp, is_v2)
SELECT (doc.data ->> 'roadSegmentId')::int,
       public.ST_GeomFromText(doc.data -> 'geometry' ->> 'wkt', (doc.data -> 'geometry' ->> 'srid')::int),
       (doc.data ->> 'startNodeId')::int,
       (doc.data ->> 'endNodeId')::int,
       stream.last_timestamp,
       TRUE
FROM (
    SELECT e.stream_id, max(e.timestamp) AS last_timestamp
    FROM eventstore.mt_events e
    WHERE e.stream_id IN (
        SELECT retired.stream_id
        FROM eventstore.mt_events retired
        WHERE retired.type IN ('road_segment_was_retired_because_of_merger', 'road_segment_was_retired_because_of_split')
    )
    GROUP BY e.stream_id
) stream
JOIN eventstore.mt_doc_roadsegment doc ON doc.id = stream.stream_id
WHERE NOT coalesce((doc.data ->> 'isRemoved')::boolean, false)
  AND doc.data -> 'geometry' ->> 'wkt' IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM projections.networktopology_roadsegments rs
      WHERE rs.id = (doc.data ->> 'roadSegmentId')::int
  );
