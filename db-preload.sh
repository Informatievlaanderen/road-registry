#!/bin/bash
set -e

DB_DIR=../road-registry-db
archiveFiles=("road-registry.bak" "road-registry-events.bak" "legacy_db_filled.tar.gz" "municipality_syndication.tar.gz" "streetname_syndication.tar.gz")
for filename in "${archiveFiles[@]}"
do
    archivePath="$DB_DIR/${filename}"
    if [ ! -f $archivePath ]; then
        printf '%s\n' "File not found! ${archivePath}" >&2
        exit 1
    fi
done

printf '%s\n' "Copy backup file for database road-registry..."
cp "$DB_DIR/road-registry.bak" ./src/RoadRegistry.BackupDatabase/filled/road-registry.bak

printf '%s\n' "Copy backup file for database road-registry-events..."
cp "$DB_DIR/road-registry-events.bak" ./src/RoadRegistry.BackupDatabase/filled/road-registry-events.bak

printf '%s\n' "Copy backup file for database legacy..."
tar -xf "$DB_DIR/legacy_db_filled.tar.gz" --overwrite --directory ./src/RoadRegistry.LegacyDatabase/filled

printf '%s\n' "Copy backup file for database municipality_syndication..."
tar -xf "$DB_DIR/municipality_syndication.tar.gz" --overwrite --directory ./src/RoadRegistry.MunicipalityDatabase/filled

printf '%s\n' "Copy backup file for database streetname_syndication..."
tar -xf "$DB_DIR/streetname_syndication.tar.gz" --overwrite --directory ./src/RoadRegistry.StreetNameDatabase/filled

printf '%s\n' "Done."