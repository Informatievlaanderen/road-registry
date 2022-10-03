#!/usr/bin/env bash
set -e

DB_DIR_NAME=road-registry-db
DOCKER_NAME_FILTER=road-registry

DB_DIR=../road-registry-db
# find parent dir `road-registry-db`
#dbDirPath=$DB_DIR_NAME

# path=$(pwd)
# shift 1
# while [[ $path != / ]];
# do
#     find "$path" -maxdepth 1 -mindepth 1 "$DB_DIR_NAME"
#     path="$(readlink -f "$path"/..)"
# done

# printf "Path found: $path"
# exit 0


archiveFiles=("database_backup.zip" "legacy_db_filled.tar.gz" "municipality_syndication.tar.gz" "streetname_syndication.tar.gz")
for filename in "${archiveFiles[@]}"
do
    archivePath="$DB_DIR/${filename}"
    if [ ! -f $archivePath ]; then
        printf '%s\n' "File '${archivePath}' not found" >&2
        exit 1
    fi
done

printf '%s\n' "Clearing docker containers..."
containerIds=$(docker ps -q -a -f name=$DOCKER_NAME_FILTER)
if [[ ! -z "$containerIds" ]]; then
    docker rm -f $containerIds
    printf '%s\n' "  Done"

    printf '%s\n' "Clearing docker volumes..."
    volumeIds=$(docker volume ls -q -f name=$DOCKER_NAME_FILTER)
    if [[ ! -z "$volumeIds" ]]; then
        docker volume rm $volumeIds
        printf '%s\n' "  Done"
    else
        printf '%s\n' "  No volumes found"
    fi
else
    printf '%s\n' "  No containers found"
fi

printf '%s\n' "Extracting database_backup.zip..."
unzip -o -qq "$DB_DIR/database_backup.zip" -d src/RoadRegistry.BackupDatabase/filled
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
docker-compose up --build import-backup filled-municipality-mssql-seed filled-streetname-mssql-seed
printf '%s\n' "  Done"

printf '%s\n' "Removing backup database files..."
find . -name "*.bak" -type f -delete
find . -name "*.bcp" -type f -delete
printf '%s\n' "  Done"

printf '\n%s' "Database initialized."