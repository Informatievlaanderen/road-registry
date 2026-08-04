-- Same repair as 20260803120000, now for the read projection documents.
--
-- That migration realigned the trailing dynamic-attribute position on the aggregate snapshot
-- (eventstore.mt_doc_roadsegment). The read models keep their own copy of those positions and were left untouched,
-- so a segment that came out of a split still shows the stale trailing position for every attribute the user has not
-- edited since - road segment 818746 reports its changed wegcategorie at 76.88 while the rest still end at 76.89.
--
-- Shape differs from the snapshot: a read attribute is an object with a "values" array rather than a bare array, and
-- the geometry is stored per projection, so the length comes from geometry -> lambert08 (the same Lambert08 geometry
-- the domain measures).
--
-- Scoped, like the snapshot migration, to segments produced by a road_segment_was_split event since the split was
-- introduced on 2026-07-06 - nothing else can carry this drift. Only the coverage(s) ending at the highest ToPosition
-- move, and only onto the geometry length.

CREATE OR REPLACE FUNCTION projections.snap_trailing_read_attribute_position(attribute jsonb, segment_length numeric) RETURNS jsonb AS
$$
DECLARE
    attribute_values jsonb;
    trailing_to numeric;
BEGIN

    -- Nested on purpose: AND/OR are not guaranteed to short-circuit, and jsonb_array_length errors on a scalar.
    IF attribute IS NULL THEN
        RETURN attribute;
    END IF;
    IF jsonb_typeof(attribute) <> 'object' THEN
        RETURN attribute;
    END IF;

    attribute_values := attribute -> 'values';

    IF attribute_values IS NULL THEN
        RETURN attribute;
    END IF;
    IF jsonb_typeof(attribute_values) <> 'array' THEN
        RETURN attribute;
    END IF;
    IF jsonb_array_length(attribute_values) = 0 THEN
        RETURN attribute;
    END IF;

    SELECT max((value ->> 'to')::numeric) INTO trailing_to
    FROM jsonb_array_elements(attribute_values) AS value;

    IF trailing_to IS NULL OR trailing_to = segment_length THEN
        RETURN attribute;
    END IF;

    -- Never let the snap invert a coverage: a trailing range that starts at or after the new end is left alone so the
    -- inconsistency stays visible instead of being replaced by a nonsensical one.
    IF EXISTS (
        SELECT 1
        FROM jsonb_array_elements(attribute_values) AS value
        WHERE (value ->> 'to')::numeric = trailing_to
          AND (value ->> 'from')::numeric >= segment_length
    ) THEN
        RETURN attribute;
    END IF;

    RETURN jsonb_set(attribute, '{values}', (
        SELECT jsonb_agg(
                   CASE
                       WHEN (value ->> 'to')::numeric = trailing_to
                           THEN jsonb_set(value, '{to}', to_jsonb(segment_length))
                       ELSE value
                   END
                   ORDER BY ordinality)
        FROM jsonb_array_elements(attribute_values) WITH ORDINALITY AS t(value, ordinality)
    ));

END;
$$ LANGUAGE plpgsql IMMUTABLE;

WITH split_road_segment_ids AS (
    SELECT DISTINCT (jsonb_array_elements_text(e.data -> 'newRoadSegmentIds'))::int AS road_segment_id
    FROM eventstore.mt_events e
    WHERE e.type = 'road_segment_was_split'
      AND e.timestamp >= TIMESTAMPTZ '2026-07-06'
      AND jsonb_typeof(e.data -> 'newRoadSegmentIds') = 'array'
),
inconsistent AS (
    SELECT doc.id,
           round(public.ST_Length(public.ST_GeomFromText(doc.data -> 'geometry' -> 'lambert08' ->> 'wkt', (doc.data -> 'geometry' -> 'lambert08' ->> 'srid')::int))::numeric, 2) AS geometry_length
    FROM projections.mt_doc_read_roadsegments doc
    JOIN split_road_segment_ids split ON split.road_segment_id = doc.id
    WHERE doc.data -> 'geometry' -> 'lambert08' ->> 'wkt' IS NOT NULL
)
UPDATE projections.mt_doc_read_roadsegments doc
SET data = doc.data
    || jsonb_build_object(
        'accessRestriction', projections.snap_trailing_read_attribute_position(doc.data -> 'accessRestriction', inconsistent.geometry_length),
        'category', projections.snap_trailing_read_attribute_position(doc.data -> 'category', inconsistent.geometry_length),
        'morphology', projections.snap_trailing_read_attribute_position(doc.data -> 'morphology', inconsistent.geometry_length),
        'streetNameId', projections.snap_trailing_read_attribute_position(doc.data -> 'streetNameId', inconsistent.geometry_length),
        'maintenanceAuthorityId', projections.snap_trailing_read_attribute_position(doc.data -> 'maintenanceAuthorityId', inconsistent.geometry_length),
        'surfaceType', projections.snap_trailing_read_attribute_position(doc.data -> 'surfaceType', inconsistent.geometry_length),
        'carTrafficDirection', projections.snap_trailing_read_attribute_position(doc.data -> 'carTrafficDirection', inconsistent.geometry_length),
        'bikeTrafficDirection', projections.snap_trailing_read_attribute_position(doc.data -> 'bikeTrafficDirection', inconsistent.geometry_length),
        'pedestrianTrafficDirection', projections.snap_trailing_read_attribute_position(doc.data -> 'pedestrianTrafficDirection', inconsistent.geometry_length)
    )
FROM inconsistent
WHERE inconsistent.id = doc.id
  AND EXISTS (
      SELECT 1
      FROM jsonb_each(doc.data) AS attribute(key, attribute_value)
      WHERE attribute.key IN ('accessRestriction', 'category', 'morphology', 'streetNameId', 'maintenanceAuthorityId',
                              'surfaceType', 'carTrafficDirection', 'bikeTrafficDirection', 'pedestrianTrafficDirection')
        AND CASE
                WHEN jsonb_typeof(attribute.attribute_value -> 'values') = 'array'
                    THEN CASE
                             WHEN jsonb_array_length(attribute.attribute_value -> 'values') > 0
                                 THEN (SELECT max((value ->> 'to')::numeric)
                                       FROM jsonb_array_elements(attribute.attribute_value -> 'values') AS value) <> inconsistent.geometry_length
                             ELSE false
                         END
                ELSE false
            END
  );

DROP FUNCTION IF EXISTS projections.snap_trailing_read_attribute_position(jsonb, numeric);
