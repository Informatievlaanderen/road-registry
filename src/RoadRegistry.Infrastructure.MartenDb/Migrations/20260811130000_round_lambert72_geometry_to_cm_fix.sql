-- Re-applies the Lambert72 coordinate rounding that the previous migration
-- (20260811120000) failed to perform.
--
-- The earlier script used PascalCase JSON paths (e.g. 'Geometry', 'Lambert72') and assumed
-- a nested coordinate-array structure.  In reality, Marten serialises documents with camelCase
-- property names, and the geometry is stored as a WKT string under:
--
--   data -> 'geometry' -> 'lambert72' -> 'wkt'
--
-- Because the paths did not match, the previous UPDATE was a no-op: the sub-query that drove
-- jsonb_set returned NULL (iterating over a missing key), and jsonb_set leaves the document
-- unchanged when the replacement value is NULL.
--
-- This script performs the rounding correctly using PostGIS:
--   1. Parse the WKT string into a PostGIS geometry.
--   2. Snap all vertices to a 0.01-unit (1 cm) grid with ST_SnapToGrid.
--   3. Write the result back as a 2-decimal-place WKT string with ST_AsText(..., 2).

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
