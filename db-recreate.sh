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

containerIds=$(docker ps -q -a -f name=road-mssql)
if [[ ! -z "$containerIds" ]]; then
    docker rm -f $containerIds
fi

compose_service_up ./docker/docker-compose.yml import-backup road-mssql-data

compose_service_down