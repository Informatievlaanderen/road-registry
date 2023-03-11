#!/usr/bin/env bash
set -e

DB_DIR_NAME=road-registry-db
DOCKER_NAME_FILTER=road-registry

DB_DIR=../road-registry-db

archiveFiles=("road-registry.bak" "road-registry-events.bak" "legacy_db_filled.tar.gz" "municipality_syndication.tar.gz" "streetname_syndication.tar.gz")
for filename in "${archiveFiles[@]}"
do
    archivePath="$DB_DIR/${filename}"
    if [ ! -f $archivePath ]; then
        printf '%s\n' "File '${archivePath}' not found" >&2
        exit 1
    fi
done

printf '%s\n' "Clearing docker containers..."
printf '%s' "$DOCKER_NAME_FILTER..."
containerIds=$(docker ps -q -a -f name=$DOCKER_NAME_FILTER)
if [[ ! -z "$containerIds" ]]; then
    docker rm -f $containerIds
    printf '%s\n' "  Done"
else
    printf '%s\n' "  No containers found"
fi

printf '%s' "seq..."
containerIds=$(docker ps -q -a -f name=seq)
if [[ ! -z "$containerIds" ]]; then
    docker rm -f $containerIds
    printf '%s\n' "  Done"
else
    printf '%s\n' "  No containers found"
fi

printf '%s' "localstack..."
containerIds=$(docker ps -q -a -f name=localstack)
if [[ ! -z "$containerIds" ]]; then
    docker rm -f $containerIds
    printf '%s\n' "  Done"
else
    printf '%s\n' "  No containers found"
fi

printf '%s\n' "Clearing docker volumes..."
volumeIds=$(docker volume ls -q -f name=$DOCKER_NAME_FILTER)
if [[ ! -z "$volumeIds" ]]; then
    docker volume rm $volumeIds
    printf '%s\n' "  Done"
else
    printf '%s\n' "  No volumes found"
fi

printf '%s\n' "Copy file road-registry.bak..."
cp "$DB_DIR/road-registry.bak" src/RoadRegistry.BackupDatabase/filled/road-registry.bak
printf '%s\n' "  Done"

printf '%s\n' "Copy file road-registry-events.bak..."
cp "$DB_DIR/road-registry-events.bak" src/RoadRegistry.BackupDatabase/filled/road-registry-events.bak
printf '%s\n' "  Done"

printf '%s\n' "Extracting legacy_db_filled.tar.gz..."
tar -xf "$DB_DIR/legacy_db_filled.tar.gz" --overwrite --directory src/RoadRegistry.LegacyDatabase/filled
printf '%s\n' "  Done"

printf '%s\n' "Extracting municipality_syndication.tar.gz..."
tar -xf "$DB_DIR/municipality_syndication.tar.gz" --overwrite --directory src/RoadRegistry.MunicipalityDatabase/filled
printf '%s\n' "  Done"

printf '%s\n' "Extracting streetname_syndication.tar.gz..."
tar -xf "$DB_DIR/streetname_syndication.tar.gz" --overwrite --directory src/RoadRegistry.StreetNameDatabase/filled
printf '%s\n' "  Done"

printf '%s\n' "Building database..."
docker compose up --build import-backup filled-municipality-mssql-seed filled-streetname-mssql-seed
printf '%s\n' "  Done"

printf '%s\n' "Removing backup database files..."
find . -name "*.bak" -type f -delete
find . -name "*.bcp" -type f -delete
printf '%s\n' "  Done"

printf '\n%s' "Database initialized."