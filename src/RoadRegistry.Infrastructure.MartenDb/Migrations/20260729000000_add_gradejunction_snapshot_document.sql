-- GradeJunction is now registered as a Marten aggregate snapshot (AddRoadAggregatesSnapshots), just like
-- RoadSegment / RoadNode / GradeSeparatedJunction, and RoadNetworkRepository.SaveEntities stores it as a document.
-- With AutoCreate.None Marten does not create the storage, so add the eventstore.mt_doc_gradejunction document table
-- and its stream-versioned upsert/insert/update/overwrite functions - identical in shape to mt_doc_gradeseparatedjunction.
CREATE TABLE IF NOT EXISTS eventstore.mt_doc_gradejunction (
    id                  varchar                     NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_dotnet_type      varchar                     NULL,
    mt_version          integer                     NOT NULL DEFAULT 0,
CONSTRAINT pkey_mt_doc_gradejunction_id PRIMARY KEY (id)
);

CREATE OR REPLACE FUNCTION eventstore.mt_upsert_gradejunction(doc JSONB, docDotNetType varchar, docId varchar, revision integer) RETURNS INTEGER LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version INTEGER;
  current_version INTEGER;
BEGIN

SELECT version into current_version FROM eventstore.mt_streams WHERE id = docId ;
if revision = 0 then
  if current_version is not null then
    revision = current_version;
  else
    revision = 1;
  end if;
else
  if current_version is not null then
    if current_version > revision then
      return 0;
    end if;
  end if;
end if;

INSERT INTO eventstore.mt_doc_gradejunction ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, revision, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = revision, mt_last_modified = transaction_timestamp() where revision > eventstore.mt_doc_gradejunction.mt_version;

  SELECT mt_version into final_version FROM eventstore.mt_doc_gradejunction WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION eventstore.mt_insert_gradejunction(doc JSONB, docDotNetType varchar, docId varchar, revision integer) RETURNS INTEGER LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO eventstore.mt_doc_gradejunction ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, revision, transaction_timestamp());
  RETURN 1;
END;
$function$;


CREATE OR REPLACE FUNCTION eventstore.mt_update_gradejunction(doc JSONB, docDotNetType varchar, docId varchar, revision integer) RETURNS INTEGER LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version INTEGER;
  current_version INTEGER;
BEGIN
  if revision <= 1 then
    SELECT mt_version FROM eventstore.mt_doc_gradejunction into current_version WHERE id = docId ;
    if current_version is not null then
      revision = current_version + 1;
    end if;
  end if;

  UPDATE eventstore.mt_doc_gradejunction SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = revision, mt_last_modified = transaction_timestamp() where revision > eventstore.mt_doc_gradejunction.mt_version and id = docId;

  SELECT mt_version FROM eventstore.mt_doc_gradejunction into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION eventstore.mt_overwrite_gradejunction(doc JSONB, docDotNetType varchar, docId varchar, revision integer) RETURNS INTEGER LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version INTEGER;
  current_version INTEGER;
BEGIN

  if revision = 0 then
    SELECT mt_version FROM eventstore.mt_doc_gradejunction into current_version WHERE id = docId ;
    if current_version is not null then
      revision = current_version + 1;
    else
      revision = 1;
    end if;
  end if;

  INSERT INTO eventstore.mt_doc_gradejunction ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, revision, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = revision, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM eventstore.mt_doc_gradejunction into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;
