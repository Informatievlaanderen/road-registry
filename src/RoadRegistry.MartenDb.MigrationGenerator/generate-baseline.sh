#!/usr/bin/env bash
#
# One-time: generate the full-schema baseline migration from Marten's model.
# Produces src/RoadRegistry.Infrastructure.MartenDb/Migrations/00000000000000_baseline.sql.
# Review & commit the result.
#
# Requires: Docker and the .NET SDK.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIG_DIR="$SCRIPT_DIR/../RoadRegistry.Infrastructure.MartenDb/Migrations"
HOST_PROJECT="$SCRIPT_DIR/RoadRegistry.MartenDb.MigrationGenerator.csproj"
CONTAINER="road-migration-scratch"
PORT="${SCRATCH_PORT:-55432}"
SCRATCH_CS="Host=localhost;Port=${PORT};Username=postgres;Password=postgres;Database=road"

cleanup() { docker rm -f "$CONTAINER" >/dev/null 2>&1 || true; }
trap cleanup EXIT
cleanup

echo "starting scratch postgres..."
docker run -d --name "$CONTAINER" -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=road \
  -p "${PORT}:5432" postgis/postgis:15-3.4 >/dev/null
until docker exec "$CONTAINER" pg_isready -U postgres -d road >/dev/null 2>&1; do sleep 1; done
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
