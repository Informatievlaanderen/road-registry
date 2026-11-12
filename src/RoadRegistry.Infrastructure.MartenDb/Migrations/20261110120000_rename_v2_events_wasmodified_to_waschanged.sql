-- The V2 events are renamed and reshaped (GAWR-7199):
--
--   1. RoadSegmentWasModified becomes RoadSegmentWasChanged and no longer carries a status or a geometry draw method.
--      A change of status is recorded by the status change events, a change of draw method by RoadSegmentWasMeasured
--      or RoadSegmentWasCorrectedToOutlined.
--   2. RoadSegmentGeometryDrawMethodWasChanged is split into RoadSegmentWasMeasured (to 'Ingemeten') and
--      RoadSegmentWasCorrectedToOutlined (to 'Ingeschetst'); the draw method is the event itself.
--   3. RoadSegmentGeometryWasModified no longer exists; it becomes a RoadSegmentWasChanged carrying the geometry and the
--      road nodes.
--   4. Every other V2 event ending in WasModified now ends in WasChanged.
--
-- An event keeps its sequence, its stream version and its data; only its type, its .NET type and - for 1 to 3 - the
-- shape of its data change.
--
-- Dropping the status and the draw method from a stored RoadSegmentWasModified only loses something when the event
-- actually changed them. For the status that never happened: on staging, the only environment where such events carry
-- a status, it always equals the status the road segment already had. The draw method did change on some of them, so
-- before the property is dropped, a RoadSegmentWasMeasured or RoadSegmentWasCorrectedToOutlined is appended to the end
-- of that road segment's stream wherever the draw method differs from the one the road segment had before - unless a
-- later event in that stream sets the draw method again, in which case that later event already decides where the
-- road segment ends up. The appended event gets a correlation of its own, so the projections apply it as a change of
-- its own; it sets the draw method the road segment already has in every read model, so nothing visibly changes.
--
-- Re-running it changes nothing: every step only touches events of the old types, which are gone after the first run.

DO $do$
DECLARE
    modified record;
    previous_method text;
    later_method_count integer;
    new_version bigint;
    new_type text;
    new_dotnet_type text;
