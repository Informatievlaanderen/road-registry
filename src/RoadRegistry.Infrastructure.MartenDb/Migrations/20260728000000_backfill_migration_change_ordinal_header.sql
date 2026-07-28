-- The read projection (RoadNetworkChangesProjection) orders each correlation's events by the emission ordinal held
-- in the "roadNetworkChangeOrdinal" event header (see EventOrdinal / IEventOrdinalProvider). Events produced by
-- MartenMigrationProjection before that header existed can recover their ordinal from the causation_id, which was
-- built as:
--     migration-<eventName>-MartenMigrationProjection-<position>-<changeIndex>
-- The trailing <changeIndex> is the change's index within its RoadNetworkChangesAccepted batch, which is exactly the
-- ordinal the migration now stamps at write time. Only migration events that actually carry a changeIndex (two
-- trailing numeric segments) are backfilled; the Imported*/organization/street-name migration sessions have no
-- changeIndex (a single trailing number) and are deliberately left untouched. The header value is written as a JSON
-- number to match the runtime SetHeader(key, long). Guarded so a re-run never overwrites an existing header.
DO $do$
BEGIN
    IF to_regclass('eventstore.mt_events') IS NOT NULL THEN
        UPDATE eventstore.mt_events
        SET headers = coalesce(headers, '{}'::jsonb)
                      || jsonb_build_object('roadNetworkChangeOrdinal', (substring(causation_id from '-([0-9]+)$'))::bigint)
        WHERE causation_id ~ 'MartenMigrationProjection-[0-9]+-[0-9]+$'
          AND NOT (coalesce(headers, '{}'::jsonb) ? 'roadNetworkChangeOrdinal');
    END IF;
END $do$;
