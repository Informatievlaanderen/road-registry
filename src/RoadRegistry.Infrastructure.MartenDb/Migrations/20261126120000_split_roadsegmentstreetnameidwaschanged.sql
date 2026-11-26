-- RoadSegmentStreetNameIdWasChanged is split in two (GAWR-7784):
--
--   1. To 'niet van toepassing' (-9) it becomes RoadSegmentStreetNameWasUnlinked: the road segment let go of the street
--      name. Its streetNameId is the street name that was let go - the old oldStreetNameId - and the attribute values
--      the road segment ends up with move from streetNameId to streetNameIdAttributeValues.
--   2. To any other street name it becomes RoadSegmentStreetNameWasRelinked: oldStreetNameId and newStreetNameId stay,
--      and the attribute values move from streetNameId to streetNameIdAttributeValues.
--
-- An event keeps its sequence, its stream version, its correlation and its headers; only its type, its .NET type and
-- the shape of its data change. The read models already hold what these events say, so nothing needs replaying.
--
-- Re-running it changes nothing: both steps only touch events of the old type, which are gone after the first run.

DO $do$
BEGIN
    IF to_regclass('eventstore.mt_events') IS NULL THEN
        RETURN;
    END IF;

    -- 1. To 'niet van toepassing': unlinked.
    UPDATE eventstore.mt_events
    SET type = 'road_segment_street_name_was_unlinked',
        mt_dotnet_type = replace(mt_dotnet_type, '.RoadSegmentStreetNameIdWasChanged,', '.RoadSegmentStreetNameWasUnlinked,'),
        data = (data - 'oldStreetNameId' - 'newStreetNameId' - 'streetNameId')
               || jsonb_build_object(
                      'streetNameId', data -> 'oldStreetNameId',
                      'streetNameIdAttributeValues', data -> 'streetNameId')
    WHERE type = 'road_segment_street_name_id_was_changed'
      AND (data ->> 'newStreetNameId')::integer = -9;

    -- 2. To another street name: relinked.
    UPDATE eventstore.mt_events
    SET type = 'road_segment_street_name_was_relinked',
        mt_dotnet_type = replace(mt_dotnet_type, '.RoadSegmentStreetNameIdWasChanged,', '.RoadSegmentStreetNameWasRelinked,'),
        data = (data - 'streetNameId')
               || jsonb_build_object('streetNameIdAttributeValues', data -> 'streetNameId')
    WHERE type = 'road_segment_street_name_id_was_changed';
END $do$;
