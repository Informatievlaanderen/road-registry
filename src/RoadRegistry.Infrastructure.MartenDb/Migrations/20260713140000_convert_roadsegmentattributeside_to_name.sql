-- RoadSegmentAttributeSide is now serialized as its (Dutch) name (Beide / Links / Rechts). Earlier, when it was an enum,
-- it was serialized either as the numeric identifier (0 / 1 / 2) or as the (camelCase) enum name (both / left / right).
-- Rewrite both of those legacy forms in the event stream and in the projected read models. Beide (0) is normally omitted,
-- but is handled defensively. The "side" key only ever holds a road segment attribute side, so a plain text rewrite is
-- safe; for the numeric form the ([,}]) capture keeps the JSON structure intact and prevents partial matches (e.g. it
-- will never touch a hypothetical "side": 10). The enum-name form is matched case-insensitively (both/Both, ...).
DO $do$
DECLARE
    target text;
BEGIN
    FOREACH target IN ARRAY ARRAY[
        'eventstore.mt_events',
        'eventstore.mt_doc_roadsegment',
        'projections.mt_doc_read_roadsegments',
        'projections.mt_doc_extract_roadsegments'
    ] LOOP
        IF to_regclass(target) IS NOT NULL THEN
            EXECUTE format(
                $sql$
                    UPDATE %s
                    SET data = regexp_replace(
                                 regexp_replace(
                                   regexp_replace(
                                     regexp_replace(
                                       regexp_replace(
                                         regexp_replace(data::text, '"side":\s*0([,}])', '"side": "Beide"\1', 'g'),
                                       '"side":\s*1([,}])', '"side": "Links"\1', 'g'),
                                     '"side":\s*2([,}])', '"side": "Rechts"\1', 'g'),
                                   '"side":\s*"both"', '"side": "Beide"', 'gi'),
                                 '"side":\s*"left"', '"side": "Links"', 'gi'),
                               '"side":\s*"right"', '"side": "Rechts"', 'gi')::jsonb
                    WHERE data::text ~* '"side":\s*(0|1|2|"both"|"left"|"right")'
                $sql$, target);
        END IF;
    END LOOP;
END $do$;
