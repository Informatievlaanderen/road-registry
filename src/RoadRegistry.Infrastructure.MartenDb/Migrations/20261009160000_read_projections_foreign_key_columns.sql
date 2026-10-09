-- The read documents no longer point at each other. A road node kept the ids of the segments knotted into it, a
-- segment kept the ids of its crossings, and two link documents kept the segments of a street name and of an
-- organization. All of that is gone: every document now carries only the foreign keys it owns, duplicated into
-- indexed columns of its own table, and whoever needs the other direction queries those columns
-- (ReadModelQueries).
--
-- Why it matters: those back-references are the only reason the read projection had to process a correlation's
-- events in emission order (a segment could not be projected before the node it pointed at existed), which is what
-- the tail fetch in RoadNetworkChangesProjection existed for - the mechanism that dropped 2004 road segments on
-- 2026-10-08 - and what broke a catch-up on every page boundary. With the references gone the read projection sets
-- RequiresEmissionOrder to false and neither the tail fetch nor the per-correlation progression applies to it.
--
-- Marten runs with AutoCreate.None, so the columns, their indexes and the storage functions that write them are
-- added here. Like 20260710205157, the columns are added nullable, backfilled from each row's stored JSON, and only
-- then marked NOT NULL where the model says so; the upsert/insert/update functions are replaced with the signature
-- that also writes the duplicated columns (arguments sorted the way Marten sorts them: arg_* alphabetically, then
-- doc, docDotNetType, docId, docVersion).

-- ---------------------------------------------------------------------------------------------------------------
-- 1. projections.mt_doc_read_roadsegments: start/end road node and the street name / organization ids
-- ---------------------------------------------------------------------------------------------------------------

ALTER TABLE projections.mt_doc_read_roadsegments
    ADD COLUMN IF NOT EXISTS start_node_id integer,
    ADD COLUMN IF NOT EXISTS end_node_id integer,
    ADD COLUMN IF NOT EXISTS street_name_ids integer[];

-- Backfill from the stored document (Marten serializes camelCase, and the id value objects as bare scalars), and
-- drop the back-references the documents no longer have a property for in the same pass - they are dead weight in
-- every segment that is not rewritten again.
--
-- The street name ids are the ones the segment's street name attribute actually points at: the "unknown" (-8) and
-- "not applicable" (-9) sentinels, and 0, are not street names, matching StreetNameLocalId.IsEmpty.
--
-- They also go into the stored document, which is where Marten refills the column from when it adds or reconciles
-- it (RoadSegmentReadItem.StreetNameIds). Without them in the document, that refill would empty the column for
-- every segment that has not been rewritten since.
UPDATE projections.mt_doc_read_roadsegments
   SET start_node_id = (data ->> 'startNodeId')::int,
       end_node_id = (data ->> 'endNodeId')::int,
       street_name_ids = (
           SELECT COALESCE(array_agg(DISTINCT street_name_id ORDER BY street_name_id), '{}'::int[])
           FROM (
               SELECT (attribute_value -> 'value' ->> 'streetNameId')::int AS street_name_id
               FROM jsonb_array_elements(COALESCE(data -> 'streetNameId' -> 'values', '[]'::jsonb)) AS attribute_value
           ) street_names
           WHERE street_name_id > 0
       ),
       data = (data - 'gradeJunctionIds' - 'gradeSeparatedJunctionIds')
           || jsonb_build_object(
               'streetNameIds', (
                   SELECT COALESCE(jsonb_agg(DISTINCT street_name_id ORDER BY street_name_id), '[]'::jsonb)
                   FROM (
                       SELECT (attribute_value -> 'value' ->> 'streetNameId')::int AS street_name_id
                       FROM jsonb_array_elements(COALESCE(data -> 'streetNameId' -> 'values', '[]'::jsonb)) AS attribute_value
                   ) street_names
                   WHERE street_name_id > 0
               ));

CREATE INDEX IF NOT EXISTS ix_read_roadsegments_startnodeid ON projections.mt_doc_read_roadsegments USING btree (start_node_id);
CREATE INDEX IF NOT EXISTS ix_read_roadsegments_endnodeid ON projections.mt_doc_read_roadsegments USING btree (end_node_id);
CREATE INDEX IF NOT EXISTS ix_read_roadsegments_streetnameids ON projections.mt_doc_read_roadsegments USING gin (street_name_ids);

