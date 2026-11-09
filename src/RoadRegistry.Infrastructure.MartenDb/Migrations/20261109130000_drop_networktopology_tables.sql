-- The road network topology is looked up on the aggregate documents since
-- 20261103120000_add_topology_columns_to_aggregate_documents, and RoadNetworkTopologyProjection - the inline
-- projection that translated every event a second time into these tables, and drifted from the aggregates - was
-- taken out with it. That migration left the tables and their functions in place because the hosts of the previous
-- release were still projecting into them; those hosts are deployed, so nothing reads or writes them any more.
--
-- The projection was inline, so it has no progression of its own to clean up: it ran in the transaction that
-- appended the events.
--
-- The indexes and primary keys go with their tables. The functions the projection called are dropped by name
-- rather than by signature, so a signature that changed along the way - networktopology_insert_roadsegment and
-- networktopology_update_roadnode were both redefined - does not leave one behind.

DROP TABLE IF EXISTS projections.networktopology_roadnodes CASCADE;
DROP TABLE IF EXISTS projections.networktopology_roadsegments CASCADE;
DROP TABLE IF EXISTS projections.networktopology_gradeseparatedjunctions CASCADE;
DROP TABLE IF EXISTS projections.networktopology_gradejunctions CASCADE;

DO
$$
    DECLARE
        topology_function record;
    BEGIN
        FOR topology_function IN
            SELECT p.oid::regprocedure AS signature
            FROM pg_proc p
                     JOIN pg_namespace n ON n.oid = p.pronamespace
            WHERE n.nspname = 'projections'
              AND p.proname LIKE 'networktopology%'
              AND p.prokind = 'f'
            LOOP
                EXECUTE format('DROP FUNCTION IF EXISTS %s;', topology_function.signature);
            END LOOP;
    END
$$;