BEGIN
    IF to_regclass('eventstore.mt_events') IS NULL THEN
        RETURN;
    END IF;

    -- 1a. Keep a real change of the geometry draw method, before it is dropped from RoadSegmentWasModified.
    FOR modified IN
        SELECT e.seq_id, e.stream_id, e.version, e.data, e.mt_dotnet_type, e.tenant_id
        FROM eventstore.mt_events e
        WHERE e.type = 'road_segment_was_modified'
          AND e.data ? 'geometryDrawMethod'
          AND jsonb_typeof(e.data -> 'geometryDrawMethod') <> 'null'
        ORDER BY e.seq_id
    LOOP
        -- The draw method the road segment had right before: the last earlier event in its stream that sets one.
        SELECT CASE WHEN p.type = 'outlined_road_segment_was_added' THEN 'Ingeschetst'
                    ELSE p.data ->> 'geometryDrawMethod' END
        INTO previous_method
        FROM eventstore.mt_events p
        WHERE p.stream_id = modified.stream_id
          AND p.version < modified.version
          AND (p.type = 'outlined_road_segment_was_added'
               OR (p.data ? 'geometryDrawMethod' AND jsonb_typeof(p.data -> 'geometryDrawMethod') <> 'null'))
        ORDER BY p.version DESC
        LIMIT 1;

        IF previous_method IS NOT DISTINCT FROM (modified.data ->> 'geometryDrawMethod') THEN
            CONTINUE;
        END IF;

        -- A later event that sets the draw method again already decides where the road segment ends up.
        SELECT count(*)
        INTO later_method_count
        FROM eventstore.mt_events l
        WHERE l.stream_id = modified.stream_id
          AND l.version > modified.version
          AND l.data ? 'geometryDrawMethod'
          AND jsonb_typeof(l.data -> 'geometryDrawMethod') <> 'null';

        IF later_method_count > 0 THEN
            CONTINUE;
        END IF;

        IF modified.data ->> 'geometryDrawMethod' = 'Ingemeten' THEN
            new_type := 'road_segment_was_measured';
            new_dotnet_type := replace(modified.mt_dotnet_type, '.RoadSegmentWasModified,', '.RoadSegmentWasMeasured,');
        ELSE
            new_type := 'road_segment_was_corrected_to_outlined';
            new_dotnet_type := replace(modified.mt_dotnet_type, '.RoadSegmentWasModified,', '.RoadSegmentWasCorrectedToOutlined,');
        END IF;

        UPDATE eventstore.mt_streams
        SET version = coalesce(version, 0) + 1,
            timestamp = now()
        WHERE id = modified.stream_id
        RETURNING version INTO new_version;

        INSERT INTO eventstore.mt_events
            (seq_id, id, stream_id, version, data, type, timestamp, tenant_id, mt_dotnet_type, correlation_id, causation_id, headers)
        VALUES
            (nextval('eventstore.mt_events_sequence'),
             gen_random_uuid(),
             modified.stream_id,
             new_version,
             jsonb_build_object(
                 'roadSegmentId', modified.data -> 'roadSegmentId',
                 'provenance', modified.data -> 'provenance'),
             new_type,
             now(),
             modified.tenant_id,
             new_dotnet_type,
             'sql-migration-20261110120000-' || modified.seq_id,
             'sql-migration-20261110120000-' || modified.seq_id,
             jsonb_build_object('roadNetworkChangeOrdinal', 0));

        RAISE NOTICE 'Appended % to stream % (version %) for the draw method change in event %',
            new_type, modified.stream_id, new_version, modified.seq_id;
    END LOOP;

    -- 1b. RoadSegmentWasModified becomes RoadSegmentWasChanged, without a status or a draw method.
    UPDATE eventstore.mt_events
    SET type = 'road_segment_was_changed',
        mt_dotnet_type = replace(mt_dotnet_type, '.RoadSegmentWasModified,', '.RoadSegmentWasChanged,'),
        data = data - 'status' - 'geometryDrawMethod'
    WHERE type = 'road_segment_was_modified';

    -- 2. RoadSegmentGeometryDrawMethodWasChanged is split by the draw method it set.
    UPDATE eventstore.mt_events
    SET type = 'road_segment_was_measured',
        mt_dotnet_type = replace(mt_dotnet_type, '.RoadSegmentGeometryDrawMethodWasChanged,', '.RoadSegmentWasMeasured,'),
        data = data - 'geometryDrawMethod'
    WHERE type = 'road_segment_geometry_draw_method_was_changed'
      AND data ->> 'geometryDrawMethod' = 'Ingemeten';

    UPDATE eventstore.mt_events
    SET type = 'road_segment_was_corrected_to_outlined',
        mt_dotnet_type = replace(mt_dotnet_type, '.RoadSegmentGeometryDrawMethodWasChanged,', '.RoadSegmentWasCorrectedToOutlined,'),
        data = data - 'geometryDrawMethod'
    WHERE type = 'road_segment_geometry_draw_method_was_changed'
      AND data ->> 'geometryDrawMethod' = 'Ingeschetst';

    -- 3. RoadSegmentGeometryWasModified becomes a RoadSegmentWasChanged with the geometry and the road nodes.
    UPDATE eventstore.mt_events
    SET type = 'road_segment_was_changed',
        mt_dotnet_type = replace(mt_dotnet_type, '.RoadSegmentGeometryWasModified,', '.RoadSegmentWasChanged,'),
        data = (data - 'startNodeId' - 'endNodeId')
               || jsonb_build_object('nodeIds', jsonb_build_object(
                      'start', coalesce(data -> 'startNodeId', 'null'::jsonb),
                      'end', coalesce(data -> 'endNodeId', 'null'::jsonb)))
    WHERE type = 'road_segment_geometry_was_modified';

    -- 4. Every other V2 event ending in WasModified now ends in WasChanged.
    UPDATE eventstore.mt_events
    SET type = 'road_node_was_changed',
        mt_dotnet_type = replace(mt_dotnet_type, '.RoadNodeWasModified,', '.RoadNodeWasChanged,')
    WHERE type = 'road_node_was_modified';

    UPDATE eventstore.mt_events
    SET type = 'grade_junction_was_changed',
        mt_dotnet_type = replace(mt_dotnet_type, '.GradeJunctionWasModified,', '.GradeJunctionWasChanged,')
    WHERE type = 'grade_junction_was_modified';

    UPDATE eventstore.mt_events
    SET type = 'grade_separated_junction_was_changed',
        mt_dotnet_type = replace(mt_dotnet_type, '.GradeSeparatedJunctionWasModified,', '.GradeSeparatedJunctionWasChanged,')
    WHERE type = 'grade_separated_junction_was_modified';

    UPDATE eventstore.mt_events
    SET type = 'organization_was_changed',
        mt_dotnet_type = replace(mt_dotnet_type, '.OrganizationWasModified,', '.OrganizationWasChanged,')
    WHERE type = 'organization_was_modified';

    UPDATE eventstore.mt_events
    SET type = 'street_name_was_changed',
        mt_dotnet_type = replace(mt_dotnet_type, '.StreetNameWasModified,', '.StreetNameWasChanged,')
    WHERE type = 'street_name_was_modified';
END $do$;
