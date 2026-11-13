-- The list of road segments (GET wegsegmenten) filters on the status a road segment has in the V2 road network, which
-- is the status the detail of that road segment shows. The read documents of the road segments get that status as a
-- column of their own, with an index, so the list can filter on it and page through the result by id.
--
-- A V2 road segment carries its V2 status in the document. A V1 road segment - one not migrated yet - carries its V1
-- status, which the detail maps to a V2 status the way RoadSegmentStatusV2Mapping does; the column does the same, so a
-- road segment is listed under the status its detail shows. RoadSegmentStatusV2MappingTests in the integration tests
-- holds the two side by side for every V1 status and geometry draw method. Once the V1 road segments are gone, the
-- column is simply the status in the document.
--
-- The values are the ones the documents serialize: the bare names of the statuses and draw methods ("Gerealiseerd",
-- "InUse", "BuildingPermitGranted", "Measured", ...). A combination the mapping does not know leaves the column NULL,
-- where the detail fails, so such a road segment is not listed under any status.
--
-- Generated from the document JSON, so Postgres keeps it in step with every upsert Marten does and the read documents
-- do not have to be rebuilt; nothing on the Marten side writes or knows about it (see HandManagedSchema). Adding a
-- stored generated column rewrites the table, so mt_doc_read_roadsegments is locked for the duration of this
-- migration.

ALTER TABLE projections.mt_doc_read_roadsegments
    ADD COLUMN status character varying
        GENERATED ALWAYS AS (
            CASE
                WHEN coalesce((data ->> 'isV2')::boolean, false) THEN data ->> 'status'
                ELSE CASE data ->> 'status'
                    WHEN 'PermitRequested' THEN 'Gepland'
                    WHEN 'BuildingPermitGranted' THEN 'Gepland'
                    WHEN 'UnderConstruction' THEN 'Gepland'
                    WHEN 'InUse' THEN 'Gerealiseerd'
                    WHEN 'OutOfUse' THEN 'BuitenGebruik'
                    WHEN 'Unknown' THEN CASE data ->> 'geometryDrawMethod'
                        WHEN 'Outlined' THEN 'Gepland'
                        WHEN 'Measured' THEN 'Gerealiseerd'
                        WHEN 'Measured_according_to_GRB_specifications' THEN 'Gerealiseerd'
                    END
                END
            END
        ) STORED;

-- Status first and id second: the list filters on the one and pages through the result in the order of the other.
CREATE INDEX ix_read_roadsegments_status ON projections.mt_doc_read_roadsegments USING btree (status, id);
