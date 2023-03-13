#!/usr/bin/env bash
set -e

# docker-compose up --build municipality-api streetname-api public-api mssql-seed localstack

docker-compose --project-name road-registry \
    -f ../docker/compose/municipality.yml --build municipality-api \
    -f ../docker/compose/streetname.yml --build streetname-api \
    -f ../docker/compose/streetname.yml --build streetname-api \
    -f ../docker/compose/mssql-seed.yml --build mssql-seed \
    up    
