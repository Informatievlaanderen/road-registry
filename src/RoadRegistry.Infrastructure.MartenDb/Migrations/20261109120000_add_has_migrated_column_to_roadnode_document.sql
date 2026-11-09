-- The road node documents get the has_migrated column the road segment and grade separated junction documents
-- already have (20261103120000_add_topology_columns_to_aggregate_documents). A road node carried over from the
-- legacy road network has no type until it is migrated, which is what RoadNode.HasMigrated() asks.
--
-- Nothing in the application reads it - the topology lookups only need the id and is_removed of a road node: it is
-- there to make debugging easier, so the wegknopen that still have collecting to do can be told apart straight from
-- the database, in QGIS next to the geometry column 20261106120000_add_geometry_column_to_roadnode_document put
-- there for the same reason.
--
-- The column is generated from the document JSON, so Postgres keeps it in step with every upsert Marten does -
-- nothing on the Marten side writes or knows about it. Adding a stored generated column rewrites the table, so
-- mt_doc_roadnode is locked for the duration of this migration.
--
-- The type is serialized as its bare name (RoadNodeTypeV2Converter), so a migrated road node has a string there and
-- one that still has collecting to do has JSON null or nothing at all - jsonb_typeof tells all three apart.

ALTER TABLE eventstore.mt_doc_roadnode
    ADD COLUMN has_migrated boolean
        GENERATED ALWAYS AS (jsonb_typeof(data -> 'type') = 'string') STORED;
