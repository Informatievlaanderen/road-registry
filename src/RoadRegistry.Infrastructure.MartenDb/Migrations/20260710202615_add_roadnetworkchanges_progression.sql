-- Adds the progression-tracking document used by the RoadNetworkChanges async projections
-- (RoadNetworkChangesReadProjection / RoadNetworkChangesExtractProjection). This table backs
-- Marten document RoadNetworkChangesProjectionProgression (alias roadnetworkchangesprojection_progression)
-- and was missing from the baseline because the migration model did not register it.
--
-- Generated via generate-migration.sh and hand-edited down to only the progression objects: the raw
-- patch also tried to drop the hand-added ix_mt_events_correlation_seq index and regenerate the custom
-- networktopology_* functions (neither is part of the Marten model), which were intentionally discarded.

DROP TABLE IF EXISTS eventstore.mt_doc_roadnetworkchangesprojection_progression CASCADE;
CREATE TABLE eventstore.mt_doc_roadnetworkchangesprojection_progression (
    id                  varchar                     NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
    projection_name     varchar                     NOT NULL,
CONSTRAINT pkey_mt_doc_roadnetworkchangesprojection_progression_id PRIMARY KEY (id)
);

CREATE INDEX ix_changesprojection_projectionname ON eventstore.mt_doc_roadnetworkchangesprojection_progression USING btree (projection_name);

CREATE OR REPLACE FUNCTION eventstore.mt_upsert_roadnetworkchangesprojection_progression(arg_projection_name varchar, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO eventstore.mt_doc_roadnetworkchangesprojection_progression ("projection_name", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_projection_name, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "projection_name" = arg_projection_name, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM eventstore.mt_doc_roadnetworkchangesprojection_progression into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION eventstore.mt_insert_roadnetworkchangesprojection_progression(arg_projection_name varchar, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO eventstore.mt_doc_roadnetworkchangesprojection_progression ("projection_name", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_projection_name, doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION eventstore.mt_update_roadnetworkchangesprojection_progression(arg_projection_name varchar, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE eventstore.mt_doc_roadnetworkchangesprojection_progression SET "projection_name" = arg_projection_name, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM eventstore.mt_doc_roadnetworkchangesprojection_progression into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;
