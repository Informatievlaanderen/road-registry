-- The Marten half of promoting the WmsWfsV2 shadow read model (SQL Server migration PromoteWmsWfsV2Shadow).
--
-- The shadow was a second copy of the projection, under a name of its own, replaying the whole event stream into the
-- 'roadTemp' schema while the live one kept serving 'road'. The other migration moves those tables into 'road'; what
-- is left is the daemon's own bookkeeping, which lives here and is keyed by projection name, so it does not travel
-- with them.
--
-- The live shard is therefore pointed at the position the shadow reached. Unconditionally, in both directions: the
-- rows now in 'road' are the ones the shadow wrote, so its position is what describes them. Ahead of it would skip
-- events that were never applied to these tables; behind it only replays events they already have, which the
-- projection-state row on the SQL Server side then skips.
--
-- Everything the shadow owned goes with it: its progression and the desired-state document the projections page kept
-- for it. There is no shard by that name any more.
--
-- A no-op where the shadow never ran - there is no row to copy from, and nothing to delete.

INSERT INTO eventstore.mt_event_progression (name, last_seq_id, last_updated)
SELECT 'RoadNetworkChangesWmsWfsV2Projection:All', last_seq_id, transaction_timestamp()
FROM eventstore.mt_event_progression
WHERE name = 'RoadNetworkChangesWmsWfsV2TempProjection:All'
ON CONFLICT ON CONSTRAINT pk_mt_event_progression
DO UPDATE SET last_seq_id = EXCLUDED.last_seq_id, last_updated = transaction_timestamp();

DELETE FROM eventstore.mt_event_progression
WHERE name = 'RoadNetworkChangesWmsWfsV2TempProjection:All';

DELETE FROM eventstore.mt_doc_martenprojection_state
WHERE id = 'RoadNetworkChangesWmsWfsV2TempProjection:All';
