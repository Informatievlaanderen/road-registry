# Marten database migrations

Marten runs with `AutoCreate.None` (see `SetupExtensions.ConfigureRoad`) — it does **not** analyze or change the
schema at runtime. The schema is applied by **DbUp** (`DatabaseMigrator`) from the versioned `.sql` files in this
folder, in filename order, tracked in `eventstore.schema_migrations`.

## File naming

`{yyyyMMddHHmmss}_{name}.sql` (EF-style timestamp prefix). The one-time baseline is `00000000000000_baseline.sql`
so it always sorts first. Files are embedded resources (see the `.csproj`) and applied by the migration owner
(the Projector, and the MartenMigration host) at startup.

## Who applies them

Only the migration owners run DbUp (`AddDatabaseMigrations`): the **Projector** and the **MartenMigration** host.
It is idempotent and serialized by a Postgres advisory lock, so both may run it. Every other host (Lambda, API,
SyncHost) runs `AutoCreate.None` and **relies on an owner having migrated first** — deploy the owner before, or
gate the deploy on, hosts that use new schema (especially the write Lambda that appends events).

## Generating migrations

The full Marten model lives in the `RoadRegistry.MartenDb.MigrationGenerator` host (it registers every document type and
writes migration SQL from it via Marten's schema API — no runtime host is involved). Requires Docker + the .NET SDK.

- One-time baseline (full schema from the model): `src/RoadRegistry.MartenDb.MigrationGenerator/generate-baseline.sh`
- A new delta after you change the model: `src/RoadRegistry.MartenDb.MigrationGenerator/generate-migration.sh <name>`

`generate-migration.sh` spins a scratch Postgres, applies the existing migrations to bring it to the current state,
then runs the model host's `patch` (Marten `CreateMigrationAsync`) to diff the model against it and writes the delta
as the next file. **Review and hand-edit** the generated SQL before committing (custom indexes, fixes for anything
Marten generates that Postgres rejects, etc.). Marten only manages its own objects, so hand-added objects like
`ix_mt_events_correlation_seq` survive future `patch` runs.
