-- Adds the desired-state document for the Marten async projections. This table backs Marten document
-- MartenProjectionDesiredState (alias martenprojection_desiredstate): one row per shard recording whether an
-- operator wants it running ("subscribed") or stopped, written by the projections stop/start/rebuild endpoints
-- and read by the status page and the MartenProjectionSupervisor.
--
-- Marten runs with AutoCreate.None, so without this migration the document has no table and the first stop/start
-- call fails with "relation does not exist".
--
-- The document has no duplicated fields, so this is the plain Marten document shape plus the three storage
-- functions. CREATE ... IF NOT EXISTS / CREATE OR REPLACE throughout, so re-running is a no-op.

CREATE TABLE IF NOT EXISTS eventstore.mt_doc_martenprojection_desiredstate (
    id                  varchar                     NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
CONSTRAINT pkey_mt_doc_martenprojection_desiredstate_id PRIMARY KEY (id)
);

CREATE OR REPLACE FUNCTION eventstore.mt_upsert_martenprojection_desiredstate(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO eventstore.mt_doc_martenprojection_desiredstate ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM eventstore.mt_doc_martenprojection_desiredstate into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION eventstore.mt_insert_martenprojection_desiredstate(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO eventstore.mt_doc_martenprojection_desiredstate ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION eventstore.mt_update_martenprojection_desiredstate(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE eventstore.mt_doc_martenprojection_desiredstate SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM eventstore.mt_doc_martenprojection_desiredstate into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;
