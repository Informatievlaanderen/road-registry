#!/bin/bash

recreate_volume () {
    docker volume create $1 > /dev/null
    docker volume rm -f $1 > /dev/null
    docker volume create $1 > /dev/null
}
compose_service_up () {
    recreate_volume $3
    docker compose --project-name road-registry-seed -f $1 up --build $2 --force-recreate --quiet-pull
}
compose_service_down () {
    docker compose --project-name road-registry-seed down
}

compose_service_up ./docker/compose/mssql.yml import-backup mssql-data
compose_service_up ./docker/compose/municipality.yml municipality-mssql-seed-filled municipality-mssql-data
compose_service_up ./docker/compose/streetname.yml streetname-mssql-seed-filled streetname-mssql-data
compose_service_up ./docker/compose/legacy.yml legacy-mssql-seed-filled legacy-mssql-data

compose_service_down
