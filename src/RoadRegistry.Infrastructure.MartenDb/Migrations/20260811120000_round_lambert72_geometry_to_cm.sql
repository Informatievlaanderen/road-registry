-- Round the Lambert72 geometry coordinates in the read projections to cm (2 decimal places).
-- The read projections store Lambert72 as a nested JSONB object under the "Geometry" field.
--
-- Road nodes:   data -> 'Geometry' -> 'Lambert72' -> 'Point'  (X, Y are double)
-- Road segments: data -> 'Geometry' -> 'Lambert72' -> 'MultiLineString' -> array of line strings,
--                each containing 'Points' -> array of {X, Y}.
--
-- Both are updated in a single pass using jsonb path manipulation.
-- The update is guarded by a table-existence check so it is a safe no-op on fresh databases.

DO $do$
BEGIN
    -- Road nodes
    IF to_regclass('projections.mt_doc_read_roadnodes') IS NOT NULL THEN
        UPDATE projections.mt_doc_read_roadnodes
        SET data = jsonb_set(
            data,
            '{Geometry,Lambert72,Point}',
            jsonb_build_object(
                'X', round((data -> 'Geometry' -> 'Lambert72' -> 'Point' ->> 'X')::numeric, 2),
                'Y', round((data -> 'Geometry' -> 'Lambert72' -> 'Point' ->> 'Y')::numeric, 2)
            )
        )
        WHERE data -> 'Geometry' -> 'Lambert72' -> 'Point' IS NOT NULL;
    END IF;

    -- Road segments
    IF to_regclass('projections.mt_doc_read_roadsegments') IS NOT NULL THEN
        UPDATE projections.mt_doc_read_roadsegments
        SET data = jsonb_set(
            data,
            '{Geometry,Lambert72,MultiLineString}',
            (
                SELECT jsonb_agg(
                    jsonb_set(
                        line_string,
                        '{Points}',
                        (
                            SELECT jsonb_agg(
                                jsonb_build_object(
                                    'X', round((pt ->> 'X')::numeric, 2),
                                    'Y', round((pt ->> 'Y')::numeric, 2)
                                )
                            )
                            FROM jsonb_array_elements(line_string -> 'Points') AS pt
                        )
                    )
                )
                FROM jsonb_array_elements(data -> 'Geometry' -> 'Lambert72' -> 'MultiLineString') AS line_string
            )
        )
        WHERE data -> 'Geometry' -> 'Lambert72' -> 'MultiLineString' IS NOT NULL;
    END IF;
END $do$;
