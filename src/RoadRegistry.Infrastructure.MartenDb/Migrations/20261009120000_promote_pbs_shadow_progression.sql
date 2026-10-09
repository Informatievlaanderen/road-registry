-- The Marten half of promoting the PBS shadow read model (SQL Server migration PromotePbsShadow).
--
-- The shadow was a second copy of the projection, under a name of its own, replaying the whole event stream into the
-- 'RoadRegistryPbsTemp' schema while the live one kept serving 'RoadRegistryPbs'. The other migration moves those
-- tables into the live schema; what is left is the daemon's own bookkeeping, which lives here and is keyed by
-- projection name, so it does not travel with them.
--
-- The live shard is therefore pointed at the position the shadow reached. Unconditionally, in both directions: the
-- rows now in 'RoadRegistryPbs' are the ones the shadow wrote, so its position is what describes them. Ahead of it
-- would skip events that were never applied to these tables; behind it only replays events they already have, which
-- the projection-state row on the SQL Server side then skips.
--
-- Everything the shadow owned goes with it: its progression and the desired-state document the projections page kept
-- for it. There is no shard by that name any more.
--
-- A no-op where the shadow never ran - there is no row to copy from, and nothing to delete. That is the case in
-- production, where PBS has not been enabled at all.
--
-- The same swap as PromoteWmsWfsV2Shadow, and the last one: PBS was the only read model still being rebuilt beside
-- itself, so the shadow scaffolding goes with this change.

INSERT INTO eventstore.mt_event_progression (name, last_seq_id, last_updated)
SELECT 'RoadNetworkChangesPbsProjection:All', last_seq_id, transaction_timestamp()
FROM eventstore.mt_event_progression
WHERE name = 'RoadNetworkChangesPbsTempProjection:All'
ON CONFLICT ON CONSTRAINT pk_mt_event_progression
DO UPDATE SET last_seq_id = EXCLUDED.last_seq_id, last_updated = transaction_timestamp();

DELETE FROM eventstore.mt_event_progression
WHERE name = 'RoadNetworkChangesPbsTempProjection:All';

DELETE FROM eventstore.mt_doc_martenprojection_state
WHERE id = 'RoadNetworkChangesPbsTempProjection:All';
