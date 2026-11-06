-- The road node documents get a geometry column of their own, generated from the document JSON exactly like the one
-- 20261103120000_add_topology_columns_to_aggregate_documents put on the road segments. Nothing in the application
-- reads it - the topology lookups only need the id and is_removed of a road node: it is there so the wegknopen can be
-- opened straight from the database in QGIS, next to the wegsegmenten that are already visualized that way.
--
-- The column is generated from the document JSON, so Postgres keeps it in step with every upsert Marten does -
-- nothing on the Marten side writes or knows about it. Adding a stored generated column rewrites the table, so
-- mt_doc_roadnode is locked for the duration of this migration.
--
-- A removed road node keeps its document, so the index covers the removed documents as well; is_removed tells them
-- apart.

ALTER TABLE eventstore.mt_doc_roadnode
    ADD COLUMN geometry public.geometry
        GENERATED ALWAYS AS (public.ST_GeomFromText(NULLIF(data -> 'geometry' ->> 'wkt', ''), (data -> 'geometry' ->> 'srid')::int)) STORED;

CREATE INDEX ix_mt_doc_roadnode_geometry ON eventstore.mt_doc_roadnode USING gist (geometry);