DROP FUNCTION IF EXISTS projections.mt_upsert_read_roadsegments(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_upsert_read_roadsegments(arg_end_node_id integer, arg_start_node_id integer, arg_street_name_ids integer[], doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO projections.mt_doc_read_roadsegments ("end_node_id", "start_node_id", "street_name_ids", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_end_node_id, arg_start_node_id, arg_street_name_ids, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "end_node_id" = arg_end_node_id, "start_node_id" = arg_start_node_id, "street_name_ids" = arg_street_name_ids, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM projections.mt_doc_read_roadsegments into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

DROP FUNCTION IF EXISTS projections.mt_insert_read_roadsegments(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_insert_read_roadsegments(arg_end_node_id integer, arg_start_node_id integer, arg_street_name_ids integer[], doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO projections.mt_doc_read_roadsegments ("end_node_id", "start_node_id", "street_name_ids", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_end_node_id, arg_start_node_id, arg_street_name_ids, doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;

DROP FUNCTION IF EXISTS projections.mt_update_read_roadsegments(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_update_read_roadsegments(arg_end_node_id integer, arg_start_node_id integer, arg_street_name_ids integer[], doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE projections.mt_doc_read_roadsegments SET "end_node_id" = arg_end_node_id, "start_node_id" = arg_start_node_id, "street_name_ids" = arg_street_name_ids, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM projections.mt_doc_read_roadsegments into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

-- ---------------------------------------------------------------------------------------------------------------
-- 2. projections.mt_doc_read_gradejunctions: the two road segments the crossing is between
-- ---------------------------------------------------------------------------------------------------------------

ALTER TABLE projections.mt_doc_read_gradejunctions
    ADD COLUMN IF NOT EXISTS road_segment_id_1 integer,
    ADD COLUMN IF NOT EXISTS road_segment_id_2 integer;

-- Both are required on the document and written by every event that creates or modifies one, so a row without them
-- would be corrupt; the SET NOT NULL below fails loudly rather than storing a wrong default.
UPDATE projections.mt_doc_read_gradejunctions
   SET road_segment_id_1 = (data ->> 'roadSegmentId1')::int,
       road_segment_id_2 = (data ->> 'roadSegmentId2')::int
 WHERE road_segment_id_1 IS NULL
    OR road_segment_id_2 IS NULL;

ALTER TABLE projections.mt_doc_read_gradejunctions
    ALTER COLUMN road_segment_id_1 SET NOT NULL,
    ALTER COLUMN road_segment_id_2 SET NOT NULL;

CREATE INDEX IF NOT EXISTS ix_read_gradejunctions_roadsegmentid1 ON projections.mt_doc_read_gradejunctions USING btree (road_segment_id_1);
CREATE INDEX IF NOT EXISTS ix_read_gradejunctions_roadsegmentid2 ON projections.mt_doc_read_gradejunctions USING btree (road_segment_id_2);

DROP FUNCTION IF EXISTS projections.mt_upsert_read_gradejunctions(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_upsert_read_gradejunctions(arg_road_segment_id_1 integer, arg_road_segment_id_2 integer, doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO projections.mt_doc_read_gradejunctions ("road_segment_id_1", "road_segment_id_2", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_road_segment_id_1, arg_road_segment_id_2, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "road_segment_id_1" = arg_road_segment_id_1, "road_segment_id_2" = arg_road_segment_id_2, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM projections.mt_doc_read_gradejunctions into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

DROP FUNCTION IF EXISTS projections.mt_insert_read_gradejunctions(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_insert_read_gradejunctions(arg_road_segment_id_1 integer, arg_road_segment_id_2 integer, doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO projections.mt_doc_read_gradejunctions ("road_segment_id_1", "road_segment_id_2", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_road_segment_id_1, arg_road_segment_id_2, doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;

DROP FUNCTION IF EXISTS projections.mt_update_read_gradejunctions(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_update_read_gradejunctions(arg_road_segment_id_1 integer, arg_road_segment_id_2 integer, doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE projections.mt_doc_read_gradejunctions SET "road_segment_id_1" = arg_road_segment_id_1, "road_segment_id_2" = arg_road_segment_id_2, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM projections.mt_doc_read_gradejunctions into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

-- ---------------------------------------------------------------------------------------------------------------
-- 3. projections.mt_doc_read_gradeseparatedjunctions: the lower and upper road segment
-- ---------------------------------------------------------------------------------------------------------------

ALTER TABLE projections.mt_doc_read_gradeseparatedjunctions
    ADD COLUMN IF NOT EXISTS lower_road_segment_id integer,
    ADD COLUMN IF NOT EXISTS upper_road_segment_id integer;

UPDATE projections.mt_doc_read_gradeseparatedjunctions
   SET lower_road_segment_id = (data ->> 'lowerRoadSegmentId')::int,
       upper_road_segment_id = (data ->> 'upperRoadSegmentId')::int
 WHERE lower_road_segment_id IS NULL
    OR upper_road_segment_id IS NULL;

ALTER TABLE projections.mt_doc_read_gradeseparatedjunctions
    ALTER COLUMN lower_road_segment_id SET NOT NULL,
    ALTER COLUMN upper_road_segment_id SET NOT NULL;

CREATE INDEX IF NOT EXISTS ix_read_gradeseparatedjunctions_lowerroadsegmentid ON projections.mt_doc_read_gradeseparatedjunctions USING btree (lower_road_segment_id);
CREATE INDEX IF NOT EXISTS ix_read_gradeseparatedjunctions_upperroadsegmentid ON projections.mt_doc_read_gradeseparatedjunctions USING btree (upper_road_segment_id);

DROP FUNCTION IF EXISTS projections.mt_upsert_read_gradeseparatedjunctions(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_upsert_read_gradeseparatedjunctions(arg_lower_road_segment_id integer, arg_upper_road_segment_id integer, doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO projections.mt_doc_read_gradeseparatedjunctions ("lower_road_segment_id", "upper_road_segment_id", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_lower_road_segment_id, arg_upper_road_segment_id, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "lower_road_segment_id" = arg_lower_road_segment_id, "upper_road_segment_id" = arg_upper_road_segment_id, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM projections.mt_doc_read_gradeseparatedjunctions into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

DROP FUNCTION IF EXISTS projections.mt_insert_read_gradeseparatedjunctions(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_insert_read_gradeseparatedjunctions(arg_lower_road_segment_id integer, arg_upper_road_segment_id integer, doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO projections.mt_doc_read_gradeseparatedjunctions ("lower_road_segment_id", "upper_road_segment_id", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_lower_road_segment_id, arg_upper_road_segment_id, doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;

DROP FUNCTION IF EXISTS projections.mt_update_read_gradeseparatedjunctions(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION projections.mt_update_read_gradeseparatedjunctions(arg_lower_road_segment_id integer, arg_upper_road_segment_id integer, doc JSONB, docDotNetType varchar, docId integer, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE projections.mt_doc_read_gradeseparatedjunctions SET "lower_road_segment_id" = arg_lower_road_segment_id, "upper_road_segment_id" = arg_upper_road_segment_id, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM projections.mt_doc_read_gradeseparatedjunctions into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

-- ---------------------------------------------------------------------------------------------------------------
-- 4. projections.mt_doc_read_roadnodes: the segment back-references are gone from the document
-- ---------------------------------------------------------------------------------------------------------------

UPDATE projections.mt_doc_read_roadnodes
   SET data = data - 'roadSegmentIds'
 WHERE data ? 'roadSegmentIds';

-- ---------------------------------------------------------------------------------------------------------------
-- 5. the two link documents are gone
-- ---------------------------------------------------------------------------------------------------------------

DROP FUNCTION IF EXISTS projections.mt_upsert_read_streetname_roadsegments_link(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;
DROP FUNCTION IF EXISTS projections.mt_insert_read_streetname_roadsegments_link(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;
DROP FUNCTION IF EXISTS projections.mt_update_read_streetname_roadsegments_link(doc jsonb, docdotnettype character varying, docid integer, docversion uuid) cascade;
DROP TABLE IF EXISTS projections.mt_doc_read_streetname_roadsegments_link CASCADE;

DROP FUNCTION IF EXISTS projections.mt_upsert_read_organization_roadsegments_link(doc jsonb, docdotnettype character varying, docid character varying, docversion uuid) cascade;
DROP FUNCTION IF EXISTS projections.mt_insert_read_organization_roadsegments_link(doc jsonb, docdotnettype character varying, docid character varying, docversion uuid) cascade;
DROP FUNCTION IF EXISTS projections.mt_update_read_organization_roadsegments_link(doc jsonb, docdotnettype character varying, docid character varying, docversion uuid) cascade;
DROP TABLE IF EXISTS projections.mt_doc_read_organization_roadsegments_link CASCADE;
