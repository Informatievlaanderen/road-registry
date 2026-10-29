-- The road node aggregate now records which of the two removals put it where it is: IsRemovedBecauseOfMigration. Only
-- an inwinning's removal of a road node may be undone again (RoadNodeRemovalWasCorrectedBecauseOfMigration), and a
-- road node removed by an editing action or merged away is removed for good.
--
-- The snapshot documents written before this have no such key, and Newtonsoft hands the constructor the default for a
-- key that is not there - false. That reads as "removed, but not by an inwinning" for every road node an inwinning
-- did remove, which is exactly the set the correction is about, so those have to be written out.
--
-- Which removal it was is decided by the last one in the stream: a road node can be removed, added again and removed
-- a second time, and it is the most recent removal that says where it stands now. Nothing is written for a road node
-- that is not removed, or whose last removal was not an inwinning's: false is what reading its document already
-- yields, and rewriting every other document to say so would rewrite the whole table for nothing.
--
-- Re-running it changes nothing (the key is only written where it is not already true).

UPDATE eventstore.mt_doc_roadnode doc
SET data = jsonb_set(doc.data, '{isRemovedBecauseOfMigration}', 'true'::jsonb, TRUE)
FROM (
    SELECT DISTINCT ON (e.stream_id) e.stream_id, e.type
    FROM eventstore.mt_events e
    WHERE e.type IN ('road_node_was_removed', 'road_node_was_removed_because_of_migration')
    ORDER BY e.stream_id, e.version DESC
) last_removal
WHERE last_removal.stream_id = doc.id
  AND last_removal.type = 'road_node_was_removed_because_of_migration'
  AND coalesce((doc.data ->> 'isRemoved')::boolean, FALSE)
  AND NOT coalesce((doc.data ->> 'isRemovedBecauseOfMigration')::boolean, FALSE);
