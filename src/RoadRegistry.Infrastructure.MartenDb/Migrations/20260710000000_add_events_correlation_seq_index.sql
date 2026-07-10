-- Composite index used by RoadNetworkChangesProjection to fetch the tail of a correlation that a page cut in half
-- (WHERE correlation_id = ANY(...) AND seq_id > pageMax). Marten does not manage this index itself.
CREATE INDEX IF NOT EXISTS ix_mt_events_correlation_seq
    ON eventstore.mt_events (correlation_id, seq_id);
