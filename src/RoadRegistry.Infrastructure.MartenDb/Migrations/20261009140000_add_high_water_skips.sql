-- The two objects Marten needs to record that it skipped a gap in the event sequence, and that our schema has never
-- had.
--
-- When the high water detector decides a gap is permanent it does not move the mark with mt_mark_event_progression;
-- it calls mt_mark_progression_with_skip, which moves the mark and writes the skipped range to mt_high_water_skips in
-- the same statement. Neither exists here, so that call fails: the mark does not move and the skip leaves no trace.
--
-- Failing is, by accident, the behaviour we want - a gap is never silently skipped - but it fails blind. There is no
-- record of which sequences were involved, and the only sign is an error the daemon retries. When 2004 road segments
-- went missing on 2026-10-08 this was the first place we looked, and the table was not there to answer.
--
-- The table definition is Marten's own: taken from Marten.Events.Schema.EventProgressionSkippingTable as it stands in
-- 8.13.0, and the function body from the embedded resource Marten.Schema.SQL.mt_mark_progression_with_skip.sql, with
-- {databaseSchema} resolved to our event store schema. Both are written exactly as Marten would create them, because
-- Marten is what reads and writes them.
--
-- Why a hand-written migration: the migration generator diffs the Marten model against the database, and these two
-- are not in the model it sees - a delta generated against a database that already has all our migrations did not
-- emit them. The same blind spot cuts the other way, so when regenerating migrations, check the delta for a DROP of
-- mt_high_water_skips and strip it out, the same hand-edit ix_mt_events_correlation_seq already needs.

CREATE TABLE IF NOT EXISTS eventstore.mt_high_water_skips (
    ending_sequence      bigint                      NOT NULL,
    starting_sequence    bigint                      NOT NULL,
    timestamp            timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
CONSTRAINT pkey_mt_high_water_skips_ending_sequence PRIMARY KEY (ending_sequence)
);

CREATE
OR REPLACE FUNCTION eventstore.mt_mark_progression_with_skip(shard_name varchar, ending_sequence bigint, starting_sequence bigint) RETURNS bigint AS
$$
DECLARE
    current_value bigint;
BEGIN
    select last_seq_id into current_value from eventstore.mt_event_progression where name = shard_name;

    IF current_value is null then
        return 0;
    ELSIF current_value = starting_sequence THEN
        update eventstore.mt_event_progression SET last_seq_id = ending_sequence, last_updated = transaction_timestamp() where shard_name = name;
        insert into eventstore.mt_high_water_skips (ending_sequence, starting_sequence) values (ending_sequence, starting_sequence);
        return ending_sequence;
    ELSE
        return current_value;
    END IF;
END
$$
LANGUAGE plpgsql;
