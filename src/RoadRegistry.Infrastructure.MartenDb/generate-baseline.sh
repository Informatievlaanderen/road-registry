#!/usr/bin/env bash
#
# One-time: generate the full-schema baseline migration from Marten's model.
# Produces src/RoadRegistry.Infrastructure.MartenDb/Migrations/00000000000000_baseline.sql.
# Review & commit the result.
#
# Requires: Docker and the .NET SDK.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIG_DIR="$SCRIPT_DIR/Migrations"
HOST_PROJECT="$SCRIPT_DIR/../RoadRegistry.MartenDb.MigrationGenerator/RoadRegistry.MartenDb.MigrationGenerator.csproj"
CONTAINER="road-migration-scratch"
PORT="${SCRATCH_PORT:-55432}"
SCRATCH_CS="Host=localhost;Port=${PORT};Username=postgres;Password=postgres;Database=road"

cleanup() { docker rm -f "$CONTAINER" >/dev/null 2>&1 || true; }
trap cleanup EXIT
cleanup

echo "starting scratch postgres..."
docker run -d --name "$CONTAINER" -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=road \
  -p "${PORT}:5432" postgis/postgis:15-3.4 >/dev/null
echo "waiting for postgres to accept TCP connections..."
# The postgres/postgis entrypoint runs a temporary, socket-only server to initialise the database, then
# shuts it down and starts the real server on TCP. A unix-socket pg_isready can pass against that temporary
# server and then race its shutdown ("the database system is shutting down"). Probe over TCP (127.0.0.1) —
# which only the real, post-init server listens on — and require it to hold for two consecutive checks.
ready=0
for _ in $(seq 1 90); do
  if docker exec "$CONTAINER" pg_isready -h 127.0.0.1 -p 5432 -U postgres -d road >/dev/null 2>&1; then
    ready=$((ready + 1))
    [ "$ready" -ge 2 ] && break
  else
    ready=0
  fi
  sleep 1
done
[ "$ready" -ge 2 ] || { echo "postgres did not become ready in time" >&2; exit 1; }
# The postgis image is still initialising its template right after pg_isready first passes, so a bare
# CREATE EXTENSION races with it ("duplicate key ... pg_extension_name_index"). Retry until it sticks.
for _ in $(seq 1 15); do
  docker exec "$CONTAINER" psql -U postgres -d road -c "CREATE EXTENSION IF NOT EXISTS postgis;" >/dev/null 2>&1 && break
  sleep 1
done

mkdir -p "$MIG_DIR"
OUT="$MIG_DIR/00000000000000_baseline.sql"

# The scratch db is empty, so the model-vs-db diff is the full creation script.
echo "generating baseline via 'patch' against the empty scratch db..."
dotnet run --project "$HOST_PROJECT" -- patch "$OUT" "$SCRATCH_CS"

echo "wrote $OUT - review & commit."
