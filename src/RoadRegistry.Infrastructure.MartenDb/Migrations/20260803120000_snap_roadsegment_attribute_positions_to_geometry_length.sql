-- Realigns the trailing dynamic-attribute position of split road segment snapshots with their own geometry length.
--
-- Splitting derived the new positions from measures along the original line (SplitAt(cutPositionMeasure,
-- totalLength)) and only afterwards rounded the two part geometries to the centimetre. Rounding moves the
-- interpolated cut vertex, so a part could measure a different centimetre than the measure its positions came from.
-- Road segment 818746 is the reported case: migrated on 2026-07-27 at 85.51529 -> 85.52 with positions of 85.52
-- (consistent), split an hour later into a part whose geometry measures 76.88342 -> 76.88 while every attribute ends
-- at 76.89.
--
-- Since 7ad83ae79 the trailing-position check is exact rather than tolerant (it used IsReasonablyEqualTo with a 1cm
-- tolerance before), so such a segment now fails on any change with a ToPositionNotEqualToLength error per attribute
-- - including the attributes the caller never touched, because RoadSegment.Modify carries those over and validates
-- them all.
--
-- The source of the drift is fixed in the same change as this script: both split paths now take each part's trailing
-- position from its own rounded geometry. This repairs what the old code already wrote.
--
-- Scope: eventstore.mt_doc_roadsegment, the aggregate snapshot, which is what RoadNetworkRepository.Load reads
-- (session.LoadManyAsync, not an event replay). Restricted to segments produced by a road_segment_was_split event
-- since 2026-07-06, the date the split was introduced - nothing else can carry this drift. The events themselves are
-- left untouched: they are the historical record of what was decided at the time. A snapshot rebuild from events
-- would reintroduce the drift, so if one is ever run, this migration has to run after it.
--
-- Only the coverage(s) ending at the highest ToPosition are moved, and only onto the geometry length. Anything else
-- about the attribute segmentation is left exactly as it is.

CREATE OR REPLACE FUNCTION eventstore.snap_trailing_attribute_position(attribute_values jsonb, segment_length numeric) RETURNS jsonb AS
$$
DECLARE
    trailing_to numeric;
BEGIN

    -- Nested on purpose: AND/OR are not guaranteed to short-circuit, and jsonb_array_length errors on a scalar.
    IF attribute_values IS NULL THEN
        RETURN attribute_values;
    END IF;
    IF jsonb_typeof(attribute_values) <> 'array' THEN
        RETURN attribute_values;
    END IF;
    IF jsonb_array_length(attribute_values) = 0 THEN
        RETURN attribute_values;
    END IF;

    SELECT max((value -> 'coverage' ->> 'to')::numeric) INTO trailing_to
    FROM jsonb_array_elements(attribute_values) AS value;

    IF trailing_to IS NULL OR trailing_to = segment_length THEN
        RETURN attribute_values;
    END IF;

    -- Never let the snap invert a coverage: a trailing range that starts at or after the new end is left alone so the
    -- inconsistency stays visible instead of being replaced by a nonsensical one.
    IF EXISTS (
        SELECT 1
        FROM jsonb_array_elements(attribute_values) AS value
        WHERE (value -> 'coverage' ->> 'to')::numeric = trailing_to
          AND (value -> 'coverage' ->> 'from')::numeric >= segment_length
    ) THEN
        RETURN attribute_values;
    END IF;

    RETURN (
        SELECT jsonb_agg(
                   CASE
                       WHEN (value -> 'coverage' ->> 'to')::numeric = trailing_to
                           THEN jsonb_set(value, '{coverage,to}', to_jsonb(segment_length))
                       ELSE value
                   END
                   ORDER BY ordinality)
        FROM jsonb_array_elements(attribute_values) WITH ORDINALITY AS t(value, ordinality)
    );

END;
$$ LANGUAGE plpgsql IMMUTABLE;

WITH split_road_segment_ids AS (
    -- Only segments that came out of a split can carry the drift, and only since the split was introduced.
    SELECT DISTINCT (jsonb_array_elements_text(e.data -> 'newRoadSegmentIds'))::int AS road_segment_id
    FROM eventstore.mt_events e
    WHERE e.type = 'road_segment_was_split'
      AND e.timestamp >= TIMESTAMPTZ '2026-07-06'
      AND jsonb_typeof(e.data -> 'newRoadSegmentIds') = 'array'
),
inconsistent AS (
    SELECT doc.id,
           round(public.ST_Length(public.ST_GeomFromText(doc.data -> 'geometry' ->> 'wkt', (doc.data -> 'geometry' ->> 'srid')::int))::numeric, 2) AS geometry_length
    FROM eventstore.mt_doc_roadsegment doc
    JOIN split_road_segment_ids split ON split.road_segment_id = (doc.data ->> 'roadSegmentId')::int
    WHERE jsonb_typeof(doc.data -> 'attributes') = 'object'
      AND doc.data -> 'geometry' ->> 'wkt' IS NOT NULL
)
UPDATE eventstore.mt_doc_roadsegment doc
SET data = jsonb_set(doc.data, '{attributes}',
        doc.data -> 'attributes'
        || jsonb_build_object(
            'accessRestriction', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'accessRestriction', inconsistent.geometry_length),
            'category', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'category', inconsistent.geometry_length),
            'morphology', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'morphology', inconsistent.geometry_length),
            'streetNameId', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'streetNameId', inconsistent.geometry_length),
            'maintenanceAuthorityId', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'maintenanceAuthorityId', inconsistent.geometry_length),
            'surfaceType', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'surfaceType', inconsistent.geometry_length),
            'carTrafficDirection', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'carTrafficDirection', inconsistent.geometry_length),
            'bikeTrafficDirection', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'bikeTrafficDirection', inconsistent.geometry_length),
            'pedestrianTrafficDirection', eventstore.snap_trailing_attribute_position(doc.data -> 'attributes' -> 'pedestrianTrafficDirection', inconsistent.geometry_length)
        ))
FROM inconsistent
WHERE inconsistent.id = doc.id
  -- Only touch the segments that are actually inconsistent. The CASE nesting is deliberate: 'attributes' also holds
  -- scalars such as geometryDrawMethod, jsonb_array_length errors on those, and AND does not guarantee that the
  -- jsonb_typeof check is evaluated first.
  AND EXISTS (
      SELECT 1
      FROM jsonb_each(doc.data -> 'attributes') AS attribute(key, attribute_values)
      WHERE CASE
                WHEN jsonb_typeof(attribute.attribute_values) = 'array'
                    THEN CASE
                             WHEN jsonb_array_length(attribute.attribute_values) > 0
                                 THEN (SELECT max((value -> 'coverage' ->> 'to')::numeric)
                                       FROM jsonb_array_elements(attribute.attribute_values) AS value) <> inconsistent.geometry_length
                             ELSE false
                         END
                ELSE false
            END
  );

DROP FUNCTION IF EXISTS eventstore.snap_trailing_attribute_position(jsonb, numeric);
