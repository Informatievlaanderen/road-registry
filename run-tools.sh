#!/usr/bin/env bash
set -e

docker volume create seq-data > /dev/null

docker-compose --project-name road-registry-tools \
    -f ./docker/compose/mssql.yml \
    -f ./docker/compose/tools.yml \
    up \
    --remove-orphans
