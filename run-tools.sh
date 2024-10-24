#!/usr/bin/env bash
set -e

create_network () {
    networkId=$(docker network ls -f name=$1 -q)
    [ -z "$networkId" ] && docker network create $1
    echo ""
}

create_network road-registry-network

docker volume create seq-data > /dev/null

docker-compose --project-name road-registry \
    -f ./docker/docker-compose.yml \
    up $1 \
    --remove-orphans \
    --force-recreate
