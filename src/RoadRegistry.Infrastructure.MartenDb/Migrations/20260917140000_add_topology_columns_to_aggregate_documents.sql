-- The road network topology lookups (RoadNetworkRepository.GetUnderlyingIds*) query the aggregate documents directly
-- instead of the networktopology_* tables of RoadNetworkTopologyProjection. That projection translated every event a
-- second time into the same state the aggregates already keep, and drifted from them: a road segment retired by a
-- merger or a split is historized on the aggregate but was deleted from the topology.
--
-- The columns the lookups need are generated from the document JSON, so Postgres keeps them in step with every upsert
-- Marten does - nothing on the Marten side writes or knows about them. Adding a stored generated column rewrites the
-- table, so the tables are locked for the duration of this migration.
--
-- The networktopology_* tables and functions are left in place for now: hosts still running the previous release
-- project into them until they are deployed. A later migration drops them.

ALTER TABLE eventstore.mt_doc_roadsegment
    ADD COLUMN road_segment_id int
        GENERATED ALWAYS AS ((data ->> 'roadSegmentId')::int) STORED,
    ADD COLUMN geometry public.geometry
        GENERATED ALWAYS AS (public.ST_GeomFromText(NULLIF(data -> 'geometry' ->> 'wkt', ''), (data -> 'geometry' ->> 'srid')::int)) STORED,
    ADD COLUMN start_node_id int
        GENERATED ALWAYS AS ((data ->> 'startNodeId')::int) STORED,
    ADD COLUMN end_node_id int
        GENERATED ALWAYS AS ((data ->> 'endNodeId')::int) STORED,
    ADD COLUMN is_removed boolean
        GENERATED ALWAYS AS (coalesce((data ->> 'isRemoved')::boolean, false)) STORED,
    -- RoadSegment.HasMigrated: a segment carried over from the legacy road network has no attributes until it is migrated.
    ADD COLUMN has_migrated boolean
        GENERATED ALWAYS AS (jsonb_typeof(data -> 'attributes') = 'object') STORED;

-- The lookups only ever look at segments that are not removed.
CREATE INDEX ix_mt_doc_roadsegment_road_segment_id ON eventstore.mt_doc_roadsegment (road_segment_id) WHERE NOT is_removed;
CREATE INDEX ix_mt_doc_roadsegment_geometry ON eventstore.mt_doc_roadsegment USING gist (geometry) WHERE NOT is_removed;
CREATE INDEX ix_mt_doc_roadsegment_start_node_id ON eventstore.mt_doc_roadsegment (start_node_id) WHERE NOT is_removed;
CREATE INDEX ix_mt_doc_roadsegment_end_node_id ON eventstore.mt_doc_roadsegment (end_node_id) WHERE NOT is_removed;

ALTER TABLE eventstore.mt_doc_gradeseparatedjunction
    ADD COLUMN grade_separated_junction_id int
        GENERATED ALWAYS AS ((data ->> 'gradeSeparatedJunctionId')::int) STORED,
    ADD COLUMN lower_road_segment_id int
        GENERATED ALWAYS AS ((data ->> 'lowerRoadSegmentId')::int) STORED,
    ADD COLUMN upper_road_segment_id int
        GENERATED ALWAYS AS ((data ->> 'upperRoadSegmentId')::int) STORED,
    ADD COLUMN is_removed boolean
        GENERATED ALWAYS AS (coalesce((data ->> 'isRemoved')::boolean, false)) STORED,
    -- A junction carried over from the legacy road network has no type until it is migrated.
    ADD COLUMN has_migrated boolean
        GENERATED ALWAYS AS (jsonb_typeof(data -> 'type') = 'string') STORED;

CREATE INDEX ix_mt_doc_gradeseparatedjunction_grade_separated_junction_id ON eventstore.mt_doc_gradeseparatedjunction (grade_separated_junction_id) WHERE NOT is_removed;
CREATE INDEX ix_mt_doc_gradeseparatedjunction_lower_road_segment_id ON eventstore.mt_doc_gradeseparatedjunction (lower_road_segment_id) WHERE NOT is_removed;
CREATE INDEX ix_mt_doc_gradeseparatedjunction_upper_road_segment_id ON eventstore.mt_doc_gradeseparatedjunction (upper_road_segment_id) WHERE NOT is_removed;

-- Grade junctions only exist in the V2 road network, so there is nothing to migrate.
ALTER TABLE eventstore.mt_doc_gradejunction
    ADD COLUMN grade_junction_id int
        GENERATED ALWAYS AS ((data ->> 'gradeJunctionId')::int) STORED,
    ADD COLUMN road_segment_id_1 int
        GENERATED ALWAYS AS ((data ->> 'roadSegmentId1')::int) STORED,
    ADD COLUMN road_segment_id_2 int
        GENERATED ALWAYS AS ((data ->> 'roadSegmentId2')::int) STORED,
    ADD COLUMN is_removed boolean
        GENERATED ALWAYS AS (coalesce((data ->> 'isRemoved')::boolean, false)) STORED;

CREATE INDEX ix_mt_doc_gradejunction_grade_junction_id ON eventstore.mt_doc_gradejunction (grade_junction_id) WHERE NOT is_removed;
CREATE INDEX ix_mt_doc_gradejunction_road_segment_id_1 ON eventstore.mt_doc_gradejunction (road_segment_id_1) WHERE NOT is_removed;
CREATE INDEX ix_mt_doc_gradejunction_road_segment_id_2 ON eventstore.mt_doc_gradejunction (road_segment_id_2) WHERE NOT is_removed;
