#!/usr/bin/env bash
set -e

docker-compose \
    -f ./docker-compose.yml \
    up $1 \
    --remove-orphans \
    --force-recreate
