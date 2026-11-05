-- MartenMigrationProjection.RemoveRoadNode loaded the aggregate, applied RoadNodeWasRemoved to it and never handed
-- it back to the session, so the snapshot document was left saying isRemoved: false for every road node a V1
-- RoadNodeRemoved took out. Its road segment counterpart does call session.Store, which is why only road nodes
-- drifted. The omission is fixed in the same change as this migration; this repairs the documents it already wrote.
--
-- It matters beyond a stale flag: since the road network topology is looked up in these documents rather than in the
-- networktopology_* projection (1a813da62), every one of those road nodes reads as still existing.
--
-- WHICH ROAD NODES. Not "has a removal event somewhere in its stream" - a road node can be removed and then added,
-- migrated or have its removal corrected again, and by now many have been. The last event in the stream that moves
-- existence either way is the one that says where the node stands now, so that is what is asked for. A node whose
-- stream ends on a create is left alone, which is the whole guard against touching anything that migrated since.
--
-- The event list is the one RoadRegistry.Pbs.Compare.Console keeps (its Existences), and --eventtypes prints what
-- the store actually holds, which is how that list is checked against reality.
--
-- isRemovedBecauseOfMigration is written alongside it, because the two belong together: RoadNode.Apply sets it true
-- only for RoadNodeWasRemovedBecauseOfMigration and false for the other two removals. In practice only the V1
-- RoadNodeRemoved rows are unmarked here - the V2 path stores what it applies - but writing both keeps any row this
-- touches consistent with what replaying its stream would give.
--
-- One direction only: nothing is cleared. A document saying isRemoved while its stream ends on a create would be a
-- different fault with a different cause, there were none on tst, and rewriting every other row to say what reading
-- it already yields would rewrite the whole table for nothing.
--
-- Re-running it changes nothing: the rows it would write are exactly the ones that do not already say isRemoved.
--
-- This rewrites a few hundred thousand documents and recomputes the columns generated from them, so it is not quick.

UPDATE eventstore.mt_doc_roadnode doc
SET data = jsonb_set(
        jsonb_set(doc.data, '{isRemoved}', 'true'::jsonb, TRUE),
        '{isRemovedBecauseOfMigration}',
        to_jsonb(last_existence.type = 'road_node_was_removed_because_of_migration'),
        TRUE)
FROM (
    SELECT DISTINCT ON (e.stream_id) e.stream_id, e.type
    FROM eventstore.mt_events e
    WHERE e.type IN (
        -- creates
        'imported_road_node',
        'road_node_added',
        'road_node_was_added',
        'road_node_was_migrated',
        'road_node_removal_was_corrected_because_of_migration',
        -- removals
        'road_node_removed',
        'road_node_was_removed',
        'road_node_was_removed_because_of_migration'
    )
    ORDER BY e.stream_id, e.version DESC
) last_existence
WHERE last_existence.stream_id = doc.id
  AND last_existence.type IN (
      'road_node_removed',
      'road_node_was_removed',
      'road_node_was_removed_because_of_migration'
  )
  AND NOT coalesce((doc.data ->> 'isRemoved')::boolean, FALSE);
