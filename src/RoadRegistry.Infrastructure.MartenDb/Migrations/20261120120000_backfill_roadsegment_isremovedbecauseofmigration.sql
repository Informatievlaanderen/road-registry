-- The counterpart of 20261023120000_backfill_roadnode_isremovedbecauseofmigration, for the road segment. The road
-- segment aggregate now records which of the removals put it where it is: IsRemovedBecauseOfMigration. Only an
-- inwinning's removal of a road segment may be undone again (RoadSegmentRemovalWasCorrectedBecauseOfMigration), and a
-- road segment removed by an editing action is removed for good.
--
-- The snapshot documents written before this have no such key, and Newtonsoft hands the constructor the default for a
-- key that is not there - false. That reads as "removed, but not by an inwinning" for every road segment an inwinning
-- did remove, which is exactly the set the correction is about: the road segments a neighbouring municipality removed
-- across the border, and that the other one still has to collect. Without this they would stay refused with
-- "Het wegsegment is verwijderd".
--
-- Which removal it was is decided by the last one in the stream: a road segment can be removed, added again and
-- removed a second time, and it is the most recent removal that says where it stands now. The three below are the
-- removals that mark a road segment removed; outlined_road_segment_removed is deliberately not among them - it says a
-- road segment stopped being outlined and carried on as measured, which is what RepairRoadSegmentSnapshots is about,
-- and a road segment whose last real removal was an inwinning's is removed by that one either way.
--
-- Nothing is written for a road segment that is not removed, or whose last removal was not an inwinning's: false is
-- what reading its document already yields, and rewriting every other document to say so would rewrite the whole
-- table for nothing.
--
-- Re-running it changes nothing (the key is only written where it is not already true).

UPDATE eventstore.mt_doc_roadsegment doc
SET data = jsonb_set(doc.data, '{isRemovedBecauseOfMigration}', 'true'::jsonb, TRUE)
FROM (
    SELECT DISTINCT ON (e.stream_id) e.stream_id, e.type
    FROM eventstore.mt_events e
    WHERE e.type IN ('road_segment_removed', 'road_segment_was_removed', 'road_segment_was_removed_because_of_migration')
    ORDER BY e.stream_id, e.version DESC
) last_removal
WHERE last_removal.stream_id = doc.id
  AND last_removal.type = 'road_segment_was_removed_because_of_migration'
  AND coalesce((doc.data ->> 'isRemoved')::boolean, FALSE)
  AND NOT coalesce((doc.data ->> 'isRemovedBecauseOfMigration')::boolean, FALSE);
