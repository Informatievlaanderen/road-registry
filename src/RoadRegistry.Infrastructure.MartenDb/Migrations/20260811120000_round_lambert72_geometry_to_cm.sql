-- Round the Lambert72 geometry coordinates in the read projections to cm (2 decimal places).
--
-- Marten serialises documents with camelCase property names, and RoadNodeGeometry /
-- RoadSegmentGeometry both inherit from GeometryObject which stores the geometry as a WKT
-- string, not as a nested coordinate array.  The JSON structure is therefore:
--
--   data -> 'geometry' -> 'lambert72' -> 'wkt'   (WKT string, e.g. "POINT (x y)")
--   data -> 'geometry' -> 'lambert72' -> 'srid'  (integer SRID)
--
-- The rounding is done with PostGIS: parse the WKT into a geometry, snap every vertex to a
-- 0.01-unit (1 cm) grid, then write it back as WKT limited to 2 decimal places.
-- The update is guarded by a table-existence check so it is a safe no-op on fresh databases.

DO $do$
BEGIN
    -- Road nodes
    IF to_regclass('projections.mt_doc_read_roadnodes') IS NOT NULL THEN
        UPDATE projections.mt_doc_read_roadnodes
        SET data = jsonb_set(
            data,
            '{geometry,lambert72,wkt}',
            to_jsonb(ST_AsText(
                ST_SnapToGrid(
                    ST_SetSRID(ST_GeomFromText(
                        data -> 'geometry' -> 'lambert72' ->> 'wkt'
                    ), (data -> 'geometry' -> 'lambert72' ->> 'srid')::int),
                    0.01
                ),
                2
            ))
        )
        WHERE data -> 'geometry' -> 'lambert72' ->> 'wkt' IS NOT NULL;
    END IF;

    -- Road segments
    IF to_regclass('projections.mt_doc_read_roadsegments') IS NOT NULL THEN
        UPDATE projections.mt_doc_read_roadsegments
        SET data = jsonb_set(
            data,
            '{geometry,lambert72,wkt}',
            to_jsonb(ST_AsText(
                ST_SnapToGrid(
                    ST_SetSRID(ST_GeomFromText(
                        data -> 'geometry' -> 'lambert72' ->> 'wkt'
                    ), (data -> 'geometry' -> 'lambert72' ->> 'srid')::int),
                    0.01
                ),
                2
            ))
        )
        WHERE data -> 'geometry' -> 'lambert72' ->> 'wkt' IS NOT NULL;
    END IF;
END $do$;
