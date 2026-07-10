-- Adds LastSequenceId as a duplicated column on the RoadNetworkChanges progression document
-- (eventstore.mt_doc_roadnetworkchangesprojection_progression), matching the .Duplicate(x => x.LastSequenceId)
-- registration in ConfigureRoadNetworkChangesProgression.
--
-- Marten cannot generate this delta itself: adding a NOT NULL column to an existing table is rejected under
-- AutoCreate.CreateOrUpdate (and AutoCreate.All would drop & recreate the table, losing data). So the column is
-- added nullable, backfilled from each row's stored JSON document, and only then marked NOT NULL. The upsert/
-- insert/update functions are replaced with the new signature that also writes the duplicated column.

-- 1. Add the duplicated column nullable so existing rows survive the change.
ALTER TABLE eventstore.mt_doc_roadnetworkchangesprojection_progression
    ADD COLUMN IF NOT EXISTS last_sequence_id bigint;

-- 2. Backfill from the stored JSON document. Marten serializes with camelCase, so LastSequenceId -> lastSequenceId.
--    If any row were missing the value the SET NOT NULL below would fail loudly rather than store a wrong default.
UPDATE eventstore.mt_doc_roadnetworkchangesprojection_progression
   SET last_sequence_id = (data ->> 'lastSequenceId')::bigint
 WHERE last_sequence_id IS NULL;

-- 3. Enforce NOT NULL now that every row has a value (matches the notNull: true duplicated-field registration).
ALTER TABLE eventstore.mt_doc_roadnetworkchangesprojection_progression
    ALTER COLUMN last_sequence_id SET NOT NULL;

CREATE INDEX IF NOT EXISTS ix_changesprojection_lastsequenceid ON eventstore.mt_doc_roadnetworkchangesprojection_progression USING btree (last_sequence_id);

-- 4. Replace the storage functions with the new signature (arg_last_sequence_id) that populates the column.
DROP FUNCTION IF EXISTS eventstore.mt_upsert_roadnetworkchangesprojection_progression(arg_projection_name character varying, doc jsonb, docdotnettype character varying, docid character varying, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION eventstore.mt_upsert_roadnetworkchangesprojection_progression(arg_last_sequence_id bigint, arg_projection_name varchar, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO eventstore.mt_doc_roadnetworkchangesprojection_progression ("last_sequence_id", "projection_name", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_last_sequence_id, arg_projection_name, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "last_sequence_id" = arg_last_sequence_id, "projection_name" = arg_projection_name, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM eventstore.mt_doc_roadnetworkchangesprojection_progression into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

DROP FUNCTION IF EXISTS eventstore.mt_insert_roadnetworkchangesprojection_progression(arg_projection_name character varying, doc jsonb, docdotnettype character varying, docid character varying, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION eventstore.mt_insert_roadnetworkchangesprojection_progression(arg_last_sequence_id bigint, arg_projection_name varchar, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO eventstore.mt_doc_roadnetworkchangesprojection_progression ("last_sequence_id", "projection_name", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_last_sequence_id, arg_projection_name, doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;

DROP FUNCTION IF EXISTS eventstore.mt_update_roadnetworkchangesprojection_progression(arg_projection_name character varying, doc jsonb, docdotnettype character varying, docid character varying, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION eventstore.mt_update_roadnetworkchangesprojection_progression(arg_last_sequence_id bigint, arg_projection_name varchar, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE eventstore.mt_doc_roadnetworkchangesprojection_progression SET "last_sequence_id" = arg_last_sequence_id, "projection_name" = arg_projection_name, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM eventstore.mt_doc_roadnetworkchangesprojection_progression into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;
